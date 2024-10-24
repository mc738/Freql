namespace Freql.Sqlite

[<CompilerMessage("This module is intended for internal use. To remove this warning add #nowarn \"6140001\"", 6140001)>]
module QueryHelpers =

    open System
    open System.IO
    open Freql.Core
    open Freql.Core.Mapping
    open Microsoft.Data.Sqlite
    open Freql.Core.Utils

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

    // TODO test this to see if there as a performance hit.
    // FUTURE: Use this for mapping a single. value.
    // BENCHMARK: Test this versus current implementation
    [<CompilerMessage("This is a new implementation and requires testing/benchmarking. To remove this warning add #nowarn \"6140003\"", 6140003)>]
    let inline mapResult<'T>  (reader: SqliteDataReader) ordinal supportType =
        match supportType with
            | SupportedType.Boolean -> reader.GetBoolean(ordinal) :> obj
            | SupportedType.Byte -> reader.GetByte(ordinal) :> obj
            | SupportedType.Char -> reader.GetChar(ordinal) :> obj
            | SupportedType.Decimal -> reader.GetDecimal(ordinal) :> obj
            | SupportedType.Double -> reader.GetDouble(ordinal) :> obj
            | SupportedType.Float -> reader.GetFloat(ordinal) :> obj
            | SupportedType.Int -> reader.GetInt32(ordinal) :> obj
            | SupportedType.Short -> reader.GetInt16(ordinal) :> obj
            | SupportedType.Long -> reader.GetInt64(ordinal) :> obj
            | SupportedType.String -> reader.GetString(ordinal) :> obj
            | SupportedType.DateTime -> reader.GetDateTime(ordinal) :> obj
            | SupportedType.Guid -> reader.GetGuid(ordinal) :> obj
            | SupportedType.Blob -> BlobField.FromStream(reader.GetStream(ordinal)) :> obj
            | SupportedType.Option st ->
                match reader.IsDBNull(ordinal) with
                | true -> None :> obj
                | false ->
                    match st with
                    | SupportedType.Boolean -> Some(reader.GetBoolean(ordinal)) :> obj
                    | SupportedType.Byte -> Some(reader.GetByte(ordinal)) :> obj
                    | SupportedType.Char -> Some(reader.GetChar(ordinal)) :> obj
                    | SupportedType.Decimal -> Some(reader.GetDecimal(ordinal)) :> obj
                    | SupportedType.Double -> Some(reader.GetDouble(ordinal)) :> obj
                    | SupportedType.Float -> Some(reader.GetFloat(ordinal)) :> obj
                    | SupportedType.Int -> Some(reader.GetInt32(ordinal)) :> obj
                    | SupportedType.Short -> Some(reader.GetInt16(ordinal)) :> obj
                    | SupportedType.Long -> Some(reader.GetInt64(ordinal)) :> obj
                    | SupportedType.String -> Some(reader.GetString(ordinal)) :> obj
                    | SupportedType.DateTime -> Some(reader.GetDateTime(ordinal)) :> obj
                    | SupportedType.Guid -> Some(reader.GetGuid(ordinal)) :> obj
                    | SupportedType.Blob -> Some(BlobField.FromStream(reader.GetStream(ordinal))) :> obj
                    | SupportedType.Option _ -> None :> obj // Nested options not allowed.

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
    
    // FUTURE: Use this for operations that return a single result
    [<CompilerMessage("This is a new implementation and requires testing/benchmarking. To remove this warning add #nowarn \"6140003\"", 6140003)>]
    let mapSingleResult<'T> (mappedObj: MappedObject) (reader: SqliteDataReader) =
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

        // TODO this needs testing
        if reader.Read() then
            mappedObj.Fields
              |> List.map (fun f ->
                  let o = reader.GetOrdinal(f.MappingName)
                  let value = getValue reader o f.Type
                  { Index = f.Index; Value = value })
              |> (fun v -> RecordBuilder.Create<'T> v)
              |> Some
        else None
            
    /// <summary>
    /// Map the requests of a sql command in a deferred way.
    /// This takes a SqliteCommand rather than a SqliteDataReader because the reader needs to still be open when it
    /// the seq is enumerated.
    /// </summary>
    /// <param name="mappedObj"></param>
    /// <param name="comm"></param>
    let deferredMapResults<'T> (mappedObj: MappedObject) (comm: SqliteCommand) =
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

    let deferredBespoke<'T>
        (connection: SqliteConnection)
        (sql: string)
        (parameters: obj list)
        (mapper: SqliteDataReader -> 'T seq)
        (transaction: SqliteTransaction option)
        =

        let comm = prepareAnon connection sql parameters transaction

        seq {
            use reader = comm.ExecuteReader()

            yield! mapper reader
        }

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

    let deferredSelectAll<'T>
        (tableName: string)
        (connection: SqliteConnection)
        (transaction: SqliteTransaction option)
        =
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

        deferredMapResults<'T> mappedObj comm

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
     
    // FUTURE: USe this for operations that return a single result.
    // TODO rename to selectSingle
    [<CompilerMessage("This is a new implementation and requires testing.", 6140003)>]
    let _selectSingle<'T, 'P>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        use reader = comm.ExecuteReader()

        mapSingleResult<'T> tMappedObj reader

    let deferredSelect<'T, 'P>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()
        let pMappedObj = MappedObject.Create<'P>()

        let comm = prepare connection sql pMappedObj parameters transaction

        //use reader = comm.ExecuteReader()

        deferredMapResults<'T> tMappedObj comm

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
        
     // FUTURE: Use this for operations that return a single result
    [<CompilerMessage("This is a new implementation and requires testing/benchmarking. To remove this warning add #nowarn \"6140003\"", 6140003)>]
    let selectAnonSingle<'T>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: obj list)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = prepareAnon connection sql parameters transaction

        use reader = comm.ExecuteReader()

        mapSingleResult<'T> tMappedObj reader

    let deferredSelectAnon<'T>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: obj list)
        (transaction: SqliteTransaction option)
        =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = prepareAnon connection sql parameters transaction

        //use reader = comm.ExecuteReader()

        deferredMapResults<'T> tMappedObj comm

    let selectSingle<'T, 'P>
        (sql: string)
        (connection: SqliteConnection)
        (parameters: 'P)
        (transaction: SqliteTransaction option)
        =
        // TODO fix this up, it appears not be selecting a single.
        
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

    let deferredSelectSql<'T> (sql: string) (connection: SqliteConnection) (transaction: SqliteTransaction option) =
        let tMappedObj = MappedObject.Create<'T>()

        let comm = noParam connection sql transaction

        deferredMapResults<'T> tMappedObj comm

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

    let attempt<'T> (fn: unit -> 'T) =
        try
            fn () |> Ok
        with ex ->
            SQLiteFailure.FromException ex |> Error
