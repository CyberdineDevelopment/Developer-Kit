# Package Reference to Project Reference Conversion Plan

## Objective
Convert ALL FractalDataWorks package references to project references while preserving EVERY attribute from the original PackageReference elements.

## Critical Requirements
- **PRESERVE ALL ATTRIBUTES**: PrivateAssets, ExcludeAssets, AnalyzerLanguage, etc.
- **SYSTEMATIC CONVERSION**: Process every .csproj file 
- **COMPLETE REPORTING**: Document every conversion with before/after details
- **ZERO LOSS**: No simplification of complex attribute configurations

## Target Package References for Conversion
Based on standard FractalDataWorks naming patterns, convert these packages:
- FractalDataWorks.EnhancedEnums → ProjectReference
- FractalDataWorks.Messages → ProjectReference  
- FractalDataWorks.Results → ProjectReference
- FractalDataWorks.Configuration → ProjectReference
- FractalDataWorks.Configuration.Abstractions → ProjectReference
- FractalDataWorks.Services.* → ProjectReference
- FractalDataWorks.Data → ProjectReference
- FractalDataWorks.CodeBuilder.* → ProjectReference
- FractalDataWorks.EnhancedEnums.* → ProjectReference (analyzers/source generators)

## Special Attention Required
- **Source Generators**: FractalDataWorks.EnhancedEnums.SourceGenerators has complex attributes
- **Analyzers**: EnhancedEnums.Analyzers and CodeFixes have special analyzer attributes
- **Test Projects**: May reference multiple FractalDataWorks packages

## Conversion Process
1. Scan all .csproj files systematically
2. Identify FractalDataWorks PackageReference elements
3. Convert to ProjectReference with relative path
4. Preserve ALL original attributes exactly
5. Document each conversion with complete details

## Quality Verification
- Build verification after each major conversion batch
- No loss of functionality or build behavior
- All tests continue to pass

Starting systematic conversion...