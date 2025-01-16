namespace Freql.Sqlite

open System
open Freql.Core
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

        member failure.GetMessage() =
            match failure with
            | SQLiteException sqliteException -> sqliteException.Message
            | GeneralException ``exception`` -> ``exception``.Message

    let supportedTypeToSqliteType (supportedType: SupportedType) =
        match supportedType with
        | SupportedType.Boolean -> "INTEGER NOT NULL"
        | SupportedType.Byte -> "INTEGER NOT NULL"
        | SupportedType.SByte -> "INTEGER NOT NULL"
        | SupportedType.Int -> "INTEGER NOT NULL"
        | SupportedType.UInt -> "INTEGER NOT NULL"
        | SupportedType.Short -> "INTEGER NOT NULL"
        | SupportedType.UShort -> "INTEGER NOT NULL"
        | SupportedType.Long -> "INTEGER NOT NULL"
        | SupportedType.ULong -> "INTEGER NOT NULL"
        | SupportedType.Double -> "REAL NOT NULL"
        | SupportedType.Single -> "REAL NOT NULL"
        | SupportedType.Decimal -> "REAL NOT NULL"
        | SupportedType.Char -> "TEXT NOT NULL"
        | SupportedType.String -> "TEXT NOT NULL"
        | SupportedType.DateTime -> "TEXT NOT NULL"
        | SupportedType.TimeSpan -> "TEXT NOT NULL"
        | SupportedType.Guid -> "TEXT NOT NULL"
        | SupportedType.Blob -> "BLOB NOT NULL"
        | SupportedType.Option ost ->
            match ost with
            | SupportedType.Boolean -> "INTEGER"
            | SupportedType.Byte -> "INTEGER"
            | SupportedType.SByte -> "INTEGER"
            | SupportedType.Int -> "INTEGER"
            | SupportedType.UInt -> "INTEGER"
            | SupportedType.Short -> "INTEGER"
            | SupportedType.UShort -> "INTEGER"
            | SupportedType.Long -> "INTEGER"
            | SupportedType.ULong -> "INTEGER"
            | SupportedType.Double -> "REAL"
            | SupportedType.Single -> "REAL"
            | SupportedType.Decimal -> "REAL"
            | SupportedType.Char -> "TEXT"
            | SupportedType.String -> "TEXT"
            | SupportedType.DateTime -> "TEXT"
            | SupportedType.TimeSpan -> "TEXT"
            | SupportedType.Guid -> "TEXT"
            | SupportedType.Blob -> "BLOB"
            | SupportedType.Option _ -> failwith "Nested options not supported."
