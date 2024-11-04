namespace Freql.Sqlite.Tools.Core

module SqliteMetadata =

    open System.Text.Json.Serialization
    open Freql.Sqlite
    
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

