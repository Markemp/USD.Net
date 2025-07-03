# USD.Net

A modern .NET implementation of Pixar's Universal Scene Description (USD) for C# developers.

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)

## Overview

USD.Net brings the power of Pixar's Universal Scene Description to the .NET ecosystem, enabling C# developers to read, write, and manipulate USD files for 3D graphics pipelines, visual effects, and game development.

### Key Features

- **Native C# Implementation** - Pure .NET implementation optimized for C# developers
- **USDA File Export** - Create USD ASCII files compatible with Blender, Maya, Houdini, and other USD tools
- **Geometry Support** - Create and manipulate 3D geometry including meshes, spheres, cubes, and transforms
- **Time-based Animation** - Support for time-varying attributes and animated properties
- **Modern API Design** - Idiomatic C# API that follows .NET conventions while maintaining USD compatibility
- **Comprehensive Testing** - 295+ tests ensuring reliability and USD specification compliance

## Quick Start

```csharp
using Pxr.Usd;
using Pxr.Usd.UsdGeom;

// Create a new USD stage
var stage = UsdStage.CreateInMemory();

// Create a sphere
var spherePath = new SdfPath("/World/Sphere");
var sphere = UsdGeomSphere.Define(stage, spherePath, radius: 2.0);
sphere.SetDisplayColor(UsdGeomGprim.Red);

// Add animation
var translateOp = sphere.AddTranslateOp();
for (int frame = 0; frame <= 30; frame++)
{
    var time = (double)frame;
    translateOp.Set(new GfVec3f(frame * 0.1f, 0, 0), time);
}

// Export to USDA file
stage.Export("animated_sphere.usda");
```

## Installation

USD.Net will be available as a NuGet package (coming soon):

```bash
dotnet add package USD.Net
```

For now, clone and build from source:

```bash
git clone https://github.com/[your-org]/USD.Net.git
cd USD.Net
dotnet build
```

## Project Structure

```
USD.Net/
├── src/
│   └── USD.Net/              # Main library
├── tests/
│   ├── USD.Net.Tests/        # Unit tests
│   └── USD.Net.IntegrationTests/  # Integration tests
├── examples/
│   └── 3DModelCreation.cs    # Example code
└── docs/                     # Documentation
```

## Current Implementation Status

### ✅ Completed
- **Core USD Types**: TfToken, VtValue, SdfPath, SdfLayer
- **Scene Graph**: UsdStage, UsdPrim, UsdAttribute, UsdRelationship
- **Geometry Schemas**: Full UsdGeom implementation (Xform, Mesh, Sphere, Cube)
- **Transform System**: Complete xformOp support with animation
- **USDA Export**: Full serialization to USD ASCII format
- **Time Sampling**: Animation and time-varying attributes

### 🚧 In Progress
- USDA file reading/parsing
- Binary USD (USDC) support
- Additional geometry schemas

### 📋 Planned
- Complete USD file I/O (read/write all formats)
- Advanced composition features
- Python bindings for cross-language compatibility
- Performance optimizations

## Documentation

- [API Documentation](docs/api/index.md) (coming soon)
- [Getting Started Guide](docs/getting-started.md) (coming soon)
- [Examples](examples/3DModelCreation.cs)
- [OpenUSD Serialization Analysis](docs/OpenUSD_USDA_Serialization_Analysis.md)

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Install .NET 9.0 SDK or later
2. Clone the repository
3. Run `dotnet build` to build
4. Run `dotnet test` to run all tests
5. Run `dotnet test --filter "Category=Unit"` for unit tests only

## Examples

Check out the [examples](examples/) directory for more comprehensive examples including:
- Basic primitive creation
- Hierarchical scene construction
- Animation and time samples
- Procedural geometry generation
- Complex mesh creation

## Compatibility

USD.Net aims for compatibility with:
- **USD Version**: OpenUSD 24.x
- **.NET Version**: .NET 9.0+
- **Platforms**: Windows, macOS, Linux
- **Tools**: Blender, Maya, Houdini, Omniverse, and other USD-compatible applications

## License

USD.Net is licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

## Acknowledgments

This project is based on Pixar's Universal Scene Description (USD) technology. USD is a trademark of Pixar.

Special thanks to:
- The Pixar USD team for creating this amazing technology
- The OpenUSD community for continued development
- All contributors to this project

## Status

This project is under active development. APIs may change as we work toward a stable 1.0 release.

---

For questions, issues, or contributions, please visit our [GitHub repository](https://github.com/[your-org]/USD.Net).