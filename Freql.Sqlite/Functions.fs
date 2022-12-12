namespace Freql.Sqlite

open System.Text.RegularExpressions
open Microsoft.Data.Sqlite

module Functions =
    
    
    let regexp (connection: SqliteConnection) =
        connection.CreateFunction("regexp", fun (pattern: string, input: string) -> Regex.IsMatch(input, pattern))

