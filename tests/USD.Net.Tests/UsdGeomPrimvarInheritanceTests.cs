using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Xunit;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdGeomPrimvarInheritanceTests
{
    private UsdStage CreateTestStage()
    {
        return UsdStage.CreateInMemory();
    }

    #region Inheritance Behavior Tests

    [Fact]
    public void UsdGeomPrimvar_ConstantInterpolation_InheritsCorrectly()
    {
        var stage = CreateTestStage();
        
        // Create hierarchy: /World/Group/Mesh
        var worldPrim = stage.DefinePrim(new SdfPath("/World"));
        var groupPrim = stage.DefinePrim(new SdfPath("/World/Group"));
        var meshPrim = stage.DefinePrim(new SdfPath("/World/Group/Mesh"));
        
        var worldAPI = new UsdGeomPrimvarsAPI(worldPrim);
        var groupAPI = new UsdGeomPrimvarsAPI(groupPrim);
        var meshAPI = new UsdGeomPrimvarsAPI(meshPrim);

        // Add constant primvar to world (should inherit)
        var worldVar = worldAPI.CreateNonIndexedPrimvar(
            new TfToken("materialId"),
            "int[]",
            new List<int> { 1 },
            UsdGeomInterpolation.Constant);

        // Child should inherit the constant primvar
        var inheritedVar = meshAPI.FindPrimvarWithInheritance(new TfToken("materialId"));
        Assert.True(inheritedVar.IsValid());
        Assert.True(inheritedVar.Get(out List<int> materialId));
        Assert.Equal(1, materialId[0]);

        // Override at group level
        var groupVar = groupAPI.CreateNonIndexedPrimvar(
            new TfToken("materialId"),
            "int[]",
            new List<int> { 2 },
            UsdGeomInterpolation.Constant);

        // Mesh should now inherit the group's override
        var overriddenVar = meshAPI.FindPrimvarWithInheritance(new TfToken("materialId"));
        Assert.True(overriddenVar.IsValid());
        Assert.True(overriddenVar.Get(out List<int> overriddenId));
        Assert.Equal(2, overriddenId[0]);

        // Local definition should take precedence
        var meshVar = meshAPI.CreateNonIndexedPrimvar(
            new TfToken("materialId"),
            "int[]",
            new List<int> { 3 },
            UsdGeomInterpolation.Constant);

        var localVar = meshAPI.FindPrimvarWithInheritance(new TfToken("materialId"));
        Assert.True(localVar.IsValid());
        Assert.True(localVar.Get(out List<int> localId));
        Assert.Equal(3, localId[0]);
    }

    [Fact]
    public void UsdGeomPrimvar_NonConstantInterpolation_DoesNotInherit()
    {
        var stage = CreateTestStage();
        
        var parentPrim = stage.DefinePrim(new SdfPath("/Parent"));
        var childPrim = stage.DefinePrim(new SdfPath("/Parent/Child"));
        
        var parentAPI = new UsdGeomPrimvarsAPI(parentPrim);
        var childAPI = new UsdGeomPrimvarsAPI(childPrim);

        // Create primvars with different interpolations on parent
        var interpolations = new[]
        {
            UsdGeomInterpolation.Uniform,
            UsdGeomInterpolation.Varying,
            UsdGeomInterpolation.Vertex,
            UsdGeomInterpolation.FaceVarying
        };

        foreach (var interpolation in interpolations)
        {
            var varName = $"var_{interpolation}";
            parentAPI.CreateNonIndexedPrimvar(
                new TfToken(varName),
                "float[]",
                new List<float> { 1.0f },
                interpolation);

            // Child should NOT inherit non-constant primvars
            var inheritedVar = childAPI.FindPrimvarWithInheritance(new TfToken(varName));
            Assert.False(inheritedVar.IsValid());
        }

        // But constant should inherit
        parentAPI.CreateNonIndexedPrimvar(
            new TfToken("var_Constant"),
            "float[]",
            new List<float> { 1.0f },
            UsdGeomInterpolation.Constant);

        var constantVar = childAPI.FindPrimvarWithInheritance(new TfToken("var_Constant"));
        Assert.True(constantVar.IsValid());
    }

    [Fact]
    public void UsdGeomPrimvar_DeepHierarchy_InheritsCorrectly()
    {
        var stage = CreateTestStage();
        
        // Create deep hierarchy: /A/B/C/D/E
        var paths = new[]
        {
            new SdfPath("/A"),
            new SdfPath("/A/B"), 
            new SdfPath("/A/B/C"),
            new SdfPath("/A/B/C/D"),
            new SdfPath("/A/B/C/D/E")
        };

        var prims = paths.Select(p => stage.DefinePrim(p)).ToArray();
        var apis = prims.Select(p => new UsdGeomPrimvarsAPI(p)).ToArray();

        // Add constant primvar at root
        apis[0].CreateNonIndexedPrimvar(
            new TfToken("deepVar"),
            "string[]",
            new List<string> { "root_value" },
            UsdGeomInterpolation.Constant);

        // Override at middle level (C)
        apis[2].CreateNonIndexedPrimvar(
            new TfToken("deepVar"),
            "string[]",
            new List<string> { "middle_value" },
            UsdGeomInterpolation.Constant);

        // All descendants of C should get middle_value
        var leafVar = apis[4].FindPrimvarWithInheritance(new TfToken("deepVar"));
        Assert.True(leafVar.IsValid());
        Assert.True(leafVar.Get(out List<string> leafValue));
        Assert.Equal("middle_value", leafValue[0]);

        // B should get root_value (no override between A and B)
        var bVar = apis[1].FindPrimvarWithInheritance(new TfToken("deepVar"));
        Assert.True(bVar.IsValid());
        Assert.True(bVar.Get(out List<string> bValue));
        Assert.Equal("root_value", bValue[0]);
    }

    [Fact]
    public void UsdGeomPrimvar_FindPrimvarsWithInheritance_ReturnsAllAccessible()
    {
        var stage = CreateTestStage();
        
        var rootPrim = stage.DefinePrim(new SdfPath("/Root"));
        var childPrim = stage.DefinePrim(new SdfPath("/Root/Child"));
        
        var rootAPI = new UsdGeomPrimvarsAPI(rootPrim);
        var childAPI = new UsdGeomPrimvarsAPI(childPrim);

        // Add various primvars to root
        rootAPI.CreateNonIndexedPrimvar(
            new TfToken("constantVar1"),
            "float[]",
            new List<float> { 1.0f },
            UsdGeomInterpolation.Constant);

        rootAPI.CreateNonIndexedPrimvar(
            new TfToken("constantVar2"),
            "float[]",
            new List<float> { 2.0f },
            UsdGeomInterpolation.Constant);

        rootAPI.CreateNonIndexedPrimvar(
            new TfToken("vertexVar"),
            "float[]",
            new List<float> { 3.0f },
            UsdGeomInterpolation.Vertex); // Should not inherit

        // Add local primvar to child
        childAPI.CreateNonIndexedPrimvar(
            new TfToken("localVar"),
            "float[]",
            new List<float> { 4.0f },
            UsdGeomInterpolation.Uniform);

        // Override one of the inherited ones
        childAPI.CreateNonIndexedPrimvar(
            new TfToken("constantVar1"),
            "float[]",
            new List<float> { 5.0f },
            UsdGeomInterpolation.Constant);

        var allPrimvars = childAPI.FindPrimvarsWithInheritance();
        
        // Should have: constantVar1 (overridden), constantVar2 (inherited), localVar (local)
        // Should NOT have: vertexVar (not inheritable)
        Assert.Equal(3, allPrimvars.Count);

        var primvarDict = allPrimvars.ToDictionary(
            p => p.GetPrimvarName().GetText(),
            p => p);

        Assert.True(primvarDict.ContainsKey("constantVar1"));
        Assert.True(primvarDict.ContainsKey("constantVar2"));
        Assert.True(primvarDict.ContainsKey("localVar"));
        Assert.False(primvarDict.ContainsKey("vertexVar"));

        // Verify values
        Assert.True(primvarDict["constantVar1"].Get(out List<float> var1Val));
        Assert.Equal(5.0f, var1Val[0]); // Overridden value

        Assert.True(primvarDict["constantVar2"].Get(out List<float> var2Val));
        Assert.Equal(2.0f, var2Val[0]); // Inherited value

        Assert.True(primvarDict["localVar"].Get(out List<float> localVal));
        Assert.Equal(4.0f, localVal[0]); // Local value
    }

    [Fact]
    public void UsdGeomPrimvar_FindInheritablePrimvars_ReturnsOnlyConstant()
    {
        var stage = CreateTestStage();
        var prim = stage.DefinePrim(new SdfPath("/TestPrim"));
        var api = new UsdGeomPrimvarsAPI(prim);

        // Create primvars with different interpolations
        api.CreateNonIndexedPrimvar(
            new TfToken("constantVar"),
            "float[]",
            new List<float> { 1.0f },
            UsdGeomInterpolation.Constant);

        api.CreateNonIndexedPrimvar(
            new TfToken("vertexVar"),
            "float[]",
            new List<float> { 2.0f },
            UsdGeomInterpolation.Vertex);

        api.CreateNonIndexedPrimvar(
            new TfToken("uniformVar"),
            "float[]",
            new List<float> { 3.0f },
            UsdGeomInterpolation.Uniform);

        api.CreateNonIndexedPrimvar(
            new TfToken("constantVar2"),
            "float[]",
            new List<float> { 4.0f },
            UsdGeomInterpolation.Constant);

        var inheritablePrimvars = api.FindInheritablePrimvars();
        
        // Should only return the constant ones
        Assert.Equal(2, inheritablePrimvars.Count);
        
        var names = inheritablePrimvars.Select(p => p.GetPrimvarName().GetText()).ToList();
        Assert.Contains("constantVar", names);
        Assert.Contains("constantVar2", names);
        Assert.DoesNotContain("vertexVar", names);
        Assert.DoesNotContain("uniformVar", names);
    }

    #endregion

    #region Interpolation Behavior Tests

    [Fact]
    public void UsdGeomPrimvar_InterpolationTypes_BehaveProperly()
    {
        var stage = CreateTestStage();
        var prim = stage.DefinePrim(new SdfPath("/TestMesh"));
        var api = new UsdGeomPrimvarsAPI(prim);

        // Test each interpolation type
        var testCases = new[]
        {
            new { Name = "constantColor", Interpolation = UsdGeomInterpolation.Constant, ExpectedCount = 1 },
            new { Name = "uniformIds", Interpolation = UsdGeomInterpolation.Uniform, ExpectedCount = 6 }, // Per face
            new { Name = "vertexColors", Interpolation = UsdGeomInterpolation.Vertex, ExpectedCount = 8 }, // Per vertex
            new { Name = "varyingData", Interpolation = UsdGeomInterpolation.Varying, ExpectedCount = 4 }, // UV patches
            new { Name = "faceVaryingUVs", Interpolation = UsdGeomInterpolation.FaceVarying, ExpectedCount = 24 } // Per face-vertex
        };

        foreach (var testCase in testCases)
        {
            var values = Enumerable.Range(0, testCase.ExpectedCount)
                                  .Select(i => (float)i)
                                  .ToList();

            var primvar = api.CreateNonIndexedPrimvar(
                new TfToken(testCase.Name),
                "float[]",
                values,
                testCase.Interpolation);

            Assert.True(primvar.IsValid());
            Assert.Equal(testCase.Interpolation, primvar.GetInterpolation());
            
            Assert.True(primvar.Get(out List<float> retrievedValues));
            Assert.Equal(testCase.ExpectedCount, retrievedValues.Count);
        }
    }

    [Fact]
    public void UsdGeomPrimvar_InterpolationValidation_Works()
    {
        // Test interpolation validation
        Assert.True(UsdGeomPrimvarConstants.IsValidInterpolation(UsdGeomInterpolation.Constant));
        Assert.True(UsdGeomPrimvarConstants.IsValidInterpolation(UsdGeomInterpolation.Uniform));
        Assert.True(UsdGeomPrimvarConstants.IsValidInterpolation(UsdGeomInterpolation.Varying));
        Assert.True(UsdGeomPrimvarConstants.IsValidInterpolation(UsdGeomInterpolation.Vertex));
        Assert.True(UsdGeomPrimvarConstants.IsValidInterpolation(UsdGeomInterpolation.FaceVarying));

        // Test string conversion round-trip
        foreach (UsdGeomInterpolation interpolation in Enum.GetValues<UsdGeomInterpolation>())
        {
            var str = UsdGeomPrimvarConstants.InterpolationToString(interpolation);
            var backToEnum = UsdGeomPrimvarConstants.StringToInterpolation(str);
            Assert.Equal(interpolation, backToEnum);
        }
    }

    #endregion

    #region Complex Inheritance Scenarios

    [Fact]
    public void UsdGeomPrimvar_MultipleInheritanceLevels_WorkCorrectly()
    {
        var stage = CreateTestStage();
        
        // Create complex hierarchy: /Scene/Group1/Group2/Mesh1, /Scene/Group1/Mesh2
        var scenePrim = stage.DefinePrim(new SdfPath("/Scene"));
        var group1Prim = stage.DefinePrim(new SdfPath("/Scene/Group1"));
        var group2Prim = stage.DefinePrim(new SdfPath("/Scene/Group1/Group2"));
        var mesh1Prim = stage.DefinePrim(new SdfPath("/Scene/Group1/Group2/Mesh1"));
        var mesh2Prim = stage.DefinePrim(new SdfPath("/Scene/Group1/Mesh2"));
        
        var sceneAPI = new UsdGeomPrimvarsAPI(scenePrim);
        var group1API = new UsdGeomPrimvarsAPI(group1Prim);
        var group2API = new UsdGeomPrimvarsAPI(group2Prim);
        var mesh1API = new UsdGeomPrimvarsAPI(mesh1Prim);
        var mesh2API = new UsdGeomPrimvarsAPI(mesh2Prim);

        // Scene level: global material settings
        sceneAPI.CreateNonIndexedPrimvar(
            new TfToken("lightingModel"),
            "string[]",
            new List<string> { "phong" },
            UsdGeomInterpolation.Constant);

        sceneAPI.CreateNonIndexedPrimvar(
            new TfToken("globalTint"),
            "color3f[]",
            new List<GfVec3f> { new GfVec3f(1.0f, 1.0f, 1.0f) },
            UsdGeomInterpolation.Constant);

        // Group1 level: material override
        group1API.CreateNonIndexedPrimvar(
            new TfToken("materialId"),
            "int[]",
            new List<int> { 1 },
            UsdGeomInterpolation.Constant);

        group1API.CreateNonIndexedPrimvar(
            new TfToken("globalTint"),
            "color3f[]",
            new List<GfVec3f> { new GfVec3f(0.9f, 0.9f, 1.0f) }, // Slight blue tint
            UsdGeomInterpolation.Constant);

        // Group2 level: more specific overrides
        group2API.CreateNonIndexedPrimvar(
            new TfToken("materialId"),
            "int[]",
            new List<int> { 2 },
            UsdGeomInterpolation.Constant);

        // Test Mesh1 inheritance (deepest nesting)
        var mesh1Primvars = mesh1API.FindPrimvarsWithInheritance();
        var mesh1Dict = mesh1Primvars.ToDictionary(p => p.GetPrimvarName().GetText());

        Assert.Equal(3, mesh1Dict.Count);
        
        // Should inherit lightingModel from scene
        Assert.True(mesh1Dict["lightingModel"].Get(out List<string> lightingModel));
        Assert.Equal("phong", lightingModel[0]);

        // Should inherit globalTint from group1 (closer than scene)
        Assert.True(mesh1Dict["globalTint"].Get(out List<GfVec3f> tint));
        Assert.Equal(0.9f, tint[0].X);

        // Should inherit materialId from group2 (closest)
        Assert.True(mesh1Dict["materialId"].Get(out List<int> materialId));
        Assert.Equal(2, materialId[0]);

        // Test Mesh2 inheritance (shallower nesting)
        var mesh2Primvars = mesh2API.FindPrimvarsWithInheritance();
        var mesh2Dict = mesh2Primvars.ToDictionary(p => p.GetPrimvarName().GetText());

        Assert.Equal(3, mesh2Dict.Count);
        
        // Should inherit materialId from group1 (not group2)
        Assert.True(mesh2Dict["materialId"].Get(out List<int> mesh2MaterialId));
        Assert.Equal(1, mesh2MaterialId[0]);
    }

    [Fact]
    public void UsdGeomPrimvar_HasPossiblyInheritedPrimvar_WorksCorrectly()
    {
        var stage = CreateTestStage();
        
        var parentPrim = stage.DefinePrim(new SdfPath("/Parent"));
        var childPrim = stage.DefinePrim(new SdfPath("/Parent/Child"));
        
        var parentAPI = new UsdGeomPrimvarsAPI(parentPrim);
        var childAPI = new UsdGeomPrimvarsAPI(childPrim);

        // Add constant primvar to parent
        parentAPI.CreateNonIndexedPrimvar(
            new TfToken("inheritableVar"),
            "float[]",
            new List<float> { 1.0f },
            UsdGeomInterpolation.Constant);

        // Add non-constant primvar to parent
        parentAPI.CreateNonIndexedPrimvar(
            new TfToken("nonInheritableVar"),
            "float[]",
            new List<float> { 2.0f },
            UsdGeomInterpolation.Vertex);

        // Child should report having the inheritable one
        Assert.True(childAPI.HasPossiblyInheritedPrimvar(new TfToken("inheritableVar")));
        Assert.False(childAPI.HasPossiblyInheritedPrimvar(new TfToken("nonInheritableVar")));
        Assert.False(childAPI.HasPossiblyInheritedPrimvar(new TfToken("nonexistent")));

        // After child defines its own version, should still return true
        childAPI.CreateNonIndexedPrimvar(
            new TfToken("inheritableVar"),
            "float[]",
            new List<float> { 3.0f },
            UsdGeomInterpolation.Constant);

        Assert.True(childAPI.HasPossiblyInheritedPrimvar(new TfToken("inheritableVar")));
    }

    #endregion
}