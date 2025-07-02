# USD.Net Project Context

## Project Overview
USD.Net is a C# implementation of Pixar's Universal Scene Description (USD) format for .NET. The goal is to create a NuGet package that provides USD functionality for .NET applications.

## Key Project Information

### Directory Structure
- **Source USD C++ Reference**: `../OpenUSD` (relative to this project)
- **Main Library**: `src/USD.Net/` - Contains the main USD.Net library
- **Tests**: `tests/USD.Net.Tests/` - Unit tests for the library
- **Namespace Structure**: Follows USD C++ structure with `Pxr.*` namespaces
  - `Pxr.Base.Tf` - Foundation types (TfToken)
  - `Pxr.Base.Vt` - Value types (VtValue - use native C# types when possible)
  - `Pxr.Usd.Sdf` - Scene description foundation (SdfPath, SdfLayer)
  - `Pxr.Usd` - Main USD classes (UsdStage, UsdPrim, UsdAttribute, UsdRelationship)

### Development Environment
- **Target Framework**: .NET 9
- **Language Features**: File-scoped namespaces
- **Coding Style**: No comments unless explicitly requested
- **dotnet CLI**: Available - can run `dotnet build`, `dotnet test`, etc.

### Key Implementation Notes

#### USD Class Hierarchy and Dependencies
Implementation order based on dependencies:
1. **Foundation**: TfToken, VtValue, SdfPath, SdfLayer
2. **Core**: UsdStage, UsdObject  
3. **Properties**: UsdProperty, UsdAttribute, UsdRelationship
4. **Scene Graph**: UsdPrim

#### Critical Bug Fixes Applied
1. **SdfPath.GetName()**: Fixed to handle property paths correctly
   - For "/prim.property" returns "property" (not "prim.property")
2. **SdfPath.GetParentPath()**: Fixed to handle property paths
   - For "/prim.property" returns "/prim" (not root)
3. **SdfPath Constructor**: Fixed empty path handling
   - `SdfPath.EmptyPath()` now correctly returns empty path (not "/")
   - Relative paths stay relative and don't get converted to "/"
4. **UsdStage.DefinePrim()**: Added string overload for test compatibility

#### Current Status
- **Completed Classes**: TfToken, VtValue, SdfPath, SdfLayer, UsdStage, UsdObject, UsdProperty, UsdAttribute, UsdRelationship, UsdPrim, UsdEditTarget
- **Test Status**: All SdfLayer tests passing, UsdRelationship tests passing, UsdPrim tests passing
- **Recent Completions**: 
  - SdfLayer fully implemented (Save, Export, IsDirty, Clear, Reload, FindOrOpen)
  - UsdPrim fully implemented (hierarchy navigation, active flags, metadata, kind classification, instance/prototype handling)
  - UsdEditTarget fully implemented (path mapping, layer routing, UsdEditContext RAII pattern, UsdStage integration)

### Development Workflow

#### When Implementing New Classes
1. **Reference C++ Implementation**: Check `../OpenUSD` for the original C++ class
2. **Find Test Examples**: Look for test files in OpenUSD project to understand expected behavior
3. **Follow Dependency Order**: Implement dependencies before dependent classes
4. **Create Equivalent Tests**: Ensure USD.Net has comprehensive tests matching C++ behavior

#### When Debugging Test Failures
1. **Run Tests**: Use `dotnet test` to run all tests
2. **Check Path Handling**: Many issues relate to SdfPath property vs prim path handling
3. **Verify Method Signatures**: Ensure C# API matches expected USD patterns
4. **Reference USD Documentation**: Use https://openusd.org/release/api/ for API reference

### Code Conventions
- **No Comments**: Don't add code comments unless explicitly requested
- **File-Scoped Namespaces**: Use `namespace Pxr.Usd;` format
- **Nullable Reference Types**: Enabled in project
- **Method Naming**: Follow C++ USD naming conventions exactly
- **Error Handling**: Return invalid objects rather than throwing exceptions (USD pattern)
- **Prefer C# Idioms**: Always use C# equivalents over C++ patterns when possible:
  - Use LINQ + `yield return` for traversal instead of complex iterator classes
  - Use `IEnumerable<T>` with simple recursion for tree traversal
  - Prefer C# collection patterns over low-level pointer-style iteration
  - Use C# functional patterns (LINQ, delegates) over C++ callback patterns
- **Type Mapping**: Use native C# types instead of custom value type wrappers:
  - Use `Dictionary<string, object>` instead of `VtDictionary`
  - Use `List<T>` instead of custom list types
  - Use built-in .NET types whenever possible
  - Only create custom types when USD semantics require it (e.g., SdfPath, TfToken)

### Testing Strategy
- **Unit Tests**: Comprehensive coverage in `tests/USD.Net.Tests/`
- **Test Framework**: xUnit
- **Test Patterns**: Follow USD behavior patterns, not typical C# patterns
- **Cross-Reference**: Always compare with OpenUSD C++ tests when available

### Implementation Phases and Current Status

#### Phase 1: Foundation Classes (Minimal Dependencies) ✅ COMPLETED
1. **TfToken** ✅ - String interning for efficient identifiers
2. **VtValue** ✅ - Type-erased value container
3. **SdfPath** ✅ - Path representation for USD objects (FIXED: property path handling)
4. **SdfLayer** ✅ - Layer storage and metadata (COMPLETED: all I/O operations implemented)
5. **UsdStage** ✅ - Scene container and composition manager
6. **UsdObject** ✅ - Base class for all USD objects

#### Phase 2: Core Scene Graph Classes ✅ COMPLETED
7. **UsdProperty** ✅ - Base class for attributes and relationships
8. **UsdAttribute** ✅ - Typed property storage with time-sampling
9. **UsdRelationship** ✅ - Object connections/references (FIXED: target validation)
10. **UsdPrim** ✅ - Core scene graph node with property management (COMPLETED: all 32 missing methods implemented)

#### Phase 3: Composition Classes ✅ COMPLETED
11. **UsdEditTarget** ✅ - Edit target mapping and layer routing (COMPLETED: path mapping, factory methods, UsdEditContext)
12. **UsdReferences** ✅ - Reference composition (COMPLETED: SdfReference, SdfLayerOffset, UsdListPosition, comprehensive tests)
13. **UsdPayloads** ✅ - Optional content loading (COMPLETED: SdfPayload, UsdPayloads, loading state management, comprehensive tests)
14. **UsdVariantSets** ✅ - Variant composition (COMPLETED: UsdVariantSet, UsdVariantSets, selection management, edit contexts, comprehensive tests)

#### Phase 4: Enhanced Attribute System ✅ COMPLETED  
15. **Enhanced Time Sampling** ✅ - UsdTimeCode enhancements, interpolation system, time-varying attributes (COMPLETED: UsdTimeCode arithmetic, UsdInterpolation with held/linear, comprehensive time sampling tests)

#### Phase 5: Schema Foundation ✅ COMPLETED
16. **UsdSchemaBase** ✅ - Base for all schema classes (COMPLETED: abstract base with property creation helpers, compatibility checking, modern C# patterns)
17. **UsdAPISchemaBase** ✅ - Base for API schemas (COMPLETED: single/multiple-apply support, instance name handling, property prefixing)
18. **UsdTyped** ✅ - Base for typed schemas (COMPLETED: type name management, factory methods, inheritance checking)

#### Phase 6: Basic Geometry ⏳ PLANNED
19. **UsdGeomImageable** ⏳ - Visibility/rendering foundation
20. **UsdGeomXform** ⏳ - Transformation handling
21. **UsdGeomMesh** ⏳ - Polygon mesh geometry
22. **UsdGeomSphere** ⏳ - Sphere primitive

#### Phase 7: Advanced Features ⏳ FUTURE
23. **Scene Traversal**: UsdPrimRange, iteration patterns
24. **Metadata**: Comprehensive metadata system
25. **I/O**: File format support, serialization
26. **Schema Generation**: Automatic schema code generation

### Additional Schema Foundation Completed
19. **UsdSchemaRegistry** ✅ - Schema type registration and discovery system (COMPLETED: singleton registry, type lookup, schema classification, auto-discovery)
20. **Schema Integration** ✅ - UsdPrim integration with schema system (COMPLETED: IsA(), HasAPI(), ApplyAPI(), RemoveAPI() with full type safety)

### Next Priority Items
Based on current completion, the next logical steps are:
1. **Basic Geometry Foundation** - UsdGeomImageable and UsdGeomXform for geometry hierarchy
2. **Concrete Geometry Schemas** - UsdGeomMesh, UsdGeomSphere for basic shapes
3. **Enhanced I/O Support** - Actual USD file format reading/writing
4. **Schema Code Generation** - Template-based schema generation from definitions
5. **Stage Load Management** - Implement UsdStage payload loading/unloading methods
6. **Advanced Composition** - Sophisticated reference and layer composition

## Important Reminders
- Always check `../OpenUSD` for C++ reference implementation
- Find and review corresponding test files in OpenUSD when implementing classes
- Use `dotnet test` to run tests and `dotnet build` to build the project
- Path handling (prim vs property paths) is a common source of bugs
- Follow USD's error handling patterns (return invalid objects, don't throw)