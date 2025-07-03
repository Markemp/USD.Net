using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Points are analogous to the RiPoints spec.
/// 
/// Points can be an efficient way to render particle effects or point clouds.
/// The widths attribute is a primvar that can be used to specify the thickness
/// of the points.
/// </summary>
[UsdSchema("Points", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomPoints : UsdGeomPointBased
{
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Points");
    
    #endregion
    
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomPoints on UsdPrim prim.
    /// </summary>
    public UsdGeomPoints(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct a UsdGeomPoints on an invalid prim.
    /// </summary>
    public UsdGeomPoints() : base()
    {
    }
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// Return a UsdGeomPoints holding the prim adhering to this schema at path on stage.
    /// </summary>
    public static UsdGeomPoints Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdGeomPoints(prim);
    }
    
    /// <summary>
    /// Attempt to ensure a UsdPrim adhering to this schema at path is defined
    /// on this stage.
    /// </summary>
    public static new UsdGeomPoints Define(UsdStage stage, SdfPath path)
    {
        var prim = stage.DefinePrim(path, "Points");
        return new UsdGeomPoints(prim);
    }
    
    #endregion
    
    #region Schema Attributes
    
    /// <summary>
    /// Widths are defined as the diameter of the points, in object space.
    /// </summary>
    public List<float> Widths
    {
        get
        {
            var attr = GetWidthsAttr();
            if (attr.IsValid() && attr.Get(out List<float> value))
                return value;
            return new List<float>();
        }
        set
        {
            var attr = CreateWidthsAttr();
            attr.Set(new VtValue(value));
            
            // Update extent when widths change
            UpdateExtentFromGeometry();
        }
    }
    
    /// <summary>
    /// Ids are optional; if authored, the ids array should be the same length
    /// as the points array, providing an identifier for each point.
    /// </summary>
    public List<long> Ids
    {
        get
        {
            var attr = GetIdsAttr();
            if (attr.IsValid() && attr.Get(out List<long> value))
                return value;
            return new List<long>();
        }
        set
        {
            var attr = CreateIdsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    /// <summary>
    /// Create the widths attribute.
    /// </summary>
    public UsdAttribute CreateWidthsAttr()
    {
        return CreateAttribute(new TfToken("widths"), "float[]");
    }
    
    /// <summary>
    /// Get the widths attribute.
    /// </summary>
    public UsdAttribute GetWidthsAttr()
    {
        return GetAttribute(new TfToken("widths"));
    }
    
    /// <summary>
    /// Create the ids attribute.
    /// </summary>
    public UsdAttribute CreateIdsAttr()
    {
        return CreateAttribute(new TfToken("ids"), "int64[]");
    }
    
    /// <summary>
    /// Get the ids attribute.
    /// </summary>
    public UsdAttribute GetIdsAttr()
    {
        return GetAttribute(new TfToken("ids"));
    }
    
    #endregion
    
    #region Interpolation Management
    
    /// <summary>
    /// Get the interpolation for the widths attribute.
    /// Returns "vertex" if not authored (one width per point).
    /// </summary>
    public UsdGeomInterpolation GetWidthsInterpolation()
    {
        var attr = GetWidthsAttr();
        if (!attr.IsValid())
            return UsdGeomInterpolation.Vertex;
            
        var interpolationValue = attr.GetMetadata<string>(new TfToken("interpolation"));
        if (!string.IsNullOrEmpty(interpolationValue))
            return UsdGeomPrimvarConstants.StringToInterpolation(interpolationValue);
            
        return UsdGeomInterpolation.Vertex; // Default
    }
    
    /// <summary>
    /// Set the interpolation for the widths attribute.
    /// </summary>
    public bool SetWidthsInterpolation(UsdGeomInterpolation interpolation)
    {
        var attr = GetWidthsAttr();
        if (!attr.IsValid())
            attr = CreateWidthsAttr();
            
        if (!UsdGeomPrimvarConstants.IsValidInterpolation(interpolation))
            return false;
            
        var interpolationStr = UsdGeomPrimvarConstants.InterpolationToString(interpolation);
        return attr.SetMetadata(new TfToken("interpolation"), interpolationStr);
    }
    
    /// <summary>
    /// Check if widths interpolation has been explicitly authored.
    /// </summary>
    public bool HasAuthoredWidthsInterpolation()
    {
        var attr = GetWidthsAttr();
        if (!attr.IsValid())
            return false;
            
        var interpolationValue = attr.GetMetadata<string>(new TfToken("interpolation"));
        return !string.IsNullOrEmpty(interpolationValue);
    }
    
    #endregion
    
    #region Point Count and Validation
    
    /// <summary>
    /// Validate that widths array (if present) matches the number of points.
    /// </summary>
    public bool ValidateWidths(UsdTimeCode time = default)
    {
        var pointCount = GetPointCount(time);
        if (pointCount == 0)
            return true; // No points to validate
            
        var widths = Widths;
        if (widths.Count == 0)
            return true; // No widths specified is valid
            
        var interpolation = GetWidthsInterpolation();
        
        // For vertex interpolation, should have one width per point
        if (interpolation == UsdGeomInterpolation.Vertex)
            return widths.Count == pointCount;
            
        // For constant interpolation, should have exactly one width
        if (interpolation == UsdGeomInterpolation.Constant)
            return widths.Count == 1;
            
        // For other interpolations, accept the count as-is
        return true;
    }
    
    /// <summary>
    /// Validate that ids array (if present) matches the number of points.
    /// </summary>
    public bool ValidateIds(UsdTimeCode time = default)
    {
        var pointCount = GetPointCount(time);
        if (pointCount == 0)
            return true; // No points to validate
            
        var ids = Ids;
        if (ids.Count == 0)
            return true; // No ids specified is valid
            
        return ids.Count == pointCount;
    }
    
    #endregion
    
    #region Extent Computation
    
    /// <summary>
    /// Compute the extent for the points defined by the given points and widths arrays.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(List<GfVec3f> points, List<float>? widths = null)
    {
        if (points == null || points.Count == 0)
            return (Vector3.Zero, Vector3.Zero);
        
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var pointVec = new Vector3(point.X, point.Y, point.Z);
            
            // Get width for this point (if available)
            var width = 0.0f;
            if (widths != null && widths.Count > 0)
            {
                if (widths.Count == 1)
                {
                    // Constant interpolation - use single width for all points
                    width = widths[0];
                }
                else if (i < widths.Count)
                {
                    // Vertex interpolation - use per-point width
                    width = widths[i];
                }
            }
            
            var radius = width * 0.5f;
            var radiusVec = new Vector3(radius);
            
            // Expand bounds to include the point as a sphere
            min = Vector3.Min(min, pointVec - radiusVec);
            max = Vector3.Max(max, pointVec + radiusVec);
        }
        
        return (min, max);
    }
    
    /// <summary>
    /// Compute the extent for the points defined by the given points and widths arrays,
    /// transformed by the given matrix.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(List<GfVec3f> points, List<float>? widths, Matrix4x4 transform)
    {
        if (points == null || points.Count == 0)
            return (Vector3.Zero, Vector3.Zero);
        
        var transformedMin = new Vector3(float.MaxValue);
        var transformedMax = new Vector3(float.MinValue);
        
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var pointVec = new Vector3(point.X, point.Y, point.Z);
            
            // Transform the point
            var transformedPoint = Vector3.Transform(pointVec, transform);
            
            // Get width for this point (if available)
            var width = 0.0f;
            if (widths != null && widths.Count > 0)
            {
                if (widths.Count == 1)
                {
                    width = widths[0];
                }
                else if (i < widths.Count)
                {
                    width = widths[i];
                }
            }
            
            // Transform the radius (approximate by taking max scale component)
            var radius = width * 0.5f;
            var scaleX = new Vector3(transform.M11, transform.M12, transform.M13).Length();
            var scaleY = new Vector3(transform.M21, transform.M22, transform.M23).Length();
            var scaleZ = new Vector3(transform.M31, transform.M32, transform.M33).Length();
            var maxScale = Math.Max(scaleX, Math.Max(scaleY, scaleZ));
            var transformedRadius = radius * maxScale;
            var radiusVec = new Vector3(transformedRadius);
            
            // Expand bounds to include the transformed point as a sphere
            transformedMin = Vector3.Min(transformedMin, transformedPoint - radiusVec);
            transformedMax = Vector3.Max(transformedMax, transformedPoint + radiusVec);
        }
        
        return (transformedMin, transformedMax);
    }
    
    #endregion
    
    #region ComputeExtentFromGeometry Override
    
    /// <summary>
    /// Compute extent for this points object from its current geometry.
    /// </summary>
    protected override bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        var points = Points;
        if (points == null || points.Count == 0)
            return false;
        
        var widths = Widths;
        var (min, max) = ComputeExtent(points, widths.Count > 0 ? widths : null);
        
        extent.Add(new GfVec3f(min.X, min.Y, min.Z));
        extent.Add(new GfVec3f(max.X, max.Y, max.Z));
        
        return true;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get the effective width for a specific point index.
    /// Handles constant and vertex interpolation correctly.
    /// </summary>
    public float GetPointWidth(int pointIndex)
    {
        var widths = Widths;
        if (widths.Count == 0)
            return 0.0f; // No width specified
        
        var interpolation = GetWidthsInterpolation();
        
        if (interpolation == UsdGeomInterpolation.Constant && widths.Count > 0)
            return widths[0];
        
        if (pointIndex >= 0 && pointIndex < widths.Count)
            return widths[pointIndex];
        
        return 0.0f; // Out of bounds or no width available
    }
    
    /// <summary>
    /// Get the ID for a specific point index.
    /// </summary>
    public long GetPointId(int pointIndex)
    {
        var ids = Ids;
        if (pointIndex >= 0 && pointIndex < ids.Count)
            return ids[pointIndex];
        
        return -1; // No ID available or out of bounds
    }
    
    /// <summary>
    /// Check if a point with the given position and width contains the test point.
    /// </summary>
    public static bool PointContains(Vector3 pointCenter, float width, Vector3 testPoint)
    {
        var radius = width * 0.5f;
        var distance = Vector3.Distance(pointCenter, testPoint);
        return distance <= radius;
    }
    
    /// <summary>
    /// Find the nearest point to the given test point.
    /// Returns the index of the nearest point, or -1 if no points exist.
    /// </summary>
    public int FindNearestPoint(Vector3 testPoint, UsdTimeCode time = default)
    {
        var points = Points;
        if (points == null || points.Count == 0)
            return -1;
        
        var nearestIndex = -1;
        var nearestDistance = float.MaxValue;
        
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var pointVec = new Vector3(point.X, point.Y, point.Z);
            var distance = Vector3.Distance(pointVec, testPoint);
            
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        return nearestIndex;
    }
    
    /// <summary>
    /// Find all points that contain the given test point (accounting for widths).
    /// Returns indices of containing points.
    /// </summary>
    public List<int> FindContainingPoints(Vector3 testPoint, UsdTimeCode time = default)
    {
        var result = new List<int>();
        var points = Points;
        if (points == null || points.Count == 0)
            return result;
        
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var pointVec = new Vector3(point.X, point.Y, point.Z);
            var width = GetPointWidth(i);
            
            if (PointContains(pointVec, width, testPoint))
            {
                result.Add(i);
            }
        }
        
        return result;
    }
    
    #endregion
    
    #region Creation Helper
    
    /// <summary>
    /// Create a points primitive with specified parameters.
    /// </summary>
    public static UsdGeomPoints CreatePrimitive(UsdStage stage, SdfPath path, List<GfVec3f>? points = null, List<float>? widths = null, List<long>? ids = null)
    {
        var pointsGeom = Define(stage, path);
        
        if (points != null && points.Count > 0)
            pointsGeom.Points = points;
        
        if (widths != null && widths.Count > 0)
            pointsGeom.Widths = widths;
        
        if (ids != null && ids.Count > 0)
            pointsGeom.Ids = ids;
        
        return pointsGeom;
    }
    
    /// <summary>
    /// Create a simple point cloud from an array of positions with uniform width.
    /// </summary>
    public static UsdGeomPoints CreatePointCloud(UsdStage stage, SdfPath path, List<Vector3> positions, float uniformWidth = 0.1f)
    {
        var points = positions.Select(p => new GfVec3f(p.X, p.Y, p.Z)).ToList();
        var widths = new List<float> { uniformWidth }; // Constant interpolation
        
        var pointsGeom = CreatePrimitive(stage, path, points, widths);
        pointsGeom.SetWidthsInterpolation(UsdGeomInterpolation.Constant);
        
        return pointsGeom;
    }
    
    #endregion
}