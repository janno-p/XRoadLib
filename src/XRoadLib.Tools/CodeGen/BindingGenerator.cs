using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            var type = SF.ClassDeclaration(BindingName)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SF.SimpleBaseType(SF.ParseTypeName($"{bindingElement.Attribute("type").Value}")));

            var methods = bindingElement.Elements(XName.Get("operation", NamespaceConstants.WSDL))
                .Select(operation =>
                {
                    var methodName = operation.Attribute("name").Value;

                    return SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), methodName)
                        .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SF.Block());
                });

            var ctor = SF.ConstructorDeclaration(BindingName)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(SF.Parameter(SF.Identifier("producerName")).WithType(SF.PredefinedType(SF.Token(SyntaxKind.StringKeyword))))
                .WithBody(SF.Block());

            type = type.AddMembers(ctor).AddMembers(methods.ToArray());

            return SF.CompilationUnit()
                     .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName("MyNamespace"))
                                   .AddMembers(type));
        }
    }
}