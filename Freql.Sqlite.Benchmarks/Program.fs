

open System
open System.IO
open System.Reflection
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Running
open Freql.Sqlite.Benchmarks

printfn $"{Environment.GetCommandLineArgs()}"
printfn $"{Assembly.GetEntryAssembly().FullName}"

//let d = InMemorySQLiteBenchmark()

//d.Setup()

//d.InsertTest()

printfn "Enter a benchmark name:"

printf "> "
let name = Console.ReadLine()

let basePath = $"/home/max/Data/benchmarks/dotnet/freql/sqlite/{DateTime.UtcNow:yyyyMMdd}/"

let path = Path.Combine(basePath, name)

Directory.CreateDirectory(path) |> ignore

let customConfig = DefaultConfig.Instance.WithArtifactsPath(path)

BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()).Run(Environment.GetCommandLineArgs(), customConfig) |> ignore

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"