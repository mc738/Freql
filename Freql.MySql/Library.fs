﻿namespace Freql.MySql

open System
open System.Data
open System.IO
open Freql.Core
open Freql.Core.Mapping
open Freql.Core.Utils
open MySql.Data.MySqlClient

module private QueryHelpers =

    let mapParameters<'T> (mappedObj: MappedObject) (parameters: 'T) =
        mappedObj.Fields
        |> List.sortBy (fun p -> p.Index)
        |> List.map (fun f ->
            let v = mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters)

            f.MappingName, v)
        |> Map.ofList

    let mapResults<'T> (mappedObj: MappedObject) (reader: MySqlDataReader) =
        let getValue (reader: MySqlDataReader) (o: int) supportType =
            match supportType with
            | SupportedType.Boolean -> reader.GetBoolean(o) :> obj
            | SupportedType.Byte -> reader.GetByte(o) :> obj
            | SupportedType.SByte -> reader.GetSByte(o) :> obj
            | SupportedType.Char -> reader.GetChar(o) :> obj
            | SupportedType.Decimal -> reader.GetDecimal(o) :> obj
            | SupportedType.Double -> reader.GetDouble(o) :> obj
            | SupportedType.Single -> reader.GetFloat(o) :> obj
            | SupportedType.Int -> reader.GetInt32(o) :> obj
            | SupportedType.UInt -> reader.GetUInt32(o) :> obj
            | SupportedType.Short -> reader.GetInt16(o) :> obj
            | SupportedType.UShort -> reader.GetUInt16(o) :> obj
            | SupportedType.Long -> reader.GetInt64(o) :> obj
            | SupportedType.ULong -> reader.GetUInt64(o) :> obj
            | SupportedType.String -> reader.GetString(o) :> obj
            | SupportedType.DateTime -> reader.GetDateTime(o) :> obj
            | SupportedType.TimeSpan -> reader.GetTimeSpan(o) :> obj
            | SupportedType.Guid -> reader.GetGuid(o) :> obj
            | SupportedType.Blob -> BlobField.FromStream(reader.GetStream(o)) :> obj
            | SupportedType.Option st ->
                match reader.IsDBNull(o) with
                | true -> None :> obj
                | false ->
                    match st with
                    | SupportedType.Boolean -> Some(reader.GetBoolean(o)) :> obj
                    | SupportedType.Byte -> Some(reader.GetByte(o)) :> obj
                    | SupportedType.SByte -> Some(reader.GetSByte(o)) :> obj
                    | SupportedType.Char -> Some(reader.GetChar(o)) :> obj
                    | SupportedType.Decimal -> Some(reader.GetDecimal(o)) :> obj
                    | SupportedType.Double -> Some(reader.GetDouble(o)) :> obj
                    | SupportedType.Single -> Some(reader.GetFloat(o)) :> obj
                    | SupportedType.Int -> Some(reader.GetInt32(o)) :> obj
                    | SupportedType.UInt -> Some(reader.GetUInt32(o)) :> obj
                    | SupportedType.Short -> Some(reader.GetInt16(o)) :> obj
                    | SupportedType.UShort -> Some(reader.GetUInt16(o)) :> obj
                    | SupportedType.Long -> Some(reader.GetInt64(o)) :> obj
                    | SupportedType.ULong -> Some(reader.GetUInt64(o)) :> obj
                    | SupportedType.String -> Some(reader.GetString(o)) :> obj
                    | SupportedType.DateTime -> Some(reader.GetDateTime(o)) :> obj
                    | SupportedType.TimeSpan -> Some(reader.GetTimeSpan(o)) :> obj
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

    let deferredMapResults<'T> (mappedObj: MappedObject) (comm: MySqlCommand) =
        // TODO remove code duplication
        let getValue (reader: MySqlDataReader) (o: int) supportType =
            match supportType with
            | SupportedType.Boolean -> reader.GetBoolean(o) :> obj
            | SupportedType.Byte -> reader.GetByte(o) :> obj
            | SupportedType.SByte -> reader.GetSByte(o) :> obj
            | SupportedType.Char -> reader.GetChar(o) :> obj
            | SupportedType.Decimal -> reader.GetDecimal(o) :> obj
            | SupportedType.Double -> reader.GetDouble(o) :> obj
            | SupportedType.Single -> reader.GetFloat(o) :> obj
            | SupportedType.Int -> reader.GetInt32(o) :> obj
            | SupportedType.UInt -> reader.GetUInt32(o) :> obj
            | SupportedType.Short -> reader.GetInt16(o) :> obj
            | SupportedType.UShort -> reader.GetUInt16(o) :> obj
            | SupportedType.Long -> reader.GetInt64(o) :> obj
            | SupportedType.ULong -> reader.GetUInt64(o) :> obj
            | SupportedType.String -> reader.GetString(o) :> obj
            | SupportedType.DateTime -> reader.GetDateTime(o) :> obj
            | SupportedType.TimeSpan -> reader.GetTimeSpan(o) :> obj
            | SupportedType.Guid -> reader.GetGuid(o) :> obj
            | SupportedType.Blob -> BlobField.FromStream(reader.GetStream(o)) :> obj
            | SupportedType.Option st ->
                match reader.IsDBNull(o) with
                | true -> None :> obj
                | false ->
                    match st with
                    | SupportedType.Boolean -> Some(reader.GetBoolean(o)) :> obj
                    | SupportedType.Byte -> Some(reader.GetByte(o)) :> obj
                    | SupportedType.SByte -> reader.GetSByte(o) :> obj
                    | SupportedType.Char -> Some(reader.GetChar(o)) :> obj
                    | SupportedType.Decimal -> Some(reader.GetDecimal(o)) :> obj
                    | SupportedType.Double -> Some(reader.GetDouble(o)) :> obj
                    | SupportedType.Single -> Some(reader.GetFloat(o)) :> obj
                    | SupportedType.Int -> Some(reader.GetInt32(o)) :> obj
                    | SupportedType.UInt -> Some(reader.GetInt32(o)) :> obj
                    | SupportedType.Short -> Some(reader.GetInt16(o)) :> obj
                    | SupportedType.UShort -> Some(reader.GetUInt16(o)) :> obj
                    | SupportedType.Long -> Some(reader.GetInt64(o)) :> obj
                    | SupportedType.ULong -> Some(reader.GetUInt64(o)) :> obj
                    | SupportedType.String -> Some(reader.GetString(o)) :> obj
                    | SupportedType.DateTime -> Some(reader.GetDateTime(o)) :> obj
                    | SupportedType.TimeSpan -> Some(reader.GetTimeSpan(o)) :> obj
                    | SupportedType.Guid -> Some(reader.GetGuid(o)) :> obj
                    | SupportedType.Blob -> Some(BlobField.FromStream(reader.GetStream(o))) :> obj
                    | SupportedType.Option _ -> None :> obj // Nested options not allowed.

        seq {
            use reader = comm.ExecuteReader()

            while reader.Read() do
                mappedObj.Fields
                |> List.map (fun f ->
                    let o = reader.GetOrdinal(f.MappingName)
                    let value = getValue reader o f.Type
                    { Index = f.Index; Value = value })
                |> (fun v -> RecordBuilder.Create<'T> v)
        }

    let noParam (connection: MySqlConnection) (sql: string) (transaction: MySqlTransaction option) =

        if connection.State = ConnectionState.Closed then
            connection.Open()

        use comm =
            match transaction with
            | Some t -> new MySqlCommand(sql, connection, t)
            | None -> new MySqlCommand(sql, connection)

        // TODO add ability to set timeout?
        // comm.CommandTimeout <- 5000

        comm

    let prepare<'P>
        (connection: MySqlConnection)
        (sql: string)
        (mappedObj: MappedObject)
        (parameters: 'P)
        (transaction: MySqlTransaction option)
        =

        if connection.State = ConnectionState.Closed then
            connection.Open()


        use comm =
            match transaction with
            | Some t -> new MySqlCommand(sql, connection, t)
            | None -> new MySqlCommand(sql, connection)

        parameters
        |> mapParameters<'P> mappedObj
        |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
        |> ignore

        // TODO add ability to set timeout?
        // comm.CommandTimeout <- 5000

        comm.Prepare()
        comm

    let prepareAnon
        (connection: MySqlConnection)
        (sql: string)
        (parameters: obj list)
        (transaction: MySqlTransaction option)
        =

        if connection.State = ConnectionState.Closed then
            connection.Open()

        use comm =
            match transaction with
            | Some t -> new MySqlCommand(sql, connection, t)
            | None -> new MySqlCommand(sql, connection)

        parameters
        |> List.mapi (fun i v -> comm.Parameters.AddWithValue($"@{i}", v))
        |> ignore

        // TODO add ability to set timeout?
        // comm.CommandTimeout <- 5000

        comm.Prepare()
        comm

    let rawNonQuery connection sql transaction =
        let comm = noParam connection sql transaction

        comm.ExecuteNonQuery()

    let verbatimNonQuery<'P> connection sql (parameters: 'P) transaction =
        let mappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql mappedObj parameters transaction

        comm.ExecuteNonQuery()

    let verbatimNonQueryAnon<'P> connection (sql: string) (parameters: obj list) transaction =
        let comm = prepareAnon connection sql parameters transaction
        comm.ExecuteNonQuery()

    /// A bespoke query, the caller needs to provide a mapping function. This returns a list of 'T.
    let bespoke<'T> connection (sql: string) (parameters: obj list) (mapper: MySqlDataReader -> 'T list) transaction =
        let comm = prepareAnon connection sql parameters transaction
        use reader = comm.ExecuteReader()
        mapper reader

    /// A bespoke query, the caller needs to provide a mapping function. This returns a single 'T.
    let bespokeSingle<'T> connection (sql: string) (parameters: obj list) (mapper: MySqlDataReader -> 'T) transaction =
        let comm = prepareAnon connection sql parameters transaction
        use reader = comm.ExecuteReader()
        mapper reader

    let selectAll<'T> (tableName: string) connection transaction =
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

    let deferredSelectAll<'T> (tableName: string) connection transaction =
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

        deferredMapResults<'T> mappedObj comm

    let select<'T, 'P> (sql: string) connection (parameters: 'P) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        use reader = comm.ExecuteReader()

        let r = mapResults<'T> tMappedObj reader
        connection.Close()
        r

    let deferredSelect<'T, 'P> (sql: string) connection (parameters: 'P) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        // TODO test this actually works - the data reader is slightly different from MySql than Sqlite.
        deferredMapResults<'T> tMappedObj comm

    let selectAnon<'T> (sql: string) connection (parameters: obj list) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let comm = prepareAnon connection sql parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader


    let deferredSelectAnon<'T> (sql: string) connection (parameters: obj list) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let comm = prepareAnon connection sql parameters transaction

        deferredMapResults<'T> tMappedObj comm

    let selectSingle<'T, 'P> (sql: string) connection (parameters: 'P) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        let r = comm.ExecuteScalar()

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    let deferredSelectSingle<'T, 'P> (sql: string) connection (parameters: 'P) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        let r = comm.ExecuteScalar()

        deferredMapResults<'T> tMappedObj comm

    let executeScalar<'T> (sql: string) connection transaction =
        let comm = noParam connection sql transaction
        comm.ExecuteScalar() :?> 'T

    let selectSql<'T> (sql: string) connection transaction =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = noParam connection sql transaction

        use reader = comm.ExecuteReader()

        let r = mapResults<'T> tMappedObj reader
        connection.Close()
        r


    /// Special handling is needed for `INSERT` query to accommodate blobs.
    /// This module aims to wrap as much of that up to in one place.
    [<RequireQualifiedAccess>]
    module private Insert =

        type InsertBlobCallback = { ColumnName: string; Data: Stream }

        /// Create an insert query and return the sql and a list of `InsertBlobCallback`'s.
        let createQuery<'T> (tableName: string) (mappedObj: MappedObject) (data: 'T) =
            let fieldNames, parameterNames =
                mappedObj.Fields
                |> List.fold
                    (fun (fn, pn) f ->

                        match f.Type with
                        | SupportedType.Option SupportedType.Blob ->

                            (*
                            NOTE - example of handling optional blob fields (from Freql.Sqlite)
                            let value =
                                (mappedObj.Type.GetProperty(f.FieldName).GetValue(data) :?> BlobField option)

                            match value with
                            | Some s ->
                                let callback =
                                    { ColumnName = f.MappingName
                                      Data = s.Value }

                                (fn @ [ f.MappingName ], pn @ [ $"ZEROBLOB({s.Value.Length})" ], cb @ [ callback ])
                            | None -> (fn @ [ f.MappingName ], pn @ [ "NULL" ], cb)
                            *)
                            // TODO implement optional blobs for MySql
                            failwith "Blobs not supported in MySql."
                        | SupportedType.Blob ->
                            (* Sqlite code for handling blobs - how to handle with MySql?
                               https://stackoverflow.com/questions/13208349/how-to-insert-blob-datatype??
                            let stream =
                                (mappedObj.Type.GetProperty(f.FieldName).GetValue(data) :?> BlobField).Value

                            let callback =
                                { ColumnName = f.MappingName
                                  Data = stream }

                            (fn @ [ f.MappingName ], pn @ [ $"ZEROBLOB({stream.Length})" ], cb @ [ callback ])
                            *)
                            // TODO implement blobs for MySql
                            failwith "Blobs not supported in MySql."
                        // Get the blob.
                        | _ -> (fn @ [ f.MappingName ], pn @ [ $"@{f.MappingName}" ]))
                    ([], [])

            let fields = String.Join(',', fieldNames)
            let parameters = String.Join(',', parameterNames)

            let sql =
                $"""
            INSERT INTO {tableName} ({fields})
            VALUES ({parameters});
            """

            sql

        /// Prepare the `INSERT` query and return a `SqliteCommand` ready for execution.
        /// `BlobField` types will be skipped over, due to being handled separately.
        let prepareQuery<'P>
            (connection: MySqlConnection)
            (sql: string)
            (mappedObj: MappedObject)
            (parameters: 'P)
            (transaction: MySqlTransaction option)
            =

            if connection.State = ConnectionState.Closed then
                connection.Open()

            use comm =
                match transaction with
                | Some t -> new MySqlCommand(sql, connection, t)
                | None -> new MySqlCommand(sql, connection)

            mappedObj.Fields
            |> List.sortBy (fun p -> p.Index)
            |> List.fold
                (fun acc f ->
                    match f.Type with
                    | SupportedType.Blob -> acc // Skip blob types, they will be handled with `BlobCallBacks`.
                    | SupportedType.Option _ ->
                        match mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters) with
                        | null -> acc @ [ f.MappingName, DBNull.Value :> obj ]
                        | SomeObj(v1) -> acc @ [ f.MappingName, v1 ]
                        | _ -> acc @ [ f.MappingName, DBNull.Value :> obj ]
                    | _ ->
                        acc
                        @ [ f.MappingName, mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters) ])
                []
            |> Map.ofList
            |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
            |> ignore

            // TODO add ability to set timeout?
            // comm.CommandTimeout <- 5000

            comm.Prepare()
            comm

    let insert<'T> (tableName: string) connection (data: 'T) transaction =
        let mappedObj = MappedObject.Create<'T>()

        let sql = Insert.createQuery tableName mappedObj data

        // Get the last inserted id.
        let comm = Insert.prepareQuery connection sql mappedObj data transaction

        comm.ExecuteNonQuery() |> ignore


        let idSql = "SELECT LAST_INSERT_ID();"

        use idComm =
            match transaction with
            | Some t -> new MySqlCommand(idSql, connection, t)
            | None -> new MySqlCommand(idSql, connection)

        let rowId = idComm.ExecuteScalar() :?> uint64

        rowId

type MySqlContext(connection, transaction) =

    interface IDisposable with

        member ctx.Dispose() = ctx.Close()

    static member Connect(connectionString: string) =

        // let is used here instead of use.
        // In `MySql.Data` version `8.0.33` use was fine.
        // However, in `8.3.0` something changed and the connection would be disposed of instantly.
        // Using let means it **should** be managed by the MySqlContext object,
        // when the object is disposed the connect can be as well.
        // This should also sort the issue.
        // See https://github.com/mc738/Freql/issues/20
        let conn = new MySqlConnection(connectionString)

        new MySqlContext(conn, None)


    member _.Close() =
        connection.Close()
        connection.Dispose()

    member _.GetConnection() = connection

    member _.ClearPool() = MySqlConnection.ClearPool(connection)

    member _.ClearAllPools() = MySqlConnection.ClearAllPools()

    member _.GetConnectionState() = connection.State

    member _.GetDatabase() = connection.Database

    member _.OnStateChange(fn: StateChangeEventArgs -> unit) = connection.StateChange.Add(fn)

    /// Select all items from a table and map them to type 'T.
    member handler.Select<'T> tableName =
        QueryHelpers.selectAll<'T> tableName connection transaction

    /// Select data based on a verbatim sql and parameters of type 'P.
    /// Map the result to type 'T.
    member handler.SelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.select<'T, 'P> sql connection parameters transaction

    /// Select a list of 'T based on an sql string and a list of obj for parameters.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    member handler.SelectAnon<'T>(sql, parameters) =
        QueryHelpers.selectAnon<'T> sql connection parameters transaction

    /// Select a single 'T based on an sql string and a list of obj for parameters.
    /// This will return an optional value.
    /// Parameters will be assigned values @0,@1,@2 etc. based on their position in the list
    /// when the are parameterized.
    member handler.SelectSingleAnon<'T>(sql, parameters) =
        let r = handler.SelectAnon<'T>(sql, parameters)

        match r.Length > 0 with
        | true -> r.Head |> Some
        | false -> None

    /// Select a list of 'T based on an sql string.
    /// No parameterization will take place with this, it should only be used with static sql strings.
    member handler.SelectSql<'T> sql =
        QueryHelpers.selectSql<'T> (sql) connection transaction

    /// Select a single 'T from a table.
    /// This is useful if a table on contains one record. It will return the first from that table.
    /// Be warned, this will throw an exception if the table is empty.
    member handler.SelectSingle<'T> tableName = handler.Select<'T>(tableName).Head

    /// Select data based on a verbatim sql and parameters of type 'P.
    /// The first result is mapped to type 'T option.
    member handler.SelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        let result = handler.SelectVerbatim<'T, 'P>(sql, parameters)

        match List.isEmpty result with
        | true -> None
        | false -> Some result.Head

    /// Execute a raw sql non query. What is passed as a parameters is what will be executed.
    /// WARNING: do not used with untrusted input.
    member handler.ExecuteSqlNonQuery(sql: string) =
        QueryHelpers.rawNonQuery connection sql transaction

    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    member handler.ExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.verbatimNonQuery connection sql parameters transaction


    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    member handler.ExecuteAnonNonQuery<'P>(sql: string, parameters) =
        QueryHelpers.verbatimNonQueryAnon connection sql parameters transaction

    /// Execute an insert query.
    member handler.Insert<'T>(tableName: string, value: 'T) =
        QueryHelpers.insert<'T> tableName connection value transaction

    /// Execute a collection of insert queries.
    member handler.InsertList<'T>(tableName: string, values: 'T list) =
        values |> List.map (fun v -> handler.Insert<'T>(tableName, v)) |> ignore


    /// Execute a collection of commands in a transaction.
    /// While a transaction is active on a connection non transaction commands can not be executed.
    /// This is no check for this. Transactions are not is not thread safe.
    /// Also be warned, this use general error handling so an exception will roll the transaction back.
    member handler.ExecuteInTransaction<'R>(transactionFn: MySqlContext -> 'R) =
        if connection.State = ConnectionState.Closed then
            connection.Open()

        use transaction = connection.BeginTransaction()

        use qh = new MySqlContext(connection, Some transaction)

        try
            let r = transactionFn qh
            transaction.Commit()
            Ok r
        with exn ->
            // Needed? ensures open for rollback?
            if connection.State = ConnectionState.Closed then
                connection.Open()

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
    member handler.TryExecuteInTransaction<'R>(transactionFn: MySqlContext -> Result<'R, string>) =
        connection.Open()

        use transaction = connection.BeginTransaction()

        use qh = new MySqlContext(connection, Some transaction)

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
    member handler.ExecuteScalar<'T>(sql) =
        QueryHelpers.executeScalar<'T> sql connection transaction

    /// Execute a bespoke query, it is upto to the caller to provide the sql, the parameters and the result mapping function.
    member handler.Bespoke<'T>(sql, parameters, (mapper: MySqlDataReader -> 'T list)) =
        QueryHelpers.bespoke connection sql parameters mapper transaction

    /// Test the database connection.
    /// Useful for health checks.
    member handler.TestConnection() =
        QueryHelpers.executeScalar<int> "SELECT 1" connection transaction
