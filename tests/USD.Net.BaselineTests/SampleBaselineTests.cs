using USD.Net.BaselineTests.TestUtils;
using Pxr.Usd.Sdf;
using Pxr.Usd;

namespace USD.Net.BaselineTests;

[Trait("Category", "Baseline")]
public class SampleBaselineTests
{
    [Theory]
    [InlineData("01_empty.usda")]
    [InlineData("02_simple.usda")]
    [InlineData("03_with_properties.usda")]
    public void SdfLayer_RoundTripSerialization_MatchesBaseline(string filename)
    {
        using var env = new TestEnvironment();
        
        var inputPath = BaselineManager.GetTestDataPath("Basic", filename);
        var outputPath = env.GetTempFilePath(filename);
        
        var layer = SdfLayer.FindOrOpen(inputPath);
        Assert.NotNull(layer);
        
        layer.Export(outputPath);
        
        Assert.True(File.Exists(outputPath), $"Export failed - output file not created: {outputPath}");
        
        var baselinePath = BaselineManager.GetBaselinePath("Basic", filename);
        FileComparison.AssertFilesEqual(outputPath, baselinePath);
    }
    
    [Theory]
    [InlineData("01_empty.usda")]
    [InlineData("02_simple.usda")]
    [InlineData("03_with_properties.usda")]
    public void UsdStage_RoundTripSerialization_MatchesBaseline(string filename)
    {
        using var env = new TestEnvironment();
        
        var inputPath = BaselineManager.GetTestDataPath("Basic", filename);
        var outputPath = env.GetTempFilePath($"stage_{filename}");
        
        var stage = UsdStage.Open(inputPath);
        Assert.NotNull(stage);
        
        stage.Export(outputPath);
        
        Assert.True(File.Exists(outputPath), $"Export failed - output file not created: {outputPath}");
        
        var baselinePath = BaselineManager.GetBaselinePath("Basic", $"stage_{filename}");
        FileComparison.AssertFilesEqual(outputPath, baselinePath);
    }
    
    [Fact]
    public void FileComparison_IdenticalFiles_DoesNotThrow()
    {
        using var env = new TestEnvironment();
        
        var content = "#usda 1.0\n(doc = \"test\")\n";
        env.WriteTextFile("file1.usda", content);
        env.WriteTextFile("file2.usda", content);
        
        var path1 = env.GetTempFilePath("file1.usda");
        var path2 = env.GetTempFilePath("file2.usda");
        
        FileComparison.AssertFilesEqual(path1, path2);
    }
    
    [Fact]
    public void FileComparison_DifferentFiles_ThrowsWithDetails()
    {
        using var env = new TestEnvironment();
        
        env.WriteTextFile("file1.usda", "#usda 1.0\n(doc = \"test1\")\n");
        env.WriteTextFile("file2.usda", "#usda 1.0\n(doc = \"test2\")\n");
        
        var path1 = env.GetTempFilePath("file1.usda");
        var path2 = env.GetTempFilePath("file2.usda");
        
        var exception = Assert.Throws<Exception>(() => FileComparison.AssertFilesEqual(path1, path2));
        Assert.Contains("Files differ", exception.Message);
        Assert.Contains("failures", exception.Message);
    }
    
    [Fact]
    public void BaselineManager_GetTestDataPath_ReturnsValidPath()
    {
        var path = BaselineManager.GetTestDataPath("Basic", "01_empty.usda");
        Assert.True(File.Exists(path), $"Test data file should exist: {path}");
    }
    
    [Fact]
    public void BaselineManager_GetTestDataFiles_ReturnsExpectedFiles()
    {
        var files = BaselineManager.GetTestDataFiles("Basic");
        Assert.Contains("01_empty.usda", files);
        Assert.Contains("02_simple.usda", files);
        Assert.Contains("03_with_properties.usda", files);
    }
    
    [Fact]
    public void TestEnvironment_TempFiles_AreIsolated()
    {
        string tempPath1, tempPath2;
        
        using (var env1 = new TestEnvironment())
        {
            tempPath1 = env1.TempDirectory;
            env1.WriteTextFile("test.txt", "content1");
            Assert.True(env1.FileExists("test.txt"));
        }
        
        using (var env2 = new TestEnvironment())
        {
            tempPath2 = env2.TempDirectory;
            env2.WriteTextFile("test.txt", "content2");
            Assert.True(env2.FileExists("test.txt"));
        }
        
        Assert.NotEqual(tempPath1, tempPath2);
        Assert.False(Directory.Exists(tempPath1));
        Assert.False(Directory.Exists(tempPath2));
    }
}