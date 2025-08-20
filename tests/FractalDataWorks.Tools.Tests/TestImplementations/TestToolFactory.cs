using System.Threading.Tasks;
using FractalDataWorks.Services;

namespace FractalDataWorks.Tools.Tests.TestImplementations;

/// <summary>
/// Test implementation of IToolFactory for testing purposes.
/// </summary>
public sealed class TestToolFactory : IToolFactory
{
    public bool CreateCalled { get; set; }
    public bool CreateGenericCalled { get; set; }
    
    public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwTool
    {
        CreateGenericCalled = true;
        var tool = new TestTool { Name = $"Created from {configuration.SectionName}" };
        return FdwResult<T>.Success((T)(object)tool);
    }
    
    public IFdwResult<IFdwTool> Create(IFdwConfiguration configuration)
    {
        CreateCalled = true;
        var tool = new TestTool { Name = $"Created from {configuration.SectionName}" };
        return FdwResult<IFdwTool>.Success(tool);
    }
}

/// <summary>
/// Test implementation of IToolFactory{TTool, TConfiguration} for testing purposes.
/// </summary>
public sealed class TestGenericToolFactory : IToolFactory<TestTool, TestConfiguration>
{
    public bool CreateCalled { get; set; }
    public bool CreateGenericCalled { get; set; }
    public bool CreateTypedCalled { get; set; }
    public TestConfiguration? LastConfiguration { get; set; }
    public IFdwConfiguration? LastGenericConfiguration { get; set; }
    public string? LastConfigurationName { get; set; }
    public int? LastConfigurationId { get; set; }
    
    public IFdwResult<TestTool> Create(TestConfiguration configuration)
    {
        CreateTypedCalled = true;
        LastConfiguration = configuration;
        var tool = new TestTool { Name = $"Created from {configuration.Name}" };
        return FdwResult<TestTool>.Success(tool);
    }
    
    public IFdwResult<TestTool> Create(IFdwConfiguration configuration)
    {
        CreateCalled = true;
        LastGenericConfiguration = configuration;
        var tool = new TestTool { Name = $"Created from {configuration.SectionName}" };
        return FdwResult<TestTool>.Success(tool);
    }
    
    public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwTool
    {
        CreateGenericCalled = true;
        var tool = new TestTool { Name = $"Created from {configuration.SectionName}" };
        return FdwResult<T>.Success((T)(object)tool);
    }
    
    IFdwResult<IFdwTool> IToolFactory.Create(IFdwConfiguration configuration)
    {
        var result = Create(configuration);
        return FdwResult<IFdwTool>.Success(result.Value);
    }
    
    public Task<TestTool> GetTool(string configurationName)
    {
        LastConfigurationName = configurationName;
        return Task.FromResult(new TestTool { Name = configurationName });
    }
    
    public Task<TestTool> GetTool(int configurationId)
    {
        LastConfigurationId = configurationId;
        return Task.FromResult(new TestTool { Name = $"Config_{configurationId}" });
    }
}

/// <summary>
/// Test implementation of ToolTypeFactoryBase for testing purposes.
/// </summary>
public sealed class TestToolTypeFactory : ToolTypeFactoryBase<TestTool, TestConfiguration>
{
    public TestConfiguration? LastConfiguration { get; set; }
    public string? LastConfigurationName { get; set; }
    public int? LastConfigurationId { get; set; }
    public bool CreateCalled { get; set; }
    public bool GetToolByNameCalled { get; set; }
    public bool GetToolByIdCalled { get; set; }
    
    public TestToolTypeFactory(int id, string name, string description)
        : base(id, name, description)
    {
    }
    
    public override object Create(TestConfiguration configuration)
    {
        CreateCalled = true;
        LastConfiguration = configuration;
        return new TestTool { Name = $"Created from {configuration?.Name ?? "NULL"}" };
    }
    
    public override Task<TestTool> GetTool(string configurationName)
    {
        GetToolByNameCalled = true;
        LastConfigurationName = configurationName;
        return Task.FromResult(new TestTool { Name = configurationName });
    }
    
    public override Task<TestTool> GetTool(int configurationId)
    {
        GetToolByIdCalled = true;
        LastConfigurationId = configurationId;
        return Task.FromResult(new TestTool { Name = $"Config_{configurationId}" });
    }
}