using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen
{
    public class TypeGenerator
    {
        private readonly XElement element;

        public string TypeName { get; }

        public TypeGenerator(XElement element)
        {
            this.element = element;
            TypeName = element.Attribute("name").Value;
        }

        public CompilationUnitSyntax Generate()
        {
            var type = SF.ClassDeclaration(TypeName)
                         .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                         .AddBaseListTypes(SF.SimpleBaseType(SF.ParseTypeName("IXmlSerializable")));

            var getSchema = SF.MethodDeclaration(SF.ParseTypeName("XmlSchema"), "GetSchema")
                              .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(SF.IdentifierName("IXmlSerializable")))
                              .WithBody(SF.Block(SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.NullLiteralExpression))));

            var readXml = SF.MethodDeclaration(SF.ParseTypeName("void"), "ReadXml")
                            .AddParameterListParameters(SF.Parameter(SF.Identifier("reader")).WithType(SF.ParseTypeName("XmlReader")))
                            .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(SF.IdentifierName("IXmlSerializable")))
                            .WithBody(SF.Block());

            var writeXml = SF.MethodDeclaration(SF.ParseTypeName("void"), "WriteXml")
                             .AddParameterListParameters(SF.Parameter(SF.Identifier("writer")).WithType(SF.ParseTypeName("XmlWriter")))
                             .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(SF.IdentifierName("IXmlSerializable")))
                             .WithBody(SF.Block());

            return SF.CompilationUnit()
                     .AddUsings(SF.UsingDirective(SF.IdentifierName("System.Xml")),
                                SF.UsingDirective(SF.IdentifierName("System.Xml.Schema")),
                                SF.UsingDirective(SF.IdentifierName("System.Xml.Serialization")))
                     .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName("MyNamespace"))
                                   .AddMembers(type.AddMembers(getSchema, readXml, writeXml)));
        }
    }
}