using Microsoft.CodeAnalysis;

namespace XRoadLib.SourceGenerator;

[Generator]
public class XRoadLibSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Actual source generator goes here
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}