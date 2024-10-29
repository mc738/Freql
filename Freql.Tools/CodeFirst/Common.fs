namespace Freql.Tools.CodeFirst

open Freql.Core

[<AutoOpen>]
module Common =

    type RecordInformation = { Name: string; Fields: Field list }

    and Field =
        { Name: string
          Type: FieldType
          PrimaryKey: PrimaryKeyDefinitionType option
          ForeignKey: FieldForeignKey option
          Index: FieldIndex option
          /// <summary>
          /// A virtual field is one that doesn't exist on the record but is used for generation.
          /// For example if type `Foo` has a field of type `Bar`, `Bar` will have a virtual field added of `FooId`
          /// which will be marked as a foreign key.
          /// </summary>
          VirtualField: bool }

    and [<RequireQualifiedAccess>] PrimaryKeyDefinitionType =
        | Attribute
        | Convention
    
    and FieldForeignKey =
        { TypeName: string
          FieldName: string option }

    and FieldIndex = { Unique: bool }

    and FieldType =
        | SupportedType of SupportedType
        | Child of RecordInformation


    ()
