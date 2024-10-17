namespace Freql.Core

/// Mapping functionality.
module Mapping =
    
    open System
    open System.Reflection
    open Microsoft.FSharp.Reflection
    open Utils
    open Attributes
    
    /// A indexed field value.
    type FieldValue = { Index: int; Value: obj }

    /// A mapped field, with a name, a mapping name, a index and supported type.
    type MappedField =
        { FieldName: string
          MappingName: string
          Index: int
          Type: SupportedType }

        member field.CreateValue(value: obj) = { Index = field.Index; Value = value }

    /// A mapped object, a collection of mapped fields and `Type`.
    type MappedObject =
        { Fields: MappedField list
          Type: Type }

        /// Create a mapped object from type 'T.
        /// This will check for MappedField attributes.
        /// If not found, the field names will be converted to `snake_case`.
        static member Create<'T>() =
            let t = typeof<'T>

            let fields =
                t.GetProperties()
                |> List.ofSeq
                |> List.fold
                    (fun (acc, i) pi ->
                        let newAcc =
                            match Attribute.GetCustomAttribute(pi, typeof<MappedFieldAttribute>) with
                            | att when att <> null ->
                                let mfa = att :?> MappedFieldAttribute

                                // TODO check if supported type.

                                // TODO handle blobs and unhandled property types.

                                acc
                                @ [ { FieldName = pi.Name
                                      MappingName = mfa.Name
                                      Index = i
                                      Type = SupportedType.FromType(pi.PropertyType) } ]
                            | _ ->
                                acc
                                @ [ { FieldName = pi.Name
                                      MappingName = pi.Name.ToSnakeCase()
                                      Index = i
                                      Type = SupportedType.FromType(pi.PropertyType) } ]

                        (newAcc, i + 1))
                    ([], 0)
                |> fun (r, _) -> r

            { Fields = fields; Type = t }

        /// Create a mapped object from type 'T without checking for MappedField attributes.
        /// Unless there is a specific reason to use this, MappedObject.Create<'T> is probably better.
        static member CreateNoAtt<'T>() =
            let t = typeof<'T>

            let fields =
                t.GetProperties()
                |> List.ofSeq
                |> List.mapi (fun i pi ->
                    { FieldName = pi.Name
                      MappingName = pi.Name.ToSnakeCase()
                      Index = i
                      Type = SupportedType.FromType(pi.PropertyType) })

            { Fields = fields; Type = t }

        /// Get the fields as a map with the index as key.
        member map.GetIndexedMap() =
            map.Fields |> List.map (fun f -> f.Index, f) |> Map.ofList

        /// Get the fields as a map with the name as key.
        member map.GetNamedMap() =
            map.Fields |> List.map (fun f -> f.MappingName, f) |> Map.ofList

    /// F# record building class.
    type RecordBuilder() =

        /// Create a F# record from a list of FieldValue's and a type.
        static member Create<'T>(values: FieldValue list, ?bindingFlags: Reflection.BindingFlags) =
            let t = typeof<'T>

            let v =
                values
                |> List.sortBy (fun v -> v.Index)
                |> List.map (fun v -> v.Value)
                |> Array.ofList

            let o =
                match bindingFlags with
                | Some bf -> FSharpValue.MakeRecord(t, v, bf)
                | None -> FSharpValue.MakeRecord(t, v)

            o :?> 'T

    /// Map parameters of type 'T to a Map<string,obj> based on the MappedField's mapping name.
    /// This is a useful helper.
    let mapParameters<'T> (mappedObj: MappedObject) (parameters: 'T) =
        // TODO This could be worth adding a test to.
        mappedObj.Fields
        |> List.sortBy (fun p -> p.Index)
        |> List.map (fun f ->
            let v = mappedObj.Type.GetProperty(f.FieldName).GetValue(parameters)

            f.MappingName, v)
        |> Map.ofList

    [<RequireQualifiedAccess>]
    type BindingType =
        | Default
        | AllowPrivate

        member bt.ToFlags() =
            match bt with
            | Default -> BindingFlags.Default
            | AllowPrivate -> BindingFlags.Public ||| BindingFlags.NonPublic
