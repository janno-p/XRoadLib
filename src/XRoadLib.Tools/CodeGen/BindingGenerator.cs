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

        public BindingGenerator(XElement bindingElement)
        {
            this.bindingElement = bindingElement;
        }

        public CompilationUnitSyntax Generate()
        {
            var bindingName = bindingElement.Attribute("name").Value;

            var type = SF.ClassDeclaration(bindingName)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SF.SimpleBaseType(SF.ParseTypeName($"I{bindingElement.Attribute("type").Value}")));

            var methods = bindingElement.Elements(XName.Get("operation", NamespaceConstants.WSDL))
                .Select(operation =>
                {
                    var methodName = operation.Attribute("name").Value;

                    return SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), methodName)
                        .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SF.Block());
                });

            type = type.AddMembers(methods.ToArray());

            var cu = SF.CompilationUnit()
                .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName("MyNamespace"))
                    .AddMembers(type));

            var formattedNode = Formatter.Format(cu, new AdhocWorkspace());
            using (var writer = new StreamWriter(File.OpenWrite(Path.Combine("Output", $"{bindingName}.cs"))))
                formattedNode.WriteTo(writer);

            return cu;
        }
    }
}