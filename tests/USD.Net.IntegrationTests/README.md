# USD.Net Integration Tests

This project contains integration tests for USD.Net that involve file I/O, external dependencies, or full end-to-end scenarios.

## Test Organization

All tests in this project are marked with `[Trait("Category", "Integration")]` to distinguish them from unit tests.

## Running Tests

### Run all integration tests:
```bash
dotnet test tests/USD.Net.IntegrationTests/USD.Net.IntegrationTests.csproj
```

### Run integration tests by category:
```bash
# Run only integration tests across all test projects
dotnet test --filter "Category=Integration"

# Run all tests except integration tests
dotnet test --filter "Category!=Integration"
```

### Run specific integration test classes:
```bash
dotnet test --filter "FullyQualifiedName~UsdaExportTests"
```

## Test Categories

- **Integration**: Tests that perform file I/O, create actual USD files, or test full serialization/deserialization workflows
- **Unit** (default in USD.Net.Tests): Tests that verify individual component behavior without external dependencies

## Current Integration Tests

1. **UsdaExportTests** - Tests for USDA file export functionality
2. **UsdaOutputDemo** - Demonstration of USDA output generation with visual inspection
3. **SdfLayerIntegrationTests** - Tests for SdfLayer file operations (Export, Save)

## Best Practices

1. Always add `[Trait("Category", "Integration")]` to both the class and individual test methods
2. Clean up any generated files in test cleanup or using try/finally blocks
3. Use unique filenames to avoid conflicts when tests run in parallel
4. Consider test execution time - integration tests may take longer than unit tests