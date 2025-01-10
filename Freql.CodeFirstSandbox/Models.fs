namespace Freql.CodeFirstSandbox

module Models =

    open Freql.Tools.CodeFirst.Core.Attributes

    type Foo =
        { [<PrimaryKey>]
          Id: string
          Name: string
          Value: string }

    type Bar =
        { [<PrimaryKey>]
          Id: int
          Bazs: Baz list }

    and Baz = { Id: string }

    type FooBarLink =
        { [<PrimaryKey; ForeignKey(typeof<Foo>)>]
          FooId: string
          [<PrimaryKey; ForeignKey(typeof<Bar>)>]
          BarId: string }

    let all =


        [ typeof<Foo>; typeof<Bar>; typeof<Baz>; typeof<FooBarLink> ]
