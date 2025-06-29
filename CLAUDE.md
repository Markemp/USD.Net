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
  - `Pxr.Base.Vt` - Value types (VtValue)
  - `Pxr.Usd.Sdf` - Scene description foundation (SdfPath, SdfLayer)
  - `Pxr.Usd` - Main USD classes (UsdStage, UsdPrim, UsdAttribute, UsdRelationship)

### Development Environment
- **Target Framework**: .NET 9
- **Language Features**: File-scoped namespaces
- **Coding Style**: No comments unless explicitly requested
- **dotnet CLI**: NOT available in current environment - tests must be run manually by user

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
- **Completed Classes**: TfToken, VtValue, SdfPath, SdfLayer, UsdStage, UsdObject, UsdProperty, UsdAttribute, UsdRelationship
- **Partial Implementation**: UsdPrim (32 NotImplementedExceptions for advanced features)
- **Test Status**: Most UsdRelationship tests passing, recently fixed target validation
- **Recent Completion**: SdfLayer fully implemented (Save, Export, IsDirty, Clear, Reload, FindOrOpen)

### Development Workflow

#### When Implementing New Classes
1. **Reference C++ Implementation**: Check `../OpenUSD` for the original C++ class
2. **Find Test Examples**: Look for test files in OpenUSD project to understand expected behavior
3. **Follow Dependency Order**: Implement dependencies before dependent classes
4. **Create Equivalent Tests**: Ensure USD.Net has comprehensive tests matching C++ behavior

#### When Debugging Test Failures
1. **Run Tests Manually**: User must run tests (dotnet CLI not available)
2. **Check Path Handling**: Many issues relate to SdfPath property vs prim path handling
3. **Verify Method Signatures**: Ensure C# API matches expected USD patterns
4. **Reference USD Documentation**: Use https://openusd.org/release/api/ for API reference

### Code Conventions
- **No Comments**: Don't add code comments unless explicitly requested
- **File-Scoped Namespaces**: Use `namespace Pxr.Usd;` format
- **Nullable Reference Types**: Enabled in project
- **Method Naming**: Follow C++ USD naming conventions exactly
- **Error Handling**: Return invalid objects rather than throwing exceptions (USD pattern)

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
10. **UsdPrim** ✅ - Core scene graph node with property management

#### Phase 3: Composition Classes 🚧 IN PROGRESS
11. **UsdEditTarget** 🚧 - Stub implementation exists, needs full implementation
12. **UsdReferences** ⏳ - Reference composition (planned)
13. **UsdPayloads** ⏳ - Optional content loading (planned)
14. **UsdVariantSets** ⏳ - Variant composition (planned)

#### Phase 4: Schema Foundation ⏳ PLANNED
15. **UsdSchemaBase** ⏳ - Base for all schema classes
16. **UsdAPISchemaBase** ⏳ - Base for API schemas
17. **UsdTyped** ⏳ - Base for typed schemas

#### Phase 5: Basic Geometry ⏳ PLANNED
18. **UsdGeomImageable** ⏳ - Visibility/rendering foundation
19. **UsdGeomXform** ⏳ - Transformation handling
20. **UsdGeomMesh** ⏳ - Polygon mesh geometry
21. **UsdGeomSphere** ⏳ - Sphere primitive

#### Phase 6: Advanced Features ⏳ FUTURE
22. **Scene Traversal**: UsdPrimRange, iteration patterns
23. **Time Sampling**: Enhanced time-varying value support
24. **Metadata**: Comprehensive metadata system
25. **I/O**: File format support, serialization
26. **Schema Generation**: Automatic schema code generation

### Next Priority Items
Based on current completion, the next logical steps are:
1. **Complete UsdPrim** - 32 NotImplementedExceptions for hierarchy navigation, metadata, and advanced features
2. **Complete UsdEditTarget** - Required for proper scene modification
3. **Implement UsdReferences** - Core composition feature
4. **Add comprehensive time-sampling** - Enhanced UsdAttribute functionality
5. **Scene traversal utilities** - UsdPrimRange and iteration helpers

## Important Reminders
- Always check `../OpenUSD` for C++ reference implementation
- Find and review corresponding test files in OpenUSD when implementing classes
- User must run tests manually - provide clear instructions when needed
- Path handling (prim vs property paths) is a common source of bugs
- Follow USD's error handling patterns (return invalid objects, don't throw)