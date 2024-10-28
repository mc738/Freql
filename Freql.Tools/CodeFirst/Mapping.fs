namespace Freql.Tools.CodeFirst

open Freql.Tools.CodeFirst
open Microsoft.FSharp.Core
open Microsoft.FSharp.Reflection

module Mapping =

    open System
    open Freql.Core

    [<RequireQualifiedAccess>]
    type MappingFailure =
        | InvalidType of Message: string
        | FieldErrors of FieldMappingError list

    and [<RequireQualifiedAccess>] FieldMappingError = InvalidType of Name: string * Message: string

    let partitionResults (results: Result<'T, 'U> seq) =
        results
        |> Seq.fold
            (fun (successes, errors) result ->
                match result with
                | Ok r -> r :: successes, errors
                | Error e -> successes, e :: errors)
            ([], [])
        |> fun (successes, errors) -> successes |> List.rev, errors |> List.rev

    let mapRecord (recordType: Type) =
        match FSharpType.IsRecord recordType with
        | false -> MappingFailure.InvalidType "" |> Error
        | true ->
            FSharpType.GetRecordFields(recordType)
            |> Array.map (fun field ->
                match SupportedType.TryFromType field.PropertyType, FSharpType.IsRecord field.PropertyType with
                | Ok supportedType, _ -> FieldType.SupportedType supportedType |> Ok
                | Error _, true -> FieldType.Child () |> Ok
                | Error _, false -> Error ""
                |> Result.map (fun ft ->
                    ({ Name = "" }: Field)


                    ()))
            |> List.ofArray
            |> partitionResults





    let mapRecords =

        ()

    ()
