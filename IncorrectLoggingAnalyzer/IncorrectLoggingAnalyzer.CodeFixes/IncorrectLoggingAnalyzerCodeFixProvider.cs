using System.Collections.Generic;
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
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
                    CodeFixResources.CodeFixChangeTypeTitle,
                    c => ChangeGenericTypeParameterAsync(context.Document, declaration, fieldDeclaration, c),
                    nameof(CodeFixResources.CodeFixChangeTypeTitle)),
                diagnostic);

            if (declaration.BaseList == null)
                return;

            var semanticModel = await context.Document.GetSemanticModelAsync();
            SyntaxNode oldType = fieldDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            var oldTypeInfo = semanticModel.GetTypeInfo(oldType).Type.Name;
            var baseType = semanticModel.GetDeclaredSymbol(declaration).BaseType.Name;
            // Only show convert to base type if the type is actually the base type.
            if (!oldTypeInfo.Equals(baseType))
                return;

            // Register this code fix for base class only.
            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixResources.CodeFixSeparateBaseClassTitle,
                    c => SeparateBaseClassAndDerivedClassAsync(context.Document, declaration, fieldDeclaration, c),
                    nameof(CodeFixResources.CodeFixSeparateBaseClassTitle)),
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
            var className = classDeclaration.Identifier.ValueText;

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
            var newRoot = oldRoot.ReplaceNodes(referencesOfTypeInClass, (syntax, nameSyntax) => newType
                .WithLeadingTrivia(syntax.GetLeadingTrivia())
                .WithTrailingTrivia(syntax.GetTrailingTrivia()));
            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> SeparateBaseClassAndDerivedClassAsync(Document document,
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
            var className = classDeclaration.Identifier.ValueText;
            var baseClassName = semanticModel.GetDeclaredSymbol(classDeclaration).BaseType.Name;

            // Generate a new node of ILogger<ClassName>
            var newType = generator.GenericName("ILogger", generator.IdentifierName(className));
            var baseType = generator.GenericName("ILogger", generator.IdentifierName(baseClassName));

            // A bit more complicated here. We want to replace all the references to the existing logger
            // but add a new logger to base() calls
            var baseExpressions = classDeclaration.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where(x => x.Initializer != null)
                .ToImmutableArray();
            foreach (var constructor in baseExpressions)
            {
                // Generate a new list of arguments for the base initializer. e.g. base(logger) -> base(baseLogger)
                var newArguments = new List<ArgumentSyntax>();
                var initializer = constructor.Initializer;
                var addBaseLogger = false;
                var identifierName = GenerateIdentifierName(semanticModel, constructor.Body, "baseLogger");
                foreach (var argument in initializer.ArgumentList.Arguments)
                {
                    if (argument.Expression is IdentifierNameSyntax name)
                    {
                        var type = semanticModel.GetTypeInfo(name);
                        // Replace this argument with the baseLogger if types match
                        if (type.Equals(oldTypeInfo))
                        {
                            newArguments.Add(SF.Argument(SF.IdentifierName(identifierName)));
                            // Ensure that we add the baseLogger to the parameters list
                            addBaseLogger = true;
                            continue;
                        }
                    }

                    newArguments.Add(argument);
                }


                // Replace the ArgumentList inside the initializer
                editor.ReplaceNode(initializer.ArgumentList,
                    initializer.ArgumentList.WithArguments(SF.SeparatedList(newArguments)));

                // Now we need to add the baseLogger parameter to this constructor
                var newParameters = new List<ParameterSyntax>();
                foreach (var parameter in constructor.ParameterList.Parameters)
                {
                    // If the type of this parameter matches our old type, add it with the same identifier
                    // with the new type
                    if (semanticModel.GetTypeInfo(parameter.Type).Equals(oldTypeInfo))
                        newParameters.Add(SF.Parameter(parameter.AttributeLists, parameter.Modifiers,
                            (TypeSyntax)newType, parameter.Identifier, parameter.Default));
                    else
                        // Otherwise add the parameter as normal
                        newParameters.Add(parameter);
                }

                // Only add our baseLogger parameter if the base initializer was using the existing logger
                if (addBaseLogger)
                {
                    // Generate a new parameter at the end of this parameter list, e.g. ILogger<BaseType> baseLogger
                    var newParameter = SF.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                        (TypeSyntax)baseType, SF.Identifier(identifierName), null);
                    newParameters.Add(newParameter);
                }

                // Replace the ParametersList inside the constructor
                editor.ReplaceNode(constructor.ParameterList,
                    constructor.ParameterList.WithParameters(SF.SeparatedList(newParameters)));
            }

            // Replace the field type with the new type
            editor.ReplaceNode(oldType, newType
                .WithLeadingTrivia(oldType.GetLeadingTrivia())
                .WithTrailingTrivia(oldType.GetTrailingTrivia()));

            return editor.GetChangedDocument();
        }

        private static string GenerateIdentifierName(SemanticModel semanticModel, SyntaxNode node,
            string identifierName)
        {
            var symbols = semanticModel.LookupSymbols(node.SpanStart).OfType<IParameterSymbol>();
            var symbolNames = new HashSet<string>(symbols.Select(s => s.Name));
            var index = 0;
            var newIdentifierName = identifierName;
            while (symbolNames.Contains(newIdentifierName))
                newIdentifierName = $"{identifierName}{++index}";

            return newIdentifierName;
        }
    }
}