module Freql.Xlsx.Tests.Common

open Microsoft.VisualStudio.TestTools.UnitTesting
open Freql.Xlsx

[<TestClass>]
type TestClass() =

    [<TestMethod>]
    member this.``columnNameToIndex 'A' name``() =
        let columnName = "A"

        let expected = 0

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``columnNameToIndex 'Z' name``() =
        let columnName = "Z"

        let expected = 25

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``columnNameToIndex 'AA' name``() =
        let columnName = "AA"

        let expected = 26

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member this.``columnNameToIndex 'AZ' name``() =
        let columnName = "AZ"

        let expected = 51

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``columnNameToIndex 'BA' name``() =
        let columnName = "BA"

        let expected = 52

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member this.``columnNameToIndex 'BZ' name``() =
        let columnName = "BZ"

        let expected = 77

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``columnNameToIndex 'ZZ' name``() =
        let columnName = "ZZ"

        let expected = 701

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member _.``columnNameToIndex 'AAA' name``() =
        let columnName = "AAA"

        let expected = 702

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member _.``columnNameToIndex 'AZZ' name``() =
        let columnName = "AZZ"

        let expected = 1377

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member _.``columnNameToIndex 'BAA' name``() =
        let columnName = "BAA"

        let expected = 1378

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member _.``columnNameToIndex 'XFD' name``() =
        let columnName = "XFD"

        let expected = 16383

        let actual = columnNameToIndex columnName

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``indexToColumnName 'A' name``() =
        let expected = "A"

        let actual = indexToColumnName 0

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``indexToColumnName 'Z' name``() =
        let expected = "Z"

        let actual = indexToColumnName 25
            
        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``indexToColumnName 'AA' name``() =
        let expected = "AA"

        let actual = indexToColumnName 26

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member this.``indexToColumnName 'AZ' name``() =
        let expected = "AZ"

        let actual = indexToColumnName 51

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``indexToColumnName 'BA' name``() =
        let expected = "BA"

        let actual = indexToColumnName 52

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member this.``indexToColumnName 'BZ' name``() =
        let expected = "BZ"

        let actual = indexToColumnName 77

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.``indexToColumnName 'ZZ' name``() =
        let expected = "ZZ"

        let actual = indexToColumnName  701

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member _.``indexColumnNameTo 'AAA' name``() =
        let expected = "AAA"

        let actual = indexToColumnName 702

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member _.``indexToColumnNameTo 'AZZ' name``() =
        let expected = "AZZ"

        let actual = indexToColumnName 1377

        Assert.AreEqual(expected, actual)


    [<TestMethod>]
    member _.``indexToColumnNameTo 'BAA' name``() =
        let expected = "BAA"

        let actual = indexToColumnName 1378

        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member _.``indexColumnNameTo 'XFD' name``() =
        let expected = "XFD"

        let actual = indexToColumnName 16383

        Assert.AreEqual(expected, actual)
