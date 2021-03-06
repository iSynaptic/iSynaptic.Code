﻿// The MIT License
// 
// Copyright (c) 2012-2015 Jordan E. Terrell
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace iSynaptic.Code.Analysis.Usage.Async
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AwaitingFromResultAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Diagnostics.AwaitingFromResult);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(csac =>
            {
                var taskType = csac.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                if(taskType != null)
                    csac.RegisterSyntaxNodeAction(snac => AnalyzeAwaitExpression(snac, taskType), SyntaxKind.AwaitExpression);
            });
        }

        private void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol taskType)
        {
            if (context.CancellationToken.IsCancellationRequested)
                return;

            var exp = (AwaitExpressionSyntax)context.Node;
            var invocation = exp.Expression as InvocationExpressionSyntax;

            if (invocation == null) return;

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess.Name.Identifier.Text != "FromResult") return;

            var subjectInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Expression);
            if (subjectInfo.CandidateReason == CandidateReason.None)
            {
                var subjectType = subjectInfo.Symbol as ITypeSymbol;
                if (subjectType == null) return;

                if(subjectType == taskType)
                {
                    var diagnostic = Diagnostic.Create(
                        Diagnostics.AwaitingFromResult,
                        exp.GetLocation());

                        context.ReportDiagnostic(diagnostic);
                }
            }
        }

#pragma warning disable CSE0003
        public async System.Threading.Tasks.Task<int> BadMojo()
#pragma warning restore CSE0003
        {
            return await System.Threading.Tasks.Task.FromResult(42);
        }
    
    }
}