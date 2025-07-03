using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Pxr.Usd.UsdSkel;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdSkelAnimationTests
{
    [Fact]
    public void UsdSkelAnimation_BasicConstruction_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animPath = new SdfPath("/Animation");
        
        // Act
        var animation = UsdSkelAnimation.Define(stage, animPath);
        
        // Assert
        Assert.True(animation.IsValid);
        Assert.Equal(animPath, animation.Prim.GetPath());
        Assert.Equal("SkelAnimation", animation.Prim.GetTypeName());
    }
    
    [Fact]
    public void GfQuatf_BasicOperations_Work()
    {
        // Arrange
        var quat1 = new GfQuatf(0, 0, 0, 1);
        var quat2 = GfQuatf.Identity;
        var systemQuat = new Quaternion(0, 0, 0, 1);
        
        // Act & Assert
        Assert.Equal(GfQuatf.Identity, quat1);
        Assert.Equal(quat1, quat2);
        Assert.Equal(systemQuat, quat1.ToQuaternion());
        
        // Test implicit conversions
        GfQuatf fromSystem = systemQuat;
        Quaternion toSystem = quat1;
        Assert.Equal(quat1, fromSystem);
        Assert.Equal(systemQuat, toSystem);
    }
    
    [Fact]
    public void UsdSkelAnimation_JointsAttribute_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        var joints = new List<string> { "Root", "Root/Hip", "Root/Spine" };
        
        // Act
        animation.Joints = joints;
        var retrievedJoints = animation.Joints;
        
        // Assert
        Assert.Equal(joints.Count, retrievedJoints.Count);
        for (int i = 0; i < joints.Count; i++)
        {
            Assert.Equal(joints[i], retrievedJoints[i]);
        }
    }
    
    [Fact]
    public void UsdSkelAnimation_SetAndGetTranslations_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        var translations = new List<GfVec3f>
        {
            new GfVec3f(0, 0, 0),
            new GfVec3f(1, 0, 0),
            new GfVec3f(0, 1, 0)
        };
        var time = UsdTimeCode.Create(1.0);
        
        // Act
        var setResult = animation.SetTranslations(translations, time);
        var retrievedTranslations = animation.GetTranslations(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(translations.Count, retrievedTranslations.Count);
        for (int i = 0; i < translations.Count; i++)
        {
            Assert.Equal(translations[i], retrievedTranslations[i]);
        }
    }
    
    [Fact]
    public void UsdSkelAnimation_SetAndGetRotations_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        var rotations = new List<GfQuatf>
        {
            GfQuatf.Identity,
            new GfQuatf(0, 0, 0.707f, 0.707f), // 90 degree rotation around Z
            new GfQuatf(0.707f, 0, 0, 0.707f)  // 90 degree rotation around X
        };
        var time = UsdTimeCode.Create(2.0);
        
        // Act
        var setResult = animation.SetRotations(rotations, time);
        var retrievedRotations = animation.GetRotations(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(rotations.Count, retrievedRotations.Count);
        for (int i = 0; i < rotations.Count; i++)
        {
            Assert.Equal(rotations[i], retrievedRotations[i]);
        }
    }
    
    [Fact]
    public void UsdSkelAnimation_SetAndGetScales_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        var scales = new List<GfVec3f>
        {
            GfVec3f.One,
            new GfVec3f(2, 2, 2),
            new GfVec3f(0.5f, 1, 1.5f)
        };
        var time = UsdTimeCode.Create(3.0);
        
        // Act
        var setResult = animation.SetScales(scales, time);
        var retrievedScales = animation.GetScales(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(scales.Count, retrievedScales.Count);
        for (int i = 0; i < scales.Count; i++)
        {
            Assert.Equal(scales[i], retrievedScales[i]);
        }
    }
    
    [Fact]
    public void UsdSkelAnimation_SetTransforms_AllComponents_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root", "Child" };
        
        var translations = new List<GfVec3f> { GfVec3f.Zero, new GfVec3f(1, 0, 0) };
        var rotations = new List<GfQuatf> { GfQuatf.Identity, GfQuatf.Identity };
        var scales = new List<GfVec3f> { GfVec3f.One, GfVec3f.One };
        var time = UsdTimeCode.Create(4.0);
        
        // Act
        var setResult = animation.SetTransforms(translations, rotations, scales, time);
        var (retrievedTrans, retrievedRots, retrievedScales) = animation.GetTransforms(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(2, retrievedTrans.Count);
        Assert.Equal(2, retrievedRots.Count);
        Assert.Equal(2, retrievedScales.Count);
        
        Assert.Equal(translations[0], retrievedTrans[0]);
        Assert.Equal(translations[1], retrievedTrans[1]);
        Assert.Equal(rotations[0], retrievedRots[0]);
        Assert.Equal(scales[0], retrievedScales[0]);
    }
    
    [Fact]
    public void UsdSkelAnimation_SetTransforms_MismatchedSizes_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root", "Child" };
        
        var translations = new List<GfVec3f> { GfVec3f.Zero }; // Wrong size
        var rotations = new List<GfQuatf> { GfQuatf.Identity, GfQuatf.Identity };
        var scales = new List<GfVec3f> { GfVec3f.One, GfVec3f.One };
        
        // Act
        var result = animation.SetTransforms(translations, rotations, scales);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelAnimation_BlendShapes_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        var blendShapes = new List<string> { "smile", "frown", "blink" };
        var weights = new List<float> { 0.5f, 0.3f, 1.0f };
        var time = UsdTimeCode.Create(5.0);
        
        // Act
        animation.BlendShapes = blendShapes;
        var setResult = animation.SetBlendShapeWeights(weights, time);
        
        var retrievedShapes = animation.BlendShapes;
        var retrievedWeights = animation.GetBlendShapeWeights(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(blendShapes, retrievedShapes);
        Assert.Equal(weights, retrievedWeights);
    }
    
    [Fact]
    public void UsdSkelAnimation_ComputeJointTransforms_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        
        // Create skeleton
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Root/Child" };
        skeleton.RestTransforms = new List<GfMatrix4d> 
        { 
            GfMatrix4d.Identity, 
            GfMatrix4d.CreateTranslation(new GfVec3f(1, 0, 0)) 
        };
        
        // Create animation
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = skeleton.Joints;
        
        var translations = new List<GfVec3f> { GfVec3f.Zero, GfVec3f.Zero };
        var rotations = new List<GfQuatf> { GfQuatf.Identity, GfQuatf.Identity };
        var scales = new List<GfVec3f> { GfVec3f.One, GfVec3f.One };
        
        animation.SetTransforms(translations, rotations, scales);
        
        // Act
        var transforms = animation.ComputeJointTransforms(skeleton);
        
        // Assert
        Assert.Equal(2, transforms.Count);
        Assert.NotEqual(default(GfMatrix4d), transforms[0]);
        Assert.NotEqual(default(GfMatrix4d), transforms[1]);
    }
    
    [Fact]
    public void UsdSkelAnimation_GetTimeSamples_ReturnsAllTimes()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root" };
        
        var times = new[] { 1.0, 5.0, 10.0 };
        var translations = new List<GfVec3f> { GfVec3f.Zero };
        var rotations = new List<GfQuatf> { GfQuatf.Identity };
        var scales = new List<GfVec3f> { GfVec3f.One };
        
        // Set keyframes at different times
        foreach (var time in times)
        {
            animation.SetTransforms(translations, rotations, scales, UsdTimeCode.Create(time));
        }
        
        // Act
        var timeSamples = animation.GetTimeSamples();
        
        // Assert
        Assert.Equal(times.Length, timeSamples.Count);
        foreach (var time in times)
        {
            Assert.Contains(time, timeSamples);
        }
    }
    
    [Fact]
    public void UsdSkelAnimation_GetTimeRange_ReturnsMinMax()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root" };
        
        var translations = new List<GfVec3f> { GfVec3f.Zero };
        var rotations = new List<GfQuatf> { GfQuatf.Identity };
        var scales = new List<GfVec3f> { GfVec3f.One };
        
        animation.SetTransforms(translations, rotations, scales, UsdTimeCode.Create(2.0));
        animation.SetTransforms(translations, rotations, scales, UsdTimeCode.Create(8.0));
        animation.SetTransforms(translations, rotations, scales, UsdTimeCode.Create(5.0));
        
        // Act
        var (startTime, endTime) = animation.GetTimeRange();
        
        // Assert
        Assert.Equal(2.0, startTime);
        Assert.Equal(8.0, endTime);
    }
    
    [Fact]
    public void UsdSkelAnimation_ValidateAnimation_ValidData_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root", "Child" };
        
        var translations = new List<GfVec3f> { GfVec3f.Zero, GfVec3f.Zero };
        var rotations = new List<GfQuatf> { GfQuatf.Identity, GfQuatf.Identity };
        var scales = new List<GfVec3f> { GfVec3f.One, GfVec3f.One };
        
        animation.SetTransforms(translations, rotations, scales);
        
        // Act
        var (isValid, reason) = animation.ValidateAnimation();
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(reason);
    }
    
    [Fact]
    public void UsdSkelAnimation_ValidateAnimation_NoJoints_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        
        // Act
        var (isValid, reason) = animation.ValidateAnimation();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("no joints", reason);
    }
    
    [Fact]
    public void UsdSkelAnimation_ValidateAnimation_MismatchedArraySizes_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root", "Child" };
        
        // Set mismatched array sizes
        var translations = new List<GfVec3f> { GfVec3f.Zero }; // Wrong size
        var rotations = new List<GfQuatf> { GfQuatf.Identity, GfQuatf.Identity };
        
        animation.SetTranslations(translations);
        animation.SetRotations(rotations);
        
        // Act
        var (isValid, reason) = animation.ValidateAnimation();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("Translations count", reason);
    }
    
    [Fact]
    public void UsdSkelAnimation_CreateIdleAnimation_CreatesLoopingAnimation()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        var joints = new List<string> { "Root", "Hip", "Spine" };
        
        // Act
        animation.CreateIdleAnimation(joints, 2.0, 30);
        
        // Assert
        Assert.Equal(joints, animation.Joints);
        
        var timeSamples = animation.GetTimeSamples();
        Assert.Contains(0.0, timeSamples); // Default time
        Assert.Contains(60.0, timeSamples); // duration * frameRate
        
        // Check that transforms are identity (idle)
        var (translations, rotations, scales) = animation.GetTransforms();
        Assert.Equal(joints.Count, translations.Count);
        Assert.All(translations, t => Assert.Equal(GfVec3f.Zero, t));
        Assert.All(rotations, r => Assert.Equal(GfQuatf.Identity, r));
        Assert.All(scales, s => Assert.Equal(GfVec3f.One, s));
    }
    
    [Fact]
    public void UsdSkelAnimation_AddKeyframe_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        animation.Joints = new List<string> { "Root" };
        
        var translations = new List<GfVec3f> { new GfVec3f(1, 2, 3) };
        var rotations = new List<GfQuatf> { new GfQuatf(0.1f, 0.2f, 0.3f, 0.9f) };
        var scales = new List<GfVec3f> { new GfVec3f(2, 2, 2) };
        
        // Act
        var result = animation.AddKeyframe(10.0, translations, rotations, scales);
        
        // Assert
        Assert.True(result);
        
        var (retrievedTrans, retrievedRots, retrievedScales) = animation.GetTransforms(UsdTimeCode.Create(10.0));
        Assert.Equal(translations[0], retrievedTrans[0]);
        Assert.Equal(rotations[0], retrievedRots[0]);
        Assert.Equal(scales[0], retrievedScales[0]);
    }
    
    [Fact]
    public void UsdSkelAnimation_GetStatistics_ReturnsCorrectInfo()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        
        animation.CreateIdleAnimation(new List<string> { "Root", "Child" }, 5.0, 24);
        animation.BlendShapes = new List<string> { "smile", "frown" };
        
        // Act
        var (jointCount, blendShapeCount, keyframeCount, duration) = animation.GetStatistics();
        
        // Assert
        Assert.Equal(2, jointCount);
        Assert.Equal(2, blendShapeCount);
        Assert.True(keyframeCount >= 2); // At least start and end
        Assert.Equal(120.0, duration); // 5.0 * 24 fps
    }
    
    [Fact]
    public void UsdSkelAnimation_InvalidPrim_HandlesGracefully()
    {
        // Arrange
        var invalidAnimation = new UsdSkelAnimation();
        
        // Act & Assert
        Assert.False(invalidAnimation.IsValid);
        Assert.Empty(invalidAnimation.Joints);
        Assert.Empty(invalidAnimation.GetTranslations());
        Assert.Empty(invalidAnimation.GetRotations());
        Assert.Empty(invalidAnimation.GetScales());
        Assert.Empty(invalidAnimation.GetTimeSamples());
        
        var (isValid, reason) = invalidAnimation.ValidateAnimation();
        Assert.False(isValid);
        Assert.Contains("invalid", reason);
    }
    
    [Fact]
    public void UsdSkelAnimation_ToString_ShowsInformation()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/TestAnimation"));
        animation.CreateIdleAnimation(new List<string> { "Root", "Child", "Grandchild" }, 3.0, 30);
        
        // Act
        var result = animation.ToString();
        
        // Assert
        Assert.Contains("UsdSkelAnimation", result);
        Assert.Contains("/TestAnimation", result);
        Assert.Contains("3 joints", result);
        Assert.Contains("90.00s", result); // 3.0 * 30 fps
    }
}