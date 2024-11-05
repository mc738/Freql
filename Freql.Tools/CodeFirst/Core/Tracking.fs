namespace Freql.Tools.CodeFirst.Core

module Tracking =

    open System
    open System.Reflection
    open Microsoft.FSharp.Reflection
    open Freql.Core
    open Freql.Tools.CodeGeneration
    open Freql.Tools.CodeGeneration.Boilerplate
    open Freql.Tools.CodeFirst.Core.Attributes

    type RecordTrackingSettings =
        { DefaultStringComparison: StringComparison }

    let getComparisonCondition (settings: RecordTrackingSettings) (propertyInfo: PropertyInfo) =

        match SupportedType.TryFromType propertyInfo.PropertyType with
        | Ok supportedType ->
            match supportedType with
            | SupportedType.Boolean -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Byte -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.SByte -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Char -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Decimal -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Double -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}" // TODO improve
            | SupportedType.Single -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}" // TODO improve
            | SupportedType.Int -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.UInt -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Short -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.UShort -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Long -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.ULong -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.String ->
                Some
                    $"a.{propertyInfo.Name}.Equals(b.{propertyInfo.Name}, StringComparison.{Enum.GetName settings.DefaultStringComparison})"
            | SupportedType.DateTime -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.TimeSpan -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Guid -> Some $"a.{propertyInfo.Name} <> b.{propertyInfo.Name}"
            | SupportedType.Blob -> failwith "todo" // TODO implement
            | SupportedType.Option supportedType -> failwith "todo"
        | Error _ -> None

    let generateComparisonOperationCode
        (settings: RecordTrackingSettings)
        (indent: string)
        (tableName: string)
        (propertyInfo: PropertyInfo)
        =
        [ match getComparisonCondition settings propertyInfo with
          | Some comparisonCondition ->
              $"if {comparisonCondition} then"
              $"    {{ TableName = \"{tableName}\""
              $"      FieldName = \"{propertyInfo.Name}\""
              $"      NewValue = box b.{propertyInfo.Name} }}"
              "    |> RecordTrackingOperation.UpdateField"
          | None -> () ]

    let generateComparisonCode (indent: string) (recordType: Type) =

        match FSharpType.IsRecord recordType with
        | true ->
            let fields =
                FSharpType.GetRecordFields recordType
                |> List.ofArray
                |> List.filter (fun field ->
                    getPrimaryKeyAttribute field |> Option.isNone
                    && getIgnoreAttribute field |> Option.isNone)
                |> List.collect (
                    generateComparisonOperationCode
                        { DefaultStringComparison = StringComparison.OrdinalIgnoreCase }
                        indent
                        recordType.Name
                )

            [ match fields.IsEmpty with
              | true ->
                  $"{indent}let ``compare {recordType.Name} records`` (_: {recordType.Name}) (_: {recordType.Name}) = []"
                  ""
              | false ->
                  $"{indent}let ``compare {recordType.Name} records`` (a: {recordType.Name}) (b: {recordType.Name}) ="

                  yield!
                      fields
                      |> Utils.wrapInArray 2
                      
                      //|> List.mapi (fun i line ->
                      //    match i with
                      //    | 0 -> $"{indent}    [ {line}"
                      //    | _ when i = fields.Length - 1 -> $"{indent}    {line} ]"
                      //    | _ -> $"{indent}      {line}")

                  "" ]
        //|> Ok
        | false -> failwith "" // Error ""

    let codeGen (types: Type list) =

        let ns = "Freql.CodeFirstSandbox"

        let openStatements =
            types
            |> List.map (fun t -> $"open {t.ReflectedType.ToString()}")
            |> List.distinct

        let indent = "    "

        [ yield! Header.lines
          $"namespace {ns}"
          ""
          "open System"
          "open Freql.Tools.CodeFirst"
          "open Freql.Tools.CodeFirst.Operations"
          yield! openStatements
          ""
          "module Internal ="
          yield! types |> List.collect (generateComparisonCode indent) ]
        |> String.concat Environment.NewLine
