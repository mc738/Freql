namespace Freql.Tools.CodeFirst.CodeGeneration

open Freql.Tools.CodeFirst.Core

[<AutoOpen>]
module Common =

    open System
    open Microsoft.FSharp.Core
    open Freql.Tools.CodeFirst.Core
    open Freql.Tools.CodeGeneration.Boilerplate
    open Freql.Tools.CodeGeneration
    open Freql.Tools.CodeGeneration.Utils

    type CodeGeneratorContext =
        { Records: RecordInformation list
          NameNormalizationMode: NameNormalizationMode
          DatabaseSpecificProfile: DatabaseSpecificProfile }

    and NameNormalizationMode =
        | PascalCase
        | SnakeCase

    and DatabaseSpecificProfile =
        { ContextType: string
          OpenReferences: string list
          TopSection: CodeGeneratorContext -> string list
          TypeExtension: CodeGeneratorContext -> RecordInformation -> string list
          OperationGenerator: CodeGeneratorContext -> string list
          CreateGenerator: CodeGeneratorContext -> RecordInformation -> string list
          ReadGenerator: CodeGeneratorContext -> RecordInformation -> string list
          UpdateGenerator: CodeGeneratorContext -> RecordInformation -> string list
          DeleteGenerator: CodeGeneratorContext -> RecordInformation -> string list
          BottomSection: CodeGeneratorContext -> string list }

    let internalCompilerMessage =
        [ """[<CompilerMessage("This module is intended for internal use in generated code. It is not intended to make up a public API. To remove this warning add #nowarn \"6142001\"","""
          """                  6142001)>]""" ]


    let generateModule
        (ctx: CodeGeneratorContext)
        (name: string)
        (summaryCommentLines: string list)
        (openReferences: string list)
        (content: string list)
        =
        let parameters =
            ({ Name = name
               RequireQualifiedAccess = true
               AutoOpen = false
               SummaryCommentLines = summaryCommentLines
               AttributeLines = internalCompilerMessage
               BaseIndent = 0
               IndentContent = true
               OpenReferences = openReferences
               Content = content }
            : Modules.Parameters)

        Modules.generate parameters
