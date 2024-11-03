namespace Freql.Sqlite.Tools.DatabaseBindings

[<AutoOpen>]
module Impl =

    open System
    open Freql.Sqlite.Tools.Core.SqliteMetadata
    open Freql.Tools.DatabaseBindings

    let getType (typeReplacements: TypeReplacement list) (cd: SqliteColumnDefinition) =
        match cd.Type.ToUpper() with
        | "TEXT" -> "string"
        | "INTEGER" -> "int64"
        | "NUMBER" -> "int64"
        | "REAL" -> "decimal"
        | "BLOB" -> "BlobField"
        | _ -> failwith $"Unknown type: {cd.Type}"
        |> fun ts -> typeReplacements |> List.fold (fun ts tr -> tr.Attempt(cd.Name, ts)) ts
        |> fun s ->
            match cd.NotNull with
            | true -> s
            | false -> $"{s} option"

    let getTypeInit (typeReplacements: TypeReplacement list) (cd: SqliteColumnDefinition) =
        match cd.NotNull with
        | true ->
            match cd.Type with
            | "TEXT" -> "String.Empty"
            | "INTEGER" -> "0L"
            | "NUMBER" -> "0L"
            | "REAL" -> "0m"
            | "BLOB" -> "BlobField.Empty()"
            | _ -> failwith $"Unknown type: {cd.Type}"
            |> fun ts ->
                typeReplacements
                |> List.fold (fun ts tr -> tr.AttemptInitReplacement(cd.Name, ts)) ts
        | false -> "None"

    let generatorSettings (profile: Configuration.GeneratorProfile) =
        ({ Imports = [ "Freql.Core.Common"; "Freql.Sqlite" ]
           IncludeJsonAttributes = true
           TypeReplacements = profile.TypeReplacements |> List.ofSeq |> List.map TypeReplacement.Create
           TypeHandler = getType
           TypeInitHandler = getTypeInit
           NameHandler = fun cd -> cd.Name
           InsertColumnFilter = fun _ -> true
           (* TODO make this config
               fun cd ->
                   String.Equals(cd.Name, "id", StringComparison.InvariantCulture)
                   |> not
               *)
           ContextTypeName = "SqliteContext"
           BespokeTopSectionHandler = TopSection.generate
           BespokeBottomSectionHandler = BottomSection.generate }
        : GeneratorSettings<SqliteTableDefinition, SqliteColumnDefinition>)

    let generateIndexes (ctx: TableGeneratorContext) (table: SqliteTableDefinition) =
        let indexes = table.Indexes |> Seq.choose (fun i -> i.Sql)

        let indexCount = indexes |> Seq.length

        if indexCount = 0 then
            [ "static member CreateIndexesSql() = []" ]
        else
            [ "static member CreateIndexesSql() ="
              yield!
                  indexes
                  |> Seq.mapi (fun i index ->
                      [ if (i = 0) then "    [ \"\"\"" else "      "
                        $"      {index}"
                        if (i = indexCount - 1) then
                            "      \"\"\" ]"
                        else
                            "      \"\"\"" ])
                  |> Seq.collect id ]

    let generateTriggers (ctx: TableGeneratorContext) (table: SqliteTableDefinition) =
        let triggers = table.Triggers |> Seq.choose (fun i -> i.Sql)

        let triggerCount = triggers |> Seq.length

        if triggerCount = 0 then
            [ "static member CreateTriggersSql() = []" ]
        else
            [ "static member CreateTriggersSql() ="
              yield!
                  triggers
                  |> Seq.mapi (fun i trigger ->
                      [ if (i = 0) then "    [ \"\"\"" else "      "
                        $"      {trigger}"
                        if (i = triggerCount - 1) then
                            "      \"\"\" ]"
                        else
                            "      \"\"\"" ])
                  |> Seq.collect id ]

    let generateInitializeSql (ctx: TableGeneratorContext) (table: SqliteTableDefinition) =
        [ "static member InitializationSql(checkIfExists: bool) ="
          $"    [ {ctx.Name}.CreateTableSql()"
          "      |> Utils.updateCheckIfExists checkIfExists \"TABLE\""
          "      yield!"
          $"          {ctx.Name}.CreateIndexesSql()"
          "          |> List.map (Utils.updateCheckIfExists checkIfExists \"INDEX\")"
          "      yield!"
          $"          {ctx.Name}.CreateTriggersSql()"
          "          |> List.map (Utils.updateCheckIfExists checkIfExists \"TRIGGER\")  ]" ]

    let createTableDetails (profile: Configuration.GeneratorProfile) (table: SqliteTableDefinition) =

        ({ OriginalName = table.Name
           ReplacementName =
             profile.TableNameReplacements
             |> List.ofSeq
             |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal))
             |> Option.map (fun tnr -> tnr.ReplacementName)
           Sql = table.Sql
           Table = table
           Columns = table.Columns |> List.ofSeq
           BespokeMethodsHandler =
             fun ctx ->
                 [ yield! generateIndexes ctx table
                   ""
                   yield! generateTriggers ctx table
                   ""
                   yield! generateInitializeSql ctx table ]
                 |> Some }
        : TableDetails<SqliteTableDefinition, SqliteColumnDefinition>)

    /// Generate F# records from a list of MySqlTableDefinition records.
    let generate (profile: Configuration.GeneratorProfile) (database: SqliteDatabaseDefinition) =
        let settings = generatorSettings profile

        database.Tables
        |> List.ofSeq
        |> List.map (createTableDetails profile)
        |> generateCode profile settings
