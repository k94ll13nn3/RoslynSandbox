using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RoslynSandbox
{
    public class RoslynSandboxSymbolVisitor : SymbolVisitor
    {
        public override void DefaultVisit(ISymbol symbol)
        {
            if (symbol.DeclaredAccessibility != Accessibility.Public && symbol.DeclaredAccessibility != Accessibility.NotApplicable)
            {
                return;
            }

            if (symbol is INamespaceOrTypeSymbol namespaceOrTypeSymbol)
            {
                foreach (var child in namespaceOrTypeSymbol.GetMembers())
                {
                    child.Accept(this);
                }
            }

            var kind = symbol.Kind.ToString();
            var implementedSymbol = IsInterfaceImplementation(symbol);
            var symbolType = string.Empty;
            if (implementedSymbol.isInterfaceImplementation)
            {
                symbolType = $"implements {implementedSymbol.symbol}";
            }
            else if (symbol.IsOverride && symbol is IMethodSymbol m)
            {
                symbolType = $"overrides {m.OverriddenMethod}";
            }
            else if (symbol.IsOverride && symbol is IPropertySymbol p)
            {
                symbolType = $"overrides {p.OverriddenProperty}";
            }

            Console.WriteLine($"{symbol} ({kind}): {symbolType}");
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        private (ISymbol symbol, bool isInterfaceImplementation) IsInterfaceImplementation<T>(T method) where T : class, ISymbol
        {
            var symbol = method?
                .ContainingType?
                .AllInterfaces
                .SelectMany(@interface => @interface
                    .GetMembers()
                    .OfType<T>())
                .FirstOrDefault(interfaceMethod => method
                    .ContainingType
                    .FindImplementationForInterfaceMember(interfaceMethod)?.Equals(method) ?? false);

            return (symbol, symbol != null);
        }
    }
}