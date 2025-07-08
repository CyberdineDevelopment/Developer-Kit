using System;
using FractalDataWorks.Tools.EnhancedEnums;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Tools.FileSystem
{
	/// <summary>
	/// Enhanced enum registration for FileSystemTool
	/// </summary>
	[EnumOption("FileSystem", typeof(ToolTypeEnum))]
	public class FileSystemToolEnum : ToolTypeEnum
	{
		public override string ToolName => "FileSystem";
		public override string Description => "Performs file system operations including read, write, list, and delete";
		public override ToolCategory Category => ToolCategory.FileSystem;
		public override Type ToolType => typeof(FileSystemTool);
		public override string Value => "filesystem";
		public override int Order => 100;

		public override ITool CreateTool(IServiceProvider serviceProvider)
		{
			return serviceProvider.GetRequiredService<FileSystemTool>();
		}

		public override void RegisterTool(IServiceCollection services)
		{
			services.AddTransient<FileSystemTool>();
			services.AddTransient<ITool>(provider => provider.GetRequiredService<FileSystemTool>());
		}
	}
}