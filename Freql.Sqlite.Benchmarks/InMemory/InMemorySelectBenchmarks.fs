namespace Freql.Sqlite.Benchmarks.InMemory

open BenchmarkDotNet.Attributes
open Freql.Sqlite

type Foo = { Id: int; Name: string }

type InMemorySelectBenchmarks() =
    
    [<DefaultValue>]
    val mutable Ctx: SqliteContext

    [<GlobalSetup>]
    member this.Setup() =

        this.Ctx <- SqliteContext.Open(":memory:")
        
        this.Ctx.CreateTable<Foo>("foo") |> ignore

    [<Benchmark>]
    member this.InsertTest() =
        
        this.Ctx.Insert("foo", { Id = 1; Name = "Hello, World!" }) //|> ignore
        
