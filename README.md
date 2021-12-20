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
