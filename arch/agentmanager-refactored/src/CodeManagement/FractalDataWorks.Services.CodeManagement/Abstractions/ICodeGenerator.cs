using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.CodeManagement.Abstractions
{
	/// <summary>
	/// Interface for code generation services
	/// </summary>
	public interface ICodeGenerator : IGenericService
	{
		/// <summary>
		/// Gets the language this generator supports
		/// </summary>
		string Language { get; }

		/// <summary>
		/// Generates code based on a template or specification
		/// </summary>
		Task<Result<GenerationResult>> Generate(GenerationRequest request);

		/// <summary>
		/// Gets available templates for generation
		/// </summary>
		Task<Result<TemplatesResult>> GetTemplates(TemplatesRequest request);

		/// <summary>
		/// Validates a generation request before executing
		/// </summary>
		Task<Result<ValidationResult>> ValidateRequest(GenerationRequest request);
	}

	/// <summary>
	/// Request for code generation
	/// </summary>
	public class GenerationRequest
	{
		public GenerationType Type { get; set; }
		public string? TemplateName { get; set; }
		public string? TargetPath { get; set; }
		public string? Namespace { get; set; }
		public Dictionary<string, object> Parameters { get; set; } = new();
		public GenerationOptions Options { get; set; } = new();
		public List<string> References { get; set; } = new();
	}

	/// <summary>
	/// Types of code generation
	/// </summary>
	public enum GenerationType
	{
		Class,
		Interface,
		Enum,
		Record,
		Struct,
		Method,
		Property,
		Test,
		Service,
		Controller,
		Repository,
		Configuration,
		Custom
	}

	/// <summary>
	/// Options for code generation
	/// </summary>
	public class GenerationOptions
	{
		public bool GenerateDocumentation { get; set; } = true;
		public bool GenerateTests { get; set; } = false;
		public bool OverwriteExisting { get; set; } = false;
		public string? BaseClass { get; set; }
		public List<string> Interfaces { get; set; } = new();
		public List<string> Attributes { get; set; } = new();
		public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
		public bool IsPartial { get; set; } = false;
		public bool IsSealed { get; set; } = false;
		public bool IsAbstract { get; set; } = false;
		public Dictionary<string, string> CustomOptions { get; set; } = new();
	}

	/// <summary>
	/// Access modifiers
	/// </summary>
	public enum AccessModifier
	{
		Public,
		Private,
		Protected,
		Internal,
		ProtectedInternal,
		PrivateProtected
	}

	/// <summary>
	/// Result of code generation
	/// </summary>
	public class GenerationResult
	{
		public List<GeneratedFile> Files { get; set; } = new();
		public bool Success { get; set; }
		public string? Message { get; set; }
		public List<string> Warnings { get; set; } = new();
		public Dictionary<string, object> Metadata { get; set; } = new();
	}

	/// <summary>
	/// Represents a generated file
	/// </summary>
	public class GeneratedFile
	{
		public string FilePath { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public GeneratedFileKind Kind { get; set; }
		public Dictionary<string, string> Properties { get; set; } = new();
	}

	/// <summary>
	/// Kind of generated file
	/// </summary>
	public enum GeneratedFileKind
	{
		Source,
		Test,
		Configuration,
		Documentation,
		Resource,
		Other
	}

	/// <summary>
	/// Request for templates
	/// </summary>
	public class TemplatesRequest
	{
		public GenerationType? Type { get; set; }
		public string? Category { get; set; }
		public bool IncludeBuiltIn { get; set; } = true;
		public bool IncludeCustom { get; set; } = true;
	}

	/// <summary>
	/// Result containing templates
	/// </summary>
	public class TemplatesResult
	{
		public List<Template> Templates { get; set; } = new();
	}

	/// <summary>
	/// Represents a code generation template
	/// </summary>
	public class Template
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public GenerationType Type { get; set; }
		public string? Category { get; set; }
		public List<TemplateParameter> Parameters { get; set; } = new();
		public Dictionary<string, object> Metadata { get; set; } = new();
		public bool IsBuiltIn { get; set; }
	}

	/// <summary>
	/// Parameter for a template
	/// </summary>
	public class TemplateParameter
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public bool IsRequired { get; set; }
		public object? DefaultValue { get; set; }
		public List<string>? AllowedValues { get; set; }
		public string? ValidationPattern { get; set; }
	}

	/// <summary>
	/// Validation result
	/// </summary>
	public class ValidationResult
	{
		public bool IsValid { get; set; }
		public List<ValidationError> Errors { get; set; } = new();
		public List<string> Warnings { get; set; } = new();
	}

	/// <summary>
	/// Validation error
	/// </summary>
	public class ValidationError
	{
		public string Field { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public ValidationErrorKind Kind { get; set; }
	}

	/// <summary>
	/// Kind of validation error
	/// </summary>
	public enum ValidationErrorKind
	{
		MissingRequired,
		InvalidValue,
		InvalidType,
		PatternMismatch,
		ReferenceNotFound,
		Conflict,
		Other
	}
}