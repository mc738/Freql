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

    let getRow (worksheet: WorksheetPart) =
        ()

    ()
