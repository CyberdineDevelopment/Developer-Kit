using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.CodeManagement.Abstractions
{
	/// <summary>
	/// Interface for code analysis services
	/// </summary>
	public interface ICodeAnalyzer : IGenericService
	{
		/// <summary>
		/// Gets the language this analyzer supports
		/// </summary>
		string Language { get; }

		/// <summary>
		/// Analyzes code and returns diagnostics
		/// </summary>
		Task<Result<CodeAnalysisResult>> Analyze(CodeAnalysisRequest request);

		/// <summary>
		/// Gets symbols from the code
		/// </summary>
		Task<Result<SymbolsResult>> GetSymbols(SymbolsRequest request);

		/// <summary>
		/// Gets references within the code
		/// </summary>
		Task<Result<ReferencesResult>> GetReferences(ReferencesRequest request);
	}

	/// <summary>
	/// Request for code analysis
	/// </summary>
	public class CodeAnalysisRequest
	{
		public string FilePath { get; set; } = string.Empty;
		public string? Content { get; set; }
		public List<string> AdditionalFiles { get; set; } = new();
		public Dictionary<string, object> Options { get; set; } = new();
		public AnalysisScope Scope { get; set; } = AnalysisScope.File;
	}

	/// <summary>
	/// Scope of analysis
	/// </summary>
	public enum AnalysisScope
	{
		File,
		Project,
		Solution
	}

	/// <summary>
	/// Result of code analysis
	/// </summary>
	public class CodeAnalysisResult
	{
		public List<Diagnostic> Diagnostics { get; set; } = new();
		public CodeMetrics? Metrics { get; set; }
		public Dictionary<string, object> Metadata { get; set; } = new();
	}

	/// <summary>
	/// Represents a diagnostic (error, warning, info)
	/// </summary>
	public class Diagnostic
	{
		public string Id { get; set; } = string.Empty;
		public DiagnosticSeverity Severity { get; set; }
		public string Message { get; set; } = string.Empty;
		public string FilePath { get; set; } = string.Empty;
		public TextSpan Location { get; set; }
		public string? Category { get; set; }
		public Dictionary<string, string> Properties { get; set; } = new();
	}

	/// <summary>
	/// Diagnostic severity levels
	/// </summary>
	public enum DiagnosticSeverity
	{
		Hidden,
		Info,
		Warning,
		Error
	}

	/// <summary>
	/// Represents a text span in code
	/// </summary>
	public class TextSpan
	{
		public int Start { get; set; }
		public int End { get; set; }
		public int StartLine { get; set; }
		public int StartColumn { get; set; }
		public int EndLine { get; set; }
		public int EndColumn { get; set; }
	}

	/// <summary>
	/// Code metrics
	/// </summary>
	public class CodeMetrics
	{
		public int LinesOfCode { get; set; }
		public int CyclomaticComplexity { get; set; }
		public int MaintainabilityIndex { get; set; }
		public int DepthOfInheritance { get; set; }
		public int CouplingBetweenObjects { get; set; }
		public Dictionary<string, object> CustomMetrics { get; set; } = new();
	}

	/// <summary>
	/// Request for symbols
	/// </summary>
	public class SymbolsRequest
	{
		public string FilePath { get; set; } = string.Empty;
		public string? Content { get; set; }
		public SymbolKind[]? IncludeKinds { get; set; }
		public bool IncludeExternal { get; set; }
	}

	/// <summary>
	/// Symbol kinds
	/// </summary>
	public enum SymbolKind
	{
		Class,
		Interface,
		Struct,
		Enum,
		Method,
		Property,
		Field,
		Event,
		Namespace,
		Parameter,
		Variable,
		TypeParameter
	}

	/// <summary>
	/// Result containing symbols
	/// </summary>
	public class SymbolsResult
	{
		public List<Symbol> Symbols { get; set; } = new();
	}

	/// <summary>
	/// Represents a code symbol
	/// </summary>
	public class Symbol
	{
		public string Name { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public SymbolKind Kind { get; set; }
		public TextSpan Location { get; set; }
		public string? ContainingType { get; set; }
		public List<string> Modifiers { get; set; } = new();
		public Dictionary<string, object> Properties { get; set; } = new();
	}

	/// <summary>
	/// Request for references
	/// </summary>
	public class ReferencesRequest
	{
		public string FilePath { get; set; } = string.Empty;
		public TextSpan Position { get; set; }
		public bool IncludeDeclaration { get; set; } = true;
		public List<string> SearchPaths { get; set; } = new();
	}

	/// <summary>
	/// Result containing references
	/// </summary>
	public class ReferencesResult
	{
		public List<Reference> References { get; set; } = new();
		public Symbol? Symbol { get; set; }
	}

	/// <summary>
	/// Represents a reference to a symbol
	/// </summary>
	public class Reference
	{
		public string FilePath { get; set; } = string.Empty;
		public TextSpan Location { get; set; }
		public string Text { get; set; } = string.Empty;
		public ReferenceKind Kind { get; set; }
	}

	/// <summary>
	/// Kind of reference
	/// </summary>
	public enum ReferenceKind
	{
		Declaration,
		Read,
		Write,
		Call,
		Instantiation
	}
}