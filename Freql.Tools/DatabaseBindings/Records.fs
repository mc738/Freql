namespace Freql.Tools.DatabaseBindings

module Records =

    open Freql.Core.Utils.Extensions

    type RecordField =
        { Name: string
          Type: string
          Initialization: string }

    type Record =
        { Name: string
          Fields: RecordField list
          IncludeBlank: bool
          AdditionMethods: string list
          DocumentCommentLines: string list }

    let create (profile: Configuration.GeneratorProfile) (record: Record) =
        let fields =
            record.Fields
            |> List.mapi (fun i rf ->
                let name =
                    rf.Name
                    |> fun n -> n.ToPascalCase()
                    |> fun n ->
                        match profile.IncludeJsonAttributes with
                        | true -> $"[<JsonPropertyName(\"{n.ToCamelCase()}\")>] {n}"
                        | false -> n
                    |> fun n -> $"{n}: {rf.Type}"

                match i with
                | 0 when record.Fields.Length = 1 -> $"    {{ {name} }}"
                | 0 -> $"    {{ {name}"
                | _ when i = record.Fields.Length - 1 -> $"      {name} }}"
                | _ -> $"      {name}")

        let blank =
            record.Fields
            |> List.mapi (fun i rf ->
                let name = rf.Name |> fun n -> n.ToPascalCase()

                let content = $"{name} = {rf.Initialization}"

                match i with
                | 0 when record.Fields.Length = 1 -> $"        {{ {content} }}"
                | 0 -> $"        {{ {content}"
                | _ when i = record.Fields.Length - 1 -> $"          {content} }}"
                | _ -> $"          {content}")
            |> fun r -> [ "    static member Blank() =" ] @ r

        match fields.Length with
        | 0 -> []
        //| 1 -> [ $"type {table.Name.ToPascalCase()} = {fields.[0].Trim()} }}" ]
        | _ ->
            [ yield! record.DocumentCommentLines
              $"type {record.Name.ToPascalCase()} ="
              yield! fields
              ""
              yield! blank
              match record.AdditionMethods.IsEmpty |> not with
              | true ->
                  ""
                  yield! record.AdditionMethods
              | false -> () ]