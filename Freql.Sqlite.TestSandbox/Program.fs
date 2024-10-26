open System.IO
open Freql.Sqlite

module ``deferred results development test 30-11-23`` =

    type TestRecord = { Id: string }

    let initialize (path: string) =
        match File.Exists path with
        | true -> SqliteContext.Open path
        | false ->
            let ctx = SqliteContext.Create path

            ctx.ExecuteSqlNonQuery "CREATE TABLE test_data (id TEXT)" |> ignore

            [ 0..1000 ]
            |> List.iter (fun i ->
                ctx.ExecuteVerbatimNonQueryAnon("INSERT INTO test_data (id) VALUES (@0)", [ $"item_{i}" ])
                |> ignore)

            ctx


    let run (path: string) =

        use ctx = initialize path

        let nonDeferred = ctx.Select<TestRecord> "test_data" |> List.take 5
        
        let deferred = ctx.DeferredSelect<TestRecord> "test_data" |> Seq.take 5
        
        let deferred = ctx.DeferredSelect<TestRecord> "test_data" |> Seq.skip 5 |> Seq.take 5


        nonDeferred |> List.iter (fun r -> printfn $"Non deferred - {r.Id}")
        deferred |> Seq.iter (fun r -> printfn $"Deferred - {r.Id}")

module ``Diagnostics test 26-10-24`` =
    
    open OpenTelemetry
    open OpenTelemetry.Trace
    
    type Foo =
        {
            Id: int
            Value: string
        }
    
    let run (path: string) =
        
        let traceProvider =
            Sdk
                .CreateTracerProviderBuilder()
                .AddSource("Freql.Sqlite.SqliteContextTelemetry")
                .AddConsoleExporter()
                .Build()
        
        
        use ctx = SqliteContext.Create(path)
        
        ctx.CreateTable<Foo>("foo") |> ignore
        
        ctx.Insert("foo", { Id = 1; Value = "Hello, World!" })
        
        let _ = ctx.TrySelect<Foo>("foo", true)
        traceProvider.ForceFlush() |> ignore
       
        traceProvider.Dispose()  
    
    ()

//``deferred results development test 30-11-23``.run "C:\\ProjectData\\Freql\\deferred_query_test.db"
``Diagnostics test 26-10-24``.run "/run/media/max/Shared/ProjectData/Freql/test.db"
