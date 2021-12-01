namespace Freql.Tools

open System
open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

module CodeGeneration =


    module Configuration =
        
        type DatabaseConfiguration =
            { [<JsonPropertyName("name")>]
              Name: string
              [<JsonPropertyName("type")>]
              Type: string
              [<JsonPropertyName("connectionString")>]
              ConnectionString: string
              [<JsonPropertyName("generatorProfiles")>]
              GeneratorProfiles: GeneratorProfile seq }

            static member TryLoad(path: string) =
                try
                    File.ReadAllText path
                    |> JsonSerializer.Deserialize<DatabaseConfiguration seq>
                    |> Ok
                with
                | exn -> Error exn.Message

        and GeneratorProfile =
            { [<JsonPropertyName("name")>]
              Name: string
              [<JsonPropertyName("outputPath")>]
              OutputPath: string
              [<JsonPropertyName("namespace")>]
              Namespace: string
              [<JsonPropertyName("moduleName")>]
              ModuleName: string
              [<JsonPropertyName("includeJsonAttributes")>]
              IncludeJsonAttributes: bool
              [<JsonPropertyName("nameSuffix")>]
              NameSuffix: string
              [<JsonPropertyName("typeReplacements")>]
              TypeReplacements: TypeReplacementConfiguration seq
              [<JsonPropertyName("tableNameReplacements")>]
              TableNameReplacements: TableNameReplacement seq }

        and TypeReplacementConfiguration =
            { [<JsonPropertyName("matchValue")>]
              MatchValue: string
              [<JsonPropertyName("matchType")>]
              MatchType: string
              [<JsonPropertyName("replacementValue")>]
              ReplacementValue: string
              [<JsonPropertyName("replacementInitValue")>]
              ReplacementInitValue: string }

        and TableNameReplacement =
            { [<JsonPropertyName("name")>]
              Name: string
              [<JsonPropertyName("replacementName")>]
              ReplacementName: string }

    open System.Text.RegularExpressions
    open Freql.Core.Utils

    [<RequireQualifiedAccess>]
    type MatchType =
        | Regex of string
        | String of string

        member mt.Test(value: string) =
            match mt with
            | Regex pattern -> Regex.IsMatch(value, pattern)
            | String str -> String.Equals(value, str, StringComparison.Ordinal)

    type TypeReplacement =
        { Match: MatchType
          ReplacementType: string
          Initialization: string option }

        static member Create(config: Configuration.TypeReplacementConfiguration) =
            { Match =
                  match config.MatchType with
                  | "regex" -> MatchType.Regex config.MatchValue
                  | _ -> MatchType.String config.MatchValue
              ReplacementType = config.ReplacementValue
              Initialization = Some config.ReplacementInitValue }



        member tr.Attempt(name: string, typeString: string) =
            match tr.Match.Test name with
            | true -> tr.ReplacementType
            | false -> typeString

        member tr.AttemptInitReplacement(name: string, initValue: string) =
            match tr.Initialization, tr.Match.Test name with
            | Some init, true -> init
            | _ -> initValue

    type GeneratorSettings<'Col> =
        { Imports: string list
          IncludeJsonAttributes: bool
          TypeReplacements: TypeReplacement list
          TypeHandler: TypeReplacement list -> 'Col -> string
          TypeInitHandler: TypeReplacement list -> 'Col -> string
          NameHandler: 'Col -> string }

    type TableDetails<'Col> =
        { Name: string
          Sql: string
          Columns: 'Col list }

    let createRecord<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (table: TableDetails<'Col>)
        =
        let fields =
            table.Columns
            |> List.mapi
                (fun i cd ->
                    let name =
                        settings.NameHandler cd
                        |> fun n -> n.ToPascalCase()
                        |> fun n ->
                            match settings.IncludeJsonAttributes with
                            | true -> $"[<JsonPropertyName(\"{n.ToCamelCase()}\")>] {n}"
                            | false -> n
                        |> fun n -> $"{n}: {settings.TypeHandler settings.TypeReplacements cd}"

                    match i with
                    | 0 -> $"    {{ {name}"
                    | _ when i = table.Columns.Length - 1 -> $"      {name} }}"
                    | _ -> $"      {name}")

        let blank =
            table.Columns
            |> List.mapi
                (fun i cd ->
                    let name =
                        settings.NameHandler cd
                        |> fun n -> n.ToPascalCase()

                    let content =
                        $"{name} = {settings.TypeInitHandler settings.TypeReplacements cd}"

                    match i with
                    | 0 -> $"        {{ {content}"
                    | _ when i = table.Columns.Length - 1 -> $"          {content} }}"
                    | _ -> $"          {content}")
            |> fun r -> [ "    static member Blank() =" ] @ r

        let createSql =
            [ "    static member CreateTableSql() = \"\"\""
              $"    {table.Sql}"
              "    \"\"\"" ]

        let selectFields =
            table.Columns
            |> List.map (fun cd -> $"          {settings.NameHandler cd}")
            |> String.concat $",{Environment.NewLine}    "

        let selectSql =
            [ "    static member SelectSql() = \"\"\""
              $"    SELECT"
              $"{selectFields}"
              $"    FROM {table.Name}"
              "    \"\"\"" ]

        match fields.Length with
        | 0 -> []
        //| 1 -> [ $"type {table.Name.ToPascalCase()} = {fields.[0].Trim()} }}" ]
        | _ ->
            let name =
                match profile.TableNameReplacements
                      |> List.ofSeq
                      |> List.tryFind (fun tnr -> String.Equals(tnr.Name, table.Name, StringComparison.Ordinal)) with
                | Some tnr -> $"{tnr.ReplacementName.ToPascalCase()}{profile.NameSuffix}"
                | None -> $"{table.Name.ToPascalCase()}{profile.NameSuffix}"

            [ $"type {name} ="
              yield! fields
              ""
              yield! blank
              ""
              yield! createSql
              ""
              yield! selectSql ]

    let indent value (text: string) = $"{String(' ', value * 4)}{text}"

    let indent1 text = indent 1 text

    let createRecords<'Col>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'Col>)
        (tables: TableDetails<'Col> list)
        =

        // Create the core record.
        let records =
            tables
            |> List.map (fun t -> createRecord profile settings t @ [ "" ])
            |> List.concat
            |> List.map indent1

        [ $"namespace {profile.Namespace}"
          ""
          "open System"
          if settings.IncludeJsonAttributes then
              "open System.Text.Json.Serialization"
          yield!
              settings.Imports
              |> List.map (fun i -> $"open {i}")
          ""
          $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Sqlite.Tools."
          $"module {profile.ModuleName} =" ]
        @ records
        |> String.concat Environment.NewLine
