module EFConcurrencyModeTest.Tests

open System
open System.Reflection
open System.Xml.Linq
open NUnit.Framework
open EFConcurrencyModeTest


// EDMX content, where tables A and C have columns with bad concurrency modes
// and tables B and D's columns have correct ones.
let csdl = """
      <Schema Namespace="FooModel" 
              Alias="Self" 
              annotation:UseStrongSpatialTypes="false" 
              xmlns:annotation="http://schemas.microsoft.com/ado/2009/02/edm/annotation" 
              xmlns:customannotation="http://schemas.microsoft.com/ado/2013/11/edm/customannotation" 
              xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
        <EntityType Name="TableARenamedInDesigner">
          <Key>
            <PropertyRef Name="ID" />
          </Key>
          <Property Name="ID" Type="Int64" Nullable="false" />
          <Property Name="VerNo" Type="Int32" Nullable="false" />
          <Property Name="RowVer" Type="Binary" MaxLength="8" FixedLength="true" Nullable="false" annotation:StoreGeneratedPattern="Computed" />
        </EntityType>
        <EntityType Name="TableB">
          <Key>
            <PropertyRef Name="ID" />
          </Key>
          <Property Name="ID" Type="Int64" Nullable="false" />
          <Property Name="VerNo" Type="Int" Nullable="false" ConcurrencyMode="Fixed" />
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
        <EntityContainer Name="FooEntities1" annotation:LazyLoadingEnabled="true">
          <EntitySet Name="TableARenamedInDesigners" EntityType="FooModel.TableARenamedInDesigner" />
          <EntitySet Name="TableBs" EntityType="Self.TableB" />
          <EntitySet Name="TableCs" EntityType="Self.TableC" />
          <EntitySet Name="TableDs" EntityType="Self.TableD" />
        </EntityContainer>
      </Schema>
    """

let ssdl = """
      <Schema Namespace="FooModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012" Alias="Self" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns:customannotation="http://schemas.microsoft.com/ado/2013/11/edm/customannotation" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
        <EntityType Name="TableA">
          <Key>
            <PropertyRef Name="ID" />
          </Key>
          <Property Name="ID" Type="bigint" Nullable="false" />
          <Property Name="VerNo" Type="int" Nullable="false" />
          <Property Name="RowVer" Type="timestamp" StoreGeneratedPattern="Computed" Nullable="false" />
        </EntityType>
        <EntityType Name="TableB">
          <Key>
            <PropertyRef Name="ID" />
          </Key>
          <Property Name="ID" Type="bigint" Nullable="false" />
          <Property Name="VerNo" Type="int" Nullable="false" />
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
          <EntityContainer Name="FooModelStoreContainer">
          <EntitySet Name="TableA" EntityType="Self.TableA" Schema="dbo" store:Type="Tables" />
          <EntitySet Name="TableB" EntityType="Self.TableB" Schema="dbo" store:Type="Tables" />
          <EntitySet Name="TableC" EntityType="Self.TableC" Schema="dbo" store:Type="Tables" />
          <EntitySet Name="TableD" EntityType="Self.TableD" Schema="dbo" store:Type="Tables" />
        </EntityContainer>
      </Schema>
    """

let msl = """
    <!-- C-S mapping content -->
      <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
        <EntityContainerMapping StorageEntityContainer="FooModelStoreContainer" CdmEntityContainer="FooEntities1">
          <EntitySetMapping Name="TableARenamedInDesigners">
            <EntityTypeMapping TypeName="FooModel.TableARenamedInDesigner">
              <MappingFragment StoreEntitySet="TableA">
                <ScalarProperty Name="ID" ColumnName="ID" />
                <ScalarProperty Name="VerNo" ColumnName="VerNo" />
                <ScalarProperty Name="RowVer" ColumnName="RowVer" />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
          <EntitySetMapping Name="TableBs">
            <EntityTypeMapping TypeName="FooModel.TableB">
              <MappingFragment StoreEntitySet="TableB">
                <ScalarProperty Name="ID" ColumnName="ID" />
                <ScalarProperty Name="VerNo" ColumnName="VerNo" />
                <ScalarProperty Name="RowVer" ColumnName="RowVer" />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
          <EntitySetMapping Name="TableCs">
            <EntityTypeMapping TypeName="FooModel.TableC">
              <MappingFragment StoreEntitySet="TableC">
                <ScalarProperty Name="ID" ColumnName="ID" />
                <ScalarProperty Name="RowVer" ColumnName="RowVer" />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
          <EntitySetMapping Name="TableDs">
            <EntityTypeMapping TypeName="FooModel.TableD">
              <MappingFragment StoreEntitySet="TableD">
                <ScalarProperty Name="ID" ColumnName="ID" />
                <ScalarProperty Name="RowVer" ColumnName="RowVer" />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
        </EntityContainerMapping>
      </Mapping>
    """

type TestTest() =
    [<Test>]
    static member ``Test of the ConcurrencyMode tester`` () =
        let cmt = ConcurrencyModeTester()
        cmt.ConcurrencyColumnNamePatterns <- [| "VerNo" |]
        let result = cmt.BadConcurrencyModes(XDocument.Parse csdl, XDocument.Parse ssdl, XDocument.Parse msl)
        let expected =
            [ { CsdlEntity = "TableARenamedInDesigner"; CsdlProperty = "VerNo";  SsdlEntity = "TableA"; SsdlProperty = "VerNo"  }
              { CsdlEntity = "TableARenamedInDesigner"; CsdlProperty = "RowVer"; SsdlEntity = "TableA"; SsdlProperty = "RowVer" }
              { CsdlEntity = "TableC"; CsdlProperty = "RowVer"; SsdlEntity = "TableC"; SsdlProperty = "RowVer" }
            ]
        CollectionAssert.AreEquivalent(expected, result)
