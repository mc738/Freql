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

        let addTag (activity: Activity) (key:string) (value:string) =
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
            
            
        
        let ``add db.query.text tag`` (activity: Activity) (query:string) =
            addTag activity "db.query.text" query
        
        /// <summary>
        /// From https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/database-spans.md:
        /// The name of a collection (table, container) within the database. [2]
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="query"></param>
        let ``add db.collection.name tag`` (activity: Activity) (query:string) =
            addTag activity "db.query.text" query
        
        let ``add db.query.summary tag`` (activity: Activity) (summary:string) =
            addTag activity "db.query.summary" summary
