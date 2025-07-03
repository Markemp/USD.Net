using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Xunit;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdGeomPointsTests
{
    private UsdStage CreateTestStage()
    {
        return UsdStage.CreateInMemory();
    }

    #region Basic Properties Tests

    [Fact]
    public void UsdGeomPoints_BasicProperties_Work()
    {
        var stage = CreateTestStage();
        var pointsPath = new SdfPath("/TestPoints");
        var points = UsdGeomPoints.Define(stage, pointsPath);

        Assert.True(points.IsValid);
        Assert.Equal(pointsPath, points.Path);

        // Initially should have empty arrays
        Assert.Empty(points.Points);
        Assert.Empty(points.Widths);
        Assert.Empty(points.Ids);
        Assert.Equal(0, points.GetPointCount());
    }

    [Fact]
    public void UsdGeomPoints_PointsAttribute_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        var pointsData = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f),
            new GfVec3f(0.0f, 0.0f, 1.0f)
        };

        points.Points = pointsData;

        Assert.Equal(4, points.GetPointCount());
        Assert.Equal(pointsData.Count, points.Points.Count);
        for (int i = 0; i < pointsData.Count; i++)
        {
            Assert.Equal(pointsData[i], points.Points[i]);
        }
    }

    [Fact]
    public void UsdGeomPoints_WidthsAttribute_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Set some points first
        points.Points = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f)
        };

        // Test vertex interpolation (per-point widths)
        var widths = new List<float> { 0.1f, 0.2f, 0.3f };
        points.Widths = widths;

        Assert.Equal(widths.Count, points.Widths.Count);
        for (int i = 0; i < widths.Count; i++)
        {
            Assert.Equal(widths[i], points.Widths[i]);
            Assert.Equal(widths[i], points.GetPointWidth(i));
        }

        // Test constant interpolation (single width for all points)
        var constantWidth = new List<float> { 0.5f };
        points.Widths = constantWidth;
        points.SetWidthsInterpolation(UsdGeomInterpolation.Constant);

        Assert.Single(points.Widths);
        Assert.Equal(0.5f, points.Widths[0]);
        
        // All points should return the same width
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(0.5f, points.GetPointWidth(i));
        }
    }

    [Fact]
    public void UsdGeomPoints_IdsAttribute_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Set some points first
        points.Points = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f)
        };

        var ids = new List<long> { 100, 200, 300 };
        points.Ids = ids;

        Assert.Equal(ids.Count, points.Ids.Count);
        for (int i = 0; i < ids.Count; i++)
        {
            Assert.Equal(ids[i], points.Ids[i]);
            Assert.Equal(ids[i], points.GetPointId(i));
        }

        // Test out of bounds
        Assert.Equal(-1, points.GetPointId(10));
    }

    #endregion

    #region Interpolation Tests

    [Fact]
    public void UsdGeomPoints_WidthsInterpolation_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Default interpolation should be vertex
        Assert.Equal(UsdGeomInterpolation.Vertex, points.GetWidthsInterpolation());
        Assert.False(points.HasAuthoredWidthsInterpolation());

        // Set interpolation
        Assert.True(points.SetWidthsInterpolation(UsdGeomInterpolation.Constant));
        Assert.Equal(UsdGeomInterpolation.Constant, points.GetWidthsInterpolation());
        Assert.True(points.HasAuthoredWidthsInterpolation());

        // Change interpolation
        Assert.True(points.SetWidthsInterpolation(UsdGeomInterpolation.Uniform));
        Assert.Equal(UsdGeomInterpolation.Uniform, points.GetWidthsInterpolation());
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void UsdGeomPoints_ValidateWidths_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Empty points - validation should pass
        Assert.True(points.ValidateWidths());

        // Set 3 points
        points.Points = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f)
        };

        // No widths - should be valid
        Assert.True(points.ValidateWidths());

        // Vertex interpolation with matching count - should be valid
        points.Widths = new List<float> { 0.1f, 0.2f, 0.3f };
        points.SetWidthsInterpolation(UsdGeomInterpolation.Vertex);
        Assert.True(points.ValidateWidths());

        // Vertex interpolation with wrong count - should be invalid
        points.Widths = new List<float> { 0.1f, 0.2f };
        Assert.False(points.ValidateWidths());

        // Constant interpolation with single width - should be valid
        points.Widths = new List<float> { 0.5f };
        points.SetWidthsInterpolation(UsdGeomInterpolation.Constant);
        Assert.True(points.ValidateWidths());

        // Constant interpolation with multiple widths - should be invalid
        points.Widths = new List<float> { 0.1f, 0.2f, 0.3f };
        points.SetWidthsInterpolation(UsdGeomInterpolation.Constant);
        Assert.False(points.ValidateWidths());
    }

    [Fact]
    public void UsdGeomPoints_ValidateIds_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Empty points - validation should pass
        Assert.True(points.ValidateIds());

        // Set 3 points
        points.Points = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f)
        };

        // No ids - should be valid
        Assert.True(points.ValidateIds());

        // Matching count - should be valid
        points.Ids = new List<long> { 100, 200, 300 };
        Assert.True(points.ValidateIds());

        // Wrong count - should be invalid
        points.Ids = new List<long> { 100, 200 };
        Assert.False(points.ValidateIds());
    }

    #endregion

    #region Extent Computation Tests

    [Fact]
    public void UsdGeomPoints_ExtentComputation_WithoutWidths_Works()
    {
        var pointsData = new List<GfVec3f>
        {
            new GfVec3f(-1.0f, -2.0f, -3.0f),
            new GfVec3f(2.0f, 1.0f, 0.0f),
            new GfVec3f(0.0f, 3.0f, -1.0f)
        };

        var (min, max) = UsdGeomPoints.ComputeExtent(pointsData);

        Assert.Equal(-1.0f, min.X, precision: 5);
        Assert.Equal(-2.0f, min.Y, precision: 5);
        Assert.Equal(-3.0f, min.Z, precision: 5);
        Assert.Equal(2.0f, max.X, precision: 5);
        Assert.Equal(3.0f, max.Y, precision: 5);
        Assert.Equal(0.0f, max.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomPoints_ExtentComputation_WithVertexWidths_Works()
    {
        var pointsData = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f)
        };

        var widths = new List<float> { 2.0f, 4.0f }; // Radii: 1.0f, 2.0f

        var (min, max) = UsdGeomPoints.ComputeExtent(pointsData, widths);

        // First point: center (0,0,0) + radius 1.0 = extent (-1,-1,-1) to (1,1,1)
        // Second point: center (1,0,0) + radius 2.0 = extent (-1,-2,-2) to (3,2,2)
        // Combined: (-1,-2,-2) to (3,2,2)
        Assert.Equal(-1.0f, min.X, precision: 5);
        Assert.Equal(-2.0f, min.Y, precision: 5);
        Assert.Equal(-2.0f, min.Z, precision: 5);
        Assert.Equal(3.0f, max.X, precision: 5);
        Assert.Equal(2.0f, max.Y, precision: 5);
        Assert.Equal(2.0f, max.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomPoints_ExtentComputation_WithConstantWidth_Works()
    {
        var pointsData = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(2.0f, 0.0f, 0.0f)
        };

        var widths = new List<float> { 2.0f }; // Constant width, radius = 1.0f

        var (min, max) = UsdGeomPoints.ComputeExtent(pointsData, widths);

        // Both points get radius 1.0f
        // Combined extent: (-1,-1,-1) to (3,1,1)
        Assert.Equal(-1.0f, min.X, precision: 5);
        Assert.Equal(-1.0f, min.Y, precision: 5);
        Assert.Equal(-1.0f, min.Z, precision: 5);
        Assert.Equal(3.0f, max.X, precision: 5);
        Assert.Equal(1.0f, max.Y, precision: 5);
        Assert.Equal(1.0f, max.Z, precision: 5);
    }

    [Fact]
    public void UsdGeomPoints_StaticExtentComputation_Works()
    {
        // Test with empty points
        var (min1, max1) = UsdGeomPoints.ComputeExtent(new List<GfVec3f>());
        Assert.Equal(Vector3.Zero, min1);
        Assert.Equal(Vector3.Zero, max1);

        // Add points and widths
        var pointsData = new List<GfVec3f>
        {
            new GfVec3f(-1.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f)
        };
        var widths = new List<float> { 2.0f }; // Constant width

        var (min, max) = UsdGeomPoints.ComputeExtent(pointsData, widths);

        // Expect extent (-2,-1,-1) to (2,1,1)
        Assert.Equal(-2.0f, min.X, precision: 5);
        Assert.Equal(-1.0f, min.Y, precision: 5);
        Assert.Equal(-1.0f, min.Z, precision: 5);
        Assert.Equal(2.0f, max.X, precision: 5);
        Assert.Equal(1.0f, max.Y, precision: 5);
        Assert.Equal(1.0f, max.Z, precision: 5);
    }

    #endregion

    #region Utility Methods Tests

    [Fact]
    public void UsdGeomPoints_PointContains_Works()
    {
        var pointCenter = new Vector3(0.0f, 0.0f, 0.0f);
        var width = 2.0f; // radius = 1.0f

        Assert.True(UsdGeomPoints.PointContains(pointCenter, width, new Vector3(0.0f, 0.0f, 0.0f)));
        Assert.True(UsdGeomPoints.PointContains(pointCenter, width, new Vector3(0.5f, 0.5f, 0.0f)));
        Assert.True(UsdGeomPoints.PointContains(pointCenter, width, new Vector3(1.0f, 0.0f, 0.0f)));
        Assert.False(UsdGeomPoints.PointContains(pointCenter, width, new Vector3(1.5f, 0.0f, 0.0f)));
    }

    [Fact]
    public void UsdGeomPoints_FindNearestPoint_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Test with no points
        Assert.Equal(-1, points.FindNearestPoint(Vector3.Zero));

        // Add points
        points.Points = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),  // Index 0
            new GfVec3f(5.0f, 0.0f, 0.0f),  // Index 1  
            new GfVec3f(0.0f, 3.0f, 0.0f)   // Index 2
        };

        // Test point should be closest to index 0
        Assert.Equal(0, points.FindNearestPoint(new Vector3(0.1f, 0.1f, 0.0f)));

        // Test point should be closest to index 1
        Assert.Equal(1, points.FindNearestPoint(new Vector3(4.8f, 0.0f, 0.0f)));

        // Test point should be closest to index 2
        Assert.Equal(2, points.FindNearestPoint(new Vector3(0.0f, 2.9f, 0.0f)));
    }

    [Fact]
    public void UsdGeomPoints_FindContainingPoints_Works()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Add points with widths
        points.Points = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),  // Index 0
            new GfVec3f(2.0f, 0.0f, 0.0f),  // Index 1  
            new GfVec3f(1.0f, 1.0f, 0.0f)   // Index 2
        };
        points.Widths = new List<float> { 2.0f, 1.0f, 1.5f }; // Different widths per point

        // Test point at origin - should be contained by point 0 (radius 1.0)
        var containing = points.FindContainingPoints(Vector3.Zero);
        Assert.Single(containing);
        Assert.Equal(0, containing[0]);

        // Test point at (1.5, 0, 0) - should be contained by points 0 and 1
        // Point 0: distance = 1.5, radius = 1.0 → NOT contained
        // Point 1: distance = 0.5, radius = 0.5 → contained
        containing = points.FindContainingPoints(new Vector3(1.5f, 0.0f, 0.0f));
        Assert.Single(containing);
        Assert.Equal(1, containing[0]);

        // Test point far away - should not be contained by any point
        containing = points.FindContainingPoints(new Vector3(10.0f, 10.0f, 10.0f));
        Assert.Empty(containing);
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void UsdGeomPoints_FactoryMethods_Work()
    {
        var stage = CreateTestStage();

        // Test Get method with undefined prim
        var points = UsdGeomPoints.Get(stage, new SdfPath("/TestPoints"));
        Assert.False(points.IsValid);

        // Test Define method
        points = UsdGeomPoints.Define(stage, new SdfPath("/TestPoints"));
        Assert.True(points.IsValid);

        // Test Get method with defined prim
        var samePoints = UsdGeomPoints.Get(stage, new SdfPath("/TestPoints"));
        Assert.True(samePoints.IsValid);
        Assert.Equal(points.Path, samePoints.Path);
    }

    [Fact]
    public void UsdGeomPoints_CreatePrimitive_Works()
    {
        var stage = CreateTestStage();

        var pointsData = new List<GfVec3f>
        {
            new GfVec3f(0.0f, 0.0f, 0.0f),
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f)
        };

        var widths = new List<float> { 0.1f, 0.2f, 0.3f };
        var ids = new List<long> { 100, 200, 300 };

        var points = UsdGeomPoints.CreatePrimitive(stage, new SdfPath("/Points"), pointsData, widths, ids);

        Assert.True(points.IsValid);
        Assert.Equal(3, points.GetPointCount());
        Assert.Equal(pointsData.Count, points.Points.Count);
        Assert.Equal(widths.Count, points.Widths.Count);
        Assert.Equal(ids.Count, points.Ids.Count);

        for (int i = 0; i < pointsData.Count; i++)
        {
            Assert.Equal(pointsData[i], points.Points[i]);
            Assert.Equal(widths[i], points.Widths[i]);
            Assert.Equal(ids[i], points.Ids[i]);
        }
    }

    [Fact]
    public void UsdGeomPoints_CreatePointCloud_Works()
    {
        var stage = CreateTestStage();

        var positions = new List<Vector3>
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 2.0f, -0.5f)
        };

        var uniformWidth = 0.25f;

        var points = UsdGeomPoints.CreatePointCloud(stage, new SdfPath("/PointCloud"), positions, uniformWidth);

        Assert.True(points.IsValid);
        Assert.Equal(3, points.GetPointCount());
        Assert.Single(points.Widths); // Should have constant width
        Assert.Equal(uniformWidth, points.Widths[0]);
        Assert.Equal(UsdGeomInterpolation.Constant, points.GetWidthsInterpolation());

        // Verify all points get the same width
        for (int i = 0; i < positions.Count; i++)
        {
            Assert.Equal(uniformWidth, points.GetPointWidth(i));
            
            var expectedPoint = new GfVec3f(positions[i].X, positions[i].Y, positions[i].Z);
            Assert.Equal(expectedPoint, points.Points[i]);
        }
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void UsdGeomPoints_EmptyArrays_HandleCorrectly()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        // Test with empty points
        var (min, max) = UsdGeomPoints.ComputeExtent(new List<GfVec3f>());
        Assert.Equal(Vector3.Zero, min);
        Assert.Equal(Vector3.Zero, max);

        // Test with null widths
        var pointsData = new List<GfVec3f> { new GfVec3f(0, 0, 0) };
        var (min2, max2) = UsdGeomPoints.ComputeExtent(pointsData, null);
        Assert.Equal(Vector3.Zero, min2);
        Assert.Equal(Vector3.Zero, max2);

        // Test with empty widths
        var (min3, max3) = UsdGeomPoints.ComputeExtent(pointsData, new List<float>());
        Assert.Equal(Vector3.Zero, min3);
        Assert.Equal(Vector3.Zero, max3);
    }

    [Fact]
    public void UsdGeomPoints_OutOfBounds_HandlesGracefully()
    {
        var stage = CreateTestStage();
        var points = UsdGeomPoints.Define(stage, new SdfPath("/Points"));

        points.Points = new List<GfVec3f> { new GfVec3f(0, 0, 0) };
        points.Widths = new List<float> { 0.5f };

        // Test out of bounds access
        Assert.Equal(0.0f, points.GetPointWidth(-1));
        Assert.Equal(0.0f, points.GetPointWidth(10));
        Assert.Equal(-1, points.GetPointId(-1));
        Assert.Equal(-1, points.GetPointId(10));
    }

    #endregion
}