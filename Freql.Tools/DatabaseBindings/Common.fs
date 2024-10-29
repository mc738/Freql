namespace Freql.Tools.DatabaseBindings

[<AutoOpen>]
module Common =

    open System
    open System.Text.RegularExpressions
   
    [<RequireQualifiedAccess>]
    type MatchType =
        | Regex of string
        | String of string

        member mt.Test(value: string) =
            match mt with
            | Regex pattern -> Regex.IsMatch(value, pattern)
            | String str -> String.Equals(value, str, StringComparison.Ordinal)

    type TypeReplacement =
        { Match: MatchType
          ReplacementType: string
          Initialization: string option }

        static member Create(config: Configuration.TypeReplacementConfiguration) =
            { Match =
                match config.MatchType with
                | "regex" -> MatchType.Regex config.MatchValue
                | _ -> MatchType.String config.MatchValue
              ReplacementType = config.ReplacementValue
              Initialization = Some config.ReplacementInitValue }

        member tr.Attempt(name: string, typeString: string) =
            match tr.Match.Test name with
            | true -> tr.ReplacementType
            | false -> typeString

        member tr.AttemptInitReplacement(name: string, initValue: string) =
            match tr.Initialization, tr.Match.Test name with
            | Some init, true -> init
            | _ -> initValue

    type GeneratorSettings<'TTable, 'TColumn> =
        {
            Imports: string list
            IncludeJsonAttributes: bool
            TypeReplacements: TypeReplacement list
            TypeHandler: TypeReplacement list -> 'TColumn -> string
            TypeInitHandler: TypeReplacement list -> 'TColumn -> string
            NameHandler: 'TColumn -> string
            InsertColumnFilter: 'TColumn -> bool
            ContextTypeName: string
            /// <summary>
            /// A handler to generate database engine specific code that will appear at the top of output file.
            /// This is useful for generating utility functions etc. that could be used in other modules,
            /// such as in BespokeMethodsHandlers and additional methods.
            /// </summary>
            BespokeTopSectionHandler: GeneratorContext<'TTable, 'TColumn> -> string list option
            /// <summary>
            /// A handler to generate database engine specific code that will appear at the bottom of output file.
            /// This is useful for generating helper and extension functions based on the generated code.
            /// </summary>
            BespokeBottomSectionHandler: GeneratorContext<'TTable, 'TColumn> -> string list option
        }
        
    and GeneratorContext<'TTable, 'TColumn> = { Profile: Configuration.GeneratorProfile; Tables: TableDetails<'TTable, 'TColumn> list }

    and TableDetails<'TTable, 'TColumn> =
        { OriginalName: string
          ReplacementName: string option
          Sql: string
          Table: 'TTable
          Columns: 'TColumn list
          BespokeMethodsHandler: TableGeneratorContext -> string list option }

    and TableGeneratorContext = { Name: string }

