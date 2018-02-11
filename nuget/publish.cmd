@for %%f in (..\bin\*.nupkg) do @..\.nuget\NuGet.exe push %%f -source nuget.org
