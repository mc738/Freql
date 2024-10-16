namespace Freql.Sqlite.Benchmarks.InMemory

open BenchmarkDotNet.Attributes
open Freql.Sqlite

type Foo = { Id: int; Value: string }

type InMemorySelectBenchmarks() =
    
    [<DefaultValue>]
    val mutable Ctx: SqliteContext

    [<GlobalSetup>]
    member this.Setup() =

        this.Ctx <- SqliteContext.Open(":memory:")
        
        this.Ctx.CreateTable("foo") |> ignore

    member _.InsertTest() =
        
        
        ()
