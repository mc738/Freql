namespace Freql.Sqlite.Tools

open System
open System.Text.RegularExpressions
open Freql.Core.Utils
open Freql.Sqlite
open Org.BouncyCastle.Bcpg.OpenPgp

module Metadata =

    module Internal =

        type MasterRecord =
            { Type: string
              Name: string
              TblName: string
              Rootpage: int
              Sql: string option }

        type TableInfoRecord =
            { Cid: int
              Name: string
              Type: string
              Notnull: bool
              DfltValue: string option
              Pk: bool }

        type ForeignKeyRecord =
            { Id: int
              Seq: int
              Table: string
              From: string
              To: string
              OnUpdate: string
              OnDelete: string
              Match: string }

        type FunctionRecord =
            { Name: string
              Builtin: bool
              Type: string
              Enc: string
              Narg: int
              Flags: int }

        type IndexRecord = { Seqno: int; Cid: int; Name: string }

        let getMasterRecords (qh: QueryHandler) =
            let sql =
                "SELECT * FROM sqlite_master ORDER BY name;"

            qh.SelectVerbatim<MasterRecord, unit>(sql, ())

        let getTableRecord (name: string) (qh: QueryHandler) =
            qh.SelectAnon<TableInfoRecord>($"PRAGMA table_info({name});", [ name ])

        let getIndexRecord (name: string) (qh: QueryHandler) =
            qh.SelectSingleAnon<IndexRecord>($"PRAGMA index_info({name});", [ name ])

        let getForeignKeysRecord (name: string) (qh: QueryHandler) =
            qh.SelectAnon<ForeignKeyRecord>($"PRAGMA foreign_key_list({name});", [ name ])

        let getFunctionRecords (qh: QueryHandler) =
            qh.Select<FunctionRecord>("PRAGMA function_list;")



    [<RequireQualifiedAccess>]
    type ColumnType =
        | Text
        | Integer
        | Real
        | Blob

    type ColumnDefinition =
        { CID: int
          Name: string
          Type: string //ColumnType
          NotNull: bool
          DefaultValue: string option
          PrimaryKey: bool }

    type IndexDefinition =
        { TableName: string
          Name: string
          SeqNo: int }

    type ForeignKeyDefinition =
        { Id: int
          Seq: int
          Table: string
          From: string
          To: string
          OnUpdate: string
          OnDelete: string
          Match: string }

    type TableDefinition =
        { Name: string
          Sql: string
          Columns: ColumnDefinition list
          ForeignKeys: ForeignKeyDefinition list
          Indexes: IndexDefinition list }

    type DatabaseDefinition = { Tables: TableDefinition list }

    let createIndexDefinition (tableName: string) (record: Internal.IndexRecord) =
        { TableName = tableName
          Name = record.Name
          SeqNo = record.Seqno }

    let get (qh: QueryHandler) =
        let masterRecords = Internal.getMasterRecords qh

        let tables =
            masterRecords
            |> List.filter (fun mr -> mr.Type = "table")

        let views =
            masterRecords
            |> List.filter (fun mr -> mr.Type = "view")

        let indexes =
            masterRecords
            |> List.filter (fun mr -> mr.Type = "index")

        tables
        |> List.map
            (fun tmr ->
                let indexes =
                    indexes
                    |> List.filter (fun fmr -> fmr.TblName = tmr.TblName)
                    |> List.map
                        (fun fmr ->
                            match Internal.getIndexRecord fmr.Name qh with
                            | Some ir -> createIndexDefinition fmr.TblName ir
                            | None -> failwith $"Missing index record: {fmr.Name}")

                let foreignKeys =
                    Internal.getForeignKeysRecord tmr.TblName qh
                    |> List.map
                        (fun fkr ->
                            ({ Id = fkr.Id
                               Seq = fkr.Seq
                               Table = fkr.Table
                               From = fkr.From
                               To = fkr.To
                               OnUpdate = fkr.OnUpdate
                               OnDelete = fkr.OnDelete
                               Match = fkr.Match }: ForeignKeyDefinition))

                let columns =
                    Internal.getTableRecord tmr.Name qh
                    |> List.map
                        (fun tir ->

                            ({ CID = tir.Cid
                               Name = tir.Name
                               Type = tir.Type
                               NotNull = tir.Notnull
                               DefaultValue = tir.DfltValue
                               PrimaryKey = tir.Pk }: ColumnDefinition))

                ({ Name = tmr.TblName
                   Sql = tmr.Sql |> Option.defaultValue ""
                   Columns = columns
                   ForeignKeys = foreignKeys
                   Indexes = indexes }: TableDefinition))
        |> fun tdl -> ({ Tables = tdl }: DatabaseDefinition)

module CodeGen =

    open Metadata


    [<RequireQualifiedAccess>]
    type MatchType =
        | Regex of string
        | String of string

        member mt.Test(value: string) =
            match mt with
            | Regex pattern -> Regex.IsMatch(value, pattern)
            | String str -> String.Equals(value, str)

    type TypeReplacement =
        { Match: MatchType
          ReplacementType: string }

        member tr.Attempt(name: string, typeString: string) =
            match tr.Match.Test name with
            | true -> tr.ReplacementType
            | false -> typeString


    let getType (typeReplacements: TypeReplacement list) (cd: ColumnDefinition) =
        match cd.Type with
        | "TEXT" -> "string"
        | "NUMBER" -> "int64"
        | "REAL" -> "decimal"
        | "BLOB" -> "BlobField"
        | _ -> failwith $"Unknown type: {cd.Type}"
        |> fun ts ->
            typeReplacements
            |> List.fold (fun ts tr -> tr.Attempt(cd.Name, ts)) ts
        |> fun s ->
            match cd.NotNull with
            | true -> s
            | false -> $"{s} option"

    let createRecord (typeReplacements: TypeReplacement list) (includeJsonAttribute: bool) (table: TableDefinition) =
        let fields =
            table.Columns
            |> List.mapi
                (fun i cd ->
                    let name =
                        cd.Name.ToPascalCase()
                        |> fun n ->
                            match includeJsonAttribute with
                            | true -> $"[<JsonPropertyName(\"{n.ToCamelCase()}\")>] {n}"
                            | false -> n
                        |> fun n -> $"{n}: {getType typeReplacements cd}"

                    match i with
                    | 0 -> $"    {{ {name}"
                    | _ when i = table.Columns.Length - 1 -> $"      {name} }}"
                    | _ -> $"      {name}")


        match fields.Length with
        | 0 -> []
        | 1 -> [ $"type {table.Name.ToPascalCase()} = {fields.[0].Trim()} }}" ]
        | _ ->
            [ $"type {table.Name.ToPascalCase()} ="
              yield! fields ]

    let indent value (text: string) = $"{String(' ', value * 4)}{text}"

    let indent1 text = indent 1 text

    let createRecords
        (name: string)
        (ns: string)
        (typeReplacements: TypeReplacement list)
        (includeJsonAttributes: bool)
        (database: DatabaseDefinition)
        =

        let records =
            database.Tables
            |> List.map
                (fun t ->
                    createRecord typeReplacements includeJsonAttributes t
                    @ [ "" ])
            |> List.concat
            |> List.map indent1

        [ ns
          ""
          "open System"
          if includeJsonAttributes then
              "open System.Text.Json"
          "open Freql.Core.Utils"
          "open Freql.Sqlite"
          ""
          $"module {name} =" ]
        @ records
        |> String.concat Environment.NewLine

module StructureComparison =

    open Metadata

    type ColumnDifference =
        | Type of string * string
        | DefaultValue of string option * string option
        | NotNull of bool * bool
        | PrimaryKey of bool * bool

    type ForeignKeyDifference =
        | From of string * string
        | To of string * string

    type TableAlteration =
        | ColumnRemoved of string
        | ColumnAdded of string
        | ColumnAltered of ColumnDifference list
        | IndexRemoved of string
        | IndexAdded of string
        | IndexAltered of string
        | ForeignKeyRemoved of string
        | ForeignKeyAdded of string
        | ForeignKeyAltered of string

    type TableComparisonResult =
        | TableRemoved of string
        | TableAdded of string
        | TableAltered of TableAlteration list

    let compareColumns (colA: ColumnDefinition) (colB: ColumnDefinition) =
        [ if (colA.Type = colB.Name) |> not then
              ColumnDifference.Type(colA.Type, colB.Type)
          if (colA.DefaultValue = colB.DefaultValue) |> not then
              ColumnDifference.DefaultValue(colA.DefaultValue, colB.DefaultValue)
          if (colA.NotNull = colB.NotNull) |> not then
              ColumnDifference.NotNull(colA.NotNull, colB.NotNull)
          if (colA.PrimaryKey = colB.PrimaryKey) |> not then
              ColumnDifference.PrimaryKey(colA.PrimaryKey, colB.PrimaryKey) ]
        |> fun r ->
            match r.IsEmpty with
            | true -> None
            | false -> TableAlteration.ColumnAltered r |> Some

    let compareForeignKeys (fkA: ForeignKeyDefinition) (fkB: ForeignKeyDefinition) =
        [
            if (fkA.From = fkB.From) |> not then ""
        ]
    
    let compareTables (tableA: TableDefinition) (tableB: TableDefinition) =
        //let tableAMap = tableA.Columns |> List.map (fun cd -> cd.Name, cd) |> Map.ofList
        let tableBMap =
            tableB.Columns
            |> List.map (fun cd -> cd.Name, cd)
            |> Map.ofList

        tableA.Columns
        |> List.fold
            (fun (tbm: Map<string, ColumnDefinition>, (acc: TableAlteration list)) cd ->
                match tbm.TryFind cd.Name with
                | Some tb ->
                    // If found:
                    // Compare columns.
                    // Remove from tbm (table b map).
                    let newTbm = tbm.Remove cd.Name

                    match compareColumns cd tb with
                    | Some r -> (newTbm, acc @ [ r ])
                    | None -> (newTbm, acc)
                | None -> (tbm, acc @ [ TableAlteration.ColumnRemoved cd.Name ]))
            (tableBMap, [])
        |> fun (notFound, results) ->
            results
            @ (notFound
               |> Map.toList
               |> List.map (fun (_, v) -> TableAlteration.ColumnAdded v.Name))
        |> fun r ->
            match r.IsEmpty with
            | true -> None
            | false -> TableComparisonResult.TableAltered r |> Some

    let compareDatabases (databaseA: DatabaseDefinition) (databaseB: DatabaseDefinition) =
        let databaseBTableMap =
            databaseB.Tables
            |> List.map (fun t -> t.Name, t)
            |> Map.ofList

        databaseA.Tables
        |> List.fold
            (fun (tbm: Map<string, TableDefinition>, (acc: TableComparisonResult list)) td ->
                match tbm.TryFind td.Name with
                | Some tb ->
                    // If found:
                    // Compare columns.
                    // Remove from tbm (table b map).
                    let newTbm = tbm.Remove td.Name

                    match compareTables td tb with
                    | Some r -> (newTbm, acc @ [ r ])
                    | None -> (newTbm, acc)
                | None ->
                    (tbm,
                     acc
                     @ [ TableComparisonResult.TableRemoved td.Name ]))
            (databaseBTableMap, [])
        |> fun (notFound, results) ->
            results
            @ (notFound
               |> Map.toList
               |> List.map (fun (_, v) -> TableComparisonResult.TableAdded v.Name))
