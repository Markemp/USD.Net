using System;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Pxr.Usd.UsdUtils;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing USDA Export...");
        
        // Create a simple stage with a sphere
        var stage = UsdStage.CreateInMemory();
        var spherePath = new SdfPath("/Sphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath, 2.0);
        sphere.SetDisplayColor(UsdGeomGprim.Red);
        
        // Test the USDA export
        var success = stage.Export("test_sphere.usda");
        
        if (success)
        {
            Console.WriteLine("✅ USDA export successful!");
            
            // Check if file was created and show some content
            if (System.IO.File.Exists("test_sphere.usda"))
            {
                var content = System.IO.File.ReadAllText("test_sphere.usda");
                Console.WriteLine("File content preview:");
                Console.WriteLine(content.Substring(0, Math.Min(500, content.Length)));
                if (content.Length > 500) Console.WriteLine("...");
            }
        }
        else
        {
            Console.WriteLine("❌ USDA export failed!");
        }
    }
}