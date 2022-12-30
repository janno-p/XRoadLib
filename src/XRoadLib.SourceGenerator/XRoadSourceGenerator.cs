using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XRoadLib.SourceGenerator.Attributes;

namespace XRoadLib.SourceGenerator;

[Generator]
public class XRoadSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations =
            context.SyntaxProvider
                   .CreateSyntaxProvider(
                       static (s, _) => IsClassTargetForGeneration(s),
                       static (c, _) => GetClassSemanticTargetForGeneration(c)
                   )
                   .Where(static x => x is not null)!;

        var compilationAndValues =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndValues,
            static (sourceProductionContext, source) => Execute(source.Left, source.Right, sourceProductionContext)
        );
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classDeclarationSyntax in classes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
            if (ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            var dir = new FileInfo(semanticModel.SyntaxTree.FilePath).Directory;
            var input = File.ReadAllText(Path.Combine(dir.FullName, "Calculator.wsdl"));
            var content = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(input)).ToFullString();

            var namespaceName = GetNamespace(classSymbol);
            
            var fileName = $"{namespaceName}.{classSymbol.Name}.g.cs";
            
            context.AddSource(fileName, $"#nullable enable\r\n\r\nnamespace {namespaceName}\r\n{{\r\n    public partial class {classSymbol.Name}\r\n    {{\r\n        public const string _wsdlContent = {content};\r\n    }}\r\n}}\r\n");
        }
    }

    private static string GetNamespace(ISymbol namedTypeSymbol)
    {
        var list = new List<string>();
        var ns = namedTypeSymbol.ContainingNamespace;
        while (!string.IsNullOrEmpty(ns.Name))
        {
            list.Add(ns.Name);
            ns = ns.ContainingNamespace;
        }

        list.Reverse();
        return string.Join(".", list);
    }

    private static bool IsClassTargetForGeneration(SyntaxNode node) =>
        node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetClassSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            return null;

        foreach (var attributeSyntax in classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes))
        {
            if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            var fullName = attributeContainingTypeSymbol.ToDisplayString();

            if (fullName == typeof(XRoadClientAttribute).FullName)
                return classDeclarationSyntax;
        }

        return null;
    }
}