using Xunit;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdUtils;

namespace USD.Net.IntegrationTests;

[Trait("Category", "Integration")]
public class UsdaParserEnhancedTests
{
    [Fact]
    public void ParseComplexUsdaFile_WithCompositionArcs_ParsesSuccessfully()
    {
        // Create a test USDA content with advanced features
        var usdaContent = """
            #usda 1.0
            (
                defaultPrim = "World"
                startTimeCode = 1
                endTimeCode = 240
            )

            def Xform "World" (
                add references = [@asset.usda@</Model>, </LocalRef>]
                add payload = [@heavy_asset.usda@</Geometry>]
                add inherits = </_class_World>
                add variantSets = ["modelingVariant", "shadingVariant"]
                variants = {
                    string modelingVariant = "high_poly",
                    string shadingVariant = "full"
                }
            )
            {
                custom string description = "Main world root"
                uniform bool visible = true
                double3 xformOp:translate = (0, 0, 0)
                
                rel material:binding = </Materials/DefaultMaterial>
                
                def Mesh "Cube" (
                    add specializes = </_class_Mesh>
                )
                {
                    float3[] points = [(0, 0, 0), (1, 0, 0), (1, 1, 0), (0, 1, 0)]
                    int[] faceVertexCounts = [4]
                    int[] faceVertexIndices = [0, 1, 2, 3]
                }
            }

            class "_class_World" {
                custom dictionary customData = {
                    string creator: "USD.Net Test",
                    int version: 1
                }
            }
            """;

        // Create temporary file
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, usdaContent);
        
        try
        {
            // Test parsing
            var layer = SdfLayer.CreateNew("test_enhanced");
            var parser = new UsdaParser();
            
            var success = parser.ParseFile(tempFile, layer);
            
            Assert.True(success, "Parser should successfully parse complex USDA file");
            
            // Check layer metadata
            var layerMetadata = layer.GetMetadata();
            Assert.True(layerMetadata.Count > 0, "Layer should have metadata");
            
            // Check if we have prim specs
            var primSpecs = layer.GetPrimSpecs();
            Assert.True(primSpecs.Count >= 2, "Should have at least 2 prim specs (World and _class_World)");
            
            // Find the World prim
            var worldPrim = primSpecs.FirstOrDefault(p => p.GetName() == "World");
            Assert.NotNull(worldPrim);
            
            // Check that World prim has composition metadata
            var worldMetadata = worldPrim.GetMetadata();
            Assert.True(worldMetadata.Count > 0, "World prim should have metadata");
            
            // Check properties
            var properties = worldPrim.GetProperties();
            Assert.True(properties.Count > 0, "World prim should have properties");
            
            // Check child prims
            var children = worldPrim.GetChildren();
            Assert.True(children.Count > 0, "World prim should have child prims");
            
            var cubePrim = children.FirstOrDefault(c => c.GetName() == "Cube");
            Assert.NotNull(cubePrim);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ParseTimeSamples_ParsesCorrectly()
    {
        var usdaContent = @"#usda 1.0

def ""TestPrim""
{
    float opacity.timeSamples = {
        1: 1.0,
        120: 0.5,
        240: 1.0,
    }
}
";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, usdaContent);
        
        try
        {
            var layer = SdfLayer.CreateNew("test_timesamples");
            var parser = new UsdaParser();
            
            var success = parser.ParseFile(tempFile, layer);
            Assert.True(success, "Should parse time samples successfully");
            
            var primSpecs = layer.GetPrimSpecs();
            Assert.Single(primSpecs);
            
            var testPrim = primSpecs[0];
            Assert.Equal("TestPrim", testPrim.GetName());
            
            // Check if opacity property was created
            var properties = testPrim.GetProperties();
            Assert.True(properties.Count > 0, "Should have properties");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ParseVariantSets_ParsesCorrectly()
    {
        var usdaContent = @"#usda 1.0

def ""TestPrim"" (
    add variantSets = [""modelingVariant""]
    variants = {
        string modelingVariant = ""high_poly""
    }
)
{
    variantSet ""modelingVariant"" = {
        ""low_poly"" {
            float3[] extent = [(-0.5, -0.5, -0.5), (0.5, 0.5, 0.5)]
        }
        ""high_poly"" {
            float3[] extent = [(-1, -1, -1), (1, 1, 1)]
        }
    }
}
";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, usdaContent);
        
        try
        {
            var layer = SdfLayer.CreateNew("test_variants");
            var parser = new UsdaParser();
            
            var success = parser.ParseFile(tempFile, layer);
            Assert.True(success, "Should parse variant sets successfully");
            
            var primSpecs = layer.GetPrimSpecs();
            Assert.Single(primSpecs);
            
            var testPrim = primSpecs[0];
            Assert.Equal("TestPrim", testPrim.GetName());
            
            // Check if variant metadata was stored
            var metadata = testPrim.GetMetadata();
            Assert.True(metadata.Count > 0, "Should have variant metadata");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ParseRelationships_ParsesCorrectly()
    {
        var usdaContent = @"#usda 1.0

def ""TestPrim""
{
    rel material:binding = </Materials/DefaultMaterial>
    rel connections = [</Target1>, </Target2>]
}
";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, usdaContent);
        
        try
        {
            var layer = SdfLayer.CreateNew("test_relationships");
            var parser = new UsdaParser();
            
            var success = parser.ParseFile(tempFile, layer);
            Assert.True(success, "Should parse relationships successfully");
            
            var primSpecs = layer.GetPrimSpecs();
            Assert.Single(primSpecs);
            
            var testPrim = primSpecs[0];
            Assert.Equal("TestPrim", testPrim.GetName());
            
            // Check if relationship properties were created
            var properties = testPrim.GetProperties();
            Assert.True(properties.Count > 0, "Should have relationship properties");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}