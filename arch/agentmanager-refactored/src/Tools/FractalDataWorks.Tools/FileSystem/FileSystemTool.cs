using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Tools.FileSystem
{
	/// <summary>
	/// Request for file system operations
	/// </summary>
	public class FileSystemRequest
	{
		public string Path { get; set; } = string.Empty;
		public FileSystemOperation Operation { get; set; }
		public string? Pattern { get; set; }
		public bool Recursive { get; set; }
		public string? Content { get; set; }
	}

	/// <summary>
	/// Response from file system operations
	/// </summary>
	public class FileSystemResponse
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public IReadOnlyList<string>? Items { get; set; }
		public string? Content { get; set; }
		public FileSystemInfo? Info { get; set; }
	}

	/// <summary>
	/// File system operations
	/// </summary>
	public enum FileSystemOperation
	{
		List,
		Read,
		Write,
		Delete,
		CreateDirectory,
		Exists,
		GetInfo
	}

	/// <summary>
	/// File system tool configuration
	/// </summary>
	public class FileSystemToolConfiguration : ConfigurationBase
	{
		public int MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
		public bool AllowHiddenFiles { get; set; } = false;
		public List<string> RestrictedPaths { get; set; } = new();

		public override ValidationResult Validate()
		{
			var errors = new List<string>();

			if (MaxFileSize <= 0)
				errors.Add("MaxFileSize must be greater than 0");

			return errors.Count == 0 
				? ValidationResult.Success() 
				: ValidationResult.Failure(errors.ToArray());
		}
	}

	/// <summary>
	/// File system operations tool
	/// </summary>
	public class FileSystemTool : ServiceBase<FileSystemToolConfiguration>, 
		ITool<FileSystemRequest, FileSystemResponse>
	{
		private readonly ILogger<FileSystemTool> _logger;

		public FileSystemTool(
			FileSystemToolConfiguration configuration,
			ILogger<FileSystemTool> logger) 
			: base(configuration)
		{
			_logger = logger;
		}

		public string ToolName => "FileSystem";
		public string Description => "Performs file system operations";
		public ToolCategory Category => ToolCategory.FileSystem;

		public async Task<Result<FileSystemResponse>> Execute(FileSystemRequest request)
		{
			try
			{
				if (IsRestrictedPath(request.Path))
				{
					return Result<FileSystemResponse>.Failure(
						ServiceMessage.Error($"Access to path '{request.Path}' is restricted"));
				}

				return request.Operation switch
				{
					FileSystemOperation.List => await ListDirectory(request),
					FileSystemOperation.Read => await ReadFile(request),
					FileSystemOperation.Write => await WriteFile(request),
					FileSystemOperation.Delete => await DeletePath(request),
					FileSystemOperation.CreateDirectory => await CreateDirectory(request),
					FileSystemOperation.Exists => await CheckExists(request),
					FileSystemOperation.GetInfo => await GetInfo(request),
					_ => Result<FileSystemResponse>.Failure(
						ServiceMessage.Error($"Unknown operation: {request.Operation}"))
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "File system operation failed");
				return Result<FileSystemResponse>.Failure(
					ServiceMessage.Error($"Operation failed: {ex.Message}"));
			}
		}

		private bool IsRestrictedPath(string path)
		{
			var fullPath = Path.GetFullPath(path);
			return Configuration.RestrictedPaths.Any(restricted => 
				fullPath.StartsWith(Path.GetFullPath(restricted), StringComparison.OrdinalIgnoreCase));
		}

		private Task<Result<FileSystemResponse>> ListDirectory(FileSystemRequest request)
		{
			if (!Directory.Exists(request.Path))
			{
				return Task.FromResult(Result<FileSystemResponse>.Failure(
					ServiceMessage.Error($"Directory not found: {request.Path}")));
			}

			var searchOption = request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var pattern = request.Pattern ?? "*";

			var items = Directory.GetFileSystemEntries(request.Path, pattern, searchOption)
				.Where(item => Configuration.AllowHiddenFiles || !IsHidden(item))
				.ToList();

			var response = new FileSystemResponse
			{
				Success = true,
				Items = items,
				Message = $"Found {items.Count} items"
			};

			return Task.FromResult(Result<FileSystemResponse>.Success(
				response, response.Message));
		}

		private async Task<Result<FileSystemResponse>> ReadFile(FileSystemRequest request)
		{
			if (!File.Exists(request.Path))
			{
				return Result<FileSystemResponse>.Failure(
					ServiceMessage.Error($"File not found: {request.Path}"));
			}

			var fileInfo = new FileInfo(request.Path);
			if (fileInfo.Length > Configuration.MaxFileSize)
			{
				return Result<FileSystemResponse>.Failure(
					ServiceMessage.Error($"File too large: {fileInfo.Length} bytes (max: {Configuration.MaxFileSize})"));
			}

			var content = await File.ReadAllTextAsync(request.Path);
			var response = new FileSystemResponse
			{
				Success = true,
				Content = content,
				Message = $"Read {content.Length} characters"
			};

			return Result<FileSystemResponse>.Success(response, response.Message);
		}

		private async Task<Result<FileSystemResponse>> WriteFile(FileSystemRequest request)
		{
			if (string.IsNullOrEmpty(request.Content))
			{
				return Result<FileSystemResponse>.Failure(
					ServiceMessage.Error("Content cannot be empty"));
			}

			var directory = Path.GetDirectoryName(request.Path);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			await File.WriteAllTextAsync(request.Path, request.Content);
			
			var response = new FileSystemResponse
			{
				Success = true,
				Message = $"Written {request.Content.Length} characters to {request.Path}"
			};

			return Result<FileSystemResponse>.Success(response, response.Message);
		}

		private Task<Result<FileSystemResponse>> DeletePath(FileSystemRequest request)
		{
			if (File.Exists(request.Path))
			{
				File.Delete(request.Path);
				return Task.FromResult(Result<FileSystemResponse>.Success(
					new FileSystemResponse { Success = true, Message = $"File deleted: {request.Path}" },
					"File deleted successfully"));
			}

			if (Directory.Exists(request.Path))
			{
				Directory.Delete(request.Path, request.Recursive);
				return Task.FromResult(Result<FileSystemResponse>.Success(
					new FileSystemResponse { Success = true, Message = $"Directory deleted: {request.Path}" },
					"Directory deleted successfully"));
			}

			return Task.FromResult(Result<FileSystemResponse>.Failure(
				ServiceMessage.Error($"Path not found: {request.Path}")));
		}

		private Task<Result<FileSystemResponse>> CreateDirectory(FileSystemRequest request)
		{
			Directory.CreateDirectory(request.Path);
			
			return Task.FromResult(Result<FileSystemResponse>.Success(
				new FileSystemResponse { Success = true, Message = $"Directory created: {request.Path}" },
				"Directory created successfully"));
		}

		private Task<Result<FileSystemResponse>> CheckExists(FileSystemRequest request)
		{
			var exists = File.Exists(request.Path) || Directory.Exists(request.Path);
			
			return Task.FromResult(Result<FileSystemResponse>.Success(
				new FileSystemResponse { Success = exists, Message = exists ? "Path exists" : "Path does not exist" },
				exists ? "Path found" : "Path not found"));
		}

		private Task<Result<FileSystemResponse>> GetInfo(FileSystemRequest request)
		{
			FileSystemInfo? info = null;

			if (File.Exists(request.Path))
				info = new FileInfo(request.Path);
			else if (Directory.Exists(request.Path))
				info = new DirectoryInfo(request.Path);

			if (info == null)
			{
				return Task.FromResult(Result<FileSystemResponse>.Failure(
					ServiceMessage.Error($"Path not found: {request.Path}")));
			}

			return Task.FromResult(Result<FileSystemResponse>.Success(
				new FileSystemResponse { Success = true, Info = info, Message = "Info retrieved" },
				"File system info retrieved"));
		}

		private bool IsHidden(string path)
		{
			try
			{
				var info = new FileInfo(path);
				return (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
			}
			catch
			{
				return false;
			}
		}
	}
}