namespace Freql.Tools.DatabaseComparisons

[<AutoOpen>]
module Common =

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
