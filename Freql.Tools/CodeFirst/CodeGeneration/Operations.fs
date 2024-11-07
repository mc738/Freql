namespace Freql.Tools.CodeFirst.CodeGeneration

open System
open Freq.MySql.Tools.Core.DataStore
open Freql.Tools.CodeFirst.Core
open Freql.Tools.CodeGeneration


module Operations =

    open System
    open Freql.Tools.CodeGeneration.Utils
    open Freql.Tools.CodeGeneration.Boilerplate

    let databaseOperationType () =
        [ "type DatabaseOperation =" ]

        ()

    let generateCreateFunction (ctx: CodeGeneratorContext) (record: RecordInformation) =
        let databaseSpecificCode =
            ctx.DatabaseSpecificProfile.CreateGenerator ctx record |> List.map indent1

        [ yield! Comments.freqlRemark DateTime.UtcNow
          $"let ``create {record.Name} Record`` (ctx: {ctx.DatabaseSpecificProfile.ContextType}) (newRecord: {record.Name}) : Result<{record.Name}, string> ="
          match databaseSpecificCode.IsEmpty with
          | true ->
              $"failwith \"TODO - Implement ``create {record.Name} Record`` function\""
              |> indent1
          | false -> yield! databaseSpecificCode
          "" ]

    let generateReadFunction (ctx: CodeGeneratorContext) (record: RecordInformation) =
        [ yield! Comments.freqlRemark DateTime.UtcNow
          $"let ``read {record.Name} Record`` (ctx: {ctx.DatabaseSpecificProfile.ContextType}) (newRecord: {record.Name}) : {record.Name} option ="
          $"failwith \"TODO - Implement ``read {record.Name} Record`` function\""
          |> indent1
          "" ]

    let generateUpdateFunction (ctx: CodeGeneratorContext) (record: RecordInformation) =
        let databaseSpecific =
            ctx.DatabaseSpecificProfile.UpdateGenerator ctx record |> List.map (indent 2)

        [ yield! Comments.freqlRemark DateTime.UtcNow
          $"let ``update {record.Name} Record`` (ctx: {ctx.DatabaseSpecificProfile.ContextType}) (newRecord: {record.Name}) : Result<{record.Name}, string> ="
          $"match Read.``read {record.Name} Record`` ctx newRecord with" |> indent1
          "| Some oldRecord ->" |> indent1
          $"let ops = RecordComparisons.``compare {record.Name} records`` oldRecord newRecord"
          |> indent 2

          match databaseSpecific.IsEmpty with
          | true -> "failwith \"\"" |> indent 2
          | false ->
              ""
              yield! databaseSpecific
          $"| None -> Create.``create {record.Name} Record`` ctx newRecord" |> indent1
          //$"failwith \"TODO - Implement ``update {recordType.Name} Record`` function\""
          //|> indent1
          "" ]

    let generateDeleteFunction (ctx: CodeGeneratorContext) (record: RecordInformation) =
        [ yield! Comments.freqlRemark DateTime.UtcNow
          $"let ``delete {record.Name} Record`` (ctx: {ctx.DatabaseSpecificProfile.ContextType}) (newRecord: {record.Name}) : Result<unit, string> ="
          $"failwith \"TODO - Implement ``delete {record.Name} Record`` function\""
          |> indent1
          "" ]

    let generateDatabaseOperations (ctx: CodeGeneratorContext) =
        [ yield! Comments.freqlRemark DateTime.UtcNow
          "type OperationType = "
          yield!
              ctx.Records
              |> List.collect (fun record ->
                  [ $"| Insert{record.Name} of {record.Name}" |> indent1
                    $"| Update{record.Name} of {record.Name}" |> indent1
                    $"| Delete{record.Name} of {record.Name}" |> indent1 ])
          ""
          yield! Comments.freqlRemark DateTime.UtcNow
          $"let ``commit database changes`` (ctx: {ctx.DatabaseSpecificProfile.ContextType}) (operations: OperationType seq) : Result<unit, string> ="
          "ctx.TryExecuteInTransaction(fun t ->" |> indent1
          "    operations" |> indent1
          "    |> Seq.fold (fun state op ->" |> indent1
          "        match state, op with" |> indent1
          "        | Error _, _ -> state" |> indent1
          yield!
              ctx.Records
              |> List.mapi (fun i r ->
                  let eol =
                      match i = ctx.Records.Length - 1 with
                      | true -> ") (Ok ()))"
                      | false -> ""

                  [ $"        | Ok _, Insert{r.Name} record -> Create.``create {r.Name} Record`` t record |> Result.map ignore // TODO handle better - see code gen"
                    |> indent1
                    $"        | Ok _, Update{r.Name} record -> Update.``update {r.Name} Record`` t record |> Result.map ignore // TODO handle better - see code gen"
                    |> indent1
                    $"        | Ok _, Delete{r.Name} record -> Delete.``delete {r.Name} Record`` t record{eol}"
                    |> indent1 ])
              |> List.concat
          "|> Result.mapError (fun err -> err.Message)" |> indent1
          "" ]

    let generateModule
        (ctx: CodeGeneratorContext)
        (name: string)
        (summaryCommentLines: string list)
        (content: string list)
        =
        ({ Name = name
           RequireQualifiedAccess = true
           AutoOpen = false
           SummaryCommentLines = summaryCommentLines
           AttributeLines = internalCompilerMessage
           BaseIndent = 0
           IndentContent = true
           OpenReferences = [ "Extensions" ]
           Content = content }
        : Modules.Parameters)
        |> Modules.generate

    let generateCreateModule (ctx: CodeGeneratorContext) =
        ctx.Records
        |> List.collect (generateCreateFunction ctx)
        |> generateModule ctx "Create" []

    let generateReadModule (ctx: CodeGeneratorContext) =
        ctx.Records
        |> List.collect (generateReadFunction ctx)
        |> generateModule ctx "Read" []

    let generateUpdateModule (ctx: CodeGeneratorContext) =
        ctx.Records
        |> List.collect (generateUpdateFunction ctx)
        |> generateModule ctx "Update" []

    let generateDeleteModule (ctx: CodeGeneratorContext) =
        ctx.Records
        |> List.collect (generateDeleteFunction ctx)
        |> generateModule ctx "Delete" []

    let generateDatabaseOperationsModule (ctx: CodeGeneratorContext) =
        generateDatabaseOperations ctx |> generateModule ctx "DatabaseOperations" []
