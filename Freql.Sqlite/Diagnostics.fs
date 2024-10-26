namespace Freql.Sqlite

open System
open System.Data
open Microsoft.Data.Sqlite

module Diagnostics =

    open System.Diagnostics
    open Freql.Core.Diagnostics

    [<Literal>]
    let ``db.system`` = "sqlite"

    type ActivityParameters =
        { OperationName: string
          ConnectionState: ConnectionState
          QuerySummary: string option
          QueryText: string option
          SqlOperation: SqlOperation option
          CollectName: TargetPlaceholder option
          Overrides: Activities.DiagnosticOverrides option }

    module Internal =

        let setupActivity (parameters: ActivityParameters) (activity: Activity) =

            Activities.CommonTags.``add db.system tag`` activity ``db.system`` |> ignore

            Activities.CommonTags.``add db.client.connection.state tag``
                activity
                (Enum.GetName parameters.ConnectionState)
            |> ignore

            parameters.Overrides
            |> Option.bind (fun overrides -> overrides.QuerySummary)
            |> Option.orElse parameters.QuerySummary
            |> Option.iter (Activities.CommonTags.``add db.query.summary tag`` activity >> ignore)

            parameters.QueryText
            |> Option.iter (Activities.CommonTags.``add db.query.text tag`` activity >> ignore)

            parameters.SqlOperation
            |> Option.iter (fun op ->
                Activities.CommonTags.``add db.operation.name tag`` activity (op.Serialize())
                |> ignore)

            parameters.CollectName
            |> Option.iter (fun cn ->
                Activities.CommonTags.``add db.collection.name tag`` activity (cn.Serialize())
                |> ignore)

            activity

        let addStartEvent (parameters: ActivityParameters) (activity: Activity) =
            let startName = $"freql.sqlite.{parameters.OperationName}.start"

            activity.AddEvent(
                ActivityEvent(startName, tags = (seq { "event.name", box startName } |> Activities.createTagCollection))
            )

        let addEndEvent (parameters: ActivityParameters) (activity: Activity) =
            let endName = $"freql.sqlite.{parameters.OperationName}.end"

            activity
                .AddEvent(
                    ActivityEvent(endName, tags = (seq { "event.name", box endName } |> Activities.createTagCollection))
                )
                .SetStatus(ActivityStatusCode.Ok)

        let addSqliteException (ex: SqliteException) (activity: Activity) =
            Activities.CommonTags.``add error.type tag`` activity (string ex.SqliteErrorCode)
            |> Activities.addException ex

        let addException (ex: Exception) (activity: Activity) = Activities.addException ex activity

    let getName operation target diagnosticOverrides =
        Activities.getName ``db.system`` operation target diagnosticOverrides

    let wrapInActivity<'TResult> (fn: unit -> 'TResult) (parameters: ActivityParameters) (activity: Activity) =
        match activity |> Option.ofObj with
        | Some activity ->
            try
                Internal.setupActivity parameters activity
                |> Internal.addStartEvent parameters
                |> ignore

                let result = fn ()

                Internal.addEndEvent parameters activity |> ignore

                result
            with
            | :? SqliteException as ex ->
                Internal.addSqliteException ex activity |> ignore
                reraise ()
            | ex ->
                Internal.addException ex activity |> ignore
                reraise ()
        | None -> fn ()

    let wrapTryInActivity<'TResult> (fn: unit -> 'TResult) (parameter: ActivityParameters)  (activity: Activity) =
        match activity |> Option.ofObj with
        | Some activity ->
            try
                Internal.setupActivity parameter activity
                |> Internal.addStartEvent parameter
                |> ignore

                let result = fn ()

                Internal.addEndEvent parameter activity |> ignore

                Ok result
            with
            | :? SqliteException as ex ->
                Internal.addSqliteException ex activity |> ignore
                SQLiteFailure.SQLiteException ex |> Error
            | ex ->
                Internal.addException ex activity |> ignore
                SQLiteFailure.GeneralException ex |> Error
        | None ->
            try
                fn () |> Ok
            with ex ->
                SQLiteFailure.FromException ex |> Error
