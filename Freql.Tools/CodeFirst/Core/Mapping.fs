namespace Freql.Tools.CodeFirst.Core

open System
open System.Reflection
open Freql.Tools.CodeFirst
open Freql.Tools.CodeFirst.Core.Attributes
open Google.Protobuf.WellKnownTypes
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

    let mapRecord (recordType: Type) =
        match FSharpType.IsRecord recordType with
        | false -> MappingFailure.InvalidType "" |> Error
        | true ->
            FSharpType.GetRecordFields(recordType)
            |> Array.filter (fun field -> getIgnoreAttribute field |> Option.isNone)
            |> Array.map (fun field ->
                match
                    SupportedType.TryFromType field.PropertyType,
                    FSharpType.IsRecord field.PropertyType,
                    FSharpCollectionType.TryFromType field.PropertyType
                with
                | Ok st, _, _ -> FieldType.SupportedType st |> Ok
                | _, true, _ -> FieldType.Record field.PropertyType |> Ok
                | _, _, Some ct -> FieldType.Collection ct |> Ok
                | Error _, false, None -> FieldMappingError.InvalidType(field.Name, "") |> Error
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
                               Type = fk.OtherType
                               FieldName = None })
                       PropertyInformation = field
                       Index =
                         // TODO
                         None
                       Optional = Types.typeIsOption field.PropertyType }
                    : FieldInformation)))
            |> partitionResults
            |> fun (successes, errors) ->
                match errors.IsEmpty with
                | true ->
                    ({ Name = recordType.Name
                       Type = recordType
                       Fields = successes
                       VirtualFields = [] }
                    : RecordInformation)
                    |> Ok
                | false -> errors |> MappingFailure.FieldErrors |> Error



    (*
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
    *)

    /// <summary>
    /// Dedupe a list of types.
    /// This will filter out types that are either not FSharp records or as exist as
    /// </summary>
    /// <param name="types"></param>
    //let dedupeTypes (types: Type List) =
    //    let rec searchForType (iteration: int) (searchType: Type) (currentType: Type) =
    //        match iteration > 10 with
    //        | true -> failwith "Maximum recursion depth reached"
    //        | false ->
    //            match FSharpType.IsRecord currentType with
    //            | false -> false
    //            | true ->
    //                FSharpType.GetRecordFields currentType
    //                |> Array.exists (fun field ->
    //
    //
    //
    //                    ())
    //
    //
    //
    //
    //    types
    //    |> List.choose (fun t ->
    //        match FSharpType.IsRecord t with
    //        | false -> None
    //        | true ->
    //
    //
    //
    //
    //            ()

    //        )

    //    ()

    let mapRecords (types: Type list) =
        types
        |> List.map mapRecord
        |> partitionResults
        |> fun (successes, errors) ->
            match errors.IsEmpty with
            | true ->
                // After mapping the records add any virtual fields.
                successes
                |> List.map (fun record ->
                    { record with
                        VirtualFields =
                            successes
                            |> List.filter (fun otherRecord ->
                                otherRecord.Fields
                                |> List.exists (fun field ->
                                    match field.Type with
                                    | SupportedType supportedType -> false
                                    | Record ``type`` ->
                                        ``type``.FullName.Equals(
                                            record.Type.FullName,
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                    | Collection collectionType ->
                                        collectionType
                                            .GetInnerType()
                                            .FullName.Equals(
                                                record.Type.FullName,
                                                StringComparison.OrdinalIgnoreCase
                                            ))
                                )
                            |> List.map (fun otherRecord -> { Type = otherRecord.Type }) })
                |> Ok

            | false -> Error errors
