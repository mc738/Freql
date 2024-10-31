namespace Freql.Core

open Microsoft.FSharp.Quotations

module Queries =

    open Microsoft.FSharp.Quotations.Patterns
    open Microsoft.FSharp.Quotations.DerivedPatterns

    type Query<'T> = NA

    type FreqlQueryBuilder() =

        member fqb.For(tz: Query<'T>, f: 'T -> Query<'R>) : Query<'R> = NA

        //member fqb.For<'R when 'R : not Query<_>>(tz: Query<'T>, f: 'T -> Query<'R>) : Query<'R> = NA

        member fqb.Yield(v: 'T) : Query<'T> = NA

        //member fqb.Yield(v: 'R) : Query<'T> = NA

        member fqb.Quote(e: Expr<_>) = e

        [<CustomOperation("where", MaintainsVariableSpace = true)>]
        member fqb.Where(source: Query<'T>, [<ProjectionParameter>] f: 'T -> bool) : Query<'T> = NA

        [<CustomOperation("includeFiltered", MaintainsVariableSpace = true)>]
        member fqb.IncludeFiltered(source: Query<'T>, [<ProjectionParameter>] f: 'T -> Expr<'R>) : Query<'T> = NA

        [<CustomOperation("selectAttrs")>]
        member fqb.SelectAttrs(source: Query<'T>, [<ProjectionParameter>] f: 'T -> 'R) : Query<'R> = NA

        [<CustomOperation("selectCount")>]
        member fqb.SelectCount(source: Query<'T>) : int = failwith ""

    let toQuery<'R> (v: 'R) = NA

    let inline (!@) v = toQuery v

    let freqlQuery = FreqlQueryBuilder()

    type Foo =
        { Id: int
          Name: string
          Bars: Bar list }

    and Bar = { Id: int; Name: string }

    let foos: Query<Foo> = NA

    let q =
        freqlQuery {
            for f in foos do
                where (f.Id > 10)

                includeFiltered (
                    freqlQuery {
                        for b in !@f.Bars do
                            where (b.Id < 10)
                            where (b.Name = "")
                            selectAttrs ()
                    }
                )

                selectCount
        }


    type QueryCondition =
        { Property: string
          Operator: string
          Constant: obj }

    type QueryProject =
        | SelectAttributes of string list
        | SelectCount


    type Query =
        { Source: string
          Where: QueryCondition list
          Select: QueryProject option }

    let translateWhere =
        function
        | Lambda(var1, Call(None, op, [ left; right ])) ->
            match left, right with
            | PropertyGet(Some(Var var2), prop, []), Value(value, _) when
                var1.Name = var2.Name && op.Name.StartsWith("op_")
                ->
                // We got 'where' that we understand. Build QueryCondition!
                { Property = prop.Name
                  Operator = op.Name.Substring(3)
                  Constant = value }
            | e ->
                // 'where' with not supported format
                // (this can happen so report more useful error)
                failwithf "%s\nGot: %A" ("Only expressions of the form " + "'p.Prop <op> <value>' are supported!") e
        // This should not happen - the parameter is always lambda!
        | _ -> failwith "Expected lambda expression"

    let translatePropGet varName =
        function
        | PropertyGet(Some(Var v), prop, []) when v.Name = varName -> prop.Name
        | e ->
            // Too complex expression in projection
            failwithf "%s\nGot: %A" ("Only expressions of the form " + "'p.Prop' are supported!") e
            
    let translateProjection e =
        match e with
        | Lambda(var1, NewTuple args) ->
            // Translate all tuple items
            List.map (translatePropGet var1.Name) args
        | Lambda(var1, args) ->
            // There is just one body expression
            [ translatePropGet var1.Name args ]
        | _ -> failwith "Expected lambda expression"

    let rec translateQuery (e: Expr) =
        match e with
        | SpecificCall <@@ freqlQuery.SelectAttrs @@> (builder, [ tTyp; rTyp ], [ source; projection ]) ->
            let q = translateQuery source
            let s = translateProjection projection

            { q with
                Select = Some(SelectAttributes s) }
        | SpecificCall <@@ freqlQuery.SelectCount @@> (builder, [ tTyp; rTyp ], [ source; projection ]) ->
            let q = translateQuery source

            { q with Select = Some SelectCount }
        | SpecificCall <@@ freqlQuery.Where @@> (builder, [ tTyp; rTyp ], [ source; projection ]) ->
            let q = translateQuery source
            let w = translateWhere projection

            { q with Where = w :: q.Where }
        | SpecificCall <@@ freqlQuery.For @@> (builder, [ tTyp; rTyp ], [ source; body ]) ->
            let source =
                match source with
                | PropertyGet(None, prop, []) when prop.DeclaringType = typeof<obj> -> prop.Name
                | _ -> failwith "Only sources of the form 'DB.<Prop>' are supported!"

            { Source = source
              Where = []
              Select = None }

        // This should never happen
        | e -> failwithf "Unsupported query operation: %A" e
