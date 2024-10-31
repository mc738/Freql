namespace Freql.Tools.CodeFirst.CodeGeneration

open Freql.Tools.CodeFirst.Core
open Microsoft.FSharp.Reflection

module Extensions =

    open System
    open Freql.Tools.CodeGeneration
    open Freql.Tools.CodeGeneration.Utils

    let generateGetPrimaryKey (recordType: Type) =
        FSharpType.GetRecordFields recordType
        |> Array.choose (fun field ->
            //match Attributes.getPrimaryKeyAttribute field, 
            
            ())
        
        ()
    
    
    let generateTypeExtensions (record: RecordInformation) =
        [ $"type {record.Name} with"
          "member this.GetPrimaryKey() =" |> indent1
          "failwith \"todo\"" |> indent 2
          "" ]

    let generateModule (ctx: CodeGeneratorContext) =
        ({ Name = "Extensions"
           RequireQualifiedAccess = false
           AutoOpen = false
           SummaryCommentLines =
             [ "A collection of extensions on domain models to help with code-first binds."
               "Extensions are added so domain models do not need to be polluted with methods only needed for bindings." ]
           AttributeLines = internalCompilerMessage
           BaseIndent = 0
           IndentContent = true
           OpenReferences = []
           Content = ctx.Records |> List.collect generateTypeExtensions }
        : Modules.Parameters)
        |> Modules.generate
