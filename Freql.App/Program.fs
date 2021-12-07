open System
open System.ComponentModel.DataAnnotations
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Freql.MySql
open Freql.MySql.Tools
open Freql.Tools
open Freql.Tools.CodeGeneration.Configuration
open Microsoft.Data.Sqlite

[<RequireQualifiedAccess>]
type DatabaseType =
    | MySql
    | Sqlite
    | SqlServer

    static member FromString(str: string) =
        match str.ToLower() with
        | "mysql" -> Ok DatabaseType.MySql
        | "sqlite" -> Ok DatabaseType.Sqlite
        | "sqlserver" -> Ok DatabaseType.SqlServer
        | _ -> Error $"Unknown type: `{str}`"

type GenerateOptions =
    { ConfigurationPath: string
      DatabaseName: string
      Profile: string }

    static member Blank() =
        { ConfigurationPath = String.Empty
          DatabaseName = String.Empty
          Profile = String.Empty }

    static member TryParse(argv: string list) =
        let rec parse args (options: GenerateOptions) =
            match args with
            | "-c" :: rest
            | "--config" :: rest ->
                match rest with
                | configPath :: otherArgs ->
                    let newOptions =
                        { options with
                              ConfigurationPath = configPath }

                    parse otherArgs newOptions
                | [] -> failwith "Missing connection name."
            | "-d" :: rest
            | "-db" :: rest
            | "--database" :: rest ->
                match rest with
                | dbName :: otherArgs ->
                    let newOptions = { options with DatabaseName = dbName }
                    parse otherArgs newOptions
                | [] -> failwith "Missing database name."
            | "-p" :: rest
            | "--profile" :: rest ->
                match rest with
                | profile :: otherArgs ->
                    let newOptions = { options with Profile = profile }
                    parse otherArgs newOptions
                | [] -> failwith "Missing s3 config path"
            | option :: rest ->
                printfn $"Unrecognized option `{option}`. Skipping."
                parse rest options
            | [] -> options

        try
            parse argv (GenerateOptions.Blank())
            |> fun o ->
                if String.IsNullOrWhiteSpace o.ConfigurationPath then
                    failwith "Missing config parameter. Example: `-c path/to/config`"

                if String.IsNullOrWhiteSpace o.DatabaseName then
                    failwith "Missing database parameter. Example: `-d database_name`"

                if String.IsNullOrWhiteSpace o.Profile then
                    failwith "Missing profile parameter. Example: -p `my-profile`"

                Ok o
        with
        | exn -> Error exn.Message

type Action = GenerateCode of GenerateOptions

let save (path: string) (data: string) =
    try
        File.WriteAllText(path, data) |> Ok
    with
    | exn -> Error exn.Message

let getOptions (argv: string array) =
    argv
    |> List.ofArray
    |> List.tail // discard the app name.
    |> fun argv ->
        match argv.IsEmpty with
        | true -> Error "Missing action."
        | false ->
            match argv.Head with
            | a when String.Equals(a, "gen", StringComparison.OrdinalIgnoreCase) ->
                match GenerateOptions.TryParse argv.Tail with
                | Ok options -> Action.GenerateCode options |> Ok
                | Error e -> Error e
            | a -> Error $"Unknown action: `{a}`."

[<RequireQualifiedAccess>]
module MySqlActions =

    let generate (databaseName: string) (profile: GeneratorProfile) (context: MySqlContext) =
        MySqlMetaData.get databaseName context
        |> MySqlCodeGeneration.generate profile

module GenerationActions =

    let handle (options: GenerateOptions) =

        match DatabaseConfiguration.TryLoad options.ConfigurationPath with
        | Ok config ->
            let dbcr =
                config
                |> List.ofSeq
                |> List.tryFind (fun c -> String.Equals(c.Name, options.DatabaseName, StringComparison.Ordinal))

            match dbcr with
            | Some dbc ->
                let profile =
                    dbc.GeneratorProfiles
                    |> List.ofSeq
                    |> List.tryFind (fun gp -> String.Equals(gp.Name, options.Profile, StringComparison.Ordinal))

                match DatabaseType.FromString dbc.Type, profile with
                | Ok t, Some p ->
                    match t with
                    | DatabaseType.MySql ->
                        try
                            printfn $"{dbc.ConnectionString}"
                            let context =
                                MySqlContext.Connect dbc.ConnectionString

                            MySqlActions.generate dbc.Name p context
                            |> save p.OutputPath
                        with
                        | exn -> Error exn.Message
                    | DatabaseType.Sqlite -> Error "Sqlite code gen not implement yet."
                    | DatabaseType.SqlServer -> Error "SqlServer code gen not implement yet."
                | Error e, _ -> Error e
                | _, None -> Error $"Profile not found: `{options.Profile}`"
            | None -> Error $"Database not found: {options.DatabaseName}"

        | Error e -> Error e


let cprintfn color str =
    Console.ForegroundColor <- color
    printfn $"{str}"
    Console.ResetColor()

let printError str = cprintfn ConsoleColor.Red str

let printSuccess str = cprintfn ConsoleColor.Green str

match Environment.GetCommandLineArgs() |> getOptions with
| Ok a ->
    match a with
    | Action.GenerateCode go ->
        match GenerationActions.handle go with
        | Ok _ -> printSuccess "Code generation complete."
        | Error e -> printError e
| Error e -> printError e
