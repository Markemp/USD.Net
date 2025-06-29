using System;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdEditContextTests
{
    #region Basic Functionality

    [Fact]
    public void UsdEditContext_Constructor_ShouldSetEditTarget()
    {
        // Arrange
        var sessionLayer = SdfLayer.CreateAnonymous("test://session");
        var stage = UsdStage.CreateInMemory("test://context", sessionLayer);
        var originalTarget = stage.GetEditTarget();
        var rootTarget = stage.GetEditTargetForRootLayer();

        // Act
        using var context = new UsdEditContext(stage, rootTarget);

        // Assert
        var currentTarget = stage.GetEditTarget();
        Assert.NotEqual(originalTarget, currentTarget);
        Assert.Same(stage.GetRootLayer(), currentTarget.GetLayer());
        Assert.Same(stage, context.GetStage());
        Assert.Equal(originalTarget, context.GetOriginalEditTarget());
    }

    [Fact]
    public void UsdEditContext_Dispose_ShouldRestoreOriginalTarget()
    {
        // Arrange
        var sessionLayer = SdfLayer.CreateAnonymous("test://session");
        var stage = UsdStage.CreateInMemory("test://restore", sessionLayer);
        var originalTarget = stage.GetEditTarget(); // Should be session layer
        var rootTarget = stage.GetEditTargetForRootLayer();

        // Act
        using (var context = new UsdEditContext(stage, rootTarget))
        {
            // Verify the target was changed
            Assert.NotEqual(originalTarget, stage.GetEditTarget());
        } // Dispose called here

        // Assert
        var restoredTarget = stage.GetEditTarget();
        Assert.Equal(originalTarget, restoredTarget);
    }

    [Fact]
    public void UsdEditContext_NullStage_ShouldThrow()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://null_stage");
        var target = UsdEditTarget.ForLocalLayer(layer);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UsdEditContext(null!, target));
    }

    [Fact]
    public void UsdEditContext_MultipleNested_ShouldRestoreCorrectly()
    {
        // Arrange - Create stage with session layer
        var sessionLayer = SdfLayer.CreateAnonymous("test://session");
        var stage = UsdStage.CreateInMemory("test://nested", sessionLayer);
        var originalTarget = stage.GetEditTarget(); // Session layer
        
        var rootTarget = stage.GetEditTargetForRootLayer();
        var sessionTarget = stage.GetEditTargetForSessionLayer()!.Value;

        // Act & Assert
        using (var context1 = new UsdEditContext(stage, rootTarget))
        {
            Assert.Same(stage.GetRootLayer(), stage.GetEditTarget().GetLayer());
            
            using (var context2 = new UsdEditContext(stage, sessionTarget))
            {
                Assert.Same(sessionLayer, stage.GetEditTarget().GetLayer());
            } // context2 disposed
            
            // Should restore to root layer
            Assert.Same(stage.GetRootLayer(), stage.GetEditTarget().GetLayer());
        } // context1 disposed
        
        // Should restore to original (session layer)
        Assert.Equal(originalTarget, stage.GetEditTarget());
    }

    #endregion

    #region UsdStage Integration

    [Fact]
    public void UsdStage_GetEditTarget_ShouldReturnCurrentTarget()
    {
        // Arrange
        var rootLayer = SdfLayer.CreateAnonymous("test://root");
        var sessionLayer = SdfLayer.CreateAnonymous("test://session");
        var stage = UsdStage.CreateInMemory("test://stage", sessionLayer);

        // Act
        var editTarget = stage.GetEditTarget();

        // Assert
        Assert.True(editTarget.IsValid());
        Assert.Same(sessionLayer, editTarget.GetLayer());
    }

    [Fact]
    public void UsdStage_SetEditTarget_ValidTarget_ShouldSetTarget()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var newLayer = SdfLayer.CreateAnonymous("test://new_target");
        
        // Add the layer to the stage's layer stack first
        // Note: We need a way to add layers to the stage for this to work
        // For now, we'll use the root layer
        var newTarget = stage.GetEditTargetForRootLayer();

        // Act
        stage.SetEditTarget(newTarget);

        // Assert
        Assert.Equal(newTarget, stage.GetEditTarget());
    }

    [Fact]
    public void UsdStage_SetEditTarget_InvalidTarget_ShouldThrow()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var invalidTarget = new UsdEditTarget();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => stage.SetEditTarget(invalidTarget));
    }

    [Fact]
    public void UsdStage_SetEditTarget_LayerNotInStack_ShouldThrow()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var externalLayer = SdfLayer.CreateAnonymous("test://external");
        var externalTarget = UsdEditTarget.ForLocalLayer(externalLayer);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => stage.SetEditTarget(externalTarget));
    }

    [Fact]
    public void UsdStage_GetEditTargetForRootLayer_ShouldReturnRootTarget()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();

        // Act
        var rootTarget = stage.GetEditTargetForRootLayer();

        // Assert
        Assert.True(rootTarget.IsValid());
        Assert.Same(stage.GetRootLayer(), rootTarget.GetLayer());
    }

    [Fact]
    public void UsdStage_GetEditTargetForSessionLayer_WithSession_ShouldReturnSessionTarget()
    {
        // Arrange
        var sessionLayer = SdfLayer.CreateAnonymous("test://session_target");
        var stage = UsdStage.CreateInMemory("test://stage_session", sessionLayer);

        // Act
        var sessionTarget = stage.GetEditTargetForSessionLayer();

        // Assert
        Assert.NotNull(sessionTarget);
        Assert.True(sessionTarget.Value.IsValid());
        Assert.Same(sessionLayer, sessionTarget.Value.GetLayer());
    }

    [Fact]
    public void UsdStage_GetEditTargetForSessionLayer_WithoutSession_ShouldReturnNull()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();

        // Act
        var sessionTarget = stage.GetEditTargetForSessionLayer();

        // Assert
        Assert.Null(sessionTarget);
    }

    [Fact]
    public void UsdStage_GetEditTargetForLocalLayer_ValidLayer_ShouldReturnTarget()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootLayer = stage.GetRootLayer();

        // Act
        var localTarget = stage.GetEditTargetForLocalLayer(rootLayer);

        // Assert
        Assert.True(localTarget.IsValid());
        Assert.Same(rootLayer, localTarget.GetLayer());
    }

    [Fact]
    public void UsdStage_GetEditTargetForLocalLayer_InvalidLayer_ShouldThrow()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var externalLayer = SdfLayer.CreateAnonymous("test://external_layer");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => stage.GetEditTargetForLocalLayer(externalLayer));
    }

    #endregion

    #region Real-World Usage Scenarios

    [Fact]
    public void UsdEditContext_EditingWorkflow_ShouldWorkCorrectly()
    {
        // Arrange - Create a stage with session layer
        var sessionLayer = SdfLayer.CreateAnonymous("test://workflow_session");
        var stage = UsdStage.CreateInMemory("test://workflow", sessionLayer);
        
        // Initial target should be session layer, switch to root for this test
        var rootTarget = stage.GetEditTargetForRootLayer();
        stage.SetEditTarget(rootTarget);
        
        // Create a prim in the root layer
        var rootPrim = stage.DefinePrim("/rootPrim");
        Assert.Same(stage.GetRootLayer(), stage.GetEditTarget().GetLayer());

        // Act - Switch to session layer for temporary edits
        var sessionTarget = stage.GetEditTargetForSessionLayer()!.Value;
        using (var context = new UsdEditContext(stage, sessionTarget))
        {
            // Edits should go to session layer
            var sessionPrim = stage.DefinePrim("/sessionPrim");
            Assert.Same(sessionLayer, stage.GetEditTarget().GetLayer());
        }

        // Assert - Should be back to root layer
        Assert.Same(stage.GetRootLayer(), stage.GetEditTarget().GetLayer());
    }

    [Fact]
    public void UsdEditTarget_PathMappingWorkflow_ShouldMapPaths()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://path_mapping");
        var editTarget = UsdEditTarget.ForLocalLayer(layer);
        var scenePath = new SdfPath("/test/prim");

        // Act
        var specPath = editTarget.MapToSpecPath(scenePath);

        // Assert
        Assert.Equal(scenePath, specPath); // Identity mapping for now
        Assert.True(specPath.IsPrimPath());
    }

    #endregion
}