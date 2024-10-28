namespace Freql.Tools.Migrations

[<AutoOpen>]
module Common =

    type Migrations =
        { Name: string
          Comments: string option
          MigrationSql: string list
          RollbackSql: string list }
