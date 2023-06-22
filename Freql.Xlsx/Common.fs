namespace Freql.Xlsx

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

    ()
