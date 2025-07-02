using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace USD.Net.Tests;

public class UsdGeomTests
{
    private UsdStage CreateTestStage()
    {
        return UsdStage.CreateInMemory();
    }

    #region UsdGeomImageable Tests

    [Fact]
    public void UsdGeomImageable_VisibilityManagement_Works()
    {
        var stage = CreateTestStage();
        var spherePath = new SdfPath("/TestSphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath);

        // Test default visibility
        Assert.Equal(UsdGeomVisibility.Inherited, sphere.Visibility);
        Assert.True(sphere.IsVisible());

        // Test setting visibility to invisible
        sphere.MakeInvisible();
        Assert.Equal(UsdGeomVisibility.Invisible, sphere.Visibility);
        Assert.False(sphere.IsVisible());

        // Test making visible again
        sphere.MakeVisible();
        Assert.Equal(UsdGeomVisibility.Inherited, sphere.Visibility);
        Assert.True(sphere.IsVisible());
    }

    [Fact]
    public void UsdGeomImageable_PurposeManagement_Works()
    {
        var stage = CreateTestStage();
        var cubePath = new SdfPath("/TestCube");
        var cube = UsdGeomCube.Define(stage, cubePath);

        // Test default purpose
        Assert.Equal(UsdGeomPurpose.Default, cube.Purpose);

        // Test setting different purposes
        cube.Purpose = UsdGeomPurpose.Render;
        Assert.Equal(UsdGeomPurpose.Render, cube.Purpose);

        cube.Purpose = UsdGeomPurpose.Proxy;
        Assert.Equal(UsdGeomPurpose.Proxy, cube.Purpose);

        cube.Purpose = UsdGeomPurpose.Guide;
        Assert.Equal(UsdGeomPurpose.Guide, cube.Purpose);
    }

    #endregion

    #region UsdGeomXformable Tests

    [Fact]
    public void UsdGeomXformable_TransformOperations_Work()
    {
        var stage = CreateTestStage();
        var xformPath = new SdfPath("/TestXform");
        var xform = UsdGeomXform.Define(stage, xformPath);

        // Add transform operations
        var translateOp = xform.AddTranslateOp();
        var rotateOp = xform.AddRotateXYZOp();
        var scaleOp = xform.AddScaleOp();

        // Set values
        translateOp.Set(new GfVec3f(1, 2, 3));
        rotateOp.Set(new GfVec3f(0, 45, 0)); // 45 degrees Y rotation
        scaleOp.Set(new GfVec3f(2, 2, 2));

        // Get ordered operations
        var ops = xform.GetOrderedXformOps(out var resetsXformStack);
        Assert.False(resetsXformStack);
        Assert.Equal(3, ops.Count);

        // Compute local transformation
        var success = xform.GetLocalTransformation(out var transform, out var resets);
        Assert.True(success);
        Assert.False(resets);
        Assert.NotEqual(GfMatrix4d.Identity, transform);
    }

    [Fact]
    public void UsdGeomXformable_ResetXformStack_Works()
    {
        var stage = CreateTestStage();
        var xformPath = new SdfPath("/TestXform");
        var xform = UsdGeomXform.Define(stage, xformPath);

        // Test reset xform stack
        Assert.False(xform.GetResetXformStack());
        
        xform.SetResetXformStack(true);
        Assert.True(xform.GetResetXformStack());
        
        xform.SetResetXformStack(false);
        Assert.False(xform.GetResetXformStack());
    }

    #endregion

    #region UsdGeomGprim Tests

    [Fact]
    public void UsdGeomGprim_DisplayProperties_Work()
    {
        var stage = CreateTestStage();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Test display color
        var red = new GfVec3Color(1, 0, 0);
        mesh.SetDisplayColor(red);
        var colors = mesh.DisplayColor;
        Assert.Single(colors);
        Assert.Equal(red, colors[0]);

        // Test display opacity
        mesh.SetDisplayOpacity(0.5f);
        var opacities = mesh.DisplayOpacity;
        Assert.Single(opacities);
        Assert.Equal(0.5f, opacities[0]);

        // Test orientation
        Assert.True(mesh.IsRightHanded);
        mesh.Orientation = "leftHanded";
        Assert.True(mesh.IsLeftHanded);

        // Test double sided
        Assert.False(mesh.DoubleSided);
        mesh.DoubleSided = true;
        Assert.True(mesh.DoubleSided);
    }

    #endregion

    #region UsdGeomMesh Tests

    [Fact]
    public void UsdGeomMesh_BasicCreation_Works()
    {
        var stage = CreateTestStage();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Create a simple triangle
        var p0 = new GfVec3f(0, 0, 0);
        var p1 = new GfVec3f(1, 0, 0);
        var p2 = new GfVec3f(0.5f, 1, 0);
        mesh.CreateTriangle(p0, p1, p2);

        // Verify mesh data
        var points = mesh.Points;
        Assert.Equal(3, points.Count);
        Assert.Equal(p0, points[0]);
        Assert.Equal(p1, points[1]);
        Assert.Equal(p2, points[2]);

        var indices = mesh.FaceVertexIndices;
        Assert.Equal(3, indices.Count);
        Assert.Equal(new[] { 0, 1, 2 }, indices);

        var counts = mesh.FaceVertexCounts;
        Assert.Single(counts);
        Assert.Equal(3, counts[0]);

        // Verify mesh statistics
        var stats = mesh.GetStatistics();
        Assert.Equal(3, stats.VertexCount);
        Assert.Equal(1, stats.FaceCount);
        Assert.Equal(1, stats.TriangleCount);
        Assert.Equal(0, stats.QuadCount);
        Assert.Equal(0, stats.NgonCount);
    }

    [Fact]
    public void UsdGeomMesh_CubeCreation_Works()
    {
        var stage = CreateTestStage();
        var meshPath = new SdfPath("/CubeMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        mesh.CreateCube(2.0f);

        // Verify cube mesh
        var points = mesh.Points;
        Assert.Equal(8, points.Count); // 8 vertices

        var counts = mesh.FaceVertexCounts;
        Assert.Equal(6, counts.Count); // 6 faces
        Assert.All(counts, count => Assert.Equal(4, count)); // All quads

        var indices = mesh.FaceVertexIndices;
        Assert.Equal(24, indices.Count); // 6 faces * 4 vertices each

        // Verify it's quad-dominant
        Assert.True(mesh.IsQuadDominant());
        Assert.False(mesh.IsTriangulated());

        // Verify statistics
        var stats = mesh.GetStatistics();
        Assert.Equal(8, stats.VertexCount);
        Assert.Equal(6, stats.FaceCount);
        Assert.Equal(0, stats.TriangleCount);
        Assert.Equal(6, stats.QuadCount);
        Assert.Equal(0, stats.NgonCount);
    }

    [Fact]
    public void UsdGeomMesh_TopologyValidation_Works()
    {
        var stage = CreateTestStage();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Create invalid mesh (empty)
        var (isValid, reason) = mesh.ValidateTopology();
        Assert.False(isValid);
        Assert.Contains("no points", reason);

        // Create valid triangle
        mesh.CreateTriangle(
            new GfVec3f(0, 0, 0),
            new GfVec3f(1, 0, 0),
            new GfVec3f(0, 1, 0)
        );

        (isValid, reason) = mesh.ValidateTopology();
        Assert.True(isValid);
        Assert.Empty(reason);
    }

    [Fact]
    public void UsdGeomMesh_ExtentComputation_Works()
    {
        var stage = CreateTestStage();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Create a cube mesh
        mesh.CreateCube(2.0f);

        // Compute extent
        var success = mesh.ComputeExtent(UsdTimeCode.Default(), out var extent);
        Assert.True(success);
        Assert.Equal(2, extent.Count);

        var min = extent[0];
        var max = extent[1];
        Assert.Equal(-1.0f, min.X, 1e-6f);
        Assert.Equal(-1.0f, min.Y, 1e-6f);
        Assert.Equal(-1.0f, min.Z, 1e-6f);
        Assert.Equal(1.0f, max.X, 1e-6f);
        Assert.Equal(1.0f, max.Y, 1e-6f);
        Assert.Equal(1.0f, max.Z, 1e-6f);
    }

    #endregion

    #region UsdGeomSphere Tests

    [Fact]
    public void UsdGeomSphere_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var spherePath = new SdfPath("/TestSphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath);

        // Test default radius
        Assert.Equal(1.0, sphere.Radius);

        // Test setting radius
        sphere.Radius = 2.5;
        Assert.Equal(2.5, sphere.Radius);

        // Test geometric properties
        var volume = sphere.GetVolume();
        var expectedVolume = (4.0 / 3.0) * Math.PI * Math.Pow(2.5, 3);
        Assert.Equal(expectedVolume, volume, 1e-10);

        var surfaceArea = sphere.GetSurfaceArea();
        var expectedSurfaceArea = 4.0 * Math.PI * Math.Pow(2.5, 2);
        Assert.Equal(expectedSurfaceArea, surfaceArea, 1e-10);

        Assert.Equal(5.0, sphere.GetDiameter());
    }

    [Fact]
    public void UsdGeomSphere_PointOperations_Work()
    {
        var stage = CreateTestStage();
        var spherePath = new SdfPath("/TestSphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath, 2.0);

        // Test point inside sphere
        var insidePoint = new GfVec3f(0.5f, 0.5f, 0.5f);
        Assert.True(sphere.ContainsPoint(insidePoint));
        Assert.True(sphere.GetDistanceToPoint(insidePoint) < 0);

        // Test point outside sphere  
        var outsidePoint = new GfVec3f(3, 0, 0);
        Assert.False(sphere.ContainsPoint(outsidePoint));
        Assert.True(sphere.GetDistanceToPoint(outsidePoint) > 0);

        // Test point on surface
        var surfacePoint = new GfVec3f(2, 0, 0);
        Assert.Equal(0.0, sphere.GetDistanceToPoint(surfacePoint), 1e-10);

        // Test closest point on surface
        var testPoint = new GfVec3f(4, 0, 0);
        var closest = sphere.GetClosestPointOnSurface(testPoint);
        Assert.Equal(2.0f, closest.X, 1e-6f);
        Assert.Equal(0.0f, closest.Y, 1e-6f);
        Assert.Equal(0.0f, closest.Z, 1e-6f);
    }

    [Fact]
    public void UsdGeomSphere_ExtentComputation_Works()
    {
        var stage = CreateTestStage();
        var spherePath = new SdfPath("/TestSphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath, 1.5);

        // Debug: Check radius values
        var actualRadius = sphere.Radius;
        Console.WriteLine($"DEBUG: Sphere radius after Define: {actualRadius}");
        var radiusAtDefault = sphere.GetRadius();
        Console.WriteLine($"DEBUG: Sphere radius at default time: {radiusAtDefault}");

        // Test extent computation
        var success = sphere.ComputeExtent(UsdTimeCode.Default(), out var extent);
        Assert.True(success);
        Assert.Equal(2, extent.Count);

        var min = extent[0];
        var max = extent[1];
        Console.WriteLine($"DEBUG: Computed extent min: ({min.X}, {min.Y}, {min.Z})");
        Console.WriteLine($"DEBUG: Computed extent max: ({max.X}, {max.Y}, {max.Z})");
        
        Assert.Equal(-1.5f, min.X);
        Assert.Equal(-1.5f, min.Y);
        Assert.Equal(-1.5f, min.Z);
        Assert.Equal(1.5f, max.X);
        Assert.Equal(1.5f, max.Y);
        Assert.Equal(1.5f, max.Z);

        // Test static extent computation
        var staticExtent = UsdGeomSphere.ComputeExtentFromRadius(2.0);
        Assert.Equal(-2.0f, staticExtent[0].X);
        Assert.Equal(2.0f, staticExtent[1].X);
    }

    #endregion

    #region UsdGeomCube Tests

    [Fact]
    public void UsdGeomCube_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var cubePath = new SdfPath("/TestCube");
        var cube = UsdGeomCube.Define(stage, cubePath);

        // Test default size
        Assert.Equal(2.0, cube.Size);

        // Test setting size
        cube.Size = 3.0;
        Assert.Equal(3.0, cube.Size);

        // Test geometric properties
        var volume = cube.GetVolume();
        Assert.Equal(27.0, volume); // 3^3

        var surfaceArea = cube.GetSurfaceArea();
        Assert.Equal(54.0, surfaceArea); // 6 * 3^2

        var diagonal = cube.GetDiagonalLength();
        var expectedDiagonal = 3.0 * Math.Sqrt(3.0);
        Assert.Equal(expectedDiagonal, diagonal, 1e-10);
    }

    [Fact]
    public void UsdGeomCube_PointOperations_Work()
    {
        var stage = CreateTestStage();
        var cubePath = new SdfPath("/TestCube");
        var cube = UsdGeomCube.Define(stage, cubePath, 2.0);

        // Test point inside cube
        var insidePoint = new GfVec3f(0.5f, 0.5f, 0.5f);
        Assert.True(cube.ContainsPoint(insidePoint));
        Assert.True(cube.GetDistanceToPoint(insidePoint) < 0);

        // Test point outside cube
        var outsidePoint = new GfVec3f(2, 0, 0);
        Assert.False(cube.ContainsPoint(outsidePoint));
        Assert.True(cube.GetDistanceToPoint(outsidePoint) > 0);

        // Test point on face
        var facePoint = new GfVec3f(1, 0, 0);
        Assert.Equal(0.0, cube.GetDistanceToPoint(facePoint), 1e-10);

        // Test closest point on surface
        var testPoint = new GfVec3f(2, 0, 0);
        var closest = cube.GetClosestPointOnSurface(testPoint);
        Assert.Equal(1.0f, closest.X);
        Assert.Equal(0.0f, closest.Y);
        Assert.Equal(0.0f, closest.Z);
    }

    [Fact]
    public void UsdGeomCube_CornerAndFaceAccess_Works()
    {
        var stage = CreateTestStage();
        var cubePath = new SdfPath("/TestCube");
        var cube = UsdGeomCube.Define(stage, cubePath, 2.0);

        // Test corner vertices
        var corners = cube.GetCornerVertices();
        Assert.Equal(8, corners.Count);
        
        // Verify all corners are at distance 1 from origin
        foreach (var corner in corners)
        {
            Assert.Equal(1.0f, Math.Abs(corner.X));
            Assert.Equal(1.0f, Math.Abs(corner.Y));
            Assert.Equal(1.0f, Math.Abs(corner.Z));
        }

        // Test face centers
        var faceCenters = cube.GetFaceCenters();
        Assert.Equal(6, faceCenters.Count);
        
        // Verify face centers are at distance 1 from origin on primary axes
        var expectedCenters = new[]
        {
            new GfVec3f(0, 0, 1),   // Front
            new GfVec3f(0, 0, -1),  // Back
            new GfVec3f(-1, 0, 0),  // Left
            new GfVec3f(1, 0, 0),   // Right
            new GfVec3f(0, -1, 0),  // Bottom
            new GfVec3f(0, 1, 0)    // Top
        };

        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(expectedCenters[i].X, faceCenters[i].X);
            Assert.Equal(expectedCenters[i].Y, faceCenters[i].Y);
            Assert.Equal(expectedCenters[i].Z, faceCenters[i].Z);
        }
    }

    [Fact]
    public void UsdGeomCube_ExtentComputation_Works()
    {
        var stage = CreateTestStage();
        var cubePath = new SdfPath("/TestCube");
        var cube = UsdGeomCube.Define(stage, cubePath, 4.0);

        // Test extent computation
        var success = cube.ComputeExtent(UsdTimeCode.Default(), out var extent);
        Assert.True(success);
        Assert.Equal(2, extent.Count);

        var min = extent[0];
        var max = extent[1];
        Assert.Equal(-2.0f, min.X);
        Assert.Equal(-2.0f, min.Y);
        Assert.Equal(-2.0f, min.Z);
        Assert.Equal(2.0f, max.X);
        Assert.Equal(2.0f, max.Y);
        Assert.Equal(2.0f, max.Z);

        // Test static extent computation
        var staticExtent = UsdGeomCube.ComputeExtentFromSize(6.0);
        Assert.Equal(-3.0f, staticExtent[0].X);
        Assert.Equal(3.0f, staticExtent[1].X);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void UsdGeom_ComplexScene_CanBeCreated()
    {
        var stage = CreateTestStage();

        // Create root transform
        var rootPath = new SdfPath("/Root");
        var root = UsdGeomXform.Define(stage, rootPath);
        
        // Add translation to root
        var translateOp = root.AddTranslateOp();
        translateOp.Set(new GfVec3f(0, 0, 5));

        // Create a sphere under root
        var spherePath = rootPath.AppendChild(new TfToken("Sphere"));
        var sphere = UsdGeomSphere.Define(stage, spherePath, 1.5);
        sphere.SetDisplayColor(UsdGeomGprim.Red);

        // Create a cube under root
        var cubePath = rootPath.AppendChild(new TfToken("Cube"));
        var cube = UsdGeomCube.Define(stage, cubePath, 2.0);
        cube.SetDisplayColor(UsdGeomGprim.Blue);
        
        // Add transform to cube
        var cubeTranslateOp = cube.AddTranslateOp();
        cubeTranslateOp.Set(new GfVec3f(3, 0, 0));

        // Create mesh under root
        var meshPath = rootPath.AppendChild(new TfToken("Mesh"));
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        mesh.CreatePlane(2.0f, 2.0f, 2, 2);
        mesh.SetDisplayColor(UsdGeomGprim.Green);

        // Verify the scene structure
        Assert.True(root.IsValid);
        Assert.True(sphere.IsValid);
        Assert.True(cube.IsValid);
        Assert.True(mesh.IsValid);

        // Verify transformations
        var rootSuccess = root.GetLocalTransformation(out var rootTransform, out var rootResets);
        Assert.True(rootSuccess);
        Assert.False(rootResets);

        var cubeSuccess = cube.GetLocalTransformation(out var cubeTransform, out var cubeResets);
        Assert.True(cubeSuccess);
        Assert.False(cubeResets);

        // Verify geometry properties
        Assert.Equal(1.5, sphere.Radius);
        Assert.Equal(2.0, cube.Size);
        
        var meshStats = mesh.GetStatistics();
        Assert.Equal(9, meshStats.VertexCount); // 3x3 grid
        Assert.Equal(4, meshStats.FaceCount);   // 2x2 quads
        Assert.Equal(4, meshStats.QuadCount);
    }

    [Fact]
    public void UsdGeom_TimeVaryingAttributes_Work()
    {
        var stage = CreateTestStage();
        var spherePath = new SdfPath("/AnimatedSphere");
        var sphere = UsdGeomSphere.Define(stage, spherePath);

        // Set radius at different time samples
        sphere.SetRadius(1.0, UsdTimeCode.Create(0));
        sphere.SetRadius(2.0, UsdTimeCode.Create(24));
        sphere.SetRadius(3.0, UsdTimeCode.Create(48));

        // Verify values at different times
        Assert.Equal(1.0, sphere.GetRadius(UsdTimeCode.Create(0)));
        Assert.Equal(2.0, sphere.GetRadius(UsdTimeCode.Create(24)));
        Assert.Equal(3.0, sphere.GetRadius(UsdTimeCode.Create(48)));

        // Test extent updates with time-varying radius
        var success0 = sphere.ComputeExtent(UsdTimeCode.Create(0), out var extent0);
        var success24 = sphere.ComputeExtent(UsdTimeCode.Create(24), out var extent24);
        
        Assert.True(success0);
        Assert.True(success24);
        
        // Extent should be larger at time 24
        Assert.True(extent24[1].X > extent0[1].X);
    }

    #endregion
}