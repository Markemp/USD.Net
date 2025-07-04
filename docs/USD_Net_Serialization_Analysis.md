# USD.Net Serialization Analysis: Current State and Issues

## Executive Summary

Round-trip serialization is currently **not working** in USD.Net because:
1. **No USDA file reader/parser has been implemented** - Files cannot be loaded from disk
2. **Incomplete serialization** - Current export functionality is missing critical components
3. **Architecture gap** - No connection between SdfLayer file I/O and UsdStage content

## Current Implementation State

### Reading/Deserialization ❌ NOT IMPLEMENTED

**SdfLayer.FindOrOpen()**:
```csharp
public static SdfLayer? FindOrOpen(string identifier)
{
    // ... registry check ...
    
    // TODO: Implement actual file loading when file format support is added
    // For now, create a new layer for any identifier
    return new SdfLayer(identifier);
}
```
- **Current behavior**: Creates an empty layer regardless of file contents
- **Expected behavior**: Should parse USDA file and populate layer with content

**UsdStage.Open()**:
```csharp
public static UsdStage? Open(string filePath, ArResolverContext? context = null)
{
    var rootLayer = SdfLayer.FindOrOpen(filePath);
    if (rootLayer == null)
        return null;

    return new UsdStage(rootLayer, null, context);
}
```
- **Current behavior**: Creates empty stage with empty layer
- **Expected behavior**: Should load complete scene graph from file

### Writing/Serialization ⚠️ PARTIALLY IMPLEMENTED

**Two separate serialization paths exist:**

#### 1. SdfLayer.Export() - Basic Implementation
```csharp
public bool Export(string filename)
{
    // Writes:
    // - #usda 1.0 header
    // - Basic metadata (but with bugs)
    // Does NOT write:
    // - Prims
    // - Properties
    // - Relationships
    // - Composition arcs
}
```

**Issues:**
- Metadata mapping bug: looks for "comment" key but should be "doc"
- No prim content serialization
- No connection to stage content

#### 2. UsdStage.Export() via UsdaWriter - More Complete
```csharp
public bool Export(string fileName)
{
    return UsdaWriter.SaveStageToFile(this, fileName);
}
```

**UsdaWriter capabilities:**
- ✅ Writes prims with types and names
- ✅ Writes attributes with values
- ✅ Handles time samples
- ✅ Formats various value types correctly
- ❌ Doesn't write layer metadata from original file
- ❌ Missing some composition features

## Baseline Test Failures Explained

The baseline tests are failing because:

1. **Input files contain layer metadata and prims**:
   ```usda
   #usda 1.0
   (
       doc = "Simple USD file with a single prim"
   )
   
   def "root"
   {
       string description = "A simple root prim"
   }
   ```

2. **FindOrOpen creates empty layer**: No parsing occurs, so no content is loaded

3. **Export writes empty file**:
   ```usda
   #usda 1.0
   ```

4. **No prims exist in the stage**: The stage has no knowledge of the file's content

## Architecture Issues

### 1. Missing Parser
- No USDA lexer/parser implementation
- No token recognition for USD keywords (def, over, class, etc.)
- No value parsing for different types
- No metadata extraction

### 2. Disconnected Layer/Stage Model
- SdfLayer doesn't store prim specs
- No mechanism to populate stage from layer content
- No bidirectional sync between layers and stages

### 3. Incomplete Object Model
- SdfPrimSpec not fully implemented
- SdfPropertySpec missing
- SdfAttributeSpec missing
- No composition arc storage in layers

## Required Components for Round-Trip

### 1. USDA Parser (High Priority)
```csharp
public class UsdaParser
{
    public SdfLayer ParseFile(string filePath);
    public SdfLayer ParseString(string usdaContent);
    
    // Token recognition
    private Token NextToken();
    
    // Value parsing
    private VtValue ParseValue(string typeName);
    
    // Structure parsing
    private SdfPrimSpec ParsePrim();
    private void ParseMetadata(Dictionary<string, object> metadata);
}
```

### 2. Enhanced SdfLayer (High Priority)
```csharp
public class SdfLayer
{
    // Add prim storage
    private Dictionary<SdfPath, SdfPrimSpec> _primSpecs;
    
    // Add methods to manipulate content
    public SdfPrimSpec CreatePrimSpec(SdfPath path, string typeName);
    public SdfPrimSpec? GetPrimAtPath(SdfPath path);
    
    // Enhanced Export that writes all content
    public bool Export(string filename);
}
```

### 3. SdfPrimSpec Implementation (High Priority)
```csharp
public class SdfPrimSpec
{
    public string Name { get; set; }
    public string TypeName { get; set; }
    public SdfSpecifier Specifier { get; set; }
    public Dictionary<string, SdfPropertySpec> Properties { get; }
    public List<SdfPrimSpec> Children { get; }
}
```

### 4. Layer-to-Stage Population (Medium Priority)
```csharp
// In UsdStage
private void PopulateFromLayer(SdfLayer layer)
{
    foreach (var primSpec in layer.GetRootPrimSpecs())
    {
        CreatePrimFromSpec(primSpec);
    }
}
```

## Recommended Implementation Order

### Phase 1: Basic Parser (1-2 weeks)
1. Implement USDA tokenizer
2. Add basic value parsing (strings, numbers, bools)
3. Parse simple prim definitions
4. Parse basic metadata

### Phase 2: Layer Content Model (1 week)
1. Implement SdfPrimSpec
2. Add prim storage to SdfLayer
3. Connect parser to create prim specs
4. Update Export to write prim content

### Phase 3: Stage Population (1 week)
1. Implement layer-to-stage population
2. Add attribute creation from specs
3. Handle prim hierarchy
4. Test round-trip with simple files

### Phase 4: Advanced Features (2-3 weeks)
1. Array/vector parsing
2. Time samples
3. Composition arcs (references, payloads)
4. Complex metadata
5. List operations

## Quick Fix for Demo

For immediate demonstration of working serialization, create stages programmatically:
```csharp
// Instead of loading from file
var stage = UsdStage.CreateInMemory();
var rootPrim = stage.DefinePrim("/root");
rootPrim.CreateAttribute("description", "string").Set("A simple root prim");

// This will export correctly
stage.Export("output.usda");
```

## Conclusion

The core issue is that **USD.Net cannot read USDA files**. The serialization infrastructure exists but only works for programmatically created content. To achieve true round-trip serialization, a complete USDA parser must be implemented along with proper layer content storage and stage population mechanisms.

The baseline testing infrastructure successfully identified this critical gap, demonstrating its value for ensuring USD.Net compatibility with the USD ecosystem.