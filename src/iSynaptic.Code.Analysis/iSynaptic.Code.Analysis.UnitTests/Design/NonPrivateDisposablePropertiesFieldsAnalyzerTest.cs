// The MIT License
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

using Microsoft.CodeAnalysis;
using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace iSynaptic.Code.Analysis.Design
{
    [TestFixture]
    public class NonPrivateDisposablePropertiesFieldsAnalyzerTest : DiagnosticVerifier<NonPrivateDisposablePropertiesFieldsAnalyzer>
    {
        [Test]
        public void PropertyOfTypeIDisposable_YieldsDiagnostic()
        {
            var code = @"
                using System;

                namespace ConsoleApplication1
                {
                    class UserCode
                    {
                        public IDisposable BadMojo
                        {
                            get { return null; }
                        }
                    }
                }";


            var expected = new DiagnosticResult
            {
                Id = Diagnostics.NonPrivateDisposablePropertiesFields.Id,
                Message = "Property 'BadMojo' exposes a type that implements IDisposable.",
                Severity = Diagnostics.NonPrivateDisposablePropertiesFields.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 44)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Test]
        public void PropertyOfTypeThatDirectlyImplementsIDisposable_YieldsDiagnostic()
        {
            var code = @"
                using System;

                namespace ConsoleApplication1
                {
                    class UserCode
                    {
                        public DisposableResource BadMojo
                        {
                            get { return new DisposableResource(); }
                        }
                    }

                    class DisposableResource : IDisposable
                    {
                        public void Dispose() { }
                    }
                }";


            var expected = new DiagnosticResult
            {
                Id = Diagnostics.NonPrivateDisposablePropertiesFields.Id,
                Message = "Property 'BadMojo' exposes a type that implements IDisposable.",
                Severity = Diagnostics.NonPrivateDisposablePropertiesFields.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 51)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Test]
        public void PropertyOfTypeThatIndirectlyImplementsIDisposable_YieldsDiagnostic()
        {
            var code = @"
                using System;

                namespace ConsoleApplication1
                {
                    class UserCode
                    {
                        public SneakyResource BadMojo
                        {
                            get { return new SneakyResource(); }
                        }
                    }

                    class SneakyResource : DisposableResource
                    {
                    }

                    class DisposableResource : IDisposable
                    {
                        public void Dispose() { }
                    }
                }";


            var expected = new DiagnosticResult
            {
                Id = Diagnostics.NonPrivateDisposablePropertiesFields.Id,
                Message = "Property 'BadMojo' exposes a type that implements IDisposable.",
                Severity = Diagnostics.NonPrivateDisposablePropertiesFields.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 47)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Test]
        public void FieldOfTypeIDisposable_YieldsDiagnostic()
        {
            var code = @"
                using System;

                namespace ConsoleApplication1
                {
                    class UserCode
                    {
                        public IDisposable BadMojo;
                    }
                }";


            var expected = new DiagnosticResult
            {
                Id = Diagnostics.NonPrivateDisposablePropertiesFields.Id,
                Message = "Field 'BadMojo' exposes a type that implements IDisposable.",
                Severity = Diagnostics.NonPrivateDisposablePropertiesFields.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 44)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Test]
        public void FieldOfTypeThatDirectlyImplementsIDisposable_YieldsDiagnostic()
        {
            var code = @"
                using System;

                namespace ConsoleApplication1
                {
                    class UserCode
                    {
                        public DisposableResource BadMojo;
                    }

                    class DisposableResource : IDisposable
                    {
                        public void Dispose() { }
                    }
                }";


            var expected = new DiagnosticResult
            {
                Id = Diagnostics.NonPrivateDisposablePropertiesFields.Id,
                Message = "Field 'BadMojo' exposes a type that implements IDisposable.",
                Severity = Diagnostics.NonPrivateDisposablePropertiesFields.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 51)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

        [Test]
        public void FieldOfTypeThatIndirectlyImplementsIDisposable_YieldsDiagnostic()
        {
            var code = @"
                using System;

                namespace ConsoleApplication1
                {
                    class UserCode
                    {
                        public SneakyResource BadMojo;
                    }

                    class SneakyResource : DisposableResource
                    {
                    }

                    class DisposableResource : IDisposable
                    {
                        public void Dispose() { }
                    }
                }";


            var expected = new DiagnosticResult
            {
                Id = Diagnostics.NonPrivateDisposablePropertiesFields.Id,
                Message = "Field 'BadMojo' exposes a type that implements IDisposable.",
                Severity = Diagnostics.NonPrivateDisposablePropertiesFields.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 47)
                }
            };

            VerifyCSharpDiagnostic(code, expected);
        }

    }
}
