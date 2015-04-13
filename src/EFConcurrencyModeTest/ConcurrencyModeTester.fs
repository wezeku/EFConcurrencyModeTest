namespace EFConcurrencyModeTest

open System
open System.Linq
open System.Reflection
open System.Text.RegularExpressions
open System.Xml.Linq

/// Class that stores Entity and Property names.
///
///  - `CsdlEntity` - Conceptual entity (class representing a database record).
///  - `CsdlProperty` - Conceptual property (class field, representing a database column).
///  - `SsdlEntity` - Storage entity (database table).
///  - `SsdlProperty` - Storage property (database column).
type EntityProperty =
    { CsdlEntity   : string
      CsdlProperty : string
      SsdlEntity   : string
      SsdlProperty : string
    }


/// Class used to test ConcurrencyMode settings in EDMX files.
type ConcurrencyModeTester() =
    // EDMX namespaces.
    static member EdmxNs = "http://schemas.microsoft.com/ado/2009/11/edmx"
    static member CsdlNs = "http://schemas.microsoft.com/ado/2009/11/edm"
    static member MslNs  = "http://schemas.microsoft.com/ado/2009/11/mapping/cs"
    static member SsdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"

    /// Names of the types used by the database for row versioning.
    /// The default value is [|"timestamp"; "rowversion"|].
    member val RowVersionTypes = [|"timestamp"; "rowversion"|] with set, get

    /// Regex patterns of the column names used for row versioning. Not needed if column
    /// types like rowversion or timestamp are used.
    /// The default is empty.
    member val ConcurrencyColumnNamePatterns = [||] with set, get

    /// Find bad ConcurrencyMode settings in the associated CSDL, SSDL and MSL elements.
    ///
    /// ## Parameters
    ///  - `csdlElement` - CSDL element.
    ///  - `ssdlElement` - SSDL element.
    ///  - `mslElement` - MSL element.
    ///
    /// ## Return Value
    /// An EntityProperty array with all properties which have a bad ConcurrencyMode
    /// setting.
    member o.BadConcurrencyModes(csdlElement : XElement, ssdlElement : XElement, mslElement : XElement) =
        let filterProperties (xElem : XElement) namespace_ filter = 
            [for ent in xElem.Elements(XName.Get("EntityType", namespace_)) do
                let tableName = ent.Attribute(XName.Get("Name")).Value
                let props = ent.Elements(XName.Get("Property", namespace_))
                               .Where(fun p -> filter p)
                yield! [for p in props -> (tableName, p.Attribute(XName.Get("Name")).Value)]
            ]

        // Make a set of all ROWVERSION fields from SSDL.
        let isRowVersion (property : XElement) =
            let propType = property.Attribute(XName.Get("Type")).Value
            let propName = property.Attribute(XName.Get("Name")).Value
            o.RowVersionTypes |> Array.exists ((=) propType)
            || o.ConcurrencyColumnNamePatterns |> Array.exists (fun i -> Regex.IsMatch(propName, i))
        let ssdlRowVersionedFields = filterProperties ssdlElement ConcurrencyModeTester.SsdlNs isRowVersion

        let ssdlSchemaNamespace = ssdlElement.Attribute(XName.Get("Namespace")).Value
        let ssdlAlias = ssdlElement.Attribute(XName.Get("Alias")).Value
        let csdlSchemaNamespace = csdlElement.Attribute(XName.Get("Namespace")).Value
        let csdlAlias = csdlElement.Attribute(XName.Get("Alias")).Value

        let isSameEntityType tableName (el : XElement) =
            let entityType = el.Attribute(XName.Get("EntityType")).Value
            entityType = ssdlAlias + "." + tableName || entityType = ssdlSchemaNamespace + "." + tableName

        let getCsdlTypeAndPropertyName (tableName, columnName) =
            let entitySet = 
                ssdlElement.Descendants(XName.Get("EntitySet", ConcurrencyModeTester.SsdlNs))
                           .Where(isSameEntityType tableName).Single()
            let entityContainerMapping = 
                mslElement.Elements(XName.Get("EntityContainerMapping", ConcurrencyModeTester.MslNs))
                          .Where(fun e -> entitySet.Parent.Attribute(XName.Get("Name")).Value =
                                           e.Attribute(XName.Get("StorageEntityContainer")).Value)
                          .Single()
            let mappingFragment =
                entityContainerMapping.Descendants(XName.Get("MappingFragment", ConcurrencyModeTester.MslNs))
                                      .Where(fun mf -> mf.Attribute(XName.Get("StoreEntitySet")).Value = tableName)
                                      .Single()
            let scalarProperty =
                mappingFragment.Elements(XName.Get("ScalarProperty", ConcurrencyModeTester.MslNs))
                               .Where(fun sp -> sp.Attribute(XName.Get("ColumnName")).Value = columnName)
                               .Single()
            let csdlTypeName = scalarProperty.Parent.Parent.Attribute(XName.Get("TypeName")).Value
            let scalarPropertyName = scalarProperty.Attribute(XName.Get("Name")).Value
            (csdlTypeName, scalarPropertyName)

        let correspondingCsdlProperty (tableName, columnName) =
            let (csdlTypeName, scalarPropertyName) = getCsdlTypeAndPropertyName (tableName, columnName)
            let csdlEntityType = 
                csdlElement.Elements(XName.Get("EntityType", ConcurrencyModeTester.CsdlNs))
                           .Where(fun e -> 
                               let eName = e.Attribute(XName.Get("Name")).Value
                               csdlSchemaNamespace + "." + eName = csdlTypeName
                               || csdlAlias + "." + eName = csdlTypeName)
                           .Single()
            let csdlProperty =
                csdlEntityType.Elements(XName.Get("Property", ConcurrencyModeTester.CsdlNs))
                              .Where(fun p -> p.Attribute(XName.Get("Name")).Value = scalarPropertyName)
                              .Single()
            csdlProperty

        let isConcurrent (property : XElement) =
            let cMode = property.Attribute(XName.Get("ConcurrencyMode"))
            cMode <> null && cMode.Value = "Fixed"

        // Filter out all CSDL properties that should have concurrency checks, but haven't.    
        let csdlPropsMissingConcurrency =
            ssdlRowVersionedFields 
            |> Seq.map (fun ssdlField -> (ssdlField, correspondingCsdlProperty ssdlField))
            |> Seq.filter (fun (ssdlField, csdlPropertyElement) -> not (isConcurrent csdlPropertyElement))
            |> Seq.map (fun ((ssdlTable, ssdlColumn), csdlPropertyElement) -> 
                { CsdlEntity   = csdlPropertyElement.Parent.Attribute(XName.Get("Name")).Value
                  CsdlProperty = csdlPropertyElement.Attribute(XName.Get("Name")).Value
                  SsdlEntity   = ssdlTable
                  SsdlProperty = ssdlColumn })
        csdlPropsMissingConcurrency
        |> Seq.toArray


    /// Find bad ConcurrencyMode settings in the associated CSDL, SSDL and MSL documents.
    ///
    /// ## Parameters
    ///  - `csdlDocument` - CSDL document.
    ///  - `ssdlDocument` - SSDL document.
    ///  - `mslDocument` - MSL document.
    ///
    /// ## Return Value
    /// An EntityProperty array with all properties which have a bad ConcurrencyMode
    /// setting.
    member o.BadConcurrencyModes(csdlDocument : XDocument, ssdlDocument : XDocument, mslDocument : XDocument) =
        o.BadConcurrencyModes(csdlDocument.Root, ssdlDocument.Root, mslDocument.Root)


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
        let csdl = o.GetXDocument(assembly, edmxName + ".csdl")
        let ssdl = o.GetXDocument(assembly, edmxName + ".ssdl")
        let msl  = o.GetXDocument(assembly, edmxName + ".msl")
        o.BadConcurrencyModes(csdl, ssdl, msl)


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
        let sb = new Text.StringBuilder("Bad ConcurrencyModes (<CsdlEntity>.<CsdlProperty>; <SsdlEntity>.<SsdlProperty>):\n")
        for i in entityProperties do
            sb.AppendFormat("{0}.{1}; {2}.{3}\n", i.CsdlEntity, i.CsdlProperty, i.SsdlEntity, i.SsdlProperty) |> ignore
        sb.ToString()


    member private __.GetXDocument(assembly : Assembly, resourceName : string) : XDocument =
        use stream = assembly.GetManifestResourceStream(resourceName)
        use reader = new IO.StreamReader(stream)
        let xmlText = reader.ReadToEnd()
        XDocument.Parse(xmlText)
