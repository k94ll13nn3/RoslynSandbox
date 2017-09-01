using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
                foreach (ISymbol child in namespaceOrTypeSymbol.GetMembers())
                {
                    child.Accept(this);
                }
            }

            var kind = symbol.Kind.ToString();
            (ISymbol symbol, bool isInterfaceImplementation) implementedSymbol = IsInterfaceImplementation(symbol);
            var symbolType = string.Empty;
            if (implementedSymbol.isInterfaceImplementation)
            {
                symbolType = $"implements {implementedSymbol.symbol}";
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

        public override void VisitMethod(IMethodSymbol symbol)
        {
            if (symbol.MethodKind != MethodKind.Ordinary)
            {
                DefaultVisit(symbol);
                return;
            }

            ITypeSymbol returnType = symbol.ReturnType;
            ImmutableArray<IParameterSymbol> parameters = symbol.Parameters;
            ImmutableArray<ITypeParameterSymbol> typeParameters = symbol.TypeParameters;

            var kind = symbol.MethodKind.ToString();
            (ISymbol symbol, bool isInterfaceImplementation) implementedSymbol = IsInterfaceImplementation(symbol);
            var symbolType = string.Empty;
            if (implementedSymbol.isInterfaceImplementation)
            {
                symbolType = $"implements {implementedSymbol.symbol}";
            }
            else if (symbol.IsOverride)
            {
                symbolType = $"overrides {symbol.OverriddenMethod}";
            }

            var info = new StringBuilder();
            info.Append($"{DisplayReturnType(returnType)} {symbol.Name}");

            if (typeParameters.Length > 0)
            {
                info.Append($"<{string.Join(", ", typeParameters)}>");
            }

            info.Append("(");
            info.Append(string.Join(", ", parameters.Select(p => DisplayParameter(p))));
            info.Append(")");

            if (typeParameters.Any(x => HasConstraints(x)))
            {
                foreach (ITypeParameterSymbol type in typeParameters.Where(x => HasConstraints(x)))
                {
                    // struct or class then herited class then interfaces then new()
                    info.Append(" where ");
                    info.Append($"{type.Name} : ");

                    var constraints = new List<string>();
                    if (type.HasReferenceTypeConstraint)
                    {
                        constraints.Add("class");
                    }
                    if (type.HasValueTypeConstraint)
                    {
                        constraints.Add("struct");
                    }
                    constraints.AddRange(type.ConstraintTypes.Select(x => x.Name));
                    if (type.HasConstructorConstraint)
                    {
                        constraints.Add("new()");
                    }

                    info.Append(string.Join(", ", constraints));
                }
            }

            // T Convert<U, V>(U data, string name, V id) where U : Data, new()
            Console.WriteLine($"{info} ({kind}): {symbolType}");
        }

        private bool HasConstraints(ITypeParameterSymbol symbol) =>
            symbol.HasValueTypeConstraint ||
            symbol.HasConstructorConstraint ||
            symbol.HasReferenceTypeConstraint ||
            symbol.ConstraintTypes.Length > 0;

        private (ISymbol symbol, bool isInterfaceImplementation) IsInterfaceImplementation<T>(T method) where T : class, ISymbol
        {
            T symbol = method?
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

        private string DisplayReturnType(ITypeSymbol typeSymbol)
        {
            var info = new StringBuilder();
            info.Append($"{typeSymbol.Name}");

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Length > 0)
            {
                info.Append($"<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(x => x.Name))}>");
            }
            else if (typeSymbol.IsTupleType && typeSymbol is INamedTypeSymbol tupleTypeSymbol)
            {
                return $"({string.Join(", ", tupleTypeSymbol.TupleElements.Select(x => $"{x.Type.Name} {x.Name}"))})";
            }

            return info.ToString();
        }

        private string DisplayParameter(IParameterSymbol parameter)
        {
            if (parameter.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                return $"{arrayTypeSymbol.ElementType.Name}[] {parameter.Name}";
            }
            else if (parameter.Type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Length > 0)
            {
                return $"{parameter.Type.Name}<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(x => x.Name))}> {parameter.Name}";
            }
            else if (parameter.Type.IsTupleType && parameter.Type is INamedTypeSymbol tupleTypeSymbol)
            {
                return $"({string.Join(", ", tupleTypeSymbol.TupleElements.Select(x => $"{x.Type.Name} {x.Name}"))})";
            }
            else
            {
                return $"{parameter.Type.Name} {parameter.Name}";
            }
        }
    }
}