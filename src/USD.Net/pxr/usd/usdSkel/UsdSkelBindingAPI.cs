using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace Pxr.Usd.UsdSkel;

/// <summary>
/// API schema for binding mesh geometry to skeletons.
/// </summary>
public class UsdSkelBindingAPI : UsdAPISchemaBase
{
    #region Construction
    
    public UsdSkelBindingAPI(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdSkelBindingAPI() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.NonAppliedAPI;
    protected override TfToken GetSchemaTypeName() => new TfToken("SkelBindingAPI");
    
    #endregion
    
    #region API Application
    
    /// <summary>
    /// Check if SkelBindingAPI can be applied to the given prim.
    /// </summary>
    public static bool CanApply(UsdPrim prim)
    {
        if (!prim.IsValid())
            return false;
            
        // Can apply to any geometric primitive
        // For now, accept any valid prim since IsA<> checking may not be fully implemented
        var typeName = prim.GetTypeName();
        return typeName == "Mesh" || typeName == "Sphere" || typeName == "Cube" || 
               typeName == "Capsule" || typeName == "Cylinder" || typeName == "Cone" ||
               typeName == "Plane" || typeName == "Points" || prim.IsA<UsdGeomImageable>();
    }
    
    /// <summary>
    /// Apply SkelBindingAPI to the given prim.
    /// </summary>
    public static UsdSkelBindingAPI Apply(UsdPrim prim)
    {
        if (!CanApply(prim))
            return new UsdSkelBindingAPI();
            
        return new UsdSkelBindingAPI(prim);
    }
    
    /// <summary>
    /// Get SkelBindingAPI from the given prim.
    /// </summary>
    public static UsdSkelBindingAPI Get(UsdPrim prim)
    {
        return new UsdSkelBindingAPI(prim);
    }
    
    /// <summary>
    /// Get SkelBindingAPI from stage and path.
    /// </summary>
    public static UsdSkelBindingAPI Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return Get(prim);
    }
    
    #endregion
    
    #region Joint Binding Data
    
    /// <summary>
    /// Get the joint indices primvar.
    /// </summary>
    public UsdGeomPrimvar GetJointIndicesPrimvar()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.GetPrimvar(new TfToken("skel:jointIndices"));
    }
    
    /// <summary>
    /// Create the joint indices primvar.
    /// </summary>
    public UsdGeomPrimvar CreateJointIndicesPrimvar(UsdGeomInterpolation interpolation = UsdGeomInterpolation.Vertex, int elementSize = 4)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.CreatePrimvar(new TfToken("skel:jointIndices"), "int[]", interpolation);
        primvar.SetElementSize(elementSize);
        return primvar;
    }
    
    /// <summary>
    /// Get the joint weights primvar.
    /// </summary>
    public UsdGeomPrimvar GetJointWeightsPrimvar()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.GetPrimvar(new TfToken("skel:jointWeights"));
    }
    
    /// <summary>
    /// Create the joint weights primvar.
    /// </summary>
    public UsdGeomPrimvar CreateJointWeightsPrimvar(UsdGeomInterpolation interpolation = UsdGeomInterpolation.Vertex, int elementSize = 4)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.CreatePrimvar(new TfToken("skel:jointWeights"), "float[]", interpolation);
        primvar.SetElementSize(elementSize);
        return primvar;
    }
    
    /// <summary>
    /// Set joint indices and weights for skinning.
    /// </summary>
    public bool SetJointInfluences(
        List<List<int>> jointIndices, 
        List<List<float>> jointWeights, 
        UsdTimeCode time = default)
    {
        if (jointIndices.Count != jointWeights.Count)
            return false;
            
        // Flatten the data and determine element size
        var flatIndices = new List<int>();
        var flatWeights = new List<float>();
        var elementSize = 0;
        
        foreach (var indices in jointIndices)
        {
            elementSize = Math.Max(elementSize, indices.Count);
        }
        
        foreach (var weights in jointWeights)
        {
            elementSize = Math.Max(elementSize, weights.Count);
        }
        
        // Flatten with padding
        for (int i = 0; i < jointIndices.Count; i++)
        {
            var indices = jointIndices[i];
            var weights = jointWeights[i];
            
            // Pad to element size
            for (int j = 0; j < elementSize; j++)
            {
                flatIndices.Add(j < indices.Count ? indices[j] : 0);
                flatWeights.Add(j < weights.Count ? weights[j] : 0.0f);
            }
        }
        
        // Create or get primvars
        var indicesPrimvar = GetJointIndicesPrimvar();
        if (!indicesPrimvar.IsDefined())
        {
            indicesPrimvar = CreateJointIndicesPrimvar(UsdGeomInterpolation.Vertex, elementSize);
        }
        
        var weightsPrimvar = GetJointWeightsPrimvar();
        if (!weightsPrimvar.IsDefined())
        {
            weightsPrimvar = CreateJointWeightsPrimvar(UsdGeomInterpolation.Vertex, elementSize);
        }
        
        // Set the data
        var success = true;
        success &= indicesPrimvar.Set(flatIndices, time);
        success &= weightsPrimvar.Set(flatWeights, time);
        
        return success;
    }
    
    /// <summary>
    /// Get joint indices and weights for skinning.
    /// </summary>
    public (List<List<int>> JointIndices, List<List<float>> JointWeights) GetJointInfluences(UsdTimeCode time = default)
    {
        var jointIndices = new List<List<int>>();
        var jointWeights = new List<List<float>>();
        
        var indicesPrimvar = GetJointIndicesPrimvar();
        var weightsPrimvar = GetJointWeightsPrimvar();
        
        if (!indicesPrimvar.IsDefined() || !weightsPrimvar.IsDefined())
            return (jointIndices, jointWeights);
            
        if (!indicesPrimvar.Get(out List<int> flatIndices, time) || 
            !weightsPrimvar.Get(out List<float> flatWeights, time))
            return (jointIndices, jointWeights);
            
        var elementSize = indicesPrimvar.GetElementSize();
        if (elementSize <= 0 || flatIndices.Count != flatWeights.Count)
            return (jointIndices, jointWeights);
            
        // Unflatten the data
        var vertexCount = flatIndices.Count / elementSize;
        for (int i = 0; i < vertexCount; i++)
        {
            var indices = new List<int>();
            var weights = new List<float>();
            
            for (int j = 0; j < elementSize; j++)
            {
                var index = i * elementSize + j;
                indices.Add(flatIndices[index]);
                weights.Add(flatWeights[index]);
            }
            
            jointIndices.Add(indices);
            jointWeights.Add(weights);
        }
        
        return (jointIndices, jointWeights);
    }
    
    #endregion
    
    #region Geometry Transform
    
    /// <summary>
    /// Get the geometry bind transform primvar.
    /// </summary>
    public UsdGeomPrimvar GetGeomBindTransformPrimvar()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.GetPrimvar(new TfToken("skel:geomBindTransform"));
    }
    
    /// <summary>
    /// Create the geometry bind transform primvar.
    /// </summary>
    public UsdGeomPrimvar CreateGeomBindTransformPrimvar()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.CreatePrimvar(new TfToken("skel:geomBindTransform"), "matrix4d", UsdGeomInterpolation.Constant);
    }
    
    /// <summary>
    /// Set the geometry bind transform.
    /// </summary>
    public bool SetGeomBindTransform(GfMatrix4d transform, UsdTimeCode time = default)
    {
        var primvar = GetGeomBindTransformPrimvar();
        if (!primvar.IsDefined())
        {
            primvar = CreateGeomBindTransformPrimvar();
        }
        
        return primvar.Set(transform, time);
    }
    
    /// <summary>
    /// Get the geometry bind transform.
    /// </summary>
    public GfMatrix4d GetGeomBindTransform(UsdTimeCode time = default)
    {
        var primvar = GetGeomBindTransformPrimvar();
        if (primvar.IsDefined() && primvar.Get(out GfMatrix4d transform, time))
            return transform;
        return GfMatrix4d.Identity;
    }
    
    #endregion
    
    #region Skinning Method
    
    /// <summary>
    /// Get the skinning method primvar.
    /// </summary>
    public UsdGeomPrimvar GetSkinningMethodPrimvar()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.GetPrimvar(new TfToken("skel:skinningMethod"));
    }
    
    /// <summary>
    /// Create the skinning method primvar.
    /// </summary>
    public UsdGeomPrimvar CreateSkinningMethodPrimvar()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.CreatePrimvar(new TfToken("skel:skinningMethod"), "token", UsdGeomInterpolation.Constant);
    }
    
    /// <summary>
    /// Set the skinning method ("classicLinear" or "dualQuaternion").
    /// </summary>
    public bool SetSkinningMethod(string method)
    {
        if (method != "classicLinear" && method != "dualQuaternion")
            return false;
            
        var primvar = GetSkinningMethodPrimvar();
        if (!primvar.IsDefined())
        {
            primvar = CreateSkinningMethodPrimvar();
        }
        
        return primvar.Set(method);
    }
    
    /// <summary>
    /// Get the skinning method.
    /// </summary>
    public string GetSkinningMethod()
    {
        var primvar = GetSkinningMethodPrimvar();
        if (primvar.IsDefined() && primvar.Get(out string method))
            return method;
        return "classicLinear"; // Default
    }
    
    #endregion
    
    #region Joint Subset
    
    /// <summary>
    /// Get the joints attribute (subset of skeleton joints).
    /// </summary>
    public UsdAttribute GetJointsAttr()
    {
        return GetAttribute(new TfToken("skel:joints"));
    }
    
    /// <summary>
    /// Create the joints attribute.
    /// </summary>
    public UsdAttribute CreateJointsAttr()
    {
        return CreateAttribute(
            new TfToken("skel:joints"), 
            "token[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    /// <summary>
    /// Get or set the joint subset for this binding.
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
    
    #region Relationships
    
    /// <summary>
    /// Get the skeleton relationship.
    /// </summary>
    public UsdRelationship GetSkeletonRel()
    {
        return GetRelationship(new TfToken("skel:skeleton"));
    }
    
    /// <summary>
    /// Create the skeleton relationship.
    /// </summary>
    public UsdRelationship CreateSkeletonRel()
    {
        return CreateRelationship(new TfToken("skel:skeleton"), false);
    }
    
    /// <summary>
    /// Get the bound skeleton.
    /// </summary>
    public UsdSkelSkeleton GetSkeleton()
    {
        var rel = GetSkeletonRel();
        if (rel.IsValid())
        {
            var targets = rel.GetTargets();
            if (targets.Length > 0)
            {
                var stage = Prim.GetStage();
                if (stage != null)
                {
                    var skelPrim = stage.GetPrimAtPath(targets[0]);
                    if (skelPrim.IsA<UsdSkelSkeleton>())
                        return new UsdSkelSkeleton(skelPrim);
                }
            }
        }
        return new UsdSkelSkeleton();
    }
    
    /// <summary>
    /// Bind to a skeleton.
    /// </summary>
    public bool BindSkeleton(UsdSkelSkeleton skeleton)
    {
        if (!skeleton.IsValid)
            return false;
            
        var rel = CreateSkeletonRel();
        return rel.SetTargets(new[] { skeleton.Prim.GetPath() });
    }
    
    /// <summary>
    /// Get the animation source relationship.
    /// </summary>
    public UsdRelationship GetAnimationSourceRel()
    {
        return GetRelationship(new TfToken("skel:animationSource"));
    }
    
    /// <summary>
    /// Create the animation source relationship.
    /// </summary>
    public UsdRelationship CreateAnimationSourceRel()
    {
        return CreateRelationship(new TfToken("skel:animationSource"), false);
    }
    
    /// <summary>
    /// Get the bound animation source.
    /// </summary>
    public UsdSkelAnimation GetAnimationSource()
    {
        var rel = GetAnimationSourceRel();
        if (rel.IsValid())
        {
            var targets = rel.GetTargets();
            if (targets.Length > 0)
            {
                var stage = Prim.GetStage();
                if (stage != null)
                {
                    var animPrim = stage.GetPrimAtPath(targets[0]);
                    if (animPrim.IsA<UsdSkelAnimation>())
                        return new UsdSkelAnimation(animPrim);
                }
            }
        }
        return new UsdSkelAnimation();
    }
    
    /// <summary>
    /// Bind to an animation source.
    /// </summary>
    public bool BindAnimationSource(UsdSkelAnimation animation)
    {
        if (!animation.IsValid)
            return false;
            
        var rel = CreateAnimationSourceRel();
        return rel.SetTargets(new[] { animation.Prim.GetPath() });
    }
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validate the binding configuration.
    /// </summary>
    public (bool IsValid, string Reason) ValidateBinding()
    {
        if (!IsValid)
            return (false, "Binding prim is invalid");
            
        // Check that we have a skeleton
        var skeleton = GetSkeleton();
        if (!skeleton.IsValid)
            return (false, "No skeleton bound");
            
        // Check joint data consistency
        var indicesPrimvar = GetJointIndicesPrimvar();
        var weightsPrimvar = GetJointWeightsPrimvar();
        
        if (!indicesPrimvar.IsDefined() || !weightsPrimvar.IsDefined())
            return (false, "Missing joint indices or weights");
            
        var indicesElementSize = indicesPrimvar.GetElementSize();
        var weightsElementSize = weightsPrimvar.GetElementSize();
        
        if (indicesElementSize != weightsElementSize)
            return (false, $"Joint indices element size ({indicesElementSize}) != weights element size ({weightsElementSize})");
            
        // Check that joint subset is valid
        var joints = Joints;
        var skelJoints = skeleton.Joints;
        
        foreach (var joint in joints)
        {
            if (!skelJoints.Contains(joint))
                return (false, $"Joint '{joint}' not found in skeleton");
        }
        
        return (true, string.Empty);
    }
    
    #endregion
    
    #region Game Asset Helpers
    
    /// <summary>
    /// Set up basic binding for a game mesh.
    /// </summary>
    public bool SetupGameMeshBinding(
        UsdSkelSkeleton skeleton,
        List<List<int>> jointIndices,
        List<List<float>> jointWeights,
        GfMatrix4d? bindTransform = null)
    {
        if (!skeleton.IsValid)
            return false;
            
        // Bind to skeleton
        if (!BindSkeleton(skeleton))
            return false;
            
        // Set joint influences
        if (!SetJointInfluences(jointIndices, jointWeights))
            return false;
            
        // Set bind transform
        var transform = bindTransform ?? GfMatrix4d.Identity;
        if (!SetGeomBindTransform(transform))
            return false;
            
        // Set default skinning method
        SetSkinningMethod("classicLinear");
        
        // Use all skeleton joints by default
        Joints = skeleton.Joints;
        
        return true;
    }
    
    /// <summary>
    /// Create simple rigid binding (one joint per vertex).
    /// </summary>
    public bool CreateRigidBinding(UsdSkelSkeleton skeleton, List<int> jointIndices)
    {
        if (!skeleton.IsValid || jointIndices.Count == 0)
            return false;
            
        var vertexJointIndices = new List<List<int>>();
        var vertexJointWeights = new List<List<float>>();
        
        foreach (var jointIndex in jointIndices)
        {
            vertexJointIndices.Add(new List<int> { jointIndex });
            vertexJointWeights.Add(new List<float> { 1.0f });
        }
        
        return SetupGameMeshBinding(skeleton, vertexJointIndices, vertexJointWeights);
    }
    
    /// <summary>
    /// Normalize joint weights to sum to 1.0 per vertex.
    /// </summary>
    public static void NormalizeWeights(List<List<float>> jointWeights)
    {
        foreach (var weights in jointWeights)
        {
            var sum = weights.Sum();
            if (sum > 0.0001f) // Avoid division by zero
            {
                for (int i = 0; i < weights.Count; i++)
                {
                    weights[i] /= sum;
                }
            }
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if this prim has valid binding data.
    /// </summary>
    public bool HasValidBinding()
    {
        var (isValid, _) = ValidateBinding();
        return isValid;
    }
    
    /// <summary>
    /// Get binding statistics.
    /// </summary>
    public (int VertexCount, int MaxInfluences, bool HasSkeleton, bool HasAnimation) GetStatistics()
    {
        var (jointIndices, jointWeights) = GetJointInfluences();
        var vertexCount = jointIndices.Count;
        var maxInfluences = jointIndices.Count > 0 ? jointIndices[0].Count : 0;
        var hasSkeleton = GetSkeleton().IsValid;
        var hasAnimation = GetAnimationSource().IsValid;
        
        return (vertexCount, maxInfluences, hasSkeleton, hasAnimation);
    }
    
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid)
            return "Invalid UsdSkelBindingAPI";
            
        var (vertexCount, maxInfluences, hasSkeleton, hasAnimation) = GetStatistics();
        return $"UsdSkelBindingAPI '{Prim.GetPath()}' ({vertexCount} vertices, {maxInfluences} max influences, skel:{hasSkeleton}, anim:{hasAnimation})";
    }
    
    #endregion
}