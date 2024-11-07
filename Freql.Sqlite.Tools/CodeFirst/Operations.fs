namespace Freql.Sqlite.Tools.CodeFirst

[<RequireQualifiedAccess>]
module OperationsGenerator =

    open Freql.Tools.CodeGeneration.Utils
    open Freql.Tools.CodeFirst.Core
    open Freql.Tools.CodeFirst.CodeGeneration

    [<RequireQualifiedAccess>]
    module Create =

        let generate (ctx: CodeGeneratorContext) (record: RecordInformation) =
            [ "match ctx.TryExecuteVerbatimNonQueryAnon(newRecord.InsertRecordSql(), newRecord.GetValues()) with"
              "| Ok _ ->"
              "// TODO update primary key if required" |> indent1
              "Ok newRecord" |> indent1
              "| Error failure -> failure.GetMessage() |> Error" ]

    [<RequireQualifiedAccess>]
    module Read =

        let generate (ctx: CodeGeneratorContext) (record: RecordInformation) = []

    [<RequireQualifiedAccess>]
    module Update =

        let generate (ctx: CodeGeneratorContext) (record: RecordInformation) = [
            "let updates ="
            "ops" |> indent1
            "|> List.choose (function" |> indent1
            "| RecordTrackingOperation.UpdateField update -> Some update" |> indent 2
            "| _ -> None)" |> indent 2
            ""
            "match"
            "ctx.TryExecuteVerbatimNonQueryAnon(" |> indent1
            "newRecord.UpdateRecordsSql(updates)," |> indent 2
            "updates |> List.map (fun update -> update.NewValue)" |> indent 2
            ")" |> indent1
            "with"
            "| Ok _ ->"
            "// TODO update primary key if required" |> indent1
            "Ok newRecord" |> indent1
            "| Error failure -> failure.GetMessage() |> Error"
        ]

    [<RequireQualifiedAccess>]
    module Delete =

        let generate (ctx: CodeGeneratorContext) (record: RecordInformation) = []





    let generate (ctx: CodeGeneratorContext) = []
