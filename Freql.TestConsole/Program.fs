// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Globalization
open System.IO
open System.Runtime.Serialization
open System.Text
open System.Text.RegularExpressions
open Freql.MySql
open Freql.Sqlite
open Freql.Tools.CodeGeneration
open Freql.Tools.DatabaseComparisons
open Microsoft.FSharp.Core
open Microsoft.FSharp.Reflection


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

    let ctx =
        SqliteContext.Create("C:\\ProjectData\\OpenReferralUk\\delete-me.db")

    ctx.ExecuteSqlNonQuery sql |> ignore

    [ { Id = 1; Bar = Some "baz" }
      { Id = 2; Bar = None } ]
    |> List.map (fun b -> ctx.Insert("foo", b))
    |> ignore


    printfn "%A" (ctx.Select<Foo>("foo"))

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
    |> List.map
        (fun t ->
            match t.Type with
            | TableComparisonResultType.Added -> printAdd $"+ Table `{t.Name}` added"
            | TableComparisonResultType.Altered ->
                printAltered $"! Table `{t.Name}` altered"

                t.Columns
                |> List.map
                    (fun c ->
                        match c.Type with
                        | ColumnComparisonResultType.Added -> printAdd $"+     Column `{c.Name}` added."
                        | ColumnComparisonResultType.Altered columnDifferences ->
                            printAltered $"!     Column `{c.Name}` altered."

                            columnDifferences
                            |> List.map
                                (fun cd ->
                                    match cd with
                                    | Type (o, n) -> printAltered $"!         Type changed. Old: {o} new: {n}"
                                    | DefaultValue (o, n) ->
                                        printAltered $"!         Default value changed. Old: {o} new: {n}"
                                    | NotNull (o, n) -> printAltered $"!         Not null changed. Old: {o} new: {n}"
                                    | Key (o, n) -> printAltered $"!         Key changed. Old: {o} new: {n}")
                            |> ignore
                        | ColumnComparisonResultType.Removed -> printRemove $"-     Column `{c.Name}` removed."
                        | ColumnComparisonResultType.NoChange -> printfn $"      Column `{c.Name}` unaltered.")
                |> ignore
            | TableComparisonResultType.Removed -> printRemove $"- Table `{t.Name}` removed."
            | TableComparisonResultType.NoChange -> printfn $"  Table `{t.Name}` unaltered.")
    |> ignore


type Name =
    { Foo: string
      Bar: int
      Baz: string option }

module Csv =

    type FormatAttribute(format: string) =

        inherit Attribute()

        member att.Format = format

    
    module Parsing =

        let inBounds (input: string) i = i >= 0 && i < input.Length

        let getChar (input: string) i =
            match inBounds input i with
            | true -> Some input.[i]
            | false -> None

        let readUntilChar (input: string) (c: Char) (start: int) (sb: StringBuilder) =
            let rec read i (sb: StringBuilder) =
                match getChar input i with
                | Some r when r = '"' && getChar input (i + 1) = Some '"' -> read (i + 2) <| sb.Append('"')
                | Some r when r = c -> Some <| (sb.ToString(), i)
                | Some r -> read (i + 1) <| sb.Append(r)
                | None -> Some <| (sb.ToString(), i + 1)

            read start sb

    module Records =
        
        open Freql.Core.Common.Types

        (*
        type RecordField =
            { Index: int
              Type: SupportedType
              Format: string option }


        type RecordDefinition =
            { Fields: RecordField list
              Type: Type }
            
            static member Create<'T>() =
                let t = typeof<'T>
                
                let fields =
                    t.GetProperties()
                    |> List.ofSeq
                    |> List.mapi
                        (fun i pi ->
                            // Look for format field.
                             match Attribute.GetCustomAttribute(pi, typeof<FormatAttribute>) with
                             | att when att <> null ->
                                 let fa = att :?> FormatAttribute

                                 {
                                     Index = i
                                     Type = SupportedType.FromType(pi.PropertyType)
                                     Format = Some fa.Format
                                 }
                             | _ ->
                                 {
                                     Index = i
                                     Type = SupportedType.FromType(pi.PropertyType)
                                     Format = None
                                 }
                            )
                {
                    Fields = fields
                    Type = t
                }
        *)    
        let tryGetAtIndex (values: string array) (i: int) =
            match i >= 0 && i < values.Length with
            | true -> Some values.[i]
            | false -> None
            
        //type Field = { Index: int; Value: obj }
         
        
        let createRecord<'T> (values: string list) =
            //let definition = RecordDefinition.Create<'T>()
            
            let getValue = values |> Array.ofList |> tryGetAtIndex
            
            
            let t = typeof<'T>
                
            let values =
                t.GetProperties()
                |> List.ofSeq
                |> List.mapi
                    (fun i pi ->
                         // Look for format field.
                         let format =
                             match Attribute.GetCustomAttribute(pi, typeof<FormatAttribute>) with
                             | att when att <> null -> Some <| (att :?> FormatAttribute).Format
                             | _ -> None
                         
                         let t = SupportedType.FromType(pi.PropertyType)
                         
                         //let value =
                         match getValue i, t with
                         | Some v, SupportedType.Blob -> failwith ""
                         | Some v, SupportedType.Boolean -> bool.Parse v :> obj
                         | Some v, SupportedType.Byte -> Byte.Parse v :> obj
                         | Some v, SupportedType.Char -> v.[0] :> obj
                         | Some v, SupportedType.Decimal -> Decimal.Parse v :> obj
                         | Some v, SupportedType.Double -> Double.TryParse v :> obj
                         | Some v, SupportedType.DateTime ->
                             match format with
                             | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                             | None -> DateTime.Parse(v)
                             :> obj
                         | Some v, SupportedType.Float -> failwith "" // Double.TryParse v :> obj
                         | Some v, SupportedType.Guid ->
                             match format with
                             | Some f -> Guid.ParseExact(v, f)
                             | None -> Guid.Parse(v)
                             :> obj
                         | Some v, SupportedType.Int -> Int32.Parse v :> obj
                         | Some v, SupportedType.Long -> Int64.Parse v :> obj
                         | Some v, SupportedType.Option _ -> failwith ""
                         | Some v, SupportedType.Short -> Int16.Parse v :> obj
                         | Some v, SupportedType.String -> v :> obj
                         | None, _ -> failwith ""
                        )
            
            let o = FSharpValue.MakeRecord(t, values |> Array.ofList)

            o :?> 'T
             

    let parseLine (input: string) =
        let sb = StringBuilder()

        let rec readBlock (i, sb: StringBuilder, acc: string list) =

            match Parsing.getChar input i with
            | Some c when c = '"' ->
                match Parsing.readUntilChar input '"' (i + 1) sb with
                | Some (r, i) ->
                    // Plus 2 to skip end " and ,
                    readBlock (i + 2, sb.Clear(), acc @ [ r ])
                | None -> acc
            // Read until " (not delimited).
            | Some _ ->
                match Parsing.readUntilChar input ',' i sb with
                | Some (r, i) -> readBlock (i + 1, sb.Clear(), acc @ [ r ])
                | None -> acc
            | None -> acc

        readBlock (0, sb, [])

open Csv

type CustomerPurchase = {
    RowId: int
    OrderId: string
    [<Format("M/d/yyyy")>]
    OrderDate: DateTime
    [<Format("M/d/yyyy")>]
    ShipDate: DateTime
    ShipMode: string
    CustomerId: string
    CustomerName: string
    Segment: string
    Country: string
    City: string
    State: string
    PostalCode: string
    Region: string
    ProductId: string
    Category: string
    SubCategory: string
    ProductName: string
    Sales: decimal
    Quantity: int
    Discount: decimal
    Profit: decimal
}

[<EntryPoint>]
let main argv =

    let r =
        DateTime.ParseExact("5/4/2017", "M/d/yyyy", CultureInfo.InvariantCulture)


    // CSV line
    let line =
        """9994,CA-2017-119914,5/4/2017,5/9/2017,Second Class,CC-12220,Chris Cortes,Consumer,United States,Westminster,California,92683,West,OFF-AP-10002684,Office Supplies,Appliances,"Acco 7-Outlet Masterpiece Power Center, Wihtout Fax/Phone Line Protection",243.16,2,0,72.948"""

    let line2 =
        "9993,CA-2017-121258,2/26/2017,3/3/2017,Standard Class,DB-13060,Dave Brooks,Consumer,United States,Costa Mesa,California,92627,West,OFF-PA-10004041,Office Supplies,Paper,\"It's Hot Message Books with Stickers, 2 3/4\"\" x 5\"\"\",29.6,4,0,13.32"

    let r = Csv.parseLine line |> Records.createRecord<CustomerPurchase>
    let r2 = Csv.parseLine line2 |> Records.createRecord<CustomerPurchase>

    // Read line char by char.

    // If char


    let ctx =
        SqliteContext.Create("C:\\ProjectData\\Freql\\test.db")

    ctx.CreateTable<Name>("test_table") |> ignore


    let context1 =
        MySqlContext.Connect("Server=localhost;Database=test_db_1;Uid=max;Pwd=letmein;")

    let context2 =
        MySqlContext.Connect("Server=localhost;Database=test_db_2;Uid=max;Pwd=letmein;")

    let foo =
        Freql.MySql.Tools.MySqlMetaData.get "test_db_1" context1

    let bar =
        Freql.MySql.Tools.MySqlMetaData.get "test_db_2" context2


    //let diff = Freql.MySql.Tools.StructuralComparison.compareDatabases foo bar

    let diff =
        Freql.Tools.DatabaseComparisons.compare Freql.MySql.Tools.MySqlDatabaseComparison.settings foo bar

    printDiff diff

    let migration =
        Freql.MySql.Tools.Migrations.generateSql foo bar diff

    migration
    |> List.map (fun m -> printfn $"{m}")
    |> ignore

    (*
    let context = MySqlContext.Connect("Server=localhost;Database=community_bridges_dev;Uid=max;Pwd=letmein;")

    let tables = Freql.MySql.Tools.MetaData.getTableData "community_bridges_dev" context

    let procedures = Freql.MySql.Tools.MetaData.getProcedures "community_bridges_dev" context

    let triggers = Freql.MySql.Tools.MetaData.getTriggers "sys" context

    let columns = Freql.MySql.Tools.MetaData.getColumns "community_bridges_dev" context

    let constraints = Freql.MySql.Tools.MetaData.getConstraints "community_bridges_dev" context

    printfn "%A" constraints

    let ctx =
        SqliteContext.Open("C:\\ProjectData\\Fiket\\prototypes\\workspace_v1.db")

    let dbd = Metadata.get ctx

    let gen =
        CodeGen.createRecords "Records" "My.Test.App" typeReplacements true dbd

    printfn $"{gen}"

    File.WriteAllText("C:\\Users\\44748\\fiket.io\\dotnet\\Fiket.Workspaces\\Records.fs", gen)

    *)

    0 // return an integer exit code
