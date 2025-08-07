using System;
using System.Collections.Generic;
using System.Text;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// Builder for generating C# property definitions.
/// </summary>
public sealed class PropertyBuilder : CodeBuilderBase, IPropertyBuilder
{
    private string _name = "Property";
    private string _type = "object";
    private string _accessModifier = "public";
    private bool _isStatic;
    private bool _isVirtual;
    private bool _isOverride;
    private bool _isAbstract;
    private bool _isReadOnly;
    private bool _isWriteOnly;
    private bool _hasInitSetter;
    private string? _getterBody;
    private string? _setterBody;
    private string? _getterAccessModifier;
    private string? _setterAccessModifier;
    private string? _initializer;
    private string? _expressionBody;
    private readonly List<string> _attributes = new();
    private string? _xmlDocSummary;

    /// <inheritdoc/>
    public IPropertyBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithAccessModifier(string accessModifier)
    {
        _accessModifier = accessModifier;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder AsStatic()
    {
        _isStatic = true;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder AsVirtual()
    {
        _isVirtual = true;
        _isOverride = false;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder AsOverride()
    {
        _isOverride = true;
        _isVirtual = false;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder AsAbstract()
    {
        _isAbstract = true;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder AsReadOnly()
    {
        _isReadOnly = true;
        _isWriteOnly = false;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder AsWriteOnly()
    {
        _isWriteOnly = true;
        _isReadOnly = false;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithGetter(string getterBody)
    {
        _getterBody = getterBody;
        _isWriteOnly = false;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithSetter(string setterBody)
    {
        _setterBody = setterBody;
        _isReadOnly = false;
        _hasInitSetter = false;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithGetterAccessModifier(string accessModifier)
    {
        _getterAccessModifier = accessModifier;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithSetterAccessModifier(string accessModifier)
    {
        _setterAccessModifier = accessModifier;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithInitializer(string initializer)
    {
        _initializer = initializer;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithInitSetter()
    {
        _hasInitSetter = true;
        _isReadOnly = false;
        _setterBody = null;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithAttribute(string attribute)
    {
        _attributes.Add(attribute);
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithExpressionBody(string expression)
    {
        _expressionBody = expression;
        return this;
    }

    /// <inheritdoc/>
    public IPropertyBuilder WithXmlDoc(string summary)
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

        // Property signature
        var signature = new StringBuilder();
        signature.Append(_accessModifier);

        if (_isStatic) signature.Append(" static");
        if (_isAbstract) signature.Append(" abstract");
        if (_isVirtual) signature.Append(" virtual");
        if (_isOverride) signature.Append(" override");

        signature.Append($" {_type} {_name}");

        if (_expressionBody != null)
        {
            AppendLine($"{signature} => {_expressionBody};");
        }
        else
        {
            Append(signature.ToString());

            // Property accessors
            if (_getterBody != null || _setterBody != null)
            {
                AppendLine("");
                AppendLine("{");
                Indent();

                if (!_isWriteOnly)
                {
                    if (_getterAccessModifier != null)
                        Append($"{_getterAccessModifier} ");
                    
                    if (_getterBody != null)
                    {
                        AppendLine("get");
                        AppendLine("{");
                        Indent();
                        foreach (var line in _getterBody.Split('\n'))
                        {
                            AppendLine(line.TrimEnd());
                        }
                        Outdent();
                        AppendLine("}");
                    }
                    else
                    {
                        AppendLine("get;");
                    }
                }

                if (!_isReadOnly)
                {
                    if (_setterAccessModifier != null)
                        Append($"{_setterAccessModifier} ");

                    if (_hasInitSetter)
                    {
                        AppendLine("init;");
                    }
                    else if (_setterBody != null)
                    {
                        AppendLine("set");
                        AppendLine("{");
                        Indent();
                        foreach (var line in _setterBody.Split('\n'))
                        {
                            AppendLine(line.TrimEnd());
                        }
                        Outdent();
                        AppendLine("}");
                    }
                    else
                    {
                        AppendLine("set;");
                    }
                }

                Outdent();
                Append("}");
            }
            else
            {
                Append(" { ");
                
                if (!_isWriteOnly)
                {
                    if (_getterAccessModifier != null)
                        Append($"{_getterAccessModifier} ");
                    Append("get; ");
                }

                if (!_isReadOnly)
                {
                    if (_setterAccessModifier != null)
                        Append($"{_setterAccessModifier} ");
                    
                    if (_hasInitSetter)
                        Append("init; ");
                    else
                        Append("set; ");
                }

                Append("}");
            }

            if (_initializer != null)
            {
                Append($" = {_initializer};");
            }
            else if (!_isAbstract)
            {
                AppendLine("");
            }
        }

        return Builder.ToString().TrimEnd();
    }
}