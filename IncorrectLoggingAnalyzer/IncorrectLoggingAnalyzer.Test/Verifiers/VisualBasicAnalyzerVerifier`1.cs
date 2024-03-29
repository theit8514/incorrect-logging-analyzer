﻿using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace IncorrectLoggingAnalyzer.Test.Verifiers
{
    [ExcludeFromCodeCoverage]
    [PublicAPI]
    public static partial class VisualBasicAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()" />
        public static DiagnosticResult Diagnostic()
        {
            return VisualBasicAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic();
        }

        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)" />
        public static DiagnosticResult Diagnostic(string diagnosticId)
        {
            return VisualBasicAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(diagnosticId);
        }

        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)" />
        public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        {
            return VisualBasicAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(descriptor);
        }

        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])" />
        public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new Test
            {
                TestCode = source
            };

            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync(CancellationToken.None);
        }
    }
}