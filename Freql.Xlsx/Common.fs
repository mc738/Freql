namespace Freql.Xlsx

open System
open System.Text.RegularExpressions
open DocumentFormat.OpenXml
open DocumentFormat.OpenXml.Office2019.Excel.RichData2
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

    let getRows (worksheet: WorksheetPart) (lowerBound: uint32 option) (upperBound: uint32 option) =
        worksheet.Worksheet.Descendants<Row>()
        |> Seq.filter (fun r ->
            match lowerBound, upperBound with
            | Some lb, Some ub -> r.RowIndex >= UInt32Value lb && r.RowIndex <= UInt32Value ub
            | Some lb, None -> r.RowIndex >= UInt32Value lb
            | None, Some ub -> r.RowIndex <= UInt32Value ub
            | None, None -> true)

    let getRowRange (worksheet: WorksheetPart) (startIndex: uint32) (endIndex: uint32) =
        worksheet.Worksheet.Descendants<Row>()
        |> Seq.filter (fun r -> r.RowIndex >= UInt32Value startIndex && r.RowIndex <= UInt32Value endIndex)

    let getAllRows (worksheet: WorksheetPart) =
        worksheet.Worksheet.Descendants<Row>() :> seq<_>

    let getCell (worksheet: WorksheetPart) (cellRef: string) =
        worksheet.Worksheet.Descendants<Cell>()
        |> Seq.tryFind (fun c -> c.CellReference = StringValue cellRef)

    let getCellValue (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.map (fun c -> c.CellValue)

    let cellToString (cell: Cell) = cell.CellValue.Text

    let cellToBool (cell: Cell) =
        match cell.CellValue.TryGetBoolean() with
        | true, v -> Some v
        | false, _ -> None

    let cellToDecimal (cell: Cell) =
        match cell.CellValue.TryGetDecimal() with
        | true, v -> Some v
        | false, _ -> None
    
    let cellToDouble (cell: Cell) =
        match cell.CellValue.TryGetDouble() with
        | true, v -> Some v
        | false, _ -> None

    let cellToInt (cell: Cell) =
        match cell.CellValue.TryGetInt() with
        | true, v -> Some v
        | false, _ -> None

    let cellToDateTime (cell: Cell) =
        match cell.CellValue.TryGetDateTime() with
        | true, v -> Some v
        | false, _ -> None
        
    let cellToOADateTime (cell: Cell) =
        cellToDouble cell |> Option.map DateTime.FromOADate

    let cellToDateTimeOffset (cell: Cell) =
        match cell.CellValue.TryGetDateTimeOffset() with
        | true, v -> Some v
        | false, _ -> None

    let getCellValueAsString (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.map (fun c -> c.CellValue.Text)

    let getCellValueAsBool (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.bind cellToBool

    let getCellValueAsDecimal (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.bind cellToDecimal

    let getCellValueAsDouble (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.bind cellToDouble

    let getCellValueAsInt (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.bind cellToInt

    let getCellValueAsDateTime (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.bind cellToDateTime

    let getCellValueAsDateTimeOffset (worksheet: WorksheetPart) (cellRef: string) =
        getCell worksheet cellRef |> Option.bind cellToDateTimeOffset

    let getCellFromRow (row: Row) (columnName: string) =
        row.Descendants<Cell>()
        |> Seq.tryFind (fun c -> c.CellReference = StringValue $"{columnName}{row.RowIndex}")

    let getCellsFromRow (row: Row) = row.Descendants<Cell>() :> seq<_>

    let tryColumnNameToIndex (columnName: string) =
        let charValue (c: Char) =
            match Char.IsLetter c with
            | true -> (Char.ToUpper c |> int) - 65 |> Ok
            | false -> Error "Character is not a letter and not supported for column names."

        match columnName.Length with
        | 0 -> Error "Column name is blank."
        | 1 -> columnName[0] |> charValue

        | 2 ->
            match columnName[0] |> charValue, columnName[1] |> charValue with
            | Ok pv, Ok cv -> (pv + 1) * 26 + cv |> Ok
            | Error e, _ -> Error e
            | _, Error e -> Error e
        | 3 ->
            match columnName[0] |> charValue, columnName[1] |> charValue, columnName[2] |> charValue with
            | Ok opv, Ok pv, Ok cv -> ((opv + 1) * 26 * 26) + ((pv + 1) * 26) + cv |> Ok
            | Error e, _, _ -> Error e
            | _, Error e, _ -> Error e
            | _, _, Error e -> Error e
        | _ -> Error "Column name is too long"

    let columnNameToIndex (columnName: string) =
        match tryColumnNameToIndex columnName with
        | Ok v -> v
        | Error e -> failwith e

    let tryIndexToColumnName (index: int) =
        // Base on https://stackoverflow.com/questions/181596/how-to-convert-a-column-number-e-g-127-into-an-excel-column-e-g-aa
        // NOTE This could be refactored to be more optimized/cleaner.
        match index with
        | i when i > 16383 || i < 0 -> Error "Index out of bounds"
        | i when i >= 702 -> Ok [| 0; 1; 2 |]
        | i when i >= 26 -> Ok [| 0; 1 |]
        | i -> Ok [| 0 |]
        |> Result.map (fun a ->
            a
            |> Array.fold
                (fun (acc, cn) _ ->
                    let modulo = (cn - 1) % 26
                    (char (65 + modulo)) :: acc, (cn - modulo) / 26)
                ([], index + 1) // + 1 because excel cells are base 1 indexex.
            |> fst
            |> fun r -> String(r |> Array.ofList))

    let indexToColumnName (index: int) =
        match tryIndexToColumnName index with
        | Ok cn -> cn
        | Error e -> failwith e

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
