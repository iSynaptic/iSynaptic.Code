using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSynaptic.Code.Analysis
{
    public static class Diagnostics
    {
        public static DiagnosticDescriptor NonPrivateDisposablePropertiesFields = new DiagnosticDescriptor(
            id: "SYNA0001",
            title:"Disposable types should not be exposed as properties or fields.",
            messageFormat: "{0} '{1}' exposes a type that implements IDisposable.",
            category: "Design",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
