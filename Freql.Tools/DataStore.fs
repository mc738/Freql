namespace Freq.MySql.Tools.DataStore

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 11/12/2021 14:22:26 (utc) via Freql.Sqlite.Tools.
module Records =
    type DatabaseDefinitionRecord =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("databaseType")>] DatabaseType: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              DatabaseType = String.Empty
              CreatedOn = DateTime.UtcNow }
    
        static member CreateTableSql() = """
        CREATE TABLE database_definitions (
	reference TEXT NOT NULL,
	name TEXT NOT NULL,
	database_type TEXT NOT NULL,
	created_on TEXT NOT NULL,
	CONSTRAINT database_definitions_PK PRIMARY KEY (reference),
	CONSTRAINT database_definitions_UN UNIQUE (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              reference,
              name,
              database_type,
              created_on
        FROM database_definitions
        """
    
        static member TableName() = "database_definitions"
    
    type MetadataRecord =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("database")>] Database: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("metadataBlob")>] MetadataBlob: BlobField
          [<JsonPropertyName("metadataHash")>] MetadataHash: string
          [<JsonPropertyName("verison")>] Verison: int64 }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Database = String.Empty
              CreatedOn = DateTime.UtcNow
              MetadataBlob = BlobField.Empty()
              MetadataHash = String.Empty
              Verison = 0L }
    
        static member CreateTableSql() = """
        CREATE TABLE metadata (
	reference TEXT NOT NULL,
	"database" TEXT NOT NULL,
	created_on TEXT NOT NULL,
	metadata_blob BLOB NOT NULL,
	metadata_hash TEXT NOT NULL,
	verison INTEGER NOT NULL,
	CONSTRAINT metadata_PK PRIMARY KEY (reference),
	CONSTRAINT metadata_UN UNIQUE ("database",verison),
	CONSTRAINT metadata_FK FOREIGN KEY ("database") REFERENCES database_definitions(reference)
)
        """
    
        static member SelectSql() = """
        SELECT
              reference,
              database,
              created_on,
              metadata_blob,
              metadata_hash,
              verison
        FROM metadata
        """
    
        static member TableName() = "metadata"
    
    type MigrationRecord =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("comments")>] Comments: string option
          [<JsonPropertyName("migrationSqlBlob")>] MigrationSqlBlob: BlobField
          [<JsonPropertyName("migrationHash")>] MigrationHash: string
          [<JsonPropertyName("rollbackSql")>] RollbackSql: int64
          [<JsonPropertyName("rollbackHash")>] RollbackHash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("fromReference")>] FromReference: string
          [<JsonPropertyName("toReference")>] ToReference: string }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              Comments = None
              MigrationSqlBlob = BlobField.Empty()
              MigrationHash = String.Empty
              RollbackSql = 0L
              RollbackHash = String.Empty
              CreatedOn = DateTime.UtcNow
              FromReference = String.Empty
              ToReference = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE migrations (
	reference TEXT NOT NULL,
	name TEXT NOT NULL,
	comments TEXT,
	migration_sql_blob BLOB NOT NULL,
	migration_hash TEXT NOT NULL,
	rollback_sql INTEGER NOT NULL,
	rollback_hash TEXT NOT NULL,
	created_on TEXT NOT NULL,
	from_reference TEXT NOT NULL,
	to_reference TEXT NOT NULL,
	CONSTRAINT migrations_PK PRIMARY KEY (reference),
	CONSTRAINT migrations_FK FOREIGN KEY (from_reference) REFERENCES metadata(reference),
	CONSTRAINT migrations_FK_1 FOREIGN KEY (to_reference) REFERENCES metadata(reference)
)
        """
    
        static member SelectSql() = """
        SELECT
              reference,
              name,
              comments,
              migration_sql_blob,
              migration_hash,
              rollback_sql,
              rollback_hash,
              created_on,
              from_reference,
              to_reference
        FROM migrations
        """
    
        static member TableName() = "migrations"
    
module Operations =
    type AddDatabaseDefinitionParameters =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("databaseType")>] DatabaseType: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              DatabaseType = String.Empty
              CreatedOn = DateTime.UtcNow }
    
    let insertDatabaseDefinition (parameters: AddDatabaseDefinitionParameters) (context: QueryHandler) =
        context.Insert("database_definitions", parameters)
    
    type AddMetadataParameters =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("database")>] Database: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("metadataBlob")>] MetadataBlob: BlobField
          [<JsonPropertyName("metadataHash")>] MetadataHash: string
          [<JsonPropertyName("verison")>] Verison: int64 }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Database = String.Empty
              CreatedOn = DateTime.UtcNow
              MetadataBlob = BlobField.Empty()
              MetadataHash = String.Empty
              Verison = 0L }
    
    let insertMetadata (parameters: AddMetadataParameters) (context: QueryHandler) =
        context.Insert("metadata", parameters)
    
    type AddMigrationParameters =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("comments")>] Comments: string option
          [<JsonPropertyName("migrationSqlBlob")>] MigrationSqlBlob: BlobField
          [<JsonPropertyName("migrationHash")>] MigrationHash: string
          [<JsonPropertyName("rollbackSql")>] RollbackSql: int64
          [<JsonPropertyName("rollbackHash")>] RollbackHash: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("fromReference")>] FromReference: string
          [<JsonPropertyName("toReference")>] ToReference: string }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              Comments = None
              MigrationSqlBlob = BlobField.Empty()
              MigrationHash = String.Empty
              RollbackSql = 0L
              RollbackHash = String.Empty
              CreatedOn = DateTime.UtcNow
              FromReference = String.Empty
              ToReference = String.Empty }
    
    let insertMigration (parameters: AddMigrationParameters) (context: QueryHandler) =
        context.Insert("migrations", parameters)
    