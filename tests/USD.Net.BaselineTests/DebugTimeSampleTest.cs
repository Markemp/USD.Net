using USD.Net.BaselineTests.TestUtils;
using Pxr.Usd;
using Pxr.Base.Vt;

namespace USD.Net.BaselineTests;

[Trait("Category", "Debug")]
public class DebugTimeSampleTest
{
    [Fact]
    public void DebugAttributeSetBehavior()
    {
        using var env = new TestEnvironment();
        
        // Create a simple stage and attribute
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/hello");
        var attr = prim.CreateAttribute("myString", "string");
        
        // Set the value
        attr.Set("world");
        
        // Debug the state
        Console.WriteLine($"=== Debug Attribute State ===");
        Console.WriteLine($"Attribute name: {attr.GetName()}");
        Console.WriteLine($"Attribute type: {attr.GetTypeName()}");
        
        // Check default value
        var hasDefault = attr.Get(out VtValue defaultValue);
        Console.WriteLine($"Has default value: {hasDefault}");
        Console.WriteLine($"Default value: {(hasDefault ? defaultValue.ToString() : "NONE")}");
        
        // Check time samples
        var timeSamples = attr.GetTimeSamples();
        Console.WriteLine($"Number of explicit time samples: {timeSamples.Length}");
        Console.WriteLine($"Time sample times: [{string.Join(", ", timeSamples)}]");
        
        // Check total time samples
        var numTimeSamples = attr.GetNumTimeSamples();
        Console.WriteLine($"Total time samples (including default): {numTimeSamples}");
        
        // Test with explicit time
        attr.Set("explicit_value", UsdTimeCode.Create(1.0));
        
        var timeSamples2 = attr.GetTimeSamples();
        Console.WriteLine($"After explicit time sample:");
        Console.WriteLine($"Number of explicit time samples: {timeSamples2.Length}");
        Console.WriteLine($"Time sample times: [{string.Join(", ", timeSamples2)}]");
        
        var numTimeSamples2 = attr.GetNumTimeSamples();
        Console.WriteLine($"Total time samples (including default): {numTimeSamples2}");
        
        // Export and check
        var outputPath = env.GetTempFilePath("debug.usda");
        stage.Export(outputPath);
        
        var content = File.ReadAllText(outputPath);
        Console.WriteLine($"=== Generated USDA ===");
        Console.WriteLine(content);
        Console.WriteLine($"======================");
    }
}