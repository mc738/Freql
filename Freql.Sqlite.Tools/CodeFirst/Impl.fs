namespace Freql.Sqlite.Tools.CodeFirst

[<AutoOpen>]
module Impl =

    open System
    open Freql.Tools.CodeFirst.Core
    open Freql.Tools.CodeFirst.CodeGeneration

    let createContext (types: Type list) =
        match Mapping.mapRecords types with
        | Ok records ->
            ({ Records = records
               NameNormalizationMode = NameNormalizationMode.PascalCase
               DatabaseSpecificProfile =
                 { ContextType = "SqliteContext"
                   OpenReferences = [ "Freql.Sqlite" ]
                   TopSection = TopSection.generate
                   TypeExtension = TypeExtensions.generate
                   OperationGenerator = OperationsGenerator.generate
                   CreateGenerator = OperationsGenerator.Create.generate
                   ReadGenerator = OperationsGenerator.Read.generate
                   UpdateGenerator = OperationsGenerator.Update.generate
                   DeleteGenerator = OperationsGenerator.Delete.generate
                   BottomSection = BottomSection.generate } }
            : CodeGeneratorContext)
        | Error errorValue -> failwith "todo"
