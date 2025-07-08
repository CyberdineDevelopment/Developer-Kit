using System.Collections.Generic;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.CodeManagement.Configuration
{
	/// <summary>
	/// Configuration for the code management service
	/// </summary>
	public class CodeManagementConfiguration : ConfigurationBase
	{
		/// <summary>
		/// Gets or sets the default language for operations
		/// </summary>
		public string DefaultLanguage { get; set; } = "CSharp";

		/// <summary>
		/// Gets or sets whether to enable caching of analysis results
		/// </summary>
		public bool EnableCaching { get; set; } = true;

		/// <summary>
		/// Gets or sets the cache duration in minutes
		/// </summary>
		public int CacheDurationMinutes { get; set; } = 30;

		/// <summary>
		/// Gets or sets the maximum file size to analyze (in bytes)
		/// </summary>
		public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

		/// <summary>
		/// Gets or sets whether to enable parallel processing
		/// </summary>
		public bool EnableParallelProcessing { get; set; } = true;

		/// <summary>
		/// Gets or sets the maximum degree of parallelism
		/// </summary>
		public int MaxDegreeOfParallelism { get; set; } = 4;

		/// <summary>
		/// Gets or sets language-specific configurations
		/// </summary>
		public Dictionary<string, LanguageConfiguration> LanguageConfigurations { get; set; } = new();

		/// <summary>
		/// Gets or sets global analysis options
		/// </summary>
		public AnalysisOptions AnalysisOptions { get; set; } = new();

		/// <summary>
		/// Gets or sets global refactoring options
		/// </summary>
		public RefactoringOptions RefactoringOptions { get; set; } = new();

		/// <summary>
		/// Gets or sets global generation options
		/// </summary>
		public GenerationGlobalOptions GenerationOptions { get; set; } = new();

		public override ValidationResult Validate()
		{
			var errors = new List<string>();

			if (string.IsNullOrWhiteSpace(DefaultLanguage))
				errors.Add("DefaultLanguage cannot be empty");

			if (CacheDurationMinutes < 0)
				errors.Add("CacheDurationMinutes must be non-negative");

			if (MaxFileSize <= 0)
				errors.Add("MaxFileSize must be greater than 0");

			if (MaxDegreeOfParallelism <= 0)
				errors.Add("MaxDegreeOfParallelism must be greater than 0");

			// Validate nested configurations
			var analysisValidation = AnalysisOptions.Validate();
			if (!analysisValidation.IsValid)
				errors.AddRange(analysisValidation.Errors);

			var refactoringValidation = RefactoringOptions.Validate();
			if (!refactoringValidation.IsValid)
				errors.AddRange(refactoringValidation.Errors);

			var generationValidation = GenerationOptions.Validate();
			if (!generationValidation.IsValid)
				errors.AddRange(generationValidation.Errors);

			foreach (var (language, config) in LanguageConfigurations)
			{
				var langValidation = config.Validate();
				if (!langValidation.IsValid)
				{
					errors.AddRange(langValidation.Errors.Select(e => $"{language}: {e}"));
				}
			}

			return errors.Count == 0 
				? ValidationResult.Success() 
				: ValidationResult.Failure(errors.ToArray());
		}
	}

	/// <summary>
	/// Language-specific configuration
	/// </summary>
	public class LanguageConfiguration : ConfigurationBase
	{
		/// <summary>
		/// Gets or sets whether this language is enabled
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Gets or sets file extensions for this language
		/// </summary>
		public List<string> FileExtensions { get; set; } = new();

		/// <summary>
		/// Gets or sets custom options for this language
		/// </summary>
		public Dictionary<string, object> CustomOptions { get; set; } = new();

		public override ValidationResult Validate()
		{
			var errors = new List<string>();

			if (Enabled && FileExtensions.Count == 0)
				errors.Add("Enabled language must have at least one file extension");

			return errors.Count == 0 
				? ValidationResult.Success() 
				: ValidationResult.Failure(errors.ToArray());
		}
	}

	/// <summary>
	/// Global analysis options
	/// </summary>
	public class AnalysisOptions : ConfigurationBase
	{
		/// <summary>
		/// Gets or sets whether to include compiler diagnostics
		/// </summary>
		public bool IncludeCompilerDiagnostics { get; set; } = true;

		/// <summary>
		/// Gets or sets whether to include analyzer diagnostics
		/// </summary>
		public bool IncludeAnalyzerDiagnostics { get; set; } = true;

		/// <summary>
		/// Gets or sets the minimum diagnostic severity to report
		/// </summary>
		public DiagnosticSeverity MinimumSeverity { get; set; } = DiagnosticSeverity.Info;

		/// <summary>
		/// Gets or sets diagnostic IDs to suppress
		/// </summary>
		public List<string> SuppressedDiagnosticIds { get; set; } = new();

		/// <summary>
		/// Gets or sets whether to compute code metrics
		/// </summary>
		public bool ComputeMetrics { get; set; } = true;

		public override ValidationResult Validate()
		{
			return ValidationResult.Success();
		}
	}

	/// <summary>
	/// Global refactoring options
	/// </summary>
	public class RefactoringOptions : ConfigurationBase
	{
		/// <summary>
		/// Gets or sets whether to preview changes before applying
		/// </summary>
		public bool AlwaysPreview { get; set; } = false;

		/// <summary>
		/// Gets or sets whether to create backups before refactoring
		/// </summary>
		public bool CreateBackups { get; set; } = true;

		/// <summary>
		/// Gets or sets the backup directory path
		/// </summary>
		public string? BackupDirectory { get; set; }

		/// <summary>
		/// Gets or sets whether to format code after refactoring
		/// </summary>
		public bool FormatAfterRefactoring { get; set; } = true;

		public override ValidationResult Validate()
		{
			var errors = new List<string>();

			if (CreateBackups && string.IsNullOrWhiteSpace(BackupDirectory))
				errors.Add("BackupDirectory must be specified when CreateBackups is true");

			return errors.Count == 0 
				? ValidationResult.Success() 
				: ValidationResult.Failure(errors.ToArray());
		}
	}

	/// <summary>
	/// Global generation options
	/// </summary>
	public class GenerationGlobalOptions : ConfigurationBase
	{
		/// <summary>
		/// Gets or sets the default template directory
		/// </summary>
		public string? TemplateDirectory { get; set; }

		/// <summary>
		/// Gets or sets whether to use built-in templates
		/// </summary>
		public bool UseBuiltInTemplates { get; set; } = true;

		/// <summary>
		/// Gets or sets whether to generate XML documentation
		/// </summary>
		public bool GenerateXmlDocumentation { get; set; } = true;

		/// <summary>
		/// Gets or sets the default author name for generated code
		/// </summary>
		public string? DefaultAuthor { get; set; }

		/// <summary>
		/// Gets or sets the default company name for generated code
		/// </summary>
		public string? DefaultCompany { get; set; }

		/// <summary>
		/// Gets or sets custom generation variables
		/// </summary>
		public Dictionary<string, string> CustomVariables { get; set; } = new();

		public override ValidationResult Validate()
		{
			return ValidationResult.Success();
		}
	}
}