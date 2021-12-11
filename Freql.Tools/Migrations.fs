namespace Freql.Tools

module Migrations =

    type Migrations =
        { Name: string
          Comments: string option
          MigrationSql: string list
          RollbackSql: string list }
