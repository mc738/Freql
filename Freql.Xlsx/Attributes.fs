namespace Freql.Xlsx

open System

[<AutoOpen>]
module Attributes =


    type XlsxColumnNameAttribute(name: string) =

        inherit Attribute()

        member att.Name = name

    type XlsxDefaultValueAttribute<'T>(value: 'T) =

        inherit Attribute()


    type XlsxFormatAttribute(format: string) =
        inherit Attribute()

        member att.Format = format // format

        member val OADate = false with get, set


    type XlsxOptionsAttribute() =
        inherit Attribute()

        member val ColumnName: string = String.Empty with get, set

        member val OptionalErrorsToNone = false with get, set

        member val Format: string = String.Empty with get, set

        member val OADate = false with get, set
