using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace USD.Net.Examples;

/// <summary>
/// Comprehensive examples of 3D model creation using USD.Net geometry system.
/// These examples demonstrate how to create, transform, and organize 3D models
/// using the USD geometry framework.
/// </summary>
public static class ThreeDModelCreationExamples
{
    /// <summary>
    /// Example 1: Create a simple scene with basic primitives
    /// </summary>
    public static UsdStage CreateBasicPrimitivesScene()
    {
        Console.WriteLine("Creating basic primitives scene...");
        
        var stage = UsdStage.CreateInMemory();
        
        // Create root transform
        var rootPath = new SdfPath("/World");
        var root = UsdGeomXform.Define(stage, rootPath);
        
        // Create a sphere
        var spherePath = rootPath.AppendChild(new TfToken("Sphere"));
        var sphere = UsdGeomSphere.Define(stage, spherePath, 1.0);
        sphere.SetDisplayColor(UsdGeomGprim.Red);
        sphere.SetDisplayOpacity(0.8f);
        
        // Position sphere
        var sphereTranslate = sphere.AddTranslateOp();
        sphereTranslate.Set(new GfVec3f(-3, 0, 0));
        
        // Create a cube
        var cubePath = rootPath.AppendChild(new TfToken("Cube"));
        var cube = UsdGeomCube.Define(stage, cubePath, 2.0);
        cube.SetDisplayColor(UsdGeomGprim.Blue);
        cube.DoubleSided = true;
        
        // No translation for cube (at origin)
        
        // Create a custom mesh (pyramid)
        var pyramidPath = rootPath.AppendChild(new TfToken("Pyramid"));
        var pyramid = UsdGeomMesh.Define(stage, pyramidPath);
        CreatePyramidMesh(pyramid);
        pyramid.SetDisplayColor(UsdGeomGprim.Green);
        
        // Position pyramid
        var pyramidTranslate = pyramid.AddTranslateOp();
        pyramidTranslate.Set(new GfVec3f(3, 0, 0));
        
        Console.WriteLine("Basic primitives scene created successfully!");
        return stage;
    }
    
    /// <summary>
    /// Example 2: Create a complex hierarchical scene with transforms
    /// </summary>
    public static UsdStage CreateHierarchicalScene()
    {
        Console.WriteLine("Creating hierarchical scene...");
        
        var stage = UsdStage.CreateInMemory();
        
        // Create scene root
        var scenePath = new SdfPath("/Scene");
        var scene = UsdGeomXform.Define(stage, scenePath);
        
        // Add overall scene transform
        var sceneTranslate = scene.AddTranslateOp();
        sceneTranslate.Set(new GfVec3f(0, 0, 10));
        
        // Create solar system model
        CreateSolarSystemModel(stage, scenePath);
        
        // Create building model
        CreateSimpleBuildingModel(stage, scenePath);
        
        Console.WriteLine("Hierarchical scene created successfully!");
        return stage;
    }
    
    /// <summary>
    /// Example 3: Create animated 3D model with time-varying attributes
    /// </summary>
    public static UsdStage CreateAnimatedScene()
    {
        Console.WriteLine("Creating animated scene...");
        
        var stage = UsdStage.CreateInMemory();
        
        // Create root
        var rootPath = new SdfPath("/Animation");
        var root = UsdGeomXform.Define(stage, rootPath);
        
        // Create animated sphere
        var spherePath = rootPath.AppendChild(new TfToken("AnimatedSphere"));
        var sphere = UsdGeomSphere.Define(stage, spherePath);
        
        // Animate radius over time
        for (int frame = 0; frame <= 100; frame += 10)
        {
            var time = (double)frame;
            var radius = 1.0 + 0.5 * Math.Sin(time * 0.1);
            sphere.SetRadius(radius, time);
        }
        
        // Animate position
        var translateOp = sphere.AddTranslateOp();
        for (int frame = 0; frame <= 100; frame += 10)
        {
            var time = (double)frame;
            var x = (float)(3.0 * Math.Cos(time * 0.05));
            var y = (float)(2.0 * Math.Sin(time * 0.08));
            translateOp.Set(new GfVec3f(x, y, 0), time);
        }
        
        // Create orbiting cube
        var cubeOrbitPath = rootPath.AppendChild(new TfToken("CubeOrbit"));
        var cubeOrbit = UsdGeomXform.Define(stage, cubeOrbitPath);
        
        var orbitRotate = cubeOrbit.AddRotateZOp();
        for (int frame = 0; frame <= 100; frame += 5)
        {
            var time = (double)frame;
            var rotation = (float)(frame * 3.6); // Full rotation every 100 frames
            orbitRotate.Set(rotation, time);
        }
        
        var cubePath = cubeOrbitPath.AppendChild(new TfToken("Cube"));
        var cube = UsdGeomCube.Define(stage, cubePath, 0.5);
        var cubeTranslate = cube.AddTranslateOp();
        cubeTranslate.Set(new GfVec3f(5, 0, 0));
        
        Console.WriteLine("Animated scene created successfully!");
        return stage;
    }
    
    /// <summary>
    /// Example 4: Create a procedural city model
    /// </summary>
    public static UsdStage CreateProceduralCity()
    {
        Console.WriteLine("Creating procedural city...");
        
        var stage = UsdStage.CreateInMemory();
        var random = new Random(42); // Fixed seed for reproducibility
        
        // Create city root
        var cityPath = new SdfPath("/City");
        var city = UsdGeomXform.Define(stage, cityPath);
        
        // Create ground plane
        var groundPath = cityPath.AppendChild(new TfToken("Ground"));
        var ground = UsdGeomMesh.Define(stage, groundPath);
        ground.CreatePlane(50.0f, 50.0f, 10, 10);
        ground.SetDisplayColor(new GfVec3Color(0.3f, 0.5f, 0.3f)); // Green ground
        
        // Create buildings in a grid
        for (int x = -4; x <= 4; x++)
        {
            for (int z = -4; z <= 4; z++)
            {
                // Skip center area
                if (Math.Abs(x) <= 1 && Math.Abs(z) <= 1) continue;
                
                var buildingName = $"Building_{x}_{z}";
                var buildingPath = cityPath.AppendChild(new TfToken(buildingName));
                
                // Random building height and size
                var height = 2.0 + random.NextDouble() * 8.0;
                var width = 1.0 + random.NextDouble() * 2.0;
                var depth = 1.0 + random.NextDouble() * 2.0;
                
                CreateBuilding(stage, buildingPath, width, height, depth, x * 6.0, z * 6.0);
            }
        }
        
        // Add some decorative elements
        CreateCityDecorations(stage, cityPath, random);
        
        Console.WriteLine("Procedural city created successfully!");
        return stage;
    }
    
    /// <summary>
    /// Example 5: Create a complex mesh model (spaceship)
    /// </summary>
    public static UsdStage CreateSpaceshipModel()
    {
        Console.WriteLine("Creating spaceship model...");
        
        var stage = UsdStage.CreateInMemory();
        
        // Create spaceship root
        var shipPath = new SdfPath("/Spaceship");
        var ship = UsdGeomXform.Define(stage, shipPath);
        
        // Main hull (elongated cube)
        var hullPath = shipPath.AppendChild(new TfToken("Hull"));
        var hull = UsdGeomMesh.Define(stage, hullPath);
        CreateSpaceshipHull(hull);
        hull.SetDisplayColor(new GfVec3Color(0.7f, 0.7f, 0.8f)); // Light gray
        
        // Cockpit (sphere)
        var cockpitPath = shipPath.AppendChild(new TfToken("Cockpit"));
        var cockpit = UsdGeomSphere.Define(stage, cockpitPath, 0.8);
        cockpit.SetDisplayColor(new GfVec3Color(0.2f, 0.4f, 0.8f)); // Blue
        var cockpitTranslate = cockpit.AddTranslateOp();
        cockpitTranslate.Set(new GfVec3f(2, 0.5f, 0));
        
        // Wings
        CreateSpaceshipWings(stage, shipPath);
        
        // Engines
        CreateSpaceshipEngines(stage, shipPath);
        
        Console.WriteLine("Spaceship model created successfully!");
        return stage;
    }
    
    #region Helper Methods
    
    private static void CreatePyramidMesh(UsdGeomMesh mesh)
    {
        // Create a 4-sided pyramid
        var points = new List<GfVec3f>
        {
            // Base vertices
            new GfVec3f(-1, 0, -1),
            new GfVec3f( 1, 0, -1),
            new GfVec3f( 1, 0,  1),
            new GfVec3f(-1, 0,  1),
            // Apex
            new GfVec3f( 0, 2,  0)
        };
        
        var faceVertexIndices = new List<int>
        {
            // Base (quad)
            0, 1, 2, 3,
            // Side faces (triangles)
            0, 4, 1,
            1, 4, 2,
            2, 4, 3,
            3, 4, 0
        };
        
        var faceVertexCounts = new List<int> { 4, 3, 3, 3, 3 };
        
        mesh.Points = points;
        mesh.FaceVertexIndices = faceVertexIndices;
        mesh.FaceVertexCounts = faceVertexCounts;
    }
    
    private static void CreateSolarSystemModel(UsdStage stage, SdfPath parentPath)
    {
        var solarSystemPath = parentPath.AppendChild(new TfToken("SolarSystem"));
        var solarSystem = UsdGeomXform.Define(stage, solarSystemPath);
        
        // Position solar system
        var solarTranslate = solarSystem.AddTranslateOp();
        solarTranslate.Set(new GfVec3f(-10, 5, 0));
        
        // Sun
        var sunPath = solarSystemPath.AppendChild(new TfToken("Sun"));
        var sun = UsdGeomSphere.Define(stage, sunPath, 1.5);
        sun.SetDisplayColor(new GfVec3Color(1.0f, 1.0f, 0.3f)); // Yellow
        
        // Earth orbit
        var earthOrbitPath = solarSystemPath.AppendChild(new TfToken("EarthOrbit"));
        var earthOrbit = UsdGeomXform.Define(stage, earthOrbitPath);
        
        var earthPath = earthOrbitPath.AppendChild(new TfToken("Earth"));
        var earth = UsdGeomSphere.Define(stage, earthPath, 0.6);
        earth.SetDisplayColor(new GfVec3Color(0.3f, 0.5f, 1.0f)); // Blue
        var earthTranslate = earth.AddTranslateOp();
        earthTranslate.Set(new GfVec3f(4, 0, 0));
        
        // Moon orbit around Earth
        var moonOrbitPath = earthPath.AppendChild(new TfToken("MoonOrbit"));
        var moonOrbit = UsdGeomXform.Define(stage, moonOrbitPath);
        
        var moonPath = moonOrbitPath.AppendChild(new TfToken("Moon"));
        var moon = UsdGeomSphere.Define(stage, moonPath, 0.2);
        moon.SetDisplayColor(new GfVec3Color(0.8f, 0.8f, 0.7f)); // Gray
        var moonTranslate = moon.AddTranslateOp();
        moonTranslate.Set(new GfVec3f(1.2f, 0, 0));
    }
    
    private static void CreateSimpleBuildingModel(UsdStage stage, SdfPath parentPath)
    {
        var buildingPath = parentPath.AppendChild(new TfToken("Building"));
        var building = UsdGeomXform.Define(stage, buildingPath);
        
        // Position building
        var buildingTranslate = building.AddTranslateOp();
        buildingTranslate.Set(new GfVec3f(10, 0, 0));
        
        // Main structure
        var mainPath = buildingPath.AppendChild(new TfToken("MainStructure"));
        var main = UsdGeomCube.Define(stage, mainPath, 3.0);
        main.SetDisplayColor(new GfVec3Color(0.6f, 0.6f, 0.5f)); // Beige
        var mainTranslate = main.AddTranslateOp();
        mainTranslate.Set(new GfVec3f(0, 1.5f, 0)); // Lift up so base is on ground
        
        // Roof
        var roofPath = buildingPath.AppendChild(new TfToken("Roof"));
        var roof = UsdGeomMesh.Define(stage, roofPath);
        CreateRoofMesh(roof);
        roof.SetDisplayColor(new GfVec3Color(0.8f, 0.3f, 0.2f)); // Red roof
        var roofTranslate = roof.AddTranslateOp();
        roofTranslate.Set(new GfVec3f(0, 3.5f, 0));
    }
    
    private static void CreateRoofMesh(UsdGeomMesh mesh)
    {
        // Simple peaked roof
        var points = new List<GfVec3f>
        {
            // Base corners (slightly larger than building)
            new GfVec3f(-1.6f, 0, -1.6f),
            new GfVec3f( 1.6f, 0, -1.6f),
            new GfVec3f( 1.6f, 0,  1.6f),
            new GfVec3f(-1.6f, 0,  1.6f),
            // Ridge line
            new GfVec3f(-1.6f, 1.2f, 0),
            new GfVec3f( 1.6f, 1.2f, 0)
        };
        
        var faceVertexIndices = new List<int>
        {
            // Front slope
            0, 1, 5, 4,
            // Back slope  
            2, 3, 4, 5,
            // Left end
            3, 0, 4,
            // Right end
            1, 2, 5
        };
        
        var faceVertexCounts = new List<int> { 4, 4, 3, 3 };
        
        mesh.Points = points;
        mesh.FaceVertexIndices = faceVertexIndices;
        mesh.FaceVertexCounts = faceVertexCounts;
    }
    
    private static void CreateBuilding(UsdStage stage, SdfPath buildingPath, double width, double height, double depth, double x, double z)
    {
        var building = UsdGeomCube.Define(stage, buildingPath, 1.0);
        
        // Scale to desired proportions
        var scaleOp = building.AddScaleOp();
        scaleOp.Set(new GfVec3f((float)width, (float)height, (float)depth));
        
        // Position
        var translateOp = building.AddTranslateOp();
        translateOp.Set(new GfVec3f((float)x, (float)(height * 0.5), (float)z));
        
        // Random color variation
        var gray = 0.4f + (float)(new Random((int)(x * 13 + z * 17)).NextDouble() * 0.3);
        building.SetDisplayColor(new GfVec3Color(gray, gray, gray + 0.1f));
    }
    
    private static void CreateCityDecorations(UsdStage stage, SdfPath cityPath, Random random)
    {
        // Add some trees (green spheres on brown cylinders)
        for (int i = 0; i < 10; i++)
        {
            var treePath = cityPath.AppendChild(new TfToken($"Tree_{i}"));
            var tree = UsdGeomXform.Define(stage, treePath);
            
            var x = (random.NextDouble() - 0.5) * 20;
            var z = (random.NextDouble() - 0.5) * 20;
            
            var treeTranslate = tree.AddTranslateOp();
            treeTranslate.Set(new GfVec3f((float)x, 0, (float)z));
            
            // Trunk (cube scaled to cylinder-like)
            var trunkPath = treePath.AppendChild(new TfToken("Trunk"));
            var trunk = UsdGeomCube.Define(stage, trunkPath, 1.0);
            var trunkScale = trunk.AddScaleOp();
            trunkScale.Set(new GfVec3f(0.2f, 2.0f, 0.2f));
            trunk.SetDisplayColor(new GfVec3Color(0.4f, 0.2f, 0.1f)); // Brown
            var trunkTranslateOp = trunk.AddTranslateOp();
            trunkTranslateOp.Set(new GfVec3f(0, 1, 0));
            
            // Foliage (green sphere)
            var foliagePath = treePath.AppendChild(new TfToken("Foliage"));
            var foliage = UsdGeomSphere.Define(stage, foliagePath, 1.5);
            foliage.SetDisplayColor(new GfVec3Color(0.2f, 0.7f, 0.2f)); // Green
            var foliageTranslate = foliage.AddTranslateOp();
            foliageTranslate.Set(new GfVec3f(0, 2.5f, 0));
        }
    }
    
    private static void CreateSpaceshipHull(UsdGeomMesh mesh)
    {
        // Create an elongated, tapered hull
        var points = new List<GfVec3f>
        {
            // Front section (narrow)
            new GfVec3f( 3, -0.2f, -0.3f), new GfVec3f( 3,  0.2f, -0.3f),
            new GfVec3f( 3,  0.2f,  0.3f), new GfVec3f( 3, -0.2f,  0.3f),
            
            // Middle section (wide)
            new GfVec3f( 0, -0.5f, -0.8f), new GfVec3f( 0,  0.5f, -0.8f),
            new GfVec3f( 0,  0.5f,  0.8f), new GfVec3f( 0, -0.5f,  0.8f),
            
            // Back section (narrow)
            new GfVec3f(-2, -0.3f, -0.4f), new GfVec3f(-2,  0.3f, -0.4f),
            new GfVec3f(-2,  0.3f,  0.4f), new GfVec3f(-2, -0.3f,  0.4f)
        };
        
        var faceVertexIndices = new List<int>();
        var faceVertexCounts = new List<int>();
        
        // Create faces connecting the sections
        // Front to middle (4 faces)
        AddQuadFace(faceVertexIndices, faceVertexCounts, 0, 1, 5, 4); // Top
        AddQuadFace(faceVertexIndices, faceVertexCounts, 1, 2, 6, 5); // Right
        AddQuadFace(faceVertexIndices, faceVertexCounts, 2, 3, 7, 6); // Bottom
        AddQuadFace(faceVertexIndices, faceVertexCounts, 3, 0, 4, 7); // Left
        
        // Middle to back (4 faces)
        AddQuadFace(faceVertexIndices, faceVertexCounts, 4, 5, 9, 8);   // Top
        AddQuadFace(faceVertexIndices, faceVertexCounts, 5, 6, 10, 9);  // Right
        AddQuadFace(faceVertexIndices, faceVertexCounts, 6, 7, 11, 10); // Bottom
        AddQuadFace(faceVertexIndices, faceVertexCounts, 7, 4, 8, 11);  // Left
        
        mesh.Points = points;
        mesh.FaceVertexIndices = faceVertexIndices;
        mesh.FaceVertexCounts = faceVertexCounts;
    }
    
    private static void CreateSpaceshipWings(UsdStage stage, SdfPath shipPath)
    {
        // Left wing
        var leftWingPath = shipPath.AppendChild(new TfToken("LeftWing"));
        var leftWing = UsdGeomMesh.Define(stage, leftWingPath);
        leftWing.CreatePlane(1.5f, 3.0f);
        leftWing.SetDisplayColor(new GfVec3Color(0.5f, 0.5f, 0.6f));
        var leftWingTransform = leftWing.AddTransformOp();
        var leftWingMatrix = GfMatrix4d.CreateTranslation(new GfVec3f(0, 0, -1.5f)) *
                           GfMatrix4d.CreateRotation(0, 0, (float)(Math.PI / 2));
        leftWingTransform.Set(leftWingMatrix);
        
        // Right wing
        var rightWingPath = shipPath.AppendChild(new TfToken("RightWing"));
        var rightWing = UsdGeomMesh.Define(stage, rightWingPath);
        rightWing.CreatePlane(1.5f, 3.0f);
        rightWing.SetDisplayColor(new GfVec3Color(0.5f, 0.5f, 0.6f));
        var rightWingTransform = rightWing.AddTransformOp();
        var rightWingMatrix = GfMatrix4d.CreateTranslation(new GfVec3f(0, 0, 1.5f)) *
                            GfMatrix4d.CreateRotation(0, 0, (float)(Math.PI / 2));
        rightWingTransform.Set(rightWingMatrix);
    }
    
    private static void CreateSpaceshipEngines(UsdStage stage, SdfPath shipPath)
    {
        // Left engine
        var leftEnginePath = shipPath.AppendChild(new TfToken("LeftEngine"));
        var leftEngine = UsdGeomCube.Define(stage, leftEnginePath, 0.6);
        leftEngine.SetDisplayColor(new GfVec3Color(0.3f, 0.3f, 0.4f)); // Dark gray
        var leftEngineTranslate = leftEngine.AddTranslateOp();
        leftEngineTranslate.Set(new GfVec3f(-2.5f, 0, -1.0f));
        
        // Right engine  
        var rightEnginePath = shipPath.AppendChild(new TfToken("RightEngine"));
        var rightEngine = UsdGeomCube.Define(stage, rightEnginePath, 0.6);
        rightEngine.SetDisplayColor(new GfVec3Color(0.3f, 0.3f, 0.4f)); // Dark gray
        var rightEngineTranslate = rightEngine.AddTranslateOp();
        rightEngineTranslate.Set(new GfVec3f(-2.5f, 0, 1.0f));
    }
    
    private static void AddQuadFace(List<int> indices, List<int> counts, int a, int b, int c, int d)
    {
        indices.AddRange(new[] { a, b, c, d });
        counts.Add(4);
    }
    
    #endregion
    
    /// <summary>
    /// Utility method to save a stage to a USD file
    /// </summary>
    public static void SaveStageToFile(UsdStage stage, string filePath)
    {
        try
        {
            Console.WriteLine($"Saving stage to: {filePath}");
            
            var success = stage.Export(filePath);
            if (success)
            {
                Console.WriteLine($"✅ Successfully saved USD file: {filePath}");
                
                // Show file size
                var fileInfo = new System.IO.FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    Console.WriteLine($"   File size: {fileInfo.Length:N0} bytes");
                }
            }
            else
            {
                Console.WriteLine($"❌ Failed to save USD file: {filePath}");
            }
            
            // Print stage contents for debugging
            Console.WriteLine("\nStage contents:");
            PrintStageHierarchy(stage, stage.GetPseudoRoot(), 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving stage: {ex.Message}");
        }
    }
    
    private static void PrintStageHierarchy(UsdStage stage, UsdPrim prim, int depth)
    {
        var indent = new string("  ", depth);
        Console.WriteLine($"{indent}{prim.GetPath()} ({prim.GetTypeName()})");
        
        foreach (var child in prim.GetChildren())
        {
            PrintStageHierarchy(stage, child, depth + 1);
        }
    }
}

/// <summary>
/// Main program to run the 3D model creation examples
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("USD.Net 3D Model Creation Examples");
        Console.WriteLine("==================================\n");
        
        try
        {
            // Example 1: Basic primitives
            var basicScene = ThreeDModelCreationExamples.CreateBasicPrimitivesScene();
            ThreeDModelCreationExamples.SaveStageToFile(basicScene, "basic_primitives.usda");
            
            Console.WriteLine();
            
            // Example 2: Hierarchical scene
            var hierarchicalScene = ThreeDModelCreationExamples.CreateHierarchicalScene();
            ThreeDModelCreationExamples.SaveStageToFile(hierarchicalScene, "hierarchical_scene.usda");
            
            Console.WriteLine();
            
            // Example 3: Animated scene
            var animatedScene = ThreeDModelCreationExamples.CreateAnimatedScene();
            ThreeDModelCreationExamples.SaveStageToFile(animatedScene, "animated_scene.usda");
            
            Console.WriteLine();
            
            // Example 4: Procedural city
            var cityScene = ThreeDModelCreationExamples.CreateProceduralCity();
            ThreeDModelCreationExamples.SaveStageToFile(cityScene, "procedural_city.usda");
            
            Console.WriteLine();
            
            // Example 5: Complex spaceship model
            var spaceshipScene = ThreeDModelCreationExamples.CreateSpaceshipModel();
            ThreeDModelCreationExamples.SaveStageToFile(spaceshipScene, "spaceship_model.usda");
            
            Console.WriteLine("\nAll examples completed successfully!");
            Console.WriteLine("\nThese examples demonstrate:");
            Console.WriteLine("• Creating basic primitives (spheres, cubes, meshes)");
            Console.WriteLine("• Hierarchical scene organization");
            Console.WriteLine("• Transform operations and positioning");
            Console.WriteLine("• Time-varying animation");
            Console.WriteLine("• Procedural model generation");
            Console.WriteLine("• Complex mesh creation");
            Console.WriteLine("• Display properties and materials");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running examples: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}