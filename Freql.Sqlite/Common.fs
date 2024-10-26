namespace Freql.Sqlite

open System
open System.Diagnostics
open Microsoft.Data.Sqlite

[<AutoOpen>]
module Common =

    [<RequireQualifiedAccess>]
    type SQLiteFailure =
        | SQLiteException of SqliteException
        | GeneralException of Exception

        static member FromException(ex: exn) =
            match ex with
            | :? SqliteException as ex -> SQLiteFailure.SQLiteException ex
            | ex -> SQLiteFailure.GeneralException ex