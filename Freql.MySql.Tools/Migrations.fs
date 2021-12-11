namespace Freql.MySql.Tools

open System
open Freql.MySql.Tools.MySqlMetaData
open Freql.Tools.DatabaseComparisons

module Migrations =

    // Table the metadata of a current database and a table comparison results and generate sql for the migration.
    let generateSql (databaseA: MySqlDatabaseDefinition) (databaseB: MySqlDatabaseDefinition) (diff: TableComparisonResult list) =
        let (tableAMap, tableAForeignKeys) =
            databaseA.Tables
            |> List.fold (fun (mapAcc, fkAcc) t -> mapAcc @ [t.Name, t], fkAcc @  t.ForeignKeys) ([], [])
            |> fun (kv, fkc) ->
                kv |> Map.ofList, fkc
        
        
        let (tableBMap, tableBForeignKeys) =
            databaseB.Tables
            |> List.fold (fun (mapAcc, fkAcc) t -> mapAcc @ [t.Name, t], fkAcc @  t.ForeignKeys) ([], [])
            |> fun (kv, fkc) ->
                kv |> Map.ofList, fkc
                
        diff
        |> List.map
            (fun tcr ->
                match tcr.Type with
                | TableComparisonResultType.Added ->
                    tableBMap.TryFind tcr.Name
                    |> Option.bind (fun r -> Some r.Sql)
                | TableComparisonResultType.Altered ->
                    match tableBMap.TryFind tcr.Name with
                    | Some table ->
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
                                        let notNull = match cd.NotNull with true -> " NOT NULL" | false -> ""
                                        
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
                    | None -> None
                | TableComparisonResultType.Removed ->
                    // Find any foreign keys to and from this table in database A.
                    // Generate sql to drop them and then the drop table query.
                    tableAForeignKeys
                    |> List.filter (fun fk ->
                        String.Equals(fk.TableName, tcr.Name) ||
                        String.Equals(fk.ReferenceTableName, tcr.Name))
                    |> List.map (fun fk -> $"ALTER TABLE {fk.TableName} DROP CONSTRAINT {fk.Name};")
                    |> fun r -> r @ [ $"DROP TABLE {tcr.Name};" ]
                    |> String.concat Environment.NewLine
                    |> Some
                    
                | TableComparisonResultType.NoChange -> None)
        |> List.choose id
