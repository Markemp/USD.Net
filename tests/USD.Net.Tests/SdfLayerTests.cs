using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class SdfLayerTests
{
    [Fact]
    public void SdfLayer_Constructor_ShouldSetIdentifier()
    {
        // Arrange & Act
        var layer = new SdfLayer("test://layer1");

        // Assert
        Assert.Equal("test://layer1", layer.GetIdentifier());
        Assert.Equal("test://layer1", layer.GetDisplayName());
    }

    [Fact]
    public void SdfLayer_Constructor_ShouldThrowOnNullIdentifier()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SdfLayer(null!));
    }

    [Fact]
    public void SdfLayer_CreateNew_ShouldCreateLayerWithIdentifier()
    {
        // Act
        var layer = SdfLayer.CreateNew("test://new_layer");

        // Assert
        Assert.NotNull(layer);
        Assert.Equal("test://new_layer", layer.GetIdentifier());
        Assert.False(layer.IsDirty());
    }

    [Fact]
    public void SdfLayer_CreateAnonymous_ShouldCreateUniqueIdentifier()
    {
        // Act
        var layer1 = SdfLayer.CreateAnonymous();
        var layer2 = SdfLayer.CreateAnonymous();

        // Assert
        Assert.NotNull(layer1);
        Assert.NotNull(layer2);
        Assert.NotEqual(layer1.GetIdentifier(), layer2.GetIdentifier());
        Assert.StartsWith("anon:", layer1.GetIdentifier());
        Assert.StartsWith("anon:", layer2.GetIdentifier());
    }

    [Fact]
    public void SdfLayer_CreateAnonymous_WithTag_ShouldIncludeTag()
    {
        // Act
        var layer = SdfLayer.CreateAnonymous("myTag");

        // Assert
        Assert.NotNull(layer);
        Assert.StartsWith("anon:", layer.GetIdentifier());
        Assert.Contains(":myTag", layer.GetIdentifier());
    }

    [Fact]
    public void SdfLayer_IsDirty_ShouldStartClean()
    {
        // Arrange
        var layer = new SdfLayer("test://clean");

        // Act & Assert
        Assert.False(layer.IsDirty());
    }

    [Fact]
    public void SdfLayer_SetMetadata_ShouldMarkDirty()
    {
        // Arrange
        var layer = new SdfLayer("test://metadata");
        var key = new TfToken("testKey");
        var value = new VtValue("testValue");

        // Act
        layer.SetMetadata(key, value);

        // Assert
        Assert.True(layer.IsDirty());
        Assert.True(layer.HasMetadata(key));
        Assert.Equal("testValue", layer.GetMetadata(key).Get<string>());
    }

    [Fact]
    public void SdfLayer_GetMetadata_NonExistent_ShouldReturnEmpty()
    {
        // Arrange
        var layer = new SdfLayer("test://empty_meta");
        var key = new TfToken("nonExistent");

        // Act
        var result = layer.GetMetadata(key);

        // Assert
        Assert.True(result.IsEmpty());
        Assert.False(layer.HasMetadata(key));
    }

    [Fact]
    public void SdfLayer_ClearMetadata_ShouldMarkDirtyAndRemove()
    {
        // Arrange
        var layer = new SdfLayer("test://clear_meta");
        var key = new TfToken("toClear");
        var value = new VtValue(42);

        layer.SetMetadata(key, value);
        Assert.True(layer.HasMetadata(key));

        // Act
        layer.ClearMetadata(key);

        // Assert
        Assert.False(layer.HasMetadata(key));
        Assert.True(layer.IsDirty()); // Should still be dirty
    }

    [Fact]
    public void SdfLayer_ClearMetadata_NonExistent_ShouldNotMarkDirty()
    {
        // Arrange
        var layer = new SdfLayer("test://clear_nonexistent");
        var key = new TfToken("doesNotExist");

        // Ensure layer starts clean
        Assert.False(layer.IsDirty());

        // Act
        layer.ClearMetadata(key);

        // Assert
        Assert.False(layer.IsDirty()); // Should remain clean
    }

    [Fact]
    public void SdfLayer_Save_WhenClean_ShouldSucceed()
    {
        // Arrange
        var layer = new SdfLayer("test://save_clean");
        Assert.False(layer.IsDirty());

        // Act
        var result = layer.Save();

        // Assert
        Assert.True(result);
        Assert.False(layer.IsDirty());
    }

    [Fact]
    public void SdfLayer_Save_WhenDirty_ShouldCleanAndSucceed()
    {
        // Arrange
        var layer = new SdfLayer("test://save_dirty");
        layer.SetMetadata(new TfToken("test"), new VtValue("value"));
        Assert.True(layer.IsDirty());

        // Act
        var result = layer.Save();

        // Assert
        Assert.True(result);
        Assert.False(layer.IsDirty());
    }

    [Fact]
    public void SdfLayer_Save_WithForce_ShouldAlwaysSucceed()
    {
        // Arrange
        var layer = new SdfLayer("test://save_force");
        Assert.False(layer.IsDirty());

        // Act
        var result = layer.Save(force: true);

        // Assert
        Assert.True(result);
        Assert.False(layer.IsDirty());
    }

    [Fact]
    public void SdfLayer_Clear_ShouldRemoveAllMetadataAndMarkDirty()
    {
        // Arrange
        var layer = new SdfLayer("test://clear");
        layer.SetMetadata(new TfToken("key1"), new VtValue("value1"));
        layer.SetMetadata(new TfToken("key2"), new VtValue(123));

        // Verify metadata exists
        Assert.True(layer.HasMetadata(new TfToken("key1")));
        Assert.True(layer.HasMetadata(new TfToken("key2")));

        // Act
        layer.Clear();

        // Assert
        Assert.False(layer.HasMetadata(new TfToken("key1")));
        Assert.False(layer.HasMetadata(new TfToken("key2")));
        Assert.True(layer.IsDirty());
    }

    [Fact]
    public void SdfLayer_Reload_ShouldClearAndMarkClean()
    {
        // Arrange
        var layer = new SdfLayer("test://reload");
        layer.SetMetadata(new TfToken("toRemove"), new VtValue("value"));
        Assert.True(layer.IsDirty());
        Assert.True(layer.HasMetadata(new TfToken("toRemove")));

        // Act
        var result = layer.Reload();

        // Assert
        Assert.True(result);
        Assert.False(layer.IsDirty());
        Assert.False(layer.HasMetadata(new TfToken("toRemove")));
    }

    [Fact]
    public void SdfLayer_Reload_WhenClean_ShouldOnlyReloadWithForce()
    {
        // Arrange
        var layer = new SdfLayer("test://reload_clean");
        layer.SetMetadata(new TfToken("persistent"), new VtValue("data"));
        layer.Save(); // Make it clean

        Assert.False(layer.IsDirty());
        Assert.True(layer.HasMetadata(new TfToken("persistent")));

        // Act - reload without force should not clear
        var result1 = layer.Reload(force: false);
        Assert.True(result1);
        Assert.True(layer.HasMetadata(new TfToken("persistent")));

        // Act - reload with force should clear
        var result2 = layer.Reload(force: true);
        Assert.True(result2);
        Assert.False(layer.HasMetadata(new TfToken("persistent")));
    }

    [Fact]
    public void SdfLayer_FindOrOpen_NewIdentifier_ShouldCreateLayer()
    {
        // Arrange
        var identifier = "test://find_new_" + Guid.NewGuid();

        // Act
        var layer = SdfLayer.FindOrOpen(identifier);

        // Assert
        Assert.NotNull(layer);
        Assert.Equal(identifier, layer.GetIdentifier());
    }

    [Fact]
    public void SdfLayer_FindOrOpen_ExistingIdentifier_ShouldReturnSameLayer()
    {
        // Arrange
        var identifier = "test://find_existing_" + Guid.NewGuid();
        var originalLayer = new SdfLayer(identifier);
        originalLayer.SetMetadata(new TfToken("marker"), new VtValue("original"));

        // Act
        var foundLayer = SdfLayer.FindOrOpen(identifier);

        // Assert
        Assert.NotNull(foundLayer);
        Assert.Same(originalLayer, foundLayer);
        Assert.True(foundLayer.HasMetadata(new TfToken("marker")));
        Assert.Equal("original", foundLayer.GetMetadata(new TfToken("marker")).Get<string>());
    }

    [Fact]
    public void SdfLayer_FindOrOpen_EmptyIdentifier_ShouldReturnNull()
    {
        // Act & Assert
        Assert.Null(SdfLayer.FindOrOpen(""));
        Assert.Null(SdfLayer.FindOrOpen(null!));
    }

    [Fact]
    public void SdfLayer_MetadataOperations_ShouldHandleMultipleTypes()
    {
        // Arrange
        var layer = new SdfLayer("test://multi_types");

        // Act & Assert - String
        var stringKey = new TfToken("stringValue");
        var stringValue = new VtValue("hello world");
        layer.SetMetadata(stringKey, stringValue);
        Assert.Equal("hello world", layer.GetMetadata(stringKey).Get<string>());

        // Act & Assert - Integer
        var intKey = new TfToken("intValue");
        var intValue = new VtValue(42);
        layer.SetMetadata(intKey, intValue);
        Assert.Equal(42, layer.GetMetadata(intKey).Get<int>());

        // Act & Assert - Double
        var doubleKey = new TfToken("doubleValue");
        var doubleValue = new VtValue(3.14159);
        layer.SetMetadata(doubleKey, doubleValue);
        Assert.Equal(3.14159, layer.GetMetadata(doubleKey).Get<double>(), 5);

        // Act & Assert - Boolean
        var boolKey = new TfToken("boolValue");
        var boolValue = new VtValue(true);
        layer.SetMetadata(boolKey, boolValue);
        Assert.True(layer.GetMetadata(boolKey).Get<bool>());
    }
}