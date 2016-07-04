using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen
{
    public class PortTypeGenerator
    {
        private readonly XElement portTypeElement;

        public string PortTypeName { get; }

        public PortTypeGenerator(XElement portTypeElement)
        {
            this.portTypeElement = portTypeElement;
            PortTypeName = portTypeElement.Attribute("name").Value;
        }

        public CompilationUnitSyntax Generate()
        {
            var type = InterfaceDeclaration(PortTypeName).AddModifiers(Token(SyntaxKind.PublicKeyword));

            var methods = portTypeElement.Elements(XName.Get("operation", NamespaceConstants.WSDL))
                .Select(operation =>
                {
                    var methodName = operation.Attribute("name").Value;

                    return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), methodName)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                });

            type = type.AddMembers(methods.ToArray());

            return CompilationUnit().AddMembers(NamespaceDeclaration(IdentifierName("MyNamespace")).AddMembers(type));
        }
    }
}