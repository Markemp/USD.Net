using USD.Net.BaselineTests.TestUtils;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace USD.Net.BaselineTests;

[Trait("Category", "WriterValidation")]
public class WriterValidationTests
{
    [Fact]
    public void UsdaWriter_SimpleStage_ProducesValidFormat()
    {
        using var env = new TestEnvironment();
        
        // Create a simple stage
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/hello");
        prim.CreateAttribute("myString", "string").Set("world");
        
        // Export it
        var outputPath = env.GetTempFilePath("simple.usda");
        stage.Export(outputPath);
        
        // Read and verify content
        var content = File.ReadAllText(outputPath);
        Console.WriteLine("=== Simple Stage Output ===");
        Console.WriteLine(content);
        Console.WriteLine("=========================");
        
        // Basic format checks
        Assert.Contains("#usda 1.0", content);
        Assert.Contains("def \"hello\"", content); // No longer defaults to Xform type
        Assert.Contains("string myString = \"world\"", content);
    }
    
    [Fact]
    public void UsdaWriter_EmptyStage_ProducesValidHeader()
    {
        using var env = new TestEnvironment();
        
        // Create empty stage
        var stage = UsdStage.CreateInMemory();
        
        // Export it
        var outputPath = env.GetTempFilePath("empty.usda");
        stage.Export(outputPath);
        
        // Read and verify content
        var content = File.ReadAllText(outputPath);
        Console.WriteLine("=== Empty Stage Output ===");
        Console.WriteLine(content);
        Console.WriteLine("========================");
        
        // Should have header (no default metadata added for empty stages)
        Assert.Contains("#usda 1.0", content);
    }
    
    [Fact]
    public void UsdaWriter_HierarchicalStage_PreservesStructure()
    {
        using var env = new TestEnvironment();
        
        // Create hierarchical structure
        var stage = UsdStage.CreateInMemory();
        var root = stage.DefinePrim("/root");
        var child1 = stage.DefinePrim("/root/child1");
        var child2 = stage.DefinePrim("/root/child2");
        var grandchild = stage.DefinePrim("/root/child1/grandchild");
        
        // Add some attributes
        root.CreateAttribute("rootAttr", "string").Set("rootValue");
        child1.CreateAttribute("child1Attr", "int").Set(42);
        grandchild.CreateAttribute("gcAttr", "float").Set(3.14f);
        
        // Export it
        var outputPath = env.GetTempFilePath("hierarchy.usda");
        stage.Export(outputPath);
        
        // Read and verify content
        var content = File.ReadAllText(outputPath);
        Console.WriteLine("=== Hierarchical Stage Output ===");
        Console.WriteLine(content);
        Console.WriteLine("================================");
        
        // Check structure (no longer defaults to Xform type)
        Assert.Contains("def \"root\"", content);
        Assert.Contains("def \"child1\"", content);
        Assert.Contains("def \"child2\"", content);
        Assert.Contains("def \"grandchild\"", content);
        
        // Check attributes
        Assert.Contains("string rootAttr = \"rootValue\"", content);
        Assert.Contains("int child1Attr = 42", content);
        Assert.Contains("float gcAttr = 3.14", content);
    }
    
    [Fact]
    public void UsdaWriter_GeometryPrims_UsesCorrectTypeNames()
    {
        using var env = new TestEnvironment();
        
        // Create various geometry types
        var stage = UsdStage.CreateInMemory();
        
        // Create typed prims
        var xform = UsdGeomXform.Define(stage, "/myXform");
        var sphere = UsdGeomSphere.Define(stage, "/mySphere");
        var cube = UsdGeomCube.Define(stage, "/myCube");
        var mesh = UsdGeomMesh.Define(stage, "/myMesh");
        
        // Set some geometry-specific attributes
        sphere.CreateRadiusAttr().Set(2.0);
        cube.CreateSizeAttr().Set(1.0);
        
        // Export it
        var outputPath = env.GetTempFilePath("geometry.usda");
        stage.Export(outputPath);
        
        // Read and verify content
        var content = File.ReadAllText(outputPath);
        Console.WriteLine("=== Geometry Stage Output ===");
        Console.WriteLine(content);
        Console.WriteLine("============================");
        
        // Check type names
        Assert.Contains("def Xform \"myXform\"", content);
        Assert.Contains("def Sphere \"mySphere\"", content);
        Assert.Contains("def Cube \"myCube\"", content);
        Assert.Contains("def Mesh \"myMesh\"", content);
        
        // Check attributes
        Assert.Contains("double radius = 2", content);
        Assert.Contains("double size = 1", content);
    }
    
    [Fact]
    public void UsdaWriter_ComplexAttributes_FormatsCorrectly()
    {
        using var env = new TestEnvironment();
        
        // Create stage with various attribute types
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        
        // Different value types
        prim.CreateAttribute("boolAttr", "bool").Set(true);
        prim.CreateAttribute("intAttr", "int").Set(123);
        prim.CreateAttribute("floatAttr", "float").Set(45.67f);
        prim.CreateAttribute("doubleAttr", "double").Set(89.01);
        prim.CreateAttribute("stringAttr", "string").Set("hello world");
        prim.CreateAttribute("tokenAttr", "token").Set(new TfToken("myToken"));
        
        // Vector types
        prim.CreateAttribute("vec3fAttr", "float3").Set(new GfVec3f(1.0f, 2.0f, 3.0f));
        prim.CreateAttribute("colorAttr", "color3f").Set(new GfVec3Color(0.5f, 0.5f, 0.5f));
        
        // Array types
        prim.CreateAttribute("intArrayAttr", "int[]").Set(new List<int> { 1, 2, 3, 4, 5 });
        prim.CreateAttribute("floatArrayAttr", "float[]").Set(new List<float> { 1.1f, 2.2f, 3.3f });
        prim.CreateAttribute("stringArrayAttr", "token[]").Set(new string[] { "a", "b", "c" });
        
        // Export it
        var outputPath = env.GetTempFilePath("complex.usda");
        stage.Export(outputPath);
        
        // Read and verify content
        var content = File.ReadAllText(outputPath);
        Console.WriteLine("=== Complex Attributes Output ===");
        Console.WriteLine(content);
        Console.WriteLine("================================");
        
        // Check formatting
        Assert.Contains("bool boolAttr = true", content);
        Assert.Contains("int intAttr = 123", content);
        Assert.Contains("float floatAttr = 45.67", content);
        Assert.Contains("double doubleAttr = 89.01", content);
        Assert.Contains("string stringAttr = \"hello world\"", content);
        Assert.Contains("token tokenAttr = \"myToken\"", content);
        Assert.Contains("float3 vec3fAttr = (1.0, 2.0, 3.0)", content);
        Assert.Contains("color3f colorAttr = (0.5, 0.5, 0.5)", content);
        Assert.Contains("int[] intArrayAttr = [1, 2, 3, 4, 5]", content);
        Assert.Contains("float[] floatArrayAttr = [1.1, 2.2, 3.3]", content);
        Assert.Contains("token[] stringArrayAttr = [\"a\", \"b\", \"c\"]", content);
    }
    
    [Fact]
    public void UsdaWriter_DefaultPrim_WritesMetadata()
    {
        using var env = new TestEnvironment();
        
        // Create stage with default prim
        var stage = UsdStage.CreateInMemory();
        var hero = stage.DefinePrim("/hero");
        stage.SetDefaultPrim(hero);
        
        // Export it
        var outputPath = env.GetTempFilePath("defaultPrim.usda");
        stage.Export(outputPath);
        
        // Read and verify content
        var content = File.ReadAllText(outputPath);
        Console.WriteLine("=== Default Prim Output ===");
        Console.WriteLine(content);
        Console.WriteLine("==========================");
        
        // Check for default prim metadata
        Assert.Contains("defaultPrim = \"hero\"", content);
    }
    
    [Fact]
    public void CompareSdfLayerVsUsdStageExport()
    {
        using var env = new TestEnvironment();
        
        // Test 1: SdfLayer.Export on empty layer
        var layer = SdfLayer.CreateNew("test://layer");
        layer.SetMetadata(new TfToken("customData"), new VtValue("test data"));
        
        var layerPath = env.GetTempFilePath("layer_export.usda");
        layer.Export(layerPath);
        
        var layerContent = File.ReadAllText(layerPath);
        Console.WriteLine("=== SdfLayer Export ===");
        Console.WriteLine(layerContent);
        Console.WriteLine("======================");
        
        // Test 2: UsdStage.Export on empty stage
        var stage = UsdStage.CreateInMemory();
        var stagePath = env.GetTempFilePath("stage_export.usda");
        stage.Export(stagePath);
        
        var stageContent = File.ReadAllText(stagePath);
        Console.WriteLine("=== UsdStage Export ===");
        Console.WriteLine(stageContent);
        Console.WriteLine("======================");
        
        // Both should have headers
        Assert.Contains("#usda 1.0", layerContent);
        Assert.Contains("#usda 1.0", stageContent);
        
        // Both exports should be consistent and not add default metadata
        Assert.DoesNotContain("upAxis", layerContent);
        Assert.DoesNotContain("upAxis", stageContent);
    }
}