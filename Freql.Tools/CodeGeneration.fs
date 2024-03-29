﻿namespace Freql.Tools

open System
open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

module CodeGeneration =

    module Configuration =

        type DatabaseConfiguration =
            { [<JsonPropertyName("name")>]
              Name: string
              [<JsonPropertyName("type")>]
              Type: string
              [<JsonPropertyName("connectionString")>]
              ConnectionString: string
              [<JsonPropertyName("generatorProfiles")>]
              GeneratorProfiles: GeneratorProfile seq }

            static member TryLoad(path: string) =
                try
                    File.ReadAllText path
                    |> JsonSerializer.Deserialize<DatabaseConfiguration seq>
                    |> Ok
                with
                | exn -> Error exn.Message

        and GeneratorProfile =
            { [<JsonPropertyName("name")>]
              Name: string
              [<JsonPropertyName("outputPath")>]
              OutputPath: string
              [<JsonPropertyName("namespace")>]
              Namespace: string
              [<JsonPropertyName("moduleName")>]
              ModuleName: string
              [<JsonPropertyName("includeJsonAttributes")>]
              IncludeJsonAttributes: bool
              [<JsonPropertyName("nameSuffix")>]
              NameSuffix: string
              [<JsonPropertyName("typeReplacements")>]
              TypeReplacements: TypeReplacementConfiguration seq
              [<JsonPropertyName("tableNameReplacements")>]
              TableNameReplacements: TableNameReplacement seq }

        and TypeReplacementConfiguration =
            { [<JsonPropertyName("matchValue")>]
              MatchValue: string
              [<JsonPropertyName("matchType")>]
              MatchType: string
              [<JsonPropertyName("replacementValue")>]
              ReplacementValue: string
              [<JsonPropertyName("replacementInitValue")>]
              ReplacementInitValue: string }

        and TableNameReplacement =
            { [<JsonPropertyName("name")>]
              Name: string
              [<JsonPropertyName("replacementName")>]
              ReplacementName: string }

    module Records =

        open Freql.Core.Utils.Extensions

        type RecordField =
            { Name: string
              Type: string
              Initialization: string }

        type Record =
            { Name: string
              Fields: RecordField list
              IncludeBlank: bool
              AdditionMethods: string list
              DocumentCommentLines: string list }

        let create (profile: Configuration.GeneratorProfile) (record: Record) =
            let fields =
                record.Fields
                |> List.mapi
                    (fun i rf ->
                        let name =
                            rf.Name
                            |> fun n -> n.ToPascalCase()
                            |> fun n ->
                                match profile.IncludeJsonAttributes with
                                | true -> $"[<JsonPropertyName(\"{n.ToCamelCase()}\")>] {n}"
                                | false -> n
                            |> fun n -> $"{n}: {rf.Type}"

                        match i with
                        | 0 when record.Fields.Length = 1 -> $"    {{ {name} }}"
                        | 0 -> $"    {{ {name}"
                        | _ when i = record.Fields.Length - 1 -> $"      {name} }}"
                        | _ -> $"      {name}")

            let blank =
                record.Fields
                |> List.mapi
                    (fun i rf ->
                        let name = rf.Name |> fun n -> n.ToPascalCase()

                        let content = $"{name} = {rf.Initialization}"

                        match i with
                        | 0 when record.Fields.Length = 1 -> $"        {{ {content} }}"
                        | 0 -> $"        {{ {content}"
                        | _ when i = record.Fields.Length - 1 -> $"          {content} }}"
                        | _ -> $"          {content}")
                |> fun r -> [ "    static member Blank() =" ] @ r

            match fields.Length with
            | 0 -> []
            //| 1 -> [ $"type {table.Name.ToPascalCase()} = {fields.[0].Trim()} }}" ]
            | _ ->
                [ yield! record.DocumentCommentLines
                  $"type {record.Name.ToPascalCase()} ="
                  yield! fields
                  ""
                  yield! blank
                  ""
                  yield! record.AdditionMethods ]

    module Functions =

        let create = ()

    open System.Text.RegularExpressions
    open Freql.Core.Utils

    [<RequireQualifiedAccess>]
    type MatchType =
        | Regex of string
        | String of string

        member mt.Test(value: string) =
            match mt with
            | Regex pattern -> Regex.IsMatch(value, pattern)
            | String str -> String.Equals(value, str, StringComparison.Ordinal)

    type TypeReplacement =
        { Match: MatchType
          ReplacementType: string
          Initialization: string option }

        static member Create(config: Configuration.TypeReplacementConfiguration) =
            { Match =
                  match config.MatchType with
                  | "regex" -> MatchType.Regex config.MatchValue
                  | _ -> MatchType.String config.MatchValue
              ReplacementType = config.ReplacementValue
              Initialization = Some config.ReplacementInitValue }

        member tr.Attempt(name: string, typeString: string) =
            match tr.Match.Test name with
            | true -> tr.ReplacementType
            | false -> typeString

        member tr.AttemptInitReplacement(name: string, initValue: string) =
            match tr.Initialization, tr.Match.Test name with
            | Some init, true -> init
            | _ -> initValue

    type GeneratorSettings<'Col> =
        { Imports: string list
          IncludeJsonAttributes: bool
          TypeReplacements: TypeReplacement list
          TypeHandler: TypeReplacement list -> 'Col -> string
          TypeInitHandler: TypeReplacement list -> 'Col -> string
          NameHandler: 'Col -> string
          InsertColumnFilter: 'Col -> bool
          ContextTypeName: string }

    type TableDetails<'Col> =
        { Name: string
          Sql: string
          Columns: 'Col list }

    let createRecord<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (table: TableDetails<'Col>)
        =

        let fields =
            table.Columns
            |> List.map
                (fun cd ->
                    ({ Name =
                           settings.NameHandler cd
                           |> fun n -> n.ToPascalCase()
                       Type = settings.TypeHandler settings.TypeReplacements cd
                       Initialization = settings.TypeInitHandler settings.TypeReplacements cd }: Records.RecordField))

        let createSql =
            [ "    static member CreateTableSql() = \"\"\""
              $"    {table.Sql}"
              "    \"\"\"" ]

        let selectFields =
            table.Columns
            |> List.map (fun cd -> $"          {table.Name}.`{settings.NameHandler cd}`")
            |> String.concat $",{Environment.NewLine}    "

        let selectSql =
            [ "    static member SelectSql() = \"\"\""
              $"    SELECT"
              $"{selectFields}"
              $"    FROM {table.Name}"
              "    \"\"\"" ]

        let tableName =
            $"    static member TableName() = \"{table.Name}\""

        ({ Name =
               match profile.TableNameReplacements
                     |> List.ofSeq
                     |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal)) with
               | Some tnr -> $"{tnr.ReplacementName.ToPascalCase()}"
               | None -> $"{table.Name.ToPascalCase()}"
           Fields = fields
           IncludeBlank = true
           AdditionMethods =
               [ yield! createSql
                 ""
                 yield! selectSql
                 ""
                 tableName ]
           DocumentCommentLines = [
               $"/// A record representing a row in the table `{table.Name}`."
           ] }: Records.Record)
        |> Records.create profile

    let indent value (text: string) = $"{String(' ', value * 4)}{text}"

    let indent1 text = indent 1 text

    let createRecords<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (tables: TableDetails<'Col> list)
        =

        // Create the core record.
        let records =
            tables
            |> List.map (fun t -> createRecord profile settings t @ [ "" ])
            |> List.concat
            |> List.map indent1

        [ $"namespace {profile.Namespace}"
          ""
          "open System"
          if settings.IncludeJsonAttributes then
              "open System.Text.Json.Serialization"
          yield!
              settings.Imports
              |> List.map (fun i -> $"open {i}")
          ""
          $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Tools."
          $"[<RequireQualifiedAccess>]"
          $"module Records =" ]
        @ records


    // Generate records/code for insert etc.
    //
    // Need -
    // Configuration change
    // * Property for skip fields (i.e. id which could be auto increment and not needed on inserts)
    //  "operations": [
    //      {
    //          "name": "test",
    //          "tableFilter": "",
    //          "init":
    //      }
    //  ]
    //
    // Example:
    // let insertFoo (parameters: AddFooParameters) (context: MySqlContext) =
    //     context.insert(Records.FooRecord.TableName(), parameters)

    let generateAddParameters<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (table: TableDetails<'Col>)
        =
        let name =
            match profile.TableNameReplacements
                  |> List.ofSeq
                  |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal)) with
            | Some tnr -> $"{tnr.ReplacementName.ToPascalCase()}"
            | None -> $"{table.Name.ToPascalCase()}"

        //let parametersRecords =
        table.Columns
        |> List.filter settings.InsertColumnFilter
        |> List.map
            (fun cd ->
                ({ Name =
                       settings.NameHandler cd
                       |> fun n -> n.ToPascalCase()
                   Type = settings.TypeHandler settings.TypeReplacements cd
                   Initialization = settings.TypeInitHandler settings.TypeReplacements cd }: Records.RecordField))
        |> fun f ->
            ({ Name = $"New{name}"
               Fields = f
               IncludeBlank = true
               AdditionMethods = []
               DocumentCommentLines = [
                   $"/// A record representing a new row in the table `{table.Name}`."
               ] }: Records.Record)
        |> Records.create profile


    let generateInsertOperation<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (table: TableDetails<'Col>)
        =

        let name =
            match profile.TableNameReplacements
                  |> List.ofSeq
                  |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal)) with
            | Some tnr -> $"{tnr.ReplacementName.ToPascalCase()}"
            | None -> $"{table.Name.ToPascalCase()}"

        [ $"let insert{name} (context: {settings.ContextTypeName}) (parameters: Parameters.New{name}) ="
          $"    context.Insert(\"{table.Name}\", parameters)" ]

    let generateSelectOperation<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (table: TableDetails<'Col>)
        =

        let name =
            match profile.TableNameReplacements
                  |> List.ofSeq
                  |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal)) with
            | Some tnr -> $"{tnr.ReplacementName.ToPascalCase()}"
            | None -> $"{table.Name.ToPascalCase()}"

       

        [ $"/// Select a `Records.{name}` from the table `{table.Name}`."
          $"/// Internally this calls `context.SelectSingleAnon<Records.{name}>` and uses Records.{name}.SelectSql()."
          $"/// The caller can provide extra string lines to create a query and boxed parameters."
          $"/// It is up to the caller to verify the sql and parameters are correct,"
          "/// this should be considered an internal function (not exposed in public APIs)."
          "/// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2..."
          $"/// Example: select{name}Record ctx \"WHERE `field` = @0\" [ box `value` ]"
          $"let select{name}Record (context: {settings.ContextTypeName}) (query: string list) (parameters: obj list) ="
          $"    let sql = [ Records.{name}.SelectSql() ] @ query |> buildSql"
          $"    context.SelectSingleAnon<Records.{name}>(sql, parameters)"
          ""
          $"/// Internally this calls `context.SelectAnon<Records.{name}>` and uses Records.{name}.SelectSql()."
          $"/// The caller can provide extra string lines to create a query and boxed parameters."
          $"/// It is up to the caller to verify the sql and parameters are correct,"
          "/// this should be considered an internal function (not exposed in public APIs)."
          "/// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2..."
          $"/// Example: select{name}Records ctx \"WHERE `field` = @0\" [ box `value` ]"
          $"let select{name}Records (context: {settings.ContextTypeName}) (query: string list) (parameters: obj list) ="
          $"    let sql = [ Records.{name}.SelectSql() ] @ query |> buildSql"
          $"    context.SelectAnon<Records.{name}>(sql, parameters)" ]

    let createParameters<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (tables: TableDetails<'Col> list)
        =

        // Create the core record.
        let records =
            tables
            |> List.map (fun t -> generateAddParameters profile settings t @ [ "" ])
            |> List.concat
            |> List.map indent1

        [ ""
          $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Tools."
          "[<RequireQualifiedAccess>]"
          "module Parameters =" ]
        @ records

    let generateOperations<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (tables: TableDetails<'Col> list)
        =

        let buildSql =
            "let buildSql (lines: string list) = lines |> String.concat Environment.NewLine"

        // Create the core record.
        let ops =
            tables
            |> List.map
                (fun t ->
                    [ yield! generateSelectOperation profile settings t
                      ""
                      yield! generateInsertOperation profile settings t
                      "" ])
            //
            |> List.concat
            |> List.map indent1

        [ $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Tools."
          "[<RequireQualifiedAccess>]"
          $"module Operations ="
          ""
          indent1 buildSql
          "" ]
        @ ops


