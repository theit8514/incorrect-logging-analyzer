﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace IncorrectLoggingAnalyzer.Test.Verifiers
{
    public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
        where TCodeRefactoring : CodeRefactoringProvider, new()
    {
        public class Test : CSharpCodeRefactoringTest<TCodeRefactoring, MSTestVerifier>
        {
            public Test()
            {
                ReferenceAssemblies = ReferenceAssemblies
                    .AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Microsoft.Extensions.Logging.Abstractions", "2.2.0"),
                        new PackageIdentity("Serilog", "3.1.1")))
                    .AddAssemblies(ImmutableArray.Create(
                        "Microsoft.Extensions.Logging.Abstractions",
                        "Serilog"));
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
                    compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    return solution;
                });
            }
        }
    }
}