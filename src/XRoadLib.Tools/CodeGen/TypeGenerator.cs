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
                         .AddBaseListTypes(SF.SimpleBaseType(SF.ParseTypeName("IXRoadXmlSerializable")));

            var readXml = SF.MethodDeclaration(SF.ParseTypeName("void"), "ReadXml")
                            .AddParameterListParameters(SF.Parameter(SF.Identifier("reader")).WithType(SF.ParseTypeName("XmlReader")),
                                                        SF.Parameter(SF.Identifier("message")).WithType(SF.ParseTypeName("XRoadMessage")))
                            .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(SF.IdentifierName("IXRoadXmlSerializable")))
                            .WithBody(SF.Block());

            var writeXml = SF.MethodDeclaration(SF.ParseTypeName("void"), "WriteXml")
                             .AddParameterListParameters(SF.Parameter(SF.Identifier("writer")).WithType(SF.ParseTypeName("XmlWriter")),
                                                         SF.Parameter(SF.Identifier("message")).WithType(SF.ParseTypeName("XRoadMessage")))
                             .WithExplicitInterfaceSpecifier(SF.ExplicitInterfaceSpecifier(SF.IdentifierName("IXRoadXmlSerializable")))
                             .WithBody(SF.Block());

            return SF.CompilationUnit()
                     .AddUsings(SF.UsingDirective(SF.IdentifierName("System.Xml")),
                                SF.UsingDirective(SF.IdentifierName("XRoadLib.Serialization")))
                     .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName("MyNamespace"))
                                   .AddMembers(type.AddMembers(readXml, writeXml)));
        }
    }
}