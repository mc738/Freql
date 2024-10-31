module Freql.Tools.CodeGeneration

open System
open Freql.Tools.CodeGeneration

[<RequireQualifiedAccess>]
module Modules =

    open Utils
    open Boilerplate

    type Parameters =
        { Name: string
          RequireQualifiedAccess: bool
          AutoOpen: bool
          SummaryCommentLines: string list
          AttributeLines: string list
          BaseIndent: int
          IndentContent: bool
          OpenReferences: string list
          Content: string list }

    let generate (parameters: Parameters) =
        [ match parameters.SummaryCommentLines.IsEmpty with
          | true -> ()
          | false ->
              "/// <summary>"
              yield! parameters.SummaryCommentLines |> List.map (fun scl -> $"/// {scl}")
              "/// </summary>"
          yield! Comments.freqlRemark DateTime.UtcNow
          yield! parameters.AttributeLines
          match parameters.RequireQualifiedAccess with
          | true -> "[<RequireQualifiedAccess>]"
          | false -> ()
          match parameters.AutoOpen with
          | true -> "[<AutoOpen>]"
          | false -> ()
          $"module {parameters.Name} ="
          yield! parameters.OpenReferences |> List.map (fun ref -> indent1 $"open {ref}")
          match parameters.IndentContent with
          | true -> yield! parameters.Content |> List.map indent1
          | false -> yield! parameters.Content ]
        |> List.map (indent parameters.BaseIndent)
