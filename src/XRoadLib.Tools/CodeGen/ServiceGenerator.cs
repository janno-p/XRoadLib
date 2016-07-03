using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            var type = SF.ClassDeclaration(ServiceName)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword));

            var properties = serviceElement.Elements(XName.Get("port", NamespaceConstants.WSDL))
                .Select(port =>
                {
                    var typeName = SF.ParseTypeName(port.Attribute("binding").Value);

                    var initializer = SF.ObjectCreationExpression(typeName);
                    var producerName = port.Element(XName.Get("address", NamespaceConstants.XROAD)).Attribute("producer").Value;

                    if (!string.IsNullOrEmpty(producerName))
                        initializer = initializer.AddArgumentListArguments(SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(producerName))));

                    return SF.PropertyDeclaration(typeName, port.Attribute("name").Value)
                        .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))
                        .WithInitializer(SF.EqualsValueClause(initializer))
                        .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
                });

            type = type.AddMembers(properties.ToArray());

            return SF.CompilationUnit()
                     .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName("MyNamespace"))
                                   .AddMembers(type));
        }
    }
}