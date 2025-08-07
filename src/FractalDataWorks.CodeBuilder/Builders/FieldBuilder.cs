using System;
using System.Collections.Generic;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// Builder for generating C# field definitions.
/// </summary>
public sealed class FieldBuilder : CodeBuilderBase, IFieldBuilder
{
    private string _name = "field";
    private string _type = "object";
    private string _accessModifier = "private";
    private bool _isStatic;
    private bool _isReadOnly;
    private bool _isConst;
    private bool _isVolatile;
    private string? _initializer;
    private readonly List<string> _attributes = new();
    private string? _xmlDocSummary;

    /// <inheritdoc/>
    public IFieldBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder WithAccessModifier(string accessModifier)
    {
        _accessModifier = accessModifier;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder AsStatic()
    {
        _isStatic = true;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder AsReadOnly()
    {
        _isReadOnly = true;
        _isConst = false;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder AsConst()
    {
        _isConst = true;
        _isReadOnly = false;
        _isVolatile = false;
        _isStatic = false; // const implies static
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder AsVolatile()
    {
        _isVolatile = true;
        _isConst = false;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder WithInitializer(string initializer)
    {
        _initializer = initializer;
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder WithAttribute(string attribute)
    {
        _attributes.Add(attribute);
        return this;
    }

    /// <inheritdoc/>
    public IFieldBuilder WithXmlDoc(string summary)
    {
        _xmlDocSummary = summary;
        return this;
    }

    /// <inheritdoc/>
    public override string Build()
    {
        Clear();

        // XML documentation
        if (!string.IsNullOrEmpty(_xmlDocSummary))
        {
            AppendLine("/// <summary>");
            foreach (var line in _xmlDocSummary.Split('\n'))
            {
                AppendLine($"/// {line.Trim()}");
            }
            AppendLine("/// </summary>");
        }

        // Attributes
        foreach (var attribute in _attributes)
        {
            AppendLine($"[{attribute}]");
        }

        // Field declaration
        var declaration = new StringBuilder();
        declaration.Append(_accessModifier);

        if (_isConst)
        {
            declaration.Append(" const");
        }
        else
        {
            if (_isStatic) declaration.Append(" static");
            if (_isVolatile) declaration.Append(" volatile");
            if (_isReadOnly) declaration.Append(" readonly");
        }

        declaration.Append($" {_type} {_name}");

        if (_initializer != null)
        {
            declaration.Append($" = {_initializer}");
        }
        else if (_isConst)
        {
            throw new InvalidOperationException("Const fields must have an initializer");
        }

        declaration.Append(";");
        AppendLine(declaration.ToString());

        return Builder.ToString().TrimEnd();
    }
}