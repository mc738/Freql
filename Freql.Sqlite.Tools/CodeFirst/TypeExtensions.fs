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
        (scalarFields: ScalarField list)
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
              scalarFields
              |> List.mapi (fun i scalarField ->
                  let normalizedFieldName =
                      match ctx.NameNormalizationMode with
                      | PascalCase -> scalarField.Field.Name.ToPascalCase()
                      | SnakeCase -> scalarField.Field.Name.ToSnakeCase()

                  let primaryKey =
                      match scalarField.Field.PrimaryKey with
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
                      match i = scalarFields.Length - 1 with
                      | true -> ""
                      | false -> ","

                  $"{normalizedFieldName} {supportedTypeToSqliteType scalarField.SupportedType}{primaryKey}{eol}")
              |> List.map (indent 2)
          ")" |> indent1
          "\"\"\"" |> indent1 ]

    let generateInsertSql
        (ctx: CodeGeneratorContext)
        (record: RecordInformation)
        (scalarFields: ScalarField list)
        =
        let normalizedName =
            match ctx.NameNormalizationMode with
            | PascalCase -> record.Name.ToPascalCase()
            | SnakeCase -> record.Name.ToSnakeCase()

        let (fields, parameters) =
            scalarFields
            |> List.mapi (fun i scalarField ->
                let normalizedFieldName =
                    match ctx.NameNormalizationMode with
                    | PascalCase -> scalarField.Field.Name.ToPascalCase()
                    | SnakeCase -> scalarField.Field.Name.ToSnakeCase()

                let eol =
                    match i = scalarFields.Length - 1 with
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
          "\"\"\"" |> indent1 ]

    let generateUpdateSql
        (ctx: CodeGeneratorContext)
        (record: RecordInformation)
        (scalarFields: ScalarField list)
        =
        let normalizedName =
            match ctx.NameNormalizationMode with
            | PascalCase -> record.Name.ToPascalCase()
            | SnakeCase -> record.Name.ToSnakeCase()

        let parameters =
            scalarFields
            |> List.mapi (fun i scalarField ->
                let normalizedFieldName =
                    match ctx.NameNormalizationMode with
                    | PascalCase -> scalarField.Field.Name.ToPascalCase()
                    | SnakeCase -> scalarField.Field.Name.ToSnakeCase()

                $"{normalizedFieldName} = @{i}")
            |> String.concat ", "

        let whereClause =
            match record.GetActualPrimaryKeyFields() with
            | [] -> failwith "No primary key"
            | [ pk ] ->
                let normalizedFieldName =
                    match ctx.NameNormalizationMode with
                    | PascalCase -> pk.Name.ToPascalCase()
                    | SnakeCase -> pk.Name.ToSnakeCase()

                $"{normalizedFieldName} = @{scalarFields.Length}"
            | pks ->
                let clause =
                    pks
                    |> List.mapi (fun i pk ->
                        let normalizedFieldName =
                            match ctx.NameNormalizationMode with
                            | PascalCase -> pk.Name.ToPascalCase()
                            | SnakeCase -> pk.Name.ToSnakeCase()

                        $"{normalizedFieldName} = @{scalarFields.Length + i}")
                    |> String.concat ", "

                $"({clause})"

        [ "member this.UpdateRecordsSql(updates: UpdateFieldOperation list) ="
          "\"\"\"" |> indent1
          $"UPDATE {normalizedName}" |> indent1
          $"SET {parameters}" |> indent1
          $"WHERE {whereClause}" |> indent1
          "\"\"\"" |> indent1 ]
        
    let generateDeleteSql
        (ctx: CodeGeneratorContext)
        (record: RecordInformation)
        (scalarFields: ScalarField list)
        =
        let normalizedName =
            match ctx.NameNormalizationMode with
            | PascalCase -> record.Name.ToPascalCase()
            | SnakeCase -> record.Name.ToSnakeCase()

        let whereClause =
            match record.GetActualPrimaryKeyFields() with
            | [] -> failwith "No primary key"
            | [ pk ] ->
                let normalizedFieldName =
                    match ctx.NameNormalizationMode with
                    | PascalCase -> pk.Name.ToPascalCase()
                    | SnakeCase -> pk.Name.ToSnakeCase()

                $"{normalizedFieldName} = @0"
            | pks ->
                let clause =
                    pks
                    |> List.mapi (fun i pk ->
                        let normalizedFieldName =
                            match ctx.NameNormalizationMode with
                            | PascalCase -> pk.Name.ToPascalCase()
                            | SnakeCase -> pk.Name.ToSnakeCase()

                        $"{normalizedFieldName} = @{i}")
                    |> String.concat ", "

                $"({clause})"

        [ "member this.DeleteRecordsSql() ="
          "\"\"\"" |> indent1
          $"DELETE {normalizedName}" |> indent1
          $"WHERE {whereClause}" |> indent1
          "\"\"\"" |> indent1 ]

    let generate (ctx: CodeGeneratorContext) (record: RecordInformation) =
        let scalarFields = record.GetScalarFields()

        [ yield! generateSelectSql ctx record scalarFields
          ""
          yield! generateInsertSql ctx record scalarFields
          ""
          yield! generateUpdateSql ctx record scalarFields
          ""
          yield! generateDeleteSql ctx record scalarFields

          ]
