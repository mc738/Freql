namespace Freql.Sqlite.UnitTests

open Microsoft.VisualStudio.TestTools.UnitTesting

module Assembly =
    // Explicitly turn off test parallelization.
    // This is because tests could be dealing with the same in memory or local database.
    // Running in parallel could cause issues with memory exhaustion, set up/tear downs etc.
    // However, this does not mean unit tests can rely on previous tests.
    // Each one should be isolated!
    [<assembly: DoNotParallelize>]
    ()
