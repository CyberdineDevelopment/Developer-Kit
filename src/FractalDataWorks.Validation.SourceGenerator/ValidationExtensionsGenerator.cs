using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Validation.SourceGenerator.CodeBuilder;

namespace FractalDataWorks.Validation.SourceGenerator;

/// <summary>
/// Source generator that creates validation extension methods for configuration classes.
/// Finds all classes that inherit from AbstractValidator&lt;T&gt; and generates corresponding extension methods.
/// </summary>
[Generator]
public sealed class ValidationExtensionsGenerator : ISourceGenerator
{
    /// <summary>
    /// Initializes the source generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for simple source generator
    }

    /// <summary>
    /// Executes the source generation.
    /// </summary>
    /// <param name="context">The generation context.</param>
    public void Execute(GeneratorExecutionContext context)
    {
        var validators = new List<ValidatorInfo>();

        // Find all validator classes across all syntax trees
        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
            var validatorClasses = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(IsValidatorClass);

            foreach (var validatorClass in validatorClasses)
            {
                var validatorInfo = GetValidatorInfo(validatorClass, semanticModel);
                if (validatorInfo != null)
                {
                    validators.Add(validatorInfo);
                }
            }
        }

        if (validators.Count == 0)
            return;

        GenerateExtensionMethods(context, validators);
    }

    private static void GenerateExtensionMethods(GeneratorExecutionContext context, List<ValidatorInfo> validators)
    {
        // Group validators by namespace
        var validatorsByNamespace = validators.GroupBy(v => v.Namespace, StringComparer.Ordinal);

        foreach (var namespaceGroup in validatorsByNamespace)
        {
            GenerateExtensionClass(context, namespaceGroup.Key, namespaceGroup.ToList());
        }
    }

    private static void GenerateExtensionClass(GeneratorExecutionContext context, string namespaceName, List<ValidatorInfo> validatorsInNamespace)
    {
        var classBuilder = new ClassBuilder()
            .WithNamespace(namespaceName)
            .WithUsings(
                "System",
                "System.Linq",
                "System.Threading",
                "System.Threading.Tasks",
                "FluentValidation",
                "FractalDataWorks.Results")
            .WithName("ValidationExtensions")
            .WithAccessModifier("public")
            .AsStatic()
            .AsPartial();

        foreach (var validator in validatorsInNamespace)
        {
            AddValidationMethods(classBuilder, validator);
        }

        var generatedCode = classBuilder.Build();
        var fileName = $"ValidationExtensions_{namespaceName.Replace(".", "_")}.g.cs";
        context.AddSource(fileName, generatedCode);
    }

    private static void AddValidationMethods(ClassBuilder classBuilder, ValidatorInfo validator)
    {
        // Generate synchronous validation method
        var syncMethod = new MethodBuilder()
            .WithName("Validate")
            .WithReturnType("IFdwResult")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter($"this {validator.ConfigurationTypeName}", "config")
            .WithBody($@"var validator = new {validator.ValidatorTypeName}();
var result = validator.Validate(config);

if (result.IsValid)
    return FdwResult.Success();

var errors = string.Join(""; "", result.Errors.Select(e => e.ErrorMessage));
return FdwResult.Failure($""Validation failed: {{errors}}"");");

        // Generate asynchronous validation method
        var asyncMethod = new MethodBuilder()
            .WithName("ValidateAsync")
            .WithReturnType("Task<IFdwResult>")
            .WithAccessModifier("public")
            .AsStatic()
            .AsAsync()
            .WithParameter($"this {validator.ConfigurationTypeName}", "config")
            .WithParameter("CancellationToken", "cancellationToken", "default")
            .WithBody($@"var validator = new {validator.ValidatorTypeName}();
var result = await validator.ValidateAsync(config, cancellationToken);

if (result.IsValid)
    return FdwResult.Success();

var errors = string.Join(""; "", result.Errors.Select(e => e.ErrorMessage));
return FdwResult.Failure($""Validation failed: {{errors}}"");");

        classBuilder
            .WithMethod(syncMethod)
            .WithMethod(asyncMethod);
    }

    private static bool IsValidatorClass(SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax { BaseList: not null } classDeclaration &&
               classDeclaration.BaseList.Types.Any(baseType =>
                   baseType.Type is GenericNameSyntax genericName &&
                   string.Equals(genericName.Identifier.ValueText, "AbstractValidator", StringComparison.Ordinal));
    }

    private static ValidatorInfo? GetValidatorInfo(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol is null)
            return null;

        // Find the AbstractValidator<T> base type
        var abstractValidatorBase = classSymbol.BaseType;
        while (abstractValidatorBase is not null)
        {
            if (string.Equals(abstractValidatorBase.Name, "AbstractValidator", StringComparison.Ordinal) &&
                abstractValidatorBase.IsGenericType &&
                abstractValidatorBase.TypeArguments.Length == 1)
            {
                var configType = abstractValidatorBase.TypeArguments[0];
                var namespaceName = classSymbol.ContainingNamespace?.ToDisplayString() ?? "";

                return new ValidatorInfo(
                    ValidatorTypeName: classSymbol.Name,
                    ConfigurationTypeName: configType.Name,
                    ConfigurationTypeFullName: configType.ToDisplayString(),
                    Namespace: namespaceName);
            }
            abstractValidatorBase = abstractValidatorBase.BaseType;
        }

        return null;
    }

    private sealed record ValidatorInfo(
        string ValidatorTypeName,
        string ConfigurationTypeName,
        string ConfigurationTypeFullName,
        string Namespace);
}