namespace Freql.Tools.CodeFirst.Core

open System.Reflection
open Freql.Tools.CodeFirst
open Freql.Tools.CodeFirst.Core.Attributes
open Microsoft.FSharp.Core
open Microsoft.FSharp.Reflection
open Freql.Core.Utils.Attributes

module Mapping =

    open System
    open Freql.Core

    [<RequireQualifiedAccess>]
    type MappingFailure =
        | InvalidType of Message: string
        | FieldErrors of FieldMappingError list

    and [<RequireQualifiedAccess>] FieldMappingError =
        | InvalidType of Name: string * Message: string
        | InvalidChild of Name: string * Failure: MappingFailure

    let partitionResults (results: Result<'T, 'U> seq) =
        results
        |> Seq.fold
            (fun (successes, errors) result ->
                match result with
                | Ok r -> r :: successes, errors
                | Error e -> successes, e :: errors)
            ([], [])
        |> fun (successes, errors) -> successes |> List.rev, errors |> List.rev

    let getPrimaryKeyAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<PrimaryKeyAttribute> propertyInfo

    let getForeignKeyAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<ForeignKeyAttribute> propertyInfo

    let getIndexAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<IndexAttribute> propertyInfo

    let getIgnoreAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<IgnoreFieldAttribute> propertyInfo

    let rec mapRecord (virtualField: FieldInformation option) (recordType: Type) =
        match FSharpType.IsRecord recordType with
        | false -> MappingFailure.InvalidType "" |> Error
        | true ->
            FSharpType.GetRecordFields(recordType)
            |> Array.filter (fun field -> getIgnoreAttribute field |> Option.isNone)
            |> Array.map (fun field ->
                match SupportedType.TryFromType field.PropertyType, FSharpType.IsRecord field.PropertyType with
                | Ok supportedType, _ -> FieldType.SupportedType supportedType |> Ok
                | Error _, true ->
                    
                    let virtualField =
                        ({ Name = $"{recordType.Name}Id"
                           Type = failwith "todo"
                           PrimaryKey = None
                           ForeignKey =
                             { TypeName = recordType.Name
                               FieldName = None }
                             |> Some
                           PropertyInformation = None
                           Index = failwith "todo"
                           VirtualField = true }
                        : FieldInformation)
                        |> Some

                    mapRecord virtualField field.PropertyType
                    |> Result.map FieldType.Child
                    |> Result.mapError (fun mf -> FieldMappingError.InvalidChild("", mf))
                | Error _, false -> FieldMappingError.InvalidType("", "") |> Error
                |> Result.map (fun ft ->
                    ({ Name = field.Name
                       Type = ft
                       PrimaryKey =
                         getPrimaryKeyAttribute field
                         |> Option.map (fun _ -> PrimaryKeyDefinitionType.Attribute)
                         |> Option.orElseWith (fun _ ->
                             match
                                 // TODO move conventions to config
                                 field.Name.Equals("Id", StringComparison.Ordinal)
                                 || field.Name.Equals($"{recordType.Name}Id", StringComparison.Ordinal)
                             with
                             | true -> Some PrimaryKeyDefinitionType.Convention
                             | false -> None)
                       ForeignKey =
                         getForeignKeyAttribute field
                         |> Option.map (fun fk ->
                             { TypeName = fk.OtherType.Name
                               FieldName = None })
                       PropertyInformation = Some field
                       Index =
                         // TODO
                         None
                       VirtualField = false }
                    : FieldInformation)))
            |> partitionResults
            |> fun (successes, errors) ->
                match errors.IsEmpty with
                | true ->
                    ({ Name = recordType.Name
                       Type = recordType
                       Fields =
                         match virtualField with
                         | Some vf -> vf :: successes
                         | None -> successes }
                    : RecordInformation)
                    |> Ok
                | false -> errors |> MappingFailure.FieldErrors |> Error

    /// <summary>
    /// Dedupe a list of types.
    /// This will filter out types that are either not FSharp records or as exist as 
    /// </summary>
    /// <param name="types"></param>
    let dedupeTypes (types: Type List) =
        
        ()
    
    let mapRecords (types: Type list) =
        types
        |> List.map (mapRecord None)
        |> partitionResults
        |> fun (successes, errors) ->
            match errors.IsEmpty with
            | true -> Ok successes
            | false -> Error errors