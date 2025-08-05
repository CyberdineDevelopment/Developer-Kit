using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.CodeGeneration;

/// <summary>
/// Generates C# code from immutable code definitions.
/// This is separate from the builder pattern and focused only on string generation.
/// </summary>
public sealed class CSharpCodeGenerator : ICodeGenerator
{
    private readonly CodeFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpCodeGenerator"/> class.
    /// </summary>
    /// <param name="formatter">Optional code formatter. If null, uses default formatting.</param>
    public CSharpCodeGenerator(CodeFormatter? formatter = null)
    {
        _formatter = formatter ?? new CodeFormatter();
    }

    /// <inheritdoc/>
    public string Language => "csharp";

    /// <inheritdoc/>
    public string Generate(IAstNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return node switch
        {
            ClassDefinition classDef => GenerateClass(classDef),
            MethodDefinition methodDef => GenerateMethod(methodDef),
            PropertyDefinition propertyDef => GenerateProperty(propertyDef),
            FieldDefinition fieldDef => GenerateField(fieldDef),
            ParameterDefinition paramDef => GenerateParameter(paramDef),
            NamespaceDefinition namespaceDef => GenerateNamespace(namespaceDef),
            CompilationUnitDefinition compilationDef => GenerateCompilationUnit(compilationDef),
            InterfaceDefinition interfaceDef => GenerateInterface(interfaceDef),
            _ => throw new NotSupportedException($"Code generation not supported for node type: {node.GetType().Name}")
        };
    }

    /// <inheritdoc/>
    public string GenerateMultiple(IEnumerable<IAstNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var sb = new StringBuilder();
        foreach (var node in nodes)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(Generate(node));
        }
        return sb.ToString();
    }

    private string GenerateClass(ClassDefinition classDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(classDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {classDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in classDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        // Generate class declaration
        var declaration = new StringBuilder();
        
        if (classDef.Access != AccessModifier.None)
        {
            declaration.Append(GetAccessModifierString(classDef.Access));
            declaration.Append(' ');
        }

        if (classDef.IsStatic)
        {
            declaration.Append("static ");
        }
        else if (classDef.IsAbstract)
        {
            declaration.Append("abstract ");
        }
        else if (classDef.IsSealed)
        {
            declaration.Append("sealed ");
        }

        if (classDef.IsPartial)
        {
            declaration.Append("partial ");
        }

        declaration.Append("class ");
        declaration.Append(classDef.Name);

        // Add generic parameters
        if (classDef.GenericParameters.Count > 0)
        {
            declaration.Append('<');
            declaration.Append(string.Join(", ", classDef.GenericParameters.Select(p => p.Name)));
            declaration.Append('>');
        }

        // Add inheritance
        var inheritanceList = new List<string>();
        if (!string.IsNullOrEmpty(classDef.BaseClass))
        {
            inheritanceList.Add(classDef.BaseClass);
        }
        inheritanceList.AddRange(classDef.Interfaces);

        if (inheritanceList.Count > 0)
        {
            declaration.Append(" : ");
            declaration.Append(string.Join(", ", inheritanceList));
        }

        sb.AppendLine(declaration.ToString());
        sb.AppendLine("{");

        // Generate class body
        using (_formatter.IncreaseIndent())
        {
            // Fields first
            foreach (var field in classDef.Fields)
            {
                var fieldCode = GenerateField(field);
                sb.AppendLine(_formatter.IndentLines(fieldCode));
                sb.AppendLine();
            }

            // Properties
            foreach (var property in classDef.Properties)
            {
                var propertyCode = GenerateProperty(property);
                sb.AppendLine(_formatter.IndentLines(propertyCode));
                sb.AppendLine();
            }

            // Methods
            foreach (var method in classDef.Methods)
            {
                var methodCode = GenerateMethod(method);
                sb.AppendLine(_formatter.IndentLines(methodCode));
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        return sb.ToString().TrimEnd();
    }

    private string GenerateMethod(MethodDefinition methodDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(methodDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {methodDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in methodDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        // Generate method signature
        var signature = new StringBuilder();

        if (methodDef.Access != AccessModifier.None)
        {
            signature.Append(GetAccessModifierString(methodDef.Access));
            signature.Append(' ');
        }

        if (methodDef.IsStatic)
        {
            signature.Append("static ");
        }

        if (methodDef.IsAbstract)
        {
            signature.Append("abstract ");
        }
        else if (methodDef.IsVirtual)
        {
            signature.Append("virtual ");
        }
        else if (methodDef.IsOverride)
        {
            signature.Append("override ");
        }

        signature.Append(methodDef.ReturnType);
        signature.Append(' ');
        signature.Append(methodDef.Name);
        signature.Append('(');

        if (methodDef.Parameters.Count > 0)
        {
            signature.Append(string.Join(", ", methodDef.Parameters.Select(GenerateParameter)));
        }

        signature.Append(')');

        sb.Append(signature.ToString());

        // Method body
        if (methodDef.IsAbstract || string.IsNullOrEmpty(methodDef.Body))
        {
            sb.AppendLine(";");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("{");
            using (_formatter.IncreaseIndent())
            {
                sb.AppendLine(_formatter.IndentLines(methodDef.Body));
            }
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateProperty(PropertyDefinition propertyDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(propertyDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {propertyDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in propertyDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        // Generate property declaration
        var declaration = new StringBuilder();

        if (propertyDef.Access != AccessModifier.None)
        {
            declaration.Append(GetAccessModifierString(propertyDef.Access));
            declaration.Append(' ');
        }

        if (propertyDef.IsStatic)
        {
            declaration.Append("static ");
        }

        if (propertyDef.IsVirtual)
        {
            declaration.Append("virtual ");
        }
        else if (propertyDef.IsOverride)
        {
            declaration.Append("override ");
        }

        declaration.Append(propertyDef.Type);
        declaration.Append(' ');
        declaration.Append(propertyDef.Name);

        sb.Append(declaration.ToString());

        // Property accessors
        if (propertyDef.Getter != null || propertyDef.Setter != null)
        {
            var isAutoImplemented = (propertyDef.Getter?.Body == null || string.IsNullOrEmpty(propertyDef.Getter.Body)) &&
                                   (propertyDef.Setter?.Body == null || string.IsNullOrEmpty(propertyDef.Setter.Body));

            if (isAutoImplemented)
            {
                sb.Append(" { ");
                if (propertyDef.Getter != null)
                {
                    if (propertyDef.Getter.Access.HasValue)
                    {
                        sb.Append(GetAccessModifierString(propertyDef.Getter.Access.Value));
                        sb.Append(' ');
                    }
                    sb.Append("get; ");
                }
                if (propertyDef.Setter != null)
                {
                    if (propertyDef.Setter.Access.HasValue)
                    {
                        sb.Append(GetAccessModifierString(propertyDef.Setter.Access.Value));
                        sb.Append(' ');
                    }
                    sb.Append("set; ");
                }
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("{");
                using (_formatter.IncreaseIndent())
                {
                    if (propertyDef.Getter != null)
                    {
                        sb.Append(_formatter.IndentText("get"));
                        if (!string.IsNullOrEmpty(propertyDef.Getter.Body))
                        {
                            sb.AppendLine();
                            sb.AppendLine(_formatter.IndentText("{"));
                            using (_formatter.IncreaseIndent())
                            {
                                sb.AppendLine(_formatter.IndentLines(propertyDef.Getter.Body));
                            }
                            sb.AppendLine(_formatter.IndentText("}"));
                        }
                        else
                        {
                            sb.AppendLine(";");
                        }
                    }

                    if (propertyDef.Setter != null)
                    {
                        sb.Append(_formatter.IndentText("set"));
                        if (!string.IsNullOrEmpty(propertyDef.Setter.Body))
                        {
                            sb.AppendLine();
                            sb.AppendLine(_formatter.IndentText("{"));
                            using (_formatter.IncreaseIndent())
                            {
                                sb.AppendLine(_formatter.IndentLines(propertyDef.Setter.Body));
                            }
                            sb.AppendLine(_formatter.IndentText("}"));
                        }
                        else
                        {
                            sb.AppendLine(";");
                        }
                    }
                }
                sb.AppendLine("}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateField(FieldDefinition fieldDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(fieldDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {fieldDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in fieldDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        // Generate field declaration
        var declaration = new StringBuilder();

        if (fieldDef.Access != AccessModifier.None)
        {
            declaration.Append(GetAccessModifierString(fieldDef.Access));
            declaration.Append(' ');
        }

        if (fieldDef.IsStatic)
        {
            declaration.Append("static ");
        }

        if (fieldDef.IsConst)
        {
            declaration.Append("const ");
        }
        else if (fieldDef.IsReadOnly)
        {
            declaration.Append("readonly ");
        }

        declaration.Append(fieldDef.Type);
        declaration.Append(' ');
        declaration.Append(fieldDef.Name);

        if (!string.IsNullOrEmpty(fieldDef.InitialValue))
        {
            declaration.Append(" = ");
            declaration.Append(fieldDef.InitialValue);
        }

        declaration.Append(';');

        sb.Append(declaration.ToString());

        return sb.ToString();
    }

    private string GenerateParameter(ParameterDefinition paramDef)
    {
        var sb = new StringBuilder();

        if (paramDef.IsRef)
        {
            sb.Append("ref ");
        }
        else if (paramDef.IsOut)
        {
            sb.Append("out ");
        }
        else if (paramDef.IsParams)
        {
            sb.Append("params ");
        }

        sb.Append(paramDef.Type);
        sb.Append(' ');
        sb.Append(paramDef.Name);

        if (!string.IsNullOrEmpty(paramDef.DefaultValue))
        {
            sb.Append(" = ");
            sb.Append(paramDef.DefaultValue);
        }

        return sb.ToString();
    }

    private string GenerateNamespace(NamespaceDefinition namespaceDef)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"namespace {namespaceDef.Name};");
        sb.AppendLine();

        foreach (var classDef in namespaceDef.Classes)
        {
            sb.AppendLine(GenerateClass(classDef));
            sb.AppendLine();
        }

        foreach (var interfaceDef in namespaceDef.Interfaces)
        {
            sb.AppendLine(GenerateInterface(interfaceDef));
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateCompilationUnit(CompilationUnitDefinition compilationDef)
    {
        var sb = new StringBuilder();

        // Generate using statements
        foreach (var usingStatement in compilationDef.Usings)
        {
            sb.AppendLine($"using {usingStatement};");
        }

        if (compilationDef.Usings.Count > 0)
        {
            sb.AppendLine();
        }

        // Generate namespaces
        foreach (var namespaceDef in compilationDef.Namespaces)
        {
            sb.AppendLine(GenerateNamespace(namespaceDef));
            sb.AppendLine();
        }

        // Generate top-level classes
        foreach (var classDef in compilationDef.Classes)
        {
            sb.AppendLine(GenerateClass(classDef));
            sb.AppendLine();
        }

        // Generate top-level interfaces
        foreach (var interfaceDef in compilationDef.Interfaces)
        {
            sb.AppendLine(GenerateInterface(interfaceDef));
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private string GenerateInterface(InterfaceDefinition interfaceDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(interfaceDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {interfaceDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in interfaceDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        // Generate interface declaration
        var declaration = new StringBuilder();

        if (interfaceDef.Access != AccessModifier.None)
        {
            declaration.Append(GetAccessModifierString(interfaceDef.Access));
            declaration.Append(' ');
        }

        declaration.Append("interface ");
        declaration.Append(interfaceDef.Name);

        // Add generic parameters
        if (interfaceDef.GenericParameters.Count > 0)
        {
            declaration.Append('<');
            declaration.Append(string.Join(", ", interfaceDef.GenericParameters.Select(p => p.Name)));
            declaration.Append('>');
        }

        // Add base interfaces
        if (interfaceDef.BaseInterfaces.Count > 0)
        {
            declaration.Append(" : ");
            declaration.Append(string.Join(", ", interfaceDef.BaseInterfaces));
        }

        sb.AppendLine(declaration.ToString());
        sb.AppendLine("{");

        // Generate interface body
        using (_formatter.IncreaseIndent())
        {
            // Properties
            foreach (var property in interfaceDef.Properties)
            {
                var propertyCode = GenerateInterfaceProperty(property);
                sb.AppendLine(_formatter.IndentLines(propertyCode));
                sb.AppendLine();
            }

            // Methods
            foreach (var method in interfaceDef.Methods)
            {
                var methodCode = GenerateInterfaceMethod(method);
                sb.AppendLine(_formatter.IndentLines(methodCode));
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        return sb.ToString().TrimEnd();
    }

    private string GenerateInterfaceProperty(PropertyDefinition propertyDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(propertyDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {propertyDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in propertyDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        sb.Append(propertyDef.Type);
        sb.Append(' ');
        sb.Append(propertyDef.Name);
        sb.Append(" { ");

        if (propertyDef.Getter != null)
        {
            sb.Append("get; ");
        }

        if (propertyDef.Setter != null)
        {
            sb.Append("set; ");
        }

        sb.Append("}");

        return sb.ToString();
    }

    private string GenerateInterfaceMethod(MethodDefinition methodDef)
    {
        var sb = new StringBuilder();

        // Generate XML documentation
        if (!string.IsNullOrEmpty(methodDef.Documentation))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {methodDef.Documentation}");
            sb.AppendLine("/// </summary>");
        }

        // Generate attributes
        foreach (var attr in methodDef.Attributes)
        {
            sb.AppendLine(GenerateAttribute(attr));
        }

        sb.Append(methodDef.ReturnType);
        sb.Append(' ');
        sb.Append(methodDef.Name);
        sb.Append('(');

        if (methodDef.Parameters.Count > 0)
        {
            sb.Append(string.Join(", ", methodDef.Parameters.Select(GenerateParameter)));
        }

        sb.Append(");");

        return sb.ToString();
    }

    private string GenerateAttribute(AttributeDefinition attrDef)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(attrDef.Name);

        if (attrDef.Arguments.Count > 0)
        {
            sb.Append('(');
            sb.Append(string.Join(", ", attrDef.Arguments.Select(arg => arg.ToString())));
            sb.Append(')');
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static string GetAccessModifierString(AccessModifier access) => access switch
    {
        AccessModifier.Public => "public",
        AccessModifier.Private => "private",
        AccessModifier.Protected => "protected",
        AccessModifier.Internal => "internal",
        AccessModifier.ProtectedInternal => "protected internal",
        AccessModifier.PrivateProtected => "private protected",
        _ => string.Empty
    };
}

/// <summary>
/// Interface for code generators that convert AST nodes to source code strings.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Gets the language this generator supports.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Generates source code from an AST node.
    /// </summary>
    /// <param name="node">The AST node to generate code from.</param>
    /// <returns>The generated source code.</returns>
    string Generate(IAstNode node);

    /// <summary>
    /// Generates source code from multiple AST nodes.
    /// </summary>
    /// <param name="nodes">The AST nodes to generate code from.</param>
    /// <returns>The generated source code.</returns>
    string GenerateMultiple(IEnumerable<IAstNode> nodes);
}