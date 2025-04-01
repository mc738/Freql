// For more information see https://aka.ms/fsharp-console-apps

open System
open System.IO
open Freq.MySql.Tools.Core.DataStore
open Freql.CodeFirstSandbox
open Freql.CodeFirstSandbox.Models
open Freql.Tools.CodeFirst.Core
open Freql.Sqlite.Tools.CodeFirst

module Test =

    let mappedRecords () =
        let t = typeof<Foo>

        let i = 0
        printfn $"{t.ReflectedType.ToString()}"
        ()


module OperationsTest =

    open System

    type Foo = { Id: int; Value: string }

    type Operations<'T> =
        { Handler1: 'T -> string
          Handler2: 'T -> string }

    type OperationCollection = { Operations: Map<string, obj> }


    let run _ =

        let fooOperations =
            ({ Handler1 =
                fun foo ->
                    printfn $"Id is {foo.Id}"
                    ""
               Handler2 =
                 fun foo ->
                     printfn $"Value is {foo.Value}"
                     "" }
            : Operations<Foo>)

        let operations =
            { Operations = [ typeof<Foo>.Name, box fooOperations ] |> Map.ofList }

        let myType = typeof<Foo>

        let i = unbox 1

        //let i = 1 :?> myType

        let ops =

            operations.Operations.TryFind myType.Name
            |> Option.map (fun ops -> ops :?> Operations<Foo>)

        ()

open OperationsTest

module Fetch =

    let fetchFoo (record: Foo) : Foo option =
        // TODO database specific code to fetch a `Foo` record based on a `Foo` input.

        // This will use the primary key.

        None

module Update =

    let ``update Foo record`` (newFoo: Foo) =
        // Fetch the existing foo record
        match Fetch.fetchFoo newFoo with
        | Some oldFoo ->
            // Compare the existing and new record to only update changed fields.
            ()
        | None ->
            // A completely new record, so return a Add operation.
            ()


        ()

type CodeFirstContext(autoCommit: bool) =
    let fetchedObjects = ResizeArray<obj>()

    let uncommitedChanges = ResizeArray()

    interface IDisposable with

        member this.Dispose() =
            if autoCommit then
                this.Commit()

    // Other disposal code.

    member _.Commit() =

        ()

    member _.GetFooRecord() = ()

    member _.GetFooRecords() = ()

    member _.AddFooRecord(foo: Foo) =

        ()

    member _.UpdateFooRecord(foo: Foo) = ()

    member _.DeleteFooRecord() =

        ()


module TestGenerate =

    open Freql.Tools.CodeFirst.CodeGeneration

    let run _ =

        let path = "/home/max/Projects/dotnet/Freql/Freql.CodeFirstSandbox/CodeGen.fs"

        let mappedRecords = Mapping.mapRecords Models.all

        let ctx = createContext Models.all


        runCodeGeneration
            ctx
            { Namespace = "Freql.CodeFirstSandbox"
              ProjectFile = "/home/max/Projects/dotnet/Freql/Freql.CodeFirstSandbox/Freql.CodeFirstSandbox.fsproj"
              OutputMode =
                OutputMode.SingleFile
                    { OutputFilePath = "/home/max/Projects/dotnet/Freql/Freql.CodeFirstSandbox/CodeGen.fs" } (*
                 OutputMode.MultiFile
                    { OutputDirectoryPath = "/home/max/Projects/dotnet/Freql/Freql.CodeFirstSandbox/CodeFirst" }
                
                *) }

//codeGen Models.all |> fun r -> File.WriteAllText(path, r)

module TypesTest =

    let run _ =
        let l = typeof<(Foo * string) list>
        let a = typeof<Foo array>
        let s = typeof<string seq>

        let at = a.GetElementType()

        ()

let ``this is a test`` =
    
    let i = id
    id.GetType().Name
    
    
let name = ``this is a test``

//TypesTest.run ()
TestGenerate.run ()

printfn "Hello from F#"
