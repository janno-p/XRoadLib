using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen
{
    public class ServiceGenerator
    {
        private readonly XElement serviceElement;

        public string ServiceName { get; }

        public ServiceGenerator(XElement serviceElement)
        {
            this.serviceElement = serviceElement;
            ServiceName = serviceElement.Attribute("name").Value;
        }

        public CompilationUnitSyntax Generate()
        {
            var type = ClassDeclaration(ServiceName).AddModifiers(Token(SyntaxKind.PublicKeyword));

            var properties = serviceElement.Elements(XName.Get("port", NamespaceConstants.WSDL))
                .Select(port =>
                {
                    var typeName = ParseTypeName(port.Attribute("binding").Value);

                    var initializer = ObjectCreationExpression(typeName);
                    var producerName = port.Element(XName.Get("address", NamespaceConstants.XROAD)).Attribute("producer").Value;

                    if (!string.IsNullOrEmpty(producerName))
                        initializer = initializer.AddArgumentListArguments(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(producerName))));

                    return PropertyDeclaration(typeName, port.Attribute("name").Value)
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                        .WithInitializer(EqualsValueClause(initializer))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                });

            type = type.AddMembers(properties.ToArray());

            return CompilationUnit().AddMembers(NamespaceDeclaration(IdentifierName("MyNamespace")).AddMembers(type));
        }
    }
}