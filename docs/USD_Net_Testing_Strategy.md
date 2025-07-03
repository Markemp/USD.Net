# USD.Net Testing Strategy: OpenUSD-Equivalent Testing Implementation

## Executive Summary

This document outlines a comprehensive testing strategy for USD.Net that mirrors OpenUSD's sophisticated testing methodology while leveraging .NET ecosystem tools. The strategy focuses on file comparison testing, baseline validation, and comprehensive coverage of USD functionality.

## Current State Analysis

### Existing USD.Net Test Structure
- **Unit Tests**: `tests/USD.Net.Tests/` - 289+ passing tests
- **Integration Tests**: `tests/USD.Net.IntegrationTests/` - 6 passing tests
- **Test Framework**: xUnit with trait-based categorization
- **Current Coverage**: Core USD API, geometry classes, serialization

### Gaps Identified
1. **No File Comparison Testing**: Missing baseline comparison infrastructure
2. **Limited Test Data**: No comprehensive test file collections
3. **No Round-trip Testing**: Limited export/import validation
4. **No Error Case Testing**: Missing negative testing scenarios
5. **No Environment Control**: No isolated test environments

## Recommended Testing Architecture

### Directory Structure
```
tests/
├── USD.Net.Tests/                      # Unit tests (existing)
├── USD.Net.IntegrationTests/           # Integration tests (existing)
├── USD.Net.BaselineTests/              # NEW: File comparison tests
│   ├── TestData/                       # Test input files
│   │   ├── Basic/                      # Basic USD files
│   │   │   ├── 01_empty.usda
│   │   │   ├── 02_simple.usda
│   │   │   └── ...
│   │   ├── Geometry/                   # Geometry-specific tests
│   │   ├── Composition/                # References, payloads, variants
│   │   ├── ErrorCases/                 # Files expected to fail
│   │   └── Complex/                    # Complex scene files
│   ├── Baselines/                      # Expected output files
│   │   ├── Basic/
│   │   ├── Geometry/
│   │   └── ...
│   └── TestUtils/                      # Utility classes
│       ├── FileComparison.cs
│       ├── TestEnvironment.cs
│       └── BaselineManager.cs
└── TestData/                           # Shared test data
    ├── OpenUSD/                        # OpenUSD test files (imported)
    └── Custom/                         # USD.Net specific test files
```

### Test Categories and Implementation

#### 1. Baseline Comparison Tests
**Pattern**: Load → Process → Export → Compare with baseline
```csharp
[Trait("Category", "Baseline")]
public class SdfLayerBaselineTests
{
    [Theory]
    [InlineData("01_empty.usda")]
    [InlineData("02_simple.usda")]
    [InlineData("03_complex.usda")]
    public void RoundTripSerialization_MatchesBaseline(string filename)
    {
        // Load test file
        var inputPath = TestData.GetPath("Basic", filename);
        var layer = SdfLayer.FindOrOpen(inputPath);
        
        // Export to temporary file
        var outputPath = Path.GetTempFileName() + ".usda";
        layer.Export(outputPath);
        
        // Compare with baseline
        var baselinePath = Baselines.GetPath("Basic", filename);
        FileComparison.AssertFilesEqual(outputPath, baselinePath);
    }
}
```

#### 2. Error Case Testing
**Pattern**: Test files designed to fail or produce warnings
```csharp
[Trait("Category", "ErrorHandling")]
public class ErrorCaseTests
{
    [Theory]
    [InlineData("bad_syntax.usda")]
    [InlineData("bad_types.usda")]
    [InlineData("bad_references.usda")]
    public void LoadErrorCase_HandlesGracefully(string filename)
    {
        var inputPath = TestData.GetPath("ErrorCases", filename);
        var layer = SdfLayer.FindOrOpen(inputPath);
        
        // Should create invalid layer, not throw exception
        Assert.False(layer.IsValid);
    }
}
```

#### 3. Comprehensive API Tests
**Pattern**: Test all public API methods with file validation
```csharp
[Trait("Category", "API")]
public class UsdStageAPITests
{
    [Fact]
    public void CreateStage_ExportAndCompare()
    {
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/root");
        
        // Export and compare with expected structure
        var outputPath = Path.GetTempFileName() + ".usda";
        stage.Export(outputPath);
        
        FileComparison.AssertContains(outputPath, "def \"root\"");
    }
}
```

#### 4. Performance and Regression Tests
**Pattern**: Validate performance characteristics and prevent regressions
```csharp
[Trait("Category", "Performance")]
public class PerformanceTests
{
    [Fact]
    public void LoadLargeFile_WithinTimeLimit()
    {
        var sw = Stopwatch.StartNew();
        var layer = SdfLayer.FindOrOpen(TestData.GetPath("Complex", "large_scene.usda"));
        sw.Stop();
        
        Assert.True(sw.ElapsedMilliseconds < 1000, $"Load took {sw.ElapsedMilliseconds}ms");
    }
}
```

## Implementation Plan

### Phase 1: Infrastructure (2-3 weeks)
1. **Create FileComparison Utility**
   - Text file comparison with normalization
   - Path-independent comparison
   - Diff reporting for failures
   - Visual diff integration

2. **Create BaselineManager**
   - Baseline file management
   - Baseline generation/regeneration
   - Version control integration
   - Baseline validation

3. **Create TestEnvironment**
   - Isolated test directories
   - Environment variable control
   - Cleanup mechanisms
   - Parallel test isolation

4. **Set up Test Data Structure**
   - Import OpenUSD test files
   - Create USD.Net specific test files
   - Organize by category and complexity
   - Version control large files (Git LFS)

### Phase 2: Core Testing (3-4 weeks)
1. **Implement Baseline Tests**
   - SdfLayer round-trip tests
   - UsdStage serialization tests
   - Property and metadata tests
   - Path handling tests

2. **Implement Error Case Tests**
   - Invalid syntax handling
   - Type validation
   - Reference resolution failures
   - Composition errors

3. **Implement API Coverage Tests**
   - All public methods tested
   - File output validation
   - Cross-platform compatibility
   - Memory leak detection

### Phase 3: Advanced Testing (2-3 weeks)
1. **Implement Performance Tests**
   - Load/save performance benchmarks
   - Memory usage validation
   - Scalability testing
   - Regression prevention

2. **Implement Compatibility Tests**
   - OpenUSD file compatibility
   - Version compatibility
   - Platform-specific testing
   - Encoding handling

## Test Data Management

### OpenUSD Test File Import
```csharp
public static class OpenUSDTestFiles
{
    public static void ImportTestFiles()
    {
        // Import relevant test files from ../OpenUSD/pxr/usd/sdf/testenv/
        var sourceDir = "../OpenUSD/pxr/usd/sdf/testenv/testSdfParsing.testenv";
        var targetDir = "TestData/OpenUSD/SdfParsing";
        
        // Copy .usda files with proper organization
        ImportTestDirectory(sourceDir, targetDir);
    }
}
```

### Baseline Generation
```csharp
public static class BaselineGenerator
{
    public static void GenerateBaselines()
    {
        // Generate baseline files by exporting current implementation
        foreach (var testFile in TestData.GetAllTestFiles())
        {
            var layer = SdfLayer.FindOrOpen(testFile);
            var baselinePath = Baselines.GetPath(testFile);
            layer.Export(baselinePath);
        }
    }
}
```

## Continuous Integration Integration

### MSBuild Integration
```xml
<Target Name="RunBaselineTests" BeforeTargets="Build">
  <ItemGroup>
    <BaselineTest Include="tests/USD.Net.BaselineTests/**/*.cs" />
  </ItemGroup>
  <Exec Command="dotnet test --filter Category=Baseline" />
</Target>
```

### GitHub Actions
```yaml
name: Baseline Tests
on: [push, pull_request]
jobs:
  baseline-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
      with:
        lfs: true  # For large test files
    - name: Run Baseline Tests
      run: dotnet test --filter Category=Baseline
    - name: Upload Failure Artifacts
      if: failure()
      uses: actions/upload-artifact@v3
      with:
        name: test-failures
        path: test-failures/
```

## Tools and Utilities

### FileComparison.cs
```csharp
public static class FileComparison
{
    public static void AssertFilesEqual(string actualPath, string expectedPath)
    {
        var actual = NormalizeFile(File.ReadAllText(actualPath));
        var expected = NormalizeFile(File.ReadAllText(expectedPath));
        
        if (actual != expected)
        {
            SaveFailureArtifacts(actualPath, expectedPath);
            throw new AssertionException($"Files differ. See {GetFailureDir()}");
        }
    }
    
    private static string NormalizeFile(string content)
    {
        // Remove absolute paths
        // Normalize line endings
        // Remove timestamps
        // Sort metadata for consistent comparison
    }
}
```

### TestEnvironment.cs
```csharp
public class TestEnvironment : IDisposable
{
    private readonly string _tempDir;
    private readonly Dictionary<string, string> _originalEnvVars;
    
    public TestEnvironment()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        
        // Set up isolated environment
        Environment.SetEnvironmentVariable("USD_NET_TEST_MODE", "1");
    }
    
    public void Dispose()
    {
        // Cleanup temp directory
        // Restore environment variables
    }
}
```

## Success Metrics

### Coverage Targets
- **File Comparison Tests**: 100+ test files covering all USD features
- **Round-trip Tests**: 95% of public API methods tested with file validation
- **Error Case Coverage**: 50+ error scenarios tested
- **Performance Tests**: All core operations have performance baselines

### Quality Gates
- **No Regression**: All baseline tests must pass
- **File Compatibility**: 100% compatibility with OpenUSD-generated files
- **Performance**: No performance regression > 10%
- **Memory**: No memory leaks detected

## Migration Strategy

### Phase 1: Parallel Implementation
- Keep existing tests running
- Add baseline tests alongside
- Gradually migrate tests to use file comparison

### Phase 2: Integration
- Merge test categories
- Consolidate test data
- Standardize on baseline approach

### Phase 3: Optimization
- Optimize test performance
- Minimize test data size
- Improve failure reporting

## Long-term Maintenance

### Baseline Management
- **Version Control**: Track baseline changes
- **Automated Updates**: Tools for baseline regeneration
- **Review Process**: Manual review of baseline changes
- **Rollback Capability**: Ability to revert problematic changes

### Test Data Evolution
- **Regular Updates**: Import new OpenUSD test files
- **Custom Test Cases**: Add USD.Net specific scenarios
- **Performance Tracking**: Monitor test execution performance
- **Cleanup**: Remove obsolete test cases

This comprehensive testing strategy ensures USD.Net maintains high compatibility with OpenUSD while providing robust validation of all functionality through file comparison and baseline testing.