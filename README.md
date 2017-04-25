EFConcurrencyModeTest
=====================

EFConcurrencyModeTest helps you write unit tests (for e.g. NUnit) that test
whether optimistic concurrency for Entity Framework is enabled in your EDMX
files.

<a href="http://wezeku.github.io/EFConcurrencyModeTest" target="_blank">See the documentation for more information.</a>

<a href="http://blog.wezeku.com/2014/03/10/concurrencymode-unit-tests-for-entity-framework-edmx-files/">Associated blog post.</a>

How to build
------------

For some reason, on certain computers you can only run "build.cmd" from the 
"Developer Command Prompt" (under the Visual Studio folder of the start menu).

Building in Visual Studio 2017
----------------------------------

As of April 2017, Visual Studio 2017 doesn't install the
\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v15.0\FSharp\Microsoft.FSharp.Targets
file correctly (at least not on my computer). If this is the case, you need to create
that file and insert the following content:

```
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildProgramFiles32)\Microsoft SDKs\F#\4.1\Framework\v4.0\Microsoft.FSharp.Targets" />
</Project>
```

<a href="https://github.com/Microsoft/visualfsharp/issues/2340">See Visual F# issue #2340 for more information.</a>
