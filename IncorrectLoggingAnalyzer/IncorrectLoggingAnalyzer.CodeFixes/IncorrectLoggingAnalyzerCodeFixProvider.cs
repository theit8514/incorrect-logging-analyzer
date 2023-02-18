using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace IncorrectLoggingAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IncorrectLoggingAnalyzerCodeFixProvider))]
    [Shared]
    public class IncorrectLoggingAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(IncorrectLoggingAnalyzerAnalyzer.DiagnosticIdChangeType);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var syntaxToken = root.FindToken(diagnosticSpan.Start);
            var fieldDeclaration = syntaxToken.Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();
            var declaration = syntaxToken.Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixResources.CodeFixTitle,
                    c => ChangeGenericTypeParameterAsync(context.Document, declaration, fieldDeclaration, c),
                    nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private static async Task<Document> ChangeGenericTypeParameterAsync(Document document,
            BaseTypeDeclarationSyntax classDeclaration, BaseFieldDeclarationSyntax fieldDeclaration,
            CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var generator = editor.Generator;

            // Get the node of the current field's type
            SyntaxNode oldType = fieldDeclaration.Declaration.Type;
            var oldTypeInfo = semanticModel.GetTypeInfo(oldType);

            // Get the class name as a string.
            var className = classDeclaration.Identifier.Value as string;

            // Generate a new node of ILogger<ClassName>
            var newType = generator.GenericName("ILogger", generator.IdentifierName(className));

            // Find all nodes inside this class that contain a GenericNameSyntax of our old type.
            var referencesOfTypeInClass = classDeclaration.DescendantNodes()
                .OfType<GenericNameSyntax>()
                .Where(x =>
                {
                    var typeInfo = semanticModel.GetTypeInfo(x);
                    return typeInfo.Equals(oldTypeInfo);
                })
                .ToImmutableArray();

            // Replace all GenericNameSyntax nodes with the new type node.
            // This should hit all nodes in the class, including those in QualifiedNameSyntax (fully qualified).
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNodes(referencesOfTypeInClass, (syntax, nameSyntax) => newType);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}