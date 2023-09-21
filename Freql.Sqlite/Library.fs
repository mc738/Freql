namespace Freql.Sqlite

open System
open System.Data
open System.IO
open Freql.Core.Common
open Freql.Core.Common.Mapping
open Microsoft.Data.Sqlite
open Freql.Core.Utils

module private QueryHelpers =

    let createConnectionString
        (path: string)
        (mode: SqliteOpenMode option)
        (cache: SqliteCacheMode option)
        (password: string option)
        (pooling: bool option)
        (defaultTimeOut: int option)
        =
        let mutable connectionString = SqliteConnectionStringBuilder()

        connectionString.DataSource <- path

        match mode with
        | Some m -> connectionString.Mode <- m
        | None -> ()

        match cache with
        | Some c -> connectionString.Cache <- c
        | None -> ()

        match password with
        | Some p -> connectionString.Password <- p
        | None -> ()

        match pooling with
        | Some p -> connectionString.Pooling <- p
        | None -> ()

        match defaultTimeOut with
        | Some dto -> connectionString.DefaultTimeout <- dto
        | None -> ()

        connectionString.ToString()

    let mapParameters<'T> (mappedObj: MappedObject) (parameters: 'T) =
        mappedObj.Fields
        |> List.sortBy (fun p -> p.Index)
        |> List.map (fun f ->
            let v = mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters)

            f.MappingName, v)
        |> Map.ofList

    let mapResults<'T> (mappedObj: MappedObject) (reader: SqliteDataReader) =
        let getValue (reader: SqliteDataReader) o supportType =
            match supportType with
            | SupportedType.Boolean -> reader.GetBoolean(o) :> obj
            | SupportedType.Byte -> reader.GetByte(o) :> obj
            | SupportedType.Char -> reader.GetChar(o) :> obj
            | SupportedType.Decimal -> reader.GetDecimal(o) :> obj
            | SupportedType.Double -> reader.GetDouble(o) :> obj
            | SupportedType.Float -> reader.GetFloat(o) :> obj
            | SupportedType.Int -> reader.GetInt32(o) :> obj
            | SupportedType.Short -> reader.GetInt16(o) :> obj
            | SupportedType.Long -> reader.GetInt64(o) :> obj
            | SupportedType.String -> reader.GetString(o) :> obj
            | SupportedType.DateTime -> reader.GetDateTime(o) :> obj
            | SupportedType.Guid -> reader.GetGuid(o) :> obj
            | SupportedType.Blob -> BlobField.FromStream(reader.GetStream(o)) :> obj
            | SupportedType.Option st ->
                match reader.IsDBNull(o) with
                | true -> None :> obj
                | false ->
                    match st with
                    | SupportedType.Boolean -> Some(reader.GetBoolean(o)) :> obj
                    | SupportedType.Byte -> Some(reader.GetByte(o)) :> obj
                    | SupportedType.Char -> Some(reader.GetChar(o)) :> obj
                    | SupportedType.Decimal -> Some(reader.GetDecimal(o)) :> obj
                    | SupportedType.Double -> Some(reader.GetDouble(o)) :> obj
                    | SupportedType.Float -> Some(reader.GetFloat(o)) :> obj
                    | SupportedType.Int -> Some(reader.GetInt32(o)) :> obj
                    | SupportedType.Short -> Some(reader.GetInt16(o)) :> obj
                    | SupportedType.Long -> Some(reader.GetInt64(o)) :> obj
                    | SupportedType.String -> Some(reader.GetString(o)) :> obj
                    | SupportedType.DateTime -> Some(reader.GetDateTime(o)) :> obj
                    | SupportedType.Guid -> Some(reader.GetGuid(o)) :> obj
                    | SupportedType.Blob -> Some(BlobField.FromStream(reader.GetStream(o))) :> obj
                    | SupportedType.Option _ -> None :> obj // Nested options not allowed.

        [ while reader.Read() do
              mappedObj.Fields
              |> List.map (fun f ->
                  let o = reader.GetOrdinal(f.MappingName)
                  let value = getValue reader o f.Type
                  { Index = f.Index; Value = value })
              |> (fun v -> RecordBuilder.Create<'T> v) ]

    let noParam (connection: SqliteConnection) (sql: string) (transaction: SqliteTransaction option) =
        connection.Open()

        use comm =
            match transaction with
            | Some t -> new SqliteCommand(sql, connection, t)
            | None -> new SqliteCommand(sql, connection)

        comm

    let prepare<'P>
        (connection: SqliteConnection)
        (sql: string)
        (mappedObj: MappedObject)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        connection.Open()

        use comm =
            match transaction with
            | Some t -> new SqliteCommand(sql, connection, t)
            | None -> new SqliteCommand(sql, connection)

        parameters
        |> mapParameters<'P> mappedObj
        |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
        |> ignore

        comm.Prepare()
        comm

    let prepareAnon
        (connection: SqliteConnection)
        (sql: string)
        (parameters: obj list)
        (transaction: SqliteTransaction option)
        =
        connection.Open()

        use comm =
            match transaction with
            | Some t -> new SqliteCommand(sql, connection, t)
            | None -> new SqliteCommand(sql, connection)

        parameters
        |> List.mapi (fun i v -> comm.Parameters.AddWithValue($"@{i}", v))
        |> ignore

        comm.Prepare()
        comm

    let rawNonQuery (connection: SqliteConnection) (sql: string) (transaction: SqliteTransaction option) =
        let comm = noParam connection sql transaction

        comm.ExecuteNonQuery()

    let verbatimNonQuery<'P>
        (connection: SqliteConnection)
        (sql: string)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        let mappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql mappedObj parameters transaction

        comm.ExecuteNonQuery()

    let verbatimNonQueryAnon<'P>
        (connection: SqliteConnection)
        (sql: string)
        (parameters: obj list)
        (transaction: SqliteTransaction option)
        =
        let comm = prepareAnon connection sql parameters transaction

        comm.ExecuteNonQuery()

    /// A bespoke query, the caller needs to provide a mapping function. This returns a list of 'T.
    let bespoke<'T>
        (connection: SqliteConnection)
        (sql: string)
        (parameters: obj list)
        (mapper: SqliteDataReader -> 'T list)
        (transaction: SqliteTransaction option)
        =
        let comm = prepareAnon connection sql parameters transaction

        use reader = comm.ExecuteReader()
        mapper reader

    /// A bespoke query, the caller needs to provide a mapping function. This returns a single 'T.
    let bespokeSingle<'T>
        (connection: SqliteConnection)
        (sql: string)
        (parameters: obj list)
        (mapper: SqliteDataReader -> 'T)
        (transaction: SqliteTransaction option)
        =
        let comm = prepareAnon connection sql parameters transaction

        use reader = comm.ExecuteReader()
        mapper reader

    let create<'T> (tableName: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let mappedObj = MappedObject.Create<'T>()

        let columns =
            mappedObj.Fields
            |> List.sortBy (fun p -> p.Index)
            |> List.map (fun f ->
                let template (colType: string) = $"{f.MappingName} {colType}"

                let blobField = $"{f.MappingName} BLOB, {f.MappingName}_sha256_hash TEXT"

                match f.Type with
                | SupportedType.Boolean -> template "INTEGER NOT NULL"
                | SupportedType.Byte -> template "INTEGER NOT NULL"
                | SupportedType.Int -> template "INTEGER NOT NULL"
                | SupportedType.Short -> template "INTEGER NOT NULL"
                | SupportedType.Long -> template "INTEGER NOT NULL"
                | SupportedType.Double -> template "REAL NOT NULL"
                | SupportedType.Float -> template "REAL NOT NULL"
                | SupportedType.Decimal -> template "REAL NOT NULL"
                | SupportedType.Char -> template "TEXT NOT NULL"
                | SupportedType.String -> template "TEXT NOT NULL"
                | SupportedType.DateTime -> template "TEXT NOT NULL"
                | SupportedType.Guid -> template "TEXT NOT NULL"
                | SupportedType.Blob -> template "BLOB NOT NULL"
                | SupportedType.Option ost ->
                    match ost with
                    | SupportedType.Boolean -> template "INTEGER"
                    | SupportedType.Byte -> template "INTEGER"
                    | SupportedType.Int -> template "INTEGER"
                    | SupportedType.Short -> template "INTEGER"
                    | SupportedType.Long -> template "INTEGER"
                    | SupportedType.Double -> template "REAL"
                    | SupportedType.Float -> template "REAL"
                    | SupportedType.Decimal -> template "REAL"
                    | SupportedType.Char -> template "TEXT"
                    | SupportedType.String -> template "TEXT"
                    | SupportedType.DateTime -> template "TEXT"
                    | SupportedType.Guid -> template "TEXT"
                    | SupportedType.Blob -> template "BLOB"
                    | SupportedType.Option _ -> failwith "Nested options not supported.")
        //| SupportedType.Json -> template "BLOB")

        let columnsString = String.Join(',', columns)

        let sql =
            $"""
        CREATE TABLE {tableName} ({columnsString});
        """

        let comm = noParam connection sql transaction

        comm.ExecuteNonQuery()

    let selectAll<'T> (tableName: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let mappedObj = MappedObject.Create<'T>()

        let fields =
            mappedObj.Fields
            |> List.sortBy (fun p -> p.Index)
            |> List.map (fun f -> f.MappingName)

        let fieldsString = String.Join(',', fields)

        let sql =
            $"""
        SELECT {fieldsString}
        FROM {tableName}
        """

        let comm = noParam connection sql transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> mappedObj reader

    let select<'T, 'P>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    let selectAnon<'T>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: obj list)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = prepareAnon connection sql parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    let selectSingle<'T, 'P>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    let executeScalar<'T> (sql: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let comm = noParam connection sql transaction

        comm.ExecuteScalar() :?> 'T

    let selectSql<'T> (sql: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = noParam connection sql transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    /// Special handling is needed for `INSERT` query to accommodate blobs.
    /// This module aims to wrap as much of that up to in one place.
    [<RequireQualifiedAccess>]
    module private Insert =

        type InsertBlobCallback = { ColumnName: string; Data: Stream }

        /// Create an insert query and return the sql and a list of `InsertBlobCallback`'s.
        let createQuery<'T> (tableName: string) (mappedObj: MappedObject) (data: 'T) =
            let fieldNames, parameterNames, blobCallbacks =
                mappedObj.Fields
                |> List.fold
                    (fun (fn, pn, cb) f ->

                        match f.Type with
                        | SupportedType.Option SupportedType.Blob ->
                            let value =
                                (mappedObj.Type.GetProperty(f.FieldName).GetValue(data) :?> BlobField option)

                            match value with
                            | Some s ->
                                let callback =
                                    { ColumnName = f.MappingName
                                      Data = s.Value }

                                (fn @ [ f.MappingName ], pn @ [ $"ZEROBLOB({s.Value.Length})" ], cb @ [ callback ])
                            | None -> (fn @ [ f.MappingName ], pn @ [ "NULL" ], cb)
                        | SupportedType.Blob ->
                            // Get the blob.
                            let stream =
                                (mappedObj.Type.GetProperty(f.FieldName).GetValue(data) :?> BlobField).Value

                            let callback =
                                { ColumnName = f.MappingName
                                  Data = stream }

                            (fn @ [ f.MappingName ], pn @ [ $"ZEROBLOB({stream.Length})" ], cb @ [ callback ])
                        | _ -> (fn @ [ f.MappingName ], pn @ [ $"@{f.MappingName}" ], cb))
                    ([], [], [])

            let fields = String.Join(',', fieldNames)

            let parameters = String.Join(',', parameterNames)

            let sql =
                $"""
            INSERT INTO {tableName} ({fields})
            VALUES ({parameters});
            SELECT last_insert_rowid();
            """

            (sql, blobCallbacks)

        /// Prepare the `INSERT` query and return a `SqliteCommand` ready for execution.
        /// `BlobField` types will be skipped over, due to being handled separately.
        let prepareQuery<'P>
            (connection: SqliteConnection)
            (sql: string)
            (mappedObj: MappedObject)
            (parameters: 'P)
            (transaction: SqliteTransaction option)
            =
            connection.Open()

            use comm =
                match transaction with
                | Some t -> new SqliteCommand(sql, connection, t)
                | None -> new SqliteCommand(sql, connection)

            mappedObj.Fields
            |> List.sortBy (fun p -> p.Index)
            |> List.fold
                (fun acc f ->
                    match f.Type with
                    | SupportedType.Blob -> acc // Skip blob types, they will be handled with `BlobCallBacks`.
                    | SupportedType.Option _ ->
                        match mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters) with
                        | null -> acc @ [ f.MappingName, DBNull.Value :> obj ]
                        | SomeObj v1 -> acc @ [ f.MappingName, v1 ]
                        | _ -> acc @ [ f.MappingName, DBNull.Value :> obj ]
                    | _ ->
                        acc
                        @ [ f.MappingName, mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters) ])
                []
            |> Map.ofList
            |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
            |> ignore

            comm.Prepare()
            comm

        let handleBlobCallbacks
            (connection: SqliteConnection)
            (tableName: string)
            (callbacks: InsertBlobCallback list)
            rowId
            =
            callbacks
            |> List.map (fun cb ->
                use writeStream = new SqliteBlob(connection, tableName, cb.ColumnName, rowId)

                cb.Data.CopyTo(writeStream))
            |> ignore

    let insert<'T>
        (tableName: string)
        (connection: SqliteConnection)
        (data: 'T)
        (transaction: SqliteTransaction option)
        =
        let mappedObj = MappedObject.Create<'T>()

        let sql, callbacks = Insert.createQuery tableName mappedObj data

        // Get the last inserted id.
        let comm = Insert.prepareQuery connection sql mappedObj data transaction

        let rowId = comm.ExecuteScalar() :?> int64

        Insert.handleBlobCallbacks connection tableName callbacks rowId

/// <summary>The Sqlite context wraps up the internals of connecting to the database.</summary>
type SqliteContext(connection: SqliteConnection, transaction: SqliteTransaction option) =

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

        use conn = new SqliteConnection(QueryHelpers.createConnectionString path mode cache password pooling defaultTimeOut)

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
        connection.Close()
        connection.Dispose()

    member _.GetConnection() = connection

    member _.ClearPool() = SqliteConnection.ClearPool(connection)

    member _.ClearAllPools() = SqliteConnection.ClearAllPools()

    member _.GetConnectionState() = connection.State

    member _.GetDatabase() = connection.Database


    member _.OnStateChange(fn: StateChangeEventArgs -> unit) = connection.StateChange.Add(fn)

    /// <summary>
    /// Select all items from a table and map them to type 'T.
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>A list of type 'T</returns>
    member handler.Select<'T> tableName =
        QueryHelpers.selectAll<'T> tableName connection transaction

    /// <summary>
    /// Select data based on a verbatim sql and parameters of type 'P.
    /// Map the result to type 'T.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>A list of type 'T</returns>
    member handler.SelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.select<'T, 'P> sql connection parameters transaction

    /// <summary>
    /// Select a list of 'T based on an sql string and a list of obj for parameters.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    /// <returns>A list of type 'T</returns>
    member handler.SelectAnon<'T>(sql, parameters) =
        QueryHelpers.selectAnon<'T> sql connection parameters transaction

    /// <summary>
    /// Select a single 'T based on an sql string and a list of obj for parameters.
    /// This will return an optional value.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    /// <returns>An optional 'T</returns>
    member handler.SelectSingleAnon<'T>(sql, parameters) =
        let r = handler.SelectAnon<'T>(sql, parameters)

        match r.Length > 0 with
        | true -> r.Head |> Some
        | false -> None

    /// <summary>
    /// Select a list of 'T based on an sql string.
    /// No parameterization will take place with this, it should only be used with static sql strings.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <returns>A list of type 'T</returns>
    member handler.SelectSql<'T> sql =
        QueryHelpers.selectSql<'T> sql connection transaction

    /// <summary>
    /// Select a single 'T from a table.
    /// This is useful if a table on contains one record. It will return the first from that table.
    /// Be warned, this will throw an exception if the table is empty.
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>A 'T record.</returns>
    member handler.SelectSingle<'T> tableName = handler.Select<'T>(tableName).Head

    /// <summary>
    /// Select data based on a verbatim sql and parameters of type 'P.
    /// The first result is mapped to type 'T option.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>An optional 'T</returns>
    member handler.SelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        let result = handler.SelectVerbatim<'T, 'P>(sql, parameters)

        match result.Length with
        | 0 -> None
        | _ -> Some result.Head

    /// <summary>
    /// Execute a create table query based on a generic record type.
    /// </summary>
    /// <param name="tableName">The new tables name.</param>
    /// <returns>An int value representing the result.</returns>
    member handler.CreateTable<'T>(tableName: string) =
        QueryHelpers.create<'T> tableName connection transaction

    /// <summary>
    /// Execute a raw sql non query. What is passed as a parameters is what will be executed.
    /// WARNING: do not used with untrusted input.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <returns>An int value representing the result.</returns>
    member handler.ExecuteSqlNonQuery(sql: string) =
        QueryHelpers.rawNonQuery connection sql transaction

    /// <summary>
    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A record of type 'P representing query parameters.</param>
    /// <returns>An int value representing the result.</returns>
    member handler.ExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.verbatimNonQuery connection sql parameters transaction

    /// <summary>
    /// Execute a verbatim anonymous non query. Parameters are provided as an obj list.
    /// </summary>
    /// <param name="sql">The sql query to be run</param>
    /// <param name="parameters">A list of objects to be used are query parameters</param>
    member handler.ExecuteVerbatimNonQueryAnon(sql: string, parameters: obj list) =
        QueryHelpers.verbatimNonQueryAnon connection sql parameters transaction

    /// <summary>
    /// Execute an insert query.
    /// </summary>
    /// <param name="tableName">The name of the table to insert the record into.</param>
    /// <param name="value">The record of type 'T to be inserted.</param>
    member handler.Insert<'T>(tableName: string, value: 'T) =
        QueryHelpers.insert<'T> tableName connection value transaction

    /// <summary>
    /// Execute a collection of insert queries.
    /// </summary>
    /// <param name="tableName">The name of the table to insert the record into.</param>
    /// <param name="values">A list of records of 'T to be inserted.</param>
    member handler.InsertList<'T>(tableName: string, values: 'T list) =
        values |> List.map (fun v -> handler.Insert<'T>(tableName, v)) |> ignore

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
        with _ ->
            transaction.Rollback()
            Error "Could not complete transaction"

    /// <summary>
    /// Execute a collection of commands in a transaction.
    /// While a transaction is active on a connection non transaction commands can not be executed.
    /// This is no check for this for this is not thread safe.
    /// Also be warned, this use general error handling so an exception will roll the transaction back.
    /// This accepts a function that returns a result. If the result is Error, the transaction will be rolled back.
    /// This means you no longer have to throw an exception to rollback the transaction.
    /// </summary>
    /// <param name="transactionFn">The transaction function to be attempted.</param>
    member handler.ExecuteInTransactionV2<'R>(transactionFn: SqliteContext -> Result<'R, string>) =
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
                Error e
        with exn ->
            transaction.Rollback()
            Error $"Could not complete transaction. Exception: {exn.Message}"


    /// Execute sql that produces a scalar result.
    member handler.ExecuteScalar<'T>(sql) =
        QueryHelpers.executeScalar<'T> sql connection transaction

    /// <summary>
    /// Execute a bespoke query, it is upto to the caller to provide the sql, the parameters and the result mapping function.
    /// </summary>
    /// <param name="sql">The sql to be executed.</param>
    /// <param name="parameters">A list of boxed parameters to be used in the query.</param>
    /// <param name="mapper">A function to handle the result.</param>
    /// <returns>A list of 'T.</returns>
    member handler.Bespoke<'T>(sql, parameters, mapper: SqliteDataReader -> 'T list) =
        QueryHelpers.bespoke connection sql parameters mapper transaction

    /// <summary>
    /// Test the database connection.
    /// Useful for health checks.
    /// </summary>
    member handler.TestConnection() =
        QueryHelpers.executeScalar<int64> "SELECT 1" connection transaction

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

    member ctx.RegisterRegexFunction() =
        ctx.CreateFunction(
            "regexp",
            fun (pattern: string, input: string) -> System.Text.RegularExpressions.Regex.IsMatch(input, pattern)
        )
