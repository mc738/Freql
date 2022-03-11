// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Text.RegularExpressions
open Freql.MySql
open Freql.Sqlite
open Freql.Tools.CodeGeneration
open Freql.Tools.DatabaseComparisons


// Convert an obj to amd obj option.
let (|SomeObj|_|) =
    let ty = typedefof<option<_>>

    fun (a: obj) ->
        let aty = a.GetType()
        let v = aty.GetProperty("Value")

        if aty.IsGenericType
           && aty.GetGenericTypeDefinition() = ty then
            if a = null then
                None
            else
                Some(v.GetValue(a, [||]))
        else
            None

type Foo = { Id: int; Bar: string option }

let optionTest _ =
    let get value =
        let result =
            Regex.Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)")

        match result.Success with
        | true -> Some result.Value
        | false -> None


    let sql =
        """
    CREATE TABLE foo (
		id INTEGER,
        bar TEXT
	);
    """

    let qh =
        QueryHandler.Create("C:\\ProjectData\\OpenReferralUk\\delete-me.db")

    qh.ExecuteSqlNonQuery sql |> ignore

    [ { Id = 1; Bar = Some "baz" }
      { Id = 2; Bar = None } ]
    |> List.map (fun b -> qh.Insert("foo", b))
    |> ignore


    printfn "%A" (qh.Select<Foo>("foo"))

    printfn "%A" (typeof<string option>.FullName |> get)
    printfn "%A" (typeof<int option>.FullName |> get)
    printfn "%A" (typeof<int64 option>.FullName |> get)
    printfn "%A" (typeof<DateTime option>.FullName |> get)
    printfn "%A" (typeof<byte option>.FullName |> get)
    printfn "%A" (typeof<Guid option>.FullName |> get)

(*
    let test (x: obj) =
        match x with
        | null -> None
        | SomeObj(x1) -> Some x1
        | _ -> None

    printfn "%A" (test ((Some "Hello world") :> obj))
    printfn "%A" (test ((Some 1) :> obj))
    printfn "%A" (test ((None) :> obj))
    printfn "%A" (test ("" :> obj))
    *)

let typeReplacements =
    ([ { Match = MatchType.Regex "created_on"
         ReplacementType = "DateTime"
         Initialization = Some "DateTime.UtcNow" }
       { Match = MatchType.Regex "updated_on"
         ReplacementType = "DateTime"
         Initialization = Some "DateTime.UtcNow" }
       { Match = MatchType.String "reference"
         ReplacementType = "Guid"
         Initialization = Some "Guid.NewGuid()" }
       { Match = MatchType.String "active"
         ReplacementType = "bool"
         Initialization = Some "true" } ]: TypeReplacement list)

let printDiff (results: TableComparisonResult list) =
      
    let cprintfn color str =
        Console.ForegroundColor <- color
        printfn $"{str}"
        Console.ResetColor()

    let printRemove str = cprintfn ConsoleColor.Red str

    let printAdd str = cprintfn ConsoleColor.Green str
    
    let printAltered str = cprintfn ConsoleColor.DarkYellow str
    
    results
    |> List.map (fun t ->
        match t.Type with
        | TableComparisonResultType.Added ->
            printAdd $"+ Table `{t.Name}` added"
        | TableComparisonResultType.Altered ->
            printAltered $"! Table `{t.Name}` altered"

            t.Columns
            |> List.map (fun c ->
                match c.Type with
                | ColumnComparisonResultType.Added ->
                    printAdd $"+     Column `{c.Name}` added."
                | ColumnComparisonResultType.Altered columnDifferences ->
                    printAltered $"!     Column `{c.Name}` altered."
                    columnDifferences
                    |> List.map (fun cd ->
                        match cd with
                        | Type (o, n) ->
                            printAltered $"!         Type changed. Old: {o} new: {n}"
                        | DefaultValue (o, n) ->
                            printAltered $"!         Default value changed. Old: {o} new: {n}"
                        | NotNull (o, n) ->
                            printAltered $"!         Not null changed. Old: {o} new: {n}"
                        | Key (o, n) ->
                            printAltered $"!         Key changed. Old: {o} new: {n}")
                    |> ignore
                | ColumnComparisonResultType.Removed ->
                    printRemove $"-     Column `{c.Name}` removed."
                | ColumnComparisonResultType.NoChange ->
                    printfn $"      Column `{c.Name}` unaltered.")
            |> ignore
        | TableComparisonResultType.Removed ->
            printRemove $"- Table `{t.Name}` removed."
        | TableComparisonResultType.NoChange ->
            printfn $"  Table `{t.Name}` unaltered.")
    |> ignore


type Name = {
    Foo: string
    Bar: int
    Baz: string option
}    

[<EntryPoint>]
let main argv =

    let ctx = SqliteContext.Create("C:\\ProjectData\\Freql\\test.db")
    
    ctx.CreateTable<Name>("test_table") |> ignore
    
    
    let context1 = MySqlContext.Connect("Server=localhost;Database=test_db_1;Uid=max;Pwd=letmein;")
    let context2 = MySqlContext.Connect("Server=localhost;Database=test_db_2;Uid=max;Pwd=letmein;")
    
    let foo = Freql.MySql.Tools.MySqlMetaData.get "test_db_1" context1
    let bar = Freql.MySql.Tools.MySqlMetaData.get "test_db_2" context2
    
    
    //let diff = Freql.MySql.Tools.StructuralComparison.compareDatabases foo bar
    
    let diff = Freql.Tools.DatabaseComparisons.compare Freql.MySql.Tools.MySqlDatabaseComparison.settings foo bar
    
    printDiff diff
        
    let migration = Freql.MySql.Tools.Migrations.generateSql foo bar diff
    
    migration |> List.map (fun m -> printfn $"{m}") |> ignore
    
    (*
    let context = MySqlContext.Connect("Server=localhost;Database=community_bridges_dev;Uid=max;Pwd=letmein;")
    
    let tables = Freql.MySql.Tools.MetaData.getTableData "community_bridges_dev" context

    let procedures = Freql.MySql.Tools.MetaData.getProcedures "community_bridges_dev" context
    
    let triggers = Freql.MySql.Tools.MetaData.getTriggers "sys" context
    
    let columns = Freql.MySql.Tools.MetaData.getColumns "community_bridges_dev" context
    
    let constraints = Freql.MySql.Tools.MetaData.getConstraints "community_bridges_dev" context
    
    printfn "%A" constraints
    
    let qh =
        QueryHandler.Open("C:\\ProjectData\\Fiket\\prototypes\\workspace_v1.db")

    let dbd = Metadata.get qh

    let gen =
        CodeGen.createRecords "Records" "My.Test.App" typeReplacements true dbd

    printfn $"{gen}"
    
    File.WriteAllText("C:\\Users\\44748\\fiket.io\\dotnet\\Fiket.Workspaces\\Records.fs", gen)

    *)
    
    0 // return an integer exit code
