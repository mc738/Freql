namespace Freql.Tools.CodeGeneration

open System.IO
open System.Text.Json

module Configuration =

    open System.Text.Json.Serialization

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
            with exn ->
                Error exn.Message

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
