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

[UsdSchema("Skeleton", UsdSchemaKind.ConcreteTyped, TypeName = "Skeleton")]
public class UsdSkelSkeleton : UsdGeomBoundable
{
    #region Construction
    
    public UsdSkelSkeleton(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdSkelSkeleton() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Skeleton");
    protected override TfToken GetTypeName() => new TfToken("Skeleton");
    
    #endregion
    
    #region Core Skeleton Attributes
    
    /// <summary>
    /// Get the joints attribute, which defines the joint hierarchy as an array of paths.
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
    /// Get or set the joint hierarchy as an array of joint paths.
    /// Each path defines a joint in the skeleton hierarchy.
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
    
    /// <summary>
    /// Get the joint names attribute, which provides optional unique names for joints.
    /// </summary>
    public UsdAttribute GetJointNamesAttr()
    {
        return GetAttribute(new TfToken("jointNames"));
    }
    
    /// <summary>
    /// Create the joint names attribute.
    /// </summary>
    public UsdAttribute CreateJointNamesAttr()
    {
        return CreateAttribute(
            new TfToken("jointNames"), 
            "token[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    /// <summary>
    /// Get or set the joint names array (optional alternative to joint paths).
    /// </summary>
    public List<string> JointNames
    {
        get
        {
            var attr = GetJointNamesAttr();
            if (attr.IsValid() && attr.Get(out List<string> value))
                return value;
            return new List<string>();
        }
        set
        {
            var attr = CreateJointNamesAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    /// <summary>
    /// Get the bind transforms attribute, which stores world-space bind pose matrices.
    /// </summary>
    public UsdAttribute GetBindTransformsAttr()
    {
        return GetAttribute(new TfToken("bindTransforms"));
    }
    
    /// <summary>
    /// Create the bind transforms attribute.
    /// </summary>
    public UsdAttribute CreateBindTransformsAttr()
    {
        return CreateAttribute(
            new TfToken("bindTransforms"), 
            "matrix4d[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    /// <summary>
    /// Get or set the bind transforms (world-space bind pose matrices).
    /// </summary>
    public List<GfMatrix4d> BindTransforms
    {
        get
        {
            var attr = GetBindTransformsAttr();
            if (attr.IsValid() && attr.Get(out List<GfMatrix4d> value))
                return value;
            return new List<GfMatrix4d>();
        }
        set
        {
            var attr = CreateBindTransformsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    /// <summary>
    /// Get the rest transforms attribute, which stores local-space rest pose matrices.
    /// </summary>
    public UsdAttribute GetRestTransformsAttr()
    {
        return GetAttribute(new TfToken("restTransforms"));
    }
    
    /// <summary>
    /// Create the rest transforms attribute.
    /// </summary>
    public UsdAttribute CreateRestTransformsAttr()
    {
        return CreateAttribute(
            new TfToken("restTransforms"), 
            "matrix4d[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    /// <summary>
    /// Get or set the rest transforms (local-space rest pose matrices).
    /// </summary>
    public List<GfMatrix4d> RestTransforms
    {
        get
        {
            var attr = GetRestTransformsAttr();
            if (attr.IsValid() && attr.Get(out List<GfMatrix4d> value))
                return value;
            return new List<GfMatrix4d>();
        }
        set
        {
            var attr = CreateRestTransformsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Relationships
    
    /// <summary>
    /// Get the animation source relationship.
    /// </summary>
    public UsdRelationship GetAnimationSourceRel()
    {
        return GetRelationship(new TfToken("animationSource"));
    }
    
    /// <summary>
    /// Create the animation source relationship.
    /// </summary>
    public UsdRelationship CreateAnimationSourceRel()
    {
        return CreateRelationship(new TfToken("animationSource"), false);
    }
    
    /// <summary>
    /// Get the animation source (if any).
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
    /// Set the animation source.
    /// </summary>
    public bool SetAnimationSource(UsdSkelAnimation animation)
    {
        if (!animation.IsValid)
            return false;
            
        var rel = CreateAnimationSourceRel();
        return rel.SetTargets(new[] { animation.Prim.GetPath() });
    }
    
    #endregion
    
    #region Joint Hierarchy Management
    
    /// <summary>
    /// Get the number of joints in this skeleton.
    /// </summary>
    public int GetJointCount()
    {
        return Joints.Count;
    }
    
    /// <summary>
    /// Get the parent index for each joint (-1 for root joints).
    /// </summary>
    public List<int> GetJointParentIndices()
    {
        var joints = Joints;
        var parentIndices = new List<int>();
        
        for (int i = 0; i < joints.Count; i++)
        {
            var jointPath = joints[i];
            var parentIndex = -1;
            
            // Find parent by looking for longest prefix match
            for (int j = 0; j < joints.Count; j++)
            {
                if (i == j) continue;
                
                var potentialParent = joints[j];
                if (jointPath.StartsWith(potentialParent + "/"))
                {
                    // Check if this is the immediate parent (longest match)
                    if (parentIndex == -1 || potentialParent.Length > joints[parentIndex].Length)
                    {
                        parentIndex = j;
                    }
                }
            }
            
            parentIndices.Add(parentIndex);
        }
        
        return parentIndices;
    }
    
    /// <summary>
    /// Get the children indices for a given joint.
    /// </summary>
    public List<int> GetJointChildren(int jointIndex)
    {
        var children = new List<int>();
        if (jointIndex < 0 || jointIndex >= GetJointCount())
            return children;
            
        var parentIndices = GetJointParentIndices();
        for (int i = 0; i < parentIndices.Count; i++)
        {
            if (parentIndices[i] == jointIndex)
                children.Add(i);
        }
        
        return children;
    }
    
    /// <summary>
    /// Check if a joint is a root joint (has no parent).
    /// </summary>
    public bool IsRootJoint(int jointIndex)
    {
        var parentIndices = GetJointParentIndices();
        return jointIndex >= 0 && jointIndex < parentIndices.Count && parentIndices[jointIndex] == -1;
    }
    
    /// <summary>
    /// Get all root joint indices.
    /// </summary>
    public List<int> GetRootJoints()
    {
        var roots = new List<int>();
        var parentIndices = GetJointParentIndices();
        
        for (int i = 0; i < parentIndices.Count; i++)
        {
            if (parentIndices[i] == -1)
                roots.Add(i);
        }
        
        return roots;
    }
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validate the skeleton's joint hierarchy and transform data.
    /// </summary>
    public (bool IsValid, string Reason) ValidateSkeleton()
    {
        if (!IsValid)
            return (false, "Skeleton prim is invalid");
            
        var joints = Joints;
        if (joints.Count == 0)
            return (false, "Skeleton has no joints");
            
        // Check for duplicate joint paths
        var uniqueJoints = new HashSet<string>(joints);
        if (uniqueJoints.Count != joints.Count)
            return (false, "Skeleton has duplicate joint paths");
            
        // Check that bind transforms match joint count
        var bindTransforms = BindTransforms;
        if (bindTransforms.Count > 0 && bindTransforms.Count != joints.Count)
            return (false, $"Bind transforms count ({bindTransforms.Count}) != joints count ({joints.Count})");
            
        // Check that rest transforms match joint count
        var restTransforms = RestTransforms;
        if (restTransforms.Count > 0 && restTransforms.Count != joints.Count)
            return (false, $"Rest transforms count ({restTransforms.Count}) != joints count ({joints.Count})");
            
        // Check for valid hierarchy (no cycles)
        var parentIndices = GetJointParentIndices();
        for (int i = 0; i < parentIndices.Count; i++)
        {
            var parent = parentIndices[i];
            if (parent == i)
                return (false, $"Joint {i} is its own parent");
                
            // Check for cycles by walking up the hierarchy
            var visited = new HashSet<int>();
            var current = parent;
            while (current != -1)
            {
                if (visited.Contains(current))
                    return (false, $"Cycle detected in joint hierarchy involving joint {current}");
                visited.Add(current);
                current = current < parentIndices.Count ? parentIndices[current] : -1;
            }
        }
        
        return (true, string.Empty);
    }
    
    #endregion
    
    #region Game Asset Helpers
    
    /// <summary>
    /// Create a simple bipedal skeleton for game characters.
    /// </summary>
    public void CreateBipedalSkeleton()
    {
        var joints = new List<string>
        {
            "Root",
            "Root/Hips",
            "Root/Hips/Spine",
            "Root/Hips/Spine/Chest",
            "Root/Hips/Spine/Chest/Neck",
            "Root/Hips/Spine/Chest/Neck/Head",
            "Root/Hips/Spine/Chest/LeftShoulder",
            "Root/Hips/Spine/Chest/LeftShoulder/LeftArm",
            "Root/Hips/Spine/Chest/LeftShoulder/LeftArm/LeftForearm",
            "Root/Hips/Spine/Chest/LeftShoulder/LeftArm/LeftForearm/LeftHand",
            "Root/Hips/Spine/Chest/RightShoulder",
            "Root/Hips/Spine/Chest/RightShoulder/RightArm",
            "Root/Hips/Spine/Chest/RightShoulder/RightArm/RightForearm",
            "Root/Hips/Spine/Chest/RightShoulder/RightArm/RightForearm/RightHand",
            "Root/Hips/LeftHip",
            "Root/Hips/LeftHip/LeftThigh",
            "Root/Hips/LeftHip/LeftThigh/LeftShin",
            "Root/Hips/LeftHip/LeftThigh/LeftShin/LeftFoot",
            "Root/Hips/RightHip",
            "Root/Hips/RightHip/RightThigh",
            "Root/Hips/RightHip/RightThigh/RightShin",
            "Root/Hips/RightHip/RightThigh/RightShin/RightFoot"
        };
        
        Joints = joints;
        
        // Create identity bind transforms
        var bindTransforms = new List<GfMatrix4d>();
        var restTransforms = new List<GfMatrix4d>();
        
        for (int i = 0; i < joints.Count; i++)
        {
            bindTransforms.Add(GfMatrix4d.Identity);
            restTransforms.Add(GfMatrix4d.Identity);
        }
        
        BindTransforms = bindTransforms;
        RestTransforms = restTransforms;
    }
    
    /// <summary>
    /// Set up bind pose transforms from world-space joint matrices.
    /// </summary>
    public void SetBindPose(List<GfMatrix4d> worldTransforms)
    {
        if (worldTransforms.Count != GetJointCount())
            throw new ArgumentException($"Transform count ({worldTransforms.Count}) must match joint count ({GetJointCount()})");
            
        BindTransforms = worldTransforms;
        
        // Compute local-space rest transforms
        var restTransforms = new List<GfMatrix4d>();
        var parentIndices = GetJointParentIndices();
        
        for (int i = 0; i < worldTransforms.Count; i++)
        {
            var parentIndex = parentIndices[i];
            if (parentIndex == -1)
            {
                // Root joint - rest transform is the world transform
                restTransforms.Add(worldTransforms[i]);
            }
            else
            {
                // Child joint - compute local transform relative to parent
                if (Matrix4x4.Invert(worldTransforms[parentIndex].ToMatrix4x4(), out var parentInverse))
                {
                    var localTransform = worldTransforms[i].ToMatrix4x4() * parentInverse;
                    restTransforms.Add(new GfMatrix4d(localTransform));
                }
                else
                {
                    restTransforms.Add(GfMatrix4d.Identity);
                }
            }
        }
        
        RestTransforms = restTransforms;
    }
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdSkelSkeleton Get(UsdStage stage, SdfPath path)
    {
        return Get<UsdSkelSkeleton>(stage, path);
    }
    
    public new static UsdSkelSkeleton Define(UsdStage stage, SdfPath path)
    {
        return Define<UsdSkelSkeleton>(stage, path);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get skeleton statistics.
    /// </summary>
    public (int JointCount, int RootCount, bool HasBindPose, bool HasRestPose) GetStatistics()
    {
        var jointCount = GetJointCount();
        var rootCount = GetRootJoints().Count;
        var hasBindPose = BindTransforms.Count > 0;
        var hasRestPose = RestTransforms.Count > 0;
        
        return (jointCount, rootCount, hasBindPose, hasRestPose);
    }
    
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid)
            return "Invalid UsdSkelSkeleton";
            
        var (jointCount, rootCount, hasBindPose, hasRestPose) = GetStatistics();
        return $"UsdSkelSkeleton '{Prim.GetPath()}' ({jointCount} joints, {rootCount} roots, bind:{hasBindPose}, rest:{hasRestPose})";
    }
    
    #endregion
}