using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace XRoadLib.Tools.CodeGen.Extensions
{
    public static class CompilationUnitSyntaxExtensions
    {
        public static void SaveFile(this CompilationUnitSyntax compilationUnitSyntax, string path)
        {
            var formattedNode = Formatter.Format(compilationUnitSyntax, new AdhocWorkspace());
            using (var file = File.OpenWrite(path))
            {
                file.SetLength(0);
                using (var writer = new StreamWriter(file))
                    formattedNode.WriteTo(writer);
            }
        }
    }
}