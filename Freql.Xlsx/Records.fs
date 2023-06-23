namespace Freql.Xlsx

open System.Reflection
open DocumentFormat.OpenXml.Spreadsheet
open Microsoft.FSharp.Reflection


module Records =

    
    open Freql.Core.Common.Types

    type RecordProperty =
        { PropertyInfo: PropertyInfo
          Type: SupportedType
          Index: int
          ColumnName: string }
          //NumberStyle: NumberStyles option
          //Format: string option
          //Replacement: string option -> string option }

        static member Create(propertyInfo: PropertyInfo, index: int) =
            ({ PropertyInfo = propertyInfo
               Type = SupportedType.FromType(propertyInfo.PropertyType)
               Index = index
               ColumnName = "" 
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
                 
                 }: RecordProperty)
            
        //member rp.IntNumberStyle() =
        //    rp.NumberStyle
        //    |> Option.defaultValue NumberStyles.Integer
            
        //member rp.FloatNumberStyle() =
        //    rp.NumberStyle
        //    |> Option.defaultValue NumberStyles.Float
        
    let createRecord<'T> (properties: RecordProperty array) (row: Row) =

        let values =
            properties
            |> Array.map (fun rp ->
                //printfn $"*** {i}"
                // Look for format field.
                
                match getCellFromRow row rp.ColumnName, rp.Type with
                | Some v, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                | Some v, SupportedType.Boolean -> bool.Parse v :> obj
                | Some v, SupportedType.Byte -> Byte.Parse v :> obj
                | Some v, SupportedType.Char -> v.[0] :> obj
                | Some v, SupportedType.Decimal -> Decimal.Parse(v, rp.FloatNumberStyle()) :> obj
                | Some v, SupportedType.Double -> Double.Parse(v, rp.FloatNumberStyle()) :> obj
                | Some v, SupportedType.DateTime ->
                    match rp.Format with
                    | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                    | None -> DateTime.Parse(v)
                    :> obj
                | Some v, SupportedType.Float -> Single.Parse(v, rp.FloatNumberStyle()) // Double.TryParse v :> obj
                | Some v, SupportedType.Guid ->
                    match rp.Format with
                    | Some f -> Guid.ParseExact(v, f)
                    | None -> Guid.Parse(v)
                    :> obj
                | Some v, SupportedType.Int -> Int32.Parse(v, rp.IntNumberStyle()) :> obj
                | Some v, SupportedType.Long -> Int64.Parse(v, rp.IntNumberStyle()) :> obj
                | Some v, SupportedType.Short -> Int16.Parse(v, rp.IntNumberStyle()) :> obj
                | Some v, SupportedType.String -> v :> obj
                | Some v, SupportedType.Option st ->
                    // TODO this could be tidied up/ cleaned up
                    // Note, an error in here will return None. This might not be correct?
                    try
                        match v |> String.IsNullOrWhiteSpace |> not, st with
                        | true, SupportedType.Blob -> failwith "Blob types not supported in CSV."
                        | true, SupportedType.Boolean -> bool.Parse v |> Some :> obj
                        | true, SupportedType.Byte -> Byte.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                        | true, SupportedType.Char -> v.[0] |> Some :> obj
                        | true, SupportedType.Decimal -> Decimal.Parse(v, rp.FloatNumberStyle()) |> Some :> obj
                        | true, SupportedType.Double -> Double.Parse(v, rp.FloatNumberStyle()) |> Some :> obj
                        | true, SupportedType.DateTime ->
                            match rp.Format with
                            | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                            | None -> DateTime.Parse(v)
                            |> Some
                            :> obj
                        | true, SupportedType.Float -> Single.Parse(v, rp.FloatNumberStyle()) |> Some :> obj // Double.TryParse v :> obj
                        | true, SupportedType.Guid ->
                            match rp.Format with
                            | Some f -> Guid.ParseExact(v, f)
                            | None -> Guid.Parse(v)
                            |> Some
                            :> obj
                        | true, SupportedType.Int -> Int32.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                        | true, SupportedType.Long -> Int64.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                        | true, SupportedType.Short -> Int16.Parse(v, rp.IntNumberStyle()) |> Some :> obj
                        | true, SupportedType.String -> v |> Some :> obj
                        | _, SupportedType.Option _ -> failwith "Nested option types not supported in CSV."
                        | false, _ -> None :> obj
                    with
                    | _ -> None :> obj
                | None, _ -> failwith "")

        let o =
            FSharpValue.MakeRecord(typeof<'T>, values)

        o :?> 'T
    

