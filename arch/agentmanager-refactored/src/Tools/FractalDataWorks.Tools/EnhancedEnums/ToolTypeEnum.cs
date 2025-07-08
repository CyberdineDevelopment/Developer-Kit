using System;
using FractalDataWorks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Tools.EnhancedEnums
{
	/// <summary>
	/// Enhanced enum for automatic tool discovery
	/// </summary>
	[EnhancedEnum("ToolTypes", IncludeReferencedAssemblies = true)]
	public abstract class ToolTypeEnum : IEnhancedEnum<ToolTypeEnum>
	{
		/// <summary>
		/// Gets the tool name
		/// </summary>
		public abstract string ToolName { get; }

		/// <summary>
		/// Gets the tool description
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Gets the tool category
		/// </summary>
		public abstract ToolCategory Category { get; }

		/// <summary>
		/// Gets the tool implementation type
		/// </summary>
		public abstract Type ToolType { get; }

		/// <summary>
		/// Creates an instance of the tool
		/// </summary>
		public abstract ITool CreateTool(IServiceProvider serviceProvider);

		/// <summary>
		/// Registers the tool with the service collection
		/// </summary>
		public virtual void RegisterTool(IServiceCollection services)
		{
			services.AddTransient(ToolType);
			services.AddTransient(typeof(ITool), provider => CreateTool(provider));
		}

		public string Name => ToolName;
		public abstract string Value { get; }
		public abstract int Order { get; }
	}
}