namespace Freql.Csv.Writer

open System
open System.IO

module Common =

    type CsvWriterSettings =
        { Separator: Separator
          WrapAllStrings: bool
          NewLine: string option }

        static member Default =
            { Separator = Separator.Comma
              WrapAllStrings = true
              NewLine = None }

    and Separator =
        | Comma
        | Tab
        | Semicolon
        | Pipe
        | Bespoke of string


    type CsvWriter(writer: TextWriter, settings: CsvWriterSettings) =

        interface IDisposable with
            member this.Dispose() =
                writer.Flush()
                writer.Dispose()

        static member Create(stream: Stream, settings: CsvWriterSettings) =
            new CsvWriter(new StreamWriter(stream), settings)

        static member CreateFromFileStream(fileStream: FileStream, settings: CsvWriterSettings) =
            new CsvWriter(new StreamWriter(fileStream), settings)

        member csv.Flush() = writer.Flush()

        member csv.WriteHeaders(headers: string list) =
            headers
            |> List.iteri (fun i h ->
                writer.Write h

                match i >= headers.Length - 1 with
                | true -> csv.NewLine()
                | false -> csv.WriteSeparator())

        member csv.Write(value: string, ?includeSeparator: bool, ?lastValueInRow: bool) =
            match settings.WrapAllStrings with
            | true when value[0] <> '"' -> writer.Write $"\"{value}\""
            | true -> writer.Write value // NOTE this assumes the value is already wrapped
            | false ->
                // TODO add check to see if value requires wrapping and delimiting.
                writer.Write value

            //writer.Write value

            match includeSeparator |> Option.defaultValue false with
            | true -> csv.WriteSeparator()
            | false -> ()

            match lastValueInRow |> Option.defaultValue false with
            | true -> csv.NewLine()
            | false -> ()

        member csv.WriteSeparator() =
            match settings.Separator with
            | Comma -> writer.Write ","
            | Tab -> writer.Write "\t"
            | Semicolon -> writer.Write ";"
            | Pipe -> writer.Write "|"
            | Bespoke s -> writer.Write s

        member csv.NewLine() =
            writer.Write(settings.NewLine |> Option.defaultValue Environment.NewLine)
