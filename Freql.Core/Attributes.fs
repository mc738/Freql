namespace Freql.Core


module Attributes =
    
    open System
    
    /// <summary>
    /// An attribute for declaring a specific field is sensitive and should be masked.
    /// For example for Monitoring purposes
    /// </summary>
    /// <example>
    /// <code>
    /// {
    ///     [&lt;SensitiveData&gt;] Foo: string
    /// }
    /// </code>
    /// <code>
    /// {
    ///     [&lt;SensitiveData(mask = "***")&gt;] Foo: string
    /// }
    /// </code>
    /// <code>
    /// {
    ///     [&lt;SensitiveData(excludeFromMonitoring = true)&gt;] Foo: string
    /// }
    /// </code>
    /// </example>
    [<AttributeUsage(AttributeTargets.Property)>]
    type SensitiveDataAttribute(?mask: string, ?excludeFromMonitoring) =
        
        inherit Attribute()
        
    
    /// <summary>
    /// Attribute for declaring a specific column name for a field to be read from.
    /// </summary> 
    [<AttributeUsage(AttributeTargets.Property)>]
    type MappedFieldAttribute(name: string) =

        inherit Attribute()

        member att.Name = name
    
    /// Mapped record attribute. Not used.
    [<Obsolete "This attribute isn't currently used and will likely be removed in the future">]
    type MappedRecordAttribute(name: string) =

        inherit Attribute()

        member att.Name = name

