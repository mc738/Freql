namespace Freql.Tools.CodeFirst.CodeGeneration

open System
open System.Collections.Generic
open System.IO

[<AutoOpen>]
module Impl =

    open System
    open Freql.Tools.CodeFirst.Core.Mapping

    type CodeFirstGeneratorSettings =
        { ProjectFile: string
          Namespace: string
          OutputMode: OutputMode }

    and [<RequireQualifiedAccess>] OutputMode =
        | SingleFile of SingleFileSettings
        | MultiFile of MultiFileSettings

    and SingleFileSettings = { OutputFilePath: string }

    and MultiFileSettings =
        { OutputDirectoryPath: string

        }

    module Xml =

        open Microsoft.Build.Construction

        let addFiles (path: string) =
            let root = ProjectRootElement.Open(path)


            //root.ImportGroups
            (*
            let t =
                root.ImportGroups
                |> Seq.tryFind (fun ig ->
                    // Test if this is a project element for 
                    ig.Children
                    |> Seq.exists (fun i -> i))
            
            
            root.Save()
            *)
            ""



        open System.Xml.Linq

    //let document


    let initializeMultiFileMode (settings: CodeFirstGeneratorSettings) (outputDirectoryPath: string) =
        Xml.addFiles settings.ProjectFile

        Directory.CreateDirectory outputDirectoryPath |> ignore

        // Create placeholder files. This will overwrite what already exists but this should be fine.
        File.WriteAllText(Path.Combine(outputDirectoryPath, "Tracking.fs"), $"namespace {settings.Namespace}")
        
        ()


    module SingleFile =

        let fileReferences (ctx: CodeGeneratorContext) =
            ctx.Records
            |> List.map (fun t -> t.Type.ReflectedType.ToString())
            |> List.distinct

        let build
            (ctx: CodeGeneratorContext)
            (settings: CodeFirstGeneratorSettings)
            (singleFileSettings: SingleFileSettings)
            =
            [ yield! fileReferences ctx |> Boilerplate.fileHeader ctx settings.Namespace
              ""
              yield! Extensions.generateModule ctx
              yield! Tracking.generateRecordComparisonCode ctx
              yield! Operations.generateCreateModule ctx
              yield! Operations.generateReadModule ctx
              yield! Operations.generateUpdateModule ctx
              yield! Operations.generateDeleteModule ctx
              yield! Operations.generateDatabaseOperationsModule ctx
              
              yield! Context.generate ctx

              ]
            |> String.concat Environment.NewLine
            |> fun r -> File.WriteAllText(singleFileSettings.OutputFilePath, r)

    let runCodeGeneration (ctx: CodeGeneratorContext) (settings: CodeFirstGeneratorSettings) =
        match settings.OutputMode with
            | OutputMode.SingleFile singleFileSettings -> SingleFile.build ctx settings singleFileSettings
            | OutputMode.MultiFile multiFileSettings ->
                initializeMultiFileMode settings ""
                failwith "todo"
