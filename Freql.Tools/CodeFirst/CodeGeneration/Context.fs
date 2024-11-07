namespace Freql.Tools.CodeFirst.CodeGeneration

module Context =

    open Freql.Core.Utils
    open Freql.Tools.CodeGeneration
    open Freql.Tools.CodeGeneration.Utils

    let generateDisposeMethod (ctx: CodeGeneratorContext) =
        [ "interface IDisposable with" |> indent1
          "member this.Dispose() =" |> indent 2
          "if autoComment then this.Commit() |> ignore // TODO Handle better" |> indent 3 ]

    let generateOperationMethods (ctx: CodeGeneratorContext) =
        [ "member this.Insert<'T>(value: 'T) =" |> indent1
          "match box value with" |> indent 2
          yield!
              ctx.Records
              |> List.map (fun record ->
                  $"| :? {record.Name} as {record.Name.ToCamelCase()} -> {record.Name.ToCamelCase()} |> DatabaseOperations.OperationType.Insert{record.Name} |> operations.Add |> Ok"
                  |> indent 2)
          "| _ -> Error $\"Type {typeof<'T>.FullName}\ not supported\"" |> indent 2
          ""
          "member this.Update<'T>(value: 'T) =" |> indent1
          "match box value with" |> indent 2
          yield!
              ctx.Records
              |> List.map (fun record ->
                  $"| :? {record.Name} as {record.Name.ToCamelCase()} -> {record.Name.ToCamelCase()} |> DatabaseOperations.OperationType.Update{record.Name} |> operations.Add |> Ok"
                  |> indent 2)
          "| _ -> Error $\"Type {typeof<'T>.FullName}\ not supported\"" |> indent 2
          ""
          "member this.Delete<'T>(value: 'T) =" |> indent1
          "match box value with" |> indent 2
          yield!
              ctx.Records
              |> List.map (fun record ->
                  $"| :? {record.Name} as {record.Name.ToCamelCase()} -> {record.Name.ToCamelCase()} |> DatabaseOperations.OperationType.Delete{record.Name} |> operations.Add |> Ok"
                  |> indent 2)
          "| _ -> Error $\"Type {typeof<'T>.FullName}\ not supported\"" |> indent 2 ]

    let generate (ctx: CodeGeneratorContext) =
        [ $"type CodeFirstContext(ctx: {ctx.DatabaseSpecificProfile.ContextType}, ?commitOnClose: bool) ="
          "let autoComment = commitOnClose |> Option.defaultValue false" |> indent1
          "let operations = ResizeArray<DatabaseOperations.OperationType>()" |> indent1
          ""
          yield! generateDisposeMethod ctx
          ""
          "member this.HasPendingCommits() = operations.Count > 0" |> indent1
          ""
          "member this.Commit() =" |> indent1
          "DatabaseOperations.``commit database changes`` ctx operations" |> indent 2
          "|> Result.map (fun _ -> operations.Clear())" |> indent 2
          ""
          yield! generateOperationMethods ctx
          "" ]
