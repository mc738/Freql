namespace Freql.Sqlite.Tools.Core

open System

module Exceptions =

    type SqliteCodeGenerationException(message: string, innerException: Exception) =

        inherit Exception(message, innerException)
