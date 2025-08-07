using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// Builder for generating C# method definitions.
/// </summary>
public sealed class MethodBuilder : CodeBuilderBase, IMethodBuilder
{
    private string _name = "Method";
    private string _returnType = "void";
    private string _accessModifier = "public";
    private bool _isStatic;
    private bool _isVirtual;
    private bool _isOverride;
    private bool _isAbstract;
    private bool _isAsync;
    private readonly List<(string Type, string Name, string? Default)> _parameters = new();
    private readonly List<string> _genericParameters = new();
    private readonly Dictionary<string, List<string>> _genericConstraints = new();
    private readonly List<string> _attributes = new();
    private readonly List<string> _bodyLines = new();
    private string? _expressionBody;
    private string? _xmlDocSummary;
    private readonly Dictionary<string, string> _paramDocs = new();
    private string? _returnDoc;

    /// <inheritdoc/>
    public IMethodBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithReturnType(string returnType)
    {
        _returnType = returnType;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithAccessModifier(string accessModifier)
    {
        _accessModifier = accessModifier;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder AsStatic()
    {
        _isStatic = true;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder AsVirtual()
    {
        _isVirtual = true;
        _isOverride = false;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder AsOverride()
    {
        _isOverride = true;
        _isVirtual = false;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder AsAbstract()
    {
        _isAbstract = true;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder AsAsync()
    {
        _isAsync = true;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithParameter(string type, string name, string? defaultValue = null)
    {
        _parameters.Add((type, name, defaultValue));
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithGenericParameters(params string[] typeParameters)
    {
        _genericParameters.AddRange(typeParameters);
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithGenericConstraint(string typeParameter, params string[] constraints)
    {
        if (!_genericConstraints.ContainsKey(typeParameter))
            _genericConstraints[typeParameter] = new List<string>();
        _genericConstraints[typeParameter].AddRange(constraints);
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithAttribute(string attribute)
    {
        _attributes.Add(attribute);
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithBody(string body)
    {
        _bodyLines.Clear();
        _bodyLines.AddRange(body.Split('\n'));
        _expressionBody = null;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder AddBodyLine(string line)
    {
        _bodyLines.Add(line);
        _expressionBody = null;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithExpressionBody(string expression)
    {
        _expressionBody = expression;
        _bodyLines.Clear();
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithXmlDoc(string summary)
    {
        _xmlDocSummary = summary;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithParamDoc(string parameterName, string description)
    {
        _paramDocs[parameterName] = description;
        return this;
    }

    /// <inheritdoc/>
    public IMethodBuilder WithReturnDoc(string description)
    {
        _returnDoc = description;
        return this;
    }

    /// <inheritdoc/>
    public override string Build()
    {
        Clear();

        // XML documentation
        if (!string.IsNullOrEmpty(_xmlDocSummary) || _paramDocs.Count > 0 || !string.IsNullOrEmpty(_returnDoc))
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

            if (!string.IsNullOrEmpty(_returnDoc))
            {
                AppendLine($"/// <returns>{_returnDoc}</returns>");
            }
        }

        // Attributes
        foreach (var attribute in _attributes)
        {
            AppendLine($"[{attribute}]");
        }

        // Method signature
        var signature = new StringBuilder();
        signature.Append(_accessModifier);

        if (_isStatic) signature.Append(" static");
        if (_isAbstract) signature.Append(" abstract");
        if (_isVirtual) signature.Append(" virtual");
        if (_isOverride) signature.Append(" override");
        if (_isAsync) signature.Append(" async");

        signature.Append($" {_returnType} {_name}");

        if (_genericParameters.Count > 0)
        {
            signature.Append($"<{string.Join(", ", _genericParameters)}>");
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

        if (_expressionBody != null)
        {
            AppendLine($"{signature} => {_expressionBody};");
        }
        else if (_isAbstract)
        {
            AppendLine($"{signature};");
        }
        else
        {
            AppendLine(signature.ToString());

            // Generic constraints
            foreach (var constraint in _genericConstraints)
            {
                Indent();
                AppendLine($"where {constraint.Key} : {string.Join(", ", constraint.Value)}");
                Outdent();
            }

            AppendLine("{");
            Indent();

            foreach (var line in _bodyLines)
            {
                AppendLine(line.TrimEnd());
            }

            Outdent();
            AppendLine("}");
        }

        return Builder.ToString().TrimEnd();
    }
}