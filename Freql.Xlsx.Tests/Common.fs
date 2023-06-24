module Freql.Xlsx.Tests.Common

open Microsoft.VisualStudio.TestTools.UnitTesting
open Freql.Xlsx

[<TestClass>]
type TestClass () =

    [<TestMethod>]
    member this.``columnNameToIndex 'A' name`` () =
        let columnName = "A"
        
        let expected = 0
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
        
    [<TestMethod>]
    member this.``columnNameToIndex 'Z' name`` () =
        let columnName = "Z"
        
        let expected = 25
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
        
    
    [<TestMethod>]
    member this.``columnNameToIndex 'AA' name`` () =
        let columnName = "AA"
        
        let expected = 26
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
        
    
    [<TestMethod>]
    member this.``columnNameToIndex 'AZ' name`` () =
        let columnName = "AZ"
        
        let expected = 51
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
    
    [<TestMethod>]
    member this.``columnNameToIndex 'BA' name`` () =
        let columnName = "BA"
        
        let expected = 52
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
        
    
    [<TestMethod>]
    member this.``columnNameToIndex 'BZ' name`` () =
        let columnName = "BZ"
        
        let expected = 77
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
    
    [<TestMethod>]
    member this.``columnNameToIndex 'ZZ' name`` () =
        let columnName = "ZZ"
        
        let expected = 701
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
    
    
    [<TestMethod>]
    member _.``columnNameToIndex 'AAA' name`` () =
        let columnName = "AAA"
        
        let expected = 702
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
    
    [<TestMethod>]
    member _.``columnNameToIndex 'AZZ' name`` () =
        let columnName = "AZZ"
        
        let expected = 1377
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)    
    
        
    [<TestMethod>]
    member _.``columnNameToIndex 'BAA' name`` () =
        let columnName = "BAA"
        
        let expected = 1378
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)    
        
    [<TestMethod>]
    member _.``columnNameToIndex 'XFD' name`` () =
        let columnName = "XFD"
        
        let expected = 16383
        
        let actual = columnNameToIndex columnName 
        
        Assert.AreEqual(expected, actual)
        