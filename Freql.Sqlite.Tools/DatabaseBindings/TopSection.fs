namespace Freql.Sqlite.Tools.DatabaseBindings

open Freql.Sqlite.Tools.Core.SqliteMetadata

[<RequireQualifiedAccess>]
module TopSection =

    open Freql.Tools.DatabaseBindings
    
    let utilsModule =
        [ "module private Utils ="
          ""
          "    open System.Text.RegularExpressions"
          ""
          "    let updateCheckIfExists (update: bool) (name: string) (value: string) ="
          "        match update with"
          "        | false -> value"
          "        | true ->"
          "            let regex = Regex($\"CREATE {name}\")"
          ""
          "            regex.Replace(value, $\"CREATE {name} IF NOT EXISTS\", 1)" ]

    let generate (ctx: GeneratorContext<SqliteTableDefinition, SqliteColumnDefinition>) = [ yield! utilsModule ] |> Some
