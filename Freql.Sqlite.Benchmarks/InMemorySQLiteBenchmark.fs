namespace Freql.Sqlite.Benchmarks

open BenchmarkDotNet.Attributes
open Freql.Sqlite

type Foo = { Id: int; Value: string }

type InMemorySQLiteBenchmark() =

    [<DefaultValue>]
    val mutable Ctx: SqliteContext

    [<GlobalSetup>]
    member this.Setup() =

        this.Ctx <- SqliteContext.Open(":memory:")
        
        this.Ctx.CreateTable("foo") |> ignore

    member _.InsertTest() =
        
        
        ()
