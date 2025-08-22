# Package Reference Conversion Tracking

## Conversion Status: STARTING SYSTEMATIC CONVERSION

### Projects Requiring Analysis (50 files identified)
All projects with FractalDataWorks references need systematic review to identify:
1. PackageReference → ProjectReference conversions needed
2. Attribute preservation requirements
3. Build verification after conversion

### Critical Conversion Examples Identified

**FractalDataWorks.EnhancedEnums.SourceGenerators** (Complex attributes):
```xml
BEFORE: <PackageReference Include="FractalDataWorks.CodeBuilder.Abstractions" />
AFTER:  <ProjectReference Include="../FractalDataWorks.CodeBuilder.Abstractions/FractalDataWorks.CodeBuilder.Abstractions.csproj" />

BEFORE: <PackageReference Include="FractalDataWorks.CodeBuilder.CSharp" PrivateAssets="none" />
AFTER:  <ProjectReference Include="../FractalDataWorks.CodeBuilder.CSharp/FractalDataWorks.CodeBuilder.CSharp.csproj" PrivateAssets="none" />

BEFORE: <PackageReference Include="FractalDataWorks.EnhancedEnums" PrivateAssets="all" />
AFTER:  <ProjectReference Include="../FractalDataWorks.EnhancedEnums/FractalDataWorks.EnhancedEnums.csproj" PrivateAssets="all" />
```

### Delegation Strategy
This requires specialized dotnet-refactor-specialist agent with:
- Systematic file analysis capability
- Attribute preservation expertise
- Build verification coordination
- Detailed conversion reporting

**DELEGATING TO DOTNET-REFACTOR-SPECIALIST FOR SYSTEMATIC CONVERSION**

## Conversion Requirements for Agent
1. **PRESERVE ALL ATTRIBUTES** - PrivateAssets, ExcludeAssets, etc.
2. **SYSTEMATIC PROCESSING** - All 50 files with FractalDataWorks references
3. **COMPLETE REPORTING** - Document every conversion with before/after
4. **BUILD VERIFICATION** - Ensure solution builds after conversion
5. **NO SIMPLIFICATION** - Keep complex configurations exactly as-is

**Status: Awaiting specialized agent execution...**