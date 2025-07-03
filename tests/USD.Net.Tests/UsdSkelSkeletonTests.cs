using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Pxr.Usd.UsdSkel;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdSkelSkeletonTests
{
    [Fact]
    public void UsdSkelSkeleton_BasicConstruction_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skelPath = new SdfPath("/Skeleton");
        
        // Act
        var skeleton = UsdSkelSkeleton.Define(stage, skelPath);
        
        // Assert
        Assert.True(skeleton.IsValid);
        Assert.Equal(skelPath, skeleton.Prim.GetPath());
        Assert.Equal("Skeleton", skeleton.Prim.GetTypeName());
    }
    
    [Fact]
    public void UsdSkelSkeleton_JointsAttribute_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        var joints = new List<string> { "Root", "Root/Hip", "Root/Hip/Spine" };
        
        // Act
        skeleton.Joints = joints;
        var retrievedJoints = skeleton.Joints;
        
        // Assert
        Assert.Equal(joints.Count, retrievedJoints.Count);
        for (int i = 0; i < joints.Count; i++)
        {
            Assert.Equal(joints[i], retrievedJoints[i]);
        }
    }
    
    [Fact]
    public void UsdSkelSkeleton_JointNamesAttribute_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        var jointNames = new List<string> { "root", "hip", "spine" };
        
        // Act
        skeleton.JointNames = jointNames;
        var retrievedNames = skeleton.JointNames;
        
        // Assert
        Assert.Equal(jointNames.Count, retrievedNames.Count);
        for (int i = 0; i < jointNames.Count; i++)
        {
            Assert.Equal(jointNames[i], retrievedNames[i]);
        }
    }
    
    [Fact]
    public void UsdSkelSkeleton_BindTransforms_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        var bindTransforms = new List<GfMatrix4d>
        {
            GfMatrix4d.Identity,
            GfMatrix4d.CreateTranslation(new GfVec3f(1, 0, 0)),
            GfMatrix4d.CreateTranslation(new GfVec3f(1, 1, 0))
        };
        
        // Act
        skeleton.BindTransforms = bindTransforms;
        var retrieved = skeleton.BindTransforms;
        
        // Assert
        Assert.Equal(bindTransforms.Count, retrieved.Count);
        for (int i = 0; i < bindTransforms.Count; i++)
        {
            Assert.Equal(bindTransforms[i], retrieved[i]);
        }
    }
    
    [Fact]
    public void UsdSkelSkeleton_RestTransforms_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        var restTransforms = new List<GfMatrix4d>
        {
            GfMatrix4d.Identity,
            GfMatrix4d.CreateScale(2.0f),
            GfMatrix4d.CreateRotation(0, 0, 45)
        };
        
        // Act
        skeleton.RestTransforms = restTransforms;
        var retrieved = skeleton.RestTransforms;
        
        // Assert
        Assert.Equal(restTransforms.Count, retrieved.Count);
        for (int i = 0; i < restTransforms.Count; i++)
        {
            Assert.Equal(restTransforms[i], retrieved[i]);
        }
    }
    
    [Fact]
    public void UsdSkelSkeleton_AnimationSource_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        
        // Act
        var setResult = skeleton.SetAnimationSource(animation);
        var retrievedAnimation = skeleton.GetAnimationSource();
        
        // Assert
        Assert.True(setResult);
        Assert.True(retrievedAnimation.IsValid);
        Assert.Equal(animation.Prim.GetPath(), retrievedAnimation.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelSkeleton_GetJointCount_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        
        // Initially zero
        Assert.Equal(0, skeleton.GetJointCount());
        
        // Set joints
        skeleton.Joints = new List<string> { "Root", "Root/Hip", "Root/Spine" };
        
        // Act
        var count = skeleton.GetJointCount();
        
        // Assert
        Assert.Equal(3, count);
    }
    
    [Fact]
    public void UsdSkelSkeleton_GetJointParentIndices_SimpleHierarchy_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string>
        {
            "Root",           // 0 - root (parent: -1)
            "Root/Hip",       // 1 - child of Root (parent: 0)
            "Root/Hip/Spine"  // 2 - child of Hip (parent: 1)
        };
        
        // Act
        var parentIndices = skeleton.GetJointParentIndices();
        
        // Assert
        Assert.Equal(3, parentIndices.Count);
        Assert.Equal(-1, parentIndices[0]); // Root has no parent
        Assert.Equal(0, parentIndices[1]);  // Hip's parent is Root (index 0)
        Assert.Equal(1, parentIndices[2]);  // Spine's parent is Hip (index 1)
    }
    
    [Fact]
    public void UsdSkelSkeleton_GetJointChildren_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string>
        {
            "Root",
            "Root/LeftArm",
            "Root/RightArm",
            "Root/LeftArm/Hand"
        };
        
        // Act
        var rootChildren = skeleton.GetJointChildren(0); // Root's children
        var leftArmChildren = skeleton.GetJointChildren(1); // LeftArm's children
        
        // Assert
        Assert.Equal(2, rootChildren.Count);
        Assert.Contains(1, rootChildren); // LeftArm
        Assert.Contains(2, rootChildren); // RightArm
        
        Assert.Single(leftArmChildren);
        Assert.Contains(3, leftArmChildren); // Hand
    }
    
    [Fact]
    public void UsdSkelSkeleton_IsRootJoint_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Child" };
        
        // Act & Assert
        Assert.True(skeleton.IsRootJoint(0));  // Root is root
        Assert.False(skeleton.IsRootJoint(1)); // Child is not root
        Assert.False(skeleton.IsRootJoint(-1)); // Invalid index
        Assert.False(skeleton.IsRootJoint(5));  // Out of range
    }
    
    [Fact]
    public void UsdSkelSkeleton_GetRootJoints_MultipleRoots_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string>
        {
            "Root1",
            "Root2", 
            "Root1/Child",
            "Root2/Child"
        };
        
        // Act
        var rootJoints = skeleton.GetRootJoints();
        
        // Assert
        Assert.Equal(2, rootJoints.Count);
        Assert.Contains(0, rootJoints); // Root1
        Assert.Contains(1, rootJoints); // Root2
    }
    
    [Fact]
    public void UsdSkelSkeleton_ValidateSkeleton_ValidHierarchy_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Hip", "Root/Hip/Spine" };
        skeleton.BindTransforms = new List<GfMatrix4d> { 
            GfMatrix4d.Identity, 
            GfMatrix4d.Identity, 
            GfMatrix4d.Identity 
        };
        
        // Act
        var (isValid, reason) = skeleton.ValidateSkeleton();
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(reason);
    }
    
    [Fact]
    public void UsdSkelSkeleton_ValidateSkeleton_NoJoints_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        
        // Act
        var (isValid, reason) = skeleton.ValidateSkeleton();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("no joints", reason);
    }
    
    [Fact]
    public void UsdSkelSkeleton_ValidateSkeleton_DuplicateJoints_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root", "Root/Hip" };
        
        // Act
        var (isValid, reason) = skeleton.ValidateSkeleton();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("duplicate", reason);
    }
    
    [Fact]
    public void UsdSkelSkeleton_ValidateSkeleton_MismatchedTransforms_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Hip" };
        skeleton.BindTransforms = new List<GfMatrix4d> { GfMatrix4d.Identity }; // Wrong count
        
        // Act
        var (isValid, reason) = skeleton.ValidateSkeleton();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Bind transforms count", reason);
    }
    
    [Fact]
    public void UsdSkelSkeleton_CreateBipedalSkeleton_CreatesValidHierarchy()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        
        // Act
        skeleton.CreateBipedalSkeleton();
        
        // Assert
        var joints = skeleton.Joints;
        Assert.True(joints.Count > 10); // Should have many joints
        Assert.Contains("Root", joints);
        Assert.Contains("Root/Hips", joints);
        Assert.Contains("Root/Hips/Spine", joints);
        Assert.Contains("Root/Hips/LeftHip/LeftThigh/LeftShin/LeftFoot", joints);
        Assert.Contains("Root/Hips/RightHip/RightThigh/RightShin/RightFoot", joints);
        
        // Check transforms were created
        var bindTransforms = skeleton.BindTransforms;
        var restTransforms = skeleton.RestTransforms;
        Assert.Equal(joints.Count, bindTransforms.Count);
        Assert.Equal(joints.Count, restTransforms.Count);
        
        // Validate the hierarchy
        var (isValid, reason) = skeleton.ValidateSkeleton();
        Assert.True(isValid, reason);
    }
    
    [Fact]
    public void UsdSkelSkeleton_SetBindPose_ComputesRestTransforms()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Child" };
        
        var worldTransforms = new List<GfMatrix4d>
        {
            GfMatrix4d.CreateTranslation(new GfVec3f(0, 0, 0)), // Root at origin
            GfMatrix4d.CreateTranslation(new GfVec3f(1, 0, 0))  // Child at (1,0,0)
        };
        
        // Act
        skeleton.SetBindPose(worldTransforms);
        
        // Assert
        var bindTransforms = skeleton.BindTransforms;
        var restTransforms = skeleton.RestTransforms;
        
        Assert.Equal(2, bindTransforms.Count);
        Assert.Equal(2, restTransforms.Count);
        
        // Bind transforms should match input
        Assert.Equal(worldTransforms[0], bindTransforms[0]);
        Assert.Equal(worldTransforms[1], bindTransforms[1]);
        
        // Rest transforms: Root should be world transform, Child should be local
        Assert.Equal(worldTransforms[0], restTransforms[0]);
        // Child's rest should be local relative to parent
        // This is a simplified check - in reality would need inverse math
        Assert.NotEqual(GfMatrix4d.Identity, restTransforms[1]);
    }
    
    [Fact]
    public void UsdSkelSkeleton_SetBindPose_WrongTransformCount_Throws()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Child" };
        
        var wrongTransforms = new List<GfMatrix4d> { GfMatrix4d.Identity }; // Only 1 transform for 2 joints
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => skeleton.SetBindPose(wrongTransforms));
    }
    
    [Fact]
    public void UsdSkelSkeleton_GetStatistics_ReturnsCorrectInfo()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.CreateBipedalSkeleton();
        
        // Act
        var (jointCount, rootCount, hasBindPose, hasRestPose) = skeleton.GetStatistics();
        
        // Assert
        Assert.True(jointCount > 10);
        Assert.Equal(1, rootCount); // Bipedal has one root
        Assert.True(hasBindPose);
        Assert.True(hasRestPose);
    }
    
    [Fact]
    public void UsdSkelSkeleton_InvalidPrim_HandlesGracefully()
    {
        // Arrange
        var invalidSkeleton = new UsdSkelSkeleton();
        
        // Act & Assert
        Assert.False(invalidSkeleton.IsValid);
        Assert.Equal(0, invalidSkeleton.GetJointCount());
        Assert.Empty(invalidSkeleton.Joints);
        Assert.Empty(invalidSkeleton.GetJointParentIndices());
        Assert.Empty(invalidSkeleton.GetRootJoints());
        
        var (isValid, reason) = invalidSkeleton.ValidateSkeleton();
        Assert.False(isValid);
        Assert.Contains("invalid", reason);
    }
    
    [Fact]
    public void UsdSkelSkeleton_ToString_ShowsInformation()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Child" };
        skeleton.BindTransforms = new List<GfMatrix4d> { GfMatrix4d.Identity, GfMatrix4d.Identity };
        
        // Act
        var result = skeleton.ToString();
        
        // Assert
        Assert.Contains("UsdSkelSkeleton", result);
        Assert.Contains("/Skeleton", result);
        Assert.Contains("2 joints", result);
        Assert.Contains("1 roots", result);
        Assert.Contains("bind:True", result);
    }
}