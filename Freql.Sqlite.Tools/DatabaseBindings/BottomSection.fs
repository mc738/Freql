namespace Freql.Sqlite.Tools.DatabaseBindings

[<RequireQualifiedAccess>]
module BottomSection =

    open System.Collections.Generic
    open Freql.Core.Utils
    open Freql.Sqlite.Tools.Core.SqliteMetadata
    open Freql.Tools.DatabaseBindings

    // This was initially generated via AI, so might need some testing.
    let topologicalSort (records: TableDetails<SqliteTableDefinition, SqliteColumnDefinition> list) =
        let graph = Dictionary<string, string list>()
        let inDegree = Dictionary<string, int>()

        // Initialize the graph and in-degree count
        for record in records do
            graph.[record.OriginalName] <- record.Table.ForeignKeys |> Seq.map (fun fk -> fk.Table) |> List.ofSeq
            inDegree.[record.OriginalName] <- 0

        for record in records do
            for fk in record.Table.ForeignKeys do
                if inDegree.ContainsKey(fk.Table) then
                    inDegree.[fk.Table] <- inDegree.[fk.Table] + 1
                else
                    inDegree.[fk.Table] <- 1

        let sortedList = List<string>()

        // Queue for records with no incoming edges
        let queue = Queue<string>()

        for kvp in inDegree do
            if kvp.Value = 0 then
                queue.Enqueue(kvp.Key)

        while queue.Count > 0 do
            let node = queue.Dequeue()
            sortedList.Add(node)

            for neighbor in graph.[node] do
                inDegree.[neighbor] <- inDegree.[neighbor] - 1

                if inDegree.[neighbor] = 0 then
                    queue.Enqueue(neighbor)

        // Return the sorted records
        sortedList
        |> Seq.map (fun id -> records |> List.find (fun r -> r.OriginalName = id))
        |> Seq.toList
        |> List.rev

    let createInitializationSql (orderedTableNames: string list) =
        match orderedTableNames.Length with
        | 0 -> [ "    let sql (checkIfExists: bool) = []" ]
        | 1 ->
            [ $"    let sql (checkIfExists: bool) = [ Records.{orderedTableNames.Head}.InitializationSql checkIfExists ]" ]
        | _ ->
            [ "    let sql (checkIfExists: bool) ="
              yield!
                  orderedTableNames
                  |> List.mapi (fun i name ->
                      let startBlock =
                          match i with
                          | 0 -> "        [ "
                          | _ -> "          "

                      let endBlock =
                          match i with
                          | _ when orderedTableNames.Length - 1 = i -> " ]"
                          | _ -> ""

                      $"{startBlock}Records.{name}.InitializationSql checkIfExists{endBlock}")
              "        |> List.concat" ]

    let generate (ctx: GeneratorContext<SqliteTableDefinition, SqliteColumnDefinition>) =
        let orderedTableNames =
            ctx.Tables
            |> topologicalSort
            |> List.map (fun t ->
                t.ReplacementName
                |> Option.defaultValue t.OriginalName
                |> fun tn -> tn.ToPascalCase())

        [ "[<RequireQualifiedAccess>]"
          "module Initialization ="
          yield! createInitializationSql orderedTableNames
          ""
          "    let run (checkIfExists: bool) (ctx: SqliteContext) ="
          "        sql checkIfExists |> List.iter (ctx.ExecuteSqlNonQuery >> ignore)"
          ""
          "    let runInTransaction (checkIfExists: bool) (ctx: SqliteContext) ="
          "        ctx.ExecuteInTransaction(fun t -> sql checkIfExists |> List.iter (t.ExecuteSqlNonQuery >> ignore))" ]
        |> Some
