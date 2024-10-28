namespace Freql.Tools.DatabaseComparisons

[<AutoOpen>]
module Impl =
     
    let compareTables<'Database, 'Table, 'Col>
        (settings: ComparerSettings<'Database, 'Table, 'Col>)
        (table1: 'Table)
        (table2: 'Table)
        =
        //let tableAMap = tableA.Columns |> List.map (fun cd -> cd.Name, cd) |> Map.ofList
        let table2Map =
            settings.GetColumns table2
            |> List.map (fun cd -> settings.GetColumnName cd, cd)
            |> Map.ofList

        settings.GetColumns table1
        |> List.fold
            (fun (tbm: Map<string, 'Col>, (acc: ColumnComparisonResult list)) cd ->
                match tbm.TryFind(settings.GetColumnName cd) with
                | Some tb ->
                    // If found:
                    // Compare columns.
                    // Remove from tbm (table b map).
                    let newTbm = tbm.Remove(settings.GetColumnName cd)
                    (newTbm, acc @ [ settings.CompareColumns cd tb ])
                | None ->
                    (tbm,
                     acc
                     @ [ ColumnComparisonResult.Remove(settings.GetColumnName cd) ]))
            (table2Map, [])
        |> fun (notFound, results) ->
            results
            @ (notFound
               |> Map.toList
               |> List.map (fun (_, v) -> ColumnComparisonResult.Added(settings.GetColumnName v)))
        |> fun cols -> TableComparisonResult.Compared(settings.GetTableName table2, cols)

    let compare<'Database, 'Table, 'Col>
        (settings: ComparerSettings<'Database, 'Table, 'Col>)
        (database1: 'Database)
        (database2: 'Database)
        =
        let database2TableMap =
            settings.GetTables database2
            |> List.map (fun t -> settings.GetTableName t, t)
            |> Map.ofList

        settings.GetTables database1
        |> List.fold
            (fun (tbm: Map<string, 'Table>, (acc: TableComparisonResult list)) td ->
                match tbm.TryFind(settings.GetTableName td) with
                | Some tb ->
                    // If found:
                    // Compare columns.
                    // Remove from tbm (table b map).
                    let newTbm = tbm.Remove(settings.GetTableName td)
                    (newTbm, acc @ [ compareTables settings td tb ])
                | None ->
                    (tbm,
                     acc
                     @ [ TableComparisonResult.Removed(settings.GetTableName td) ]))
            (database2TableMap, [])
        |> fun (notFound, results) ->
            results
            @ (notFound
               |> Map.toList
               |> List.map (fun (_, v) -> TableComparisonResult.Added(settings.GetTableName v)))

