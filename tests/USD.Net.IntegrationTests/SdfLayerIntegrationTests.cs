using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.IntegrationTests;

[Trait("Category", "Integration")]
public class SdfLayerIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void SdfLayer_Export_ValidFilename_ShouldSucceed()
    {
        // Arrange
        var layer = new SdfLayer("test://export");

        // Act
        var result = layer.Export("test_output.usda");

        // Assert
        Assert.True(result);
        
        // Cleanup
        if (File.Exists("test_output.usda"))
            File.Delete("test_output.usda");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SdfLayer_Export_WithComment_ShouldSetCommentMetadata()
    {
        // Arrange
        var layer = new SdfLayer("test://export_comment");

        // Act
        var result = layer.Export("test_output_with_comment.usda", "Test comment");

        // Assert
        Assert.True(result);
        Assert.True(layer.HasMetadata(new TfToken("comment")));
        Assert.Equal("Test comment", layer.GetMetadata(new TfToken("comment")).Get<string>());
        
        // Verify file content
        var content = File.ReadAllText("test_output_with_comment.usda");
        Assert.Contains("#usda 1.0", content);
        Assert.Contains("doc = \"Test comment\"", content);
        
        // Cleanup
        if (File.Exists("test_output_with_comment.usda"))
            File.Delete("test_output_with_comment.usda");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SdfLayer_Export_EmptyFilename_ShouldFail()
    {
        // Arrange
        var layer = new SdfLayer("test://export_empty");

        // Act & Assert
        Assert.False(layer.Export(""));
        Assert.False(layer.Export(null!));
    }
}