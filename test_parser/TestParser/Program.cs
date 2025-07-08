using System;
using System.IO;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdUtils;

// Test the enhanced USDA parser with composition arcs, variant sets, and relationships
class Program
{
    static void Main()
    {
        // Create a test USDA content with advanced features
        var usdaContent = @"#usda 1.0
(
    defaultPrim = ""World""
    startTimeCode = 1
    endTimeCode = 240
)

def Xform ""World"" (
    add references = [@asset.usda@</Model>, </LocalRef>]
    add payload = [@heavy_asset.usda@</Geometry>]
    add inherits = </_class_World>
    add variantSets = [""modelingVariant"", ""shadingVariant""]
    variants = {
        string modelingVariant = ""high_poly""
        string shadingVariant = ""full""
    }
)
{
    custom string description = ""Main world root""
    uniform bool visible = true
    double3 xformOp:translate = (0, 0, 0)
    
    rel material:binding = </Materials/DefaultMaterial>
    
    def Mesh ""Cube"" (
        add specializes = </_class_Mesh>
    )
    {
        float3[] points = [(0, 0, 0), (1, 0, 0), (1, 1, 0), (0, 1, 0)]
        int[] faceVertexCounts = [4]
        int[] faceVertexIndices = [0, 1, 2, 3]
        
        float opacity.timeSamples = {
            1: 1.0,
            120: 0.5,
            240: 1.0,
        }
    }
    
    variantSet ""modelingVariant"" = {
        ""low_poly"" {
            float3[] extent = [(-0.5, -0.5, -0.5), (0.5, 0.5, 0.5)]
        }
        ""high_poly"" {
            float3[] extent = [(-1, -1, -1), (1, 1, 1)]
            uniform bool doubleSided = true
        }
    }
}

class ""_class_World"" {
    custom dictionary customData = {
        string creator: ""USD.Net Test"",
        int version: 1
    }
}
";

        Console.WriteLine("Testing Enhanced USD Parser...");
        
        // Create temporary file
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, usdaContent);
        
        try
        {
            // Test parsing
            var layer = SdfLayer.CreateNew("test");
            var parser = new UsdaParser();
            
            var success = parser.ParseFile(tempFile, layer);
            
            if (success)
            {
                Console.WriteLine("✅ Successfully parsed USDA file with advanced features!");
                
                // Check layer metadata
                var layerMetadata = layer.GetMetadata();
                Console.WriteLine($"Layer metadata entries: {layerMetadata.Count}");
                foreach (var kvp in layerMetadata)
                {
                    Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
                }
                
                // Check if we have any prim specs
                var primSpecs = layer.GetPrimSpecs();
                Console.WriteLine($"Found {primSpecs.Count} prim specs");
                
                foreach (var primSpec in primSpecs)
                {
                    Console.WriteLine($"   Prim: {primSpec.GetName()} at {primSpec.GetPath().GetString()}");
                    
                    // Check metadata
                    var metadata = primSpec.GetMetadata();
                    Console.WriteLine($"      Metadata entries: {metadata.Count}");
                    
                    // Check for composition arcs in metadata
                    foreach (var kvp in metadata)
                    {
                        Console.WriteLine($"      {kvp.Key}: {kvp.Value}");
                    }
                    
                    // Check properties
                    var properties = primSpec.GetProperties();
                    Console.WriteLine($"      Properties: {properties.Count}");
                    foreach (var prop in properties)
                    {
                        Console.WriteLine($"      - {prop.GetName()} ({prop.GetTypeName()})");
                    }
                    
                    // Check child prims
                    var children = primSpec.GetChildren();
                    Console.WriteLine($"      Child prims: {children.Count}");
                    foreach (var child in children)
                    {
                        Console.WriteLine($"      - {child.GetName()}");
                    }
                }
            }
            else
            {
                Console.WriteLine("❌ Failed to parse USDA file");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
