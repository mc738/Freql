// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Text.RegularExpressions
open System.Text.RegularExpressions
open Freql.Core.Utils
open Freql.MySql
open Freql.Sqlite
open Freql.Sqlite.Tools
open Freql.Sqlite.Tools.CodeGen
open Freql.Sqlite.Tools.Metadata
open MySqlX.XDevAPI.Relational

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
         ReplacementType = "DateTime" }
       { Match = MatchType.Regex "updated_on"
         ReplacementType = "DateTime" }
       { Match = MatchType.String "reference"
         ReplacementType = "Guid" }
       { Match = MatchType.String "active"
         ReplacementType = "bool" } ]: TypeReplacement list)

[<EntryPoint>]
let main argv =
    
    let context = MySqlContext.Connect("Server=localhost;Database=community_bridges_dev;Uid=max;Pwd=letmein;")
    
    let tables = Freql.MySql.Tools.MetaData.getTableData "community_bridges_dev" context

    let procedures = Freql.MySql.Tools.MetaData.getProcedures "community_bridges_dev" context
    
    let triggers = Freql.MySql.Tools.MetaData.getTriggers "sys" context
    
    let columns = Freql.MySql.Tools.MetaData.getColumns "community_bridges_dev" context
    
    let constraints = Freql.MySql.Tools.MetaData.getConstraints "community_bridges_dev" context
    
    printfn "%A" constraints
    
    let qh =
        QueryHandler.Open("C:\\ProjectData\\Fiket\\data\\dev\\Events\\event_store.db")

    let dbd = Metadata.get qh


    let gen =
        CodeGen.createRecords "Records" "My.Test.App" typeReplacements true dbd

    printfn $"{gen}"

    0 // return an integer exit code
