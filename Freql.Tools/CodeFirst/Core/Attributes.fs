namespace Freql.Tools.CodeFirst.Core

module Attributes =

    open System
    open System.Reflection
    open Freql.Core.Utils.Attributes
    
    [<AttributeUsage(AttributeTargets.Property)>]
    type PrimaryKeyAttribute() =

        inherit Attribute()

    [<AttributeUsage(AttributeTargets.Property)>]
    type ForeignKeyAttribute(otherType: Type) =

        inherit Attribute()

        member fka.OtherType = otherType

    [<AttributeUsage(AttributeTargets.Property)>]
    type IndexAttribute(unique: bool) =

        inherit Attribute()
        
        member ia.Unique = unique

    [<AttributeUsage(AttributeTargets.Property)>]
    type IgnoreFieldAttribute() =
        inherit Attribute()

    
    let getPrimaryKeyAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<PrimaryKeyAttribute> propertyInfo

    let getForeignKeyAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<ForeignKeyAttribute> propertyInfo

    let getIndexAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<IndexAttribute> propertyInfo

    let getIgnoreAttribute (propertyInfo: PropertyInfo) =
        getAttributeFromProperty<IgnoreFieldAttribute> propertyInfo
