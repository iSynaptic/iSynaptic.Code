using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace iSynaptic.Code.Analysis.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPrivateDisposablePropertiesFieldsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.Create(Diagnostics.NonPrivateDisposablePropertiesFields);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(csac =>
            {
                var disposableInterface = csac.Compilation.GetTypeByMetadataName("System.IDisposable");

                if(disposableInterface != null)
                    csac.RegisterSymbolAction(sac => AnalyzePropertyOrField(sac, disposableInterface), SymbolKind.Field, SymbolKind.Property);
            });
        }

        private void AnalyzePropertyOrField(SymbolAnalysisContext context, INamedTypeSymbol disposableInterface)
        {
            if (context.CancellationToken.IsCancellationRequested)
                return;

            if (context.Symbol.DeclaredAccessibility == Accessibility.Private)
                return;

            ITypeSymbol type = 
                (context.Symbol as IPropertySymbol)?.Type
                ?? (context.Symbol as IFieldSymbol)?.Type;
            
            if(type.Equals(disposableInterface) || type.AllInterfaces.Any(x => x.Equals(disposableInterface)))
            {
                var diagnostic = Diagnostic.Create(
                    Diagnostics.NonPrivateDisposablePropertiesFields, 
                    context.Symbol.Locations[0], 
                    context.Symbol.Kind == SymbolKind.Property ? "Property" : "Field",
                    context.Symbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}