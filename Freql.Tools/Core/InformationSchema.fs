namespace Freql.Tools.Core

open System
open Freql.Core.Attributes

module InformationSchema =

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
        
        /// <summary>
        /// Sql for use in an anonymous query.
        /// The sql expects one parameter for the schema name.
        /// </summary>
        static member AnonSql() =
                """
            SELECT *
            FROM `information_schema`.`TABLES`
            WHERE TABLE_SCHEMA = @0 AND TABLE_TYPE = 'BASE TABLE';
            """
            
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
