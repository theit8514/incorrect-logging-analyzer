using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IncorrectLoggingAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IncorrectLoggingAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticIdPrefix = "ILA";
        public const string DiagnosticIdChangeType = DiagnosticIdPrefix + "1001";
        public const string DiagnosticIdStatic = DiagnosticIdPrefix + "1002";
        private const string Category = "Correctness";

        private static readonly LocalizableString HelpLinkUri =
            new LocalizableResourceString(nameof(Resources.HelpLinkUri), Resources.ResourceManager,
                typeof(Resources));

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString ChangeTypeTitle =
            new LocalizableResourceString(nameof(Resources.ChangeTypeAnalyzerTitle), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString ChangeTypeMessageFormat =
            new LocalizableResourceString(nameof(Resources.ChangeTypeAnalyzerMessageFormat), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString ChangeTypeDescription =
            new LocalizableResourceString(nameof(Resources.ChangeTypeAnalyzerDescription), Resources.ResourceManager,
                typeof(Resources));

        private static readonly DiagnosticDescriptor RuleChangeType = new DiagnosticDescriptor(
            DiagnosticIdChangeType, ChangeTypeTitle, ChangeTypeMessageFormat, Category, DiagnosticSeverity.Warning,
            true, ChangeTypeDescription, HelpLinkUri + "#ILA1001");

        private static readonly LocalizableString StaticTitle =
            new LocalizableResourceString(nameof(Resources.StaticAnalyzerTitle), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString StaticMessageFormat =
            new LocalizableResourceString(nameof(Resources.StaticAnalyzerMessageFormat), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString StaticDescription =
            new LocalizableResourceString(nameof(Resources.StaticAnalyzerDescription), Resources.ResourceManager,
                typeof(Resources));

        private static readonly DiagnosticDescriptor RuleStatic = new DiagnosticDescriptor(
            DiagnosticIdStatic, StaticTitle, StaticMessageFormat, Category, DiagnosticSeverity.Warning,
            true, StaticDescription, HelpLinkUri + "#ILA1002");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(RuleChangeType, RuleStatic);

        public override void Initialize(AnalysisContext context)
        {
#if DEBUGGER
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
        }

        private static void AnalyzeField(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            Debug.Assert(fieldDeclaration != null, nameof(fieldDeclaration) + " != null");
            var classType = context.ContainingSymbol.ContainingType;
            var className = classType.ToDisplayString();

            // Find the GenericNameSyntax of this Field's declaration. This will strip out the QualifiedNameSyntax
            var generic = fieldDeclaration.Declaration
                ?.Type
                .DescendantNodesAndSelf()
                .OfType<GenericNameSyntax>()
                .FirstOrDefault();
            if (generic == null)
                return;

            // Check if the type info contains the Microsoft.Extensions.Logging.ILogger interface.
            var typeInfo = context.SemanticModel.GetTypeInfo(generic).Type;
            if (typeInfo == null || // Could be null if the type is not valid
                typeInfo.TypeKind != TypeKind.Interface ||
                typeInfo.ContainingNamespace.ToDisplayString() != // Probably a better way to handle this.
                "Microsoft.Extensions.Logging" ||
                typeInfo.Name != "ILogger")
                return;

            // Get the Type arguments from this generic (the T in ILogger<T>)
            var genericArgument = generic.TypeArgumentList.Arguments.Count == 1
                ? generic.TypeArgumentList.Arguments.First()
                : null;
            if (genericArgument == null)
                return;

            // Get the type info of the containing generic argument type.
            var innerType = context.SemanticModel.GetTypeInfo(genericArgument).Type.ToDisplayString();

            // If matching the current class, ignore.
            if (className == innerType)
                return;

            // Place the location at the Field Declaration's Type, not including the field name, prefixes, or assignment.
            var location = fieldDeclaration.Declaration.Type.GetLocation();
            if (classType.IsStatic)
            {
                // The system does not allow static classes to be defined in a generic.
                // ILogger<T> is invalid if T is static.
                context.ReportDiagnostic(Diagnostic.Create(RuleStatic, location, innerType));
            }
            else
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(RuleChangeType, location, innerType, className);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}