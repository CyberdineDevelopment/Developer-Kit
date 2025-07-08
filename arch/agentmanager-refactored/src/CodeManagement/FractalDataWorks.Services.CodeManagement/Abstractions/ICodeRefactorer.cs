using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.CodeManagement.Abstractions
{
	/// <summary>
	/// Interface for code refactoring services
	/// </summary>
	public interface ICodeRefactorer : IGenericService
	{
		/// <summary>
		/// Gets the language this refactorer supports
		/// </summary>
		string Language { get; }

		/// <summary>
		/// Gets available refactorings for a given context
		/// </summary>
		Task<Result<AvailableRefactoringsResult>> GetAvailableRefactorings(RefactoringContext context);

		/// <summary>
		/// Applies a specific refactoring
		/// </summary>
		Task<Result<RefactoringResult>> ApplyRefactoring(RefactoringRequest request);

		/// <summary>
		/// Previews changes for a refactoring without applying them
		/// </summary>
		Task<Result<RefactoringPreview>> PreviewRefactoring(RefactoringRequest request);
	}

	/// <summary>
	/// Context for determining available refactorings
	/// </summary>
	public class RefactoringContext
	{
		public string FilePath { get; set; } = string.Empty;
		public string? Content { get; set; }
		public TextSpan Selection { get; set; }
		public List<string> AdditionalFiles { get; set; } = new();
	}

	/// <summary>
	/// Result containing available refactorings
	/// </summary>
	public class AvailableRefactoringsResult
	{
		public List<RefactoringInfo> Refactorings { get; set; } = new();
	}

	/// <summary>
	/// Information about an available refactoring
	/// </summary>
	public class RefactoringInfo
	{
		public string Id { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public RefactoringKind Kind { get; set; }
		public TextSpan ApplicableRange { get; set; }
		public Dictionary<string, object> Properties { get; set; } = new();
	}

	/// <summary>
	/// Kinds of refactorings
	/// </summary>
	public enum RefactoringKind
	{
		Rename,
		ExtractMethod,
		ExtractInterface,
		ExtractClass,
		InlineMethod,
		InlineVariable,
		MoveType,
		ChangeSignature,
		EncapsulateField,
		IntroduceParameter,
		IntroduceVariable,
		ConvertToProperty,
		ConvertToAutoProperty,
		GenerateConstructor,
		GenerateEquals,
		OrganizeUsings,
		Custom
	}

	/// <summary>
	/// Request to apply a refactoring
	/// </summary>
	public class RefactoringRequest
	{
		public string RefactoringId { get; set; } = string.Empty;
		public string FilePath { get; set; } = string.Empty;
		public string? Content { get; set; }
		public TextSpan Selection { get; set; }
		public Dictionary<string, object> Parameters { get; set; } = new();
		public List<string> AdditionalFiles { get; set; } = new();
	}

	/// <summary>
	/// Result of applying a refactoring
	/// </summary>
	public class RefactoringResult
	{
		public List<FileChange> Changes { get; set; } = new();
		public string? Message { get; set; }
		public bool Success { get; set; }
		public List<string> Warnings { get; set; } = new();
	}

	/// <summary>
	/// Preview of refactoring changes
	/// </summary>
	public class RefactoringPreview
	{
		public List<FileChangePreview> Changes { get; set; } = new();
		public string Summary { get; set; } = string.Empty;
		public int TotalChanges { get; set; }
		public List<string> AffectedFiles { get; set; } = new();
	}

	/// <summary>
	/// Represents a change to a file
	/// </summary>
	public class FileChange
	{
		public string FilePath { get; set; } = string.Empty;
		public string? NewContent { get; set; }
		public List<TextEdit> Edits { get; set; } = new();
		public FileChangeKind Kind { get; set; }
	}

	/// <summary>
	/// Preview of a file change
	/// </summary>
	public class FileChangePreview : FileChange
	{
		public string? OldContent { get; set; }
		public List<DiffHunk> Diff { get; set; } = new();
	}

	/// <summary>
	/// Kind of file change
	/// </summary>
	public enum FileChangeKind
	{
		Modified,
		Created,
		Deleted,
		Renamed,
		Moved
	}

	/// <summary>
	/// Represents a text edit
	/// </summary>
	public class TextEdit
	{
		public TextSpan Span { get; set; }
		public string NewText { get; set; } = string.Empty;
	}

	/// <summary>
	/// Represents a diff hunk for preview
	/// </summary>
	public class DiffHunk
	{
		public int OldStart { get; set; }
		public int OldLength { get; set; }
		public int NewStart { get; set; }
		public int NewLength { get; set; }
		public List<DiffLine> Lines { get; set; } = new();
	}

	/// <summary>
	/// Represents a line in a diff
	/// </summary>
	public class DiffLine
	{
		public DiffLineKind Kind { get; set; }
		public string Text { get; set; } = string.Empty;
		public int? OldLineNumber { get; set; }
		public int? NewLineNumber { get; set; }
	}

	/// <summary>
	/// Kind of diff line
	/// </summary>
	public enum DiffLineKind
	{
		Context,
		Added,
		Removed
	}
}