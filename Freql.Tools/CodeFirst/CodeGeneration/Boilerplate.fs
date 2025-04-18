namespace Freql.Tools.CodeFirst.CodeGeneration

module Boilerplate =

    open Freql.Tools.CodeGeneration.Boilerplate

    let fileHeader (ctx: CodeGeneratorContext) (fileNamespace: string) (openFiles: string list) =
        [ yield! Header.lines
          $"namespace {fileNamespace}"
          ""
          "#nowarn \"6142001\""
          ""
          "open System"
          "open Freql.Tools.CodeFirst"
          "open Freql.Tools.CodeFirst.Core"
          "open Freql.Tools.CodeFirst.Core.Operations"
          yield!
              ctx.DatabaseSpecificProfile.OpenReferences
              |> List.map (fun openReference -> $"open {openReference}")
          yield! openFiles |> List.map (fun openFile -> $"open {openFile}") ]
