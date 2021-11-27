 module Freql.Core.Utils

open System
open System.Text.RegularExpressions

[<AutoOpen>]
module Extensions =
    
    type String with
    
        member str.ToSnakeCase() =            
            str
            |> List.ofSeq
            |> List.fold (fun (acc, i) c ->
                let newAcc =
                    match Char.IsUpper c, i = 0 with
                    | false, _ -> acc @ [ c ]
                    | true, true -> acc @ [ Char.ToLower(c) ]
                    | true, false -> acc @ [ '_'; Char.ToLower(c) ]
                (newAcc, i + 1)) ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))
        
        member str.ToPascalCase() =
            str
            |> List.ofSeq
            |> List.fold (fun (acc, i) c ->
                let newAcc =
                    //match c =
                    match i - 1 >= 0 && str.[i - 1] = '_', i = 0, c = '_' with
                    | true, _, false -> acc @ [ Char.ToUpper c ]
                    | true, _, true -> acc
                    | false, false, false -> acc @ [ c ]
                    | false, true, _ -> acc @ [ Char.ToUpper c ]
                    | false, false, true -> acc
                        
                    
                    //match Char.IsUpper c, i = 0 with
                    //| false, _ -> acc @ [ c ]
                    //| true, true -> acc @ [ Char.ToLower(c) ]
                    //| true, false -> acc @ [ '_'; Char.ToLower(c) ]
                (newAcc, i + 1)) ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))
        
        member str.ToCamelCase() =
            match str.Length > 0 with
            | true -> $"{str.[0] |> Char.ToLower}{str.[1..]}" 
            | false -> str


let (|SomeObj|_|) =
  let ty = typedefof<option<_>>
  fun (a:obj) ->
    let aty = a.GetType()
    let v = aty.GetProperty("Value")
    if aty.IsGenericType && aty.GetGenericTypeDefinition() = ty then
      if a = null then None
      else Some(v.GetValue(a, [| |]))
    else None