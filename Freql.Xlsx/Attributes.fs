namespace Freql.Xlsx

open System

[<AutoOpen>]
module Attributes =
    
    type XlsxColumnName(name: string) =

        inherit Attribute()

        member att.Name = name
    

