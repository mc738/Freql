namespace Freql.Tools.CodeFirst

open System
open System.Reflection

module Attribute =

    [<AttributeUsage(AttributeTargets.Property)>]
    type PrimaryKeyAttribute() =

        inherit Attribute()

    [<AttributeUsage(AttributeTargets.Property)>]
    type ForeignKeyAttribute(otherType: Type) =

        inherit Attribute()

    [<AttributeUsage(AttributeTargets.Property)>]
    type IndexAttribute(?unique: bool) =

        inherit Attribute()

    [<AttributeUsage(AttributeTargets.Property)>]        
    type IgnoreField() =
        inherit Attribute()
