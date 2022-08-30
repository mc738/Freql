# Overview

All examples assume a data with the following structure:

## foo

* `id`
* `name`

## bar

* `id`
* `name`
* `foo_id`

and matching records:

```fsharp
type Foo =
    { Id: int
      Name: string }
      
type Bar =
    { Id: int
      Name: string
      FooId: int }
```

# Select

There are multiple ways to select data, depending on the circumstance.

The quickest is was with `SelectAnon` and `SelectSingleAnon`. 

These both take an `sql` string and `obj list`.

The query with parameterize the values based on their position in the list (0 based).

For example the first item will be `@0`, the second `@1` and so on.

MySql:

```fsharp
open Freql.MySql

// ...

let getFoos (context: MySqlContext) =
    let sql = "SELECT id, name FROM foo;"
    context.SelectAnon<Foo>(sql, [])

let getFoo (context: MySqlContext) (id: int) =
    let sql = "SELECT id, name FROM foo WHERE id = @0;"
    context.SelectSingleAnon<Foo>(sql, [ id ])
```


Sqlite:

```fsharp
open Freql.Sqlite

// ...
 
let getFoos (context: SqliteContext) =
    let sql = "SELECT id, name FROM foo;"
    context.SelectAnon<Foo>(sql, [])

let getFoo (context: SqliteContext) (id: int) =
    let sql = "SELECT id, name FROM foo WHERE id = @0;"
    context.SelectSingleAnon<Foo>(sql, [ id ])
```

# Insert

The sql for inserts are generated automatically based on the record provided.

MySql:

```fsharp
open Freql.MySql

// ...

let addFoo (context: MySqlContext) (foo: Foo) =
    context.Insert("foo", foo)
```

MySql:

```fsharp
open Freql.Sqlite

// ...

let addFoo (context: SqliteContext) (foo: Foo) =
    context.Insert("foo", foo)
```

For version v.0.4.2