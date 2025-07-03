using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace Pxr.Usd.UsdSkel;

[UsdSchema("SkelRoot", UsdSchemaKind.ConcreteTyped)]
public class UsdSkelRoot : UsdGeomBoundable
{
    #region Construction
    
    public UsdSkelRoot(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdSkelRoot() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("SkelRoot");
    protected override TfToken GetTypeName() => new TfToken("SkelRoot");
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdSkelRoot Get(UsdStage stage, SdfPath path)
    {
        return Get<UsdSkelRoot>(stage, path);
    }
    
    public new static UsdSkelRoot Define(UsdStage stage, SdfPath path)
    {
        return Define<UsdSkelRoot>(stage, path);
    }
    
    #endregion
    
    #region SkelRoot Discovery
    
    /// <summary>
    /// Find the skel root at or above the given prim.
    /// Returns the nearest ancestor prim (including the prim itself) that is a SkelRoot.
    /// </summary>
    public static UsdSkelRoot Find(UsdPrim prim)
    {
        if (!prim.IsValid())
            return new UsdSkelRoot();
            
        // Walk up the hierarchy looking for a SkelRoot
        var currentPrim = prim;
        while (currentPrim.IsValid())
        {
            if (currentPrim.IsA<UsdSkelRoot>())
                return new UsdSkelRoot(currentPrim);
                
            currentPrim = currentPrim.GetParent();
        }
        
        return new UsdSkelRoot(); // Invalid
    }
    
    /// <summary>
    /// Check if the given prim has a SkelRoot at or above it in the hierarchy.
    /// </summary>
    public static bool HasSkelRoot(UsdPrim prim)
    {
        return Find(prim).IsValid;
    }
    
    #endregion
    
    #region Skeleton Discovery
    
    /// <summary>
    /// Find all skeletons beneath this SkelRoot.
    /// </summary>
    public List<UsdSkelSkeleton> GetSkeletons()
    {
        var skeletons = new List<UsdSkelSkeleton>();
        
        if (!IsValid)
            return skeletons;
            
        // Traverse all descendants looking for skeletons
        foreach (var descendant in new UsdPrimRange(Prim))
        {
            if (descendant.IsA<UsdSkelSkeleton>())
            {
                skeletons.Add(new UsdSkelSkeleton(descendant));
            }
        }
        
        return skeletons;
    }
    
    /// <summary>
    /// Find the first skeleton beneath this SkelRoot.
    /// </summary>
    public UsdSkelSkeleton GetSkeleton()
    {
        var skeletons = GetSkeletons();
        return skeletons.Count > 0 ? skeletons[0] : new UsdSkelSkeleton();
    }
    
    #endregion
    
    #region Skinned Primitive Discovery
    
    /// <summary>
    /// Find all skinned primitives (prims with SkelBindingAPI) beneath this SkelRoot.
    /// </summary>
    public List<UsdPrim> GetSkinnedPrims()
    {
        var skinnedPrims = new List<UsdPrim>();
        
        if (!IsValid)
            return skinnedPrims;
            
        // Traverse all descendants looking for prims with SkelBindingAPI
        foreach (var descendant in new UsdPrimRange(Prim))
        {
            if (UsdSkelBindingAPI.CanApply(descendant))
            {
                var bindingAPI = UsdSkelBindingAPI.Get(descendant);
                if (bindingAPI.IsValid)
                {
                    skinnedPrims.Add(descendant);
                }
            }
        }
        
        return skinnedPrims;
    }
    
    #endregion
    
    #region Animation Discovery
    
    /// <summary>
    /// Find all animations beneath this SkelRoot.
    /// </summary>
    public List<UsdSkelAnimation> GetAnimations()
    {
        var animations = new List<UsdSkelAnimation>();
        
        if (!IsValid)
            return animations;
            
        // Traverse all descendants looking for animations
        foreach (var descendant in new UsdPrimRange(Prim))
        {
            if (descendant.IsA<UsdSkelAnimation>())
            {
                animations.Add(new UsdSkelAnimation(descendant));
            }
        }
        
        return animations;
    }
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validate that this SkelRoot is properly configured.
    /// </summary>
    public (bool IsValid, string Reason) ValidateSkelRoot()
    {
        if (!IsValid)
            return (false, "SkelRoot prim is invalid");
            
        var skeletons = GetSkeletons();
        if (skeletons.Count == 0)
            return (false, "SkelRoot contains no skeletons");
            
        // Check that all skeletons are valid
        foreach (var skeleton in skeletons)
        {
            var (isValid, reason) = skeleton.ValidateSkeleton();
            if (!isValid)
                return (false, $"Invalid skeleton at {skeleton.Prim.GetPath()}: {reason}");
        }
        
        // Check that skinned prims have valid bindings
        var skinnedPrims = GetSkinnedPrims();
        foreach (var skinnedPrim in skinnedPrims)
        {
            var bindingAPI = UsdSkelBindingAPI.Get(skinnedPrim);
            var (isValid, reason) = bindingAPI.ValidateBinding();
            if (!isValid)
                return (false, $"Invalid binding at {skinnedPrim.GetPath()}: {reason}");
        }
        
        return (true, string.Empty);
    }
    
    #endregion
    
    #region Game Asset Helpers
    
    /// <summary>
    /// Create a complete character setup for game assets.
    /// </summary>
    public UsdSkelSkeleton CreateCharacterSetup(
        string skeletonName = "Skeleton",
        string animationName = "Animation")
    {
        if (!IsValid)
            return new UsdSkelSkeleton();
            
        var stage = Prim.GetStage();
        if (stage == null)
            return new UsdSkelSkeleton();
            
        var skelPath = Prim.GetPath().AppendChild(new TfToken(skeletonName));
        var skeleton = UsdSkelSkeleton.Define(stage, skelPath);
        
        if (!string.IsNullOrEmpty(animationName))
        {
            var animPath = skelPath.AppendChild(new TfToken(animationName));
            var animation = UsdSkelAnimation.Define(stage, animPath);
            
            // Link skeleton to animation
            skeleton.CreateAnimationSourceRel().AddTarget(animPath);
        }
        
        return skeleton;
    }
    
    /// <summary>
    /// Configure this SkelRoot for typical game asset workflows.
    /// </summary>
    public void ConfigureForGameAsset()
    {
        if (!IsValid)
            return;
            
        // Set up common metadata for game assets
        Prim.SetMetadata(new TfToken("kind"), "component");
        
        // Ensure proper purpose classification
        CreatePurposeAttr().Set("default");
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if this SkelRoot contains any skeletal data.
    /// </summary>
    public bool HasSkeletalData()
    {
        return GetSkeletons().Count > 0 || GetAnimations().Count > 0;
    }
    
    /// <summary>
    /// Get statistics about this SkelRoot.
    /// </summary>
    public (int SkeletonCount, int AnimationCount, int SkinnedPrimCount) GetStatistics()
    {
        var skeletonCount = GetSkeletons().Count;
        var animationCount = GetAnimations().Count;
        var skinnedPrimCount = GetSkinnedPrims().Count;
        
        return (skeletonCount, animationCount, skinnedPrimCount);
    }
    
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid)
            return "Invalid UsdSkelRoot";
            
        var (skeletonCount, animationCount, skinnedPrimCount) = GetStatistics();
        return $"UsdSkelRoot '{Prim.GetPath()}' ({skeletonCount} skeletons, {animationCount} animations, {skinnedPrimCount} skinned prims)";
    }
    
    #endregion
}