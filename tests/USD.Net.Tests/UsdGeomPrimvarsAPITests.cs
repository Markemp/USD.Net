using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Xunit;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdGeomPrimvarsAPITests
{
    private UsdStage CreateTestStage()
    {
        return UsdStage.CreateInMemory();
    }

    #region Construction and Factory Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_Construction_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        // Constructor from prim
        var api = new UsdGeomPrimvarsAPI(prim);
        Assert.True(api.IsValid);
        Assert.Equal(primPath, api.Path);

        // Invalid constructor
        var invalidAPI = new UsdGeomPrimvarsAPI();
        Assert.False(invalidAPI.IsValid);

        // Factory method from stage/path
        var factoryAPI = UsdGeomPrimvarsAPI.Get(stage, primPath);
        Assert.True(factoryAPI.IsValid);
        Assert.Equal(primPath, factoryAPI.Path);

        // Factory method from prim
        var primAPI = UsdGeomPrimvarsAPI.Get(prim);
        Assert.True(primAPI.IsValid);
        Assert.Equal(primPath, primAPI.Path);
    }

    #endregion

    #region Primvar Creation Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_CreatePrimvar_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        // Create basic primvar
        var primvar = api.CreatePrimvar(
            new TfToken("testVar"), 
            "float[]", 
            UsdGeomInterpolation.Vertex, 
            2);

        Assert.True(primvar.IsValid());
        Assert.Equal("testVar", primvar.GetPrimvarName().GetText());
        Assert.Equal("float[]", primvar.GetTypeName());
        Assert.Equal(UsdGeomInterpolation.Vertex, primvar.GetInterpolation());
        Assert.Equal(2, primvar.GetElementSize());

        // Create primvar with default interpolation
        var defaultPrimvar = api.CreatePrimvar(new TfToken("defaultVar"), "color3f[]");
        Assert.True(defaultPrimvar.IsValid());
        Assert.Equal(UsdGeomInterpolation.Vertex, defaultPrimvar.GetInterpolation());
        Assert.Equal(1, defaultPrimvar.GetElementSize()); // Default element size

        // Should fail with invalid name
        var invalidPrimvar = api.CreatePrimvar(new TfToken("indices"), "float[]");
        Assert.False(invalidPrimvar.IsValid());

        // Should fail with empty name
        var emptyPrimvar = api.CreatePrimvar(new TfToken(""), "float[]");
        Assert.False(emptyPrimvar.IsValid());
    }

    [Fact]
    public void UsdGeomPrimvarsAPI_CreateNonIndexedPrimvar_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        var colors = new List<GfVec3f>
        {
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f),
            new GfVec3f(0.0f, 0.0f, 1.0f)
        };

        var primvar = api.CreateNonIndexedPrimvar(
            new TfToken("colors"),
            "color3f[]",
            colors,
            UsdGeomInterpolation.Vertex);

        Assert.True(primvar.IsValid());
        Assert.True(primvar.HasValue());
        Assert.False(primvar.IsIndexed());

        // Verify values
        Assert.True(primvar.Get(out List<GfVec3f> retrievedColors));
        Assert.Equal(colors.Count, retrievedColors.Count);
        for (int i = 0; i < colors.Count; i++)
        {
            Assert.Equal(colors[i], retrievedColors[i]);
        }
    }

    [Fact]
    public void UsdGeomPrimvarsAPI_CreateIndexedPrimvar_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        var uvValues = new List<GfVec2f>
        {
            new GfVec2f(0.0f, 0.0f),
            new GfVec2f(1.0f, 0.0f),
            new GfVec2f(1.0f, 1.0f),
            new GfVec2f(0.0f, 1.0f)
        };

        var indices = new List<int> { 0, 1, 2, 2, 3, 0 };

        var primvar = api.CreateIndexedPrimvar(
            new TfToken("uvs"),
            "float2[]",
            uvValues,
            indices,
            UsdGeomInterpolation.FaceVarying);

        Assert.True(primvar.IsValid());
        Assert.True(primvar.HasValue());
        Assert.True(primvar.IsIndexed());
        Assert.Equal(UsdGeomInterpolation.FaceVarying, primvar.GetInterpolation());

        // Verify values and indices
        Assert.True(primvar.Get(out List<GfVec2f> retrievedValues));
        Assert.Equal(uvValues.Count, retrievedValues.Count);

        Assert.True(primvar.GetIndices(out var retrievedIndices));
        Assert.Equal(indices.Count, retrievedIndices.Count);
        for (int i = 0; i < indices.Count; i++)
        {
            Assert.Equal(indices[i], retrievedIndices[i]);
        }
    }

    #endregion

    #region Primvar Query Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_BasicQueries_Work()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        // Initially no primvars
        Assert.False(api.HasPrimvar(new TfToken("testVar")));
        Assert.Empty(api.GetPrimvars());
        Assert.Empty(api.GetPrimvarNames());

        // Create some primvars
        var primvar1 = api.CreatePrimvar(new TfToken("var1"), "float[]");
        var primvar2 = api.CreateNonIndexedPrimvar(
            new TfToken("var2"), 
            "color3f[]", 
            new List<GfVec3f> { new GfVec3f(1.0f, 0.0f, 0.0f) });

        // Test queries
        Assert.True(api.HasPrimvar(new TfToken("var1")));
        Assert.True(api.HasPrimvar(new TfToken("var2")));
        Assert.False(api.HasPrimvar(new TfToken("nonexistent")));

        // Test GetPrimvar
        var retrievedVar1 = api.GetPrimvar(new TfToken("var1"));
        Assert.True(retrievedVar1.IsValid());
        Assert.Equal("var1", retrievedVar1.GetPrimvarName().GetText());

        var nonexistentVar = api.GetPrimvar(new TfToken("nonexistent"));
        Assert.False(nonexistentVar.IsValid());

        // Test GetPrimvars
        var allPrimvars = api.GetPrimvars();
        Assert.Equal(2, allPrimvars.Count);
        var primvarNames = allPrimvars.Select(p => p.GetPrimvarName().GetText()).ToList();
        Assert.Contains("var1", primvarNames);
        Assert.Contains("var2", primvarNames);

        // Test GetPrimvarNames
        var names = api.GetPrimvarNames();
        Assert.Equal(2, names.Count);
        var nameStrings = names.Select(n => n.GetText()).ToList();
        Assert.Contains("var1", nameStrings);
        Assert.Contains("var2", nameStrings);
    }

    [Fact]
    public void UsdGeomPrimvarsAPI_AuthoredAndValueQueries_Work()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        // Create primvar without value
        var primvarNoValue = api.CreatePrimvar(new TfToken("noValue"), "float[]");
        
        // Create primvar with value
        var primvarWithValue = api.CreateNonIndexedPrimvar(
            new TfToken("withValue"), 
            "float[]", 
            new List<float> { 1.0f, 2.0f, 3.0f });

        // Test authored primvars
        var authoredPrimvars = api.GetAuthoredPrimvars();
        Assert.Equal(2, authoredPrimvars.Count);

        // Test primvars with values
        var primvarsWithValues = api.GetPrimvarsWithValues();
        Assert.Single(primvarsWithValues);
        Assert.Equal("withValue", primvarsWithValues[0].GetPrimvarName().GetText());

        // Test primvars with authored values
        var primvarsWithAuthoredValues = api.GetPrimvarsWithAuthoredValues();
        Assert.Equal(2, primvarsWithAuthoredValues.Count); // Both are authored, but only one has value
    }

    #endregion

    #region Primvar Removal and Blocking Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_RemoveAndBlock_Work()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        // Create primvar with value and indices
        var primvar = api.CreateIndexedPrimvar(
            new TfToken("testVar"),
            "float[]",
            new List<float> { 1.0f, 2.0f, 3.0f },
            new List<int> { 0, 1, 2, 1 });

        Assert.True(api.HasPrimvar(new TfToken("testVar")));
        Assert.True(primvar.IsIndexed());

        // Test removal (blocking)
        Assert.True(api.RemovePrimvar(new TfToken("testVar")));
        
        // Should still exist but be blocked
        var removedPrimvar = api.GetPrimvar(new TfToken("testVar"));
        Assert.True(removedPrimvar.IsValid()); // Attribute still exists
        Assert.False(removedPrimvar.HasValue()); // But has no value due to blocking

        // Test removing non-existent primvar
        Assert.False(api.RemovePrimvar(new TfToken("nonexistent")));

        // Test blocking
        var blockPrimvar = api.CreateNonIndexedPrimvar(
            new TfToken("blockTest"),
            "float[]",
            new List<float> { 4.0f, 5.0f });

        Assert.True(blockPrimvar.HasValue());
        
        api.BlockPrimvar(new TfToken("blockTest"));
        
        var blockedPrimvar = api.GetPrimvar(new TfToken("blockTest"));
        Assert.True(blockedPrimvar.IsValid());
        Assert.False(blockedPrimvar.HasValue()); // Should be blocked
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_Inheritance_Works()
    {
        var stage = CreateTestStage();
        
        // Create hierarchy: /Root/Parent/Child
        var rootPath = new SdfPath("/Root");
        var parentPath = new SdfPath("/Root/Parent");
        var childPath = new SdfPath("/Root/Parent/Child");
        
        var rootPrim = stage.DefinePrim(rootPath);
        var parentPrim = stage.DefinePrim(parentPath);
        var childPrim = stage.DefinePrim(childPath);
        
        var rootAPI = new UsdGeomPrimvarsAPI(rootPrim);
        var parentAPI = new UsdGeomPrimvarsAPI(parentPrim);
        var childAPI = new UsdGeomPrimvarsAPI(childPrim);

        // Add constant primvar to root (inheritable)
        var rootConstantVar = rootAPI.CreateNonIndexedPrimvar(
            new TfToken("inheritableVar"),
            "float[]",
            new List<float> { 1.0f },
            UsdGeomInterpolation.Constant);

        // Add vertex primvar to parent (not inheritable)
        var parentVertexVar = parentAPI.CreateNonIndexedPrimvar(
            new TfToken("nonInheritableVar"),
            "float[]",
            new List<float> { 2.0f },
            UsdGeomInterpolation.Vertex);

        // Add constant primvar to parent that overrides root
        var parentOverrideVar = parentAPI.CreateNonIndexedPrimvar(
            new TfToken("inheritableVar"),
            "float[]",
            new List<float> { 3.0f },
            UsdGeomInterpolation.Constant);

        // Test inheritance at child level
        var inheritedVar = childAPI.FindPrimvarWithInheritance(new TfToken("inheritableVar"));
        Assert.True(inheritedVar.IsValid());
        Assert.True(inheritedVar.Get(out List<float> inheritedValue));
        Assert.Equal(3.0f, inheritedValue[0]); // Should get parent's override, not root's

        // Non-inheritable variable should not be found
        var nonInheritedVar = childAPI.FindPrimvarWithInheritance(new TfToken("nonInheritableVar"));
        Assert.False(nonInheritedVar.IsValid());

        // Test FindPrimvarsWithInheritance
        var allInheritedPrimvars = childAPI.FindPrimvarsWithInheritance();
        Assert.Single(allInheritedPrimvars); // Only the constant one
        Assert.Equal("inheritableVar", allInheritedPrimvars[0].GetPrimvarName().GetText());

        // Test HasPossiblyInheritedPrimvar
        Assert.True(childAPI.HasPossiblyInheritedPrimvar(new TfToken("inheritableVar")));
        Assert.False(childAPI.HasPossiblyInheritedPrimvar(new TfToken("nonInheritableVar")));
        Assert.False(childAPI.HasPossiblyInheritedPrimvar(new TfToken("nonexistent")));

        // Test FindInheritablePrimvars
        var inheritablePrimvars = parentAPI.FindInheritablePrimvars();
        Assert.Single(inheritablePrimvars);
        Assert.Equal("inheritableVar", inheritablePrimvars[0].GetPrimvarName().GetText());
    }

    #endregion

    #region Utility Methods Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_UtilityMethods_Work()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);
        var api = new UsdGeomPrimvarsAPI(prim);

        // Test CanContainPropertyName
        Assert.True(UsdGeomPrimvarsAPI.CanContainPropertyName(new TfToken("primvars:testVar")));
        Assert.True(UsdGeomPrimvarsAPI.CanContainPropertyName(new TfToken("primvars:displayColor")));
        Assert.False(UsdGeomPrimvarsAPI.CanContainPropertyName(new TfToken("points")));
        Assert.False(UsdGeomPrimvarsAPI.CanContainPropertyName(new TfToken("normals")));

        // Test CreateDisplayPrimvars
        var colors = new List<GfVec3f>
        {
            new GfVec3f(0.8f, 0.2f, 0.1f),
            new GfVec3f(0.1f, 0.8f, 0.2f)
        };
        
        var opacities = new List<float> { 0.9f, 0.7f };

        api.CreateDisplayPrimvars(colors, opacities);

        // Verify display primvars were created
        Assert.True(api.HasPrimvar(new TfToken("displayColor")));
        Assert.True(api.HasPrimvar(new TfToken("displayOpacity")));

        var colorPrimvar = api.GetPrimvar(new TfToken("displayColor"));
        Assert.Equal(UsdGeomInterpolation.Constant, colorPrimvar.GetInterpolation());
        Assert.True(colorPrimvar.Get(out List<GfVec3f> retrievedColors));
        Assert.Equal(colors.Count, retrievedColors.Count);

        var opacityPrimvar = api.GetPrimvar(new TfToken("displayOpacity"));
        Assert.Equal(UsdGeomInterpolation.Constant, opacityPrimvar.GetInterpolation());
        Assert.True(opacityPrimvar.Get(out List<float> retrievedOpacities));
        Assert.Equal(opacities.Count, retrievedOpacities.Count);

        // Test with null parameters
        var api2 = new UsdGeomPrimvarsAPI(stage.DefinePrim(new SdfPath("/TestPrim2")));
        api2.CreateDisplayPrimvars(); // Should not crash with null parameters
    }

    #endregion

    #region Invalid API Tests

    [Fact]
    public void UsdGeomPrimvarsAPI_InvalidAPI_HandlesProperly()
    {
        var invalidAPI = new UsdGeomPrimvarsAPI();

        // All operations should fail gracefully or return empty results
        Assert.False(invalidAPI.IsValid);

        var invalidPrimvar = invalidAPI.CreatePrimvar(new TfToken("test"), "float[]");
        Assert.False(invalidPrimvar.IsValid());

        var invalidNonIndexed = invalidAPI.CreateNonIndexedPrimvar(
            new TfToken("test"), "float[]", new List<float>());
        Assert.False(invalidNonIndexed.IsValid());

        var invalidIndexed = invalidAPI.CreateIndexedPrimvar(
            new TfToken("test"), "float[]", new List<float>(), new List<int>());
        Assert.False(invalidIndexed.IsValid());

        Assert.False(invalidAPI.RemovePrimvar(new TfToken("test")));
        Assert.False(invalidAPI.HasPrimvar(new TfToken("test")));
        
        var nonexistentPrimvar = invalidAPI.GetPrimvar(new TfToken("test"));
        Assert.False(nonexistentPrimvar.IsValid());

        Assert.Empty(invalidAPI.GetPrimvars());
        Assert.Empty(invalidAPI.GetAuthoredPrimvars());
        Assert.Empty(invalidAPI.GetPrimvarsWithValues());
        Assert.Empty(invalidAPI.GetPrimvarsWithAuthoredValues());
        Assert.Empty(invalidAPI.GetPrimvarNames());
        Assert.Empty(invalidAPI.FindPrimvarsWithInheritance());
        Assert.Empty(invalidAPI.FindInheritablePrimvars());
        
        var invalidInherited = invalidAPI.FindPrimvarWithInheritance(new TfToken("test"));
        Assert.False(invalidInherited.IsValid());
        
        Assert.False(invalidAPI.HasPossiblyInheritedPrimvar(new TfToken("test")));
    }

    #endregion

    #region GfVec2f Helper Type (same as in UsdGeomPrimvarTests)

    public struct GfVec2f : IEquatable<GfVec2f>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public GfVec2f(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GfVec2f other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object? obj) => obj is GfVec2f other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(GfVec2f left, GfVec2f right) => left.Equals(right);
        public static bool operator !=(GfVec2f left, GfVec2f right) => !left.Equals(right);
    }

    #endregion
}