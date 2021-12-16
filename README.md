# Freql

`freql` is a collection for library for working with various databases in `FSharp`.

It has similarities to ORM's, however is focuses around records.

Because records are immutable, object tracking is not needed
(and isn't very `f#` like anyway).

Instead `freql` focuses on reducing boiler plate database and mapping code 
while still being flexible.

The core libraries can be use to easily access various types of database. 
They are designed to be used in apps for data access.

Core features include:

* Support for Sqlite, MySql, SqlServer (partial)
* Record mapping via generics.
* `option` value support for nullable columns.
* Typed and untyped functions for flexibility.
* Transactions.
* Run raw and parameterize sql.


## Tools

The tool libraries are for working on databases. This can include:

* Code generation.
* Database difference checking.
* Database migrations.
