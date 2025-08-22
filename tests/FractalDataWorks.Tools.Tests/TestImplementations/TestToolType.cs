using FractalDataWorks.Services;

namespace FractalDataWorks.Tools.Tests.TestImplementations;

/// <summary>
/// Test implementation of ToolTypeBase for testing purposes.
/// </summary>
public sealed class TestToolType : ToolTypeBase
{
    private readonly IToolFactory _factory;
    
    public TestToolType(int id, string name, string description, IToolFactory factory) 
        : base(id, name, description)
    {
        _factory = factory;
    }
    
    public override IToolFactory CreateFactory() => _factory;
}

/// <summary>
/// Generic test implementation of ToolTypeBase{TTool, TConfiguration} for testing purposes.
/// </summary>
public sealed class TestGenericToolType : ToolTypeBase<TestTool, TestConfiguration>
{
    private readonly IToolFactory<TestTool, TestConfiguration> _factory;
    
    public TestGenericToolType(int id, string name, string description, IToolFactory<TestTool, TestConfiguration> factory)
        : base(id, name, description)
    {
        _factory = factory;
    }
    
    public override IToolFactory<TestTool, TestConfiguration> CreateTypedFactory() => _factory;
}