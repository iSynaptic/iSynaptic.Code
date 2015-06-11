using System.Composition;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace iSynaptic.Code.Analysis.Design
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NonPrivateDisposablePropertiesFieldCodeFix)), Shared]
    public class NonPrivateDisposablePropertiesFieldCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds 
            => ImmutableArray.Create(Diagnostics.NonPrivateDisposablePropertiesFields.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var member
                = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().First();
            context.RegisterCodeFix(CodeAction.Create("Make Private", c => MakePrivate(context.Document, member, c)), diagnostic);

        }

        private async Task<Document> MakePrivate(Document document, MemberDeclarationSyntax member, CancellationToken c)
        {
            MemberDeclarationSyntax newMember = null;
            
            var field = member as FieldDeclarationSyntax;
            if(field != null)
            {
                newMember = field.WithModifiers(MakePrivate(field.Modifiers));
            }

            var property = member as PropertyDeclarationSyntax;
            if(property != null)
            {
                newMember = property.WithModifiers(MakePrivate(property.Modifiers));
            }
           
            var root = await document.GetSyntaxRootAsync();
            var newRoot = root.ReplaceNode(member, newMember);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private SyntaxTokenList MakePrivate(SyntaxTokenList modifiers)
        {
            return SyntaxFactory.TokenList(new[] { SyntaxFactory.Token(SyntaxKind.PrivateKeyword) }
                    .Concat(modifiers.Where(x => !IsVisibilityModifier(x.Kind()))));
        }

        private bool IsVisibilityModifier(SyntaxKind kind)
        {
            switch(kind)
            {
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.ProtectedKeyword:
                case SyntaxKind.InternalKeyword:
                case SyntaxKind.PrivateKeyword:
                    return true;
                default:
                    return false;
            }
        }
    }
}