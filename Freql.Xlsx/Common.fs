namespace Freql.Xlsx

open System
open System.Text.RegularExpressions
open DocumentFormat.OpenXml
open DocumentFormat.OpenXml.Spreadsheet

[<AutoOpen>]
module Common =

    open DocumentFormat.OpenXml.Packaging

    let exec<'T> (fn: SpreadsheetDocument -> 'T) (isEditable: bool) (path: string) =
        use doc = SpreadsheetDocument.Open(path, isEditable)

        fn doc

    let stringValue (str: string) = StringValue str

    let getSheet (name: string) (doc: SpreadsheetDocument) =
        doc.WorkbookPart.Workbook.Descendants<Sheet>()
        |> Seq.tryFind (fun s -> s.Name = StringValue name)

    let getSheetById (id: string) (doc: SpreadsheetDocument) =
        doc.WorkbookPart.Workbook.Descendants<Sheet>()
        |> Seq.tryFind (fun s -> s.Id = StringValue id)

    let getWorksheet (sheet: Sheet) (doc: SpreadsheetDocument) =
        doc.WorkbookPart.GetPartById(sheet.Id) :?> WorksheetPart


    let readCell (worksheet: WorksheetPart) (column: string) (row: int) =
        //workSheet.Worksheet.r

        ()

    let getRow (worksheet: WorksheetPart) (index: uint32) =
        worksheet.Worksheet.Descendants<Row>()
        |> Seq.tryFind (fun r -> r.RowIndex = UInt32Value index)

    let getRowRange (worksheet: WorksheetPart) (startIndex: uint32) (endIndex: uint32) =
        worksheet.Worksheet.Descendants<Row>()
        |> Seq.filter (fun r -> r.RowIndex >= UInt32Value startIndex && r.RowIndex <= UInt32Value endIndex)

    let getRows (worksheet: WorksheetPart) =
        worksheet.Worksheet.Descendants<Row>() :> seq<_>

    let getCell (worksheet: WorksheetPart) (cellRef: string) =
        worksheet.Worksheet.Descendants<Cell>()
        |> Seq.tryFind (fun c -> c.CellReference = StringValue cellRef)

    let getCellValue (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.map (fun c -> c.CellValue)


    let getCellValueAsString (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.map (fun c -> c.CellValue.Text)

    let getCellValueAsBool (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef
        |> Option.bind (fun c ->
            match c.CellValue.TryGetBoolean() with
            | true, v -> Some v
            | false, _ -> None)

    let getCellValueAsDecimal (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef
        |> Option.bind (fun c ->
            match c.CellValue.TryGetDecimal() with
            | true, v -> Some v
            | false, _ -> None)

    let getCellFromRow (row: Row) (columnName: string) =
        row.Descendants<Cell>()
        |> Seq.tryFind (fun c -> c.CellReference = StringValue "")

    let getCellsFromRow (row: Row) = row.Descendants<Cell>() :> seq<_>

    let getRowIndex (cellName: string) =
        let r = Regex(@"\d+")
        let m = r.Match(cellName)

        match m.Success with
        | true -> Some m.Value
        | false -> None
        |> Option.bind (fun v ->
            match UInt32.TryParse v with
            | true, r -> Some r
            | false, _ -> None)

    let getColumnName (cellName: string) =
        let r = Regex(@"\d+")
        let m = r.Match(cellName)

        match m.Success with
        | true -> Some m.Value
        | false -> None


    ()
