﻿namespace Freql.Sqlite.Tools

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
              /// <summary>
              /// The column the foreign key is linked to.
              /// This can be null.
              /// In that case it means it has been emitted and can be assumed to be the primary key of the table.
              /// </summary>
              /// <remarks>
              /// https://youtrack.jetbrains.com/issue/DBE-19348
              /// </remarks>
              To: string option
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
          /// <summary>
          /// If none, this can be assumed to be the PK of the referenced table.
          /// </summary>
          [<JsonPropertyName("to")>]
          To: string option
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

open SqliteMetadata

[<RequireQualifiedAccess>]
module SqliteCodeGeneration =

    open Freql.Tools.CodeGeneration
    open Freql.Core.Utils.Extensions

    [<RequireQualifiedAccess>]
    module TopSection =

        let utilsModule =
            [ "module private Utils ="
              ""
              "    open System.Text.RegularExpressions"
              ""
              "    let updateCheckIfExists (update: bool) (name: string) (value: string) ="
              "        match update with"
              "        | false -> value"
              "        | true ->"
              "            let regex = Regex($\"CREATE {name}\")"
              ""
              "            regex.Replace(value, $\"CREATE {name} IF NOT EXISTS\", 1)" ]

        let generate (ctx: GeneratorContext<SqliteTableDefinition, SqliteColumnDefinition>) = [ yield! utilsModule ] |> Some

    [<RequireQualifiedAccess>]
    module BottomSection =
        
        open System.Collections.Generic

        // This was initially generated via AI, so might need some testing.
        let topologicalSort (records: TableDetails<SqliteTableDefinition, SqliteColumnDefinition> list ) =
            let graph = Dictionary<string, string list>()
            let inDegree = Dictionary<string, int>()

            // Initialize the graph and in-degree count
            for record in records do
                graph.[record.OriginalName] <- record.Table.ForeignKeys |> Seq.map (fun fk -> fk.Table) |> List.ofSeq
                inDegree.[record.OriginalName] <- 0

            for record in records do
                for fk in record.Table.ForeignKeys do
                    if inDegree.ContainsKey(fk.Table) then
                        inDegree.[fk.Table] <- inDegree.[fk.Table] + 1
                    else
                        inDegree.[fk.Table] <- 1

            let sortedList = List<string>()
            
            // Queue for records with no incoming edges
            let queue = Queue<string>()
            for kvp in inDegree do
                if kvp.Value = 0 then
                    queue.Enqueue(kvp.Key)
            
            while queue.Count > 0 do
                let node = queue.Dequeue()
                sortedList.Add(node)

                for neighbor in graph.[node] do
                    inDegree.[neighbor] <- inDegree.[neighbor] - 1
                    if inDegree.[neighbor] = 0 then
                        queue.Enqueue(neighbor)

            // Return the sorted records
            sortedList
            |> Seq.map (fun id -> records |> List.find (fun r -> r.OriginalName = id))
            |> Seq.toList
            |> List.rev
        
        let createInitializationSql (orderedTableNames: string list) =
            match orderedTableNames.Length with
            | 0 -> [ "    let sql (checkIfExists: bool) = []" ]
            | 1 ->
                [ $"    let sql (checkIfExists: bool) = [ Records.{orderedTableNames.Head}.InitializationSql checkIfExists ]" ]
            | _ ->
                [ "    let sql (checkIfExists: bool) ="
                  yield!
                      orderedTableNames
                      |> List.mapi (fun i name ->
                          let startBlock =
                              match i with
                              | 0 -> "        [ "
                              | _ -> "          "

                          let endBlock =
                              match i with
                              | _ when orderedTableNames.Length - 1 = i -> " ]"
                              | _ -> ""

                          $"{startBlock}Records.{name}.InitializationSql checkIfExists{endBlock}")
                  "        |> List.concat" ]

        let generate (ctx: GeneratorContext<SqliteTableDefinition, SqliteColumnDefinition>) =
            let orderedTableNames =
                ctx.Tables
                |> topologicalSort
                |> List.map (fun t -> t.ReplacementName |> Option.defaultValue t.OriginalName |> fun tn -> tn.ToPascalCase())

            [ "[<RequireQualifiedAccess>]"
              "module Initialization ="
              yield! createInitializationSql orderedTableNames
              ""
              "    let run (checkIfExists: bool) (ctx: SqliteContext) ="
              "        sql checkIfExists |> List.iter (ctx.ExecuteSqlNonQuery >> ignore)"
              ""
              "    let runInTransaction (checkIfExists: bool) (ctx: SqliteContext) ="
              "        ctx.ExecuteInTransaction(fun t -> sql checkIfExists |> List.iter (t.ExecuteSqlNonQuery >> ignore))" ]
            |> Some

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
           TypeReplacements = profile.TypeReplacements |> List.ofSeq |> List.map TypeReplacement.Create
           TypeHandler = getType
           TypeInitHandler = getTypeInit
           NameHandler = fun cd -> cd.Name
           InsertColumnFilter = fun _ -> true
           (* TODO make this config
               fun cd ->
                   String.Equals(cd.Name, "id", StringComparison.InvariantCulture)
                   |> not
               *)
           ContextTypeName = "SqliteContext"
           BespokeTopSectionHandler = TopSection.generate
           BespokeBottomSectionHandler = BottomSection.generate }
        : GeneratorSettings<SqliteTableDefinition, SqliteColumnDefinition>)

    let generateIndexes (ctx: TableGeneratorContext) (table: SqliteTableDefinition) =
        let indexes = table.Indexes |> Seq.choose (fun i -> i.Sql)

        let indexCount = indexes |> Seq.length

        if indexCount = 0 then
            [ "static member CreateIndexesSql() = []" ]
        else
            [ "static member CreateIndexesSql() ="
              yield!
                  indexes
                  |> Seq.mapi (fun i index ->
                      [ if (i = 0) then "    [ \"\"\"" else "      "
                        $"      {index}"
                        if (i = indexCount - 1) then
                            "      \"\"\" ]"
                        else
                            "      \"\"\"" ])
                  |> Seq.collect id ]

    let generateTriggers (ctx: TableGeneratorContext) (table: SqliteTableDefinition) =
        let triggers = table.Triggers |> Seq.choose (fun i -> i.Sql)

        let triggerCount = triggers |> Seq.length

        if triggerCount = 0 then
            [ "static member CreateTriggersSql() = []" ]
        else
            [ "static member CreateTriggersSql() ="
              yield!
                  triggers
                  |> Seq.mapi (fun i trigger ->
                      [ if (i = 0) then "    [ \"\"\"" else "      "
                        $"      {trigger}"
                        if (i = triggerCount - 1) then
                            "      \"\"\" ]"
                        else
                            "      \"\"\"" ])
                  |> Seq.collect id ]

    let generateInitializeSql (ctx: TableGeneratorContext) (table: SqliteTableDefinition) =
        [ "static member InitializationSql(checkIfExists: bool) ="
          $"    [ {ctx.Name}.CreateTableSql()"
          "      |> Utils.updateCheckIfExists checkIfExists \"TABLE\""
          "      yield!"
          $"          {ctx.Name}.CreateIndexesSql()"
          "          |> List.map (Utils.updateCheckIfExists checkIfExists \"INDEX\")"
          "      yield!"
          $"          {ctx.Name}.CreateTriggersSql()"
          "          |> List.map (Utils.updateCheckIfExists checkIfExists \"TRIGGER\")  ]" ]

    let createTableDetails (profile: Configuration.GeneratorProfile) (table: SqliteTableDefinition) =

        ({ OriginalName = table.Name
           ReplacementName =
             profile.TableNameReplacements
             |> List.ofSeq
             |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal))
             |> Option.map (fun tnr -> tnr.ReplacementName)
           Sql = table.Sql
           Table = table 
           Columns = table.Columns |> List.ofSeq
           BespokeMethodsHandler =
             fun ctx ->
                 [ yield! generateIndexes ctx table
                   ""
                   yield! generateTriggers ctx table
                   ""
                   yield! generateInitializeSql ctx table ]
                 |> Some }
        : TableDetails<SqliteTableDefinition, SqliteColumnDefinition>)

    /// Generate F# records from a list of MySqlTableDefinition records.
    let generate (profile: Configuration.GeneratorProfile) (database: SqliteDatabaseDefinition) =
        let settings = generatorSettings profile

        database.Tables
        |> List.ofSeq
        |> List.map (createTableDetails profile)
        |> generateCode profile settings

[<RequireQualifiedAccess>]
module SqliteDatabaseComparison =

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
