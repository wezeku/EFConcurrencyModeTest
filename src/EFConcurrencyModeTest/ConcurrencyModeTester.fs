namespace EFConcurrencyModeTest

open System
open System.Linq
open System.Reflection
open System.Text.RegularExpressions
open System.Xml.Linq


/// Class that stores an Entity, Property pair.
/// They represent a table and a column in your database.
type EntityProperty =
    { Entity   : string
      Property : string
    }


/// Class used to test ConcurrencyMode settings in EDMX files.
type ConcurrencyModeTester() =
    // EDMX namespaces.
    static member EdmxNs = "http://schemas.microsoft.com/ado/2009/11/edmx"
    static member CsdlNs = "http://schemas.microsoft.com/ado/2009/11/edm"
    static member SsdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"

    /// Names of the types used by the database for row versioning.
    /// The default value is [|"timestamp"; "rowversion"|].
    member val RowVersionTypes = [|"timestamp"; "rowversion"|] with set, get

    /// Regex patterns of the column names used for row versioning. Not needed if column
    /// types like rowversion or timestamp are used.
    /// The default is empty.
    member val ConcurrencyColumnNamePatterns = [||] with set, get

    /// Find bad ConcurrencyMode settings in a CSDL and SSDL element pair.
    ///
    /// ## Parameters
    ///  - `csdlElement` - CSDL element.
    ///  - `ssdlElement` - SSDL element.
    ///
    /// ## Return Value
    /// An EntityProperty array with all properties which have a bad ConcurrencyMode
    /// setting.
    member o.BadConcurrencyModes(csdlElement : XElement, ssdlElement : XElement) =
        let filterProperties (xElem : XElement) namespace_ filter = 
            set [for ent in xElem.Elements(XName.Get("EntityType", namespace_)) do
                    let tableName = ent.Attribute(XName.Get("Name")).Value
                    let props = ent.Elements(XName.Get("Property", namespace_))
                                   .Where(fun p -> filter p)
                    yield! [for p in props -> (tableName, p.Attribute(XName.Get("Name")).Value)]
                ]

        // Make a set of all concurrency checked fields from CSDL.
        let isConcurrent (property : XElement) =
            let cMode = property.Attribute(XName.Get("ConcurrencyMode"))
            cMode <> null && cMode.Value = "Fixed"
        let csdlConcurrentFields = filterProperties csdlElement ConcurrencyModeTester.CsdlNs isConcurrent
    
        // Make a set of all ROWVERSION fields from SSDL.
        let isRowVersion (property : XElement) =
            let propType = property.Attribute(XName.Get("Type")).Value
            let propName = property.Attribute(XName.Get("Name")).Value
            o.RowVersionTypes |> Array.exists ((=) propType)
            || o.ConcurrencyColumnNamePatterns |> Array.exists (fun i -> Regex.IsMatch(propName, i))
        let ssdlRowVersionedFields = filterProperties ssdlElement ConcurrencyModeTester.SsdlNs isRowVersion

        ssdlRowVersionedFields - csdlConcurrentFields
        |> Seq.map (fun (e, p) -> { Entity = e; Property = p })
        |> Seq.toArray


    /// Find bad ConcurrencyMode settings in a CSDL and SSDL document pair.
    ///
    /// ## Parameters
    ///  - `csdlDocument` - CSDL document.
    ///  - `ssdlDocument` - SSDL document.
    ///
    /// ## Return Value
    /// An EntityProperty array with all properties which have a bad ConcurrencyMode
    /// setting.
    member o.BadConcurrencyModes(csdlDocument : XDocument, ssdlDocument : XDocument) =
        o.BadConcurrencyModes(csdlDocument.Root, ssdlDocument.Root)


    /// Find bad ConcurrencyMode settings in a single EDMX file embedded as a resource in an
    /// assembly (this is how Visual Studio stores EDMX files by default).
    ///
    /// ## Parameters
    ///  - `assembly` - The assembly that contains the EDMX file.
    ///  - `edmxName` - The name of the EDMX file without the ".edmx" extension.
    ///
    /// ## Return Value
    /// An EntityProperty array with all properties which have a bad ConcurrencyMode
    /// setting.
    member o.BadConcurrencyModes(assembly : Assembly, edmxName : string) =
        let csdl = o.GetXDocument(assembly, edmxName + ".csdl").Root
        let ssdl = o.GetXDocument(assembly, edmxName + ".ssdl").Root
        o.BadConcurrencyModes(csdl, ssdl)


    /// Find bad ConcurrencyMode settings in all EDMX files embedded as resources in an
    /// assembly (this is how Visual Studio stores EDMX files by default).
    ///
    /// ## Parameters
    ///  - `assembly` - The assembly that contains one or several EDMX files.
    ///
    /// ## Return Value
    /// An `EntityProperty` array with all properties which have a bad ConcurrencyMode
    /// setting.
    member o.BadConcurrencyModes(assembly : Assembly) =
        let resNames = 
            assembly.GetManifestResourceNames()
            |> Seq.filter (fun s -> s.EndsWith(".ssdl", false, null))
            |> Seq.map (fun s -> s.[0 .. s.Length - 6])
        [|for i in resNames do yield! o.BadConcurrencyModes(assembly, i)|]


    /// Format a string with the results from `BadConcurrencyModes(...)`. This is useful
    /// as output for your test runner, e.g. NUnit.
    ///
    /// ## Parameters
    ///  - `entityProperties` - The result from `BadConcurrencyModes(...)`.
    ///
    /// ## Return Value
    /// The formatted string.
    member __.FormatBadConcurrencyModes(entityProperties : EntityProperty[]) =
        let sb = new Text.StringBuilder("Bad ConcurrencyModes (<Entity>.<Property>):\n")
        for i in entityProperties do
            sb.AppendFormat("{0}.{1}\n", i.Entity, i.Property) |> ignore
        sb.ToString()


    member private __.GetXDocument(assembly : Assembly, resourceName : string) : XDocument =
        use stream = assembly.GetManifestResourceStream(resourceName)
        use reader = new IO.StreamReader(stream)
        let xmlText = reader.ReadToEnd()
        XDocument.Parse(xmlText)
