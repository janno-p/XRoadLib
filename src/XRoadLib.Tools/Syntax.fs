module XRoadLib.Tools.Syntax

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.Formatting
open System.IO
open System.Reflection

type SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory

type SourceFile =
    { Path: FileInfo
      CompilationUnit: CompilationUnitSyntax }
    static member New(path) =
        { Path = path
          CompilationUnit = SF.CompilationUnit() }

let attributeTarget = SF.AttributeTargetSpecifier(SF.Token(SyntaxKind.AssemblyKeyword))

let addUsing name sourceFile =
    { sourceFile with CompilationUnit = sourceFile.CompilationUnit.AddUsings(SF.UsingDirective(SF.IdentifierName(name: string))) }

let addAssemblyDescription description sourceFile =
    let args = SF.SeparatedList([SF.AttributeArgument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(description: string)))])
    let attr = SF.Attribute(SF.IdentifierName(SF.Identifier("AssemblyDescription")), SF.AttributeArgumentList(args))
    { sourceFile with CompilationUnit = sourceFile.CompilationUnit.AddAttributeLists(SF.AttributeList(attributeTarget, SF.SeparatedList([attr]))) }

let saveFile (sourceFile: SourceFile) =
    if not sourceFile.Path.Directory.Exists then
        sourceFile.Path.Directory.Create()
    let node = Formatter.Format(sourceFile.CompilationUnit, new AdhocWorkspace())
    use stream = sourceFile.Path.OpenWrite()
    stream.SetLength(0L)
    use writer = new StreamWriter(stream)
    node.WriteTo(writer)
