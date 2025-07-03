using System;
using System.Collections.Generic;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Xunit;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdGeomPrimitivesTests
{
    private UsdStage CreateTestStage()
    {
        return UsdStage.CreateInMemory();
    }

    #region UsdGeomCapsule Tests

    [Fact]
    public void UsdGeomCapsule_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var capsulePath = new SdfPath("/TestCapsule");
        var capsule = UsdGeomCapsule.Define(stage, capsulePath);

        Assert.True(capsule.IsValid);
        Assert.Equal(capsulePath, capsule.Path);

        // Test default values
        Assert.Equal(1.0, capsule.Height);
        Assert.Equal(0.5, capsule.Radius);
        Assert.Equal("Z", capsule.Axis.GetText());

        // Test setting values
        capsule.Height = 2.5;
        capsule.Radius = 1.2;
        capsule.Axis = new TfToken("Y");

        Assert.Equal(2.5, capsule.Height);
        Assert.Equal(1.2, capsule.Radius);
        Assert.Equal("Y", capsule.Axis.GetText());
    }

    [Fact]
    public void UsdGeomCapsule_ExtentComputation_Works()
    {
        // Test Z-axis capsule (default)
        var (min, max) = UsdGeomCapsule.ComputeExtent(2.0, 1.0, new TfToken("Z"));
        
        // Height 2.0 + radius 1.0 on each end = total length 4.0, so half-height-with-cap = 2.0
        Assert.Equal(-1.0f, min.X, precision: 5);
        Assert.Equal(-1.0f, min.Y, precision: 5);
        Assert.Equal(-2.0f, min.Z, precision: 5);
        Assert.Equal(1.0f, max.X, precision: 5);
        Assert.Equal(1.0f, max.Y, precision: 5);
        Assert.Equal(2.0f, max.Z, precision: 5);

        // Test X-axis capsule
        var (minX, maxX) = UsdGeomCapsule.ComputeExtent(2.0, 1.0, new TfToken("X"));
        Assert.Equal(-2.0f, minX.X, precision: 5);
        Assert.Equal(-1.0f, minX.Y, precision: 5);
        Assert.Equal(-1.0f, minX.Z, precision: 5);
        Assert.Equal(2.0f, maxX.X, precision: 5);
        Assert.Equal(1.0f, maxX.Y, precision: 5);
        Assert.Equal(1.0f, maxX.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomCapsule_GeometricCalculations_Work()
    {
        var stage = CreateTestStage();
        var capsule = UsdGeomCapsule.CreatePrimitive(stage, new SdfPath("/Capsule"), height: 4.0, radius: 1.0);

        // Volume = cylinder volume + sphere volume
        // Cylinder: π * r² * h = π * 1² * 4 = 4π
        // Sphere: (4/3) * π * r³ = (4/3) * π * 1³ = (4/3)π
        // Total: 4π + (4/3)π = (16/3)π
        var expectedVolume = (16.0 / 3.0) * Math.PI;
        Assert.Equal(expectedVolume, capsule.GetVolume(), precision: 10);

        // Surface area = cylinder side + sphere
        // Cylinder side: 2 * π * r * h = 2 * π * 1 * 4 = 8π
        // Sphere: 4 * π * r² = 4 * π * 1² = 4π
        // Total: 8π + 4π = 12π
        var expectedSurfaceArea = 12.0 * Math.PI;
        Assert.Equal(expectedSurfaceArea, capsule.GetSurfaceArea(), precision: 10);

        // Total length = height + 2*radius = 4 + 2*1 = 6
        Assert.Equal(6.0, capsule.GetTotalLength());
    }

    [Fact]
    public void UsdGeomCapsule_ContainsPoint_Works()
    {
        var stage = CreateTestStage();
        var capsule = UsdGeomCapsule.CreatePrimitive(stage, new SdfPath("/Capsule"), height: 2.0, radius: 1.0);

        // Point inside cylindrical portion
        Assert.True(capsule.ContainsPoint(new Vector3(0.5f, 0.5f, 0.5f)));
        
        // Point inside hemisphere cap
        Assert.True(capsule.ContainsPoint(new Vector3(0.5f, 0.5f, 1.5f)));
        
        // Point outside
        Assert.False(capsule.ContainsPoint(new Vector3(2.0f, 0, 0)));
        Assert.False(capsule.ContainsPoint(new Vector3(0, 0, 3.0f)));
    }

    #endregion

    #region UsdGeomCone Tests

    [Fact]
    public void UsdGeomCone_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var conePath = new SdfPath("/TestCone");
        var cone = UsdGeomCone.Define(stage, conePath);

        Assert.True(cone.IsValid);
        Assert.Equal(conePath, cone.Path);

        // Test default values
        Assert.Equal(2.0, cone.Height);
        Assert.Equal(1.0, cone.Radius);
        Assert.Equal("Z", cone.Axis.GetText());

        // Test setting values
        cone.Height = 3.0;
        cone.Radius = 1.5;
        cone.Axis = new TfToken("X");

        Assert.Equal(3.0, cone.Height);
        Assert.Equal(1.5, cone.Radius);
        Assert.Equal("X", cone.Axis.GetText());
    }

    [Fact]
    public void UsdGeomCone_ExtentComputation_Works()
    {
        // Test Z-axis cone (default)
        var (min, max) = UsdGeomCone.ComputeExtent(4.0, 2.0, new TfToken("Z"));
        
        Assert.Equal(-2.0f, min.X, precision: 5);
        Assert.Equal(-2.0f, min.Y, precision: 5);
        Assert.Equal(-2.0f, min.Z, precision: 5); // height/2 = 4/2 = 2
        Assert.Equal(2.0f, max.X, precision: 5);
        Assert.Equal(2.0f, max.Y, precision: 5);
        Assert.Equal(2.0f, max.Z, precision: 5);

        // Test Y-axis cone
        var (minY, maxY) = UsdGeomCone.ComputeExtent(4.0, 2.0, new TfToken("Y"));
        Assert.Equal(-2.0f, minY.X, precision: 5);
        Assert.Equal(-2.0f, minY.Y, precision: 5);
        Assert.Equal(-2.0f, minY.Z, precision: 5);
        Assert.Equal(2.0f, maxY.X, precision: 5);
        Assert.Equal(2.0f, maxY.Y, precision: 5);
        Assert.Equal(2.0f, maxY.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomCone_GeometricCalculations_Work()
    {
        var stage = CreateTestStage();
        var cone = UsdGeomCone.CreatePrimitive(stage, new SdfPath("/Cone"), height: 3.0, radius: 2.0);

        // Volume = (1/3) * π * r² * h = (1/3) * π * 4 * 3 = 4π
        var expectedVolume = 4.0 * Math.PI;
        Assert.Equal(expectedVolume, cone.GetVolume(), precision: 10);

        // Slant height = sqrt(r² + h²) = sqrt(4 + 9) = sqrt(13)
        var expectedSlantHeight = Math.Sqrt(13.0);
        Assert.Equal(expectedSlantHeight, cone.GetSlantHeight(), precision: 10);

        // Surface area = base + lateral = πr² + πr*slant = π*4 + π*2*sqrt(13) = π(4 + 2*sqrt(13))
        var expectedSurfaceArea = Math.PI * (4.0 + 2.0 * Math.Sqrt(13.0));
        Assert.Equal(expectedSurfaceArea, cone.GetSurfaceArea(), precision: 10);
    }

    [Fact]
    public void UsdGeomCone_ContainsPoint_Works()
    {
        var stage = CreateTestStage();
        var cone = UsdGeomCone.CreatePrimitive(stage, new SdfPath("/Cone"), height: 4.0, radius: 2.0);

        // Point at base center
        Assert.True(cone.ContainsPoint(new Vector3(0, 0, -2.0f)));
        
        // Point at apex
        Assert.True(cone.ContainsPoint(new Vector3(0, 0, 2.0f)));
        
        // Point inside cone
        Assert.True(cone.ContainsPoint(new Vector3(0.5f, 0.5f, 0)));
        
        // Point outside cone (too wide for height)
        Assert.False(cone.ContainsPoint(new Vector3(1.5f, 0, 0)));
        
        // Point outside height bounds
        Assert.False(cone.ContainsPoint(new Vector3(0, 0, 3.0f)));
    }

    #endregion

    #region UsdGeomCylinder Tests

    [Fact]
    public void UsdGeomCylinder_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var cylinderPath = new SdfPath("/TestCylinder");
        var cylinder = UsdGeomCylinder.Define(stage, cylinderPath);

        Assert.True(cylinder.IsValid);
        Assert.Equal(cylinderPath, cylinder.Path);

        // Test default values
        Assert.Equal(2.0, cylinder.Height);
        Assert.Equal(1.0, cylinder.Radius);
        Assert.Equal("Z", cylinder.Axis.GetText());

        // Test setting values
        cylinder.Height = 4.0;
        cylinder.Radius = 2.0;
        cylinder.Axis = new TfToken("Y");

        Assert.Equal(4.0, cylinder.Height);
        Assert.Equal(2.0, cylinder.Radius);
        Assert.Equal("Y", cylinder.Axis.GetText());
    }

    [Fact]
    public void UsdGeomCylinder_ExtentComputation_Works()
    {
        // Test Z-axis cylinder (default)
        var (min, max) = UsdGeomCylinder.ComputeExtent(6.0, 2.0, new TfToken("Z"));
        
        Assert.Equal(-2.0f, min.X, precision: 5);
        Assert.Equal(-2.0f, min.Y, precision: 5);
        Assert.Equal(-3.0f, min.Z, precision: 5); // height/2 = 6/2 = 3
        Assert.Equal(2.0f, max.X, precision: 5);
        Assert.Equal(2.0f, max.Y, precision: 5);
        Assert.Equal(3.0f, max.Z, precision: 5);

        // Test X-axis cylinder
        var (minX, maxX) = UsdGeomCylinder.ComputeExtent(6.0, 2.0, new TfToken("X"));
        Assert.Equal(-3.0f, minX.X, precision: 5);
        Assert.Equal(-2.0f, minX.Y, precision: 5);
        Assert.Equal(-2.0f, minX.Z, precision: 5);
        Assert.Equal(3.0f, maxX.X, precision: 5);
        Assert.Equal(2.0f, maxX.Y, precision: 5);
        Assert.Equal(2.0f, maxX.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomCylinder_GeometricCalculations_Work()
    {
        var stage = CreateTestStage();
        var cylinder = UsdGeomCylinder.CreatePrimitive(stage, new SdfPath("/Cylinder"), height: 4.0, radius: 1.5);

        // Volume = π * r² * h = π * 2.25 * 4 = 9π
        var expectedVolume = 9.0 * Math.PI;
        Assert.Equal(expectedVolume, cylinder.GetVolume(), precision: 10);

        // Surface area = 2 * cap area + lateral area = 2*πr² + 2πrh = 2*π*2.25 + 2*π*1.5*4 = π(4.5 + 12) = 16.5π
        var expectedSurfaceArea = 16.5 * Math.PI;
        Assert.Equal(expectedSurfaceArea, cylinder.GetSurfaceArea(), precision: 10);

        // Lateral surface area = 2πrh = 2*π*1.5*4 = 12π
        var expectedLateralArea = 12.0 * Math.PI;
        Assert.Equal(expectedLateralArea, cylinder.GetLateralSurfaceArea(), precision: 10);
    }

    [Fact]
    public void UsdGeomCylinder_ContainsPoint_Works()
    {
        var stage = CreateTestStage();
        var cylinder = UsdGeomCylinder.CreatePrimitive(stage, new SdfPath("/Cylinder"), height: 4.0, radius: 2.0);

        // Point inside cylinder
        Assert.True(cylinder.ContainsPoint(new Vector3(1.0f, 1.0f, 1.0f)));
        
        // Point on edge
        Assert.True(cylinder.ContainsPoint(new Vector3(2.0f, 0, 0)));
        
        // Point outside radius
        Assert.False(cylinder.ContainsPoint(new Vector3(3.0f, 0, 0)));
        
        // Point outside height
        Assert.False(cylinder.ContainsPoint(new Vector3(0, 0, 3.0f)));
    }

    #endregion

    #region UsdGeomPlane Tests

    [Fact]
    public void UsdGeomPlane_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var planePath = new SdfPath("/TestPlane");
        var plane = UsdGeomPlane.Define(stage, planePath);

        Assert.True(plane.IsValid);
        Assert.Equal(planePath, plane.Path);

        // Test default values
        Assert.Equal(2.0, plane.Width);
        Assert.Equal(2.0, plane.Length);
        Assert.Equal("Z", plane.Axis.GetText());
        Assert.False(plane.DoubleSided); // Default value from UsdGeomGprim

        // Test setting values
        plane.Width = 4.0;
        plane.Length = 3.0;
        plane.Axis = new TfToken("X");
        plane.DoubleSided = false;

        Assert.Equal(4.0, plane.Width);
        Assert.Equal(3.0, plane.Length);
        Assert.Equal("X", plane.Axis.GetText());
        Assert.False(plane.DoubleSided);
    }

    [Fact]
    public void UsdGeomPlane_ExtentComputation_Works()
    {
        // Test Z-axis plane (default) - xy plane
        var (min, max) = UsdGeomPlane.ComputeExtent(4.0, 6.0, new TfToken("Z"));
        
        Assert.Equal(-2.0f, min.X, precision: 5); // width/2 = 4/2 = 2
        Assert.Equal(-3.0f, min.Y, precision: 5); // length/2 = 6/2 = 3
        Assert.Equal(0.0f, min.Z, precision: 5);
        Assert.Equal(2.0f, max.X, precision: 5);
        Assert.Equal(3.0f, max.Y, precision: 5);
        Assert.Equal(0.0f, max.Z, precision: 5);

        // Test X-axis plane - yz plane
        var (minX, maxX) = UsdGeomPlane.ComputeExtent(4.0, 6.0, new TfToken("X"));
        Assert.Equal(0.0f, minX.X, precision: 5);
        Assert.Equal(-3.0f, minX.Y, precision: 5); // length on Y
        Assert.Equal(-2.0f, minX.Z, precision: 5); // width on Z
        Assert.Equal(0.0f, maxX.X, precision: 5);
        Assert.Equal(3.0f, maxX.Y, precision: 5);
        Assert.Equal(2.0f, maxX.Z, precision: 5);

        // Test Y-axis plane - xz plane
        var (minY, maxY) = UsdGeomPlane.ComputeExtent(4.0, 6.0, new TfToken("Y"));
        Assert.Equal(-2.0f, minY.X, precision: 5); // width on X
        Assert.Equal(0.0f, minY.Y, precision: 5);
        Assert.Equal(-3.0f, minY.Z, precision: 5); // length on Z
        Assert.Equal(2.0f, maxY.X, precision: 5);
        Assert.Equal(0.0f, maxY.Y, precision: 5);
        Assert.Equal(3.0f, maxY.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomPlane_GeometricCalculations_Work()
    {
        var stage = CreateTestStage();
        var plane = UsdGeomPlane.CreatePrimitive(stage, new SdfPath("/Plane"), width: 4.0, length: 3.0);

        // Area = width * length = 4 * 3 = 12
        Assert.Equal(12.0, plane.GetArea());

        // Perimeter = 2 * (width + length) = 2 * (4 + 3) = 14
        Assert.Equal(14.0, plane.GetPerimeter());

        // Normal vector for Z-axis plane
        var normal = plane.GetNormal();
        Assert.Equal(Vector3.UnitZ, normal);
    }

    [Fact]
    public void UsdGeomPlane_PointOperations_Work()
    {
        var stage = CreateTestStage();
        var plane = UsdGeomPlane.CreatePrimitive(stage, new SdfPath("/Plane"), width: 4.0, length: 6.0, axis: new TfToken("Z"));

        // Point on plane within bounds
        Assert.True(plane.ContainsPoint(new Vector3(1.0f, 2.0f, 0.0f)));
        
        // Point on plane but outside bounds
        Assert.False(plane.ContainsPoint(new Vector3(3.0f, 0.0f, 0.0f))); // Outside width
        Assert.False(plane.ContainsPoint(new Vector3(0.0f, 4.0f, 0.0f))); // Outside length
        
        // Point off plane
        Assert.False(plane.ContainsPoint(new Vector3(0.0f, 0.0f, 1.0f)));

        // Distance to point
        Assert.Equal(0.0f, plane.GetDistanceToPoint(new Vector3(1.0f, 1.0f, 0.0f)), precision: 5);
        Assert.Equal(2.0f, plane.GetDistanceToPoint(new Vector3(0.0f, 0.0f, 2.0f)), precision: 5);

        // Project point onto plane
        var projected = plane.ProjectPoint(new Vector3(1.0f, 2.0f, 5.0f));
        Assert.Equal(new Vector3(1.0f, 2.0f, 0.0f), projected);
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void UsdGeomPrimitives_FactoryMethods_Work()
    {
        var stage = CreateTestStage();

        // Test all factory methods create valid objects
        var capsule = UsdGeomCapsule.Get(stage, new SdfPath("/TestCapsule"));
        var cone = UsdGeomCone.Get(stage, new SdfPath("/TestCone"));
        var cylinder = UsdGeomCylinder.Get(stage, new SdfPath("/TestCylinder"));
        var plane = UsdGeomPlane.Get(stage, new SdfPath("/TestPlane"));

        // Objects should be created but invalid until defined
        Assert.False(capsule.IsValid);
        Assert.False(cone.IsValid);
        Assert.False(cylinder.IsValid);
        Assert.False(plane.IsValid);

        // Define the prims
        capsule = UsdGeomCapsule.Define(stage, new SdfPath("/TestCapsule"));
        cone = UsdGeomCone.Define(stage, new SdfPath("/TestCone"));
        cylinder = UsdGeomCylinder.Define(stage, new SdfPath("/TestCylinder"));
        plane = UsdGeomPlane.Define(stage, new SdfPath("/TestPlane"));

        // Now they should be valid
        Assert.True(capsule.IsValid);
        Assert.True(cone.IsValid);
        Assert.True(cylinder.IsValid);
        Assert.True(plane.IsValid);
    }

    [Fact]
    public void UsdGeomPrimitives_CreatePrimitive_SetsDefaultsCorrectly()
    {
        var stage = CreateTestStage();

        var capsule = UsdGeomCapsule.CreatePrimitive(stage, new SdfPath("/Capsule"));
        Assert.Equal(1.0, capsule.Height);
        Assert.Equal(0.5, capsule.Radius);
        Assert.Equal("Z", capsule.Axis.GetText());

        var cone = UsdGeomCone.CreatePrimitive(stage, new SdfPath("/Cone"));
        Assert.Equal(2.0, cone.Height);
        Assert.Equal(1.0, cone.Radius);
        Assert.Equal("Z", cone.Axis.GetText());

        var cylinder = UsdGeomCylinder.CreatePrimitive(stage, new SdfPath("/Cylinder"));
        Assert.Equal(2.0, cylinder.Height);
        Assert.Equal(1.0, cylinder.Radius);
        Assert.Equal("Z", cylinder.Axis.GetText());

        var plane = UsdGeomPlane.CreatePrimitive(stage, new SdfPath("/Plane"));
        Assert.Equal(2.0, plane.Width);
        Assert.Equal(2.0, plane.Length);
        Assert.Equal("Z", plane.Axis.GetText());
        Assert.True(plane.DoubleSided); // Set to true by CreatePrimitive
    }

    #endregion
}