namespace Freql.MySql.Tools

open Freql.MySql.Tools.MySqlMetaData


module StructuralComparison =
        
    type ColumnDifference =
        | Type of string * string
        | DefaultValue of string option * string option
        | NotNull of bool * bool
        | Key of string option * string option

    type ForeignKeyDifference =
        | From of string * string
        | To of string * string

    type TableAlteration =
        | ColumnRemoved of string
        | ColumnAdded of string
        | ColumnAltered of string * ColumnDifference list
        | IndexRemoved of string
        | IndexAdded of string
        | IndexAltered of string
        | ForeignKeyRemoved of string
        | ForeignKeyAdded of string
        | ForeignKeyAltered of string

    type TableComparisonResult =
        | TableRemoved of string
        | TableAdded of string
        | TableAltered of string * TableAlteration list
 
    let compareColumns (colA: MySqlColumnDefinition) (colB: MySqlColumnDefinition) =
        [ if (colA.ColumnType = colB.ColumnType) |> not then
              ColumnDifference.Type(colA.ColumnType, colB.ColumnType)
          if (colA.DefaultValue = colB.DefaultValue) |> not then
              ColumnDifference.DefaultValue(colA.DefaultValue, colB.DefaultValue)
          if (colA.NotNull = colB.NotNull) |> not then
              ColumnDifference.NotNull(colA.NotNull, colB.NotNull)
          if (colA.Key = colB.Key) |> not then
              ColumnDifference.Key(colA.Key, colB.Key) ]
        |> fun r ->
            match r.IsEmpty with
            | true -> None
            | false -> TableAlteration.ColumnAltered (colA.Name, r) |> Some

    (*
    let compareForeignKeys (fkA: ForeignKeyDefinition) (fkB: ForeignKeyDefinition) =
        [ if (fkA.From = fkB.From) |> not then "" ]
    *)

    let compareTables (tableA: MySqlTableDefinition) (tableB: MySqlTableDefinition) =
        //let tableAMap = tableA.Columns |> List.map (fun cd -> cd.Name, cd) |> Map.ofList
        let tableBMap =
            tableB.Columns
            |> List.map (fun cd -> cd.Name, cd)
            |> Map.ofList

        tableA.Columns
        |> List.fold
            (fun (tbm: Map<string, MySqlColumnDefinition>, (acc: TableAlteration list)) cd ->
                match tbm.TryFind cd.Name with
                | Some tb ->
                    // If found:
                    // Compare columns.
                    // Remove from tbm (table b map).
                    let newTbm = tbm.Remove cd.Name

                    match compareColumns cd tb with
                    | Some r -> (newTbm, acc @ [ r ])
                    | None -> (newTbm, acc)
                | None -> (tbm, acc @ [ TableAlteration.ColumnRemoved cd.Name ]))
            (tableBMap, [])
        |> fun (notFound, results) ->
            results
            @ (notFound
               |> Map.toList
               |> List.map (fun (_, v) -> TableAlteration.ColumnAdded v.Name))
        |> fun r ->
            match r.IsEmpty with
            | true -> None
            | false -> TableComparisonResult.TableAltered (tableA.Name, r) |> Some

    let compareDatabases (databaseA: MySqlDatabaseDefinition) (databaseB: MySqlDatabaseDefinition) =
        let databaseBTableMap =
            databaseB.Tables
            |> List.map (fun t -> t.Name, t)
            |> Map.ofList

        databaseA.Tables
        |> List.fold
            (fun (tbm: Map<string, MySqlTableDefinition>, (acc: TableComparisonResult list)) td ->
                match tbm.TryFind td.Name with
                | Some tb ->
                    // If found:
                    // Compare columns.
                    // Remove from tbm (table b map).
                    let newTbm = tbm.Remove td.Name

                    match compareTables td tb with
                    | Some r -> (newTbm, acc @ [ r ])
                    | None -> (newTbm, acc)
                | None ->
                    (tbm,
                     acc
                     @ [ TableComparisonResult.TableRemoved td.Name ]))
            (databaseBTableMap, [])
        |> fun (notFound, results) ->
            results
            @ (notFound
               |> Map.toList
               |> List.map (fun (_, v) -> TableComparisonResult.TableAdded v.Name))

    
module Migrations =
    
    
    
    
    
    ()

