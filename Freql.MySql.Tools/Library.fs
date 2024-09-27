namespace Freql.MySql.Tools

open System
open System
open System.Configuration
open System.Configuration
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Freql.Core.Common.Mapping
open Freql.MySql
open Freql.Tools.CodeGeneration
open Freql.Tools.CodeGeneration

module MySqlMetaData =

    module Internal =

        type TableRecord =
            { [<MappedField("TABLE_CATALOG")>]
              TableCatalog: string
              [<MappedField("TABLE_SCHEMA")>]
              TableSchema: string
              [<MappedField("TABLE_NAME")>]
              TableName: string
              [<MappedField("TABLE_TYPE")>]
              TableType: string
              [<MappedField("ENGINE")>]
              Engine: string option
              [<MappedField("VERSION")>]
              Version: int option
              [<MappedField("ROW_FORMAT")>]
              RowFormat: string option
              [<MappedField("TABLE_ROWS")>]
              TableRows: int
              [<MappedField("AVG_ROW_LENGTH")>]
              AverageRowLength: int
              [<MappedField("DATA_LENGTH")>]
              DataLength: int
              [<MappedField("MAX_DATA_LENGTH")>]
              MaxDataLength: int
              [<MappedField("INDEX_LENGTH")>]
              IndexLength: int
              [<MappedField("DATA_FREE")>]
              DataFree: int
              [<MappedField("AUTO_INCREMENT")>]
              AutoIncrement: int option
              [<MappedField("CREATE_TIME")>]
              CreateTime: DateTime
              [<MappedField("UPDATE_TIME")>]
              UpdateTime: DateTime option
              [<MappedField("CHECK_TIME")>]
              CheckTime: DateTime option
              [<MappedField("TABLE_COLLATION")>]
              TableCollation: string
              [<MappedField("CHECKSUM")>]
              Checksum: string option
              [<MappedField("CREATE_OPTIONS")>]
              CreateOptions: string option
              [<MappedField("TABLE_COMMENT")>]
              TableComment: string option }

        type RoutineRecord =
            { [<MappedField("SPECIFIC_NAME")>]
              SpecificName: string
              [<MappedField("ROUTINE_CATALOG")>]
              RoutineCatalog: string
              [<MappedField("ROUTINE_SCHEMA")>]
              RoutineSchema: string
              [<MappedField("ROUTINE_NAME")>]
              RoutineName: string
              [<MappedField("ROUTINE_TYPE")>]
              RoutineType: string
              [<MappedField("DATA_TYPE")>]
              DataType: string
              [<MappedField("CHARACTER_MAXIMUM_LENGTH")>]
              CharacterMaximumLength: int option
              [<MappedField("CHARACTER_OCTET_LENGTH")>]
              CharacterOctetLength: int option
              [<MappedField("NUMERIC_PRECISION")>]
              NumericPrecision: int option
              [<MappedField("NUMERIC_SCALE")>]
              NumericScale: int option
              [<MappedField("DATETIME_PRECISION")>]
              DateTimePrecision: int option
              [<MappedField("CHARACTER_SET_NAME")>]
              CharacterSetName: string option
              [<MappedField("COLLATION_NAME")>]
              CollationName: string option
              [<MappedField("DTD_IDENTIFIER")>]
              DtdIdentifier: string option
              [<MappedField("ROUTINE_BODY")>]
              RoutineBody: string
              [<MappedField("ROUTINE_DEFINITION")>]
              RoutineDefinition: string
              [<MappedField("EXTERNAL_NAME")>]
              ExternalName: string option
              [<MappedField("SPECIFIC_NAME")>]
              ExternalLanguage: string
              [<MappedField("PARAMETER_STYLE")>]
              ParameterStyle: string
              [<MappedField("IS_DETERMINISTIC")>]
              IsDeterministic: string
              [<MappedField("SQL_DATA_ACCESS")>]
              SqlDataAccess: string
              [<MappedField("SQL_PATH")>]
              SqlPath: string option
              [<MappedField("SECURITY_TYPE")>]
              SecurityType: string
              [<MappedField("CREATED")>]
              Created: DateTime
              [<MappedField("LAST_ALTERED")>]
              LastAltered: DateTime
              [<MappedField("SQL_MODE")>]
              SqlMode: string
              [<MappedField("ROUTINE_COMMENT")>]
              RoutineComments: string option
              [<MappedField("DEFINER")>]
              Definer: string
              [<MappedField("CHARACTER_SET_CLIENT")>]
              CharacterSetClient: string
              [<MappedField("COLLATION_CONNECTION")>]
              CollationConnection: string
              [<MappedField("DATABASE_COLLATION")>]
              DatabaseCollation: string }

        type TriggerRecord =
            { [<MappedField("TRIGGER_CATALOG")>]
              TriggerCatalog: string
              [<MappedField("TRIGGER_SCHEMA")>]
              TriggerSchema: string
              [<MappedField("TRIGGER_NAME")>]
              TriggerName: string
              [<MappedField("EVENT_MANIPULATION")>]
              EventManipulation: string
              [<MappedField("EVENT_OBJECT_CATALOG")>]
              EventObjectCatalog: string
              [<MappedField("EVENT_OBJECT_SCHEMA")>]
              EventObjectSchema: string
              [<MappedField("EVENT_OBJECT_TABLE")>]
              EventObjectTable: string
              [<MappedField("ACTION_ORDER")>]
              ActionOrder: int
              [<MappedField("ACTION_CONDITION")>]
              ActionCondition: string option
              [<MappedField("ACTION_STATEMENT")>]
              ActionStatement: string
              [<MappedField("ACTION_ORIENTATION")>]
              ActionOrientation: string
              [<MappedField("ACTION_TIMING")>]
              ActionTiming: string
              [<MappedField("ACTION_REFERENCE_OLD_TABLE")>]
              ActionReferenceOldTable: string option
              [<MappedField("ACTION_REFERENCE_NEW_TABLE")>]
              ActionReferenceNewTable: string option
              [<MappedField("ACTION_REFERENCE_OLD_ROW")>]
              ActionReferenceOldRow: string
              [<MappedField("ACTION_REFERENCE_NEW_ROW")>]
              ActionReferenceNewRow: string
              [<MappedField("CREATED")>]
              Created: DateTime
              [<MappedField("SQL_MODE")>]
              SqlMode: string
              [<MappedField("DEFINER")>]
              Definer: string
              [<MappedField("CHARACTER_SET_CLIENT")>]
              CharacterSetClient: string
              [<MappedField("COLLATION_CONNECTION")>]
              CollationConnection: string
              [<MappedField("DATABASE_COLLATION")>]
              DatabaseCollation: string }

        type ColumnRecord =
            { [<MappedField("TABLE_CATALOG")>]
              TableCatalog: string
              [<MappedField("TABLE_SCHEMA")>]
              TableSchema: string
              [<MappedField("TABLE_NAME")>]
              TableName: string
              [<MappedField("COLUMN_NAME")>]
              ColumnName: string
              [<MappedField("ORDINAL_POSITION")>]
              OrdinalPosition: int
              [<MappedField("COLUMN_DEFAULT")>]
              ColumnDefault: string option
              [<MappedField("IS_NULLABLE")>]
              IsNullable: string
              [<MappedField("DATA_TYPE")>]
              DataType: string
              [<MappedField("CHARACTER_MAXIMUM_LENGTH")>]
              CharacterMaximumLength: int option
              [<MappedField("CHARACTER_OCTET_LENGTH")>]
              CharacterOctetLength: int option
              [<MappedField("NUMERIC_PRECISION")>]
              NumericPrecision: int option
              [<MappedField("NUMERIC_SCALE")>]
              NumericScale: int option
              [<MappedField("DATETIME_PRECISION")>]
              DateTimePrecision: int option
              [<MappedField("CHARACTER_SET_NAME")>]
              CharacterSetName: string option
              [<MappedField("COLLATION_NAME")>]
              CollationName: string option
              [<MappedField("COLUMN_TYPE")>]
              ColumnType: string
              [<MappedField("COLUMN_KEY")>]
              ColumnKey: string option
              [<MappedField("EXTRA")>]
              Extra: string option
              [<MappedField("PRIVILEGES")>]
              Privileges: string
              [<MappedField("COLUMN_COMMENT")>]
              ColumnComment: string option
              [<MappedField("GENERATION_EXPRESSION")>]
              GenerationExpression: string option
              [<MappedField("SRS_ID")>]
              SrsId: string option }

        type ConstraintRecord =
            { [<MappedField("CONSTRAINT_CATALOG")>]
              ConstraintCatalog: string
              [<MappedField("CONSTRAINT_SCHEMA")>]
              ConstraintSchema: string
              [<MappedField("CONSTRAINT_NAME")>]
              ConstraintName: string
              [<MappedField("TABLE_CATALOG")>]
              TableCatalog: string
              [<MappedField("TABLE_SCHEMA")>]
              TableSchema: string
              [<MappedField("TABLE_NAME")>]
              TableName: string
              [<MappedField("COLUMN_NAME")>]
              ColumnName: string
              [<MappedField("ORDINAL_POSITION")>]
              OrdinalPosition: string
              [<MappedField("POSITION_IN_UNIQUE_CONSTRAINT")>]
              PositionInUniqueConstraint: string option
              [<MappedField("REFERENCED_TABLE_SCHEMA")>]
              ReferencedTableSchema: string option
              [<MappedField("REFERENCED_TABLE_NAME")>]
              ReferencedTableName: string option
              [<MappedField("REFERENCED_COLUMN_NAME")>]
              ReferenceColumnName: string option }

        type ByName = { Name: string }

        type TableSql =
            { [<MappedField("Table")>]
              Name: string
              [<MappedField("Create Table")>]
              Sql: string }

        let getTableData (databaseName: string) (context: MySqlContext) =
            let sql =
                """
            SELECT *
            FROM `information_schema`.`TABLES`
            WHERE TABLE_SCHEMA = @name AND TABLE_TYPE = 'BASE TABLE';
            """

            context.SelectVerbatim<TableRecord, ByName>(sql, { Name = databaseName })

        let getTableSql (databaseName: string) (context: MySqlContext) =
            // TODO this doesn't work with parameterized input.
            let sql = $"show create table {databaseName};"
            context.SelectSingleAnon<TableSql>(sql, [ databaseName ])

        let getProcedures (databaseName: string) (context: MySqlContext) =
            let sql =
                """
            SELECT *
            FROM `information_schema`.`ROUTINES`
            WHERE ROUTINE_SCHEMA = @name AND ROUTINE_TYPE = 'PROCEDURE';
            """

            context.SelectVerbatim<RoutineRecord, ByName>(sql, { Name = databaseName })

        let getTriggers (databaseName: string) (context: MySqlContext) =
            let sql =
                """
            SELECT *
            FROM `information_schema`.`TRIGGERS`
            WHERE `TRIGGER_SCHEMA` = @name
            """

            context.SelectVerbatim<TriggerRecord, ByName>(sql, { Name = databaseName })

        let getColumns (databaseName: string) (context: MySqlContext) =
            let sql =
                """
            SELECT *
            FROM information_schema.columns
            WHERE TABLE_SCHEMA = @name
            order by ordinal_position;
            """

            context.SelectVerbatim<ColumnRecord, ByName>(sql, { Name = databaseName })

        let getConstraints (databaseName: string) (context: MySqlContext) =
            let sql =
                """
            SELECT *
            FROM `information_schema`.`KEY_COLUMN_USAGE`
            WHERE `TABLE_SCHEMA` = @name
            """

            context.SelectVerbatim<ConstraintRecord, ByName>(sql, { Name = databaseName })

    type MySqlPrimaryKey = {
        [<JsonPropertyName("columns")>] Columns: string seq
    }
    
    type MySqlUniqueKey = {
        [<JsonPropertyName("name")>] Name: string
        [<JsonPropertyName("columns")>] Columns: string seq
    }
    
    type MySqlForeignKey =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("tableName")>] TableName: string
          [<JsonPropertyName("columnName")>] ColumnName: string
          [<JsonPropertyName("referenceTableName")>] ReferenceTableName: string
          [<JsonPropertyName("referenceColumnName")>] ReferenceColumnName: string }

    type MySqlColumnDefinition =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("notNull")>] NotNull: bool
          [<JsonPropertyName("dataType")>] DataType: string
          [<JsonPropertyName("columnType")>] ColumnType: string
          [<JsonPropertyName("defaultValue")>] DefaultValue: string option
          [<JsonPropertyName("autoIncrement")>] AutoIncrement: bool
          [<JsonPropertyName("key")>] Key: string option }

    type MySqlTableDefinition =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("sql")>] Sql: string
          [<JsonPropertyName("columns")>] Columns: MySqlColumnDefinition seq
          [<JsonPropertyName("primaryKey")>] PrimaryKey: MySqlPrimaryKey
          [<JsonPropertyName("uniqueKeys")>] UniqueKeys: MySqlUniqueKey seq
          [<JsonPropertyName("foreignKeys")>] ForeignKeys: MySqlForeignKey seq }

    type MySqlDatabaseDefinition =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("tables")>] Tables: MySqlTableDefinition seq }

    let get (databaseName: string) (context: MySqlContext) =
        let tables =
            let columns = Internal.getColumns databaseName context

            let constraints =
                Internal.getConstraints databaseName context

            Internal.getTableData databaseName context
            |> List.map
                (fun tr ->
                    let tableConstraints =
                        constraints
                        |> List.filter (fun c -> String.Equals(tr.TableName, c.TableName, StringComparison.Ordinal))

                    let tc =
                        columns
                        |> List.filter
                            (fun c -> String.Equals(c.TableName, tr.TableName, StringComparison.OrdinalIgnoreCase))
                        |> List.map
                            (fun c ->
                                ({ Name = c.ColumnName
                                   NotNull = String.Equals(c.IsNullable, "NO", StringComparison.OrdinalIgnoreCase)
                                   DataType = c.DataType
                                   ColumnType = c.ColumnType
                                   DefaultValue = c.ColumnDefault
                                   Key = c.ColumnKey
                                   AutoIncrement =
                                       c.Extra
                                       |> Option.bind
                                           (fun e ->
                                               String.Equals(e, "auto_increment", StringComparison.Ordinal)
                                               |> Some)
                                       |> Option.defaultValue false }: MySqlColumnDefinition))

                    let (primaryKey, uniqueKeys, foreignKeys) =
                        tableConstraints
                        |> List.fold
                            (fun (pkAcc, unAcc, fkAcc) con ->
                                match con.ReferencedTableName, con.ReferenceColumnName, con.ConstraintName with
                                | Some refTable, Some refCol, _ ->
                                    ({ Name = con.ConstraintName
                                       TableName = con.TableName
                                       ColumnName = con.ColumnName
                                       ReferenceTableName = refTable
                                       ReferenceColumnName = refCol }: MySqlForeignKey)
                                    |> fun fk -> (pkAcc, unAcc, fkAcc @ [ fk ])
                                | _, _, n when String.Equals(n, "PRIMARY", StringComparison.Ordinal) ->
                                    (pkAcc @ [ con ], unAcc, fkAcc)
                                | _ -> (pkAcc, unAcc @ [ con ], fkAcc))
                            ([], [], [])
                        |> fun (pks, uks, fks) ->
                            let pk =
                                pks
                                |> List.sortBy (fun pk -> pk.OrdinalPosition)
                                |> List.map (fun pk -> pk.ColumnName)
                                |> fun cn -> { Columns = cn }
                            
                            let ukc =
                                uks
                                |> List.groupBy (fun uk -> uk.ConstraintName)
                                |> List.map (fun (conName, records) ->
                                    records
                                    |> List.sortBy (fun r -> r.OrdinalPosition)
                                    |> List.map (fun r -> r.ColumnName)
                                    |> fun cn ->{ Name = conName; Columns = cn })
                            (pk, ukc, fks)

                    ({ Name = tr.TableName
                       Sql =
                           Internal.getTableSql tr.TableName context
                           |> Option.bind (fun r -> Some r.Sql)
                           |> Option.defaultValue ""
                       Columns = tc
                       PrimaryKey = primaryKey
                       UniqueKeys = uniqueKeys
                       ForeignKeys = foreignKeys }: MySqlTableDefinition))

        ({ Name = databaseName; Tables = tables }: MySqlDatabaseDefinition)

[<RequireQualifiedAccess>]
module MySqlCodeGeneration =

    open Freql.Tools.CodeGeneration
    open MySqlMetaData

    let getType (typeReplacements: TypeReplacement list) (cd: MySqlColumnDefinition) =
        match cd.DataType with
        | "char"
        | "varchar"
        | "tinytext"
        | "text"
        | "mediumtext"
        | "longtext"
        | "binary"
        | "varbinary"
        | "enum"
        | "set" -> "string"
        | "bit"
        | "tinyint" -> "byte"
        | "smallint" -> "int16"
        | "mediumint"
        | "int"
        | "integer" -> "int"
        | "bigint" -> "int64"
        | "decimal"
        | "dec"
        | "numeric"
        | "fixed" -> "decimal"
        | "float" -> "float"
        | "double"
        | "doubleprecision"
        | "real" -> "double"
        | "bool"
        | "boolean" -> "bool"
        | "date"
        | "datetime"
        | "timestamp"
        | "time"
        | "year" -> "DateTime"
        | "tinyblob"
        | "blob"
        | "mediumblob"
        | "longblob" -> "BlobField"
        | _ -> failwith $"Unknown type: {cd.DataType}"
        |> fun ts ->
            typeReplacements
            |> List.fold (fun ts tr -> tr.Attempt(cd.Name, ts)) ts
        |> fun s ->
            match cd.NotNull with
            | true -> s
            | false -> $"{s} option"

    let getTypeInit (typeReplacements: TypeReplacement list) (cd: MySqlColumnDefinition) =
        match cd.NotNull with
        | true ->
            match cd.DataType with
            | "char"
            | "varchar"
            | "tinytext"
            | "text"
            | "mediumtext"
            | "longtext"
            | "binary"
            | "varbinary"
            | "enum"
            | "set" -> "String.Empty"
            | "bit"
            | "tinyint" -> "0uy"
            | "smallint" -> "0s"
            | "mediumint"
            | "int"
            | "integer" -> "0"
            | "bigint" -> "0L"
            | "decimal"
            | "dec"
            | "numeric"
            | "fixed" -> "0m"
            | "float" -> "0."
            | "double"
            | "doubleprecision"
            | "real" -> "0."
            | "bool"
            | "boolean" -> "false"
            | "date"
            | "datetime"
            | "timestamp"
            | "time"
            | "year" -> "DateTime.UtcNow"
            | "tinyblob"
            | "blob"
            | "mediumblob"
            | "longblob" -> "BlobField.Empty()"
            | _ -> failwith $"Unknown type: {cd.DataType}"
            |> fun ts ->
                typeReplacements
                |> List.fold (fun ts tr -> tr.AttemptInitReplacement(cd.Name, ts)) ts
        | false -> "None"

    let generatorSettings (profile: Configuration.GeneratorProfile) =
        ({ Imports = [ "Freql.Core.Common"; "Freql.MySql" ]
           IncludeJsonAttributes = true
           TypeReplacements =
               profile.TypeReplacements
               |> List.ofSeq
               |> List.map (fun tr -> TypeReplacement.Create tr)
           TypeHandler = getType
           TypeInitHandler = getTypeInit
           NameHandler = fun cd -> cd.Name
           InsertColumnFilter =
               fun cd ->
                   String.Equals(cd.Name, "id", StringComparison.InvariantCulture)
                   |> not
           ContextTypeName = "MySqlContext" }: GeneratorSettings<MySqlColumnDefinition>)

    let createTableDetails (table: MySqlTableDefinition) =
        ({ Name = table.Name
           Sql = table.Sql
           Columns = table.Columns |> List.ofSeq
           BespokeMethodsHandler = fun _ -> None }: TableDetails<MySqlColumnDefinition>)

    /// Generate F# records from a list of MySqlTableDefinition records.
    let generate (profile: Configuration.GeneratorProfile) (database: MySqlDatabaseDefinition) =
        database.Tables
        |> List.ofSeq
        |> List.map (fun t -> createTableDetails t)
        |> fun t ->
            let settings = generatorSettings profile
            [
               createRecords profile settings t
               createParameters profile settings t
               generateOperations profile settings t
            ]
            |> List.concat
            |> String.concat Environment.NewLine