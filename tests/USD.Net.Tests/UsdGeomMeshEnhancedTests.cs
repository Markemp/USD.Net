using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdGeomMeshEnhancedTests
{
    [Fact]
    public void UsdGeomMesh_CreateUVSet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act
        var uvSet = mesh.CreateUVSet("diffuse_uv", UsdGeomInterpolation.FaceVarying);

        // Assert
        Assert.True(uvSet.IsDefined());
        Assert.Equal("diffuse_uv", uvSet.GetPrimvarName().GetText());
        Assert.Equal("texCoord2f", uvSet.GetTypeName());
        Assert.Equal(UsdGeomInterpolation.FaceVarying, uvSet.GetInterpolation());
    }

    [Fact]
    public void UsdGeomMesh_SetAndGetUVCoordinates_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        var uvCoords = new List<GfVec2f>
        {
            new GfVec2f(0.0f, 0.0f),
            new GfVec2f(1.0f, 0.0f),
            new GfVec2f(1.0f, 1.0f),
            new GfVec2f(0.0f, 1.0f)
        };

        // Act
        var result = mesh.SetUVCoordinates(uvCoords, "st");
        var retrievedUVs = mesh.GetUVCoordinates("st");

        // Assert
        Assert.True(result);
        Assert.Equal(4, retrievedUVs.Count);
        Assert.Equal(0.0f, retrievedUVs[0].X);
        Assert.Equal(0.0f, retrievedUVs[0].Y);
        Assert.Equal(1.0f, retrievedUVs[1].X);
        Assert.Equal(1.0f, retrievedUVs[2].Y);
    }

    [Fact]
    public void UsdGeomMesh_CreateGameUVSets_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        var uvSets = new Dictionary<string, List<GfVec2f>>
        {
            ["diffuse"] = new List<GfVec2f> { new GfVec2f(0, 0), new GfVec2f(1, 1) },
            ["normal"] = new List<GfVec2f> { new GfVec2f(0.5f, 0.5f), new GfVec2f(1, 0) },
            ["specular"] = new List<GfVec2f> { new GfVec2f(0, 1), new GfVec2f(1, 0) }
        };

        // Act
        mesh.CreateGameUVSets(uvSets);
        var uvSetNames = mesh.GetUVSetNames();

        // Assert
        Assert.Contains("diffuse", uvSetNames);
        Assert.Contains("normal", uvSetNames);
        Assert.Contains("specular", uvSetNames);
        
        var diffuseUVs = mesh.GetUVCoordinates("diffuse");
        Assert.Equal(2, diffuseUVs.Count);
        Assert.Equal(0, diffuseUVs[0].X);
        Assert.Equal(1, diffuseUVs[1].Y);
    }

    [Fact]
    public void UsdGeomMesh_CreateVertexColors_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act
        var colorPrimvar = mesh.CreateVertexColors("vertexColors", UsdGeomInterpolation.Vertex);

        // Assert
        Assert.True(colorPrimvar.IsDefined());
        Assert.Equal("vertexColors", colorPrimvar.GetPrimvarName().GetText());
        Assert.Equal("color3f", colorPrimvar.GetTypeName());
        Assert.Equal(UsdGeomInterpolation.Vertex, colorPrimvar.GetInterpolation());
    }

    [Fact]
    public void UsdGeomMesh_SetAndGetVertexColors_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        var colors = new List<GfVec3f>
        {
            new GfVec3f(1.0f, 0.0f, 0.0f), // Red
            new GfVec3f(0.0f, 1.0f, 0.0f), // Green
            new GfVec3f(0.0f, 0.0f, 1.0f), // Blue
            new GfVec3f(1.0f, 1.0f, 1.0f)  // White
        };

        // Act
        var result = mesh.SetVertexColors(colors, "displayColor");
        var retrievedColors = mesh.GetVertexColors("displayColor");

        // Assert
        Assert.True(result);
        Assert.Equal(4, retrievedColors.Count);
        Assert.Equal(1.0f, retrievedColors[0].X); // Red component
        Assert.Equal(1.0f, retrievedColors[1].Y); // Green component
        Assert.Equal(1.0f, retrievedColors[2].Z); // Blue component
    }

    [Fact]
    public void UsdGeomMesh_SetAndGetVertexAlpha_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        var alphas = new List<float> { 1.0f, 0.8f, 0.6f, 0.4f };

        // Act
        var result = mesh.SetVertexAlpha(alphas, "displayOpacity");
        var retrievedAlphas = mesh.GetVertexAlpha("displayOpacity");

        // Assert
        Assert.True(result);
        Assert.Equal(4, retrievedAlphas.Count);
        Assert.Equal(1.0f, retrievedAlphas[0]);
        Assert.Equal(0.8f, retrievedAlphas[1]);
        Assert.Equal(0.6f, retrievedAlphas[2]);
        Assert.Equal(0.4f, retrievedAlphas[3]);
    }

    [Fact]
    public void UsdGeomMesh_CreateCustomPrimvar_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act
        var customPrimvar = mesh.CreateCustomPrimvar("gameData", "int", UsdGeomInterpolation.Constant);

        // Assert
        Assert.True(customPrimvar.IsDefined());
        Assert.Equal("gameData", customPrimvar.GetPrimvarName().GetText());
        Assert.Equal("int", customPrimvar.GetTypeName());
        Assert.Equal(UsdGeomInterpolation.Constant, customPrimvar.GetInterpolation());
    }

    [Fact]
    public void UsdGeomMesh_SetAndGetCustomData_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act
        var setResult = mesh.SetCustomData("materialId", 42);
        var retrievedValue = mesh.GetCustomData<int>("materialId");

        // Assert
        Assert.True(setResult);
        Assert.Equal(42, retrievedValue);
    }

    [Fact]
    public void UsdGeomMesh_SetAndGetCustomData_String_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        var testString = "CryEngine_Asset_v1.2";

        // Act
        var setResult = mesh.SetCustomData("assetVersion", testString);
        var retrievedValue = mesh.GetCustomData<string>("assetVersion");

        // Assert
        Assert.True(setResult);
        Assert.Equal(testString, retrievedValue);
    }

    [Fact]
    public void UsdGeomMesh_GetAllCustomPrimvars_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        // Create multiple custom primvars
        mesh.SetCustomData("materialId", 42);
        mesh.SetCustomData("LODLevel", 2);
        mesh.CreateUVSet("diffuse_uv");
        mesh.CreateVertexColors("vertexColors");

        // Act
        var allPrimvars = mesh.GetAllCustomPrimvars();

        // Assert
        Assert.True(allPrimvars.Count >= 4);
        Assert.True(allPrimvars.ContainsKey("materialId"));
        Assert.True(allPrimvars.ContainsKey("LODLevel"));
        Assert.True(allPrimvars.ContainsKey("diffuse_uv"));
        Assert.True(allPrimvars.ContainsKey("vertexColors"));
    }

    [Fact]
    public void UsdGeomMesh_ConfigureForGameAsset_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act
        mesh.ConfigureForGameAsset();

        // Assert
        Assert.Equal("none", mesh.SubdivisionScheme);
        
        var uvSet = mesh.GetUVSet("st");
        Assert.NotNull(uvSet);
        Assert.True(uvSet.IsDefined());
    }

    [Fact]
    public void UsdGeomMesh_CreateGameAssetMesh_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        var vertices = new List<GfVec3f>
        {
            new GfVec3f(-1, -1, 0),
            new GfVec3f(1, -1, 0),
            new GfVec3f(1, 1, 0),
            new GfVec3f(-1, 1, 0)
        };
        
        var indices = new List<int> { 0, 1, 2, 3 };
        var faceCounts = new List<int> { 4 };
        
        var uvCoords = new List<GfVec2f>
        {
            new GfVec2f(0, 0),
            new GfVec2f(1, 0),
            new GfVec2f(1, 1),
            new GfVec2f(0, 1)
        };
        
        var normals = new List<GfVec3f>
        {
            new GfVec3f(0, 0, 1),
            new GfVec3f(0, 0, 1),
            new GfVec3f(0, 0, 1),
            new GfVec3f(0, 0, 1)
        };
        
        var colors = new List<GfVec3f>
        {
            new GfVec3f(1, 0, 0),
            new GfVec3f(0, 1, 0),
            new GfVec3f(0, 0, 1),
            new GfVec3f(1, 1, 1)
        };
        
        var additionalUVSets = new Dictionary<string, List<GfVec2f>>
        {
            ["lightmap"] = new List<GfVec2f>
            {
                new GfVec2f(0, 0),
                new GfVec2f(0.5f, 0.5f),
                new GfVec2f(1, 1),
                new GfVec2f(0.5f, 1)
            }
        };

        // Act
        mesh.CreateGameAssetMesh(vertices, indices, faceCounts, uvCoords, normals, colors, additionalUVSets);

        // Assert
        Assert.Equal(4, mesh.Points.Count);
        Assert.Equal(4, mesh.FaceVertexIndices.Count);
        Assert.Single(mesh.FaceVertexCounts);
        Assert.Equal("none", mesh.SubdivisionScheme);
        
        var retrievedUVs = mesh.GetUVCoordinates("st");
        Assert.Equal(4, retrievedUVs.Count);
        
        var retrievedColors = mesh.GetVertexColors("displayColor");
        Assert.Equal(4, retrievedColors.Count);
        
        var lightmapUVs = mesh.GetUVCoordinates("lightmap");
        Assert.Equal(4, lightmapUVs.Count);
        
        var uvSetNames = mesh.GetUVSetNames();
        Assert.Contains("st", uvSetNames);
        Assert.Contains("lightmap", uvSetNames);
    }

    [Fact]
    public void UsdGeomMesh_MultipleUVSets_HandledCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act - Create multiple UV sets with different data
        mesh.SetUVCoordinates(new List<GfVec2f> { new GfVec2f(0, 0), new GfVec2f(1, 1) }, "diffuse");
        mesh.SetUVCoordinates(new List<GfVec2f> { new GfVec2f(0.5f, 0.5f), new GfVec2f(0.8f, 0.2f) }, "normal");
        mesh.SetUVCoordinates(new List<GfVec2f> { new GfVec2f(0.1f, 0.9f), new GfVec2f(0.3f, 0.7f) }, "specular");

        // Assert
        var uvSetNames = mesh.GetUVSetNames();
        Assert.Equal(3, uvSetNames.Count);
        Assert.Contains("diffuse", uvSetNames);
        Assert.Contains("normal", uvSetNames);
        Assert.Contains("specular", uvSetNames);
        
        // Verify each UV set has correct data
        var diffuseUVs = mesh.GetUVCoordinates("diffuse");
        var normalUVs = mesh.GetUVCoordinates("normal");
        var specularUVs = mesh.GetUVCoordinates("specular");
        
        Assert.Equal(2, diffuseUVs.Count);
        Assert.Equal(2, normalUVs.Count);
        Assert.Equal(2, specularUVs.Count);
        
        Assert.Equal(0.5f, normalUVs[0].X);
        Assert.Equal(0.1f, specularUVs[0].X);
    }

    [Fact]
    public void UsdGeomMesh_InvalidCustomData_HandlesGracefully()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act - Try to get non-existent custom data
        var nonExistentValue = mesh.GetCustomData<int>("nonExistent");

        // Assert
        Assert.Equal(0, nonExistentValue); // Default int value
    }

    [Fact]
    public void UsdGeomMesh_EmptyUVSetName_HandlesGracefully()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);

        // Act
        var emptyUVs = mesh.GetUVCoordinates("");

        // Assert
        Assert.Empty(emptyUVs);
    }

    [Fact]
    public void UsdGeomMesh_VertexColorsWithDifferentNames_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/TestMesh");
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        var redColors = new List<GfVec3f> { new GfVec3f(1, 0, 0), new GfVec3f(1, 0, 0) };
        var blueColors = new List<GfVec3f> { new GfVec3f(0, 0, 1), new GfVec3f(0, 0, 1) };

        // Act
        mesh.SetVertexColors(redColors, "baseColor");
        mesh.SetVertexColors(blueColors, "emissiveColor");

        // Assert
        var retrievedRed = mesh.GetVertexColors("baseColor");
        var retrievedBlue = mesh.GetVertexColors("emissiveColor");
        
        Assert.Equal(2, retrievedRed.Count);
        Assert.Equal(2, retrievedBlue.Count);
        Assert.Equal(1.0f, retrievedRed[0].X); // Red
        Assert.Equal(1.0f, retrievedBlue[0].Z); // Blue
    }
}