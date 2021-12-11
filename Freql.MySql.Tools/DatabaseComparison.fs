namespace Freql.MySql.Tools

open Freql.MySql.Tools.MySqlMetaData
open Freql.Tools.DatabaseComparisons

module MySqlDatabaseComparison =

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
            | true -> ColumnComparisonResult.NoChange(colA.Name)
            | false -> ColumnComparisonResult.Altered(colA.Name, r)

    let settings =
        ({ GetTables = fun db -> db.Tables |> List.ofSeq
           GetColumns = fun table -> table.Columns |> List.ofSeq
           GetTableName = fun table -> table.Name
           GetColumnName = fun col -> col.Name
           CompareColumns = compareColumns }: ComparerSettings<MySqlDatabaseDefinition, MySqlTableDefinition, MySqlColumnDefinition>)