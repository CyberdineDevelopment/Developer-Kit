using System;
using FractalDataWorks.Services;
using FractalDataWorks.Services.CodeManagement.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.CodeManagement.EnhancedEnums
{
	/// <summary>
	/// Enhanced enum for automatic code analyzer discovery
	/// </summary>
	[EnhancedEnum("CodeAnalyzers", IncludeReferencedAssemblies = true)]
	public abstract class CodeAnalyzerTypeEnum : IEnhancedEnum<CodeAnalyzerTypeEnum>
	{
		/// <summary>
		/// Gets the programming language this analyzer supports
		/// </summary>
		public abstract string Language { get; }

		/// <summary>
		/// Gets the analyzer implementation type
		/// </summary>
		public abstract Type AnalyzerType { get; }

		/// <summary>
		/// Gets the file extensions this analyzer supports
		/// </summary>
		public abstract string[] SupportedExtensions { get; }

		/// <summary>
		/// Creates an instance of the analyzer
		/// </summary>
		public abstract ICodeAnalyzer CreateAnalyzer(IServiceProvider serviceProvider);

		/// <summary>
		/// Registers the analyzer with the service collection
		/// </summary>
		public virtual void RegisterAnalyzer(IServiceCollection services)
		{
			services.AddTransient(AnalyzerType);
			services.AddTransient<ICodeAnalyzer>(provider => CreateAnalyzer(provider));
		}

		public string Name => Language;
		public abstract string Value { get; }
		public abstract int Order { get; }
	}

	/// <summary>
	/// Enhanced enum for code refactorer discovery
	/// </summary>
	[EnhancedEnum("CodeRefactorers", IncludeReferencedAssemblies = true)]
	public abstract class CodeRefactorerTypeEnum : IEnhancedEnum<CodeRefactorerTypeEnum>
	{
		/// <summary>
		/// Gets the programming language this refactorer supports
		/// </summary>
		public abstract string Language { get; }

		/// <summary>
		/// Gets the refactorer implementation type
		/// </summary>
		public abstract Type RefactorerType { get; }

		/// <summary>
		/// Creates an instance of the refactorer
		/// </summary>
		public abstract ICodeRefactorer CreateRefactorer(IServiceProvider serviceProvider);

		/// <summary>
		/// Registers the refactorer with the service collection
		/// </summary>
		public virtual void RegisterRefactorer(IServiceCollection services)
		{
			services.AddTransient(RefactorerType);
			services.AddTransient<ICodeRefactorer>(provider => CreateRefactorer(provider));
		}

		public string Name => Language;
		public abstract string Value { get; }
		public abstract int Order { get; }
	}

	/// <summary>
	/// Enhanced enum for code generator discovery
	/// </summary>
	[EnhancedEnum("CodeGenerators", IncludeReferencedAssemblies = true)]
	public abstract class CodeGeneratorTypeEnum : IEnhancedEnum<CodeGeneratorTypeEnum>
	{
		/// <summary>
		/// Gets the programming language this generator supports
		/// </summary>
		public abstract string Language { get; }

		/// <summary>
		/// Gets the generator implementation type
		/// </summary>
		public abstract Type GeneratorType { get; }

		/// <summary>
		/// Creates an instance of the generator
		/// </summary>
		public abstract ICodeGenerator CreateGenerator(IServiceProvider serviceProvider);

		/// <summary>
		/// Registers the generator with the service collection
		/// </summary>
		public virtual void RegisterGenerator(IServiceCollection services)
		{
			services.AddTransient(GeneratorType);
			services.AddTransient<ICodeGenerator>(provider => CreateGenerator(provider));
		}

		public string Name => Language;
		public abstract string Value { get; }
		public abstract int Order { get; }
	}
}