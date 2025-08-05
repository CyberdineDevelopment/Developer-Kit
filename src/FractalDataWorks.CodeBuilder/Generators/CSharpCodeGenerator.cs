using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Definitions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Generators;

/// <summary>
/// C# code generator that converts AST definitions to C# source code.
/// Implements the visitor pattern for extensible code generation.
/// </summary>
public sealed class CSharpCodeGenerator : ICodeGenerator
{
    /// <inheritdoc/>
    public string Language => "C#";

    /// <inheritdoc/>
    public string Generate(IAstNode node)
    {
        return Generate(node, new CodeGenerationOptions());
    }

    /// <inheritdoc/>
    public string Generate(IAstNode node, CodeGenerationOptions options)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var generator = new CSharpGeneratorVisitor(options);
        return node.Accept(generator);
    }

    /// <summary>
    /// Internal visitor implementation for C# code generation.
    /// </summary>
    private sealed class CSharpGeneratorVisitor : IAstVisitor<string>
    {
        private readonly CodeGenerationOptions _options;
        private readonly CodeFormatter _formatter;

        public CSharpGeneratorVisitor(CodeGenerationOptions options)
        {
            _options = options;
            _formatter = new CodeFormatter(options);
        }

        public string Visit(IAstNode node)
        {
            return node switch
            {
                ClassDefinition classDef => GenerateClass(classDef),
                MethodDefinition methodDef => GenerateMethod(methodDef),
                PropertyDefinition propertyDef => GenerateProperty(propertyDef),
                ConstructorDefinition constructorDef => GenerateConstructor(constructorDef),
                ParameterDefinition parameterDef => GenerateParameter(parameterDef),
                AttributeDefinition attributeDef => GenerateAttribute(attributeDef),
                GenericParameterDefinition genericDef => GenerateGenericParameter(genericDef),
                AccessorDefinition accessorDef => GenerateAccessor(accessorDef),
                _ => throw new NotSupportedException($"Node type '{node.GetType().Name}' is not supported for C# code generation.")
            };
        }

        private string GenerateClass(ClassDefinition classDef)
        {
            var code = new StringBuilder();

            // Generate XML documentation
            if (_options.GenerateDocumentation && !string.IsNullOrWhiteSpace(classDef.XmlDocumentation))
            {
                code.AppendLine(GenerateXmlDocumentation(classDef.XmlDocumentation));
            }

            // Generate attributes
            if (_options.GenerateAttributes && classDef.Attributes.Count > 0)
            {
                foreach (var attribute in classDef.Attributes)
                {
                    code.AppendLine(GenerateAttribute(attribute));
                }
            }

            // Generate class declaration
            var declaration = new StringBuilder();
            
            // Access modifier
            if (classDef.Access != AccessModifier.None)
            {
                declaration.Append(classDef.Access.Keyword);
                declaration.Append(' ');
            }

            // Modifiers
            var modifiers = GetModifierKeywords(classDef.Modifiers);
            if (modifiers.Count > 0)
            {
                declaration.Append(string.Join(" ", modifiers));
                declaration.Append(' ');
            }

            declaration.Append("class ");
            declaration.Append(classDef.Name);

            // Generic parameters
            if (classDef.GenericParameters.Count > 0)
            {
                declaration.Append('<');
                declaration.Append(string.Join(", ", classDef.GenericParameters.Select(p => p.Name)));
                declaration.Append('>');
            }

            // Base types
            if (classDef.BaseTypes.Count > 0)
            {
                declaration.Append(" : ");
                declaration.Append(string.Join(", ", classDef.BaseTypes));
            }

            code.AppendLine(declaration.ToString());

            // Generic constraints
            if (classDef.GenericParameters.Count > 0)
            {
                foreach (var genericParam in classDef.GenericParameters)
                {
                    if (genericParam.Constraints.Count > 0)
                    {
                        code.AppendLine($"    where {genericParam.Name} : {string.Join(", ", genericParam.Constraints)}");
                    }
                }
            }

            code.AppendLine("{");

            // Generate members
            var memberCode = new List<string>();
            foreach (var member in classDef.Members)
            {
                var memberStr = member.Accept(this);
                if (!string.IsNullOrWhiteSpace(memberStr))
                {
                    memberCode.Add(_formatter.Indent(memberStr));
                }
            }

            if (memberCode.Count > 0)
            {
                code.AppendLine(string.Join("\n\n", memberCode));
            }

            code.AppendLine("}");

            return code.ToString().TrimEnd();
        }

        private string GenerateMethod(MethodDefinition methodDef)
        {
            var code = new StringBuilder();

            // Generate XML documentation
            if (_options.GenerateDocumentation && !string.IsNullOrWhiteSpace(methodDef.XmlDocumentation))
            {
                code.AppendLine(GenerateXmlDocumentation(methodDef.XmlDocumentation));
            }

            // Generate attributes
            if (_options.GenerateAttributes && methodDef.Attributes.Count > 0)
            {
                foreach (var attribute in methodDef.Attributes)
                {
                    code.AppendLine(GenerateAttribute(attribute));
                }
            }

            // Generate method declaration
            var declaration = new StringBuilder();
            
            // Access modifier
            if (methodDef.Access != AccessModifier.None)
            {
                declaration.Append(methodDef.Access.Keyword);
                declaration.Append(' ');
            }

            // Modifiers
            var modifiers = GetModifierKeywords(methodDef.Modifiers);
            if (modifiers.Count > 0)
            {
                declaration.Append(string.Join(" ", modifiers));
                declaration.Append(' ');
            }

            declaration.Append(methodDef.ReturnType);
            declaration.Append(' ');
            declaration.Append(methodDef.Name);

            // Generic parameters
            if (methodDef.GenericParameters.Count > 0)
            {
                declaration.Append('<');
                declaration.Append(string.Join(", ", methodDef.GenericParameters.Select(p => p.Name)));
                declaration.Append('>');
            }

            // Parameters
            declaration.Append('(');
            if (methodDef.Parameters.Count > 0)
            {
                var parameters = methodDef.Parameters.Select(p => GenerateParameter(p));
                declaration.Append(string.Join(", ", parameters));
            }
            declaration.Append(')');

            code.Append(declaration.ToString());

            // Generic constraints
            if (methodDef.GenericParameters.Count > 0)
            {
                foreach (var genericParam in methodDef.GenericParameters)
                {
                    if (genericParam.Constraints.Count > 0)
                    {
                        code.AppendLine();
                        code.Append($"    where {genericParam.Name} : {string.Join(", ", genericParam.Constraints)}");
                    }
                }
            }

            // Method body
            if (methodDef.IsAbstract || string.IsNullOrWhiteSpace(methodDef.Body))
            {
                code.AppendLine(";");
            }
            else
            {
                code.AppendLine();
                code.AppendLine("{");
                code.AppendLine(_formatter.Indent(methodDef.Body));
                code.AppendLine("}");
            }

            return code.ToString().TrimEnd();
        }

        private string GenerateProperty(PropertyDefinition propertyDef)
        {
            var code = new StringBuilder();

            // Generate XML documentation
            if (_options.GenerateDocumentation && !string.IsNullOrWhiteSpace(propertyDef.XmlDocumentation))
            {
                code.AppendLine(GenerateXmlDocumentation(propertyDef.XmlDocumentation));
            }

            // Generate attributes
            if (_options.GenerateAttributes && propertyDef.Attributes.Count > 0)
            {
                foreach (var attribute in propertyDef.Attributes)
                {
                    code.AppendLine(GenerateAttribute(attribute));
                }
            }

            // Generate property declaration
            var declaration = new StringBuilder();
            
            // Access modifier
            if (propertyDef.Access != AccessModifier.None)
            {
                declaration.Append(propertyDef.Access.Keyword);
                declaration.Append(' ');
            }

            // Modifiers
            var modifiers = GetModifierKeywords(propertyDef.Modifiers);
            if (modifiers.Count > 0)
            {
                declaration.Append(string.Join(" ", modifiers));
                declaration.Append(' ');
            }

            declaration.Append(propertyDef.Type);
            declaration.Append(' ');
            declaration.Append(propertyDef.Name);

            code.Append(declaration.ToString());

            // Accessors
            var hasBody = (propertyDef.Getter?.Body != null) || 
                         (propertyDef.Setter?.Body != null) || 
                         (propertyDef.Init?.Body != null);

            if (hasBody)
            {
                // Multi-line accessors
                code.AppendLine();
                code.AppendLine("{");

                if (propertyDef.Getter != null)
                {
                    code.AppendLine(_formatter.Indent(GenerateAccessor(propertyDef.Getter)));
                }

                if (propertyDef.Setter != null)
                {
                    code.AppendLine(_formatter.Indent(GenerateAccessor(propertyDef.Setter)));
                }

                if (propertyDef.Init != null)
                {
                    code.AppendLine(_formatter.Indent(GenerateAccessor(propertyDef.Init)));
                }

                code.AppendLine("}");
            }
            else
            {
                // Auto-implemented property
                code.Append(" { ");

                var accessors = new List<string>();
                if (propertyDef.Getter != null)
                {
                    var getterAccess = propertyDef.Getter.Access;
                    if (getterAccess != null && getterAccess != propertyDef.Access)
                    {
                        accessors.Add($"{getterAccess.Keyword} get;");
                    }
                    else
                    {
                        accessors.Add("get;");
                    }
                }

                if (propertyDef.Setter != null)
                {
                    var setterAccess = propertyDef.Setter.Access;
                    if (setterAccess != null && setterAccess != propertyDef.Access)
                    {
                        accessors.Add($"{setterAccess.Keyword} set;");
                    }
                    else
                    {
                        accessors.Add("set;");
                    }
                }

                if (propertyDef.Init != null)
                {
                    var initAccess = propertyDef.Init.Access;
                    if (initAccess != null && initAccess != propertyDef.Access)
                    {
                        accessors.Add($"{initAccess.Keyword} init;");
                    }
                    else
                    {
                        accessors.Add("init;");
                    }
                }

                code.Append(string.Join(" ", accessors));
                code.AppendLine(" }");
            }

            return code.ToString().TrimEnd();
        }

        private string GenerateConstructor(ConstructorDefinition constructorDef)
        {
            var code = new StringBuilder();

            // Generate XML documentation
            if (_options.GenerateDocumentation && !string.IsNullOrWhiteSpace(constructorDef.XmlDocumentation))
            {
                code.AppendLine(GenerateXmlDocumentation(constructorDef.XmlDocumentation));
            }

            // Generate attributes
            if (_options.GenerateAttributes && constructorDef.Attributes.Count > 0)
            {
                foreach (var attribute in constructorDef.Attributes)
                {
                    code.AppendLine(GenerateAttribute(attribute));
                }
            }

            // Generate constructor declaration
            var declaration = new StringBuilder();
            
            // Access modifier (not for static constructors)
            if (!constructorDef.IsStatic && constructorDef.Access != AccessModifier.None)
            {
                declaration.Append(constructorDef.Access.Keyword);
                declaration.Append(' ');
            }

            // Modifiers
            var modifiers = GetModifierKeywords(constructorDef.Modifiers);
            if (modifiers.Count > 0)
            {
                declaration.Append(string.Join(" ", modifiers));
                declaration.Append(' ');
            }

            declaration.Append(constructorDef.Name ?? "Constructor");

            // Parameters (not for static constructors)
            if (!constructorDef.IsStatic)
            {
                declaration.Append('(');
                if (constructorDef.Parameters.Count > 0)
                {
                    var parameters = constructorDef.Parameters.Select(p => GenerateParameter(p));
                    declaration.Append(string.Join(", ", parameters));
                }
                declaration.Append(')');

                // Base/this call
                if (!string.IsNullOrWhiteSpace(constructorDef.BaseCall))
                {
                    declaration.Append(" : ");
                    declaration.Append(constructorDef.BaseCall);
                }
            }
            else
            {
                declaration.Append("()");
            }

            code.Append(declaration.ToString());

            // Constructor body
            if (string.IsNullOrWhiteSpace(constructorDef.Body))
            {
                code.AppendLine(" { }");
            }
            else
            {
                code.AppendLine();
                code.AppendLine("{");
                code.AppendLine(_formatter.Indent(constructorDef.Body));
                code.AppendLine("}");
            }

            return code.ToString().TrimEnd();
        }

        private string GenerateParameter(ParameterDefinition parameterDef)
        {
            var parameter = new StringBuilder();

            // Attributes
            if (_options.GenerateAttributes && parameterDef.Attributes.Count > 0)
            {
                var attrs = parameterDef.Attributes.Select(a => GenerateAttribute(a));
                parameter.Append(string.Join(" ", attrs));
                parameter.Append(' ');
            }

            // Parameter modifiers
            var modifiers = GetParameterModifierKeywords(parameterDef.Modifiers);
            if (modifiers.Count > 0)
            {
                parameter.Append(string.Join(" ", modifiers));
                parameter.Append(' ');
            }

            parameter.Append(parameterDef.Type);
            parameter.Append(' ');
            parameter.Append(parameterDef.Name);

            // Default value
            if (!string.IsNullOrWhiteSpace(parameterDef.DefaultValue))
            {
                parameter.Append(" = ");
                parameter.Append(parameterDef.DefaultValue);
            }

            return parameter.ToString();
        }

        private string GenerateAttribute(AttributeDefinition attributeDef)
        {
            var attribute = new StringBuilder();
            attribute.Append('[');
            attribute.Append(attributeDef.Name);

            if (attributeDef.Arguments.Count > 0)
            {
                attribute.Append('(');
                var args = attributeDef.Arguments.Select(arg => 
                    arg is string str ? $"\"{str}\"" : arg?.ToString() ?? "null");
                attribute.Append(string.Join(", ", args));
                attribute.Append(')');
            }

            attribute.Append(']');
            return attribute.ToString();
        }

        private string GenerateGenericParameter(GenericParameterDefinition genericDef)
        {
            return genericDef.Name ?? "T";
        }

        private string GenerateAccessor(AccessorDefinition accessorDef)
        {
            var accessor = new StringBuilder();

            // Access modifier
            if (accessorDef.Access != null)
            {
                accessor.Append(accessorDef.Access.Keyword);
                accessor.Append(' ');
            }

            accessor.Append(accessorDef.Name);

            if (string.IsNullOrWhiteSpace(accessorDef.Body))
            {
                accessor.Append(';');
            }
            else
            {
                accessor.AppendLine();
                accessor.AppendLine("{");
                accessor.AppendLine(_formatter.Indent(accessorDef.Body));
                accessor.Append("}");
            }

            return accessor.ToString();
        }

        private string GenerateXmlDocumentation(string documentation)
        {
            var lines = documentation.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var xml = new StringBuilder();

            foreach (var line in lines)
            {
                xml.AppendLine($"/// {line.Trim()}");
            }

            return xml.ToString().TrimEnd();
        }

        private static List<string> GetModifierKeywords(Modifiers modifiers)
        {
            var keywords = new List<string>();

            if (modifiers.HasModifier(Modifiers.Static)) keywords.Add("static");
            if (modifiers.HasModifier(Modifiers.Abstract)) keywords.Add("abstract");
            if (modifiers.HasModifier(Modifiers.Virtual)) keywords.Add("virtual");
            if (modifiers.HasModifier(Modifiers.Override)) keywords.Add("override");
            if (modifiers.HasModifier(Modifiers.Sealed)) keywords.Add("sealed");
            if (modifiers.HasModifier(Modifiers.Partial)) keywords.Add("partial");
            if (modifiers.HasModifier(Modifiers.ReadOnly)) keywords.Add("readonly");
            if (modifiers.HasModifier(Modifiers.Const)) keywords.Add("const");
            if (modifiers.HasModifier(Modifiers.Extern)) keywords.Add("extern");
            if (modifiers.HasModifier(Modifiers.New)) keywords.Add("new");
            if (modifiers.HasModifier(Modifiers.Volatile)) keywords.Add("volatile");
            if (modifiers.HasModifier(Modifiers.Unsafe)) keywords.Add("unsafe");
            if (modifiers.HasModifier(Modifiers.Async)) keywords.Add("async");

            return keywords;
        }

        private static List<string> GetParameterModifierKeywords(Modifiers modifiers)
        {
            var keywords = new List<string>();

            if (modifiers.HasModifier(Modifiers.Ref)) keywords.Add("ref");
            if (modifiers.HasModifier(Modifiers.Out)) keywords.Add("out");
            if (modifiers.HasModifier(Modifiers.In)) keywords.Add("in");
            if (modifiers.HasModifier(Modifiers.Params)) keywords.Add("params");

            return keywords;
        }
    }
}