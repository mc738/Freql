namespace Freql.Sqlite.UnitTests
open System
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
[<DoNotParallelize>]
type TestClass () =

    [<TestMethod>]
    member this.TestMethodPassing1 () =
        Async.Sleep 5000 |> Async.RunSynchronously
        Assert.IsTrue(true)
        
    [<TestMethod>]
    member this.TestMethodPassing2 () =
        Async.Sleep 5000 |> Async.RunSynchronously
        Assert.IsTrue(true)
        
    
    [<TestMethod>]
    member this.TestMethodPassing3 () =
        Async.Sleep 5000 |> Async.RunSynchronously
        Assert.IsTrue(true)
    
    [<TestMethod>]
    member this.TestMethodPassing4 () =
        Async.Sleep 5000 |> Async.RunSynchronously
        Assert.IsTrue(true)
        
    [<TestMethod>]
    member this.TestMethodPassing5 () =
        Async.Sleep 5000 |> Async.RunSynchronously
        Assert.IsTrue(true);