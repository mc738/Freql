namespace Freql.Tools.CodeGeneration

module Utils =
    
    open System
    
    let indent value (text: string) = $"{String(' ', value * 4)}{text}"

    let indent1 text = indent 1 text
    
    let wrapInArray (indentCount: int) (lines: string list) =
        lines
        |> List.mapi (fun i line ->
            let openBlock =
                match i = 0 with
                | true -> indent indentCount "[ "
                | false -> indent indentCount "  "
            let closeBlock =
                match i = lines.Length - 1 with
                | true -> " ]"
                | false -> ""
                
            $"{openBlock}{line}{closeBlock}")
        
        
        

