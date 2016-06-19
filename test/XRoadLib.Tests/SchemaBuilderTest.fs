namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.Xml
open System.Xml.Linq
open System.Xml.Schema
open XRoadLib
open XRoadLib.Protocols.Description
open XRoadLib.Tests.Contract.Wsdl

[<TestFixture>]
type ``document literal style array schema`` () =
    class
    end
    (*
    let qn n ns = XmlQualifiedName(n, ns)
    let (?) (typ: Type) propertyName = typ.GetProperty(propertyName)
    let schemaBuilder = SchemaBuilder(XRoadProtocol.Version31, XNamespace.Get("tns"), null, Nullable())

    [<Test>]
    member x.``build schema for default array`` () =
        schemaBuilder.RequiredImports.Clear()
        let element = schemaBuilder.CreateSchemaElement(typeof<ArrayTypeTest>?Array1)
        element.Name |> should equal "Array1"
        element.MinOccurs |> should equal 0M
        element.MinOccursString |> should equal "0"
        element.MaxOccurs |> should equal 1M
        element.MaxOccursString |> should be Null
        element.IsNillable |> should be False
        element.SchemaTypeName |> should equal XmlQualifiedName.Empty
        element.Annotation |> should be Null
        element.SchemaType |> should be instanceOfType<XmlSchemaComplexType>
        let arrayType = element.SchemaType :?> XmlSchemaComplexType
        arrayType.Name |> should be Null
        arrayType.Particle |> should be instanceOfType<XmlSchemaSequence>
        let arraySequence = arrayType.Particle :?> XmlSchemaSequence
        arraySequence.Items.Count |> should equal 1
        arraySequence.Items.[0] |> should be instanceOfType<XmlSchemaElement>
        let itemElement = arraySequence.Items.[0] :?> XmlSchemaElement
        itemElement.Name |> should equal "item"
        itemElement.MinOccurs |> should equal 0M
        itemElement.MinOccursString |> should equal "0"
        itemElement.MaxOccurs |> should equal Decimal.MaxValue
        itemElement.MaxOccursString |> should equal "unbounded"
        itemElement.IsNillable |> should be False
        itemElement.SchemaTypeName |> should equal (qn "string" NamespaceConstants.XSD)
        itemElement.Annotation |> should be Null
        itemElement.SchemaType |> should be Null
        schemaBuilder.RequiredImports.Count |> should equal 0

[<TestFixture>]
type ``rpc encoded style array schema`` () =
    let qn n ns = XmlQualifiedName(n, ns)
    let (?) (typ: Type) propertyName = typ.GetProperty(propertyName)
    let schemaBuilder = SchemaBuilder(XRoadProtocol.Version20, XNamespace.Get("tns"), null, Nullable())

    [<Test>]
    member x.``build schema for default array`` () =
        schemaBuilder.RequiredImports.Clear()
        let element = schemaBuilder.CreateSchemaElement(typeof<ArrayTypeTest>?Array1)
        element.Name |> should equal "Array1"
        element.MinOccurs |> should equal 0M
        element.MinOccursString |> should equal "0"
        element.MaxOccurs |> should equal 1M
        element.MaxOccursString |> should be Null
        element.IsNillable |> should be False
        element.SchemaTypeName |> should equal XmlQualifiedName.Empty
        element.Annotation |> should be Null
        element.SchemaType |> should be instanceOfType<XmlSchemaComplexType>
        let arrayType = element.SchemaType :?> XmlSchemaComplexType
        arrayType.Name |> should be Null
        arrayType.Particle |> should be Null
        arrayType.ContentModel |> should be instanceOfType<XmlSchemaComplexContent>
        let content = arrayType.ContentModel :?> XmlSchemaComplexContent
        content.Content |> should be instanceOfType<XmlSchemaComplexContentRestriction>
        let restriction = content.Content :?> XmlSchemaComplexContentRestriction
        restriction.BaseTypeName |> should equal (qn "Array" NamespaceConstants.SOAP_ENC)
        restriction.Attributes.Count |> should equal 1
        let attribute = restriction.Attributes.[0] :?> XmlSchemaAttribute
        attribute.RefName |> should equal (qn "arrayType" NamespaceConstants.SOAP_ENC)
        attribute.UnhandledAttributes.Length |> should equal 1
        attribute.UnhandledAttributes.[0].Name |> should equal "wsdl:arrayType"
        attribute.UnhandledAttributes.[0].NamespaceURI |> should equal NamespaceConstants.WSDL
        attribute.UnhandledAttributes.[0].Value |> should equal "http://www.w3.org/2001/XMLSchema:string[]"
        restriction.Particle |> should be instanceOfType<XmlSchemaSequence>
        let sequence = restriction.Particle :?> XmlSchemaSequence
        sequence.Items.Count |> should equal 1
        sequence.Items.[0] |> should be instanceOfType<XmlSchemaElement>
        let itemElement = sequence.Items.[0] :?> XmlSchemaElement
        itemElement.Name |> should equal "item"
        itemElement.MinOccurs |> should equal 0M
        itemElement.MinOccursString |> should equal "0"
        itemElement.MaxOccurs |> should equal Decimal.MaxValue
        itemElement.MaxOccursString |> should equal "unbounded"
        itemElement.IsNillable |> should be False
        itemElement.SchemaTypeName |> should equal (qn "string" NamespaceConstants.XSD)
        itemElement.Annotation |> should be Null
        itemElement.SchemaType |> should be Null
        schemaBuilder.RequiredImports.Contains(NamespaceConstants.SOAP_ENC) |> should be True
    *)
