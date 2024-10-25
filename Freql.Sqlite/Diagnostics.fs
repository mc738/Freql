namespace Freql.Sqlite


module Diagnostics =
    
    open Freql.Core.Diagnostics
    
    let ``db.system`` = "sqlite"
    
    let getName operation target diagnosticOverrides =
        Activities.getName ``db.system`` operation target diagnosticOverrides
    
    ()

