namespace Freql.Sqlite.Tools.DatabaseComparisons

[<AutoOpen>]
module Impl =

    open Freql.Sqlite.Tools.Core.SqliteMetadata
    open Freql.Tools.DatabaseComparisons
    
    let compareColumns (colA: SqliteColumnDefinition) (colB: SqliteColumnDefinition) =
        [ if (colA.Type = colB.Type) |> not then
              ColumnDifference.Type(colA.Type, colB.Type)
          if (colA.DefaultValue = colB.DefaultValue) |> not then
              ColumnDifference.DefaultValue(colA.DefaultValue, colB.DefaultValue)
          if (colA.NotNull = colB.NotNull) |> not then
              ColumnDifference.NotNull(colA.NotNull, colB.NotNull)
          if (colA.PrimaryKey = colB.PrimaryKey) |> not then
              ColumnDifference.Key(colA.PrimaryKey.ToString() |> Some, colB.PrimaryKey.ToString() |> Some) ]
        |> fun r ->
            match r.IsEmpty with
            | true -> ColumnComparisonResult.NoChange(colA.Name)
            | false -> ColumnComparisonResult.Altered(colA.Name, r)

    let settings =
        ({ GetTables = fun db -> db.Tables |> List.ofSeq
           GetColumns = fun table -> table.Columns |> List.ofSeq
           GetTableName = fun table -> table.Name
           GetColumnName = fun col -> col.Name
           CompareColumns = compareColumns }
        : ComparerSettings<SqliteDatabaseDefinition, SqliteTableDefinition, SqliteColumnDefinition>)
