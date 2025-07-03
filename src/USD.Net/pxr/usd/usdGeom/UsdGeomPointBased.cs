using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("PointBased", UsdSchemaKind.AbstractTyped)]
public abstract class UsdGeomPointBased : UsdGeomGprim
{
    #region Construction
    
    protected UsdGeomPointBased(UsdPrim prim) : base(prim)
    {
    }
    
    protected UsdGeomPointBased() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.AbstractTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("PointBased");
    
    #endregion
    
    #region Core Point-Based Attributes
    
    public UsdAttribute GetPointsAttr()
    {
        return GetAttribute(new TfToken("points"));
    }
    
    public UsdAttribute CreatePointsAttr()
    {
        return CreateAttribute(
            new TfToken("points"), 
            "point3f[]", 
            false, 
            SdfVariability.Varying);
    }
    
    public List<GfVec3f> Points
    {
        get
        {
            var attr = GetPointsAttr();
            if (attr.IsValid() && attr.Get(out List<GfVec3f> value))
                return value;
            return new List<GfVec3f>();
        }
        set
        {
            var attr = CreatePointsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    public UsdAttribute GetVelocitiesAttr()
    {
        return GetAttribute(new TfToken("velocities"));
    }
    
    public UsdAttribute CreateVelocitiesAttr()
    {
        return CreateAttribute(
            new TfToken("velocities"), 
            "vector3f[]", 
            false, 
            SdfVariability.Varying);
    }
    
    public List<GfVec3f> Velocities
    {
        get
        {
            var attr = GetVelocitiesAttr();
            if (attr.IsValid() && attr.Get(out List<GfVec3f> value))
                return value;
            return new List<GfVec3f>();
        }
        set
        {
            var attr = CreateVelocitiesAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    public UsdAttribute GetAccelerationsAttr()
    {
        return GetAttribute(new TfToken("accelerations"));
    }
    
    public UsdAttribute CreateAccelerationsAttr()
    {
        return CreateAttribute(
            new TfToken("accelerations"), 
            "vector3f[]", 
            false, 
            SdfVariability.Varying);
    }
    
    public List<GfVec3f> Accelerations
    {
        get
        {
            var attr = GetAccelerationsAttr();
            if (attr.IsValid() && attr.Get(out List<GfVec3f> value))
                return value;
            return new List<GfVec3f>();
        }
        set
        {
            var attr = CreateAccelerationsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    public UsdAttribute GetNormalsAttr()
    {
        return GetAttribute(new TfToken("normals"));
    }
    
    public UsdAttribute CreateNormalsAttr()
    {
        return CreateAttribute(
            new TfToken("normals"), 
            "normal3f[]", 
            false, 
            SdfVariability.Varying);
    }
    
    public List<GfVec3f> Normals
    {
        get
        {
            var attr = GetNormalsAttr();
            if (attr.IsValid() && attr.Get(out List<GfVec3f> value))
                return value;
            return new List<GfVec3f>();
        }
        set
        {
            var attr = CreateNormalsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Normals Interpolation
    
    public TfToken GetNormalsInterpolation()
    {
        var attr = GetNormalsAttr();
        if (attr.IsValid())
        {
            var metadata = attr.GetMetadata<string>("interpolation");
            if (!string.IsNullOrEmpty(metadata))
                return new TfToken(metadata);
        }
        return new TfToken("vertex"); // Default interpolation
    }
    
    public bool SetNormalsInterpolation(TfToken interpolation)
    {
        var attr = CreateNormalsAttr();
        if (!attr.IsValid())
            return false;
            
        var validInterpolations = new[] { "constant", "uniform", "varying", "vertex", "faceVarying" };
        var interpStr = interpolation.GetText();
        
        if (!Array.Exists(validInterpolations, x => x == interpStr))
            return false;
            
        return attr.SetMetadata("interpolation", new VtValue(interpStr));
    }
    
    #endregion
    
    #region Extent Computation
    
    public static bool ComputeExtent(List<GfVec3f> points, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        if (points.Count == 0)
            return false;
            
        var min = new GfVec3f(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new GfVec3f(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var point in points)
        {
            if (point.X < min.X) min.X = point.X;
            if (point.Y < min.Y) min.Y = point.Y;
            if (point.Z < min.Z) min.Z = point.Z;
            
            if (point.X > max.X) max.X = point.X;
            if (point.Y > max.Y) max.Y = point.Y;
            if (point.Z > max.Z) max.Z = point.Z;
        }
        
        extent = new List<GfVec3f> { min, max };
        return true;
    }
    
    public static bool ComputeExtent(List<GfVec3f> points, GfMatrix4d transform, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        if (points.Count == 0)
            return false;
        
        var transformedPoints = new List<GfVec3f>();
        foreach (var point in points)
        {
            var transformed = transform.Transform(point);
            transformedPoints.Add(transformed);
        }
        
        return ComputeExtent(transformedPoints, out extent);
    }
    
    protected override bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        var points = Points;
        return ComputeExtent(points, out extent);
    }
    
    #endregion
    
    #region Motion Computation
    
    public bool ComputePointsAtTime(UsdTimeCode time, UsdTimeCode baseTime, out List<GfVec3f> computedPoints)
    {
        computedPoints = new List<GfVec3f>();
        
        var positions = Points;
        if (positions.Count == 0)
            return false;
            
        var velocities = Velocities;
        var accelerations = Accelerations;
        
        return ComputePointsAtTime(
            time, baseTime, positions, velocities, accelerations, out computedPoints);
    }
    
    public static bool ComputePointsAtTime(
        UsdTimeCode time, 
        UsdTimeCode baseTime, 
        List<GfVec3f> positions, 
        List<GfVec3f> velocities, 
        List<GfVec3f> accelerations, 
        out List<GfVec3f> computedPoints)
    {
        computedPoints = new List<GfVec3f>(positions);
        
        if (time == baseTime)
            return true;
            
        var dt = (float)(time.GetValue() - baseTime.GetValue());
        
        for (int i = 0; i < positions.Count; i++)
        {
            var pos = positions[i];
            
            if (velocities.Count > i)
            {
                var vel = velocities[i];
                var velVector = vel.ToVector3() * dt;
                pos = new GfVec3f(pos.ToVector3() + velVector);
                
                if (accelerations.Count > i)
                {
                    var acc = accelerations[i];
                    var accVector = acc.ToVector3() * (0.5f * dt * dt);
                    pos = new GfVec3f(pos.ToVector3() + accVector);
                }
            }
            
            computedPoints[i] = pos;
        }
        
        return true;
    }
    
    #endregion
    
    #region Utility Methods
    
    public int GetPointCount(UsdTimeCode time = default)
    {
        var points = Points;
        return points.Count;
    }
    
    public bool HasVelocities()
    {
        var attr = GetVelocitiesAttr();
        return attr.IsValid() && attr.HasValue();
    }
    
    public bool HasAccelerations()
    {
        var attr = GetAccelerationsAttr();
        return attr.IsValid() && attr.HasValue();
    }
    
    public bool HasNormals()
    {
        var attr = GetNormalsAttr();
        return attr.IsValid() && attr.HasValue();
    }
    
    #endregion
}