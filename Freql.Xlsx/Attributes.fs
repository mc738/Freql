namespace Freql.Xlsx

open System

[<AutoOpen>]
module Attributes =
    
    type XlsxColumnName(name: string) =

        inherit Attribute()

        member att.Name = name
    
    type XlsxDefaultValue<'T>(value: 'T) =
        
        inherit Attribute()

        

