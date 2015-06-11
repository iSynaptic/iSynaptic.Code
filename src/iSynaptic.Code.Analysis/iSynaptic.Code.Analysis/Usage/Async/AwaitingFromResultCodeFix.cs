using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace iSynaptic.Code.Analysis.Usage.Async
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitingFromResultCodeFix)), Shared]
    public class AwaitingFromResultCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(Diagnostics.AwaitingFromResult.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var awaitExpression = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AwaitExpressionSyntax>().First();
            context.RegisterCodeFix(CodeAction.Create("Replace with Expression", c => ReplaceWithExpression(context.Document, awaitExpression, c)), diagnostic);
        }

        private async Task<Document> ReplaceWithExpression(Document document, AwaitExpressionSyntax awaitExpression, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var invocation = awaitExpression.Expression as InvocationExpressionSyntax;
            var argumentExpression = invocation.ArgumentList.Arguments.First().Expression;
            
            var root = await document.GetSyntaxRootAsync();
            var newRoot = root.ReplaceNode(awaitExpression, argumentExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}