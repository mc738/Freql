namespace Freql.Csv

open System
open System.Globalization
open System.IO
open System.Reflection
open System.Text
open Freql.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Reflection

type CsvValueFormatAttribute(format: string) =

    inherit Attribute()

    member att.Format = format


type CsvNumberStyleAttribute(numberStyle: NumberStyles) =

    inherit Attribute()

    member att.NumberStyles = numberStyle

type CsvReplacementValueAttribute(matches: string array, replacement: string) =

    inherit Attribute()

    member att.Matches = matches

    member att.Replacement = replacement

module CsvParser =

    module Parsing =

        let inBounds (input: string) i = i >= 0 && i < input.Length

        let getChar (input: string) i =
            match inBounds input i with
            | true -> Some input.[i]
            | false -> None

        let readUntilChar (input: string) (c: Char) (start: int) (sb: StringBuilder) =
            let rec read i (sb: StringBuilder) =
                match getChar input i with
                | Some r when r = '"' && getChar input (i + 1) = Some '"' -> read (i + 2) <| sb.Append('"')
                | Some r when r = c -> Some <| (sb.ToString(), i)
                | Some r -> read (i + 1) <| sb.Append(r)
                | None -> Some <| (sb.ToString(), i + 1)

            read start sb

    module Records =

        let tryGetAtIndex (values: string array) (i: int) =
            match i >= 0 && i < values.Length with
            | true -> Some values.[i]
            | false -> None

        //type Field = { Index: int; Value: obj }

        type RecordProperty =
            { PropertyInfo: PropertyInfo
              Type: SupportedType
              Index: int
              NumberStyle: NumberStyles option
              Format: string option
              Replacement: string option -> string option }

            static member Create(propertyInfo: PropertyInfo, index: int) =
                ({ PropertyInfo = propertyInfo
                   Type = SupportedType.FromType(propertyInfo.PropertyType)
                   Index = index
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
                     | _ -> id }: RecordProperty)
                
            member rp.IntNumberStyle() =
                rp.NumberStyle
                |> Option.defaultValue NumberStyles.Integer
                
            member rp.FloatNumberStyle() =
                rp.NumberStyle
                |> Option.defaultValue NumberStyles.Float
                
        let createRecord<'T> (values: string list) =
            //let definition = RecordDefinition.Create<'T>()

            let getValue =
                values |> Array.ofList |> tryGetAtIndex


            let t = typeof<'T>

            let values =
                t.GetProperties()
                |> List.ofSeq
                //|> List.mapi (fun i pi ->



                //)
                |> List.mapi (fun i pi ->
                    //printfn $"*** {i}"
                    // Look for format field.
                    let format =
                        match Attribute.GetCustomAttribute(pi, typeof<CsvValueFormatAttribute>) with
                        | att when att <> null -> Some <| (att :?> CsvValueFormatAttribute).Format
                        | _ -> None

                    let numberStyles =
                        match Attribute.GetCustomAttribute(pi, typeof<CsvNumberStyleAttribute>) with
                        | att when att <> null ->
                            (att :?> CsvNumberStyleAttribute).NumberStyles
                            |> Some
                        | _ -> None

                    let replacement =
                        match Attribute.GetCustomAttribute(pi, typeof<CsvReplacementValueAttribute>) with
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

                    let t =
                        SupportedType.FromType(pi.PropertyType)

                    let intNumberStyle =
                        numberStyles
                        |> Option.defaultValue NumberStyles.Integer

                    let floatNumberStyle =
                        numberStyles
                        |> Option.defaultValue NumberStyles.Float

                    match getValue i |> replacement, t with
                    | Some v, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                    | Some v, SupportedType.Boolean -> bool.Parse v :> obj
                    | Some v, SupportedType.Byte -> Byte.Parse v :> obj
                    | Some v, SupportedType.SByte -> SByte.Parse v :> obj
                    | Some v, SupportedType.Char -> v.[0] :> obj
                    | Some v, SupportedType.Decimal -> Decimal.Parse(v, floatNumberStyle) :> obj
                    | Some v, SupportedType.Double -> Double.Parse(v, floatNumberStyle) :> obj
                    | Some v, SupportedType.DateTime ->
                        match format with
                        | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                        | None -> DateTime.Parse(v)
                        :> obj
                    | Some v, SupportedType.Single -> Single.Parse(v, floatNumberStyle) // Double.TryParse v :> obj
                    | Some v, SupportedType.Guid ->
                        match format with
                        | Some f -> Guid.ParseExact(v, f)
                        | None -> Guid.Parse(v)
                        :> obj
                    | Some v, SupportedType.TimeSpan ->
                        match format with
                        | Some f -> TimeSpan.ParseExact(v, f, CultureInfo.InvariantCulture)
                        | None -> TimeSpan.Parse(v)
                        :> obj
                    | Some v, SupportedType.Int -> Int32.Parse(v, intNumberStyle) :> obj
                    | Some v, SupportedType.UInt -> UInt32.Parse(v, intNumberStyle) :> obj
                    | Some v, SupportedType.Long -> Int64.Parse(v, intNumberStyle) :> obj
                    | Some v, SupportedType.ULong -> UInt64.Parse(v, intNumberStyle) :> obj
                    | Some v, SupportedType.Short -> Int16.Parse(v, intNumberStyle) :> obj
                    | Some v, SupportedType.UShort -> UInt16.Parse(v, intNumberStyle) :> obj
                    | Some v, SupportedType.String -> v :> obj
                    | Some v, SupportedType.Option st ->
                        // TODO this could be tidied up/ cleaned up
                        // Note, an error in here will return None. This might not be correct?
                        try
                            match v |> String.IsNullOrWhiteSpace |> not, st with
                            | true, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                            | true, SupportedType.Boolean -> bool.Parse v |> Some :> obj
                            | true, SupportedType.Byte -> Byte.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.SByte -> SByte.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.Char -> v.[0] |> Some :> obj
                            | true, SupportedType.Decimal -> Decimal.Parse(v, floatNumberStyle) |> Some :> obj
                            | true, SupportedType.Double -> Double.Parse(v, floatNumberStyle) |> Some :> obj
                            | true, SupportedType.DateTime ->
                                match format with
                                | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                                | None -> DateTime.Parse(v)
                                |> Some
                                :> obj
                            | true, SupportedType.Single -> Single.Parse(v, floatNumberStyle) |> Some :> obj // Double.TryParse v :> obj
                            | true, SupportedType.Guid ->
                                match format with
                                | Some f -> Guid.ParseExact(v, f)
                                | None -> Guid.Parse(v)
                                |> Some
                                :> obj
                            | true, SupportedType.TimeSpan ->
                                match format with
                                | Some f -> TimeSpan.ParseExact(v, f, CultureInfo.InvariantCulture)
                                | None -> TimeSpan.Parse(v)
                                :> obj
                            | true, SupportedType.Int -> Int32.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.UInt -> UInt32.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.Long -> Int64.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.ULong -> UInt64.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.Short -> Int16.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.UShort -> UInt16.Parse(v, intNumberStyle) |> Some :> obj
                            | true, SupportedType.String -> v |> Some :> obj
                            | _, SupportedType.Option _ -> failwith "Nested option types not supported in CSV."
                            | false, _ -> None :> obj
                        with
                        | _ -> None :> obj
                    | None, _ -> failwith "")

            let o =
                FSharpValue.MakeRecord(t, values |> Array.ofList)

            o :?> 'T
            
        let createRecordV2<'T> (properties: RecordProperty array) (values: string array) =

            let values =
                properties
                |> Array.map (fun rp ->
                    //printfn $"*** {i}"
                    // Look for format field.
                    
                    match values |> Array.tryItem rp.Index |> rp.Replacement, rp.Type with
                    | Some v, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                    | Some v, SupportedType.Boolean -> bool.Parse v :> obj
                    | Some v, SupportedType.Byte -> Byte.Parse v :> obj
                    | Some v, SupportedType.SByte -> SByte.Parse v :> obj
                    | Some v, SupportedType.Char -> v.[0] :> obj
                    | Some v, SupportedType.Decimal -> Decimal.Parse(v, rp.FloatNumberStyle()) :> obj
                    | Some v, SupportedType.Double -> Double.Parse(v, rp.FloatNumberStyle()) :> obj
                    | Some v, SupportedType.DateTime ->
                        match rp.Format with
                        | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                        | None -> DateTime.Parse(v)
                        :> obj
                    | Some v, SupportedType.Single -> Single.Parse(v, rp.FloatNumberStyle()) // Double.TryParse v :> obj
                    | Some v, SupportedType.Guid ->
                        match rp.Format with
                        | Some f -> Guid.ParseExact(v, f)
                        | None -> Guid.Parse(v)
                        :> obj
                    | Some v, SupportedType.TimeSpan ->
                        match rp.Format with
                        | Some f -> TimeSpan.ParseExact(v, f, CultureInfo.InvariantCulture)
                        | None -> TimeSpan.Parse(v)
                        :> obj
                    | Some v, SupportedType.Int -> Int32.Parse(v, rp.IntNumberStyle()) :> obj
                    | Some v, SupportedType.UInt -> UInt32.Parse(v, rp.IntNumberStyle()) :> obj
                    | Some v, SupportedType.Long -> Int64.Parse(v, rp.IntNumberStyle()) :> obj
                    | Some v, SupportedType.ULong -> UInt64.Parse(v, rp.IntNumberStyle()) :> obj
                    | Some v, SupportedType.Short -> Int16.Parse(v, rp.IntNumberStyle()) :> obj
                    | Some v, SupportedType.UShort -> UInt16.Parse(v, rp.IntNumberStyle()) :> obj
                    | Some v, SupportedType.String -> v :> obj
                    | Some v, SupportedType.Option st ->
                        // TODO this could be tidied up/ cleaned up
                        // Note, an error in here will return None. This might not be correct?
                        try
                            match v |> String.IsNullOrWhiteSpace |> not, st with
                            | true, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                            | true, SupportedType.Boolean -> bool.Parse v |> Some :> obj
                            | true, SupportedType.Byte -> Byte.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.SByte -> SByte.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.Char -> v.[0] |> Some :> obj
                            | true, SupportedType.Decimal -> Decimal.Parse(v, rp.FloatNumberStyle()) |> Some :> obj
                            | true, SupportedType.Double -> Double.Parse(v, rp.FloatNumberStyle()) |> Some :> obj
                            | true, SupportedType.DateTime ->
                                match rp.Format with
                                | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                                | None -> DateTime.Parse(v)
                                |> Some
                                :> obj
                            | true, SupportedType.Single -> Single.Parse(v, rp.FloatNumberStyle()) |> Some :> obj // Double.TryParse v :> obj
                            | true, SupportedType.Guid ->
                                match rp.Format with
                                | Some f -> Guid.ParseExact(v, f)
                                | None -> Guid.Parse(v)
                                |> Some
                                :> obj
                            | true, SupportedType.TimeSpan ->
                                match rp.Format with
                                | Some f -> TimeSpan.ParseExact(v, f, CultureInfo.InvariantCulture)
                                | None -> TimeSpan.Parse(v)
                                :> obj
                            | true, SupportedType.Int -> Int32.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.UInt -> UInt32.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.Long -> Int64.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.ULong -> UInt64.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.Short -> Int16.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.UShort -> UInt16.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                            | true, SupportedType.String -> v |> Some :> obj
                            | _, SupportedType.Option _ -> failwith "Nested option types not supported in CSV."
                            | false, _ -> None :> obj
                        with
                        | _ -> None :> obj
                    | None, _ -> failwith "")

            let o =
                FSharpValue.MakeRecord(typeof<'T>, values)

            o :?> 'T

    open Records
    
    let parseLine (input: string) =
        let sb = StringBuilder()

        let rec readBlock (i, sb: StringBuilder, acc: string list) =

            match Parsing.getChar input i with
            | Some c when c = '"' ->
                match Parsing.readUntilChar input '"' (i + 1) sb with
                | Some (r, i) ->
                    // Plus 2 to skip end " and ,
                    readBlock (i + 2, sb.Clear(), acc @ [ r ])
                | None -> acc
            // Read until " (not delimited).
            | Some _ ->
                match Parsing.readUntilChar input ',' i sb with
                | Some (r, i) -> readBlock (i + 1, sb.Clear(), acc @ [ r ])
                | None -> acc
            | None -> acc

        readBlock (0, sb, [])
        
    type ParseError = { Line: int; Error: string }

    let parseFile<'T> (hasHeader: bool) (path: string) =
        File.ReadAllLines path
        |> List.ofSeq
        |> fun l ->
            match hasHeader with
            | true -> l.Tail
            | false -> l
        |> List.mapi (fun i l ->
            try
                parseLine l |> Records.createRecord<'T> |> Ok
            with
            | exn ->
                printfn $"Error - {i + 1} {exn.Message}"
                Error { Line = i + 1; Error = exn.Message })

    let parseFileV2<'T> (hasHeader: bool) (path: string) =
        let rps = typeof<'T>.GetProperties() |> Array.mapi (fun i pi -> RecordProperty.Create(pi, i))
        
        File.ReadAllLines path
        |> List.ofSeq
        |> fun l ->
            match hasHeader with
            | true -> l.Tail
            | false -> l
        |> List.mapi (fun i l ->
            try
                parseLine l |> Array.ofList |> Records.createRecordV2<'T> rps |> Ok
            with
            | exn ->
                printfn $"Error - {i + 1} {exn.Message}"
                Error { Line = i + 1; Error = exn.Message })

    let splitResults<'T> (results: Result<'T, ParseError> list) =
        results
        |> List.fold
            (fun (ok, errors) item ->
                match item with
                | Ok i -> ok @ [ i ], errors
                | Error e -> ok, errors @ [ e ])
            ([], [])
