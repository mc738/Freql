namespace Freql.Tools.CodeGeneration

[<AutoOpen>]
module Impl =

    open System
    open Freql.Core.Utils.Extensions
        
    let createRecord<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (table: TableDetails<'TTable, 'TColumn>)
        =

        let fields =
            table.Columns
            |> List.map (fun cd ->
                ({ Name = settings.NameHandler cd |> fun n -> n.ToPascalCase()
                   Type = settings.TypeHandler settings.TypeReplacements cd
                   Initialization = settings.TypeInitHandler settings.TypeReplacements cd }
                : Records.RecordField))

        let createSql =
            [ "    static member CreateTableSql() = \"\"\""
              $"    {table.Sql}"
              "    \"\"\"" ]

        let selectFields =
            table.Columns
            |> List.map (fun cd -> $"          {table.OriginalName}.`{settings.NameHandler cd}`")
            |> String.concat $",{Environment.NewLine}    "

        let selectSql =
            [ "    static member SelectSql() = \"\"\""
              $"    SELECT"
              $"{selectFields}"
              $"    FROM {table.OriginalName}"
              "    \"\"\"" ]

        let tableName = $"    static member TableName() = \"{table.OriginalName}\""

        let name =
            table.ReplacementName
            |> Option.defaultValue table.OriginalName
            |> fun n -> n.ToPascalCase()
            
        let tgc = ({ Name = name }: TableGeneratorContext)

        ({ Name = name
           Fields = fields
           IncludeBlank = true
           AdditionMethods =
             [ yield! createSql
               ""
               yield! selectSql
               ""
               tableName
               match table.BespokeMethodsHandler tgc with
               | Some lines ->
                   ""

                   yield!
                       lines
                       |> List.map (fun l -> if String.IsNullOrWhiteSpace l |> not then $"    {l}" else l)
               | None -> () ]
           DocumentCommentLines =
             [ "/// <summary>"
               $"/// A record representing a row in the table `{table.OriginalName}`."
               "/// </summary>"
               "/// <remarks>"
               $"/// This record was generated via Freql.Tools on {DateTime.UtcNow}"
               "/// </remarks>" ] }
        : Records.Record)
        |> Records.create profile

    let indent value (text: string) = $"{String(' ', value * 4)}{text}"

    let indent1 text = indent 1 text

    let createBoilerPlate<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =
        [ yield! Header.lines
          ""
          $"namespace {profile.Namespace}"
          ""
          "open System"
          if settings.IncludeJsonAttributes then
              "open System.Text.Json.Serialization"
          yield! settings.Imports |> List.map (fun i -> $"open {i}")
          "" ]

    let createBespokeTopSection<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =
        [ match settings.BespokeTopSectionHandler { Profile = profile; Tables = tables } with
          | Some ls ->
              yield! ls
              ""
          | None -> () ]

    let createRecords<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =

        // Create the core record.
        let records =
            tables
            |> List.map (fun t -> createRecord profile settings t @ [ "" ])
            |> List.concat
            |> List.map indent1

        [ "/// <summary>"
          $"/// Records representing database bindings for `{profile.Name}`."
          "/// </summary>"
          "/// <remarks>"
          $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Tools."
          "/// </remarks>"
          $"[<RequireQualifiedAccess>]"
          $"module Records =" ]
        @ records


    // Generate records/code for insert etc.
    //
    // Need -
    // Configuration change
    // * Property for skip fields (i.e. id which could be auto increment and not needed on inserts)
    //  "operations": [
    //      {
    //          "name": "test",
    //          "tableFilter": "",
    //          "init":
    //      }
    //  ]
    //
    // Example:
    // let insertFoo (parameters: AddFooParameters) (context: MySqlContext) =
    //     context.insert(Records.FooRecord.TableName(), parameters)

    let generateAddParameters<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (table: TableDetails<'TTable, 'TColumn>)
        =
        let name =
            table.ReplacementName
            |> Option.defaultValue table.OriginalName
            |> fun n -> n.ToPascalCase()

        //let parametersRecords =
        table.Columns
        |> List.filter settings.InsertColumnFilter
        |> List.map (fun cd ->
            ({ Name = settings.NameHandler cd |> fun n -> n.ToPascalCase()
               Type = settings.TypeHandler settings.TypeReplacements cd
               Initialization = settings.TypeInitHandler settings.TypeReplacements cd }
            : Records.RecordField))
        |> fun f ->
            ({ Name = $"New{name}"
               Fields = f
               IncludeBlank = true
               AdditionMethods = []
               DocumentCommentLines =
                 [ "/// <summary>"
                   $"/// A record representing a new row in the table `{table.OriginalName}`."
                   "/// </summary>"
                   "/// <remarks>"
                   $"/// This record was generated via Freql.Tools on {DateTime.UtcNow}"
                   "/// </remarks>" ] }
            : Records.Record)
        |> Records.create profile


    let generateInsertOperation<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (table: TableDetails<'TTable, 'TColumn>)
        =

        let name =
            table.ReplacementName
            |> Option.defaultValue table.OriginalName
            |> fun n -> n.ToPascalCase()

        [ $"let insert{name} (context: {settings.ContextTypeName}) (parameters: Parameters.New{name}) ="
          $"    context.Insert(\"{table.OriginalName}\", parameters)"
          ""
          $"let tryInsert{name} (context: {settings.ContextTypeName}) (parameters: Parameters.New{name}) ="
          $"    context.TryInsert(\"{table.OriginalName}\", parameters)"  ]

    let generateSelectOperation<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (table: TableDetails<'TTable, 'TColumn>)
        =

        let name =
            table.ReplacementName
            |> Option.defaultValue table.OriginalName
            |> fun n -> n.ToPascalCase()

        [ "/// <summary>"
          $"/// Select a `Records.{name}` from the table `{table.OriginalName}`."
          $"/// Internally this calls `context.SelectSingleAnon&lt;Records.{name}&gt;` and uses Records.{name}.SelectSql()."
          $"/// The caller can provide extra string lines to create a query and boxed parameters."
          $"/// It is up to the caller to verify the sql and parameters are correct,"
          "/// this should be considered an internal function (not exposed in public APIs)."
          "/// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2..."
          "/// </summary>"
          "/// <remarks>"
          $"/// This function was generated via Freql.Tools on {DateTime.UtcNow}"
          "/// </remarks>"
          "/// <example>"
          "/// <code>"
          $"/// let result = select{name}Record ctx \"WHERE `field` = @0\" [ box `value` ]"
          "/// </code>"
          "/// </example>"
          $"let select{name}Record (context: {settings.ContextTypeName}) (query: string list) (parameters: obj list) ="
          $"    let sql = [ Records.{name}.SelectSql() ] @ query |> buildSql"
          $"    context.SelectSingleAnon<Records.{name}>(sql, parameters)"
          ""
          "/// <summary>"
          $"/// Internally this calls `context.SelectAnon&lt;Records.{name}&gt;` and uses Records.{name}.SelectSql()."
          $"/// The caller can provide extra string lines to create a query and boxed parameters."
          $"/// It is up to the caller to verify the sql and parameters are correct,"
          "/// this should be considered an internal function (not exposed in public APIs)."
          "/// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2..."
          "/// </summary>"
          "/// <remarks>"
          $"/// This function was generated via Freql.Tools on {DateTime.UtcNow}"
          "/// </remarks>"
          "/// <example>"
          "/// <code>"
          $"/// let result = select{name}Records ctx \"WHERE `field` = @0\" [ box `value` ]"
          "/// </code>"
          "/// </example>"
          $"let select{name}Records (context: {settings.ContextTypeName}) (query: string list) (parameters: obj list) ="
          $"    let sql = [ Records.{name}.SelectSql() ] @ query |> buildSql"
          $"    context.SelectAnon<Records.{name}>(sql, parameters)"
          
          "/// <summary>"
          $"/// Select a `Records.{name}` from the table `{table.OriginalName}`."
          $"/// Internally this calls `context.TrySelectSingleAnon&lt;Records.{name}&gt;` and uses Records.{name}.SelectSql()."
          $"/// The caller can provide extra string lines to create a query and boxed parameters."
          $"/// It is up to the caller to verify the sql and parameters are correct,"
          "/// this should be considered an internal function (not exposed in public APIs)."
          "/// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2..."
          "/// </summary>"
          "/// <remarks>"
          $"/// This function was generated via Freql.Tools on {DateTime.UtcNow}"
          "/// </remarks>"
          "/// <example>"
          "/// <code>"
          $"/// let result = trySelect{name}Record ctx \"WHERE `field` = @0\" [ box `value` ]"
          "/// </code>"
          "/// </example>"
          $"let trySelect{name}Record (context: {settings.ContextTypeName}) (query: string list) (parameters: obj list) ="
          $"    let sql = [ Records.{name}.SelectSql() ] @ query |> buildSql"
          $"    context.TrySelectSingleAnon<Records.{name}>(sql, parameters)"
          ""
          "/// <summary>"
          $"/// Internally this calls `context.TrySelectAnon&lt;Records.{name}&gt;` and uses Records.{name}.SelectSql()."
          $"/// The caller can provide extra string lines to create a query and boxed parameters."
          $"/// It is up to the caller to verify the sql and parameters are correct,"
          "/// this should be considered an internal function (not exposed in public APIs)."
          "/// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2..."
          "/// </summary>"
          "/// <remarks>"
          $"/// This function was generated via Freql.Tools on {DateTime.UtcNow}"
          "/// </remarks>"
          "/// <example>"
          "/// <code>"
          $"/// let result = trySelect{name}Records ctx \"WHERE `field` = @0\" [ box `value` ]"
          "/// </code>"
          "/// </example>"
          $"let trySelect{name}Records (context: {settings.ContextTypeName}) (query: string list) (parameters: obj list) ="
          $"    let sql = [ Records.{name}.SelectSql() ] @ query |> buildSql"
          $"    context.TrySelectAnon<Records.{name}>(sql, parameters)" ]

    let createParameters<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =

        // Create the core record.
        let records =
            tables
            |> List.map (fun t -> generateAddParameters profile settings t @ [ "" ])
            |> List.concat
            |> List.map indent1

        [ $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Tools."
          "[<RequireQualifiedAccess>]"
          "module Parameters =" ]
        @ records

    let createOperations<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =

        let buildSql =
            "let buildSql (lines: string list) = lines |> String.concat Environment.NewLine"

        // Create the core record.
        let ops =
            tables
            |> List.map (fun t ->
                [ yield! generateSelectOperation profile settings t
                  ""
                  yield! generateInsertOperation profile settings t
                  "" ])
            //
            |> List.concat
            |> List.map indent1

        [ $"/// Module generated on {DateTime.UtcNow} (utc) via Freql.Tools."
          "[<RequireQualifiedAccess>]"
          $"module Operations ="
          ""
          indent1 buildSql
          "" ]
        @ ops

    let createBespokeBottomSection<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =
        [ match settings.BespokeBottomSectionHandler { Profile = profile; Tables = tables } with
          | Some ls ->
              yield! ls
              ""
          | None -> () ]

    let generateCode<'TTable, 'TColumn>
        (profile: Configuration.GeneratorProfile)
        (settings: GeneratorSettings<'TTable, 'TColumn>)
        (tables: TableDetails<'TTable, 'TColumn> list)
        =

        [ createBoilerPlate profile settings tables
          createBespokeTopSection profile settings tables
          createRecords profile settings tables
          createParameters profile settings tables
          createOperations profile settings tables
          createBespokeBottomSection profile settings tables ]
        |> List.concat
        |> String.concat Environment.NewLine

