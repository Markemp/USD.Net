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
public class UsdGeomPrimvarTests
{
    private UsdStage CreateTestStage()
    {
        return UsdStage.CreateInMemory();
    }

    #region UsdGeomPrimvarConstants Tests

    [Fact]
    public void UsdGeomPrimvarConstants_InterpolationConversion_Works()
    {
        // Test enum to string conversion
        Assert.Equal("constant", UsdGeomPrimvarConstants.InterpolationToString(UsdGeomInterpolation.Constant));
        Assert.Equal("uniform", UsdGeomPrimvarConstants.InterpolationToString(UsdGeomInterpolation.Uniform));
        Assert.Equal("varying", UsdGeomPrimvarConstants.InterpolationToString(UsdGeomInterpolation.Varying));
        Assert.Equal("vertex", UsdGeomPrimvarConstants.InterpolationToString(UsdGeomInterpolation.Vertex));
        Assert.Equal("faceVarying", UsdGeomPrimvarConstants.InterpolationToString(UsdGeomInterpolation.FaceVarying));

        // Test string to enum conversion
        Assert.Equal(UsdGeomInterpolation.Constant, UsdGeomPrimvarConstants.StringToInterpolation("constant"));
        Assert.Equal(UsdGeomInterpolation.Uniform, UsdGeomPrimvarConstants.StringToInterpolation("uniform"));
        Assert.Equal(UsdGeomInterpolation.Varying, UsdGeomPrimvarConstants.StringToInterpolation("varying"));
        Assert.Equal(UsdGeomInterpolation.Vertex, UsdGeomPrimvarConstants.StringToInterpolation("vertex"));
        Assert.Equal(UsdGeomInterpolation.FaceVarying, UsdGeomPrimvarConstants.StringToInterpolation("faceVarying"));

        // Test invalid string defaults to vertex
        Assert.Equal(UsdGeomInterpolation.Vertex, UsdGeomPrimvarConstants.StringToInterpolation("invalid"));
        Assert.Equal(UsdGeomInterpolation.Vertex, UsdGeomPrimvarConstants.StringToInterpolation(""));
    }

    [Fact]
    public void UsdGeomPrimvarConstants_NameValidation_Works()
    {
        // Valid names
        Assert.True(UsdGeomPrimvarConstants.IsValidPrimvarName("st"));
        Assert.True(UsdGeomPrimvarConstants.IsValidPrimvarName("displayColor"));
        Assert.True(UsdGeomPrimvarConstants.IsValidPrimvarName("primvars:st"));
        Assert.True(UsdGeomPrimvarConstants.IsValidPrimvarName("custom:namespace:name"));

        // Invalid names (reserved keywords)
        Assert.False(UsdGeomPrimvarConstants.IsValidPrimvarName("indices"));
        Assert.False(UsdGeomPrimvarConstants.IsValidPrimvarName("custom:indices"));
        Assert.False(UsdGeomPrimvarConstants.IsValidPrimvarName("primvars:indices"));

        // Invalid names (empty)
        Assert.False(UsdGeomPrimvarConstants.IsValidPrimvarName(""));
        Assert.False(UsdGeomPrimvarConstants.IsValidPrimvarName(string.Empty));
    }

    [Fact]
    public void UsdGeomPrimvarConstants_AttributeNameManipulation_Works()
    {
        // Test making primvar attribute names
        Assert.Equal("primvars:st", UsdGeomPrimvarConstants.MakePrimvarAttrName("st"));
        Assert.Equal("primvars:displayColor", UsdGeomPrimvarConstants.MakePrimvarAttrName("displayColor"));
        Assert.Equal("primvars:st", UsdGeomPrimvarConstants.MakePrimvarAttrName("primvars:st")); // Already has prefix

        // Test extracting base names
        Assert.Equal("st", UsdGeomPrimvarConstants.GetPrimvarBaseName("primvars:st"));
        Assert.Equal("displayColor", UsdGeomPrimvarConstants.GetPrimvarBaseName("primvars:displayColor"));
        Assert.Equal("st", UsdGeomPrimvarConstants.GetPrimvarBaseName("st")); // No prefix

        // Test checking if name is primvar
        Assert.True(UsdGeomPrimvarConstants.IsPrimvarName("primvars:st"));
        Assert.True(UsdGeomPrimvarConstants.IsPrimvarName("primvars:displayColor"));
        Assert.False(UsdGeomPrimvarConstants.IsPrimvarName("st"));
        Assert.False(UsdGeomPrimvarConstants.IsPrimvarName("points"));
    }

    #endregion

    #region UsdGeomPrimvar Basic Tests

    [Fact]
    public void UsdGeomPrimvar_Construction_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        // Create a primvar attribute
        var attr = prim.CreateAttribute(new TfToken("primvars:testVar"), "float[]");
        var primvar = new UsdGeomPrimvar(attr);

        Assert.True(primvar.IsValid());
        Assert.Equal("testVar", primvar.GetPrimvarName().GetText());

        // Test invalid primvar
        var invalidPrimvar = new UsdGeomPrimvar();
        Assert.False(invalidPrimvar.IsValid());
    }

    [Fact]
    public void UsdGeomPrimvar_PrimvarValidation_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        // Valid primvar attribute
        var primvarAttr = prim.CreateAttribute(new TfToken("primvars:testVar"), "float[]");
        Assert.True(UsdGeomPrimvar.IsPrimvar(primvarAttr));

        // Invalid primvar attribute (not in primvars namespace)
        var normalAttr = prim.CreateAttribute(new TfToken("points"), "point3f[]");
        Assert.False(UsdGeomPrimvar.IsPrimvar(normalAttr));

        // Test name validation
        Assert.True(UsdGeomPrimvar.IsValidPrimvarName(new TfToken("testVar")));
        Assert.True(UsdGeomPrimvar.IsValidPrimvarName("displayColor"));
        Assert.False(UsdGeomPrimvar.IsValidPrimvarName(new TfToken("indices")));
        Assert.False(UsdGeomPrimvar.IsValidPrimvarName(string.Empty));
    }

    [Fact]
    public void UsdGeomPrimvar_InterpolationHandling_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        var attr = prim.CreateAttribute(new TfToken("primvars:testVar"), "float[]");
        var primvar = new UsdGeomPrimvar(attr);

        // Default interpolation should be vertex
        Assert.Equal(UsdGeomInterpolation.Vertex, primvar.GetInterpolation());
        Assert.False(primvar.HasAuthoredInterpolation());

        // Set interpolation
        Assert.True(primvar.SetInterpolation(UsdGeomInterpolation.FaceVarying));
        Assert.Equal(UsdGeomInterpolation.FaceVarying, primvar.GetInterpolation());
        Assert.True(primvar.HasAuthoredInterpolation());

        // Change interpolation
        Assert.True(primvar.SetInterpolation(UsdGeomInterpolation.Constant));
        Assert.Equal(UsdGeomInterpolation.Constant, primvar.GetInterpolation());

        // Test interpolation validation
        Assert.True(UsdGeomPrimvar.IsValidInterpolation(UsdGeomInterpolation.Constant));
        Assert.True(UsdGeomPrimvar.IsValidInterpolation(UsdGeomInterpolation.FaceVarying));
    }

    [Fact]
    public void UsdGeomPrimvar_ElementSizeHandling_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        var attr = prim.CreateAttribute(new TfToken("primvars:testVar"), "float[]");
        var primvar = new UsdGeomPrimvar(attr);

        // Default element size should be 1
        Assert.Equal(1, primvar.GetElementSize());
        Assert.False(primvar.HasAuthoredElementSize());

        // Set element size
        Assert.True(primvar.SetElementSize(3));
        Assert.Equal(3, primvar.GetElementSize());
        Assert.True(primvar.HasAuthoredElementSize());

        // Invalid element size should fail
        Assert.False(primvar.SetElementSize(-1));
        Assert.False(primvar.SetElementSize(0));
    }

    [Fact]
    public void UsdGeomPrimvar_ValueAccess_Works()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        var attr = prim.CreateAttribute(new TfToken("primvars:colors"), "color3f[]");
        var primvar = new UsdGeomPrimvar(attr);

        // Initially should have no value
        Assert.False(primvar.HasValue());

        // Set value
        var colors = new List<GfVec3f>
        {
            new GfVec3f(1.0f, 0.0f, 0.0f),
            new GfVec3f(0.0f, 1.0f, 0.0f),
            new GfVec3f(0.0f, 0.0f, 1.0f)
        };

        Assert.True(primvar.Set(colors));
        Assert.True(primvar.HasValue());

        // Get value
        Assert.True(primvar.Get(out List<GfVec3f> retrievedColors));
        Assert.Equal(3, retrievedColors.Count);
        Assert.Equal(colors[0], retrievedColors[0]);
        Assert.Equal(colors[1], retrievedColors[1]);
        Assert.Equal(colors[2], retrievedColors[2]);

        // Test type name
        Assert.Equal("color3f[]", primvar.GetTypeName());
    }

    #endregion

    #region Indexed Primvar Tests

    [Fact]
    public void UsdGeomPrimvar_IndexedPrimvars_Work()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        var attr = prim.CreateAttribute(new TfToken("primvars:uvs"), "float2[]");
        var primvar = new UsdGeomPrimvar(attr);

        // Initially not indexed
        Assert.False(primvar.IsIndexed());

        // Set values and indices to make it indexed
        var uvValues = new List<GfVec2f>
        {
            new GfVec2f(0.0f, 0.0f),
            new GfVec2f(1.0f, 0.0f),
            new GfVec2f(1.0f, 1.0f),
            new GfVec2f(0.0f, 1.0f)
        };

        var indices = new List<int> { 0, 1, 2, 2, 3, 0 }; // Two triangles sharing vertices

        Assert.True(primvar.Set(uvValues));
        Assert.True(primvar.SetIndices(indices));
        Assert.True(primvar.IsIndexed());

        // Test getting indices
        Assert.True(primvar.GetIndices(out var retrievedIndices));
        Assert.Equal(indices.Count, retrievedIndices.Count);
        for (int i = 0; i < indices.Count; i++)
        {
            Assert.Equal(indices[i], retrievedIndices[i]);
        }

        // Test flattening
        Assert.True(primvar.ComputeFlattened(out List<GfVec2f> flattened));
        Assert.Equal(6, flattened.Count); // Should expand to match indices
        Assert.Equal(uvValues[0], flattened[0]); // Index 0
        Assert.Equal(uvValues[1], flattened[1]); // Index 1
        Assert.Equal(uvValues[2], flattened[2]); // Index 2
        Assert.Equal(uvValues[2], flattened[3]); // Index 2 again
        Assert.Equal(uvValues[3], flattened[4]); // Index 3
        Assert.Equal(uvValues[0], flattened[5]); // Index 0 again

        // Test blocking indices
        primvar.BlockIndices();
        Assert.False(primvar.IsIndexed());

        // Non-indexed flattening should return original values
        Assert.True(primvar.ComputeFlattened(out List<GfVec2f> nonIndexedFlattened));
        Assert.Equal(uvValues.Count, nonIndexedFlattened.Count);
        for (int i = 0; i < uvValues.Count; i++)
        {
            Assert.Equal(uvValues[i], nonIndexedFlattened[i]);
        }
    }

    [Fact]
    public void UsdGeomPrimvar_IndexedPrimvars_InvalidIndices_HandleCorrectly()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        var attr = prim.CreateAttribute(new TfToken("primvars:testVar"), "float[]");
        var primvar = new UsdGeomPrimvar(attr);

        // Set values
        var values = new List<float> { 1.0f, 2.0f, 3.0f };
        Assert.True(primvar.Set(values));

        // Set invalid indices (out of range)
        var invalidIndices = new List<int> { 0, 1, 5 }; // Index 5 is out of range
        Assert.True(primvar.SetIndices(invalidIndices));

        // Flattening should fail due to invalid index
        Assert.False(primvar.ComputeFlattened(out List<float> flattened));
    }

    #endregion

    #region Utility Methods Tests

    [Fact]
    public void UsdGeomPrimvar_UtilityMethods_Work()
    {
        var stage = CreateTestStage();
        var primPath = new SdfPath("/TestPrim");
        var prim = stage.DefinePrim(primPath);

        var attr = prim.CreateAttribute(new TfToken("primvars:custom:namespace:name"), "float[]");
        var primvar = new UsdGeomPrimvar(attr);

        // Test name extraction
        Assert.Equal("custom:namespace:name", primvar.GetPrimvarName().GetText());
        Assert.True(primvar.NameContainsNamespaces());

        // Test prim access
        Assert.True(primvar.GetPrim().IsValid());
        Assert.Equal(primPath, primvar.GetPrim().GetPath());

        // Test declaration info
        primvar.SetInterpolation(UsdGeomInterpolation.FaceVarying);
        primvar.SetElementSize(2);

        primvar.GetDeclarationInfo(out var name, out var typeName, out var interpolation, out var elementSize);
        Assert.Equal("custom:namespace:name", name.GetText());
        Assert.Equal("float[]", typeName);
        Assert.Equal(UsdGeomInterpolation.FaceVarying, interpolation);
        Assert.Equal(2, elementSize);

        // Test string representation
        var stringRep = primvar.ToString();
        Assert.Contains("custom:namespace:name", stringRep);
        Assert.Contains("float[]", stringRep);
        Assert.Contains("FaceVarying", stringRep);
    }

    [Fact]
    public void UsdGeomPrimvar_InvalidPrimvar_HandlesProperly()
    {
        var invalidPrimvar = new UsdGeomPrimvar();

        Assert.False(invalidPrimvar.IsValid());
        Assert.False(invalidPrimvar.HasValue());
        Assert.Equal(UsdGeomInterpolation.Vertex, invalidPrimvar.GetInterpolation()); // Default
        Assert.Equal(1, invalidPrimvar.GetElementSize()); // Default
        Assert.False(invalidPrimvar.SetInterpolation(UsdGeomInterpolation.Constant));
        Assert.False(invalidPrimvar.SetElementSize(3));
        Assert.False(invalidPrimvar.Get(out List<float> values));
        Assert.False(invalidPrimvar.Set(new List<float>()));
        Assert.False(invalidPrimvar.IsIndexed());
        Assert.Empty(invalidPrimvar.GetPrimvarName().GetText());
        Assert.False(invalidPrimvar.NameContainsNamespaces());
        Assert.False(invalidPrimvar.GetPrim().IsValid());
        Assert.Contains("Invalid", invalidPrimvar.ToString());
    }

    #endregion

    #region GfVec2f Helper Type for UV Testing

    // Simple 2D vector for UV coordinate testing
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