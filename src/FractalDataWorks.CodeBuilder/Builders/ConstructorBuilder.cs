using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// Builder for generating C# constructor definitions.
/// </summary>
public sealed class ConstructorBuilder : CodeBuilderBase, IConstructorBuilder
{
    private string _className = "MyClass";
    private string _accessModifier = "public";
    private bool _isStatic;
    private readonly List<(string Type, string Name, string? Default)> _parameters = new();
    private readonly List<string> _baseCallArguments = new();
    private readonly List<string> _thisCallArguments = new();
    private readonly List<string> _attributes = new();
    private readonly List<string> _bodyLines = new();
    private string? _xmlDocSummary;
    private readonly Dictionary<string, string> _paramDocs = new();

    /// <summary>
    /// Sets the class name for the constructor.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <returns>The builder for method chaining.</returns>
    public ConstructorBuilder WithClassName(string className)
    {
        _className = className;
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithAccessModifier(string accessModifier)
    {
        _accessModifier = accessModifier;
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder AsStatic()
    {
        _isStatic = true;
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithParameter(string type, string name, string? defaultValue = null)
    {
        _parameters.Add((type, name, defaultValue));
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithBaseCall(params string[] arguments)
    {
        _baseCallArguments.Clear();
        _baseCallArguments.AddRange(arguments);
        _thisCallArguments.Clear();
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithThisCall(params string[] arguments)
    {
        _thisCallArguments.Clear();
        _thisCallArguments.AddRange(arguments);
        _baseCallArguments.Clear();
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithAttribute(string attribute)
    {
        _attributes.Add(attribute);
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithBody(string body)
    {
        _bodyLines.Clear();
        _bodyLines.AddRange(body.Split('\n'));
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder AddBodyLine(string line)
    {
        _bodyLines.Add(line);
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithXmlDoc(string summary)
    {
        _xmlDocSummary = summary;
        return this;
    }

    /// <inheritdoc/>
    public IConstructorBuilder WithParamDoc(string parameterName, string description)
    {
        _paramDocs[parameterName] = description;
        return this;
    }

    /// <inheritdoc/>
    public override string Build()
    {
        Clear();

        // XML documentation
        if (!string.IsNullOrEmpty(_xmlDocSummary) || _paramDocs.Count > 0)
        {
            if (!string.IsNullOrEmpty(_xmlDocSummary))
            {
                AppendLine("/// <summary>");
                foreach (var line in _xmlDocSummary.Split('\n'))
                {
                    AppendLine($"/// {line.Trim()}");
                }
                AppendLine("/// </summary>");
            }

            foreach (var param in _parameters)
            {
                if (_paramDocs.ContainsKey(param.Name))
                {
                    AppendLine($"/// <param name=\"{param.Name}\">{_paramDocs[param.Name]}</param>");
                }
            }
        }

        // Attributes
        foreach (var attribute in _attributes)
        {
            AppendLine($"[{attribute}]");
        }

        // Constructor signature
        var signature = new StringBuilder();
        
        if (_isStatic)
        {
            signature.Append("static ");
            signature.Append(_className);
        }
        else
        {
            signature.Append(_accessModifier);
            signature.Append($" {_className}");
        }

        signature.Append("(");

        var paramStrings = _parameters.Select(p =>
        {
            var paramStr = $"{p.Type} {p.Name}";
            if (p.Default != null)
                paramStr += $" = {p.Default}";
            return paramStr;
        });

        signature.Append(string.Join(", ", paramStrings));
        signature.Append(")");

        // Base or this call
        if (_baseCallArguments.Count > 0)
        {
            signature.Append($" : base({string.Join(", ", _baseCallArguments)})");
        }
        else if (_thisCallArguments.Count > 0)
        {
            signature.Append($" : this({string.Join(", ", _thisCallArguments)})");
        }

        AppendLine(signature.ToString());
        AppendLine("{");
        Indent();

        foreach (var line in _bodyLines)
        {
            AppendLine(line.TrimEnd());
        }

        Outdent();
        AppendLine("}");

        return Builder.ToString().TrimEnd();
    }
}