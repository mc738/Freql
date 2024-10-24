namespace Freql.Sqlite

#nowarn "6140001"

open System
open System.Data
open System.Diagnostics
open System.IO
open Microsoft.Data.Sqlite
open Freql.Core

/// <summary>The Sqlite context wraps up the internals of connecting to the database.</summary>
type SqliteContext(connection: SqliteConnection, transaction: SqliteTransaction option) =

    static let activitySource = new ActivitySource("Freql.Sqlite.SqliteContextTelemetry", "1.0.0")

    interface IDisposable with

        member ctx.Dispose() = ctx.Close()


    static member Create
        (
            path: string,
            ?mode: SqliteOpenMode,
            ?cache: SqliteCacheMode,
            ?password: string,
            ?pooling: bool,
            ?defaultTimeOut: int
        ) =
        File.WriteAllBytes(path, [||])

        use conn =
            new SqliteConnection(QueryHelpers.createConnectionString path mode cache password pooling defaultTimeOut)

        new SqliteContext(conn, None)

    static member Open
        (
            path: string,
            ?mode: SqliteOpenMode,
            ?cache: SqliteCacheMode,
            ?password: string,
            ?pooling: bool,
            ?defaultTimeOut: int
        ) =
        use conn =
            new SqliteConnection(QueryHelpers.createConnectionString path mode cache password pooling defaultTimeOut)

        new SqliteContext(conn, None)

    static member Connect(connectionString: string) =

        use conn = new SqliteConnection(connectionString)

        new SqliteContext(conn, None)

    member _.Close() =
        use activity = activitySource.StartActivity("CloseDatabase", ActivityKind.Client)
        
        connection.Close()
        connection.Dispose()

    member _.Test() = connection

    member _.GetConnection() = connection

    member _.ClearPool() = SqliteConnection.ClearPool(connection)

    member _.ClearAllPools() = SqliteConnection.ClearAllPools()

    member _.GetConnectionState() = connection.State

    member _.GetDatabase() = connection.Database

    member _.OnStateChange(fn: StateChangeEventArgs -> unit) = connection.StateChange.Add(fn)

    /// <summary>
    /// Select all items from a table and map them to type 'T.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A list of type 'T</returns>
    member ctx.Select<'T> tableName =
        QueryHelpers.selectAll<'T> tableName connection transaction


    /// <summary>
    /// Try and select all items from a table and map them to type 'T.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A result consisting of a list of type 'T or a SQLiteFailure.</returns>
    member ctx.TrySelect<'T> tableName =
        QueryHelpers.attempt (fun _ -> ctx.Select<'T> tableName)


    /// <summary>
    /// Select all items from a table and map to type 'T.
    /// This uses a deferred query so results are only created when the seq is enumerated.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A seq of type 'T</returns>
    member ctx.DeferredSelect<'T> tableName =
        QueryHelpers.deferredSelectAll<'T> tableName connection transaction

    member ctx.TryDeferredSelect<'T> tableName =
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelect<'T> tableName)

    /// <summary>
    /// Select data based on a verbatim sql and parameters of type 'P.
    /// Map the result to type 'T.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>A list of type 'T</returns>
    member ctx.SelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.select<'T, 'P> sql connection parameters transaction

    member ctx.TrySelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.attempt (fun _ -> ctx.SelectVerbatim<'T, 'P>(sql, parameters))

    /// <summary>
    /// Select data based on a verbatim sql and parameters of type 'P.
    /// Map the result to type 'T.
    /// This uses a deferred query so results are only created when the seq is enumerated.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>A seq of type 'T</returns>
    member ctx.DeferredSelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.deferredSelect<'T, 'P> sql connection parameters transaction

    member ctx.TryDeferredSelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelectVerbatim<'T, 'P>(sql, parameters))

    /// <summary>
    /// Select a list of 'T based on an sql string and a list of obj for parameters.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    /// <returns>A list of type 'T</returns>
    member ctx.SelectAnon<'T>(sql, parameters) =
        QueryHelpers.selectAnon<'T> sql connection parameters transaction

    member ctx.TrySelectAnon<'T>(sql, parameters) =
        QueryHelpers.attempt (fun _ -> ctx.SelectAnon<'T>(sql, parameters))

    /// <summary>
    /// Select a list of 'T based on an sql string and a list of obj for parameters.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    /// This uses a deferred query so results are only created when the seq is enumerated.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    /// <returns>A seq of type 'T</returns>
    member ctx.DeferredSelectAnon<'T>(sql, parameters) =
        QueryHelpers.deferredSelectAnon<'T> sql connection parameters transaction

    member ctx.TryDeferredSelectAnon<'T>(sql, parameters) =
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelectAnon<'T>(sql, parameters))

    /// <summary>
    /// Select a single 'T based on a sql string and a list of obj for parameters.
    /// This will return an optional value.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    /// It is best to limit the results or the query (with something like LIMIT 1),
    /// to ensure optimum memory use (i.e. not creating results just to discard them straight away).
    /// Alternatively call the DeferredSelectSingleAnon method which handles this issue via a deferred query.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    /// <returns>An optional 'T</returns>
    member ctx.SelectSingleAnon<'T>(sql, parameters) =
        // Optimization - this could be optimized to only map the first item.

        // FUTURE: Switch to this:
        // QueryHelpers.selectAnonSingle<'T> sql connection parameters transaction

        ctx.SelectAnon<'T>(sql, parameters) |> List.tryHead

    member ctx.TrySelectSingleAnon<'T>(sql, parameters) =
        QueryHelpers.attempt (fun _ -> ctx.SelectSingleAnon<'T>(sql, parameters))

    /// <summary>
    /// Select a single 'T based on an sql string and a list of obj for parameters.
    /// This will return an optional value.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    /// Because this uses a deferred query it shouldn't matter if the query could potential return more than one result.
    /// Only the first result will be handled.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    /// <returns>An optional 'T</returns>
    member ctx.DeferredSelectSingleAnon<'T>(sql, parameters) =
        ctx.DeferredSelectAnon<'T>(sql, parameters) |> Seq.tryHead

    member ctx.TryDeferredSelectSingleAnon<'T>(sql, parameters) =
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelectAnon<'T>(sql, parameters) |> Seq.tryHead)

    /// <summary>
    /// Select a list of 'T based on a sql string.
    /// No parameterization will take place with this, it should only be used with static sql strings.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <returns>A list of type 'T</returns>
    // FUTURE: Add compiler message warning of this? However all sql statements could be dangerous in all methods
    //[<CompilerMessage("", "")>]
    member ctx.SelectSql<'T> sql =
        QueryHelpers.selectSql<'T> sql connection transaction

    member ctx.TrySelectSql<'T> sql =
        QueryHelpers.attempt (fun _ -> ctx.SelectSql<'T> sql)

    /// <summary>
    /// Select a list of 'T based on an sql string.
    /// No parameterization will take place with this, it should only be used with static sql strings.
    /// This uses a deferred query so results are only created when the seq is enumerated.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <returns>A list of type 'T</returns>
    member handler.DeferredSelectSql<'T> sql =
        QueryHelpers.deferredSelectSql<'T> sql connection transaction

    member ctx.TryDeferredSelectSql<'T> sql =
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelectSql<'T> sql)

    /// <summary>
    /// Select a single 'T from a table.
    /// This is useful if a table on contains one record. It will return the first from that table.
    /// Be warned, this will throw an exception if the table is empty.
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>A 'T record.</returns>
    member ctx.SelectSingle<'T> tableName =
        // FUTURE: Rework this to not map all values. Also possibly worth changing to a option.
        ctx.Select<'T>(tableName).Head

    member ctx.TrySelectSingle<'T> sql =
        QueryHelpers.attempt (fun _ -> ctx.SelectSingle<'T> sql)

    /// <summary>
    /// Select a single 'T from a table.
    /// This is useful if a table on contains one record. It will return the first from that table.
    /// Be warned, this will throw an exception if the table is empty.
    /// This uses a deferred query so results are only created when the seq is enumerated.
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>A 'T record.</returns>
    member ctx.DeferredSelectSingle<'T> tableName =
        // FUTURE: Rework this to not map all values. Also possibly worth changing to a option.
        ctx.DeferredSelect<'T>(tableName) |> Seq.head

    member ctx.TryDeferredSelectSingle<'T> tableName =
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelectSingle<'T>(tableName))

    /// <summary>
    /// Select data based on a verbatim sql and parameters of type 'P.
    /// The first result is mapped to type 'T option.
    /// It is best to limit the results or the query (with something like LIMIT 1),
    /// to ensure optimum memory use (i.e. not creating results just to discard them straight away).
    /// Alternatively call the DeferredSelectSingleAnon method which handles this issue via a deferred query.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>An optional 'T</returns>
    member ctx.SelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        // NOTE - this could be rewritten to use List.tryHead
        // FUTURE: Rework to use:
        // QueryHelpers.selectSingle<'T, 'P> sql connection parameters transaction

        let result = ctx.SelectVerbatim<'T, 'P>(sql, parameters)

        match result.Length with
        | 0 -> None
        | _ -> Some result.Head

    member ctx.TrySelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        QueryHelpers.attempt (fun _ -> ctx.SelectSingleVerbatim<'T, 'P>(sql, parameters))

    /// <summary>
    /// Select data based on a verbatim sql and parameters of type 'P.
    /// The first result is mapped to type 'T option.
    /// Because this uses a deferred query it shouldn't matter if the query could potential return more than one result.
    /// Only the first result will be handled.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>An optional 'T</returns>
    member ctx.DeferredSelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        // FUTURE: Switch out implementation to use
        ctx.DeferredSelectVerbatim<'T, 'P>(sql, parameters) |> Seq.tryHead

    member ctx.TryDeferredSelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        // FUTURE: Switch out implementation to use
        QueryHelpers.attempt (fun _ -> ctx.DeferredSelectSingleVerbatim<'T, 'P>(sql, parameters))

    /// <summary>
    /// Execute a create table query based on a generic record type.
    /// </summary>
    /// <param name="tableName">The new tables name.</param>
    /// <returns>An int value representing the result.</returns>
    member ctx.CreateTable<'T>(tableName: string) =
        QueryHelpers.create<'T> tableName connection transaction

    member ctx.TryCreateTable<'T>(tableName: string) =
        QueryHelpers.attempt (fun _ -> ctx.CreateTable<'T> tableName)

    /// <summary>
    /// Execute a raw sql non query. What is passed as a parameters is what will be executed.
    /// WARNING: do not used with untrusted input.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <returns>An int value representing the result.</returns>
    member ctx.ExecuteSqlNonQuery(sql: string) =
        QueryHelpers.rawNonQuery connection sql transaction

    member ctx.TryExecuteSqlNonQuery(sql: string) =
        QueryHelpers.attempt (fun _ -> ctx.ExecuteSqlNonQuery(sql))

    /// <summary>
    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>An int value representing the result.</returns>
    member ctx.ExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.verbatimNonQuery connection sql parameters transaction
        
    member ctx.TryExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.attempt (fun _ -> ctx.ExecuteVerbatimNonQuery<'P>(sql, parameters))

    /// <summary>
    /// Execute a verbatim anonymous non query. Parameters are provided as an obj list.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    member ctx.ExecuteVerbatimNonQueryAnon(sql: string, parameters: obj list) =
        QueryHelpers.verbatimNonQueryAnon connection sql parameters transaction
        
    member ctx.TryExecuteVerbatimNonQueryAnon(sql: string, parameters: obj list) =
        QueryHelpers.attempt (fun _ -> ctx.ExecuteVerbatimNonQueryAnon(sql, parameters))

    /// <summary>
    /// Execute an insert query.
    /// </summary>
    /// <param name="tableName">The name of the table to insert the record into.</param>
    /// <param name="value">The record of type 'T to be inserted.</param>
    member ctx.Insert<'T>(tableName: string, value: 'T) =
        //use activity = activitySource.StartActivity("Insert record", ActivityKind.Client)

        QueryHelpers.insert<'T> tableName connection value transaction
        
    member ctx.TryInsert<'T>(tableName: string, value: 'T) =
         QueryHelpers.attempt (fun _ ->  ctx.Insert<'T>(tableName, value))

    /// <summary>
    /// Execute a collection of insert queries.
    /// </summary>
    /// <param name="tableName">The name of the table to insert the record into.</param>
    /// <param name="values">A list of records of 'T to be inserted.</param>
    member ctx.InsertList<'T>(tableName: string, values: 'T list) =
        // TODO change this to List.iter
        values |> List.map (fun v -> ctx.Insert<'T>(tableName, v)) |> ignore
        
    member ctx.TryInsertList<'T>(tableName: string, values: 'T list) =
        QueryHelpers.attempt (fun _ -> ctx.InsertList<'T>(tableName, values))

    /// <summary>
    /// Execute a collection of commands in a transaction.
    /// While a transaction is active on a connection non transaction commands can not be executed.
    /// This is no check for this for this is not thread safe.
    /// Also be warned, this use general error handling so an exception will roll the transaction back.
    /// </summary>
    /// <param name="transactionFn">The transaction function to be attempted.</param>
    member handler.ExecuteInTransaction<'R>(transactionFn: SqliteContext -> 'R) =
        connection.Open()

        use transaction = connection.BeginTransaction()

        use qh = new SqliteContext(connection, Some transaction)

        try
            let r = transactionFn qh
            transaction.Commit()
            Ok r
        with exn ->
            transaction.Rollback()

            Error
                { Message = $"Could not complete transaction. Exception: {exn.Message}"
                  Exception = Some exn }

    /// <summary>
    /// Try and execute a collection of commands in a transaction.
    /// While a transaction is active on a connection non transaction commands can not be executed.
    /// This is no check for this for this is not thread safe.
    /// Also be warned, this use general error handling so an exception will roll the transaction back.
    /// This accepts a function that returns a result (and thus is excepted to be able to fail).
    /// If the result is Error, the transaction will be rolled back.
    /// This means you no longer have to throw an exception to rollback the transaction.
    /// </summary>
    /// <param name="transactionFn">The transaction function to be attempted.</param>
    member handler.TryExecuteInTransaction<'R>(transactionFn: SqliteContext -> Result<'R, string>) =
        connection.Open()

        use transaction = connection.BeginTransaction()

        use qh = new SqliteContext(connection, Some transaction)

        try
            match transactionFn qh with
            | Ok r ->
                transaction.Commit()
                Ok r
            | Error e ->
                transaction.Rollback()
                Error { Message = e; Exception = None }
        with exn ->
            transaction.Rollback()

            Error
                { Message = $"Could not complete transaction. Exception: {exn.Message}"
                  Exception = Some exn }

    /// Execute sql that produces a scalar result.
    member ctx.ExecuteScalar<'T>(sql) =
        QueryHelpers.executeScalar<'T> sql connection transaction
        
    member ctx.TryExecuteScalar<'T>(sql) =
        QueryHelpers.attempt (fun _ -> ctx.ExecuteScalar<'T>(sql))

    /// <summary>
    /// Execute a bespoke query, it is upto to the caller to provide the sql, the parameters and the result mapping function.
    /// </summary>
    /// <param name="sql">The sql to be executed.</param>
    /// <param name="parameters">A list of boxed parameters to be used in the query.</param>
    /// <param name="mapper">A function to handle the result.</param>
    /// <returns>A list of 'T.</returns>
    member ctx.Bespoke<'T>(sql, parameters, mapper: SqliteDataReader -> 'T list) =
        QueryHelpers.bespoke connection sql parameters mapper transaction

    member ctx.TryBespoke<'T>(sql, parameters, mapper: SqliteDataReader -> 'T list) =
        QueryHelpers.attempt(fun _ -> ctx.Bespoke<'T>(sql, parameters, mapper))

    /// <summary>
    /// Test the database connection.
    /// Useful for health checks.
    /// </summary>
    member ctx.TestConnection() =
        QueryHelpers.executeScalar<int64> "SELECT 1" connection transaction
        
    member ctx.TryTestConnection() =
        QueryHelpers.attempt (fun _ -> ctx.TestConnection())
        
    member handler.Rollback(message: string) =
        match transaction with
        | Some t ->
            t.Rollback()
            Error message
        | None -> Error "No active transaction."

    member _.CreateFunction<'T1, 'TResult>(name: string, fn: 'T1 -> 'TResult, ?isDeterministic: bool) =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'TResult>(name: string, fn: 'T1 -> 'T2 -> 'TResult, ?isDeterministic: bool) =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'TResult>
        (name: string, fn: 'T1 -> 'T2 -> 'T3 -> 'TResult, ?isDeterministic: bool)
        =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'TResult>
        (name: string, fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'TResult, ?isDeterministic: bool)
        =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'TResult>
        (name: string, fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'TResult, ?isDeterministic: bool)
        =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'TResult>
        (name: string, fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'TResult, ?isDeterministic: bool)
        =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'TResult>
        (name: string, fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'T7 -> 'TResult, ?isDeterministic: bool)
        =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'TResult>
        (name: string, fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'T7 -> 'T8 -> 'TResult, ?isDeterministic: bool)
        =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9, 'TResult>
        (
            name: string,
            fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'T7 -> 'T8 -> 'T9 -> 'TResult,
            ?isDeterministic: bool
        ) =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9, 'T10, 'TResult>
        (
            name: string,
            fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'T7 -> 'T8 -> 'T9 -> 'T10 -> 'TResult,
            ?isDeterministic: bool
        ) =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9, 'T10, 'T11, 'TResult>
        (
            name: string,
            fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'T7 -> 'T8 -> 'T9 -> 'T10 -> 'T11 -> 'TResult,
            ?isDeterministic: bool
        ) =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member _.CreateFunction<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8, 'T9, 'T10, 'T11, 'T12, 'TResult>
        (
            name: string,
            fn: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'T6 -> 'T7 -> 'T8 -> 'T9 -> 'T10 -> 'T11 -> 'T12 -> 'TResult,
            ?isDeterministic: bool
        ) =
        match isDeterministic with
        | Some v -> connection.CreateFunction(name, fn, v)
        | None -> connection.CreateFunction(name, fn)

    member ctx.RegisterRegexFunction() =
        ctx.CreateFunction(
            "regexp",
            fun (pattern: string, input: string) -> System.Text.RegularExpressions.Regex.IsMatch(input, pattern)
        )

    member ctx.CreateAggregate<'T>(name: string, fn: 'T -> 'T, ?isDeterministic: bool) =
        match isDeterministic with
        | Some v -> connection.CreateAggregate(name, fn, v)
        | None -> connection.CreateAggregate(name, fn)
