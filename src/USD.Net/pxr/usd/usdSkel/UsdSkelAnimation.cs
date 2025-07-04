using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace Pxr.Usd.UsdSkel;

/// <summary>
/// Represents a quaternion rotation.
/// Compatible with USD's quatf type.
/// </summary>
public struct GfQuatf : IEquatable<GfQuatf>
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }
    
    public GfQuatf(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
    
    public GfQuatf(Quaternion q)
    {
        X = q.X;
        Y = q.Y;
        Z = q.Z;
        W = q.W;
    }
    
    public Quaternion ToQuaternion() => new Quaternion(X, Y, Z, W);
    
    public static implicit operator Quaternion(GfQuatf q) => q.ToQuaternion();
    public static implicit operator GfQuatf(Quaternion q) => new GfQuatf(q);
    
    public static GfQuatf Identity => new GfQuatf(0, 0, 0, 1);
    
    public bool Equals(GfQuatf other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is GfQuatf other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
    
    public static bool operator ==(GfQuatf left, GfQuatf right) => left.Equals(right);
    public static bool operator !=(GfQuatf left, GfQuatf right) => !left.Equals(right);
}

[UsdSchema("SkelAnimation", UsdSchemaKind.ConcreteTyped, TypeName = "SkelAnimation")]
public class UsdSkelAnimation : UsdTyped
{
    #region Construction
    
    public UsdSkelAnimation(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdSkelAnimation() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("SkelAnimation");
    protected override TfToken GetTypeName() => new TfToken("SkelAnimation");
    
    #endregion
    
    #region Joint Order and Mapping
    
    /// <summary>
    /// Get the joints attribute, which defines the joint order for animation data.
    /// </summary>
    public UsdAttribute GetJointsAttr()
    {
        return GetAttribute(new TfToken("joints"));
    }
    
    /// <summary>
    /// Create the joints attribute.
    /// </summary>
    public UsdAttribute CreateJointsAttr()
    {
        return CreateAttribute(
            new TfToken("joints"), 
            "token[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    /// <summary>
    /// Get or set the joint order for animation data.
    /// This may differ from the skeleton's joint order.
    /// </summary>
    public List<string> Joints
    {
        get
        {
            var attr = GetJointsAttr();
            if (attr.IsValid() && attr.Get(out List<string> value))
                return value;
            return new List<string>();
        }
        set
        {
            var attr = CreateJointsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Transform Animation Data
    
    /// <summary>
    /// Get the translations attribute (joint-local translations).
    /// </summary>
    public UsdAttribute GetTranslationsAttr()
    {
        return GetAttribute(new TfToken("translations"));
    }
    
    /// <summary>
    /// Create the translations attribute.
    /// </summary>
    public UsdAttribute CreateTranslationsAttr()
    {
        return CreateAttribute(
            new TfToken("translations"), 
            "float3[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Get the rotations attribute (joint-local rotations as quaternions).
    /// </summary>
    public UsdAttribute GetRotationsAttr()
    {
        return GetAttribute(new TfToken("rotations"));
    }
    
    /// <summary>
    /// Create the rotations attribute.
    /// </summary>
    public UsdAttribute CreateRotationsAttr()
    {
        return CreateAttribute(
            new TfToken("rotations"), 
            "quatf[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Get the scales attribute (joint-local scales).
    /// </summary>
    public UsdAttribute GetScalesAttr()
    {
        return GetAttribute(new TfToken("scales"));
    }
    
    /// <summary>
    /// Create the scales attribute.
    /// </summary>
    public UsdAttribute CreateScalesAttr()
    {
        return CreateAttribute(
            new TfToken("scales"), 
            "half3[]", 
            false, 
            SdfVariability.Varying);
    }
    
    #endregion
    
    #region Transform Data Access
    
    /// <summary>
    /// Set translations at the given time.
    /// </summary>
    public bool SetTranslations(List<GfVec3f> translations, UsdTimeCode time = default)
    {
        var attr = GetTranslationsAttr();
        if (!attr.IsValid())
            attr = CreateTranslationsAttr();
        return attr.Set(new VtValue(translations), time);
    }
    
    /// <summary>
    /// Get translations at the given time.
    /// </summary>
    public List<GfVec3f> GetTranslations(UsdTimeCode time = default)
    {
        var attr = GetTranslationsAttr();
        if (attr.IsValid() && attr.Get(out List<GfVec3f> value, time))
            return value;
        return new List<GfVec3f>();
    }
    
    /// <summary>
    /// Set rotations at the given time.
    /// </summary>
    public bool SetRotations(List<GfQuatf> rotations, UsdTimeCode time = default)
    {
        var attr = GetRotationsAttr();
        if (!attr.IsValid())
            attr = CreateRotationsAttr();
        return attr.Set(new VtValue(rotations), time);
    }
    
    /// <summary>
    /// Get rotations at the given time.
    /// </summary>
    public List<GfQuatf> GetRotations(UsdTimeCode time = default)
    {
        var attr = GetRotationsAttr();
        if (attr.IsValid() && attr.Get(out List<GfQuatf> value, time))
            return value;
        return new List<GfQuatf>();
    }
    
    /// <summary>
    /// Set scales at the given time.
    /// </summary>
    public bool SetScales(List<GfVec3f> scales, UsdTimeCode time = default)
    {
        var attr = GetScalesAttr();
        if (!attr.IsValid())
            attr = CreateScalesAttr();
        return attr.Set(new VtValue(scales), time);
    }
    
    /// <summary>
    /// Get scales at the given time.
    /// </summary>
    public List<GfVec3f> GetScales(UsdTimeCode time = default)
    {
        var attr = GetScalesAttr();
        if (attr.IsValid() && attr.Get(out List<GfVec3f> value, time))
            return value;
        return new List<GfVec3f>();
    }
    
    #endregion
    
    #region Convenience Transform Methods
    
    /// <summary>
    /// Set complete TRS data for all joints at the given time.
    /// </summary>
    public bool SetTransforms(
        List<GfVec3f> translations,
        List<GfQuatf> rotations,
        List<GfVec3f> scales,
        UsdTimeCode time = default)
    {
        var jointCount = Joints.Count;
        
        // Validate input sizes
        if (translations.Count != jointCount || 
            rotations.Count != jointCount || 
            scales.Count != jointCount)
        {
            return false;
        }
        
        var success = true;
        success &= SetTranslations(translations, time);
        success &= SetRotations(rotations, time);
        success &= SetScales(scales, time);
        
        return success;
    }
    
    /// <summary>
    /// Get complete TRS data for all joints at the given time.
    /// </summary>
    public (List<GfVec3f> Translations, List<GfQuatf> Rotations, List<GfVec3f> Scales) GetTransforms(UsdTimeCode time = default)
    {
        var translations = GetTranslations(time);
        var rotations = GetRotations(time);
        var scales = GetScales(time);
        
        return (translations, rotations, scales);
    }
    
    /// <summary>
    /// Compute world-space matrices from TRS data at the given time.
    /// Requires a skeleton to provide the joint hierarchy.
    /// </summary>
    public List<GfMatrix4d> ComputeJointTransforms(UsdSkelSkeleton skeleton, UsdTimeCode time = default)
    {
        if (!skeleton.IsValid)
            return new List<GfMatrix4d>();
            
        var (translations, rotations, scales) = GetTransforms(time);
        var jointCount = translations.Count;
        
        if (jointCount == 0 || rotations.Count != jointCount || scales.Count != jointCount)
            return new List<GfMatrix4d>();
            
        // Map animation joint order to skeleton joint order
        var skelJoints = skeleton.Joints;
        var animJoints = Joints;
        var jointMapping = new int[skelJoints.Count];
        
        for (int i = 0; i < skelJoints.Count; i++)
        {
            var skelJoint = skelJoints[i];
            var animIndex = animJoints.IndexOf(skelJoint);
            jointMapping[i] = animIndex;
        }
        
        // Compute local matrices from TRS
        var localTransforms = new List<GfMatrix4d>();
        for (int i = 0; i < skelJoints.Count; i++)
        {
            var animIndex = jointMapping[i];
            if (animIndex >= 0)
            {
                var t = translations[animIndex];
                var r = rotations[animIndex];
                var s = scales[animIndex];
                
                var matrix = GfMatrix4d.CreateScale(s) * 
                            GfMatrix4d.CreateFromQuaternion(r.ToQuaternion()) * 
                            GfMatrix4d.CreateTranslation(t);
                localTransforms.Add(matrix);
            }
            else
            {
                // Use rest pose if no animation data
                var restTransforms = skeleton.RestTransforms;
                if (i < restTransforms.Count)
                    localTransforms.Add(restTransforms[i]);
                else
                    localTransforms.Add(GfMatrix4d.Identity);
            }
        }
        
        // Compute world-space transforms from local transforms
        var worldTransforms = new List<GfMatrix4d>();
        var parentIndices = skeleton.GetJointParentIndices();
        
        for (int i = 0; i < localTransforms.Count; i++)
        {
            var parentIndex = parentIndices[i];
            if (parentIndex == -1)
            {
                // Root joint
                worldTransforms.Add(localTransforms[i]);
            }
            else
            {
                // Child joint - multiply by parent
                var parentTransform = worldTransforms[parentIndex];
                var worldTransform = localTransforms[i] * parentTransform;
                worldTransforms.Add(worldTransform);
            }
        }
        
        return worldTransforms;
    }
    
    #endregion
    
    #region Blend Shape Animation
    
    /// <summary>
    /// Get the blend shapes attribute.
    /// </summary>
    public UsdAttribute GetBlendShapesAttr()
    {
        return GetAttribute(new TfToken("blendShapes"));
    }
    
    /// <summary>
    /// Create the blend shapes attribute.
    /// </summary>
    public UsdAttribute CreateBlendShapesAttr()
    {
        return CreateAttribute(
            new TfToken("blendShapes"), 
            "token[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    /// <summary>
    /// Get or set the blend shape identifiers.
    /// </summary>
    public List<string> BlendShapes
    {
        get
        {
            var attr = GetBlendShapesAttr();
            if (attr.IsValid() && attr.Get(out List<string> value))
                return value;
            return new List<string>();
        }
        set
        {
            var attr = CreateBlendShapesAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    /// <summary>
    /// Get the blend shape weights attribute.
    /// </summary>
    public UsdAttribute GetBlendShapeWeightsAttr()
    {
        return GetAttribute(new TfToken("blendShapeWeights"));
    }
    
    /// <summary>
    /// Create the blend shape weights attribute.
    /// </summary>
    public UsdAttribute CreateBlendShapeWeightsAttr()
    {
        return CreateAttribute(
            new TfToken("blendShapeWeights"), 
            "float[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Set blend shape weights at the given time.
    /// </summary>
    public bool SetBlendShapeWeights(List<float> weights, UsdTimeCode time = default)
    {
        var attr = CreateBlendShapeWeightsAttr();
        return attr.Set(new VtValue(weights), time);
    }
    
    /// <summary>
    /// Get blend shape weights at the given time.
    /// </summary>
    public List<float> GetBlendShapeWeights(UsdTimeCode time = default)
    {
        var attr = GetBlendShapeWeightsAttr();
        if (attr.IsValid() && attr.Get(out List<float> value, time))
            return value;
        return new List<float>();
    }
    
    #endregion
    
    #region Time Sampling
    
    /// <summary>
    /// Get all time samples for transform animation.
    /// </summary>
    public List<double> GetTimeSamples()
    {
        var times = new HashSet<double>();
        
        // Collect time samples from all transform attributes
        var transAttr = GetTranslationsAttr();
        if (transAttr.IsValid())
        {
            foreach (var time in transAttr.GetTimeSamples())
                times.Add(time);
        }
        
        var rotAttr = GetRotationsAttr();
        if (rotAttr.IsValid())
        {
            foreach (var time in rotAttr.GetTimeSamples())
                times.Add(time);
        }
        
        var scaleAttr = GetScalesAttr();
        if (scaleAttr.IsValid())
        {
            foreach (var time in scaleAttr.GetTimeSamples())
                times.Add(time);
        }
        
        var weightsAttr = GetBlendShapeWeightsAttr();
        if (weightsAttr.IsValid())
        {
            foreach (var time in weightsAttr.GetTimeSamples())
                times.Add(time);
        }
        
        var sortedTimes = times.ToList();
        sortedTimes.Sort();
        return sortedTimes;
    }
    
    /// <summary>
    /// Get the time range for this animation.
    /// </summary>
    public (double StartTime, double EndTime) GetTimeRange()
    {
        var times = GetTimeSamples();
        if (times.Count == 0)
            return (0, 0);
        return (times[0], times[times.Count - 1]);
    }
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validate the animation data consistency.
    /// </summary>
    public (bool IsValid, string Reason) ValidateAnimation()
    {
        if (!IsValid)
            return (false, "Animation prim is invalid");
            
        var joints = Joints;
        if (joints.Count == 0)
            return (false, "Animation has no joints");
            
        // Check that all transform data arrays have consistent sizes
        var times = GetTimeSamples();
        foreach (var time in times)
        {
            var translations = GetTranslations(UsdTimeCode.Create(time));
            var rotations = GetRotations(UsdTimeCode.Create(time));
            var scales = GetScales(UsdTimeCode.Create(time));
            
            if (translations.Count > 0 && translations.Count != joints.Count)
                return (false, $"Translations count ({translations.Count}) != joints count ({joints.Count}) at time {time}");
                
            if (rotations.Count > 0 && rotations.Count != joints.Count)
                return (false, $"Rotations count ({rotations.Count}) != joints count ({joints.Count}) at time {time}");
                
            if (scales.Count > 0 && scales.Count != joints.Count)
                return (false, $"Scales count ({scales.Count}) != joints count ({joints.Count}) at time {time}");
        }
        
        // Check blend shape data consistency
        var blendShapes = BlendShapes;
        if (blendShapes.Count > 0)
        {
            foreach (var time in times)
            {
                var weights = GetBlendShapeWeights(UsdTimeCode.Create(time));
                if (weights.Count > 0 && weights.Count != blendShapes.Count)
                    return (false, $"Blend shape weights count ({weights.Count}) != blend shapes count ({blendShapes.Count}) at time {time}");
            }
        }
        
        return (true, string.Empty);
    }
    
    #endregion
    
    #region Game Asset Helpers
    
    /// <summary>
    /// Create a simple idle animation with identity transforms.
    /// </summary>
    public void CreateIdleAnimation(List<string> joints, double duration = 1.0, int frameRate = 30)
    {
        Joints = joints;
        
        var translations = new List<GfVec3f>();
        var rotations = new List<GfQuatf>();
        var scales = new List<GfVec3f>();
        
        for (int i = 0; i < joints.Count; i++)
        {
            translations.Add(GfVec3f.Zero);
            rotations.Add(GfQuatf.Identity);
            scales.Add(GfVec3f.One);
        }
        
        // Set transforms for first and last frame to create a loop
        SetTransforms(translations, rotations, scales, UsdTimeCode.Create(0.0));
        SetTransforms(translations, rotations, scales, UsdTimeCode.Create(duration * frameRate));
    }
    
    /// <summary>
    /// Add a keyframe of transform data at the specified time.
    /// </summary>
    public bool AddKeyframe(
        double time,
        List<GfVec3f> translations,
        List<GfQuatf> rotations,
        List<GfVec3f> scales)
    {
        return SetTransforms(translations, rotations, scales, UsdTimeCode.Create(time));
    }
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdSkelAnimation Get(UsdStage stage, SdfPath path)
    {
        return Get<UsdSkelAnimation>(stage, path);
    }
    
    public static UsdSkelAnimation Define(UsdStage stage, SdfPath path)
    {
        return Define<UsdSkelAnimation>(stage, path);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get animation statistics.
    /// </summary>
    public (int JointCount, int BlendShapeCount, int KeyframeCount, double Duration) GetStatistics()
    {
        var jointCount = Joints.Count;
        var blendShapeCount = BlendShapes.Count;
        var times = GetTimeSamples();
        var keyframeCount = times.Count;
        var (startTime, endTime) = GetTimeRange();
        var duration = endTime - startTime;
        
        return (jointCount, blendShapeCount, keyframeCount, duration);
    }
    
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid)
            return "Invalid UsdSkelAnimation";
            
        var (jointCount, blendShapeCount, keyframeCount, duration) = GetStatistics();
        return $"UsdSkelAnimation '{Prim.GetPath()}' ({jointCount} joints, {keyframeCount} keyframes, {duration:F2}s)";
    }
    
    #endregion
}