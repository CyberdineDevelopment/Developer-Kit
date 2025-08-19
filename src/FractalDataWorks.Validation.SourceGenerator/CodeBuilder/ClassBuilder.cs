using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FractalDataWorks.Validation.SourceGenerator.CodeBuilder;

/// <summary>
/// Builder for generating C# classes in the style of FractalDataWorks.CodeBuilder.
/// </summary>
internal sealed class ClassBuilder
{
    private readonly List<string> _usings = new();
    private string _namespace = string.Empty;
    private string _name = string.Empty;
    private string _accessModifier = "public";
    private bool _isStatic = false;
    private bool _isPartial = false;
    private readonly List<MethodBuilder> _methods = new();

    public ClassBuilder WithNamespace(string namespaceName)
    {
        _namespace = namespaceName;
        return this;
    }

    public ClassBuilder WithUsings(params string[] usings)
    {
        _usings.AddRange(usings);
        return this;
    }

    public ClassBuilder WithName(string className)
    {
        _name = className;
        return this;
    }

    public ClassBuilder WithAccessModifier(string accessModifier)
    {
        _accessModifier = accessModifier;
        return this;
    }

    public ClassBuilder AsStatic()
    {
        _isStatic = true;
        return this;
    }

    public ClassBuilder AsPartial()
    {
        _isPartial = true;
        return this;
    }

    public ClassBuilder WithMethod(MethodBuilder methodBuilder)
    {
        _methods.Add(methodBuilder);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        // Add usings
        foreach (var usingStatement in _usings.Distinct().OrderBy(u => u, StringComparer.Ordinal))
        {
            sb.AppendLine($"using {usingStatement};");
        }

        if (_usings.Count > 0)
        {
            sb.AppendLine();
        }

        // Add namespace
        sb.AppendLine($"namespace {_namespace};");
        sb.AppendLine();

        // Add class declaration
        var modifiers = new List<string> { _accessModifier };
        if (_isStatic) modifiers.Add("static");
        if (_isPartial) modifiers.Add("partial");

        sb.AppendLine($"{string.Join(" ", modifiers)} class {_name}");
        sb.AppendLine("{");

        // Add methods
        for (int i = 0; i < _methods.Count; i++)
        {
            var methodCode = _methods[i].Build();
            var indentedMethod = string.Join("\n", methodCode.Split('\n').Select(line => "    " + line));
            sb.AppendLine(indentedMethod);
            
            if (i < _methods.Count - 1)
            {
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}