namespace Freql.Xlsx

open System

module Records =

    open System.Reflection
    open DocumentFormat.OpenXml.Spreadsheet
    open Microsoft.FSharp.Reflection
    open Freql.Core.Common.Types

    type RecordProperty =
        { PropertyInfo: PropertyInfo
          Type: SupportedType
          Index: int
          ColumnName: string
          //NumberStyle: NumberStyles option
          Format: string option
          OADate: bool }
        //Replacement: string option -> string option }

        static member Create(propertyInfo: PropertyInfo, index: int, startIndex: int) =
            let options =
                match Attribute.GetCustomAttribute(propertyInfo, typeof<XlsxOptionsAttribute>) with
                | att when att <> null -> att :?> XlsxOptionsAttribute |> Some
                | _ -> None

            let strToOptional (str: string) =
                match String.IsNullOrWhiteSpace str with
                | true -> None
                | false -> Some str

            ({ PropertyInfo = propertyInfo
               Type = SupportedType.FromType(propertyInfo.PropertyType)
               Index = index
               ColumnName =
                 // NOTE currently there are 2 attributes that can see the column name. This might change.
                 match Attribute.GetCustomAttribute(propertyInfo, typeof<XlsxColumnNameAttribute>) with
                 | att when att <> null -> (att :?> XlsxColumnNameAttribute).Name |> Some
                 | _ -> None
                 |> Option.orElseWith (fun _ -> options |> Option.bind (fun o -> o.ColumnName |> strToOptional))
                 |> Option.defaultWith (fun _ -> indexToColumnName (index + startIndex))
               Format =
                 match Attribute.GetCustomAttribute(propertyInfo, typeof<XlsxFormatAttribute>) with
                 | att when att <> null -> (att :?> XlsxFormatAttribute).Format |> Some
                 | _ -> None
                 |> Option.orElseWith (fun _ -> options |> Option.bind (fun o -> o.Format |> strToOptional))
               OADate = options |> Option.map (fun o -> o.OADate) |> Option.defaultValue false

             (*
               NumberStyle =
                 match Attribute.GetCustomAttribute(propertyInfo, typeof<CsvNumberStyleAttribute>) with
                 | att when att <> null ->
                     (att :?> CsvNumberStyleAttribute).NumberStyles
                     |> Some
                 | _ -> None
               Format =
                 match Attribute.GetCustomAttribute(propertyInfo, typeof<CsvValueFormatAttribute>) with
                 | att when att <> null -> Some <| (att :?> CsvValueFormatAttribute).Format
                 | _ -> None
               Replacement =
                 match Attribute.GetCustomAttribute(propertyInfo, typeof<CsvReplacementValueAttribute>) with
                 | att when att <> null ->
                     let a =
                         (att :?> CsvReplacementValueAttribute)

                     (fun ov ->
                         ov
                         |> Option.map (fun v ->
                             if a.Matches |> Seq.contains v then
                                 a.Replacement
                             else
                                 v))
                 | _ -> id
                 *)

             }
            : RecordProperty)

    //member rp.IntNumberStyle() =
    //    rp.NumberStyle
    //    |> Option.defaultValue NumberStyles.Integer

    //member rp.FloatNumberStyle() =
    //    rp.NumberStyle
    //    |> Option.defaultValue NumberStyles.Float

    let createRecord<'T> (properties: RecordProperty array) (row: Row) =

        let tryGetValue
            (cell: Cell)
            (st: SupportedType)
            (format: string option)
            (optionalErrorToNone: bool)
            (oaDate: bool)
            =
            match st with
            | SupportedType.Blob -> Error "Blob types not supported in xlsx"
            | SupportedType.Boolean ->
                match cellToBool cell with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type bool"
            | SupportedType.Byte ->
                match cellToInt cell |> Option.map byte with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type byte"
            | SupportedType.Char ->
                match cellToString cell |> Seq.tryItem 0 with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type char"
            | SupportedType.Decimal ->
                match cellToDecimal cell with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type decimal"
            | SupportedType.Double ->
                match cellToDouble cell with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type double"
            | SupportedType.Float ->
                match cellToDouble cell |> Option.map float32 with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type float"
            | SupportedType.Int ->
                match cellToInt cell with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type int"
            | SupportedType.Short ->
                match cellToInt cell |> Option.map int16 with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type short"
            | SupportedType.Long ->
                match cellToInt cell |> Option.map int64 with
                | Some v -> box v |> Ok
                | None -> Error "Value could not be extracted as type long"
            | SupportedType.String -> cellToString cell |> box |> Ok
            | SupportedType.DateTime ->
                match oaDate with
                | true ->
                    match cellToOADateTime cell with
                    | Some v -> box v |> Ok
                    | None -> Error "Value could not be extracted as type datetime"
                | false ->
                    match cellToDateTime cell with
                    | Some v -> box v |> Ok
                    | None -> Error "Value could not be extracted as type datetime"
            | SupportedType.Guid ->
                match format with
                | Some f ->
                    match Guid.TryParseExact(cellToString cell, f) with
                    | true, v -> box v |> Ok
                    | false, _ -> Error $"Value could not be extracted as type guid (format: '{f}')"
                | None ->
                    match Guid.TryParse(cellToString cell) with
                    | true, v -> box v |> Ok
                    | false, _ -> Error "Value could not be extracted as type guid"
            | SupportedType.Option ist ->
                match ist with
                | SupportedType.Blob -> Error "Blob types not supported in xlsx"
                | SupportedType.Boolean ->
                    match cellToBool cell with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type bool"
                | SupportedType.Byte ->
                    match cellToInt cell |> Option.map byte with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type byte"
                | SupportedType.Char ->
                    match cellToString cell |> Seq.tryItem 0 with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type char"
                | SupportedType.Decimal ->
                    match cellToString cell |> Seq.tryItem 0 with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type decimal"
                | SupportedType.Double ->
                    match cellToDouble cell with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type double"
                | SupportedType.Float ->
                    match cellToDouble cell |> Option.map float32 with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type float"
                | SupportedType.Int ->
                    match cellToInt cell with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type int"
                | SupportedType.Short ->
                    match cellToInt cell |> Option.map int16 with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type short"
                | SupportedType.Long ->
                    match cellToInt cell |> Option.map int64 with
                    | Some v -> Some v |> box |> Ok
                    | None ->
                        match optionalErrorToNone with
                        | true -> None |> box |> Ok
                        | false -> Error "Value could not be extracted as type long"
                | SupportedType.String -> cellToString cell |> Some |> box |> Ok
                | SupportedType.DateTime ->
                    match oaDate with
                    | true ->
                        match cellToOADateTime cell with
                        | Some v -> Some v |> box |> Ok
                        | None ->
                            match optionalErrorToNone with
                            | true -> None |> box |> Ok
                            | false -> Error "Value could not be extracted as type datetime"
                    | false ->
                        match cellToDateTime cell with
                        | Some v -> Some v |> box |> Ok
                        | None ->
                            match optionalErrorToNone with
                            | true -> None |> box |> Ok
                            | false -> Error "Value could not be extracted as type datetime"
                | SupportedType.Guid ->
                    match format with
                    | Some f ->
                        match Guid.TryParseExact(cellToString cell, f) with
                        | true, v -> Some v |> box |> Ok
                        | false, _ ->
                            match optionalErrorToNone with
                            | true -> None |> box |> Ok
                            | false -> Error $"Value could not be extracted as type guid (format: '{f}')"
                    | None ->
                        match Guid.TryParse(cellToString cell) with
                        | true, v -> Some v |> box |> Ok
                        | false, _ ->
                            match optionalErrorToNone with
                            | true -> None |> box |> Ok
                            | false -> Error "Value could not be extracted as type guid"
                | SupportedType.Option ist -> failwith "Nested option types not supported in xlsx"

        let values =
            properties
            |> Array.map (fun rp ->
                //printfn $"*** {i}"
                // Look for format field.
                match getCellFromRow row rp.ColumnName, rp.Type with
                | None, SupportedType.Option _ -> None |> box
                | Some c, st ->
                    match tryGetValue c st rp.Format true rp.OADate with
                    | Ok v -> v
                    | Error e -> failwith e
                | None, _ -> failwith $"{rp.PropertyInfo.Name} value not found")

        let o = FSharpValue.MakeRecord(typeof<'T>, values)

        o :?> 'T
