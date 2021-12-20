module Freql.Core.Common

open System
open System.IO
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection
open Freql.Core.Utils

[<AutoOpen>]
module Types =

    /// A blob field.
    type BlobField =
        { Value: Stream }
        
        static member Empty() = { Value = Stream.Null }
        
        static member FromStream(stream: Stream) = { Value = stream }

        static member FromBytes(ms: MemoryStream) = BlobField.FromStream(ms)

        member blob.ToBytes() =
            match blob.Value with
            | :? MemoryStream -> (blob.Value :?> MemoryStream).ToArray()
            | _ ->
                use ms = new MemoryStream()
                blob.Value.CopyTo(ms)
                ms.ToArray()

    (*
    /// A json file, stored as a blob in the database.
    type JsonField =
        { Json: string  }

        static member FromStream(stream: Stream) =
            Utf8JsonReader(stream)

        static member Serialize<'T>(value: 'T) = { Json =  JsonSerializer.Serialize value }

        member json.Deserialize<'T>() =
            JsonSerializer.Deserialize<'T> json.Json



        //static member FromStream<'T>
    *)

    module private TypeHelpers =
        let getName<'T> = typeof<'T>.FullName

        let typeName (t: Type) = t.FullName

        let boolName = getName<bool>

        let uByteName = getName<uint8>

        let uShortName = getName<uint16>

        let uIntName = getName<uint32>

        let uLongName = getName<uint64>

        let byteName = getName<byte>

        let shortName = getName<int16>

        let intName = getName<int>

        let longName = getName<int64>

        let floatName = getName<float>

        let doubleName = getName<double>

        let decimalName = getName<decimal>

        let charName = getName<char>

        let timestampName = getName<DateTime>

        let uuidName = getName<Guid>

        let stringName = getName<string>

        let blobName = getName<BlobField>
        
        let isOption (value: string) =
            Regex.Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)").Success
            
        let getOptionType value =
            // Maybe a bit wasteful doing this twice.
            Regex.Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)").Value

    /// An internal DU for representing supported types.
    [<RequireQualifiedAccess>]
    type SupportedType =
        | Boolean
        | Byte
        | Char
        | Decimal
        | Double
        | Float
        | Int
        | Short
        | Long
        | String
        | DateTime
        | Guid
        | Blob
        | Option of SupportedType
        //| Json of Type

        static member TryFromName(name: String) =
            match name with
            | t when t = TypeHelpers.boolName -> Ok SupportedType.Boolean
            | t when t = TypeHelpers.byteName -> Ok SupportedType.Byte
            | t when t = TypeHelpers.charName -> Ok SupportedType.Char
            | t when t = TypeHelpers.decimalName -> Ok SupportedType.Decimal
            | t when t = TypeHelpers.doubleName -> Ok SupportedType.Double
            | t when t = TypeHelpers.floatName -> Ok SupportedType.Float
            | t when t = TypeHelpers.intName -> Ok SupportedType.Int
            | t when t = TypeHelpers.shortName -> Ok SupportedType.Short
            | t when t = TypeHelpers.longName -> Ok SupportedType.Long
            | t when t = TypeHelpers.stringName -> Ok SupportedType.String
            | t when t = TypeHelpers.timestampName -> Ok SupportedType.DateTime
            | t when t = TypeHelpers.uuidName -> Ok SupportedType.Guid
            | t when t = TypeHelpers.blobName -> Ok SupportedType.Blob
            | t when TypeHelpers.isOption t = true ->
                let ot = TypeHelpers.getOptionType t
                match SupportedType.TryFromName ot with
                | Ok st -> Ok (SupportedType.Option st)
                | Error e -> Error e
            | _ -> Error $"Type `{name}` not supported."

        static member TryFromType(typeInfo: Type) =
            SupportedType.TryFromName(typeInfo.FullName)

        static member FromName(name: string) =
            match SupportedType.TryFromName name with
            | Ok st -> st
            | Error _ -> SupportedType.String

        static member FromType(typeInfo: Type) =
            SupportedType.FromName(typeInfo.FullName)

module Mapping =

    /// Attribute for declaring a specific column name for a field to be read from. 
    type MappedFieldAttribute(name: string) =

        inherit Attribute()

        member att.Name = name

    /// Mapped record attribute. Not used.
    type MappedRecordAttribute(name: string) =

        inherit Attribute()

        member att.Name = name

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
                |> List.mapi
                    (fun i pi ->
                        { FieldName = pi.Name
                          MappingName = pi.Name.ToSnakeCase()
                          Index = i
                          Type = SupportedType.FromType(pi.PropertyType) })

            { Fields = fields; Type = t }

        /// Get the fields as a map with the index as key.
        member map.GetIndexedMap() =
            map.Fields
            |> List.map (fun f -> f.Index, f)
            |> Map.ofList

        /// Get the fields as a map with the name as key.
        member map.GetNamedMap() =
            map.Fields
            |> List.map (fun f -> f.MappingName, f)
            |> Map.ofList

    /// F# record building class.
    type RecordBuilder() =

        /// Create a F# record from a list of FieldValue's and a type.
        static member Create<'T>(values: FieldValue list) =
            let t = typeof<'T>

            let v =
                values
                |> List.sortBy (fun v -> v.Index)
                |> List.map (fun v -> v.Value)
                |> Array.ofList

            let o = FSharpValue.MakeRecord(t, v)

            o :?> 'T
    
    /// Map parameters of type 'T to a Map<string,obj> based on the MappedField's mapping name.
    /// This is a useful helper.
    let mapParameters<'T> (mappedObj: MappedObject) (parameters: 'T) =
        // TODO This could be worth adding a test to.
        mappedObj.Fields
        |> List.sortBy (fun p -> p.Index)
        |> List.map
            (fun f ->
                let v =
                    mappedObj
                        .Type
                        .GetProperty(f.FieldName)
                        .GetValue(parameters)

                f.MappingName, v)
        |> Map.ofList