namespace Freql.Sqlite.Tools.CodeFirst

open Freql.Core

[<RequireQualifiedAccess>]
module TypeExtensions =

    open Freql.Core.Utils
    open Freql.Sqlite
    open Freql.Tools.CodeGeneration.Utils
    open Freql.Tools.CodeFirst.Core
    open Freql.Tools.CodeFirst.CodeGeneration

    let generateSelectSql
        (ctx: CodeGeneratorContext)
        (record: RecordInformation)
        (filteredFields: (FieldInformation * SupportedType) list)
        =
        let normalizedName =
            match ctx.NameNormalizationMode with
            | PascalCase -> record.Name.ToPascalCase()
            | SnakeCase -> record.Name.ToSnakeCase()

        let hasForeignKeys = record.VirtualFields

        [ "member this.CreateTableSql() ="
          "\"\"\"" |> indent1
          $"CREATE TABLE IF NOT EXISTS {normalizedName} (" |> indent1
          yield!
              filteredFields
              |> List.mapi (fun i (field, supportedType) ->
                  let normalizedFieldName =
                      match ctx.NameNormalizationMode with
                      | PascalCase -> field.Name.ToPascalCase()
                      | SnakeCase -> field.Name.ToSnakeCase()

                  let primaryKey =
                      match field.PrimaryKey with
                      | None -> ""
                      | Some PrimaryKeyDefinitionType.Attribute -> " PRIMARY KEY"
                      | Some(PrimaryKeyDefinitionType.Convention priority) ->
                          match
                              record.HasAttributePrimaryKey(), record.GetTopConventionPrimaryKeyField() = priority
                          with
                          | false, true -> " PRIMARY KEY"
                          | true, _
                          | false, false -> ""

                  let eol =
                      match i = filteredFields.Length - 1 with
                      | true -> ""
                      | false -> ","

                  $"{normalizedFieldName} {supportedTypeToSqliteType supportedType}{primaryKey}{eol}")
              |> List.map (indent 2)
          ")" |> indent1
          "\"\"\"" ]

    let generateInsertSql
        (ctx: CodeGeneratorContext)
        (record: RecordInformation)
        (filteredFields: (FieldInformation * SupportedType) list)
        =
        let normalizedName =
            match ctx.NameNormalizationMode with
            | PascalCase -> record.Name.ToPascalCase()
            | SnakeCase -> record.Name.ToSnakeCase()

        let (fields, parameters) =
            filteredFields
            |> List.mapi (fun i (field, supportedType) ->
                let normalizedFieldName =
                    match ctx.NameNormalizationMode with
                    | PascalCase -> field.Name.ToPascalCase()
                    | SnakeCase -> field.Name.ToSnakeCase()

                let eol =
                    match i = filteredFields.Length - 1 with
                    | true -> ""
                    | false -> ", "

                $"{normalizedFieldName}{eol}", $"@{i}{eol}")
            |> List.unzip
            |> fun (fields, parameters) -> fields |> String.concat "", parameters |> String.concat ""
        //|> List.map (indent 2)

        [ "member this.InsertRecordSql() ="
          "\"\"\"" |> indent1
          $"INSERT INTO {normalizedName} ({fields})" |> indent1
          $"VALUES ({parameters})" |> indent1
          "\"\"\"" ]

    let generate (ctx: CodeGeneratorContext) (record: RecordInformation) =
        let filteredFields =
            record.Fields
            |> List.choose (fun field ->
                match field.Type with
                | SupportedType supportedType -> Some(field, supportedType)
                | Record ``type`` ->
                    // Do nothing, this will be a foreign key in field ``type``
                    None
                | Collection collectionType ->
                    // Do nothing, this will be a foreign key in field ``type``
                    None)


        [ yield! generateSelectSql ctx record filteredFields
          ""
          yield! generateInsertSql ctx record filteredFields

          ]
