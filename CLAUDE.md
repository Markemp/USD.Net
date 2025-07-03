# USD.Net Project Context

## Project Overview
USD.Net is a C# implementation of Pixar's Universal Scene Description (USD) format for .NET. The goal is to create a NuGet package that provides USD functionality for .NET applications.

## Key Project Information

### Directory Structure
- **Source USD C++ Reference**: `../OpenUSD` (relative to this project)
- **Main Library**: `src/USD.Net/` - Contains the main USD.Net library
- **Unit Tests**: `tests/USD.Net.Tests/` - Unit tests only (no file I/O)
- **Integration Tests**: `tests/USD.Net.IntegrationTests/` - Integration tests (file I/O, serialization)
- **Namespace Structure**: Follows USD C++ structure with `Pxr.*` namespaces
  - `Pxr.Base.Tf` - Foundation types (TfToken)
  - `Pxr.Base.Vt` - Value types (VtValue - use native C# types when possible)
  - `Pxr.Usd.Sdf` - Scene description foundation (SdfPath, SdfLayer)
  - `Pxr.Usd` - Main USD classes (UsdStage, UsdPrim, UsdAttribute, UsdRelationship)
  - `Pxr.Usd.UsdGeom` - Geometry schemas (UsdGeomImageable, UsdGeomXform, UsdGeomMesh, etc.)
  - `Pxr.Usd.UsdUtils` - Utilities (UsdaWriter for USDA serialization)

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
- **Completed Classes**: TfToken, VtValue, SdfPath, SdfLayer, UsdStage, UsdObject, UsdProperty, UsdAttribute, UsdRelationship, UsdPrim, UsdEditTarget, UsdaWriter, UsdGeom classes
- **Test Status**: 
  - Unit Tests: 289+ passing tests
  - Integration Tests: 6 passing tests (USDA export, file I/O)
- **Recent Completions**: 
  - SdfLayer fully implemented (Save, Export, IsDirty, Clear, Reload, FindOrOpen)
  - UsdPrim fully implemented (hierarchy navigation, active flags, metadata, kind classification, instance/prototype handling)
  - UsdEditTarget fully implemented (path mapping, layer routing, UsdEditContext RAII pattern, UsdStage integration)
  - USDA serialization implemented (UsdaWriter class for exporting stages to .usda files)
  - Geometry classes implemented (UsdGeomImageable, UsdGeomXformable, UsdGeomBoundable, UsdGeomGprim, UsdGeomMesh, UsdGeomSphere, UsdGeomCube)

### Development Workflow

#### When Implementing New Classes
1. **Reference C++ Implementation**: Check `../OpenUSD` for the original C++ class
2. **Find Test Examples**: Look for test files in OpenUSD project to understand expected behavior
3. **Follow Dependency Order**: Implement dependencies before dependent classes
4. **Create Equivalent Tests**: Ensure USD.Net has comprehensive tests matching C++ behavior

#### When Debugging Test Failures
1. **Run Tests**: 
   - All tests: `dotnet test`
   - Specific project: `dotnet test tests/USD.Net.Tests/USD.Net.Tests.csproj`
   - Specific category: `dotnet test --filter "Category=Unit"`
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
- **Test Organization**: 
  - **Unit Tests**: `tests/USD.Net.Tests/` - Fast, isolated tests with no external dependencies
  - **Integration Tests**: `tests/USD.Net.IntegrationTests/` - Tests involving file I/O, serialization, or full workflows
- **Test Framework**: xUnit with Trait attributes for categorization
- **Test Categories**:
  - `[Trait("Category", "Unit")]` - Default for unit tests (optional to specify)
  - `[Trait("Category", "Integration")]` - Required for all integration tests
- **Running Tests**:
  - All tests: `dotnet test`
  - Unit tests only: `dotnet test --filter "Category!=Integration"`
  - Integration tests only: `dotnet test --filter "Category=Integration"`
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

#### Phase 6: Basic Geometry ✅ COMPLETED
19. **UsdGeomImageable** ✅ - Visibility/rendering foundation (COMPLETED: visibility, purpose, proxy prim, render visibility)
20. **UsdGeomXformable** ✅ - Transformation handling (COMPLETED: xformOp system, transform stacks, matrix computation)
21. **UsdGeomBoundable** ✅ - Bounding box support (COMPLETED: extent computation, ComputeExtentFromGeometry)
22. **UsdGeomGprim** ✅ - Geometric primitives (COMPLETED: display color/opacity, double-sided, orientation)
23. **UsdGeomXform** ✅ - Transform container (COMPLETED: simple transform prim type)
24. **UsdGeomMesh** ✅ - Polygon mesh geometry (COMPLETED: points, topology, normals, UVs, subdivision)
25. **UsdGeomSphere** ✅ - Sphere primitive (COMPLETED: radius, extent computation)
26. **UsdGeomCube** ✅ - Cube primitive (COMPLETED: size, extent computation)

#### Phase 7: I/O and Serialization ✅ COMPLETED
27. **UsdaWriter** ✅ - USDA text format serialization (COMPLETED: full stage/prim/attribute serialization)
28. **SdfLayer.Export()** ✅ - Layer export to USDA (COMPLETED: metadata and basic structure export)
29. **UsdStage.Export()** ✅ - Stage export to USDA (COMPLETED: complete scene graph serialization)

#### Phase 8: Advanced Features ⏳ FUTURE
30. **Scene Traversal**: Advanced UsdPrimRange, parallel iteration patterns
31. **Metadata**: Extended metadata system with custom schemas
32. **Binary I/O**: USD crate file format support
33. **Schema Generation**: Automatic schema code generation from definitions
34. **Advanced Composition**: Sublayers, session layers, variant composition

### Additional Completed Components
- **UsdSchemaRegistry** ✅ - Schema type registration and discovery system
- **Schema Integration** ✅ - UsdPrim integration with schema system (IsA(), HasAPI(), ApplyAPI(), RemoveAPI())
- **Test Organization** ✅ - Separated unit and integration tests with category-based filtering

### Next Priority Items
Based on current completion, the next logical steps are:
1. **USD File Reading** - Parse .usda files back into stages (UsdaParser)
2. **Binary Format Support** - Implement .usdc (crate) file format for efficient storage
3. **Advanced Scene Traversal** - Parallel iteration, filtered traversal, predicate-based search
4. **Schema Code Generation** - Automatic generation of schema classes from schema.usda files
5. **Performance Optimization** - Implement caching, lazy loading, and memory pooling
6. **Extended Metadata System** - Custom metadata schemas, dictionary support, complex types

## Important Reminders
- Always check `../OpenUSD` for C++ reference implementation
- Find and review corresponding test files in OpenUSD when implementing classes
- Use `dotnet test` to run tests and `dotnet build` to build the project
- Path handling (prim vs property paths) is a common source of bugs
- Follow USD's error handling patterns (return invalid objects, don't throw)