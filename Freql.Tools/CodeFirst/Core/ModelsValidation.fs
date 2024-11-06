namespace Freql.Tools.CodeFirst.Core

open Freql.Core
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

        module ``BlobField type not supported`` =

            let id = "v1_0003"

            let rule =
                { Id = id
                  Name = "BlobField type not supported"
                  Information = [ "Version 1 of CodeFirst support does not support BlobField types." ]
                  Handler =
                    fun record allRecords ->

                        record.Fields
                        |> List.fold
                            (fun result field ->
                                match result, field.Type with
                                | Error _, _ -> result
                                | Ok _, SupportedType supportedType ->
                                    let rec checkSupportedType (st: SupportedType) =
                                        match st with
                                        | SupportedType.Blob ->
                                            { Id = id
                                              Message = "BlobField type not supported"
                                              Record = record }
                                            |> ValidatorFailureResult.Error
                                            |> Error
                                        | SupportedType.Option ist -> checkSupportedType ist
                                        | _ -> Ok()

                                    checkSupportedType supportedType
                                | Ok _, Record _ -> result
                                | Ok _, Collection _ -> result)
                            (Ok())
                  Supersedes = None }

        module ``Nested options not supported`` =

            let id = "v1_0004"

            let rule =
                { Id = id
                  Name = "Nested options not supported"
                  Information = [ "Version 1 of CodeFirst support does not support nested options." ]
                  Handler =
                    fun record allRecords ->

                        record.Fields
                        |> List.fold
                            (fun result field ->
                                match result, field.Type with
                                | Error _, _ -> result
                                | Ok _, SupportedType supportedType ->
                                    let rec checkSupportedType (nested: bool) (st: SupportedType) =
                                        match st with
                                        | SupportedType.Option ist ->
                                            match nested with
                                            | true ->
                                                { Id = id
                                                  Message = "Nested options not supported"
                                                  Record = record }
                                                |> ValidatorFailureResult.Error
                                                |> Error
                                            | false -> checkSupportedType true ist
                                        | _ -> Ok()

                                    checkSupportedType false supportedType
                                | Ok _, Record _ -> result
                                | Ok _, Collection _ -> result)
                            (Ok())
                  Supersedes = None }

        let allRules =
            [ ``Model must have a defined primary key``.rule
              ``Foreign keys require explicit fields``.rule
              ``BlobField type not supported``.rule
              ``Nested options not supported``.rule ]

    let validate (rules: ValidatorRule list) (records: RecordInformation list) =
        records
        |> List.collect (fun record -> rules |> List.map (fun rule -> rule.Handler record records))
        |> Results.partition
        |> fun (_, errors) ->
            // TODO check for supersedes
            match errors.IsEmpty with
            | true -> Ok()
            | false -> Error errors
