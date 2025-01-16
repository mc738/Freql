namespace Freql.Tools.CodeFirst.Core

open System
open System.Collections.Generic
open System.Reflection
open Freql.Core

[<AutoOpen>]
module Common =

    type RecordInformation =
        { Name: string
          Type: Type
          Fields: FieldInformation list
          VirtualFields: VirtualField list }

        member ri.HasDefinedPrimaryKey() =
            ri.Fields |> List.exists (fun f -> f.PrimaryKey.IsSome)

        member ri.HasAttributePrimaryKey() =
            ri.Fields
            |> List.exists (fun f ->
                match f.PrimaryKey with
                | Some(PrimaryKeyDefinitionType.Attribute) -> true
                | _ -> false)

        member ri.HasConventionPrimaryKey() =
            ri.Fields
            |> List.exists (fun f ->
                match f.PrimaryKey with
                | Some(PrimaryKeyDefinitionType.Convention _) -> true
                | _ -> false)

        member ri.GetAllPrimaryKeyFields() =
            ri.Fields |> List.filter (fun f -> f.PrimaryKey.IsSome)

        member ri.GetAttributePrimaryKeyFields() =
            ri.Fields
            |> List.filter (fun f ->
                match f.PrimaryKey with
                | Some(PrimaryKeyDefinitionType.Attribute) -> true
                | _ -> false)

        member ri.GetConventionPrimaryKeyFields() =
            ri.Fields
            |> List.filter (fun f ->
                match f.PrimaryKey with
                | Some(PrimaryKeyDefinitionType.Convention _) -> true
                | _ -> false)

        member ri.GetTopConventionPrimaryKeyField() =
            ri.Fields
            |> List.fold
                (fun top curr ->
                    match curr.PrimaryKey with
                    | Some(PrimaryKeyDefinitionType.Convention priority) when priority > top -> priority
                    | _ -> top)
                -1

        /// <summary>
        /// Get the actual primary key field(s).
        /// This means attribute ones will be chosen first,
        /// if not the convention based one with the highest priority will be chosen.
        /// </summary>
        member ri.GetActualPrimaryKeyFields() =
            match ri.HasAttributePrimaryKey(), ri.HasConventionPrimaryKey() with
            | true, _ -> ri.GetAttributePrimaryKeyFields()
            | _, true ->
                ri.GetConventionPrimaryKeyFields()
                |> List.sortByDescending (fun field ->
                    match field.PrimaryKey with
                    | Some(PrimaryKeyDefinitionType.Convention v) -> v
                    | _ -> -1)
            | _, _ -> []

        member ri.GetScalarFields() =
            ri.Fields
            |> List.choose (fun field ->
                match field.Type with
                | SupportedType supportedType -> { Field = field; SupportedType = supportedType } |> Some
                | Record ``type`` ->
                    // Do nothing, this will be a foreign key in field ``type``
                    None
                | Collection collectionType ->
                    // Do nothing, this will be a foreign key in field ``type``
                    None)
        
        member ri.GetPrimaryKeyScalarFields() =
            ri.Fields
            |> List.choose (fun field ->
                match field.PrimaryKey.IsSome, field.Type with
                | true, SupportedType supportedType -> { Field = field; SupportedType = supportedType } |> Some
                | false, _
                | _, Record _
                | _, Collection _ -> None)
        
    and FieldInformation =
        { Name: string
          Type: FieldType
          PrimaryKey: PrimaryKeyDefinitionType option
          ForeignKey: FieldForeignKey option
          Index: FieldIndex option
          PropertyInformation: PropertyInfo
          Optional: bool }

    and ScalarField =
        { Field: FieldInformation
          SupportedType: SupportedType }


    and [<RequireQualifiedAccess>] PrimaryKeyDefinitionType =
        | Attribute
        | Convention of Priority: int

    and FieldForeignKey =
        { TypeName: string
          Type: Type
          FieldName: string option }

        member ffk.GetTypeName() = ffk.Type.Name

        member ffk.GetTypeFullName() = ffk.Type.FullName

    and FieldIndex = { Unique: bool }

    and FieldType =
        | SupportedType of SupportedType
        | Record of Type
        | Collection of FSharpCollectionType

    and VirtualField =
        | PrimaryKey of VirtualPrimaryKeyField
        | ForeignKey of VirtualForeignKeyField

    and VirtualPrimaryKeyField = { Type: Type }

    and VirtualForeignKeyField =
        { Type: Type
          ValueType: PrimaryKeyType }

    and [<RequireQualifiedAccess>] PrimaryKeyType =
        | Simple
        | Composite
        | Virtual


    [<RequireQualifiedAccess>]
    type ScalarValue =
        | Boolean of bool
        | Byte of byte
        | SByte of int8
        | Char of char
        | Decimal of decimal
        | Double of double
        | Single of single
        | Int16 of int16
        | UInt16 of uint16
        | Int of int
        | UInt of uint32
        | Int64 of int64
        | UInt64 of uint64
        | String of string
        | DateTime of DateTime
        | TimeSpan of TimeSpan
        | Guid of Guid

    [<RequireQualifiedAccess>]
    type PrimaryKeyValue =
        | Simple of PrimaryKeyValuePart
        | Composite of PrimaryKeyValuePart list
        | Virtual of PrimaryKeyValuePart

    and PrimaryKeyValuePart =
        { FieldName: string
          Value: ScalarValue }
