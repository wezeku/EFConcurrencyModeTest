// [intro-sample]
using EFConcurrencyModeTest;
using NUnit.Framework;
using System.Reflection;

public class TestDbConcurrencyMode
{
    [Test]
    static void ConcurrencyModes()
    {
        var cmt = new ConcurrencyModeTester();
        var asm = Assembly.GetAssembly(typeof(MyDatabaseEntitiesType));
        var result = cmt.BadConcurrencyModes(asm, "MyEdmxFileNameWithoutExtension");
        Assert.IsEmpty(result, cmt.FormatBadConcurrencyModes(result));
    }
}
// [/intro-sample]

public class Example2
{
    // [intro-sample2]
    [Test]
    static void ConcurrencyModes2()
    {
        var cmt = new ConcurrencyModeTester();
        var asm = Assembly.GetAssembly(typeof(MyDatabaseEntitiesType));
        var result = cmt.BadConcurrencyModes(asm);
        Assert.IsEmpty(result, cmt.FormatBadConcurrencyModes(result));
    }
    // [/intro-sample2]
}

class MyDatabaseEntitiesType { } // Dummy class.
