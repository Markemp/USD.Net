using System;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdShade;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdShadeInputOutputTests
{
    [Fact]
    public void UsdShadeInput_CreateAndGetInput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var inputName = new TfToken("diffuseColor");
        var typeName = "color3f";

        // Act
        var createdInput = UsdShadeInput.CreateInput(prim, inputName, typeName);
        var retrievedInput = UsdShadeInput.GetInput(prim, inputName);

        // Assert
        Assert.True(createdInput.IsValid());
        Assert.True(retrievedInput.IsValid());
        Assert.Equal(inputName, createdInput.GetBaseName());
        Assert.Equal(inputName, retrievedInput.GetBaseName());
        Assert.Equal(typeName, createdInput.GetTypeName());
        Assert.Equal(typeName, retrievedInput.GetTypeName());
        Assert.Equal(createdInput.GetAttr(), retrievedInput.GetAttr());
    }

    [Fact]
    public void UsdShadeOutput_CreateAndGetOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var outputName = new TfToken("surface");
        var typeName = "token";

        // Act
        var createdOutput = UsdShadeOutput.CreateOutput(prim, outputName, typeName);
        var retrievedOutput = UsdShadeOutput.GetOutput(prim, outputName);

        // Assert
        Assert.True(createdOutput.IsValid());
        Assert.True(retrievedOutput.IsValid());
        Assert.Equal(outputName, createdOutput.GetBaseName());
        Assert.Equal(outputName, retrievedOutput.GetBaseName());
        Assert.Equal(typeName, createdOutput.GetTypeName());
        Assert.Equal(typeName, retrievedOutput.GetTypeName());
        Assert.Equal(createdOutput.GetAttr(), retrievedOutput.GetAttr());
    }

    [Fact]
    public void UsdShadeInput_SetAndGetValue_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var input = UsdShadeInput.CreateInput(prim, new TfToken("metallic"), "float");
        var testValue = 0.8f;

        // Act
        var setResult = input.Set(testValue);
        var retrievedValue = input.Get<float>();

        // Assert
        Assert.True(setResult);
        Assert.Equal(testValue, retrievedValue, 3);
    }

    [Fact]
    public void UsdShadeOutput_SetAndGetValue_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var output = UsdShadeOutput.CreateOutput(prim, new TfToken("result"), "float");
        var testValue = 1.5f;

        // Act
        var setResult = output.Set(testValue);
        var retrievedValue = output.Get<float>();

        // Assert
        Assert.True(setResult);
        Assert.Equal(testValue, retrievedValue, 3);
    }

    [Fact]
    public void UsdShadeInput_ConnectToOutput_Works()
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
        var connectionResult = targetInput.ConnectToSource(sourceOutput);

        // Assert
        Assert.True(connectionResult);
        Assert.True(targetInput.HasConnectedSource());
        
        var connectedSources = targetInput.GetConnectedSources();
        Assert.Single(connectedSources);
        Assert.Equal(sourceOutput.GetAttr().GetPath(), connectedSources[0]);
    }

    [Fact]
    public void UsdShadeOutput_ConnectToOutput_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("result"), "float");
        var targetOutput = UsdShadeOutput.CreateOutput(targetPrim, new TfToken("surface"), "token");

        // Act
        var connectionResult = targetOutput.ConnectToSource(sourceOutput);

        // Assert
        Assert.True(connectionResult);
        Assert.True(targetOutput.HasConnectedSource());
        
        var connectedSources = targetOutput.GetConnectedSources();
        Assert.Single(connectedSources);
        Assert.Equal(sourceOutput.GetAttr().GetPath(), connectedSources[0]);
    }

    [Fact]
    public void UsdShadeInput_ClearSources_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePath = new SdfPath("/SourceShader");
        var targetPath = new SdfPath("/TargetShader");
        var sourcePrim = stage.DefinePrim(sourcePath, "Shader");
        var targetPrim = stage.DefinePrim(targetPath, "Shader");

        var sourceOutput = UsdShadeOutput.CreateOutput(sourcePrim, new TfToken("color"), "color3f");
        var targetInput = UsdShadeInput.CreateInput(targetPrim, new TfToken("diffuseColor"), "color3f");
        
        targetInput.ConnectToSource(sourceOutput);
        Assert.True(targetInput.HasConnectedSource());

        // Act
        var clearResult = targetInput.ClearSources();

        // Assert
        Assert.True(clearResult);
        Assert.False(targetInput.HasConnectedSource());
        Assert.Empty(targetInput.GetConnectedSources());
    }

    [Fact]
    public void UsdShadeInput_RenderTypeMetadata_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var input = UsdShadeInput.CreateInput(prim, new TfToken("normalMap"), "normal3f");
        var renderType = new TfToken("normal");

        // Act
        var setResult = input.SetRenderType(renderType);
        var retrievedRenderType = input.GetRenderType();

        // Assert
        Assert.True(setResult);
        Assert.Equal(renderType, retrievedRenderType);
    }

    [Fact]
    public void UsdShadeInput_ConnectabilityMetadata_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var input = UsdShadeInput.CreateInput(prim, new TfToken("testInput"), "float");
        var connectability = UsdShadeAttributeType.Input;

        // Act
        var setResult = input.SetConnectability(connectability);
        var retrievedConnectability = input.GetConnectability();

        // Assert
        Assert.True(setResult);
        Assert.Equal(connectability, retrievedConnectability);
    }

    [Fact]
    public void UsdShadeInput_GetPrim_ReturnsCorrectPrim()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var input = UsdShadeInput.CreateInput(prim, new TfToken("testInput"), "float");

        // Act
        var retrievedPrim = input.GetPrim();

        // Assert
        Assert.True(retrievedPrim.IsValid());
        Assert.Equal(prim.GetPath(), retrievedPrim.GetPath());
    }

    [Fact]
    public void UsdShadeOutput_GetPrim_ReturnsCorrectPrim()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var output = UsdShadeOutput.CreateOutput(prim, new TfToken("testOutput"), "float");

        // Act
        var retrievedPrim = output.GetPrim();

        // Assert
        Assert.True(retrievedPrim.IsValid());
        Assert.Equal(prim.GetPath(), retrievedPrim.GetPath());
    }

    [Fact]
    public void UsdShadeInput_InvalidInput_HandlesGracefully()
    {
        // Arrange
        var invalidInput = new UsdShadeInput();

        // Act & Assert
        Assert.False(invalidInput.IsValid());
        Assert.True(invalidInput.GetBaseName().IsEmpty);
        Assert.False(invalidInput.HasConnectedSource());
        Assert.Empty(invalidInput.GetConnectedSources());
        Assert.Contains("invalid", invalidInput.ToString().ToLower());
    }

    [Fact]
    public void UsdShadeOutput_InvalidOutput_HandlesGracefully()
    {
        // Arrange
        var invalidOutput = new UsdShadeOutput();

        // Act & Assert
        Assert.False(invalidOutput.IsValid());
        Assert.True(invalidOutput.GetBaseName().IsEmpty);
        Assert.False(invalidOutput.HasConnectedSource());
        Assert.Empty(invalidOutput.GetConnectedSources());
        Assert.Contains("invalid", invalidOutput.ToString().ToLower());
    }

    [Fact]
    public void UsdShadeInput_CreateWithInvalidParams_ReturnsInvalid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var invalidPrim = new UsdPrim();
        var validPrim = stage.DefinePrim(new SdfPath("/TestShader"), "Shader");
        var emptyName = new TfToken();

        // Act & Assert
        var input1 = UsdShadeInput.CreateInput(invalidPrim, new TfToken("test"), "float");
        Assert.False(input1.IsValid());

        var input2 = UsdShadeInput.CreateInput(validPrim, emptyName, "float");
        Assert.False(input2.IsValid());
    }

    [Fact]
    public void UsdShadeOutput_CreateWithInvalidParams_ReturnsInvalid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var invalidPrim = new UsdPrim();
        var validPrim = stage.DefinePrim(new SdfPath("/TestShader"), "Shader");
        var emptyName = new TfToken();

        // Act & Assert
        var output1 = UsdShadeOutput.CreateOutput(invalidPrim, new TfToken("test"), "float");
        Assert.False(output1.IsValid());

        var output2 = UsdShadeOutput.CreateOutput(validPrim, emptyName, "float");
        Assert.False(output2.IsValid());
    }

    [Fact]
    public void UsdShadeInput_EqualityComparison_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var input1 = UsdShadeInput.CreateInput(prim, new TfToken("test"), "float");
        var input2 = UsdShadeInput.GetInput(prim, new TfToken("test"));
        var input3 = UsdShadeInput.CreateInput(prim, new TfToken("other"), "float");

        // Act & Assert
        Assert.Equal(input1, input2);
        Assert.True(input1 == input2);
        Assert.False(input1 != input2);
        
        Assert.NotEqual(input1, input3);
        Assert.False(input1 == input3);
        Assert.True(input1 != input3);
    }

    [Fact]
    public void UsdShadeOutput_EqualityComparison_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var shaderPath = new SdfPath("/TestShader");
        var prim = stage.DefinePrim(shaderPath, "Shader");
        var output1 = UsdShadeOutput.CreateOutput(prim, new TfToken("test"), "float");
        var output2 = UsdShadeOutput.GetOutput(prim, new TfToken("test"));
        var output3 = UsdShadeOutput.CreateOutput(prim, new TfToken("other"), "float");

        // Act & Assert
        Assert.Equal(output1, output2);
        Assert.True(output1 == output2);
        Assert.False(output1 != output2);
        
        Assert.NotEqual(output1, output3);
        Assert.False(output1 == output3);
        Assert.True(output1 != output3);
    }
}