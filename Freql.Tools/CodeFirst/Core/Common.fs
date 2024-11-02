namespace Freql.Tools.CodeFirst.Core

open System
open System.Reflection
open Freql.Core

[<AutoOpen>]
module Common =

    type RecordInformation =
        { Name: string
          Type: Type
          Fields: FieldInformation list
          VirtualFields: VirtualField list }

    and FieldInformation =
        { Name: string
          Type: FieldType
          PrimaryKey: PrimaryKeyDefinitionType option
          ForeignKey: FieldForeignKey option
          Index: FieldIndex option
          PropertyInformation: PropertyInfo
          Optional: bool }

    and [<RequireQualifiedAccess>] PrimaryKeyDefinitionType =
        | Attribute
        | Convention

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
        { Type: Type }

        member vf.GetTypeName() = vf.Type.Name

        member vf.GetTypeFullName() = vf.Type.FullName

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
        | Guid of Guid

    [<RequireQualifiedAccess>]
    type PrimaryKeyType =
        | Simple
        | Composite

    type PrimaryKeyValue =
        | Simple of PrimaryKeyValuePart
        | Composite of PrimaryKeyValuePart list

    and PrimaryKeyValuePart =
        { FieldName: string
          Value: ScalarValue }
