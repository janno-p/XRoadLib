using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen
{
    public class BindingGenerator
    {
        private readonly XElement bindingElement;

        public string BindingName { get; }

        public BindingGenerator(XElement bindingElement)
        {
            this.bindingElement = bindingElement;
            BindingName = bindingElement.Attribute("name").Value;
        }

        public CompilationUnitSyntax Generate()
        {
            var type = ClassDeclaration(BindingName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(ParseTypeName($"{bindingElement.Attribute("type").Value}")));

            var methods = bindingElement.Elements(XName.Get("operation", NamespaceConstants.WSDL))
                .Select(operation =>
                {
                    var methodName = operation.Attribute("name").Value;

                    return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), methodName)
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .WithBody(Block());
                });

            var ctor = ConstructorDeclaration(BindingName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(Parameter(Identifier("producerName")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                .WithBody(Block());

            type = type.AddMembers(ctor).AddMembers(methods.ToArray());

            return CompilationUnit().AddMembers(NamespaceDeclaration(IdentifierName("MyNamespace")).AddMembers(type));
        }
    }
}