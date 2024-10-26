namespace Freql.Core

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Core

module Diagnostics =

    type SqlOperation =
        | Select
        | Update
        | Delete
        | InsertInto
        | CreateDatabase
        | AlterDatabase
        | CreateTable
        | AlterTable
        | DropTable
        | CreateIndex
        | DropIndex
        | Other of string

        member op.Serialize() =
            match op with
            | Select -> "SELECT"
            | Update -> "UPDATE"
            | Delete -> "DELETE"
            | InsertInto -> "INSERT"
            | CreateDatabase -> "CREATE DATABASE"
            | AlterDatabase -> "ALTER DATABASE"
            | CreateTable -> "CREATE"
            | AlterTable -> "ALTER TABLE"
            | DropTable -> "DROP TABLE"
            | CreateIndex -> "CREATE INDEX"
            | DropIndex -> "DROP INDEX"
            | Other s -> s

    /// <summary>
    /// From https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#target-placeholder:
    /// The {target} SHOULD describe the entity that the operation is performed against and SHOULD adhere to one of the following values, provided they are accessible:
    /// <br />
    /// db.collection.name SHOULD be used for data manipulation operations or operations on a database collection.
    /// <br />
    /// db.namespace SHOULD be used for operations on a specific database namespace.
    /// <br />
    /// server.address:server.port SHOULD be used for other operations not targeting any specific database(s) or collection(s)
    /// </summary>
    type TargetPlaceholder =
        | CollectionName of CollectionName: string
        | Namespace of Namespace: string
        | ServerAddress of Address: string * Port: int

        member tp.Serialize() =
            match tp with
            | CollectionName collectionName -> collectionName
            | Namespace ns -> failwith ns
            | ServerAddress(address, port) -> $"{address}:{port}"

    type DiagnosticsSettings =
        { Enabled: bool
          IncludeQueries: bool
          IncludeParameters: bool
          Truncation: int
          DefaultMask: string }

    let truncateString (settings: DiagnosticsSettings) (value: string) =
        if settings.Truncation > -1 then
            value.AsSpan(0, settings.Truncation)
        else
            value.AsSpan()

    type FieldDiagnosticSettings = { Sensitive: bool; Mask: string }

    [<RequireQualifiedAccess>]
    module Activities =

        type DiagnosticOverrides =
            { Name: string option
              QuerySummary: string option
              OperationName: string option
              CollectionName: TargetPlaceholder option
              Operation: SqlOperation option }

            static member Default =
                { Name = None
                  QuerySummary = None
                  OperationName = None
                  CollectionName = None
                  Operation = None }


        // TODO move some of this to FOpTel

        let addTag (activity: Activity) (key: string) (value: string) =
            activity
            |> Option.ofObj
            |> Option.iter (fun a -> a.AddTag(key, value) |> ignore)

        let addTags (activity: Activity) (tags: (string * string) list) =
            activity
            |> Option.ofObj
            |> Option.iter (fun a -> tags |> List.iter (fun (key, value) -> a.AddTag(key, value) |> ignore))

        [<RequireQualifiedAccess>]
        type CommonAttribute =
            /// <summary>
            /// The database management system (DBMS) product as identified by the client instrumentation.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-system
            /// </summary>
            | DbSystem of string
            /// <summary>
            /// The name of a collection (table, container) within the database.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-collection-name
            /// </summary>
            | DbCollectionName of string
            /// <summary>
            /// The name of the database, fully qualified within the server address and port.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-namespace
            /// </summary>
            | DbNamespace of string
            /// <summary>
            /// The name of the operation or command being executed.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-operation-name
            /// </summary>
            | DbOperationName of string
            /// <summary>
            /// Database response status code.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-response-status-code
            /// </summary>
            | DbResponseStatusCode
            /// <summary>
            /// Describes a class of error the operation ended with.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/error.md
            /// </summary>
            | ErrorType
            /// <summary>
            /// Server port number.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/server.md#server-port
            /// </summary>
            | ServerPort
            /// <summary>
            /// The number of queries included in a batch operation.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-operation-batch-size
            /// </summary>
            | DbOperationBatchSize of int
            /// <summary>
            /// Low cardinality representation of a database query text.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-query-summary
            /// </summary>
            | DbQuerySummary of string
            /// <summary>
            /// The database query being executed.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-query-text
            /// </summary>
            | DbQueryText of string
            /// <summary>
            /// Name of the database host.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/server.md#server-address
            /// </summary>
            | ServerAddress of string
            /// <summary>
            /// A query parameter used in db.query.text, with `key` being the parameter name, and the attribute value being a string representation of the parameter value.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-query-parameter
            /// </summary>
            /// <param name="Key"></param>
            /// <param name="Value"></param>
            | DbQueryParameter of Key: string * Value: string
            /// <summary>
            /// The name of the connection pool; unique within the instrumented application.
            /// In case the connection pool implementation doesn't provide a name, instrumentation SHOULD use a combination of parameters that would make the name unique,
            /// for example, combining attributes server.address, server.port, and db.namespace,
            /// formatted as server.address:server.port/db.namespace.
            /// Instrumentations that generate connection pool name following different patterns SHOULD document it.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-client-connection-pool-name
            /// </summary>
            | DbClientConnectionPoolName of string
            /// <summary>
            /// The state of a connection in the pool
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-client-connection-state
            /// </summary>
            | DbClientConnectionState of string

        [<RequireQualifiedAccess>]
        module CommonTags =

            [<RequireQualifiedAccess>]
            module Keys =

                [<Literal>]
                let ``db.system`` = "db.system"

                [<Literal>]
                let ``db.collection.name`` = "db.collection.name"

                [<Literal>]
                let ``db.namespace`` = "db.namespace"

                [<Literal>]
                let ``db.operation.name`` = "db.operation.name"

                [<Literal>]
                let ``db.response.status_code`` = "db.response.status_code"

                [<Literal>]
                let ``error.type`` = "error.type"

                [<Literal>]
                let ``server.port`` = "server.port"

                [<Literal>]
                let ``db.operation.batch.size`` = "db.operation.batch.size"

                [<Literal>]
                let ``db.query.summary`` = "db.query.summary"

                [<Literal>]
                let ``db.query.text`` = "db.query.text"

                [<Literal>]
                let ``server.address`` = "server.address"

                let ``db.query.parameter.<key>`` (key: string) = $"db.query.parameter.{key}"

                [<Literal>]
                let ``db.client.connection.pool.name`` = "db.client.connection.pool.name"

                [<Literal>]
                let ``db.client.connection.state`` = "db.client.connection.state"

                [<Literal>]
                let ``error`` = "error"

                [<Literal>]
                let ``exception.escaped`` = "exception.escaped"

                [<Literal>]
                let ``exception.message`` = "exception.message"

                [<Literal>]
                let ``exception.stacktrace`` = "exception.stacktrace"

                [<Literal>]
                let ``exception.type`` = "exception.type"

            /// <summary>
            /// The database management system (DBMS) product as identified by the client instrumentation.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-system
            /// </summary>
            let ``add db.system tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.system``, value)

            /// <summary>
            /// The database management system (DBMS) product as identified by the client instrumentation.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-system
            /// </summary>
            let ``try add db.system tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.system`` value

            /// <summary>
            /// The name of a collection (table, container) within the database.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-collection-name
            /// </summary>
            let ``add db.collection.name tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.collection.name``, value)

            /// <summary>
            /// The name of a collection (table, container) within the database.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-collection-name
            /// </summary>
            let ``try add db.collection.name tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.collection.name`` value

            //// <summary>
            /// The name of the database, fully qualified within the server address and port.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-namespace
            /// </summary>
            let ``add db.namespace tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.namespace``, value)

            /// <summary>
            /// The name of a collection (table, container) within the database.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-collection-name
            /// </summary>
            let ``try add db.namespace tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.namespace`` value

            // <summary>
            /// The name of the operation or command being executed.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-operation-name
            /// </summary>
            let ``add db.operation.name tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.operation.name``, value)

            /// <summary>
            /// The name of the operation or command being executed.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-operation-name
            /// </summary>
            let ``try add db.operation.name tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.operation.name`` value

            /// <summary>
            /// Database response status code.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-response-status-code
            /// </summary>
            let ``add db.response.status_code tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.response.status_code``, value)

            /// <summary>
            /// Database response status code.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-response-status-code
            /// </summary>
            let ``try add db.response.status_code tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.response.status_code`` value

            /// <summary>
            /// Describes a class of error the operation ended with.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/error.md
            /// </summary>
            let ``add error.type tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``error.type``, value)

            /// <summary>
            /// Describes a class of error the operation ended with.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/error.md
            /// </summary>
            let ``try add error.type tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``error.type`` value

            /// <summary>
            /// Server port number.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/server.md#server-port
            /// </summary>
            let ``add server.port tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``server.port``, value)

            /// <summary>
            /// Server port number.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/server.md#server-port
            /// </summary>
            let ``try add server.port tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``server.port`` value

            /// <summary>
            /// The number of queries included in a batch operation.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-operation-batch-size
            /// </summary>
            let ``add db.operation.batch.size tag`` (activity: Activity) (value: int) =
                activity.AddTag(Keys.``db.operation.batch.size``, value)

            /// <summary>
            /// The number of queries included in a batch operation.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-operation-batch-size
            /// </summary>
            let ``try add db.operation.batch.size tag`` (activity: Activity) (value: int) =
                addTag activity Keys.``db.operation.batch.size`` (string value)

            /// <summary>
            /// Low cardinality representation of a database query text.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-query-summary
            /// </summary>
            let ``add db.query.summary tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.query.summary``, value)

            /// <summary>
            /// Low cardinality representation of a database query text.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-query-summary
            /// </summary>
            let ``try add db.query.summary tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.query.summary`` value

            /// <summary>
            /// The database query being executed.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-query-text
            /// </summary>
            let ``add db.query.text tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.query.text``, value)

            /// <summary>
            /// The database query being executed.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/db.md#db-query-text
            /// </summary>
            let ``try add db.query.text tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.query.text`` value

            /// <summary>
            /// Name of the database host.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/server.md#server-address
            /// </summary>
            let ``add server.address tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``server.address``, value)

            /// <summary>
            /// Name of the database host.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/attributes-registry/server.md#server-address
            /// </summary>
            let ``try add server.address tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``server.address`` value

            /// <summary>
            /// A query parameter used in db.query.text, with `key` being the parameter name, and the attribute value being a string representation of the parameter value.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-query-parameter
            /// </summary>
            let ``add db.query.parameter.<key> tag`` (activity: Activity) (key: string) (value: string) =
                activity.AddTag(Keys.``db.query.parameter.<key>`` key, value)

            /// <summary>
            /// A query parameter used in db.query.text, with `key` being the parameter name, and the attribute value being a string representation of the parameter value.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-query-parameter
            /// </summary>
            let ``try add db.query.parameter.<key> tag`` (activity: Activity) (key: string) (value: string) =
                addTag activity (Keys.``db.query.parameter.<key>`` key) value

            /// <summary>
            /// The name of the connection pool; unique within the instrumented application.
            /// In case the connection pool implementation doesn't provide a name, instrumentation SHOULD use a combination of parameters that would make the name unique,
            /// for example, combining attributes server.address, server.port, and db.namespace,
            /// formatted as server.address:server.port/db.namespace.
            /// Instrumentations that generate connection pool name following different patterns SHOULD document it.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-client-connection-pool-name
            /// </summary>
            let ``add db.client.connection.pool.name tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.client.connection.pool.name``, value)

            /// <summary>
            /// The name of the connection pool; unique within the instrumented application.
            /// In case the connection pool implementation doesn't provide a name, instrumentation SHOULD use a combination of parameters that would make the name unique,
            /// for example, combining attributes server.address, server.port, and db.namespace,
            /// formatted as server.address:server.port/db.namespace.
            /// Instrumentations that generate connection pool name following different patterns SHOULD document it.
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-client-connection-pool-name
            /// </summary>
            let ``try add db.client.connection.pool.name tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.client.connection.pool.name`` value

            /// <summary>
            /// The state of a connection in the pool
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-client-connection-state
            /// </summary>
            let ``add db.client.connection.state tag`` (activity: Activity) (value: string) =
                activity.AddTag(Keys.``db.client.connection.state``, value)

            /// <summary>
            /// The state of a connection in the pool
            /// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/sql.md#db-client-connection-state
            /// </summary>
            let ``try add db.client.connection.state tag`` (activity: Activity) (value: string) =
                addTag activity Keys.``db.client.connection.state`` value

        /// <summary>
        /// Add an exception to the activity.
        /// This will not check if the activity is null.
        /// <br />
        /// Reference: https://opentelemetry.io/docs/specs/otel/trace/exceptions/
        /// </summary>
        /// <param name="ex"></param>

        let addException (ex: exn) (activity: Activity) =
            // FUTURE: This can be replaced in .net 9 with built in handling.
            // https://github.com/dotnet/runtime/issues/53641
            // https://github.com/dotnet/runtime/pull/102905
            let tags = ActivityTagsCollection()

            tags.Add(CommonTags.Keys.``exception.message``, ex.Message)
            tags.Add(CommonTags.Keys.``exception.message``, ex.Message)
            // Recommend here: https://opentelemetry.io/docs/specs/semconv/exceptions/exceptions-spans/
            tags.Add(CommonTags.Keys.``exception.stacktrace``, ex.ToString())
            tags.Add(CommonTags.Keys.``exception.type``, ex.GetType().Name)

            activity
                .AddEvent(ActivityEvent("exception", tags = tags))
                .SetStatus(ActivityStatusCode.Error, ex.ToString())

        /// <summary>
        /// Try and add an exception to the activity.
        /// This will check if the activity is null first.
        /// If that as has already been checked call activity.AddException() directly.
        /// <br />
        /// Reference: https://opentelemetry.io/docs/specs/otel/trace/exceptions/
        /// </summary>
        /// <param name="ex"></param>
        let tryAddException (ex: exn) (activity: Activity) =
            activity
            |> Option.ofObj
            |> Option.map (addException ex)
            |> Option.defaultValue activity

        let ifExists (fn: Activity -> unit) (activity: Activity) =
            activity |> Option.ofObj |> Option.iter fn

        /// <summary>
        /// Get the name for a activitiy.
        /// From https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md#name:
        /// Database spans MUST follow the overall guidelines for span names.
        /// <br />
        /// The span name SHOULD be {db.query.summary} if a summary is available.
        /// <br />
        /// If no summary is available, the span name SHOULD be {db.operation.name} {target} provided that a (low-cardinality) db.operation.name is available (see below for the exact definition of the {target} placeholder).
        /// <br />
        /// If a (low-cardinality) db.operation.name is not available, database span names SHOULD default to the {target}.
        /// <br />
        /// If neither {db.operation.name} nor {target} are available, span name SHOULD be {db.system}.
        /// <br />
        /// Semantic conventions for individual database systems MAY specify different span name format.
        /// <br />
        /// The {target} SHOULD describe the entity that the operation is performed against and SHOULD adhere to one of the following values, provided they are accessible:
        /// <br />
        ///     * db.collection.name SHOULD be used for data manipulation operations or operations on a database collection.
        ///  <br />
        ///     * db.namespace SHOULD be used for operations on a specific database namespace.
        ///  <br />
        ///     * server.address:server.port SHOULD be used for other operations not targeting any specific database(s) or collection(s)
        /// <br />
        /// If a corresponding {target} value is not available for a specific operation, the instrumentation SHOULD omit the {target}. For example, for an operation describing SQL query on an anonymous table like SELECT * FROM (SELECT * FROM table) t, span name should be SELECT.
        /// </summary>
        /// <param name="dbSystem"></param>
        /// <param name="operation"></param>
        /// <param name="target"></param>
        /// <param name="diagnosticOverrides"></param>
        let getName
            (dbSystem: string)
            (operation: SqlOperation option)
            (target: TargetPlaceholder option)
            (diagnosticOverrides: DiagnosticOverrides option)
            =
            diagnosticOverrides
            |> Option.bind (fun diagnosticOverrides ->
                diagnosticOverrides.Name
                |> Option.orElseWith (fun _ -> diagnosticOverrides.QuerySummary)
                |> Option.orElseWith (fun _ ->
                    match operation, target with
                    | Some op, Some t -> Some $"{op.Serialize()} {t.Serialize()}"
                    | Some op, None -> op.Serialize() |> Some
                    | None, Some t -> t.Serialize() |> Some
                    | None, None -> None))
            |> Option.defaultValue dbSystem


        let createTagCollection (keyValues: (string * obj) seq) =
            let tags = ActivityTagsCollection()
            keyValues |> Seq.iter tags.Add
            tags
