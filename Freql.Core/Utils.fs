module Freql.Core.Utils

open System
open System.Reflection
open System.Text.RegularExpressions

[<AutoOpen>]
module Extensions =

    type String with

        member str.ToSnakeCase() =
            str
            |> List.ofSeq
            |> List.fold
                (fun (acc, i) c ->
                    let newAcc =
                        match Char.IsUpper c, i = 0 with
                        | false, _ -> acc @ [ c ]
                        | true, true -> acc @ [ Char.ToLower(c) ]
                        | true, false -> acc @ [ '_'; Char.ToLower(c) ]

                    (newAcc, i + 1))
                ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))

        member str.ToPascalCase() =
            str
            |> List.ofSeq
            |> List.fold
                (fun (acc, i) c ->
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
                    (newAcc, i + 1))
                ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))

        member str.ToCamelCase() =
            match str.Length > 0 with
            | true -> $"{str.[0] |> Char.ToLower}{str.[1..]}"
            | false -> str


let (|SomeObj|_|) =
    let ty = typedefof<option<_>>

    fun (a: obj) ->
        let aty = a.GetType()
        let v = aty.GetProperty("Value")

        if aty.IsGenericType && aty.GetGenericTypeDefinition() = ty then
            if a = null then None else Some(v.GetValue(a, [||]))
        else
            None

module Attributes =

    let getAttributeFromProperty<'T when 'T :> Attribute> (propertyInfo: PropertyInfo) =
        match Attribute.GetCustomAttribute(propertyInfo, typeof<'T>) with
        | att when att <> null -> Some(att :?> 'T)
        | _ -> None


module Sorting =

    open System.Collections.Generic

    // This was initially generated via AI, so might need some testing.
    let topologicalSort<'T> (getName: 'T -> string) (getRelationshipKeys: 'T -> string list) (values: 'T list) =
        let graph = Dictionary<string, string list>()
        let inDegree = Dictionary<string, int>()

        // Initialize the graph and in-degree count
        for value in values do
            let name = getName value

            graph.[name] <- getRelationshipKeys value
            inDegree.[name] <- 0

        for value in values do
            for relationshipKey in getRelationshipKeys value do
                if inDegree.ContainsKey(relationshipKey) then
                    inDegree.[relationshipKey] <- inDegree.[relationshipKey] + 1
                else
                    inDegree.[relationshipKey] <- 1

        let sortedList = List<string>()

        // Queue for records with no incoming edges
        let queue = Queue<string>()

        for kvp in inDegree do
            if kvp.Value = 0 then
                queue.Enqueue(kvp.Key)

        while queue.Count > 0 do
            let node = queue.Dequeue()
            sortedList.Add(node)

            for neighbor in graph.[node] do
                inDegree.[neighbor] <- inDegree.[neighbor] - 1

                if inDegree.[neighbor] = 0 then
                    queue.Enqueue(neighbor)

        sortedList
        |> Seq.map (fun id -> values |> List.find (fun v -> getName v = id))
        |> Seq.toList
