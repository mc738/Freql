namespace Freql.MySql.Tools

open System
open Freql.MySql.Tools.MySqlMetaData
open Freql.Tools.DatabaseComparisons

module Migrations =

    // Table the metadata of a current database and a table comparison results and generate sql for the migration.
    let generateSql (database: MySqlDatabaseDefinition) (diff: TableComparisonResult list) =
        let tableMap =
            database.Tables
            |> List.map (fun t -> t.Name, t)
            |> Map.ofList

        diff
        |> List.map
            (fun tcr ->
                match tableMap.TryFind tcr.Name, tcr.Type with
                | Some table, TableComparisonResultType.Added ->
                    tableMap.TryFind tcr.Name
                    |> Option.bind (fun r -> Some r.Sql)
                | Some table, TableComparisonResultType.Altered ->
                    let colMap =
                        table.Columns
                        |> List.map (fun r -> r.Name, r)
                        |> Map.ofList

                    tcr.Columns
                    |> List.map
                        (fun c ->
                            match c.Type with
                            | ColumnComparisonResultType.Added ->
                                //
                                //
                                match colMap.TryFind c.Name with
                                | Some cd ->
                                    let notNull = match cd.NotNull with true -> " NOT NULL" | false -> "";

                                    // TODO add keys
                                                                        
                                    [ $"ALTER TABLE {tcr.Name}"
                                      $"ADD COLUMN {cd.Name} {cd.ColumnType}{notNull};" ]
                                    |> String.concat Environment.NewLine
                                    |> Some
                                | None -> failwith $"Column `{table.Name}.{c.Name}` not found"
                            | ColumnComparisonResultType.Altered columnDifferences ->
                                match colMap.TryFind c.Name with
                                | Some cd ->
                                    let notNull = match cd.NotNull with true -> " NOT NULL" | false -> "";
                                    [ $"ALTER TABLE {tcr.Name}"
                                      $"CHANGE COLUMN {cd.Name} {cd.Name} {cd.ColumnType}{notNull};" ]
                                    |> String.concat Environment.NewLine
                                    |> Some
                                | None -> failwith $"Column `{table.Name}.{c.Name}` not found"
                            | ColumnComparisonResultType.Removed -> 
                                    [ $"ALTER TABLE {tcr.Name}"
                                      $"DROP COLUMN {c.Name};" ]
                                    |> String.concat Environment.NewLine
                                    |> Some
                            | ColumnComparisonResultType.NoChange -> None)
                    |> List.choose id
                    |> String.concat Environment.NewLine
                    |> Some
                | Some _, TableComparisonResultType.Removed
                | None, TableComparisonResultType.Removed -> Some $"DROP TABLE {tcr.Name};"
                | Some _, TableComparisonResultType.NoChange                
                | None, TableComparisonResultType.NoChange -> None
                | None, _ -> failwith $"Table `{tcr.Name}` not found")
        |> List.choose id
