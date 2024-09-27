namespace Freql.Sqlite.Tools

open System
open System.Text.Json.Serialization
open Freql.Sqlite
open Freql.Tools.DatabaseComparisons

module Exceptions =

    type SqliteCodeGenerationException(message: string, innerException: Exception) =

        inherit Exception(message, innerException)

module SqliteMetadata =

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

        let getMasterRecords (ctx: SqliteContext) =
            try
                let sql = "SELECT * FROM sqlite_master ORDER BY name;"

                ctx.SelectVerbatim<MasterRecord, unit>(sql, ())
            with ex ->
                raise (
                    Exceptions.SqliteCodeGenerationException(
                        $"Failed to retrieve master records. Error: {ex.Message}",
                        ex
                    )
                )

        let getTableRecord (name: string) (ctx: SqliteContext) =
            try
                ctx.SelectAnon<TableInfoRecord>($"PRAGMA table_info({name});", [ name ])
            with ex ->
                raise (
                    Exceptions.SqliteCodeGenerationException(
                        $"Failed to retrieve table info for {name}. Error: {ex.Message}",
                        ex
                    )
                )

        let getIndexRecord (name: string) (ctx: SqliteContext) =
            try
                ctx.SelectSingleAnon<IndexRecord>($"PRAGMA index_info({name});", [ name ])
            with ex ->
                raise (
                    Exceptions.SqliteCodeGenerationException(
                        $"Failed to retrieve index info for {name}. Error: {ex.Message}",
                        ex
                    )
                )

        let getForeignKeysRecord (name: string) (ctx: SqliteContext) =
            try
                ctx.SelectAnon<ForeignKeyRecord>($"PRAGMA foreign_key_list({name});", [ name ])
            with ex ->
                raise (
                    Exceptions.SqliteCodeGenerationException(
                        $"Failed to retrieve foreign key list for {name}. Error: {ex.Message}",
                        ex
                    )
                )

        let getFunctionRecords (ctx: SqliteContext) =
            try
                ctx.Select<FunctionRecord>("PRAGMA function_list;")
            with ex ->
                raise (
                    Exceptions.SqliteCodeGenerationException(
                        $"Failed to retrieve function list. Error: {ex.Message}",
                        ex
                    )
                )

    type SqliteColumnDefinition =
        { [<JsonPropertyName("cid")>]
          CID: int
          [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("type")>]
          Type: string
          [<JsonPropertyName("notNull")>]
          NotNull: bool
          [<JsonPropertyName("defaultValue")>]
          DefaultValue: string option
          [<JsonPropertyName("primaryKey")>]
          PrimaryKey: bool }

    type SqliteIndexDefinition =
        { [<JsonPropertyName("tableName")>]
          TableName: string
          [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("seqNo")>]
          SeqNo: int
          [<JsonPropertyName("sql")>]
          Sql: string option }

    type SqliteForeignKeyDefinition =
        { [<JsonPropertyName("id")>]
          Id: int
          [<JsonPropertyName("seq")>]
          Seq: int
          [<JsonPropertyName("table")>]
          Table: string
          [<JsonPropertyName("from")>]
          From: string
          [<JsonPropertyName("to")>]
          To: string
          [<JsonPropertyName("onUpdate")>]
          OnUpdate: string
          [<JsonPropertyName("onDelete")>]
          OnDelete: string
          [<JsonPropertyName("match")>]
          Match: string }

    type SqliteTriggerDefinition =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("sql")>]
          Sql: string option }

    type SqliteTableDefinition =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("sql")>]
          Sql: string
          [<JsonPropertyName("columns")>]
          Columns: SqliteColumnDefinition seq
          [<JsonPropertyName("foreignKeys")>]
          ForeignKeys: SqliteForeignKeyDefinition seq
          [<JsonPropertyName("indexes")>]
          Indexes: SqliteIndexDefinition seq
          [<JsonPropertyName("triggers")>]
          Triggers: SqliteTriggerDefinition seq }

    type SqliteDatabaseDefinition =
        { [<JsonPropertyName("tables")>]
          Tables: SqliteTableDefinition seq }

    let createIndexDefinition (tableName: string) (record: Internal.IndexRecord) (sql: string option) =
        { TableName = tableName
          Name = record.Name
          SeqNo = record.Seqno
          Sql = sql }

    let get (ctx: SqliteContext) =
        let masterRecords = Internal.getMasterRecords ctx

        let tables = masterRecords |> List.filter (fun mr -> mr.Type = "table")

        let views = masterRecords |> List.filter (fun mr -> mr.Type = "view")

        let indexes = masterRecords |> List.filter (fun mr -> mr.Type = "index")

        let triggers = masterRecords |> List.filter (fun mr -> mr.Type = "trigger")

        tables
        |> List.map (fun tmr ->
            let indexes =
                indexes
                |> List.filter (fun fmr -> fmr.TblName = tmr.TblName)
                |> List.map (fun fmr ->
                    match Internal.getIndexRecord fmr.Name ctx with
                    | Some ir -> createIndexDefinition fmr.TblName ir fmr.Sql
                    | None -> failwith $"Missing index record: {fmr.Name}")

            let foreignKeys =
                Internal.getForeignKeysRecord tmr.TblName ctx
                |> List.map (fun fkr ->
                    ({ Id = fkr.Id
                       Seq = fkr.Seq
                       Table = fkr.Table
                       From = fkr.From
                       To = fkr.To
                       OnUpdate = fkr.OnUpdate
                       OnDelete = fkr.OnDelete
                       Match = fkr.Match }
                    : SqliteForeignKeyDefinition))

            let columns =
                Internal.getTableRecord tmr.Name ctx
                |> List.map (fun tir ->

                    ({ CID = tir.Cid
                       Name = tir.Name
                       Type = tir.Type
                       NotNull = tir.Notnull
                       DefaultValue = tir.DfltValue
                       PrimaryKey = tir.Pk }
                    : SqliteColumnDefinition))

            ({ Name = tmr.TblName
               Sql = tmr.Sql |> Option.defaultValue ""
               Columns = columns
               ForeignKeys = foreignKeys
               Indexes = indexes
               Triggers =
                 triggers
                 |> List.filter (fun mr -> mr.TblName = tmr.TblName)
                 |> List.map (fun t -> { Name = t.Name; Sql = t.Sql }) }
            : SqliteTableDefinition))
        |> fun tdl -> ({ Tables = tdl }: SqliteDatabaseDefinition)


[<RequireQualifiedAccess>]
module SqliteCodeGeneration =

    open Freql.Tools.CodeGeneration
    open SqliteMetadata

    let getType (typeReplacements: TypeReplacement list) (cd: SqliteColumnDefinition) =
        match cd.Type.ToUpper() with
        | "TEXT" -> "string"
        | "INTEGER" -> "int64"
        | "NUMBER" -> "int64"
        | "REAL" -> "decimal"
        | "BLOB" -> "BlobField"
        | _ -> failwith $"Unknown type: {cd.Type}"
        |> fun ts -> typeReplacements |> List.fold (fun ts tr -> tr.Attempt(cd.Name, ts)) ts
        |> fun s ->
            match cd.NotNull with
            | true -> s
            | false -> $"{s} option"

    let getTypeInit (typeReplacements: TypeReplacement list) (cd: SqliteColumnDefinition) =
        match cd.NotNull with
        | true ->
            match cd.Type with
            | "TEXT" -> "String.Empty"
            | "INTEGER" -> "0L"
            | "NUMBER" -> "0L"
            | "REAL" -> "0m"
            | "BLOB" -> "BlobField.Empty()"
            | _ -> failwith $"Unknown type: {cd.Type}"
            |> fun ts ->
                typeReplacements
                |> List.fold (fun ts tr -> tr.AttemptInitReplacement(cd.Name, ts)) ts
        | false -> "None"

    let generatorSettings (profile: Configuration.GeneratorProfile) =
        ({ Imports = [ "Freql.Core.Common"; "Freql.Sqlite" ]
           IncludeJsonAttributes = true
           TypeReplacements =
             profile.TypeReplacements
             |> List.ofSeq
             |> List.map (fun tr -> TypeReplacement.Create tr)
           TypeHandler = getType
           TypeInitHandler = getTypeInit
           NameHandler = fun cd -> cd.Name
           InsertColumnFilter = fun _ -> true
           (* TODO make this config
               fun cd ->
                   String.Equals(cd.Name, "id", StringComparison.InvariantCulture)
                   |> not
               *)
           ContextTypeName = "SqliteContext" }
        : GeneratorSettings<SqliteColumnDefinition>)
    
    let generateIndexes (table: SqliteTableDefinition) =
        let indexCount = table.Indexes |> Seq.length
        if indexCount = 0 then
            [ "static member CreateIndexesSql() = []" ]
        else
            [ "static member CreateIndexesSql() ="
              yield!
                  table.Indexes
                  |> Seq.mapi (fun i index ->
                      [ if (i = 0) then "    [ \"\"\"" else "      "
                        $"      {index.Sql}"
                        if (i = indexCount - 1) then "      \"\"\" ]" else "      \"\"\"" ])
                  |> Seq.collect id ]

    let createTableDetails (table: SqliteTableDefinition) =
        ({ Name = table.Name
           Sql = table.Sql
           Columns = table.Columns |> List.ofSeq
           BespokeMethodsHandler = fun _ -> [ yield! generateIndexes table ] |> Some }
        : TableDetails<SqliteColumnDefinition>)

    /// Generate F# records from a list of MySqlTableDefinition records.
    let generate (profile: Configuration.GeneratorProfile) (database: SqliteDatabaseDefinition) =
        database.Tables
        |> List.ofSeq
        |> List.map (fun t -> createTableDetails t)
        |> fun t ->
            let settings = generatorSettings profile

            [ createRecords profile settings t
              createParameters profile settings t
              generateOperations profile settings t ]
            |> List.concat
            |> String.concat Environment.NewLine

[<RequireQualifiedAccess>]
module SqliteDatabaseComparison =

    open SqliteMetadata

    let compareColumns (colA: SqliteColumnDefinition) (colB: SqliteColumnDefinition) =
        [ if (colA.Type = colB.Type) |> not then
              ColumnDifference.Type(colA.Type, colB.Type)
          if (colA.DefaultValue = colB.DefaultValue) |> not then
              ColumnDifference.DefaultValue(colA.DefaultValue, colB.DefaultValue)
          if (colA.NotNull = colB.NotNull) |> not then
              ColumnDifference.NotNull(colA.NotNull, colB.NotNull)
          if (colA.PrimaryKey = colB.PrimaryKey) |> not then
              ColumnDifference.Key(colA.PrimaryKey.ToString() |> Some, colB.PrimaryKey.ToString() |> Some) ]
        |> fun r ->
            match r.IsEmpty with
            | true -> ColumnComparisonResult.NoChange(colA.Name)
            | false -> ColumnComparisonResult.Altered(colA.Name, r)

    let settings =
        ({ GetTables = fun db -> db.Tables |> List.ofSeq
           GetColumns = fun table -> table.Columns |> List.ofSeq
           GetTableName = fun table -> table.Name
           GetColumnName = fun col -> col.Name
           CompareColumns = compareColumns }
        : ComparerSettings<SqliteDatabaseDefinition, SqliteTableDefinition, SqliteColumnDefinition>)
