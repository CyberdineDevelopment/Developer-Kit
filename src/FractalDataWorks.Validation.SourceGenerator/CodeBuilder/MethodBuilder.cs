using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FractalDataWorks.Validation.SourceGenerator.CodeBuilder;

/// <summary>
/// Builder for generating C# methods in the style of FractalDataWorks.CodeBuilder.
/// </summary>
internal sealed class MethodBuilder
{
    private string _name = string.Empty;
    private string _returnType = "void";
    private string _accessModifier = "public";
    private bool _isStatic = false;
    private bool _isAsync = false;
    private readonly List<(string Type, string Name, string? DefaultValue)> _parameters = new();
    private readonly List<string> _bodyLines = new();

    public MethodBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public MethodBuilder WithReturnType(string returnType)
    {
        _returnType = returnType;
        return this;
    }

    public MethodBuilder WithAccessModifier(string accessModifier)
    {
        _accessModifier = accessModifier;
        return this;
    }

    public MethodBuilder AsStatic()
    {
        _isStatic = true;
        return this;
    }

    public MethodBuilder AsAsync()
    {
        _isAsync = true;
        return this;
    }

    public MethodBuilder WithParameter(string type, string name, string? defaultValue = null)
    {
        _parameters.Add((type, name, defaultValue));
        return this;
    }

    public MethodBuilder WithBody(string body)
    {
        _bodyLines.Clear();
        _bodyLines.AddRange(body.Split('\n'));
        return this;
    }

    public MethodBuilder AddBodyLine(string line)
    {
        _bodyLines.Add(line);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        // Build method signature
        var modifiers = new List<string> { _accessModifier };
        if (_isStatic) modifiers.Add("static");
        if (_isAsync) modifiers.Add("async");

        var parameters = string.Join(", ", _parameters.Select(p => 
            p.DefaultValue != null ? $"{p.Type} {p.Name} = {p.DefaultValue}" : $"{p.Type} {p.Name}"));

        sb.AppendLine($"{string.Join(" ", modifiers)} {_returnType} {_name}({parameters})");
        sb.AppendLine("{");

        // Add method body
        foreach (var line in _bodyLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine($"    {line}");
            }
            else
            {
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}