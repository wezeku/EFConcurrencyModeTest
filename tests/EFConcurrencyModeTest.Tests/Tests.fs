module EFConcurrencyModeTest.Tests

open System
open System.Reflection
open System.Xml.Linq
open NUnit.Framework
open EFConcurrencyModeTest


// EDMX content, where tables A and C have columns with bad concurrency modes
// and tables B and D's colunms have correct ones.
let private csdl = """
    <Schema Namespace="Foo" Alias="Self" annotation:UseStrongSpatialTypes="false" 
            xmlns:annotation="http://schemas.microsoft.com/ado/2009/02/edm/annotation" 
            xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
      <EntityType Name="TableA">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="Int64" Nullable="false" />
        <Property Name="RowVer" Type="Binary" MaxLength="8" FixedLength="true" Nullable="false" annotation:StoreGeneratedPattern="Computed" />
      </EntityType>
      <EntityType Name="TableB">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="Int64" Nullable="false" />
        <Property Name="RowVer" Type="Binary" MaxLength="8" FixedLength="true" Nullable="false" annotation:StoreGeneratedPattern="Computed" ConcurrencyMode="Fixed" />
      </EntityType>
      <EntityType Name="TableC">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="Int64" Nullable="false" />
        <Property Name="RowVer" Type="Binary" MaxLength="8" FixedLength="true" Nullable="false" annotation:StoreGeneratedPattern="Computed" ConcurrencyMode="None" />
      </EntityType>
      <EntityType Name="TableD">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="Int64" Nullable="false" />
        <Property Name="RowVer" Type="Binary" MaxLength="8" FixedLength="true" Nullable="false" annotation:StoreGeneratedPattern="Computed" ConcurrencyMode="Fixed" />
      </EntityType>
    </Schema>
    """

let ssdl = """
    <Schema Namespace="Foo.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2008" Alias="Self" 
            xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" 
            xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
      <EntityType Name="TableA">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="bigint" Nullable="false" />
        <Property Name="RowVer" Type="timestamp" StoreGeneratedPattern="Computed" Nullable="false" />
      </EntityType>
      <EntityType Name="TableB">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="bigint" Nullable="false" />
        <Property Name="RowVer" Type="timestamp" StoreGeneratedPattern="Computed" Nullable="false" />
      </EntityType>
      <EntityType Name="TableC">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="bigint" Nullable="false" />
        <Property Name="RowVer" Type="timestamp" StoreGeneratedPattern="Computed" Nullable="false" />
      </EntityType>
      <EntityType Name="TableD">
        <Key>
          <PropertyRef Name="ID" />
        </Key>
        <Property Name="ID" Type="bigint" Nullable="false" />
        <Property Name="RowVer" Type="timestamp" StoreGeneratedPattern="Computed" Nullable="false" />
      </EntityType>
    </Schema>
    """


type TestTest() =
    [<Test>]
    static member ``Test of the ConcurrencyMode tester`` () =
        let cmt = ConcurrencyModeTester()
        let result = cmt.BadConcurrencyModes(XDocument.Parse csdl, XDocument.Parse ssdl)
        let expected =
            [ { Entity = "TableA"; Property = "RowVer" }
              { Entity = "TableC"; Property = "RowVer" }
            ]
        CollectionAssert.AreEquivalent(expected, result)
