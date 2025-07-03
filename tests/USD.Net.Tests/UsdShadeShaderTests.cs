using System;
using System.Linq;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdShade;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdShadeShaderTests
{
    [Fact]
    public void UsdShadeShader_Define_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");

        // Act
        var shader = UsdShadeShader.Define(stage, shaderPath);

        // Assert
        Assert.True(shader.IsValid);
        Assert.Equal(shaderPath, shader.Path);
        Assert.Equal("Shader", shader.Prim.GetTypeName());
    }

    [Fact]
    public void UsdShadeShader_Get_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        stage.DefinePrim(shaderPath, "Shader");

        // Act
        var shader = UsdShadeShader.Get(stage, shaderPath);

        // Assert
        Assert.True(shader.IsValid);
        Assert.Equal(shaderPath, shader.Path);
    }

    [Fact]
    public void UsdShadeShader_ConnectableAPI_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        // Act
        var connectableAPI = shader.ConnectableAPI;

        // Assert
        Assert.True(connectableAPI.IsValid());
        Assert.Equal(shader.Prim, connectableAPI.GetPrim());
    }

    [Fact]
    public void UsdShadeShader_CreateInput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var inputName = new TfToken("diffuseColor");
        var typeName = "color3f";

        // Act
        var input = shader.CreateInput(inputName, typeName);

        // Assert
        Assert.True(input.IsValid());
        Assert.Equal(inputName, input.GetBaseName());
        Assert.Equal(typeName, input.GetTypeName());
    }

    [Fact]
    public void UsdShadeShader_GetInput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var inputName = new TfToken("diffuseColor");
        var typeName = "color3f";

        shader.CreateInput(inputName, typeName);

        // Act
        var retrievedInput = shader.GetInput(inputName);

        // Assert
        Assert.True(retrievedInput.IsValid());
        Assert.Equal(inputName, retrievedInput.GetBaseName());
    }

    [Fact]
    public void UsdShadeShader_CreateOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var outputName = new TfToken("surface");
        var typeName = "token";

        // Act
        var output = shader.CreateOutput(outputName, typeName);

        // Assert
        Assert.True(output.IsValid());
        Assert.Equal(outputName, output.GetBaseName());
        Assert.Equal(typeName, output.GetTypeName());
    }

    [Fact]
    public void UsdShadeShader_GetOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var outputName = new TfToken("surface");
        var typeName = "token";

        shader.CreateOutput(outputName, typeName);

        // Act
        var retrievedOutput = shader.GetOutput(outputName);

        // Assert
        Assert.True(retrievedOutput.IsValid());
        Assert.Equal(outputName, retrievedOutput.GetBaseName());
    }

    [Fact]
    public void UsdShadeShader_GetInputsOutputs_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        shader.CreateInput(new TfToken("diffuseColor"), "color3f");
        shader.CreateInput(new TfToken("metallic"), "float");
        shader.CreateOutput(new TfToken("surface"), "token");
        shader.CreateOutput(new TfToken("displacement"), "token");

        // Act
        var inputs = shader.GetInputs();
        var outputs = shader.GetOutputs();

        // Assert
        Assert.Equal(2, inputs.Length);
        Assert.Equal(2, outputs.Length);

        var inputNames = inputs.Select(i => i.GetBaseName().GetText()).ToArray();
        var outputNames = outputs.Select(o => o.GetBaseName().GetText()).ToArray();

        Assert.Contains("diffuseColor", inputNames);
        Assert.Contains("metallic", inputNames);
        Assert.Contains("surface", outputNames);
        Assert.Contains("displacement", outputNames);
    }

    [Fact]
    public void UsdShadeShader_SetGetShaderId_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var shaderId = new TfToken("UsdPreviewSurface");

        // Act
        var setResult = shader.SetShaderId(shaderId);
        var retrievedId = shader.GetShaderId();

        // Assert
        Assert.True(setResult);
        Assert.Equal(shaderId, retrievedId);
    }

    [Fact]
    public void UsdShadeShader_SetGetImplementationSource_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var implSource = new TfToken("sourceAsset");

        // Act
        var setResult = shader.SetImplementationSource(implSource);
        var retrievedSource = shader.GetImplementationSource();

        // Assert
        Assert.True(setResult);
        Assert.Equal(implSource, retrievedSource);
    }

    [Fact]
    public void UsdShadeShader_SetGetSourceAsset_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var assetPath = new SdfAssetPath("shaders/myshader.glsl");
        var subIdentifier = "mainFunction";

        // Act
        var setResult = shader.SetSourceAsset(assetPath, subIdentifier);
        var retrievedAsset = shader.GetSourceAsset();

        // Assert
        Assert.True(setResult);
        Assert.Equal(assetPath.GetAssetPath(), retrievedAsset.GetAssetPath());
    }

    [Fact]
    public void UsdShadeShader_SetGetSourceCode_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        var sourceCode = "float4 surface() { return float4(1,0,0,1); }";
        var language = "hlsl";

        // Act
        var setResult = shader.SetSourceCode(sourceCode, language);
        var retrievedCode = shader.GetSourceCode();

        // Assert
        Assert.True(setResult);
        Assert.Equal(sourceCode, retrievedCode);
    }

    [Fact]
    public void UsdShadeShader_CreateUsdPreviewSurface_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/PreviewSurface");

        // Act
        var shader = UsdShadeShader.CreateUsdPreviewSurface(stage, shaderPath);

        // Assert
        Assert.True(shader.IsValid);
        Assert.Equal(new TfToken("UsdPreviewSurface"), shader.GetShaderId());
        Assert.Equal(new TfToken("id"), shader.GetImplementationSource());

        // Check that PBR inputs were created
        Assert.True(shader.HasShaderInput(new TfToken("diffuseColor")));
        Assert.True(shader.HasShaderInput(new TfToken("metallic")));
        Assert.True(shader.HasShaderInput(new TfToken("roughness")));
        Assert.True(shader.HasShaderInput(new TfToken("normal")));

        // Check that outputs were created
        Assert.True(shader.HasShaderOutput(new TfToken("surface")));
        Assert.True(shader.HasShaderOutput(new TfToken("displacement")));
    }

    [Fact]
    public void UsdShadeShader_CreateUsdUVTexture_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/UVTexture");

        // Act
        var shader = UsdShadeShader.CreateUsdUVTexture(stage, shaderPath);

        // Assert
        Assert.True(shader.IsValid);
        Assert.Equal(new TfToken("UsdUVTexture"), shader.GetShaderId());
        Assert.Equal(new TfToken("id"), shader.GetImplementationSource());

        // Check that texture inputs were created
        Assert.True(shader.HasShaderInput(new TfToken("file")));
        Assert.True(shader.HasShaderInput(new TfToken("st")));
        Assert.True(shader.HasShaderInput(new TfToken("wrapS")));
        Assert.True(shader.HasShaderInput(new TfToken("wrapT")));

        // Check that outputs were created
        Assert.True(shader.HasShaderOutput(new TfToken("r")));
        Assert.True(shader.HasShaderOutput(new TfToken("g")));
        Assert.True(shader.HasShaderOutput(new TfToken("b")));
        Assert.True(shader.HasShaderOutput(new TfToken("a")));
        Assert.True(shader.HasShaderOutput(new TfToken("rgb")));
        Assert.True(shader.HasShaderOutput(new TfToken("rgba")));
    }

    [Fact]
    public void UsdShadeShader_CreateUsdPrimvarReader_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/PrimvarReader");
        var primvarType = "float2";

        // Act
        var shader = UsdShadeShader.CreateUsdPrimvarReader(stage, shaderPath, primvarType);

        // Assert
        Assert.True(shader.IsValid);
        Assert.Equal(new TfToken($"UsdPrimvarReader_{primvarType}"), shader.GetShaderId());
        Assert.Equal(new TfToken("id"), shader.GetImplementationSource());

        // Check that primvar inputs were created
        Assert.True(shader.HasShaderInput(new TfToken("varname")));
        Assert.True(shader.HasShaderInput(new TfToken("fallback")));

        // Check that output was created
        Assert.True(shader.HasShaderOutput(new TfToken("result")));

        // Check types
        var fallbackInput = shader.GetInput(new TfToken("fallback"));
        var resultOutput = shader.GetOutput(new TfToken("result"));
        Assert.Equal(primvarType, fallbackInput.GetTypeName());
        Assert.Equal(primvarType, resultOutput.GetTypeName());
    }

    [Fact]
    public void UsdShadeShader_IsShader_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        // Act
        var isShader = shader.IsShader();

        // Assert
        Assert.True(isShader);
    }

    [Fact]
    public void UsdShadeShader_GetShaderInputNames_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        shader.CreateInput(new TfToken("diffuseColor"), "color3f");
        shader.CreateInput(new TfToken("metallic"), "float");
        shader.CreateInput(new TfToken("roughness"), "float");

        // Act
        var inputNames = shader.GetShaderInputNames();

        // Assert
        Assert.Equal(3, inputNames.Length);
        var names = inputNames.Select(t => t.GetText()).ToArray();
        Assert.Contains("diffuseColor", names);
        Assert.Contains("metallic", names);
        Assert.Contains("roughness", names);
    }

    [Fact]
    public void UsdShadeShader_GetShaderOutputNames_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        shader.CreateOutput(new TfToken("surface"), "token");
        shader.CreateOutput(new TfToken("displacement"), "token");

        // Act
        var outputNames = shader.GetShaderOutputNames();

        // Assert
        Assert.Equal(2, outputNames.Length);
        var names = outputNames.Select(t => t.GetText()).ToArray();
        Assert.Contains("surface", names);
        Assert.Contains("displacement", names);
    }

    [Fact]
    public void UsdShadeShader_HasShaderInputOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        shader.CreateInput(new TfToken("diffuseColor"), "color3f");
        shader.CreateOutput(new TfToken("surface"), "token");

        // Act & Assert
        Assert.True(shader.HasShaderInput(new TfToken("diffuseColor")));
        Assert.False(shader.HasShaderInput(new TfToken("nonexistent")));

        Assert.True(shader.HasShaderOutput(new TfToken("surface")));
        Assert.False(shader.HasShaderOutput(new TfToken("nonexistent")));
    }

    [Fact]
    public void UsdShadeShader_GetShaderMetadata_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);
        
        var shaderId = new TfToken("UsdPreviewSurface");
        var implSource = new TfToken("id");
        var sourceCode = "float4 surface() { return float4(1,0,0,1); }";

        shader.SetShaderId(shaderId);
        shader.SetImplementationSource(implSource);
        shader.SetSourceCode(sourceCode);

        // Act
        var metadata = shader.GetShaderMetadata();

        // Assert
        Assert.True(metadata.ContainsKey(new TfToken("id")));
        Assert.True(metadata.ContainsKey(new TfToken("implementationSource")));
        Assert.True(metadata.ContainsKey(new TfToken("sourceCode")));

        Assert.Equal(shaderId, metadata[new TfToken("id")]);
        Assert.Equal(implSource, metadata[new TfToken("implementationSource")]);
        Assert.Equal(sourceCode, metadata[new TfToken("sourceCode")]);
    }

    [Fact]
    public void UsdShadeShader_InvalidShader_HandlesGracefully()
    {
        // Arrange
        var invalidShader = new UsdShadeShader();

        // Act & Assert
        Assert.False(invalidShader.IsValid);
        Assert.True(invalidShader.GetShaderId().IsEmpty);
        Assert.Empty(invalidShader.GetShaderInputNames());
        Assert.Empty(invalidShader.GetShaderOutputNames());
        Assert.False(invalidShader.HasShaderInput(new TfToken("test")));
        Assert.False(invalidShader.HasShaderOutput(new TfToken("test")));
        Assert.Empty(invalidShader.GetShaderMetadata());
    }

    [Fact]
    public void UsdShadeShader_DefaultImplementationSource_IsId()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var shader = UsdShadeShader.Define(stage, shaderPath);

        // Act
        var implSource = shader.GetImplementationSource();

        // Assert - Should return "id" as default implementation source
        Assert.Equal(new TfToken("id"), implSource);
    }

    [Fact]
    public void UsdShadeShader_ComplexShaderNetwork_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var texturePath = new SdfPath("/Texture");
        var surfacePath = new SdfPath("/Surface");

        // Create texture shader
        var textureShader = UsdShadeShader.CreateUsdUVTexture(stage, texturePath);
        var diffuseTexture = new SdfAssetPath("textures/diffuse.jpg");
        textureShader.GetInput(new TfToken("file")).Set(diffuseTexture);

        // Create surface shader
        var surfaceShader = UsdShadeShader.CreateUsdPreviewSurface(stage, surfacePath);

        // Act - Connect texture to surface
        var textureOutput = textureShader.GetOutput(new TfToken("rgb"));
        var surfaceInput = surfaceShader.GetInput(new TfToken("diffuseColor"));
        var connectionResult = surfaceInput.ConnectToSource(textureOutput);

        // Assert
        Assert.True(connectionResult);
        Assert.True(surfaceInput.HasConnectedSource());
        
        var connectedSources = surfaceInput.GetConnectedSources();
        Assert.Single(connectedSources);
        Assert.Equal(textureOutput.GetAttr().GetPath(), connectedSources[0]);
    }
}