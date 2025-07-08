using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Services;
using FractalDataWorks.Services.CodeManagement.Abstractions;
using FractalDataWorks.Services.CodeManagement.Configuration;
using FractalDataWorks.Services.CodeManagement.EnhancedEnums;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.CodeManagement.Services
{
	/// <summary>
	/// Main code management service that orchestrates analyzers, refactorers, and generators
	/// </summary>
	public class CodeManagementService : ServiceBase<CodeManagementConfiguration>, ICodeManagementService
	{
		private readonly ILogger<CodeManagementService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<string, ICodeAnalyzer> _analyzers;
		private readonly Dictionary<string, ICodeRefactorer> _refactorers;
		private readonly Dictionary<string, ICodeGenerator> _generators;

		public CodeManagementService(
			CodeManagementConfiguration configuration,
			IServiceProvider serviceProvider,
			ILogger<CodeManagementService> logger)
			: base(configuration)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;

			// Initialize language-specific services from Enhanced Enums
			_analyzers = CodeAnalyzerTypeEnum.GetAll()
				.ToDictionary(e => e.Language, e => e.CreateAnalyzer(_serviceProvider));

			_refactorers = CodeRefactorerTypeEnum.GetAll()
				.ToDictionary(e => e.Language, e => e.CreateRefactorer(_serviceProvider));

			_generators = CodeGeneratorTypeEnum.GetAll()
				.ToDictionary(e => e.Language, e => e.CreateGenerator(_serviceProvider));
		}

		public async Task<Result<CodeAnalysisResult>> Analyze(CodeAnalysisRequest request)
		{
			try
			{
				var language = DetectLanguage(request.FilePath);
				if (!_analyzers.TryGetValue(language, out var analyzer))
				{
					return Result<CodeAnalysisResult>.Failure(
						ServiceMessage.Error($"No analyzer available for language: {language}"));
				}

				_logger.LogInformation("Analyzing {FilePath} with {Language} analyzer", 
					request.FilePath, language);

				return await analyzer.Analyze(request);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Code analysis failed for {FilePath}", request.FilePath);
				return Result<CodeAnalysisResult>.Failure(
					ServiceMessage.Error($"Analysis failed: {ex.Message}"));
			}
		}

		public async Task<Result<RefactoringResult>> Refactor(RefactoringRequest request)
		{
			try
			{
				var language = DetectLanguage(request.FilePath);
				if (!_refactorers.TryGetValue(language, out var refactorer))
				{
					return Result<RefactoringResult>.Failure(
						ServiceMessage.Error($"No refactorer available for language: {language}"));
				}

				_logger.LogInformation("Applying refactoring {RefactoringId} to {FilePath}", 
					request.RefactoringId, request.FilePath);

				return await refactorer.ApplyRefactoring(request);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Refactoring failed for {FilePath}", request.FilePath);
				return Result<RefactoringResult>.Failure(
					ServiceMessage.Error($"Refactoring failed: {ex.Message}"));
			}
		}

		public async Task<Result<GenerationResult>> Generate(GenerationRequest request)
		{
			try
			{
				var language = request.Parameters.TryGetValue("Language", out var langObj) 
					? langObj.ToString() ?? Configuration.DefaultLanguage
					: Configuration.DefaultLanguage;

				if (!_generators.TryGetValue(language, out var generator))
				{
					return Result<GenerationResult>.Failure(
						ServiceMessage.Error($"No generator available for language: {language}"));
				}

				_logger.LogInformation("Generating {Type} code with {Language} generator", 
					request.Type, language);

				return await generator.Generate(request);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Code generation failed for {Type}", request.Type);
				return Result<GenerationResult>.Failure(
					ServiceMessage.Error($"Generation failed: {ex.Message}"));
			}
		}

		public async Task<Result<MultiLanguageAnalysisResult>> AnalyzeMultiple(
			MultiLanguageAnalysisRequest request)
		{
			var results = new List<LanguageAnalysisResult>();
			var hasErrors = false;

			foreach (var file in request.Files)
			{
				var analysisRequest = new CodeAnalysisRequest
				{
					FilePath = file,
					Options = request.Options,
					Scope = request.Scope
				};

				var result = await Analyze(analysisRequest);
				
				if (result is Result<CodeAnalysisResult>.Success success)
				{
					results.Add(new LanguageAnalysisResult
					{
						FilePath = file,
						Language = DetectLanguage(file),
						Result = success.Value
					});
				}
				else if (result is Result<CodeAnalysisResult>.Failure failure)
				{
					results.Add(new LanguageAnalysisResult
					{
						FilePath = file,
						Language = DetectLanguage(file),
						Error = failure.Error.Message
					});
					hasErrors = true;
				}
			}

			var multiResult = new MultiLanguageAnalysisResult
			{
				Results = results,
				TotalFiles = request.Files.Count,
				SuccessfulAnalyses = results.Count(r => r.Error == null),
				FailedAnalyses = results.Count(r => r.Error != null)
			};

			return hasErrors && !request.ContinueOnError
				? Result<MultiLanguageAnalysisResult>.Failure(
					ServiceMessage.Error("One or more analyses failed"))
				: Result<MultiLanguageAnalysisResult>.Success(
					multiResult, $"Analyzed {multiResult.TotalFiles} files");
		}

		public IEnumerable<string> GetSupportedLanguages()
		{
			return _analyzers.Keys.Union(_refactorers.Keys).Union(_generators.Keys).Distinct();
		}

		public IEnumerable<string> GetSupportedExtensions()
		{
			return CodeAnalyzerTypeEnum.GetAll()
				.SelectMany(a => a.SupportedExtensions)
				.Distinct();
		}

		private string DetectLanguage(string filePath)
		{
			var extension = Path.GetExtension(filePath)?.ToLowerInvariant() ?? string.Empty;

			// Find analyzer that supports this extension
			var analyzer = CodeAnalyzerTypeEnum.GetAll()
				.FirstOrDefault(a => a.SupportedExtensions.Contains(extension));

			return analyzer?.Language ?? Configuration.DefaultLanguage;
		}
	}

	/// <summary>
	/// Interface for the code management service
	/// </summary>
	public interface ICodeManagementService : IGenericService
	{
		/// <summary>
		/// Analyzes code and returns diagnostics
		/// </summary>
		Task<Result<CodeAnalysisResult>> Analyze(CodeAnalysisRequest request);

		/// <summary>
		/// Applies a refactoring to code
		/// </summary>
		Task<Result<RefactoringResult>> Refactor(RefactoringRequest request);

		/// <summary>
		/// Generates code based on templates
		/// </summary>
		Task<Result<GenerationResult>> Generate(GenerationRequest request);

		/// <summary>
		/// Analyzes multiple files potentially in different languages
		/// </summary>
		Task<Result<MultiLanguageAnalysisResult>> AnalyzeMultiple(MultiLanguageAnalysisRequest request);

		/// <summary>
		/// Gets all supported programming languages
		/// </summary>
		IEnumerable<string> GetSupportedLanguages();

		/// <summary>
		/// Gets all supported file extensions
		/// </summary>
		IEnumerable<string> GetSupportedExtensions();
	}

	/// <summary>
	/// Request for multi-language analysis
	/// </summary>
	public class MultiLanguageAnalysisRequest
	{
		public List<string> Files { get; set; } = new();
		public Dictionary<string, object> Options { get; set; } = new();
		public AnalysisScope Scope { get; set; } = AnalysisScope.File;
		public bool ContinueOnError { get; set; } = true;
	}

	/// <summary>
	/// Result of multi-language analysis
	/// </summary>
	public class MultiLanguageAnalysisResult
	{
		public List<LanguageAnalysisResult> Results { get; set; } = new();
		public int TotalFiles { get; set; }
		public int SuccessfulAnalyses { get; set; }
		public int FailedAnalyses { get; set; }
	}

	/// <summary>
	/// Analysis result for a single language/file
	/// </summary>
	public class LanguageAnalysisResult
	{
		public string FilePath { get; set; } = string.Empty;
		public string Language { get; set; } = string.Empty;
		public CodeAnalysisResult? Result { get; set; }
		public string? Error { get; set; }
	}
}