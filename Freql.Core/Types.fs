namespace Freql.Core

[<AutoOpen>]
module Types =

    open System
    open System.IO
    open System.Text.RegularExpressions

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

    type TransactionFailure =
        { Message: string
          Exception: Exception option }

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

        let sByteName = getName<sbyte>

        let uShortName = getName<uint16>

        let uIntName = getName<uint32>

        let uLongName = getName<uint64>

        let byteName = getName<byte>

        let shortName = getName<int16>

        let intName = getName<int>

        let longName = getName<int64>

        let singleName = getName<single>

        let doubleName = getName<double>

        let decimalName = getName<decimal>

        let charName = getName<char>

        let datetimeName = getName<DateTime>
        
        let timeSpanName = getName<TimeSpan>

        let guidName = getName<Guid>

        let stringName = getName<string>

        let blobName = getName<BlobField>

        let isOption (value: string) =
            Regex
                .Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)")
                .Success

        let getOptionType value =
            // Maybe a bit wasteful doing this twice.
            Regex
                .Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)")
                .Value

    /// An internal DU for representing supported types.
    [<RequireQualifiedAccess>]
    type SupportedType =
        | Boolean
        | Byte
        | SByte
        | Char
        | Decimal
        | Double
        | Single
        | UInt
        | Int
        | UShort
        | Short
        | ULong
        | Long
        | String
        | DateTime
        | TimeSpan
        | Guid
        | Blob
        | Option of SupportedType
        //| Json of Type

        static member TryFromName(name: String) =
            match name with
            | t when t = TypeHelpers.boolName -> Ok SupportedType.Boolean
            | t when t = TypeHelpers.byteName -> Ok SupportedType.Byte
            | t when t = TypeHelpers.sByteName -> Ok SupportedType.SByte
            | t when t = TypeHelpers.charName -> Ok SupportedType.Char
            | t when t = TypeHelpers.decimalName -> Ok SupportedType.Decimal
            | t when t = TypeHelpers.doubleName -> Ok SupportedType.Double
            | t when t = TypeHelpers.singleName -> Ok SupportedType.Single
            | t when t = TypeHelpers.intName -> Ok SupportedType.Int
            | t when t = TypeHelpers.uIntName -> Ok SupportedType.UInt
            | t when t = TypeHelpers.shortName -> Ok SupportedType.Short
            | t when t = TypeHelpers.uShortName -> Ok SupportedType.UShort
            | t when t = TypeHelpers.longName -> Ok SupportedType.Long
            | t when t = TypeHelpers.uLongName -> Ok SupportedType.ULong
            | t when t = TypeHelpers.stringName -> Ok SupportedType.String
            | t when t = TypeHelpers.datetimeName -> Ok SupportedType.DateTime
            | t when t = TypeHelpers.timeSpanName -> Ok SupportedType.TimeSpan
            | t when t = TypeHelpers.guidName -> Ok SupportedType.Guid
            | t when t = TypeHelpers.blobName -> Ok SupportedType.Blob
            | t when TypeHelpers.isOption t = true ->
                let ot = TypeHelpers.getOptionType t

                match SupportedType.TryFromName ot with
                | Ok st -> Ok(SupportedType.Option st)
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


    let tryGetArrayType (t: Type) =
        match t.IsArray with
        | true -> t.GetElementType() |> Some
        | false -> None

    let tryGetListType (t: Type) =
        match t.Name.Equals("FSharpList`1", StringComparison.Ordinal) with
        | true -> t.GenericTypeArguments |> Array.tryHead |> Option.orElse (Some typeof<obj>)
        | false -> None

    let tryGetSeqType (t: Type) =
        match t.Name.Equals("IEnumerable`1", StringComparison.Ordinal) with
        | true -> t.GenericTypeArguments |> Array.tryHead |> Option.orElse (Some typeof<obj>)
        | false -> None

    let typeIsOption (typeInfo: Type) = TypeHelpers.isOption typeInfo.FullName

    [<RequireQualifiedAccess>]
    type FSharpCollectionType =
        | Array of Type
        | List of Type
        | Seq of Type

        static member TryFromType(t: Type) =
            match tryGetArrayType t, tryGetListType t, tryGetSeqType t with
            | Some a, _, _ -> FSharpCollectionType.Array a |> Some
            | _, Some l, _ -> FSharpCollectionType.List l |> Some
            | _, _, Some s -> FSharpCollectionType.Seq s |> Some
            | None, None, None -> None

        member ct.GetInnerType() =
            match ct with
            | Array ``type``
            | List ``type``
            | Seq ``type`` -> ``type``
