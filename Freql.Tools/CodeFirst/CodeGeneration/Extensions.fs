namespace Freql.Tools.CodeFirst.CodeGeneration

open Freql.Core
open Freql.Tools.CodeFirst.Core
open Microsoft.FSharp.Reflection

module Extensions =

    open System
    open Freql.Tools.CodeGeneration
    open Freql.Tools.CodeGeneration.Utils

    //let generateGetPrimaryKey (recordType: Type) =
    //    FSharpType.GetRecordFields recordType
    //    |> Array.choose (fun field ->
    //        //match Attributes.getPrimaryKeyAttribute field,
    //
    //        ())
    //
    //    ()

    let generateGetPrimaryKey (record: RecordInformation) =
        let pks = record.GetActualPrimaryKeyFields()

        let fieldTypeToValue (fieldType: FieldType) =
            match fieldType with
            | Record ``type`` -> failwith "Field type can not be record"
            | Collection collectionType -> failwith "Field type can not be a collection type"
            | SupportedType supportedType ->
                match supportedType with
                | SupportedType.Boolean -> "Boolean"
                | SupportedType.Byte -> "Byte"
                | SupportedType.SByte -> "SByte"
                | SupportedType.Char -> "Char"
                | SupportedType.Decimal -> "Decimal"
                | SupportedType.Double -> "Double"
                | SupportedType.Single -> "Single"
                | SupportedType.UInt -> "UInt"
                | SupportedType.Int -> "Int"
                | SupportedType.UShort -> "UInt16"
                | SupportedType.Short -> "Int16"
                | SupportedType.ULong -> "UInt64"
                | SupportedType.Long -> "Int64"
                | SupportedType.String -> "String"
                | SupportedType.DateTime -> "DateTime"
                | SupportedType.TimeSpan -> "TimeSpan"
                | SupportedType.Guid -> "Guid"
                | SupportedType.Blob -> failwith "Primary key field values do not support blob types"
                | SupportedType.Option supportedType -> failwith "Primary key field values do not support option types"

        match pks.Length with
        | 0 -> [ "member this.GetPrimaryKey() = None" |> indent1 ]
        | 1 ->
            let pk = pks.Head

            [ "member this.GetPrimaryKey() =" |> indent1
              $"{{ FieldName = \"{pk.PropertyInformation.Name}\"" |> indent 2
              $"  Value = ScalarValue.{fieldTypeToValue pk.Type} this.{pk.Name} }}"
              |> indent 2
              "|> PrimaryKeyValue.Simple" |> indent 2
              "|> Some" |> indent 2 ]
        // TODO - sort this
        | _ -> [ "member this.GetPrimaryKey() = None" |> indent1 ]

    let generateGetValues (record: RecordInformation) =
        [ "member this.GetValues() =" |> indent1
          yield!
              record.GetScalarFields()
              |> List.map (fun sf -> $"box this.{sf.Field.Name}")
              |> wrapInArray 2 ]

    let generateTypeExtensions (ctx: CodeGeneratorContext) (record: RecordInformation) =
        [ $"type {record.Name} with"
          yield! generateGetPrimaryKey record
          ""
          yield! generateGetValues record
          ""
          yield! ctx.DatabaseSpecificProfile.TypeExtension ctx record |> List.map indent1
          "" ]

    let generateModule (ctx: CodeGeneratorContext) =
        ({ Name = "Extensions"
           RequireQualifiedAccess = false
           AutoOpen = false
           SummaryCommentLines =
             [ "A collection of extensions on domain models to help with code-first binds."
               "Extensions are added so domain models do not need to be polluted with methods only needed for bindings." ]
           AttributeLines = internalCompilerMessage
           BaseIndent = 0
           IndentContent = true
           OpenReferences = []
           Content = ctx.Records |> List.collect (generateTypeExtensions ctx) }
        : Modules.Parameters)
        |> Modules.generate
