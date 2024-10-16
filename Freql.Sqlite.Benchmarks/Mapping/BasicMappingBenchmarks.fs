namespace Freql.Sqlite.Benchmarks.Mapping

open BenchmarkDotNet.Attributes

type BasicMappingBenchmarks() =

     [<GlobalCleanup>]
     member this.Setup() =
         
         ()