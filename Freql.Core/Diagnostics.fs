namespace Freql.Core

open System
open System.Diagnostics

module Diagnostics =

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

                let ``db.system`` = "db.system"

                let ``db.collection.name`` = "db.collection.name"

                let ``db.namespace`` = "db.namespace"

                let ``db.operation.name`` = "db.operation.name"

                let ``db.response.status_code`` = "db.response.status_code"

                let ``error.type`` = "error.type"

                let ``server.port`` = "server.port"

                let ``db.operation.batch.size`` = "db.operation.batch.size"

                let ``db.query.summary`` = "db.query.summary"

                let ``db.query.text`` = "db.query.text"

                let ``server.address`` = "server.address"

                let ``db.query.parameter.<key>`` (key: string) = $"db.query.parameter.{key}"

                let ``db.client.connection.pool.name`` = "db.client.connection.pool.name"

                let ``db.client.connection.state`` = "db.client.connection.state"

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
            

            
        
            
            




        let ``add db.query.text tag`` (activity: Activity) (query: string) = addTag activity "db.query.text" query

        /// <summary>
        /// From https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md:
        /// The name of a collection (table, container) within the database. [2]
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="query"></param>
        let ``add db.collection.name tag`` (activity: Activity) (query: string) = addTag activity "db.query.text" query

        let ``add db.query.summary tag`` (activity: Activity) (summary: string) =
            addTag activity "db.query.summary" summary
