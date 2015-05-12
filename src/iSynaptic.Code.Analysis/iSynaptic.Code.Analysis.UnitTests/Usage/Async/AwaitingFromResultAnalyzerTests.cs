using System;

using NUnit.Framework;

namespace iSynaptic.Code.Analysis.Usage.Async
{
    [TestFixture]
    public class AwaitingFromResultAnalyzerTests : DiagnosticAnalyzerVerifier<AwaitingFromResultAnalyzer>
    {
        protected override string WrapUsings => base.WrapUsings + " using System.Threading.Tasks;";

        [Test]
        public void AwaitingUnqualifiedTaskFromResult_YieldsDiagnostic()
        {
            var code = WrapInClass(@"
                    public async Task<int> BadMojo()
                    {
                        return await Task.FromResult(42);
                    }");

            var expected = new DiagnosticResult
            {
                Id = Diagnostics.AwaitingFromResult.Id,
                Message = "Awaiting a Task.FromResult() call is unnecessary and inefficient.",
                Severity = Diagnostics.AwaitingFromResult.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 6, 32)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }
    }
}
