﻿namespace Freql.SqlServer

open System
open System.Data
open System.Data.SqlClient
open System.IO
open System.Text.Json
open Freql.Core
open Freql.Core.Mapping
open Freql.Core.Utils
module private QueryHelpers =

    let mapParameters<'T> (mappedObj: MappedObject) (parameters: 'T) =
        mappedObj.Fields
        |> List.sortBy (fun p -> p.Index)
        |> List.map
            (fun f ->
                let v =
                    mappedObj
                        .Type
                        .GetProperty(f.FieldName)
                        .GetValue(parameters)

                f.MappingName, v)
        |> Map.ofList

    let mapResults<'T> (mappedObj: MappedObject) (reader: SqlDataReader) =
        let getValue (reader: SqlDataReader) (o: int) supportType =
            match supportType with
            | SupportedType.Boolean -> reader.GetBoolean(o) :> obj
            | SupportedType.Byte -> reader.GetByte(o) :> obj
            | SupportedType.SByte -> reader.GetByte(o) |> sbyte :> obj // TODO not supported in SQLServer - document this
            | SupportedType.Char -> reader.GetChar(o) :> obj
            | SupportedType.Decimal -> reader.GetDecimal(o) :> obj
            | SupportedType.Double -> reader.GetDouble(o) :> obj
            | SupportedType.Single -> reader.GetFloat(o) :> obj
            | SupportedType.Int -> reader.GetInt32(o) :> obj
            | SupportedType.UInt -> reader.GetInt32(o) |> uint32 :> obj // TODO not supported in SQLServer - document this
            | SupportedType.Short -> reader.GetInt16(o) :> obj
            | SupportedType.UShort -> reader.GetInt16(o) |> uint16 :> obj
            | SupportedType.Long -> reader.GetInt64(o) :> obj
            | SupportedType.ULong -> reader.GetInt64(o) |> uint64 :> obj
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
                    | SupportedType.Boolean -> Some (reader.GetBoolean(o)) :> obj
                    | SupportedType.Byte -> Some (reader.GetByte(o)) :> obj
                    | SupportedType.SByte -> Some (reader.GetByte(o) |> sbyte) :> obj
                    | SupportedType.Char -> Some (reader.GetChar(o)) :> obj
                    | SupportedType.Decimal -> Some (reader.GetDecimal(o)) :> obj
                    | SupportedType.Double -> Some (reader.GetDouble(o)) :> obj
                    | SupportedType.Single -> Some (reader.GetFloat(o)) :> obj
                    | SupportedType.Int -> Some (reader.GetInt32(o)) :> obj
                    | SupportedType.UInt -> Some (reader.GetInt32(o) |> uint32) :> obj
                    | SupportedType.Short -> Some (reader.GetInt16(o)) :> obj
                    | SupportedType.UShort -> Some (reader.GetInt16(o) |> uint16) :> obj
                    | SupportedType.Long -> Some (reader.GetInt64(o)) :> obj
                    | SupportedType.ULong -> Some (reader.GetInt64(o) |> uint64) :> obj
                    | SupportedType.String -> Some (reader.GetString(o)) :> obj
                    | SupportedType.DateTime -> Some (reader.GetDateTime(o)) :> obj
                    | SupportedType.TimeSpan -> Some (reader.GetTimeSpan(o)) :> obj
                    | SupportedType.Guid -> Some (reader.GetGuid(o)) :> obj
                    | SupportedType.Blob -> Some (BlobField.FromStream(reader.GetStream(o))) :> obj
                    | SupportedType.Option _ -> None :> obj // Nested options not allowed.        
        
        [ while reader.Read() do
              mappedObj.Fields
              |> List.map
                  (fun f ->
                      let o = reader.GetOrdinal(f.MappingName)
                      let value = getValue reader o f.Type
                      { Index = f.Index; Value = value })
              |> (fun v -> RecordBuilder.Create<'T> v) ]

    let noParam (connection: SqlConnection) (sql: string) (transaction: SqlTransaction option) =

        connection.Open()
        use comm =
            match transaction with
            | Some t -> new SqlCommand(sql, connection, t)
            | None -> new SqlCommand(sql, connection)
        comm

    let prepare<'P> (connection: SqlConnection) (sql: string) (mappedObj: MappedObject) (parameters: 'P) (transaction: SqlTransaction option) =
        connection.Open()
        
        use comm =
            match transaction with
                | Some t -> new SqlCommand(sql, connection, t)
                | None -> new SqlCommand(sql, connection)
       
        parameters
        |> mapParameters<'P> mappedObj
        |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
        |> ignore

        comm.Prepare()
        comm

    let prepareAnon (connection: SqlConnection) (sql: string) (parameters: obj list) (transaction: SqlTransaction option) =
        
        if connection.State = ConnectionState.Closed then
            connection.Open()
        
        use comm =
            match transaction with
                | Some t -> new SqlCommand(sql, connection, t)
                | None -> new SqlCommand(sql, connection)
       
        parameters
        |> List.mapi (fun i v -> comm.Parameters.AddWithValue($"@{i}", v))
        |> ignore

        comm.Prepare()
        comm
    
    let rawNonQuery connection sql transaction =
        let comm =  noParam connection sql transaction

        comm.ExecuteNonQuery()

    let verbatimNonQuery<'P> connection sql (parameters: 'P) transaction  =
        let mappedObj = MappedObject.Create<'P>()
        let comm = prepare connection sql mappedObj parameters transaction
        comm.ExecuteNonQuery()

    /// A bespoke query, the caller needs to provide a mapping function. This returns a list of 'T.    
    let bespoke<'T> connection (sql: string) (parameters: obj list) (mapper: SqlDataReader -> 'T list) transaction  =
        let comm = prepareAnon connection sql parameters transaction
        use reader = comm.ExecuteReader()
        mapper reader
        
    /// A bespoke query, the caller needs to provide a mapping function. This returns a single 'T.
    let bespokeSingle<'T> connection (sql: string) (parameters: obj list) (mapper: SqlDataReader -> 'T) transaction  =
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

    let select<'T, 'P> (sql: string) connection (parameters: 'P) transaction =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm =
            prepare connection sql pMappedObj parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    let executeScalar<'T>(sql: string) connection transaction =
        let comm = noParam connection sql transaction
        comm.ExecuteScalar() :?> 'T
    
    let selectSql<'T> (sql: string) connection transaction =
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
                            // TODO implement optional blobs for SqlServer
                            failwith "Blobs not supported in SqlServer."
                        | SupportedType.Blob ->
                            // TODO implement optional blobs for SqlServer
                            failwith "Blobs not supported in SqlServer."
                            // Get the blob.
                        | _ -> (fn @ [ f.MappingName ], pn @ [ $"@{f.MappingName}" ]))
                    ([], [])

            let fields = String.Join(',', fieldNames)
            let parameters = String.Join(',', parameterNames)

            let sql =
                $"""
            INSERT INTO {tableName} ({fields})
            VALUES ({parameters});
            SELECT last_insert_rowid();
            """

            sql

        /// Prepare the `INSERT` query and return a `SqliteCommand` ready for execution.
        /// `BlobField` types will be skipped over, due to being handled separately.
        let prepareQuery<'P> (connection: SqlConnection) (sql: string) (mappedObj: MappedObject) (parameters: 'P) (transaction: SqlTransaction option) =
            connection.Open()

            use comm =
                match transaction with
                | Some t -> new SqlCommand(sql, connection, t)
                | None -> new SqlCommand(sql, connection)

            mappedObj.Fields
            |> List.sortBy (fun p -> p.Index)
            |> List.fold
                (fun acc f ->
                    match f.Type with
                    | SupportedType.Blob -> acc // Skip blob types, they will be handled with `BlobCallBacks`.
                    | SupportedType.Option _ ->
                        match mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters) with
                        | null ->
                            acc
                            @ [ f.MappingName, DBNull.Value :> obj ]
                        | SomeObj(v1) ->
                            acc
                            @ [ f.MappingName, v1 ]
                        | _ ->
                            acc
                            @ [ f.MappingName, DBNull.Value :> obj ]
                    | _ ->
                        acc
                        @ [ f.MappingName,
                            mappedObj
                                .Type
                                .GetProperty(f.FieldName)
                                .GetValue(parameters) ])
                []
            |> Map.ofList
            |> Map.map (fun k v -> comm.Parameters.AddWithValue(k, v))
            |> ignore

            comm.Prepare()
            comm

    let insert<'T> (tableName: string) connection (data: 'T) transaction =
        let mappedObj = MappedObject.Create<'T>()

        let sql = Insert.createQuery tableName mappedObj data

        // Get the last inserted id.
        let comm = Insert.prepareQuery connection sql mappedObj data transaction

        let rowId = comm.ExecuteScalar() :?> int64

        Ok rowId

type SqlServerContext(connection, transaction) =

    static member Connect(connectionString: string) =

        use conn =
            new SqlConnection(connectionString)

        SqlServerContext(conn, None)

    member _.Close() =
       connection.Close()
       connection.Dispose()
    
    member _.GetConnection() = connection
    
    member _.ClearPool() = SqlConnection.ClearPool(connection)
    
    member _.ClearAllPools() = SqlConnection.ClearAllPools()
    
    member _.GetConnectionState() = connection.State
    
    member _.GetDatabase() = connection.Database
    
    member _.OnStateChange(fn: StateChangeEventArgs -> unit) = connection.StateChange.Add(fn)
        
    
    member handler.Select<'T> tableName =
        QueryHelpers.selectAll<'T> tableName connection transaction

    /// Select data based on a verbatim sql and parameters.
    member handler.SelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.select<'T, 'P> sql connection parameters transaction

    member handler.SelectSql<'T> sql =
        QueryHelpers.selectSql<'T>(sql) connection transaction
    
    member handler.SelectSingle<'T> tableName = handler.Select<'T>(tableName).Head

    member handler.SelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        handler
            .SelectVerbatim<'T, 'P>(
                sql,
                parameters
            )
            .Head

    /// Execute a raw sql non query. What is passed as a parameters is what will be executed.
    /// WARNING: do not used with untrusted input.
    member handler.ExecuteSqlNonQuery(sql: string) =
        QueryHelpers.rawNonQuery connection sql transaction

    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    member handler.ExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.verbatimNonQuery connection sql parameters transaction
    
    /// Execute an insert query.
    member handler.Insert<'T>(tableName: string, value: 'T) =
        QueryHelpers.insert<'T> tableName connection value transaction

    /// Execute a collection of insert queries.
    member handler.InsertList<'T>(tableName: string, values: 'T list) =
        values
        |> List.map (fun v -> handler.Insert<'T>(tableName, v))
        |> ignore


    /// Execute a collection of commands in a transaction.
    /// While a transaction is active on a connection non transaction commands can not be executed.
    /// This is no check for this for this is not thread safe.
    /// Also be warned, this use general error handling so an exception will roll the transaction back.
    member handler.ExecuteInTransaction<'R>(transactionFn: SqlServerContext -> 'R) =
        connection.Open()
        use transaction = connection.BeginTransaction()
        
        let ctx = SqlServerContext(connection, Some transaction)
        
        try
            let r = transactionFn ctx
            transaction.Commit()
            Ok r
        with
        | exn ->
            transaction.Rollback()
            Error { Message = $"Could not complete transaction. Exception: {exn.Message}"; Exception = Some exn }
                          
    /// Execute sql that produces a scalar result.
    member handler.ExecuteScalar<'T>(sql) =
        QueryHelpers.executeScalar<'T> sql connection transaction
                       
    /// Execute a bespoke query, it is upto to the caller to provide the sql, the parameters and the result mapping function.
    member handler.Bespoke<'T>(sql, parameters, (mapper: SqlDataReader -> 'T list)) =
        QueryHelpers.bespoke connection sql  parameters  mapper transaction

    /// Test the database connection.
    /// Useful for health checks.
    member handler.TestConnection() = QueryHelpers.executeScalar<int> "SELECT 1" connection transaction
    