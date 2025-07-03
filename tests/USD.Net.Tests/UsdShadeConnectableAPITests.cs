using System;
using System.Linq;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdShade;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdShadeConnectableAPITests
{
    [Fact]
    public void UsdShadeConnectableAPI_BasicConstruction_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");

        // Act
        var connectableAPI = new UsdShadeConnectableAPI(prim);

        // Assert
        Assert.True(connectableAPI.IsValid());
        Assert.Equal(prim, connectableAPI.GetPrim());
        Assert.Equal(UsdSchemaKind.SingleApplyAPI, connectableAPI.GetSchemaType());
    }

    [Fact]
    public void UsdShadeConnectableAPI_Apply_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");

        // Act
        var connectableAPI = UsdShadeConnectableAPI.Apply(prim);

        // Assert
        Assert.True(connectableAPI.IsValid());
        Assert.Equal(prim, connectableAPI.GetPrim());
    }

    [Fact]
    public void UsdShadeConnectableAPI_Get_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        stage.DefinePrim(shaderPath, "Shader");

        // Act
        var connectableAPI = UsdShadeConnectableAPI.Get(stage, shaderPath);

        // Assert
        Assert.True(connectableAPI.IsValid());
        Assert.Equal(shaderPath, connectableAPI.GetPrim().GetPath());
    }

    [Fact]
    public void UsdShadeConnectableAPI_CreateInput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);
        var inputName = new TfToken("diffuseColor");
        var typeName = "color3f";

        // Act
        var input = connectableAPI.CreateInput(inputName, typeName);

        // Assert
        Assert.True(input.IsValid());
        Assert.Equal(inputName, input.GetBaseName());
        Assert.Equal(typeName, input.GetTypeName());
    }

    [Fact]
    public void UsdShadeConnectableAPI_GetInput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);
        var inputName = new TfToken("diffuseColor");
        var typeName = "color3f";

        // Create input first
        connectableAPI.CreateInput(inputName, typeName);

        // Act
        var retrievedInput = connectableAPI.GetInput(inputName);

        // Assert
        Assert.True(retrievedInput.IsValid());
        Assert.Equal(inputName, retrievedInput.GetBaseName());
        Assert.Equal(typeName, retrievedInput.GetTypeName());
    }

    [Fact]
    public void UsdShadeConnectableAPI_CreateOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);
        var outputName = new TfToken("surface");
        var typeName = "token";

        // Act
        var output = connectableAPI.CreateOutput(outputName, typeName);

        // Assert
        Assert.True(output.IsValid());
        Assert.Equal(outputName, output.GetBaseName());
        Assert.Equal(typeName, output.GetTypeName());
    }

    [Fact]
    public void UsdShadeConnectableAPI_GetOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);
        var outputName = new TfToken("surface");
        var typeName = "token";

        // Create output first
        connectableAPI.CreateOutput(outputName, typeName);

        // Act
        var retrievedOutput = connectableAPI.GetOutput(outputName);

        // Assert
        Assert.True(retrievedOutput.IsValid());
        Assert.Equal(outputName, retrievedOutput.GetBaseName());
        Assert.Equal(typeName, retrievedOutput.GetTypeName());
    }

    [Fact]
    public void UsdShadeConnectableAPI_GetInputs_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);

        // Create multiple inputs
        connectableAPI.CreateInput(new TfToken("diffuseColor"), "color3f");
        connectableAPI.CreateInput(new TfToken("metallic"), "float");
        connectableAPI.CreateInput(new TfToken("roughness"), "float");

        // Act
        var inputs = connectableAPI.GetInputs();

        // Assert
        Assert.Equal(3, inputs.Length);
        var inputNames = inputs.Select(i => i.GetBaseName().GetText()).ToArray();
        Assert.Contains("diffuseColor", inputNames);
        Assert.Contains("metallic", inputNames);
        Assert.Contains("roughness", inputNames);
    }

    [Fact]
    public void UsdShadeConnectableAPI_GetOutputs_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);

        // Create multiple outputs
        connectableAPI.CreateOutput(new TfToken("surface"), "token");
        connectableAPI.CreateOutput(new TfToken("displacement"), "token");

        // Act
        var outputs = connectableAPI.GetOutputs();

        // Assert
        Assert.Equal(2, outputs.Length);
        var outputNames = outputs.Select(o => o.GetBaseName().GetText()).ToArray();
        Assert.Contains("surface", outputNames);
        Assert.Contains("displacement", outputNames);
    }

    [Fact]
    public void UsdShadeConnectableAPI_ConnectToSource_WithPath_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("color"), "color3f");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");

        // Act
        var result = UsdShadeConnectableAPI.ConnectToSource(targetInput.GetAttr(), sourceOutput.GetAttr().GetPath());

        // Assert
        Assert.True(result);
        Assert.True(targetInput.HasConnectedSource());
        var connectedSources = targetInput.GetConnectedSources();
        Assert.Single(connectedSources);
        Assert.Equal(sourceOutput.GetAttr().GetPath(), connectedSources[0]);
    }

    [Fact]
    public void UsdShadeConnectableAPI_ConnectToSource_WithOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("color"), "color3f");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");

        // Act
        var result = UsdShadeConnectableAPI.ConnectToSource(targetInput.GetAttr(), sourceOutput);

        // Assert
        Assert.True(result);
        Assert.True(targetInput.HasConnectedSource());
    }

    [Fact]
    public void UsdShadeConnectableAPI_ConnectToSource_WithInput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceInput = UsdShadeInput.CreateInput(sourcePrim, new TfToken("value"), "float");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("multiplier"), "float");

        // Act
        var result = UsdShadeConnectableAPI.ConnectToSource(targetInput.GetAttr(), sourceInput);

        // Assert
        Assert.True(result);
        Assert.True(targetInput.HasConnectedSource());
    }

    [Fact]
    public void UsdShadeConnectableAPI_ClearSources_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("color"), "color3f");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");

        // Connect first
        UsdShadeConnectableAPI.ConnectToSource(targetInput.GetAttr(), sourceOutput);
        Assert.True(targetInput.HasConnectedSource());

        // Act
        var result = UsdShadeConnectableAPI.ClearSources(targetInput.GetAttr());

        // Assert
        Assert.True(result);
        Assert.False(targetInput.HasConnectedSource());
        Assert.Empty(targetInput.GetConnectedSources());
    }

    [Fact]
    public void UsdShadeConnectableAPI_GetConnectedSource_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceConnectableAPI = new UsdShadeConnectableAPI(sourcePrim);
        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("color"), "color3f");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");

        // Connect first
        UsdShadeConnectableAPI.ConnectToSource(targetInput.GetAttr(), sourceOutput);

        // Act
        var result = UsdShadeConnectableAPI.GetConnectedSource(targetInput.GetAttr(), out var connectedAPI, out var sourceName, out var sourceType);

        // Assert
        Assert.True(result);
        Assert.Equal(sourceConnectableAPI.GetPrim().GetPath(), connectedAPI.GetPrim().GetPath());
        Assert.Equal(new TfToken("color"), sourceName);
        Assert.Equal(UsdShadeAttributeType.Output, sourceType);
    }

    [Fact]
    public void UsdShadeConnectableAPI_CanConnect_SameType_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("color"), "color3f");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");

        // Act
        var canConnect = UsdShadeConnectableAPI.CanConnect(targetInput.GetAttr(), sourceOutput.GetAttr());

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public void UsdShadeConnectableAPI_CanConnect_DifferentType_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("value"), "float");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");

        // Act
        var canConnect = UsdShadeConnectableAPI.CanConnect(targetInput.GetAttr(), sourceOutput.GetAttr());

        // Assert - Note: might be true due to type compatibility rules, but for this test we expect false
        // The actual result depends on the IsCompatibleType implementation
        Assert.False(canConnect);
    }

    [Fact]
    public void UsdShadeConnectableAPI_CanConnect_CompatibleTypes_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("value"), "float");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("vector"), "vector3f");

        // Act
        var canConnect = UsdShadeConnectableAPI.CanConnect(targetInput.GetAttr(), sourceOutput.GetAttr());

        // Assert - Based on the IsCompatibleType implementation, float should connect to vector
        Assert.True(canConnect);
    }

    [Fact]
    public void UsdShadeConnectableAPI_HasConnectableAPI_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var connectableAPI = new UsdShadeConnectableAPI(prim);

        // Act
        var hasAPI = connectableAPI.HasConnectableAPI();

        // Assert
        Assert.True(hasAPI); // Our implementation assumes all prims can be connectable
    }

    [Fact]
    public void UsdShadeConnectableAPI_InvalidPrim_HandlesGracefully()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        var connectableAPI = new UsdShadeConnectableAPI(invalidPrim);

        // Act & Assert
        Assert.False(connectableAPI.IsValid());
        Assert.False(connectableAPI.HasConnectableAPI());
        Assert.Contains("invalid", connectableAPI.ToString().ToLower());

        // Creating inputs/outputs should return invalid objects
        var input = connectableAPI.CreateInput(new TfToken("test"), "float");
        var output = connectableAPI.CreateOutput(new TfToken("test"), "float");
        Assert.False(input.IsValid());
        Assert.False(output.IsValid());
    }

    [Fact]
    public void UsdShadeConnectableAPI_EqualityComparison_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath1 = new SdfPath("/TestShader1");
        var shaderPath2 = new SdfPath("/TestShader2");
        var prim1 = stage.DefinePrim(shaderPath1, "Shader");
        var prim2 = stage.DefinePrim(shaderPath2, "Shader");

        var connectableAPI1a = new UsdShadeConnectableAPI(prim1);
        var connectableAPI1b = new UsdShadeConnectableAPI(prim1);
        var connectableAPI2 = new UsdShadeConnectableAPI(prim2);

        // Act & Assert
        Assert.Equal(connectableAPI1a, connectableAPI1b);
        Assert.True(connectableAPI1a == connectableAPI1b);
        Assert.False(connectableAPI1a != connectableAPI1b);

        Assert.NotEqual(connectableAPI1a, connectableAPI2);
        Assert.False(connectableAPI1a == connectableAPI2);
        Assert.True(connectableAPI1a != connectableAPI2);
    }
}