using System;
using System.IO;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace USD.Net.Tests;

public class UsdaExportTests
{
    [Fact]
    public void TestBasicUsdaExport()
    {
        // Create a simple stage with a sphere
        var stage = UsdStage.CreateInMemory();
        var spherePath = new SdfPath("/Sphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath, 2.0);
        sphere.SetDisplayColor(UsdGeomGprim.Red);
        
        // Test the USDA export
        var filename = "test_sphere.usda";
        var success = stage.Export(filename);
        
        Assert.True(success, "USDA export should succeed");
        Assert.True(File.Exists(filename), "USDA file should be created");
        
        // Read and verify content
        var content = File.ReadAllText(filename);
        Assert.Contains("#usda 1.0", content);
        Assert.Contains("def Sphere \"Sphere\"", content);
        Assert.Contains("double radius = 2", content);
        
        // Cleanup
        if (File.Exists(filename))
            File.Delete(filename);
    }
    
    [Fact]
    public void TestComplexSceneExport()
    {
        // Create a hierarchical scene
        var stage = UsdStage.CreateInMemory();
        
        // Root transform
        var rootPath = new SdfPath("/World");
        var root = UsdGeomXform.Define(stage, rootPath);
        
        // Add a sphere
        var spherePath = rootPath.AppendChild(new TfToken("Sphere"));
        var sphere = UsdGeomSphere.Define(stage, spherePath, 1.0);
        sphere.SetDisplayColor(UsdGeomGprim.Blue);
        
        // Add translation
        var translateOp = sphere.AddTranslateOp();
        translateOp.Set(new GfVec3f(2, 0, 0));
        
        // Export to USDA
        var filename = "test_complex_scene.usda";
        var success = stage.Export(filename);
        
        Assert.True(success, "Complex scene USDA export should succeed");
        Assert.True(File.Exists(filename), "Complex scene USDA file should be created");
        
        // Read and verify content
        var content = File.ReadAllText(filename);
        Assert.Contains("#usda 1.0", content);
        Assert.Contains("def Xform \"World\"", content);
        Assert.Contains("def Sphere \"Sphere\"", content);
        Assert.Contains("xformOp:translate", content);
        
        // Cleanup
        if (File.Exists(filename))
            File.Delete(filename);
    }
}