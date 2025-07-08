using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Tools
{
	/// <summary>
	/// Base interface for all tools
	/// </summary>
	public interface ITool : IGenericService
	{
		/// <summary>
		/// Gets the tool name
		/// </summary>
		string ToolName { get; }

		/// <summary>
		/// Gets the tool description
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Gets the tool category
		/// </summary>
		ToolCategory Category { get; }
	}

	/// <summary>
	/// Tool categories for organization
	/// </summary>
	public enum ToolCategory
	{
		FileSystem,
		Git,
		Process,
		Environment,
		Network,
		Security
	}

	/// <summary>
	/// Generic tool execution interface
	/// </summary>
	public interface ITool<TRequest, TResponse> : ITool
		where TRequest : class
		where TResponse : class
	{
		/// <summary>
		/// Executes the tool with the given request
		/// </summary>
		Task<Result<TResponse>> Execute(TRequest request);
	}
}