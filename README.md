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

## Freql.App

`Freql.App` wraps tools for various databases into a command line app.

Database information stored in a `json` configuration file.

Configuration example:

```json
[
    {
        "name": "[database name]",
        "type": "[database type]",
        "connectionString": "[connection string]",
        "generatorProfiles": [
            {
                "name": "[profile name]",
                "outputPath": "[output file]",
                "namespace": "[namespace]",
                "moduleName": "[module name]",
                "includeJsonAttributes": true,
                "nameSuffix": "[name suffix]",
                "typeReplacements": [
                    {
                        "matchValue": "[match value]",
                        "matchType": "[match type]",
                        "replacementValue": "[new value]",
                        "replacementInitValue": "[new initialization value]"
                    }
                ],
                "tableNameReplacements": [
                    {
                        "name": "[column name]",
                        "replacementName": "[new name]"
                    }
                ]
            }
        ]
    }
]
```

An example showing how to call then code generation (`gen`) command:

```shell
$ [app path] gen -c "[configuration path]" -d [database name] -p [profile name]
```


## Compiler messages/warnings

`Freql` uses various warnings and compiler messages, often to mark what is for internal use.

The messages all have the following format:

* `614` - prefix
* `0-9` - the specific part/project 
* `000-999` - the specific number

For projects the following is used:

* `0` - General
* `1` - `Freql.Core`
* `2` - `Freql.Tools`

### General messages

General messages are used throughout `Freql`. 
They can appear in any project.

#### Internal use (6140001)

```fsharp
[<CompilerMessage("This module is intended for internal use. To remove this warning add #nowarn \"6140001\"", 6140001)>]
```
This message signifies a module is intended for internal use and should not be seen as part of the public API.
It can also mean the module might not have documentation generated for it.

A module might be marked with this if it is used in automated tests but would otherwise be private.