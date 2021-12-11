namespace Freql.Sqlite

open System
open System.IO
open System.Text.Json
open Freql.Core.Common
open Freql.Core.Common.Mapping
open Microsoft.Data.Sqlite
open Freql.Core.Utils
open Microsoft.Data.Sqlite

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
                    | SupportedType.Boolean -> Some (reader.GetBoolean(o)) :> obj
                    | SupportedType.Byte -> Some (reader.GetByte(o)) :> obj
                    | SupportedType.Char -> Some (reader.GetChar(o)) :> obj
                    | SupportedType.Decimal -> Some (reader.GetDecimal(o)) :> obj
                    | SupportedType.Double -> Some (reader.GetDouble(o)) :> obj
                    | SupportedType.Float -> Some (reader.GetFloat(o)) :> obj
                    | SupportedType.Int -> Some (reader.GetInt32(o)) :> obj
                    | SupportedType.Short -> Some (reader.GetInt16(o)) :> obj
                    | SupportedType.Long -> Some (reader.GetInt64(o)) :> obj
                    | SupportedType.String -> Some (reader.GetString(o)) :> obj
                    | SupportedType.DateTime -> Some (reader.GetDateTime(o)) :> obj
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

    //let mapResult<'T> (mappedObj: MappedObject) 
    
    let noParam (connection: SqliteConnection) (sql: string) (transaction: SqliteTransaction option) =

        connection.Open()
        use comm =
            match transaction with
            | Some t -> new SqliteCommand(sql, connection, t)
            | None -> new SqliteCommand(sql, connection)
        comm

    let prepare<'P> (connection: SqliteConnection) (sql: string) (mappedObj: MappedObject) (parameters: 'P) (transaction: SqliteTransaction option) =
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
        
    let prepareAnon (connection: SqliteConnection) (sql: string) (parameters: obj list) (transaction: SqliteTransaction option) =
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
        let comm =  noParam connection sql transaction

        comm.ExecuteNonQuery()

    let verbatimNonQuery<'P> (connection: SqliteConnection) (sql: string) (parameters: 'P) (transaction: SqliteTransaction option)  =
        let mappedObj = MappedObject.Create<'P>()
        let comm = prepare connection sql mappedObj parameters transaction
        comm.ExecuteNonQuery()
    
    let verbatimNonQueryAnon<'P> (connection: SqliteConnection) (sql: string) (parameters: obj list) (transaction: SqliteTransaction option)  =
        let comm = prepareAnon connection sql parameters transaction
        comm.ExecuteNonQuery()
    
    /// A bespoke query, the caller needs to provide a mapping function. This returns a list of 'T.    
    let bespoke<'T>(connection: SqliteConnection) (sql: string) (parameters: obj list) (mapper: SqliteDataReader -> 'T list) (transaction: SqliteTransaction option)  =
        let comm = prepareAnon connection sql parameters transaction
        use reader = comm.ExecuteReader()
        mapper reader
        
    /// A bespoke query, the caller needs to provide a mapping function. This returns a single 'T.
    let bespokeSingle<'T>(connection: SqliteConnection) (sql: string) (parameters: obj list) (mapper: SqliteDataReader -> 'T) (transaction: SqliteTransaction option)  =
        let comm = prepareAnon connection sql parameters transaction
        use reader = comm.ExecuteReader()
        mapper reader
        
        
    let create<'T> (tableName: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let mappedObj = MappedObject.Create<'T>()

        let columns =
            mappedObj.Fields
            |> List.sortBy (fun p -> p.Index)
            |> List.map
                (fun f ->
                    let template (colType: string) = $"{f.MappingName} {colType}"

                    let blobField =
                        $"{f.MappingName} BLOB, {f.MappingName}_sha256_hash TEXT"

                    match f.Type with
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
                    | SupportedType.Blob -> template "BLOB")
        //| SupportedType.Json -> template "BLOB")

        let columnsString = System.String.Join(',', columns)

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

    let select<'T, 'P> (sql: string) (connection: SqliteConnection) (parameters: 'P) (transaction: SqliteTransaction option) =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm =
            prepare connection sql pMappedObj parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    let selectAnon<'T>  (sql: string) (connection: SqliteConnection) (parameters: obj list) (transaction: SqliteTransaction option) =
        let tMappedObj = MappedObject.Create<'T>()
        let comm =
            prepareAnon connection sql parameters transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader
       
    let selectSingle<'T, 'P> (sql: string) (connection: SqliteConnection) (parameters: 'P) (transaction: SqliteTransaction option) =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm =
            prepare connection sql pMappedObj parameters transaction

        let r = comm.ExecuteScalar()
        
        
        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader
      
    let executeScalar<'T>(sql: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let comm = noParam connection sql transaction
        comm.ExecuteScalar() :?> 'T
       
    let selectSql<'T> (sql: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = noParam connection sql transaction

        use reader = comm.ExecuteReader()

        mapResults<'T> tMappedObj reader

    [<RequireQualifiedAccess>]
    /// Special handling is needed for `INSERT` query to accommodate blobs.
    /// This module aims to wrap as much of that up to in one place.
    module private Insert =

        type InsertBlobCallback = { ColumnName: string; Data: Stream }

        /// Create an insert query and return the sql and a list of `InsertBlobCallback`'s.
        let createQuery<'T> (tableName: string) (mappedObj: MappedObject) (data: 'T) =
            let fieldNames, parameterNames, blobCallbacks =                
                mappedObj.Fields
                |> List.fold
                    (fun (fn, pn, cb) f ->

                        match f.Type with
                        | SupportedType.Blob ->
                            // Get the blob.
                            let stream =
                                (mappedObj
                                    .Type
                                    .GetProperty(f.FieldName)
                                    .GetValue(data)
                                :?> BlobField)
                                    .Value

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
        let prepareQuery<'P> (connection: SqliteConnection) (sql: string) (mappedObj: MappedObject) (parameters: 'P) (transaction: SqliteTransaction option) =
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

        let handleBlobCallbacks
            (connection: SqliteConnection)
            (tableName: string)
            (callbacks: InsertBlobCallback list)
            rowId
            =
            callbacks
            |> List.map
                (fun cb ->
                    use writeStream =
                        new SqliteBlob(connection, tableName, cb.ColumnName, rowId)

                    cb.Data.CopyTo(writeStream))
            |> ignore

    let insert<'T> (tableName: string) (connection: SqliteConnection) (data: 'T) (transaction: SqliteTransaction option) =
        let mappedObj = MappedObject.Create<'T>()

        let sql, callbacks =
            Insert.createQuery tableName mappedObj data

        // Get the last inserted id.
        let comm =
            Insert.prepareQuery connection sql mappedObj data transaction

        let rowId = comm.ExecuteScalar() :?> int64

        Insert.handleBlobCallbacks connection tableName callbacks rowId

[<Obsolete("Use SqliteContext instead.")>]
type QueryHandler(connection: SqliteConnection, transaction: SqliteTransaction option) =

    static member Create(path: string) =
        printfn $"Creating database '{path}'."
        File.WriteAllBytes(path, [||])

        use conn =
            new SqliteConnection($"Data Source={path}")

        QueryHandler(conn, None)

    static member Open(path: string) =
        printfn $"Connection to database '{path}'."

        use conn =
            new SqliteConnection($"Data Source={path}")

        QueryHandler(conn, None)

    static member Connect(connectionString: string) =

        use conn =
            new SqliteConnection(connectionString)

        QueryHandler(conn, None)

    
    member _.Close() =
       connection.Close()
       connection.Dispose()
    
    member handler.Select<'T> tableName =
        QueryHelpers.selectAll<'T> tableName connection transaction

    /// Select data based on a verbatim sql and parameters.
    member handler.SelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.select<'T, 'P> sql connection parameters transaction

    member handler.SelectAnon<'T>(sql, parameters) =
        QueryHelpers.selectAnon<'T> sql connection parameters transaction
        
    
    member handler.SelectSingleAnon<'T>(sql, parameters) =
        let r = handler.SelectAnon<'T>(sql, parameters)
        match r.Length > 0 with
        | true -> r.Head |> Some
        | false -> None
           
    member handler.SelectSql<'T> sql =
        QueryHelpers.selectSql<'T>(sql) connection transaction
    
    member handler.SelectSingle<'T> tableName = handler.Select<'T>(tableName).Head

    member handler.SelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        let result = handler.SelectVerbatim<'T, 'P>(sql, parameters)
        
        match result.Length with
        | 0 -> None
        | _ -> Some result.Head

    /// Execute a create table query based on a generic record.
    member handler.CreateTable<'T>(tableName: string) =
        QueryHelpers.create<'T> tableName connection transaction

    /// Execute a raw sql non query. What is passed as a parameters is what will be executed.
    /// WARNING: do not used with untrusted input.
    member handler.ExecuteSqlNonQuery(sql: string) =
        QueryHelpers.rawNonQuery connection sql transaction

    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    member handler.ExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.verbatimNonQuery connection sql parameters transaction
        
    member handler.ExecuteVerbatimNonQueryAnon(sql: string, parameters: obj list) =
        QueryHelpers.verbatimNonQueryAnon connection sql parameters transaction
    
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
    member handler.ExecuteInTransaction<'R>(transactionFn: QueryHandler -> 'R) =
        connection.Open()
        use transaction = connection.BeginTransaction()
        
        let qh = QueryHandler(connection, Some transaction)
        
        try
            let r = transactionFn qh
            transaction.Commit()
            Ok r
        with
        | _ ->
            transaction.Rollback()
            Error "Could not complete transaction"
                 
    member handler.ExecuteScalar<'T>(sql) =
        QueryHelpers.executeScalar<'T> sql connection transaction
                       
    member handler.Bespoke<'T>(sql, parameters, (mapper: SqliteDataReader -> 'T list)) =
        QueryHelpers.bespoke connection sql  parameters  mapper transaction
        
    member handler.TestConnection() = QueryHelpers.executeScalar<int64> "SELECT 1" connection transaction
    
type SqliteContext(connection: SqliteConnection, transaction: SqliteTransaction option) =

    static member Create(path: string) =
        printfn $"Creating database '{path}'."
        File.WriteAllBytes(path, [||])

        use conn =
            new SqliteConnection($"Data Source={path}")

        QueryHandler(conn, None)

    static member Open(path: string) =
        printfn $"Connection to database '{path}'."

        use conn =
            new SqliteConnection($"Data Source={path}")

        QueryHandler(conn, None)

    static member Connect(connectionString: string) =

        use conn =
            new SqliteConnection(connectionString)

        QueryHandler(conn, None)

    
    member _.Close() =
       connection.Close()
       connection.Dispose()
    
    member handler.Select<'T> tableName =
        QueryHelpers.selectAll<'T> tableName connection transaction

    /// Select data based on a verbatim sql and parameters.
    member handler.SelectVerbatim<'T, 'P>(sql, parameters) =
        QueryHelpers.select<'T, 'P> sql connection parameters transaction

    member handler.SelectAnon<'T>(sql, parameters) =
        QueryHelpers.selectAnon<'T> sql connection parameters transaction
        
    
    member handler.SelectSingleAnon<'T>(sql, parameters) =
        let r = handler.SelectAnon<'T>(sql, parameters)
        match r.Length > 0 with
        | true -> r.Head |> Some
        | false -> None
           
    member handler.SelectSql<'T> sql =
        QueryHelpers.selectSql<'T>(sql) connection transaction
    
    member handler.SelectSingle<'T> tableName = handler.Select<'T>(tableName).Head

    member handler.SelectSingleVerbatim<'T, 'P>(sql: string, parameters: 'P) =
        let result = handler.SelectVerbatim<'T, 'P>(sql, parameters)
        
        match result.Length with
        | 0 -> None
        | _ -> Some result.Head

    /// Execute a create table query based on a generic record.
    member handler.CreateTable<'T>(tableName: string) =
        QueryHelpers.create<'T> tableName connection transaction

    /// Execute a raw sql non query. What is passed as a parameters is what will be executed.
    /// WARNING: do not used with untrusted input.
    member handler.ExecuteSqlNonQuery(sql: string) =
        QueryHelpers.rawNonQuery connection sql transaction

    /// Execute a verbatim non query. The parameters passed will be mapped to the sql query.
    member handler.ExecuteVerbatimNonQuery<'P>(sql: string, parameters: 'P) =
        QueryHelpers.verbatimNonQuery connection sql parameters transaction
        
    member handler.ExecuteVerbatimNonQueryAnon(sql: string, parameters: obj list) =
        QueryHelpers.verbatimNonQueryAnon connection sql parameters transaction
    
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
    member handler.ExecuteInTransaction<'R>(transactionFn: QueryHandler -> 'R) =
        connection.Open()
        use transaction = connection.BeginTransaction()
        
        let qh = QueryHandler(connection, Some transaction)
        
        try
            let r = transactionFn qh
            transaction.Commit()
            Ok r
        with
        | _ ->
            transaction.Rollback()
            Error "Could not complete transaction"
                 
    member handler.ExecuteScalar<'T>(sql) =
        QueryHelpers.executeScalar<'T> sql connection transaction
                       
    member handler.Bespoke<'T>(sql, parameters, (mapper: SqliteDataReader -> 'T list)) =
        QueryHelpers.bespoke connection sql  parameters  mapper transaction
        
    member handler.TestConnection() = QueryHelpers.executeScalar<int64> "SELECT 1" connection transaction
