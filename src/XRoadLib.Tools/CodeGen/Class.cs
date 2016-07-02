using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen
{
    public class Class
    {
        public static void Save()
        {
            var cu = SF.CompilationUnit()
                .AddUsings(SF.UsingDirective(SF.IdentifierName("System")))
                .AddUsings(SF.UsingDirective(SF.IdentifierName("System.Collections.Generic")))
                .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName("MyNamespace"))
                    .AddMembers(SF.ClassDeclaration("MyClass")
                        .AddModifiers(SF.Token(SyntaxKind.PrivateKeyword))
                        .AddModifiers(SF.Token(SyntaxKind.PartialKeyword))));

            var formattedNode = Formatter.Format(cu, new AdhocWorkspace());
            using (var writer = new StreamWriter(File.OpenWrite("test.cs")))
                formattedNode.WriteTo(writer);
        }
    }
}