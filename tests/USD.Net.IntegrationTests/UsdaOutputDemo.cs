using System;
using System.IO;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace USD.Net.IntegrationTests;

[Trait("Category", "Integration")]
public class UsdaOutputDemo
{
    [Fact]
    [Trait("Category", "Integration")]
    public void GenerateUsdaDemoFile()
    {
        // Create a scene similar to the 3D model examples
        var stage = UsdStage.CreateInMemory();
        
        // Create root transform
        var rootPath = new SdfPath("/World");
        var root = UsdGeomXform.Define(stage, rootPath);
        
        // Create a sphere
        var spherePath = rootPath.AppendChild(new TfToken("Sphere"));
        var sphere = UsdGeomSphere.Define(stage, spherePath, 1.5);
        sphere.SetDisplayColor(UsdGeomGprim.Red);
        
        // Position sphere
        var sphereTranslate = sphere.AddTranslateOp();
        sphereTranslate.Set(new GfVec3f(-3, 0, 0));
        
        // Create a cube
        var cubePath = rootPath.AppendChild(new TfToken("Cube"));
        var cube = UsdGeomCube.Define(stage, cubePath, 2.0);
        cube.SetDisplayColor(UsdGeomGprim.Blue);
        cube.DoubleSided = true;
        
        // Add some animation to the cube
        var cubeRotate = cube.AddRotateXYZOp();
        for (int frame = 0; frame <= 60; frame += 10)
        {
            var time = (double)frame;
            var rotation = new GfVec3f(0, (float)(frame * 6.0), 0); // Y-axis rotation
            cubeRotate.Set(rotation, time);
        }
        
        // Export to USDA with a descriptive filename
        var filename = "demo_animated_scene.usda";
        var success = stage.Export(filename);
        
        Assert.True(success, "Demo USDA export should succeed");
        Assert.True(File.Exists(filename), "Demo USDA file should be created");
        
        // Log the content for inspection
        var content = File.ReadAllText(filename);
        Console.WriteLine($"Generated USDA file: {filename}");
        Console.WriteLine($"File size: {content.Length} characters");
        Console.WriteLine("Content preview:");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine(content.Substring(0, Math.Min(1000, content.Length)));
        if (content.Length > 1000) Console.WriteLine("... (truncated)");
        Console.WriteLine("=".PadRight(50, '='));
        
        // Don't cleanup - leave the file for inspection
    }
}