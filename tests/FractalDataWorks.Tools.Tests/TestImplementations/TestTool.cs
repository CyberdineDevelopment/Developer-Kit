namespace FractalDataWorks.Tools.Tests.TestImplementations;

/// <summary>
/// Test implementation of IFdwTool for testing purposes.
/// </summary>
public sealed class TestTool : IFdwTool
{
    public string Id { get; set; } = "test-tool-123";
    
    public string Name { get; set; } = "TestTool";
    
    public string Version { get; set; } = "1.0.0";
    
    public bool IsInitialized { get; set; }
    
    public void Initialize()
    {
        IsInitialized = true;
    }
}