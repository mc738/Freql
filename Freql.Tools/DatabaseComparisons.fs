namespace Freql.Tools

module DatabaseComparisons =

    type ColumnDifference =
        | Type of string * string
        | DefaultValue of string option * string option
        | NotNull of bool * bool
        | Key of string option * string option

    [<RequireQualifiedAccess>]
    type ColumnComparisonResultType =
        | Added
        | Removed
        | Altered of ColumnDifference list
        | NoChange

    type ColumnComparisonResult =
        { Name: string
          Type: ColumnComparisonResultType }

        static member Added(colName) =
            { Name = colName
              Type = ColumnComparisonResultType.Added }

        static member Remove(colName) =
            { Name = colName
              Type = ColumnComparisonResultType.Removed }

        static member NoChange(colName) =
            { Name = colName
              Type = ColumnComparisonResultType.NoChange }

        static member Altered(colName, diff) =
            { Name = colName
              Type = ColumnComparisonResultType.Altered diff }

    [<RequireQualifiedAccess>]
    type TableComparisonResultType =
        | Added
        | Removed
        | Altered
        | NoChange

    type ComparerSettings<'Database, 'Table, 'Col> =
        { GetTables: 'Database -> 'Table list
          GetColumns: 'Table -> 'Col list
          GetTableName: 'Table -> string
          GetColumnName: 'Col -> string
          CompareColumns: 'Col -> 'Col -> ColumnComparisonResult }

    type TableComparisonResult =
        { Name: string
          Columns: ColumnComparisonResult list
          Type: TableComparisonResultType }

        static member Added(tableName) =
            { Name = tableName
              Columns = []
              Type = TableComparisonResultType.Added }

        static member Removed(tableName) =
            { Name = tableName
              Columns = []
              Type = TableComparisonResultType.Removed }

        static member Compared(tableName, cols) =
            { Name = tableName
              Columns = cols
              Type =
                  cols
                  |> List.fold
                      (fun unaltered cr ->
                          match unaltered, cr.Type with
                          | false, _ -> false
                          | true, ColumnComparisonResultType.NoChange -> true
                          | true, _ -> false)
                      true
                  |> fun r ->
                      match r with
                      | true -> TableComparisonResultType.NoChange
                      | false -> TableComparisonResultType.Altered }

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