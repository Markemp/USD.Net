using USD.Net.BaselineTests.TestUtils;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace USD.Net.BaselineTests;

[Trait("Category", "ProgrammaticExport")]
public class ProgrammaticExportTests
{
    [Fact]
    public void UsdStage_ProgrammaticCreation_ExportsCorrectly()
    {
        using var env = new TestEnvironment();
        
        // Create stage programmatically
        var stage = UsdStage.CreateInMemory();
        
        // Create the same structure as our test file
        var rootPrim = stage.DefinePrim("/root");
        rootPrim.CreateAttribute("description", "string").Set("A simple root prim");
        
        // Export the stage
        var outputPath = env.GetTempFilePath("programmatic_export.usda");
        stage.Export(outputPath);
        
        // Verify the export contains expected content
        Assert.True(File.Exists(outputPath));
        var content = File.ReadAllText(outputPath);
        
        // Check for expected content
        Assert.Contains("#usda 1.0", content);
        Assert.Contains("def \"root\"", content); // No longer defaults to Xform type
        Assert.Contains("string description = \"A simple root prim\"", content);
    }
    
    [Fact]
    public void UsdStage_ComplexProgrammaticCreation_ExportsCorrectly()
    {
        using var env = new TestEnvironment();
        
        // Create stage with more complex structure
        var stage = UsdStage.CreateInMemory();
        
        // Create root with properties
        var rootPrim = stage.DefinePrim("/root");
        rootPrim.CreateAttribute("description", "string").Set("A root prim with properties");
        rootPrim.CreateAttribute("count", "int").Set(42);
        rootPrim.CreateAttribute("position", "float3").Set(new GfVec3f(1.0f, 2.0f, 3.0f));
        
        // Create child prim
        var childPrim = stage.DefinePrim("/root/child");
        childPrim.CreateAttribute("active", "bool").Set(true);
        childPrim.CreateAttribute("name", "string").Set("child_prim");
        
        // Export the stage
        var outputPath = env.GetTempFilePath("complex_export.usda");
        stage.Export(outputPath);
        
        // Verify the export
        Assert.True(File.Exists(outputPath));
        var content = File.ReadAllText(outputPath);
        
        // Check structure
        Assert.Contains("def \"root\"", content); // No longer defaults to Xform type
        Assert.Contains("string description = \"A root prim with properties\"", content);
        Assert.Contains("int count = 42", content);
        Assert.Contains("float3 position = (1.0, 2.0, 3.0)", content); // Updated float format
        Assert.Contains("def \"child\"", content); // No longer defaults to Xform type
        Assert.Contains("bool active = true", content);
        Assert.Contains("string name = \"child_prim\"", content);
    }
    
    [Fact]
    public void SdfLayer_CanRoundTrip_WithParser()
    {
        using var env = new TestEnvironment();
        
        // Create a test USDA file
        var testContent = @"#usda 1.0
(
    doc = ""Test file for parsing""
)

def ""TestPrim""
{
    string name = ""test""
}";
        
        var inputPath = env.GetTempFilePath("test_input.usda");
        File.WriteAllText(inputPath, testContent);
        
        // Try to load it
        var layer = SdfLayer.FindOrOpen(inputPath);
        Assert.NotNull(layer);
        
        // Export it
        var outputPath = env.GetTempFilePath("test_output.usda");
        layer.Export(outputPath);
        
        // Read the exported content
        var exportedContent = File.ReadAllText(outputPath);
        
        // The parser should now successfully load and export the content
        Assert.Contains("#usda 1.0", exportedContent);
        Assert.Contains("TestPrim", exportedContent); // Prim should be loaded
        Assert.Contains("doc =", exportedContent); // Metadata should be loaded
        Assert.Contains("string name = \"test\"", exportedContent); // Attribute should be loaded
    }
}