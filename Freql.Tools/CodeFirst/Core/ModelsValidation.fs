namespace Freql.Tools.CodeFirst.Core

open Freql.Core.Utils

module ModelsValidation =

    module Rules =

        type ValidatorRule =
            { Id: string
              Name: string
              Information: string list
              Handler: RecordInformation -> RecordInformation list -> Result<unit, ValidatorFailureResult>
              Supersedes: string option }

        and [<RequireQualifiedAccess>] ValidatorFailureResult =
            | Warning of ValidationFailureDetails
            | Error of ValidationFailureDetails

        and ValidationFailureDetails =
            { Id: string
              Message: string
              Record: RecordInformation }

    open Rules

    module V1Rules =

        open Rules

        module ``Model must have a defined primary key`` =

            let id = "v1_0001"

            let rule =
                { Id = id
                  Name = "Model must have a defined primary key"
                  Information =
                    [ "Version 1 of CodeFirst support requires models to have defined primary key."
                      "Either add a `[<PrimaryKey>]` attribute to a existing field or a field with a name that meets a primary key convention, for example `Id`." ]
                  Handler =
                    fun record allRecords ->
                        match record.HasDefinedPrimaryKey() with
                        | true -> Result.Ok()
                        | false ->
                            { Id = id
                              Message = "Model must have a defined primary key"
                              Record = record }
                            |> ValidatorFailureResult.Error
                            |> Result.Error
                  Supersedes = None }

        module ``Foreign keys require explicit fields`` =

            let id = "v1_0002"

            let rule =
                { Id = id
                  Name = "Foreign keys require explicit fields"
                  Information =
                    [ "Version 1 of CodeFirst support requires explicit foreign key fields."
                      "Either add a `[<ForeignKey>]` attribute to a existing field." ]
                  Handler =
                    fun record allRecords ->

                        let virtualForeignKeys =
                            record.VirtualFields
                            |> List.choose (fun vf ->
                                match vf with
                                | ForeignKey fk -> Some fk
                                | _ -> None)

                        match virtualForeignKeys.IsEmpty with
                        | true -> Result.Ok()
                        | false ->
                            { Id = id
                              Message = "Foreign keys require explicit fields"
                              Record = record }
                            |> ValidatorFailureResult.Error
                            |> Result.Error
                  Supersedes = None }

        let allRules =
            [ ``Model must have a defined primary key``.rule
              ``Foreign keys require explicit fields``.rule ]

    let validate (rules: ValidatorRule list) (records: RecordInformation list) =
        records
        |> List.collect (fun record -> rules |> List.map (fun rule -> rule.Handler record records))
        |> Results.partition
        |> fun (_, errors) ->
            // TODO check for supersedes
            match errors.IsEmpty with
            | true -> Ok ()
            | false -> Error errors
            