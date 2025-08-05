using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.CodeBuilder.Roslyn;

/// <summary>
/// Converts Roslyn AST nodes to code definitions.
/// Provides accurate conversion from Roslyn syntax trees to immutable code structures.
/// </summary>
public sealed class RoslynToDefinitionConverter
{
    /// <summary>
    /// Converts a Roslyn syntax tree to a compilation unit definition.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to convert.</param>
    /// <returns>The compilation unit definition.</returns>
    public CompilationUnitDefinition ConvertCompilationUnit(SyntaxTree syntaxTree)
    {
        ArgumentNullException.ThrowIfNull(syntaxTree);

        var root = syntaxTree.GetRoot();
        var compilationUnit = root as CompilationUnitSyntax 
            ?? throw new ArgumentException("Root must be a CompilationUnitSyntax", nameof(syntaxTree));

        var usings = ExtractUsings(compilationUnit);
        var namespaces = ExtractNamespaces(compilationUnit);
        var topLevelClasses = ExtractTopLevelClasses(compilationUnit);
        var topLevelInterfaces = ExtractTopLevelInterfaces(compilationUnit);

        return new CompilationUnitDefinition
        {
            Name = syntaxTree.FilePath,
            Location = CreateSourceLocation(compilationUnit),
            Usings = usings,
            Namespaces = namespaces,
            Classes = topLevelClasses,
            Interfaces = topLevelInterfaces
        };
    }

    /// <summary>
    /// Converts a class declaration syntax to a class definition.
    /// </summary>
    /// <param name="classDeclaration">The class declaration to convert.</param>
    /// <returns>The class definition.</returns>
    public ClassDefinition ConvertClass(ClassDeclarationSyntax classDeclaration)
    {
        ArgumentNullException.ThrowIfNull(classDeclaration);

        var name = classDeclaration.Identifier.ValueText;
        var access = ExtractAccessModifier(classDeclaration.Modifiers);
        var baseClass = ExtractBaseClass(classDeclaration);
        var interfaces = ExtractImplementedInterfaces(classDeclaration);
        var methods = ExtractMethods(classDeclaration);
        var properties = ExtractProperties(classDeclaration);
        var fields = ExtractFields(classDeclaration);
        var attributes = ExtractAttributes(classDeclaration.AttributeLists);
        var modifiers = ExtractClassModifiers(classDeclaration.Modifiers);
        var genericParameters = ExtractGenericParameters(classDeclaration.TypeParameterList);
        var documentation = ExtractDocumentation(classDeclaration);

        return new ClassDefinition
        {
            Name = name,
            Location = CreateSourceLocation(classDeclaration),
            Access = access,
            BaseClass = baseClass,
            Interfaces = interfaces,
            Methods = methods,
            Properties = properties,
            Fields = fields,
            Attributes = attributes,
            IsAbstract = modifiers.IsAbstract,
            IsSealed = modifiers.IsSealed,
            IsStatic = modifiers.IsStatic,
            IsPartial = modifiers.IsPartial,
            GenericParameters = genericParameters,
            Documentation = documentation
        };
    }

    /// <summary>
    /// Converts an interface declaration syntax to an interface definition.
    /// </summary>
    /// <param name="interfaceDeclaration">The interface declaration to convert.</param>
    /// <returns>The interface definition.</returns>
    public InterfaceDefinition ConvertInterface(InterfaceDeclarationSyntax interfaceDeclaration)
    {
        ArgumentNullException.ThrowIfNull(interfaceDeclaration);

        var name = interfaceDeclaration.Identifier.ValueText;
        var access = ExtractAccessModifier(interfaceDeclaration.Modifiers);
        var baseInterfaces = ExtractBaseInterfaces(interfaceDeclaration);
        var methods = ExtractMethods(interfaceDeclaration);
        var properties = ExtractProperties(interfaceDeclaration);
        var attributes = ExtractAttributes(interfaceDeclaration.AttributeLists);
        var genericParameters = ExtractGenericParameters(interfaceDeclaration.TypeParameterList);
        var documentation = ExtractDocumentation(interfaceDeclaration);

        return new InterfaceDefinition
        {
            Name = name,
            Location = CreateSourceLocation(interfaceDeclaration),
            Access = access,
            BaseInterfaces = baseInterfaces,
            Methods = methods,
            Properties = properties,
            Attributes = attributes,
            GenericParameters = genericParameters,
            Documentation = documentation
        };
    }

    /// <summary>
    /// Converts a method declaration syntax to a method definition.
    /// </summary>
    /// <param name="methodDeclaration">The method declaration to convert.</param>
    /// <returns>The method definition.</returns>
    public MethodDefinition ConvertMethod(MethodDeclarationSyntax methodDeclaration)
    {
        ArgumentNullException.ThrowIfNull(methodDeclaration);

        var name = methodDeclaration.Identifier.ValueText;
        var access = ExtractAccessModifier(methodDeclaration.Modifiers);
        var returnType = methodDeclaration.ReturnType.ToString();
        var parameters = ExtractParameters(methodDeclaration.ParameterList);
        var body = ExtractMethodBody(methodDeclaration);
        var attributes = ExtractAttributes(methodDeclaration.AttributeLists);
        var modifiers = ExtractMethodModifiers(methodDeclaration.Modifiers);
        var documentation = ExtractDocumentation(methodDeclaration);

        return new MethodDefinition
        {
            Name = name,
            Location = CreateSourceLocation(methodDeclaration),
            Access = access,
            ReturnType = returnType,
            Parameters = parameters,
            Body = body,
            Attributes = attributes,
            IsAbstract = modifiers.IsAbstract,
            IsVirtual = modifiers.IsVirtual,
            IsOverride = modifiers.IsOverride,
            IsStatic = modifiers.IsStatic,
            Documentation = documentation
        };
    }

    /// <summary>
    /// Converts a property declaration syntax to a property definition.
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to convert.</param>
    /// <returns>The property definition.</returns>
    public PropertyDefinition ConvertProperty(PropertyDeclarationSyntax propertyDeclaration)
    {
        ArgumentNullException.ThrowIfNull(propertyDeclaration);

        var name = propertyDeclaration.Identifier.ValueText;
        var access = ExtractAccessModifier(propertyDeclaration.Modifiers);
        var type = propertyDeclaration.Type.ToString();
        var getter = ExtractPropertyGetter(propertyDeclaration);
        var setter = ExtractPropertySetter(propertyDeclaration);
        var attributes = ExtractAttributes(propertyDeclaration.AttributeLists);
        var modifiers = ExtractPropertyModifiers(propertyDeclaration.Modifiers);
        var documentation = ExtractDocumentation(propertyDeclaration);

        return new PropertyDefinition
        {
            Name = name,
            Location = CreateSourceLocation(propertyDeclaration),
            Access = access,
            Type = type,
            Getter = getter,
            Setter = setter,
            Attributes = attributes,
            IsStatic = modifiers.IsStatic,
            IsVirtual = modifiers.IsVirtual,
            IsOverride = modifiers.IsOverride,
            Documentation = documentation
        };
    }

    private IReadOnlyList<string> ExtractUsings(CompilationUnitSyntax compilationUnit)
    {
        return compilationUnit.Usings
            .Select(u => u.Name?.ToString() ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToArray();
    }

    private IReadOnlyList<NamespaceDefinition> ExtractNamespaces(CompilationUnitSyntax compilationUnit)
    {
        var namespaces = new List<NamespaceDefinition>();

        // Handle regular namespace declarations
        foreach (var namespaceDecl in compilationUnit.Members.OfType<NamespaceDeclarationSyntax>())
        {
            namespaces.Add(ConvertNamespace(namespaceDecl));
        }

        // Handle file-scoped namespace declarations
        foreach (var fileScopedNamespaceDecl in compilationUnit.Members.OfType<FileScopedNamespaceDeclarationSyntax>())
        {
            namespaces.Add(ConvertFileScopedNamespace(fileScopedNamespaceDecl));
        }

        return namespaces;
    }

    private NamespaceDefinition ConvertNamespace(BaseNamespaceDeclarationSyntax namespaceDecl)
    {
        var name = namespaceDecl.Name.ToString();
        var classes = ExtractClasses(namespaceDecl.Members);
        var interfaces = ExtractInterfaces(namespaceDecl.Members);
        var nestedNamespaces = ExtractNestedNamespaces(namespaceDecl.Members);

        return new NamespaceDefinition
        {
            Name = name,
            Location = CreateSourceLocation(namespaceDecl),
            Classes = classes,
            Interfaces = interfaces,
            Namespaces = nestedNamespaces
        };
    }

    private NamespaceDefinition ConvertFileScopedNamespace(FileScopedNamespaceDeclarationSyntax namespaceDecl)
    {
        return ConvertNamespace(namespaceDecl);
    }

    private IReadOnlyList<ClassDefinition> ExtractTopLevelClasses(CompilationUnitSyntax compilationUnit)
    {
        return ExtractClasses(compilationUnit.Members);
    }

    private IReadOnlyList<ClassDefinition> ExtractClasses(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<ClassDeclarationSyntax>()
            .Select(ConvertClass)
            .ToArray();
    }

    private IReadOnlyList<InterfaceDefinition> ExtractTopLevelInterfaces(CompilationUnitSyntax compilationUnit)
    {
        return ExtractInterfaces(compilationUnit.Members);
    }

    private IReadOnlyList<InterfaceDefinition> ExtractInterfaces(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<InterfaceDeclarationSyntax>()
            .Select(ConvertInterface)
            .ToArray();
    }

    private IReadOnlyList<NamespaceDefinition> ExtractNestedNamespaces(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<NamespaceDeclarationSyntax>()
            .Select(ConvertNamespace)
            .ToArray();
    }

    private IReadOnlyList<MethodDefinition> ExtractMethods(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.Members.OfType<MethodDeclarationSyntax>()
            .Select(ConvertMethod)
            .ToArray();
    }

    private IReadOnlyList<PropertyDefinition> ExtractProperties(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.Members.OfType<PropertyDeclarationSyntax>()
            .Select(ConvertProperty)
            .ToArray();
    }

    private IReadOnlyList<FieldDefinition> ExtractFields(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.Members.OfType<FieldDeclarationSyntax>()
            .SelectMany(f => f.Declaration.Variables.Select(v => ConvertField(f, v)))
            .ToArray();
    }

    private FieldDefinition ConvertField(FieldDeclarationSyntax fieldDecl, VariableDeclaratorSyntax variable)
    {
        var name = variable.Identifier.ValueText;
        var access = ExtractAccessModifier(fieldDecl.Modifiers);
        var type = fieldDecl.Declaration.Type.ToString();
        var initialValue = variable.Initializer?.Value.ToString();
        var attributes = ExtractAttributes(fieldDecl.AttributeLists);
        var modifiers = ExtractFieldModifiers(fieldDecl.Modifiers);
        var documentation = ExtractDocumentation(fieldDecl);

        return new FieldDefinition
        {
            Name = name,
            Location = CreateSourceLocation(variable),
            Access = access,
            Type = type,
            InitialValue = initialValue,
            Attributes = attributes,
            IsStatic = modifiers.IsStatic,
            IsReadOnly = modifiers.IsReadOnly,
            IsConst = modifiers.IsConst,
            Documentation = documentation
        };
    }

    private IReadOnlyList<ParameterDefinition> ExtractParameters(ParameterListSyntax? parameterList)
    {
        if (parameterList == null)
        {
            return Array.Empty<ParameterDefinition>();
        }

        return parameterList.Parameters
            .Select(ConvertParameter)
            .ToArray();
    }

    private ParameterDefinition ConvertParameter(ParameterSyntax parameter)
    {
        var name = parameter.Identifier.ValueText;
        var type = parameter.Type?.ToString() ?? "object";
        var defaultValue = parameter.Default?.Value.ToString();
        var attributes = ExtractAttributes(parameter.AttributeLists);
        var modifiers = ExtractParameterModifiers(parameter.Modifiers);

        return new ParameterDefinition
        {
            Name = name,
            Location = CreateSourceLocation(parameter),
            Type = type,
            DefaultValue = defaultValue,
            Attributes = attributes,
            IsOptional = parameter.Default != null,
            IsRef = modifiers.IsRef,
            IsOut = modifiers.IsOut,
            IsParams = modifiers.IsParams
        };
    }

    private AccessModifier ExtractAccessModifier(SyntaxTokenList modifiers)
    {
        if (modifiers.Any(SyntaxKind.PublicKeyword)) return AccessModifier.Public;
        if (modifiers.Any(SyntaxKind.PrivateKeyword)) return AccessModifier.Private;
        if (modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            if (modifiers.Any(SyntaxKind.InternalKeyword)) return AccessModifier.ProtectedInternal;
            return AccessModifier.Protected;
        }
        if (modifiers.Any(SyntaxKind.InternalKeyword)) return AccessModifier.Internal;
        
        return AccessModifier.None;
    }

    private string? ExtractBaseClass(ClassDeclarationSyntax classDecl)
    {
        var baseType = classDecl.BaseList?.Types.FirstOrDefault()?.Type.ToString();
        
        // Simple heuristic: if it starts with 'I' and is PascalCase, it's likely an interface
        if (!string.IsNullOrEmpty(baseType) && baseType.Length > 1 && 
            baseType[0] == 'I' && char.IsUpper(baseType[1]))
        {
            return null; // This is likely an interface, not a base class
        }
        
        return baseType;
    }

    private IReadOnlyList<string> ExtractImplementedInterfaces(ClassDeclarationSyntax classDecl)
    {
        if (classDecl.BaseList == null)
        {
            return Array.Empty<string>();
        }

        var interfaces = new List<string>();
        foreach (var baseType in classDecl.BaseList.Types)
        {
            var typeName = baseType.Type.ToString();
            
            // Simple heuristic: if it starts with 'I' and is PascalCase, it's likely an interface
            if (typeName.Length > 1 && typeName[0] == 'I' && char.IsUpper(typeName[1]))
            {
                interfaces.Add(typeName);
            }
        }

        return interfaces;
    }

    private IReadOnlyList<string> ExtractBaseInterfaces(InterfaceDeclarationSyntax interfaceDecl)
    {
        if (interfaceDecl.BaseList == null)
        {
            return Array.Empty<string>();
        }

        return interfaceDecl.BaseList.Types
            .Select(t => t.Type.ToString())
            .ToArray();
    }

    private IReadOnlyList<AttributeDefinition> ExtractAttributes(SyntaxList<AttributeListSyntax> attributeLists)
    {
        var attributes = new List<AttributeDefinition>();

        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                var arguments = attribute.ArgumentList?.Arguments
                    .Select(arg => (object)arg.Expression.ToString())
                    .ToArray() ?? Array.Empty<object>();

                attributes.Add(new AttributeDefinition
                {
                    Name = name,
                    Location = CreateSourceLocation(attribute),
                    Arguments = arguments
                });
            }
        }

        return attributes;
    }

    private IReadOnlyList<GenericParameterDefinition> ExtractGenericParameters(TypeParameterListSyntax? typeParameterList)
    {
        if (typeParameterList == null)
        {
            return Array.Empty<GenericParameterDefinition>();
        }

        return typeParameterList.Parameters
            .Select(p => new GenericParameterDefinition
            {
                Name = p.Identifier.ValueText,
                Location = CreateSourceLocation(p),
                Constraints = Array.Empty<string>() // TODO: Extract constraints
            })
            .ToArray();
    }

    private string? ExtractDocumentation(SyntaxNode node)
    {
        var documentationComment = node.GetLeadingTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

        return documentationComment.IsKind(SyntaxKind.None) ? null : documentationComment.ToString();
    }

    private string? ExtractMethodBody(MethodDeclarationSyntax methodDecl)
    {
        if (methodDecl.Body != null)
        {
            return methodDecl.Body.ToString();
        }

        if (methodDecl.ExpressionBody != null)
        {
            return methodDecl.ExpressionBody.ToString();
        }

        return null;
    }

    private AccessorDefinition? ExtractPropertyGetter(PropertyDeclarationSyntax propertyDecl)
    {
        var getter = propertyDecl.AccessorList?.Accessors
            .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

        if (getter == null) return null;

        var access = ExtractAccessModifier(getter.Modifiers);
        var body = getter.Body?.ToString() ?? getter.ExpressionBody?.ToString();

        return new AccessorDefinition
        {
            Location = CreateSourceLocation(getter),
            Access = access == AccessModifier.None ? null : access,
            Body = body
        };
    }

    private AccessorDefinition? ExtractPropertySetter(PropertyDeclarationSyntax propertyDecl)
    {
        var setter = propertyDecl.AccessorList?.Accessors
            .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || 
                                a.IsKind(SyntaxKind.InitAccessorDeclaration));

        if (setter == null) return null;

        var access = ExtractAccessModifier(setter.Modifiers);
        var body = setter.Body?.ToString() ?? setter.ExpressionBody?.ToString();

        return new AccessorDefinition
        {
            Location = CreateSourceLocation(setter),
            Access = access == AccessModifier.None ? null : access,
            Body = body
        };
    }

    private (bool IsAbstract, bool IsSealed, bool IsStatic, bool IsPartial) ExtractClassModifiers(SyntaxTokenList modifiers)
    {
        return (
            modifiers.Any(SyntaxKind.AbstractKeyword),
            modifiers.Any(SyntaxKind.SealedKeyword),
            modifiers.Any(SyntaxKind.StaticKeyword),
            modifiers.Any(SyntaxKind.PartialKeyword)
        );
    }

    private (bool IsAbstract, bool IsVirtual, bool IsOverride, bool IsStatic) ExtractMethodModifiers(SyntaxTokenList modifiers)
    {
        return (
            modifiers.Any(SyntaxKind.AbstractKeyword),
            modifiers.Any(SyntaxKind.VirtualKeyword),
            modifiers.Any(SyntaxKind.OverrideKeyword),
            modifiers.Any(SyntaxKind.StaticKeyword)
        );
    }

    private (bool IsStatic, bool IsVirtual, bool IsOverride) ExtractPropertyModifiers(SyntaxTokenList modifiers)
    {
        return (
            modifiers.Any(SyntaxKind.StaticKeyword),
            modifiers.Any(SyntaxKind.VirtualKeyword),
            modifiers.Any(SyntaxKind.OverrideKeyword)
        );
    }

    private (bool IsStatic, bool IsReadOnly, bool IsConst) ExtractFieldModifiers(SyntaxTokenList modifiers)
    {
        return (
            modifiers.Any(SyntaxKind.StaticKeyword),
            modifiers.Any(SyntaxKind.ReadOnlyKeyword),
            modifiers.Any(SyntaxKind.ConstKeyword)
        );
    }

    private (bool IsRef, bool IsOut, bool IsParams) ExtractParameterModifiers(SyntaxTokenList modifiers)
    {
        return (
            modifiers.Any(SyntaxKind.RefKeyword),
            modifiers.Any(SyntaxKind.OutKeyword),
            modifiers.Any(SyntaxKind.ParamsKeyword)
        );
    }

    private static SourceLocation CreateSourceLocation(SyntaxNode node)
    {
        var span = node.GetLocation().GetLineSpan();
        return new SourceLocation
        {
            FilePath = node.SyntaxTree?.FilePath,
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1,
            StartPosition = node.SpanStart,
            EndPosition = node.Span.End
        };
    }
}