using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace RoslynSymbols
{
    public class RoslynSymbolVisitor : SymbolVisitor
    {
        public override void DefaultVisit(ISymbol symbol)
        {
            //if (symbol.DeclaredAccessibility != Accessibility.Public && symbol.DeclaredAccessibility != Accessibility.NotApplicable)
            //{
            //    return;
            //}

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

            Console.WriteLine($"{symbol} ({kind}): {symbolType}{Environment.NewLine}");
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            // TODO : Test partial methods.
            // TODO : Test method with attributes and parameter with attributes.
            // TODO : Complete accessibility switch.
            // TODO : Check if "this" property (extensions) can be found on the parameter.
            // TODO : "new" on methods.
            // TODO : Test private methods.
            // TODO : Operators.
            // TODO : Test nullables.
            // TODO : Test optional parameters.
            if (symbol.MethodKind != MethodKind.Ordinary)
            {
                DefaultVisit(symbol);
                return;
            }

            ITypeSymbol returnType = symbol.ReturnType;

            // To go in the VisitParameter method, .Accept must be called on each parameter.
            // Same for other type of symbol.
            ImmutableArray<IParameterSymbol> parameters = symbol.Parameters;
            ImmutableArray<ITypeParameterSymbol> typeParameters = symbol.TypeParameters;
            var isInterface = symbol.ContainingType.TypeKind == TypeKind.Interface;

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
            if (!isInterface)
            {
                switch (symbol.DeclaredAccessibility)
                {
                    case Accessibility.Public:
                        info.Append($"public ");
                        break;

                    case Accessibility.Private:
                        info.Append($"private ");
                        break;

                    case Accessibility.Protected:
                        info.Append($"protected ");
                        break;

                    default:
                        break;
                }
                if (symbol.IsAbstract)
                {
                    info.Append($"abstract ");
                }
                else if (symbol.IsVirtual)
                {
                    info.Append($"virtual ");
                }
                else if (symbol.IsOverride)
                {
                    info.Append($"override ");
                }
                else if (symbol.IsStatic)
                {
                    info.Append($"static ");
                }
            }

            if (symbol.ReturnsVoid)
            {
                info.Append($"void {symbol.Name}");
            }
            else
            {
                info.Append($"{(symbol.ReturnsByRef ? "ref " : string.Empty)}{DisplayTypeSymbol(returnType)} {symbol.Name}");
            }

            if (typeParameters.Length > 0)
            {
                info.Append($"<{string.Join(", ", typeParameters)}>");
            }

            info.Append("(");
            if (symbol.IsExtensionMethod)
            {
                info.Append("this ");
            }
            info.Append(string.Join(", ", parameters.Select(p => DisplayParameter(p))));
            info.Append(")");

            if (typeParameters.Any(x => HasConstraints(x)))
            {
                foreach (ITypeParameterSymbol type in typeParameters.Where(x => HasConstraints(x)))
                {
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

            Console.WriteLine($"{info}");

            var symbolDisplayFormat = new SymbolDisplayFormat(
              typeQualificationStyle:
                  SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
              genericsOptions:
                  SymbolDisplayGenericsOptions.IncludeTypeConstraints
                  | SymbolDisplayGenericsOptions.IncludeVariance
                  | SymbolDisplayGenericsOptions.IncludeTypeParameters,
              memberOptions:
                  SymbolDisplayMemberOptions.IncludeAccessibility
                  | SymbolDisplayMemberOptions.IncludeExplicitInterface
                  | SymbolDisplayMemberOptions.IncludeModifiers
                  | SymbolDisplayMemberOptions.IncludeParameters
                  | SymbolDisplayMemberOptions.IncludeType,
              delegateStyle:
                  SymbolDisplayDelegateStyle.NameAndSignature,
              extensionMethodStyle:
                  SymbolDisplayExtensionMethodStyle.StaticMethod,
              parameterOptions:
                  SymbolDisplayParameterOptions.IncludeDefaultValue
                  | SymbolDisplayParameterOptions.IncludeExtensionThis
                  | SymbolDisplayParameterOptions.IncludeName
                  | SymbolDisplayParameterOptions.IncludeParamsRefOut
                  | SymbolDisplayParameterOptions.IncludeType,
              propertyStyle:
                  SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
              kindOptions:
                  SymbolDisplayKindOptions.IncludeTypeKeyword
                  | SymbolDisplayKindOptions.IncludeMemberKeyword
                  | SymbolDisplayKindOptions.IncludeNamespaceKeyword,
              miscellaneousOptions:
                  SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
            Console.WriteLine($"{symbol.ToDisplayString(symbolDisplayFormat)}{Environment.NewLine}");
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

        private string DisplayTypeSymbol(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                return $"{GetTypeName(arrayTypeSymbol.ElementType)}[]";
            }
            else if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Length > 0)
            {
                return $"{GetTypeName(typeSymbol)}<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(x => GetTypeName(x)))}>";
            }
            else if (typeSymbol.IsTupleType && typeSymbol is INamedTypeSymbol tupleTypeSymbol)
            {
                return $"({string.Join(", ", tupleTypeSymbol.TupleElements.Select(x => $"{GetTypeName(x.Type)}{(!x.CorrespondingTupleField?.Equals(x) == true ? $" {x.Name}" : string.Empty)}"))})";
            }

            return $"{GetTypeName(typeSymbol)}";
        }

        private string DisplayParameter(IParameterSymbol parameter)
        {
            var modifier = string.Empty;
            if (parameter.IsParams)
            {
                modifier = "params ";
            }
            else if (parameter.RefKind == RefKind.Out)
            {
                modifier = "out ";
            }
            else if (parameter.RefKind == RefKind.Ref)
            {
                modifier = "ref ";
            }

            return $"{modifier}{DisplayTypeSymbol(parameter.Type)} {parameter.Name}";
        }

        private string GetTypeName(ITypeSymbol typeSymbol)
        {
            // Special type (https://github.com/dotnet/roslyn/blob/d4dab355b96955aca5b4b0ebf6282575fad78ba8/src/Compilers/CSharp/Portable/SymbolDisplay/SymbolDisplayVisitor.Types.cs#L489)
            var specialTypeName = GetSpecialTypeName(typeSymbol.SpecialType);
            return specialTypeName ?? typeSymbol.Name;

            string GetSpecialTypeName(SpecialType specialType)
            {
                switch (specialType)
                {
                    case SpecialType.System_Void:
                        return "void";

                    case SpecialType.System_SByte:
                        return "sbyte";

                    case SpecialType.System_Int16:
                        return "short";

                    case SpecialType.System_Int32:
                        return "int";

                    case SpecialType.System_Int64:
                        return "long";

                    case SpecialType.System_Byte:
                        return "byte";

                    case SpecialType.System_UInt16:
                        return "ushort";

                    case SpecialType.System_UInt32:
                        return "uint";

                    case SpecialType.System_UInt64:
                        return "ulong";

                    case SpecialType.System_Single:
                        return "float";

                    case SpecialType.System_Double:
                        return "double";

                    case SpecialType.System_Decimal:
                        return "decimal";

                    case SpecialType.System_Char:
                        return "char";

                    case SpecialType.System_Boolean:
                        return "bool";

                    case SpecialType.System_String:
                        return "string";

                    case SpecialType.System_Object:
                        return "object";

                    default:
                        return null;
                }
            }
        }
    }
}