using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static CompilationUnitSyntax ToCompilationUnit(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            return CompilationUnit()
                     .AddUsings(UsingDirective(IdentifierName("Optional")),
                                UsingDirective(IdentifierName("System")),
                                UsingDirective(IdentifierName("System.IO")),
                                UsingDirective(IdentifierName("System.Xml")),
                                UsingDirective(IdentifierName("XRoadLib.Serialization")))
                     .AddMembers(NamespaceDeclaration(IdentifierName("MyNamespace"))
                                   .AddMembers(classDeclarationSyntax));
        }

        public static void SaveToFile(this ClassDeclarationSyntax classDeclarationSyntax, string path)
        {
            classDeclarationSyntax.ToCompilationUnit().SaveFile(Path.Combine(path, $"{classDeclarationSyntax.Identifier.ValueText}.cs"));
        }
    }
}