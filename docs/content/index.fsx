(*** hide ***)
#I "../../bin"
#I "../../packages/NUnit.2.6.3/lib"

(**
EFConcurrencyModeTest
=====================

EFConcurrencyModeTest helps you write unit tests (for e.g. NUnit) that test
whether optimistic concurrency for Entity Framework is enabled in your EDMX
files.

When using Entity Framework, the ConcurrencyMode attribute for properties 
mapped to rowversion (a.k.a. timestamp) database columns, should be set to 
ConcurrencyMode.Fixed, to enable optimistic concurrency.

Unfortunately, due to <a href="https://entityframework.codeplex.com/workitem/588">a bug in the Entity Framework tools</a>,
you have to do this manually. EFConcurrencyModeTest helps you write the tests
needed to make sure that you have done so.

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      EFConcurrencyModeTest can be <a href="https://nuget.org/packages/EFConcurrencyModeTest">installed from NuGet</a>:
      <pre>PM> Install-Package EFConcurrencyModeTest</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

F# Example
----------

This example shows how a simple NUnit test using EFConcurrencyModeTest looks in F#:

*)

(*** hide ***)
#r "EFConcurrencyModeTest"
#r "nunit.framework"
#r "System.XML.Linq"
type MyDatabaseEntitiesType = Dummy // To avoid red squigglies in sample.
(** *)

open EFConcurrencyModeTest
open NUnit.Framework
open System.Reflection

type TestDbConcurrencyMode () =
    [<Test>]
    static member ConcurrencyModes () =
        let cmt = ConcurrencyModeTester()
        let asm = Assembly.GetAssembly(typeof<MyDatabaseEntitiesType>)
        let result = cmt.BadConcurrencyModes(asm, "MyEdmxFileNameWithoutExtension")
        Assert.IsEmpty(result, cmt.FormatBadConcurrencyModes result)

(**
C# Example
----------

This example shows the same NUnit test in C#:

    [lang=csharp,file=../csharp/TestDbConcurrencyMode.cs,key=intro-sample]

Documentation
-------------

The above examples show just about all you have to know about the usage of EFConcurrencyModeTest.
There are a few variants of the BadConcurrencyModes function which are documented in 
the [API Reference](reference/index.html).

Copyright
---------

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [gh]: https://github.com/wezeku/EFConcurrencyModeTest
  [license]: https://github.com/wezeku/EFConcurrencyModeTest/blob/master/LICENSE.txt
*)
