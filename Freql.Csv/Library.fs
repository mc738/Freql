namespace Freql.Csv

open System
open System.Globalization
open System.IO
open System.Text
open Freql.Core.Common
open Microsoft.FSharp.Reflection

type CsvValueFormatAttribute(format: string) =

    inherit Attribute()

    member att.Format = format

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

        open Freql.Core.Common.Types

        let tryGetAtIndex (values: string array) (i: int) =
            match i >= 0 && i < values.Length with
            | true -> Some values.[i]
            | false -> None

        //type Field = { Index: int; Value: obj }


        let createRecord<'T> (values: string list) =
            //let definition = RecordDefinition.Create<'T>()

            let getValue =
                values |> Array.ofList |> tryGetAtIndex


            let t = typeof<'T>

            let values =
                t.GetProperties()
                |> List.ofSeq
                |> List.mapi (fun i pi ->
                    // Look for format field.
                    let format =
                        match Attribute.GetCustomAttribute(pi, typeof<CsvValueFormatAttribute>) with
                        | att when att <> null -> Some <| (att :?> CsvValueFormatAttribute).Format
                        | _ -> None

                    let t =
                        SupportedType.FromType(pi.PropertyType)

                    match getValue i, t with
                    | Some v, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                    | Some v, SupportedType.Boolean -> bool.Parse v :> obj
                    | Some v, SupportedType.Byte -> Byte.Parse v :> obj
                    | Some v, SupportedType.Char -> v.[0] :> obj
                    | Some v, SupportedType.Decimal -> Decimal.Parse v :> obj
                    | Some v, SupportedType.Double -> Double.TryParse v :> obj
                    | Some v, SupportedType.DateTime ->
                        match format with
                        | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                        | None -> DateTime.Parse(v)
                        :> obj
                    | Some v, SupportedType.Float -> failwith "" // Double.TryParse v :> obj
                    | Some v, SupportedType.Guid ->
                        match format with
                        | Some f -> Guid.ParseExact(v, f)
                        | None -> Guid.Parse(v)
                        :> obj
                    | Some v, SupportedType.Int -> Int32.Parse v :> obj
                    | Some v, SupportedType.Long -> Int64.Parse v :> obj
                    | Some v, SupportedType.Short -> Int16.Parse v :> obj
                    | Some v, SupportedType.String -> v :> obj
                    | Some v, SupportedType.Option st ->
                        // TODO this could be tidied up/ cleaned up
                        // Note, an error in here will return None. This might not be correct?
                        try
                            match v |> String.IsNullOrWhiteSpace |> not, st with
                            | true, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                            | true, SupportedType.Boolean -> bool.Parse v |> Some :> obj
                            | true, SupportedType.Byte -> Byte.Parse v |> Some :> obj
                            | true, SupportedType.Char -> v.[0] |> Some :> obj
                            | true, SupportedType.Decimal -> Decimal.Parse v |> Some :> obj
                            | true, SupportedType.Double -> Double.TryParse v |> Some :> obj
                            | true, SupportedType.DateTime ->
                                match format with
                                | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                                | None -> DateTime.Parse(v)
                                |> Some
                                :> obj
                            | true, SupportedType.Float -> failwith "" // Double.TryParse v :> obj
                            | true, SupportedType.Guid ->
                                match format with
                                | Some f -> Guid.ParseExact(v, f)
                                | None -> Guid.Parse(v)
                                |> Some
                                :> obj
                            | true, SupportedType.Int -> Int32.Parse v |> Some :> obj
                            | true, SupportedType.Long -> Int64.Parse v |> Some :> obj
                            | true, SupportedType.Short -> Int16.Parse v |> Some :> obj
                            | true, SupportedType.String -> v |> Some :> obj
                            | _, SupportedType.Option _ -> failwith "Nested option types not supported in CSV."
                            | false, _ -> None :> obj
                        with
                        | _ -> None :> obj
                    | None, _ -> failwith "")

            let o =
                FSharpValue.MakeRecord(t, values |> Array.ofList)

            o :?> 'T

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
            | exn -> Error { Line = i + 1; Error = exn.Message })

    let splitResults<'T> (results: Result<'T, ParseError> list) =
        results
        |> List.fold
            (fun (ok, errors) item ->
                match item with
                | Ok i -> ok @ [ i ], errors
                | Error e -> ok, errors @ [ e ])
            ([], [])
