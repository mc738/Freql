namespace Freql.Sqlite.Tools.CodeFirst


[<RequireQualifiedAccess>]
module OperationsGenerator =

    open Freql.Tools.CodeFirst.CodeGeneration

    let generateCommitOperation (ctx: CodeGeneratorContext) =
        [ "ctx.TryExecuteInTransaction(fun t ->"
          "    operations"
          "    |> Seq.fold (fun state op ->"
          "        match operation with"
          yield!
              ctx.Records
              |> List.collect (fun r -> [
                  $"        | Insert{r.Name} record -> Create.``create {r.Name} Record`` t record"
                  $"        | Update{r.Name} record -> Update.``update {r.Name} Record`` t record"
                  $"        | Delete{r.Name} record -> Delete.``delete {r.Name} Record`` t record"
              ])
          "        "
          "        Ok ()) (Ok ()))"
          "|> Result.mapError (fun err -> err.Message)" ]

    let generate (ctx: CodeGeneratorContext) = []
