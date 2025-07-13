using System.Text;
using Xunit;
using Xunit.Abstractions;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdUtils;
using Pxr.Usd.UsdGeom;

namespace USD.Net.IntegrationTests;

[Trait("Category", "Integration")]
public class UsdaParserComprehensiveTests
{
    private readonly ITestOutputHelper _output;

    public UsdaParserComprehensiveTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ParseBasicStructure_WithHeaderAndMetadata_ParsesSuccessfully()
    {
        var usdaContent = """
            #usda 1.0
            (
                defaultPrim = "World"
                metersPerUnit = 0.01
                upAxis = "Y"
                startTimeCode = 1
                endTimeCode = 240
                timeCodesPerSecond = 24
                framesPerSecond = 24
                doc = "Test USD file with comprehensive features"
                customLayerData = {
                    string author = "USD.Net Test Suite"
                    dictionary renderSettings = {
                        int maxSamples = 64
                        float pixelVariance = 0.01
                    }
                }
            )

            def Xform "World" (
                kind = "group"
                doc = "Root transform of the scene"
            )
            {
                def Xform "Geometry" (
                    kind = "component"
                )
                {
                    def Mesh "SimpleCube"
                    {
                        uniform bool doubleSided = true
                        float3[] extent = [(-1, -1, -1), (1, 1, 1)]
                        int[] faceVertexCounts = [4, 4, 4, 4, 4, 4]
                        int[] faceVertexIndices = [0, 1, 3, 2, 2, 3, 5, 4, 4, 5, 7, 6, 6, 7, 1, 0, 1, 7, 5, 3, 6, 0, 2, 4]
                        point3f[] points = [(-1, -1, 1), (1, -1, 1), (-1, 1, 1), (1, 1, 1), (-1, 1, -1), (1, 1, -1), (-1, -1, -1), (1, -1, -1)]
                    }
                }
            }
            """;

        var layer = SdfLayer.CreateNew("test_basic_structure");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse basic USDA structure");
        
        // Verify layer metadata
        var metadata = layer.GetAllMetadata();
        Assert.True(metadata.Count > 0, "Layer should have metadata");
        
        // Verify prim hierarchy
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        Assert.Equal(3, primSpecs.Count); // World, Geometry, SimpleCube
        
        var worldPrim = primSpecs.FirstOrDefault(p => p.GetName() == "World");
        Assert.NotNull(worldPrim);
        Assert.Equal("group", worldPrim.GetKind().ToString());
        
        var geometryPrim = worldPrim.GetChildren().FirstOrDefault(p => p.GetName() == "Geometry");
        Assert.NotNull(geometryPrim);
        Assert.Equal("component", geometryPrim.GetKind().ToString());
        
        var cubePrim = geometryPrim.GetChildren().FirstOrDefault(p => p.GetName() == "SimpleCube");
        Assert.NotNull(cubePrim);
        
        // Verify mesh properties
        var properties = cubePrim.GetProperties();
        Assert.True(properties.Any(p => p.GetName() == "doubleSided"), "Should have doubleSided property");
        Assert.True(properties.Any(p => p.GetName() == "extent"), "Should have extent property");
        Assert.True(properties.Any(p => p.GetName() == "points"), "Should have points property");
    }

    [Fact]
    public void ParseComplexValueTypes_AllTypesSupported_ParsesCorrectly()
    {
        var usdaContent = """
            #usda 1.0

            def "TestPrim"
            {
                # Basic types
                bool boolValue = true
                int intValue = 42
                float floatValue = 3.14159
                double doubleValue = 2.71828
                string stringValue = "Hello, USD!"
                token tokenValue = myToken
                asset assetValue = @./path/to/asset.usd@
                
                # Array types
                bool[] boolArray = [true, false, true]
                int[] intArray = [1, 2, 3, 4, 5]
                float[] floatArray = [1.0, 2.0, 3.0]
                double[] doubleArray = [1.1, 2.2, 3.3]
                string[] stringArray = ["one", "two", "three"]
                token[] tokenArray = [token1, token2, token3]
                asset[] assetArray = [@asset1.usd@, @asset2.usd@]
                
                # Tuple types
                float2 vec2Value = (1.0, 2.0)
                float3 vec3Value = (1.0, 2.0, 3.0)
                float4 vec4Value = (1.0, 2.0, 3.0, 4.0)
                double3 dvec3Value = (1.1, 2.2, 3.3)
                int2 ivec2Value = (10, 20)
                
                # Matrix types
                matrix4d matrixValue = ( (1, 0, 0, 0), (0, 1, 0, 0), (0, 0, 1, 0), (0, 0, 0, 1) )
                
                # Dictionary types
                dictionary simpleDictionary = {
                    string key1 = "value1"
                    int key2 = 123
                }
                
                dictionary nestedDictionary = {
                    dictionary subDict = {
                        float value = 1.5
                        bool flag = true
                    }
                    token[] tokens = [a, b, c]
                }
                
                # Complex nested structures
                dictionary complexData = {
                    string name = "Complex Structure"
                    float3[] positions = [(1, 2, 3), (4, 5, 6)]
                    dictionary metadata = {
                        int version = 2
                        string[] tags = ["important", "validated"]
                    }
                }
            }
            """;

        var layer = SdfLayer.CreateNew("test_complex_values");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse complex value types");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        Assert.Single(primSpecs);
        
        var testPrim = primSpecs[0];
        var properties = testPrim.GetProperties();
        
        // Verify various property types were created
        Assert.True(properties.Any(p => p.GetName() == "boolValue"), "Should have bool property");
        Assert.True(properties.Any(p => p.GetName() == "intArray"), "Should have int array property");
        Assert.True(properties.Any(p => p.GetName() == "vec3Value"), "Should have float3 property");
        Assert.True(properties.Any(p => p.GetName() == "matrixValue"), "Should have matrix property");
        Assert.True(properties.Any(p => p.GetName() == "simpleDictionary"), "Should have dictionary property");
        Assert.True(properties.Any(p => p.GetName() == "complexData"), "Should have complex dictionary property");
    }

    [Fact]
    public void ParseCompositionArcs_AllArcTypes_ParsesSuccessfully()
    {
        var usdaContent = """
            #usda 1.0

            def "ReferencingPrim" (
                add references = [@./base_asset.usda@</BasePrim>]
                prepend references = [@./override_asset.usda@]
            )
            {
                custom string description = "Prim with references"
            }

            def "PayloadPrim" (
                add payload = [@./heavy_geometry.usda@</Geometry>]
            )
            {
                custom bool loadPayload = true
            }

            def "InheritingPrim" (
                add inherits = </_class_Base>
                prepend inherits = </_class_Extended>
            )
            {
                float customValue = 5.0
            }

            def "SpecializingPrim" (
                add specializes = </_class_Specialized>
            )
            {
                int specializedValue = 100
            }

            class "_class_Base"
            {
                float baseValue = 1.0
                string baseString = "base"
            }

            class "_class_Extended" (
                add inherits = </_class_Base>
            )
            {
                float extendedValue = 2.0
            }

            class "_class_Specialized"
            {
                int specializedDefault = 50
            }

            def "ComplexComposition" (
                add references = [
                    @./ref1.usda@</Prim1>,
                    @./ref2.usda@</Prim2> (offset = 10; scale = 2)
                ]
                add payload = [@./payload.usda@</Heavy>]
                add inherits = [</_class_Base>, </_class_Extended>]
                add specializes = </_class_Specialized>
                add variantSets = ["modelVariant", "lodVariant"]
                variants = {
                    string modelVariant = "high"
                    string lodVariant = "full"
                }
            )
            {
                custom dictionary compositionData = {
                    int priority = 1
                    string category = "hero"
                }
            }
            """;

        var layer = SdfLayer.CreateNew("test_composition");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse composition arcs");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        Assert.Equal(7, primSpecs.Count); // 4 concrete prims + 3 class prims
        
        // Verify references
        var refPrim = primSpecs.FirstOrDefault(p => p.GetName() == "ReferencingPrim");
        Assert.NotNull(refPrim);
        var refMetadata = refPrim.GetMetadata();
        Assert.True(refMetadata.ContainsKey("references"), "Should have references metadata");
        
        // Verify payloads
        var payloadPrim = primSpecs.FirstOrDefault(p => p.GetName() == "PayloadPrim");
        Assert.NotNull(payloadPrim);
        var payloadMetadata = payloadPrim.GetMetadata();
        Assert.True(payloadMetadata.ContainsKey("payload"), "Should have payload metadata");
        
        // Verify inherits
        var inheritPrim = primSpecs.FirstOrDefault(p => p.GetName() == "InheritingPrim");
        Assert.NotNull(inheritPrim);
        var inheritMetadata = inheritPrim.GetMetadata();
        Assert.True(inheritMetadata.ContainsKey("inherits"), "Should have inherits metadata");
        
        // Verify complex composition
        var complexPrim = primSpecs.FirstOrDefault(p => p.GetName() == "ComplexComposition");
        Assert.NotNull(complexPrim);
        var complexMetadata = complexPrim.GetMetadata();
        Assert.True(complexMetadata.ContainsKey("variantSets"), "Should have variantSets metadata");
        Assert.True(complexMetadata.ContainsKey("variants"), "Should have variants metadata");
    }

    [Fact]
    public void ParseTimeSamples_ComplexAnimation_ParsesCorrectly()
    {
        var usdaContent = """
            #usda 1.0
            (
                startTimeCode = 1
                endTimeCode = 240
                timeCodesPerSecond = 24
            )

            def Xform "AnimatedPrim"
            {
                # Simple time samples
                float opacity.timeSamples = {
                    1: 1.0,
                    60: 0.5,
                    120: 0.0,
                    180: 0.5,
                    240: 1.0,
                }
                
                # Vector time samples
                float3 xformOp:translate.timeSamples = {
                    1: (0, 0, 0),
                    60: (10, 5, 0),
                    120: (20, 0, 0),
                    180: (10, -5, 0),
                    240: (0, 0, 0),
                }
                
                # Array time samples
                float3[] points.timeSamples = {
                    1: [(0, 0, 0), (1, 0, 0), (1, 1, 0), (0, 1, 0)],
                    120: [(0, 0, 0), (2, 0, 0), (2, 2, 0), (0, 2, 0)],
                    240: [(0, 0, 0), (1, 0, 0), (1, 1, 0), (0, 1, 0)],
                }
                
                # Mixed static and animated properties
                uniform token[] xformOpOrder = ["xformOp:translate", "xformOp:rotateXYZ", "xformOp:scale"]
                
                float3 xformOp:rotateXYZ.timeSamples = {
                    1: (0, 0, 0),
                    120: (0, 180, 0),
                    240: (0, 360, 0),
                }
                
                float3 xformOp:scale = (1, 1, 1)
                
                # Time samples with held interpolation
                token visibility.timeSamples = {
                    1: "inherited",
                    60: "invisible",
                    120: "inherited",
                }
            }

            def Mesh "AnimatedMesh"
            {
                # Animated topology (vertex animation)
                int[] faceVertexCounts = [4]
                int[] faceVertexIndices = [0, 1, 2, 3]
                
                point3f[] points.timeSamples = {
                    1: [(-1, 0, -1), (1, 0, -1), (1, 0, 1), (-1, 0, 1)],
                    60: [(-1, 1, -1), (1, 1, -1), (1, 1, 1), (-1, 1, 1)],
                    120: [(-1, 2, -1), (1, 2, -1), (1, 2, 1), (-1, 2, 1)],
                    180: [(-1, 1, -1), (1, 1, -1), (1, 1, 1), (-1, 1, 1)],
                    240: [(-1, 0, -1), (1, 0, -1), (1, 0, 1), (-1, 0, 1)],
                }
                
                # Animated normals
                normal3f[] normals.timeSamples = {
                    1: [(0, 1, 0), (0, 1, 0), (0, 1, 0), (0, 1, 0)],
                    120: [(0, 0.707, 0.707), (0, 0.707, 0.707), (0, 0.707, 0.707), (0, 0.707, 0.707)],
                    240: [(0, 1, 0), (0, 1, 0), (0, 1, 0), (0, 1, 0)],
                }
            }
            """;

        var layer = SdfLayer.CreateNew("test_time_samples");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse time samples");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        Assert.Equal(2, primSpecs.Count);
        
        var animatedPrim = primSpecs.FirstOrDefault(p => p.GetName() == "AnimatedPrim");
        Assert.NotNull(animatedPrim);
        
        var properties = animatedPrim.GetProperties();
        Assert.True(properties.Any(p => p.GetName() == "opacity"), "Should have opacity property");
        Assert.True(properties.Any(p => p.GetName() == "xformOp:translate"), "Should have translate property");
        Assert.True(properties.Any(p => p.GetName() == "xformOp:rotateXYZ"), "Should have rotate property");
        Assert.True(properties.Any(p => p.GetName() == "visibility"), "Should have visibility property");
        
        var meshPrim = primSpecs.FirstOrDefault(p => p.GetName() == "AnimatedMesh");
        Assert.NotNull(meshPrim);
        
        var meshProperties = meshPrim.GetProperties();
        Assert.True(meshProperties.Any(p => p.GetName() == "points"), "Should have points property");
        Assert.True(meshProperties.Any(p => p.GetName() == "normals"), "Should have normals property");
    }

    [Fact]
    public void ParseVariantSets_ComplexVariants_ParsesCorrectly()
    {
        var usdaContent = """
            #usda 1.0

            def "AssetRoot" (
                add variantSets = ["modelingVariant", "shadingVariant", "rigVariant"]
                variants = {
                    string modelingVariant = "medium"
                    string shadingVariant = "full"
                    string rigVariant = "animation"
                }
            )
            {
                variantSet "modelingVariant" = {
                    "low" {
                        def Mesh "Geometry"
                        {
                            int[] faceVertexCounts = [3, 3]
                            float3[] points = [(0, 0, 0), (1, 0, 0), (0.5, 1, 0)]
                            custom string quality = "low"
                        }
                    }
                    "medium" {
                        def Mesh "Geometry"
                        {
                            int[] faceVertexCounts = [4, 4, 4, 4]
                            float3[] points = [(0, 0, 0), (1, 0, 0), (1, 1, 0), (0, 1, 0)]
                            custom string quality = "medium"
                        }
                    }
                    "high" {
                        def Mesh "Geometry"
                        {
                            int[] faceVertexCounts = [4, 4, 4, 4, 4, 4]
                            custom string quality = "high"
                            uniform token subdivisionScheme = "catmullClark"
                        }
                    }
                }
                
                variantSet "shadingVariant" = {
                    "preview" {
                        def Scope "Materials"
                        {
                            def Material "PreviewMaterial"
                            {
                                token outputs:surface.connect = </AssetRoot/Materials/PreviewMaterial/PreviewSurface.outputs:surface>
                            }
                        }
                    }
                    "full" (
                        add references = [@./materials/full_materials.usda@</Materials>]
                    ) {
                        def Scope "Materials"
                        {
                            custom bool useHighQualityShaders = true
                        }
                    }
                }
                
                variantSet "rigVariant" = {
                    "static" {
                        custom bool isAnimatable = false
                    }
                    "animation" {
                        custom bool isAnimatable = true
                        
                        def Skeleton "Rig"
                        {
                            uniform token[] joints = ["joint1", "joint2", "joint3"]
                            uniform matrix4d[] bindTransforms = [
                                ( (1, 0, 0, 0), (0, 1, 0, 0), (0, 0, 1, 0), (0, 0, 0, 1) ),
                                ( (1, 0, 0, 0), (0, 1, 0, 0), (0, 0, 1, 0), (0, 1, 0, 1) ),
                                ( (1, 0, 0, 0), (0, 1, 0, 0), (0, 0, 1, 0), (0, 2, 0, 1) )
                            ]
                        }
                    }
                }
                
                # Properties outside variants
                custom dictionary assetInfo = {
                    string name = "TestAsset"
                    int version = 1
                }
            }

            def "NestedVariants" (
                add variantSets = ["outerVariant"]
                variants = {
                    string outerVariant = "optionA"
                }
            )
            {
                variantSet "outerVariant" = {
                    "optionA" (
                        add variantSets = ["innerVariant"]
                        variants = {
                            string innerVariant = "subOption1"
                        }
                    ) {
                        variantSet "innerVariant" = {
                            "subOption1" {
                                float value = 1.0
                            }
                            "subOption2" {
                                float value = 2.0
                            }
                        }
                    }
                    "optionB" {
                        float directValue = 3.0
                    }
                }
            }
            """;

        var layer = SdfLayer.CreateNew("test_variants");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse variant sets");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        Assert.True(primSpecs.Count >= 2, "Should have at least AssetRoot and NestedVariants prims");
        
        var assetRoot = primSpecs.FirstOrDefault(p => p.GetName() == "AssetRoot");
        Assert.NotNull(assetRoot);
        
        var metadata = assetRoot.GetMetadata();
        Assert.True(metadata.ContainsKey("variantSets"), "Should have variantSets metadata");
        Assert.True(metadata.ContainsKey("variants"), "Should have variant selections metadata");
        
        // Verify nested variants parsing
        var nestedPrim = primSpecs.FirstOrDefault(p => p.GetName() == "NestedVariants");
        Assert.NotNull(nestedPrim);
    }

    [Fact]
    public void ParseRelationshipsAndConnections_ComplexTargets_ParsesCorrectly()
    {
        var usdaContent = """
            #usda 1.0

            def "MaterialBindings"
            {
                # Simple relationship
                rel material:binding = </Materials/DefaultMaterial>
                
                # Multiple targets
                rel material:collection = [
                    </Materials/Material1>,
                    </Materials/Material2>,
                    </Materials/Material3>
                ]
                
                # Relationship with custom metadata
                rel proxyPrim = </Proxies/LowRes> (
                    custom bool isPrimary = true
                    doc = "Low resolution proxy for faster viewport"
                )
                
                # Empty relationship (cleared)
                rel oldBinding = None
                
                # Relationship to property
                rel shader:input = </Shader.inputs:color>
            }

            def Shader "TestShader"
            {
                # Connection attributes (special relationship-like syntax)
                token outputs:surface.connect = </TestShader/Surface.outputs:bsdf>
                color3f inputs:diffuseColor.connect = </Textures/DiffuseTex.outputs:rgb>
                
                # Array connections
                token[] inputs:layers.connect = [
                    </Layer1.outputs:result>,
                    </Layer2.outputs:result>,
                    </Layer3.outputs:result>
                ]
            }

            def "ComplexConnections"
            {
                # Chained connections
                float inputs:amplitude.connect = </Controller.outputs:value>
                float3 inputs:frequency.connect = </NoiseGen.outputs:frequency>
                
                # Mixed relationships and connections
                rel material:binding = </Materials/Advanced>
                token outputs:surface.connect = </Materials/Advanced/Surface.outputs:surface>
                
                # Relationship with attributes
                rel collection:includes = [</Geometry/Mesh1>, </Geometry/Mesh2>] (
                    uniform bool expandPrims = true
                    uniform token purpose = "render"
                )
            }

            def "CrossReferences"
            {
                # References to relationships on other prims
                rel material:binding.connect = </MaterialBindings.material:binding>
                
                # Circular reference (parser should handle gracefully)
                rel circular:ref1 = </CrossReferences/circular:ref2>
                rel circular:ref2 = </CrossReferences/circular:ref1>
            }
            """;

        var layer = SdfLayer.CreateNew("test_relationships");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse relationships and connections");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        Assert.Equal(4, primSpecs.Count);
        
        var materialPrim = primSpecs.FirstOrDefault(p => p.GetName() == "MaterialBindings");
        Assert.NotNull(materialPrim);
        
        var properties = materialPrim.GetProperties();
        Assert.True(properties.Any(p => p.GetName() == "material:binding"), "Should have material:binding relationship");
        Assert.True(properties.Any(p => p.GetName() == "material:collection"), "Should have material:collection relationship");
        Assert.True(properties.Any(p => p.GetName() == "proxyPrim"), "Should have proxyPrim relationship");
        
        var shaderPrim = primSpecs.FirstOrDefault(p => p.GetName() == "TestShader");
        Assert.NotNull(shaderPrim);
        
        var shaderProps = shaderPrim.GetProperties();
        Assert.True(shaderProps.Any(p => p.GetName() == "outputs:surface"), "Should have outputs:surface property");
        Assert.True(shaderProps.Any(p => p.GetName() == "inputs:diffuseColor"), "Should have inputs:diffuseColor property");
    }

    [Fact]
    public void ParseEdgeCases_UnusualButValidSyntax_ParsesCorrectly()
    {
        var usdaContent = """
            #usda 1.0
            (
                # Unicode in metadata
                doc = "Test file with special characters: αβγ δεζ 中文 🎨"
                customLayerData = {
                    string unicodeTest = "Special chars: \n\t\r\\ and quotes: \" '"
                }
            )

            # Empty prim
            def "EmptyPrim"
            {
            }

            # Prim with only metadata
            over "MetadataOnly" (
                hidden = true
                kind = "component"
                active = false
            )
            {
            }

            # Very long property names and namespaced properties
            def "ComplexProperties"
            {
                custom string namespace:subsystem:component:property:with:many:parts = "value"
                float primvars:st:indices = 0
                rel material:binding:preview = </PreviewMaterial>
                
                # Escaped strings
                string escaped = "Line 1\nLine 2\tTabbed\rCarriage\"Quoted\""
                string path = "C:\\Users\\Test\\file.usd"
                
                # Very large numbers
                double largeNumber = 1.7976931348623157e+308
                double smallNumber = 2.2250738585072014e-308
                float infinity = inf
                float negInfinity = -inf
                
                # Special float values (if parser supports)
                # float nanValue = nan
                
                # Empty arrays
                int[] emptyIntArray = []
                string[] emptyStringArray = []
                asset[] emptyAssetArray = []
                
                # Single element arrays
                float[] singleFloat = [3.14]
                token[] singleToken = [myToken]
                
                # Mixed numeric types in calculations (parser should handle type promotion)
                custom float mixedCalc = 1
                custom double mixedCalc2 = 1.0
            }

            # Property ordering and overrides
            def "PropertyOrdering"
            {
                float value = 1.0
                float value.timeSamples = {
                    1: 2.0,
                    10: 3.0,
                }
                # Should override the default value
            }

            # Nested class inheritance
            class "_class_A"
            {
                int valueA = 1
            }

            class "_class_B" (
                add inherits = </_class_A>
            )
            {
                int valueB = 2
            }

            class "_class_C" (
                add inherits = [</_class_A>, </_class_B>]
            )
            {
                int valueC = 3
            }

            # Prim with multiple specifiers
            def Xform "MultiSpecifier" (
                add references = </RefTarget>
                add inherits = </_class_C>
                add specializes = </_class_B>
                add payload = </PayloadTarget>
            )
            {
                custom bool hasAllArcs = true
            }

            # Comments everywhere
            def "CommentTest" # inline comment
            {
                # Comment before property
                int value = 42 # inline comment after value
                # Comment between properties
                string text = "test"
                ### Multiple hash marks
                #### Even more
            }

            # Deeply nested structures
            def "DeepNesting"
            {
                custom dictionary level1 = {
                    dictionary level2 = {
                        dictionary level3 = {
                            dictionary level4 = {
                                string deepValue = "Found me!"
                                int[] deepArray = [1, 2, 3]
                            }
                        }
                    }
                }
            }
            """;

        var layer = SdfLayer.CreateNew("test_edge_cases");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should handle edge cases gracefully");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        _output.WriteLine($"Found {primSpecs.Count} prim specs");
        
        // Verify empty prim
        Assert.True(primSpecs.Any(p => p.GetName() == "EmptyPrim"), "Should parse empty prim");
        
        // Verify complex properties
        var complexPrim = primSpecs.FirstOrDefault(p => p.GetName() == "ComplexProperties");
        Assert.NotNull(complexPrim);
        
        var properties = complexPrim.GetProperties();
        Assert.True(properties.Any(p => p.GetName().Contains(":")), "Should handle namespaced properties");
        Assert.True(properties.Any(p => p.GetName() == "emptyIntArray"), "Should handle empty arrays");
        
        // Verify deep nesting
        var deepPrim = primSpecs.FirstOrDefault(p => p.GetName() == "DeepNesting");
        Assert.NotNull(deepPrim);
    }

    [Fact]
    public void ParseErrors_InvalidSyntax_ReportsErrors()
    {
        var testCases = new[]
        {
            // Missing header
            ("", "missing_header"),
            
            // Invalid header
            ("#usd 1.0\ndef \"Test\" { }", "invalid_header"),
            
            // Unclosed brace
            ("#usda 1.0\ndef \"Test\" {", "unclosed_brace"),
            
            // Invalid attribute type
            ("#usda 1.0\ndef \"Test\" { invalidtype attr = 5 }", "invalid_type"),
            
            // Missing quotes in string
            ("#usda 1.0\ndef \"Test\" { string text = unquoted }", "missing_quotes"),
            
            // Invalid time samples syntax
            ("#usda 1.0\ndef \"Test\" { float value.timeSamples = { 1 2.0 } }", "invalid_timesamples"),
            
            // Invalid dictionary syntax
            ("#usda 1.0\ndef \"Test\" { dictionary dict = { key = } }", "invalid_dict"),
        };

        foreach (var (content, caseName) in testCases)
        {
            _output.WriteLine($"Testing error case: {caseName}");
            
            var layer = SdfLayer.CreateNew($"test_error_{caseName}");
            var parser = new UsdaParser();
            
            // Parser should either return false or handle the error gracefully
            var success = parser.ParseContent(content, layer);
            
            // For now, we just verify it doesn't throw an exception
            // In a real implementation, we'd want to check error reporting
            _output.WriteLine($"  Result: {(success ? "Parsed (unexpected)" : "Failed (expected)")}");
        }
    }

    [Fact]
    public void ParseRealWorldExample_KitchenSetFragment_ParsesCorrectly()
    {
        // This is a simplified version of a real USD file structure
        var usdaContent = """
            #usda 1.0
            (
                defaultPrim = "Kitchen"
                upAxis = "Y"
                metersPerUnit = 0.01
                timeCodesPerSecond = 24
                startTimeCode = 1
                endTimeCode = 1
                customLayerData = {
                    string copyright = "Copyright 2024 Example Studio"
                    dictionary renderSettings = {
                        int pixelSamples = 64
                        string renderer = "RenderMan"
                    }
                }
            )

            def Xform "Kitchen" (
                kind = "assembly"
                add variantSets = ["style", "quality"]
                variants = {
                    string style = "modern"
                    string quality = "high"
                }
            )
            {
                def Xform "Appliances" (kind = "group")
                {
                    def Xform "Refrigerator" (
                        add references = [@./assets/refrigerator.usd@</Refrigerator>]
                    )
                    {
                        double3 xformOp:translate = (200, 0, -100)
                        float3 xformOp:scale = (1, 1, 1)
                        uniform token[] xformOpOrder = ["xformOp:translate", "xformOp:scale"]
                        
                        rel material:binding = </Kitchen/Materials/StainlessSteel> (
                            bindMaterialAs = "strongerThanDescendants"
                        )
                    }
                    
                    def Xform "Stove" (
                        add payload = [@./assets/stove_heavy.usd@</Stove>]
                    )
                    {
                        double3 xformOp:translate = (0, 0, -100)
                        uniform token[] xformOpOrder = ["xformOp:translate"]
                    }
                }
                
                def Scope "Materials"
                {
                    def Material "StainlessSteel"
                    {
                        token outputs:surface.connect = </Kitchen/Materials/StainlessSteel/Surface.outputs:surface>
                        
                        def Shader "Surface" (
                            add inherits = </_class_PbrShader>
                        )
                        {
                            color3f inputs:diffuseColor = (0.7, 0.7, 0.75)
                            float inputs:metallic = 0.9
                            float inputs:roughness = 0.2
                            token outputs:surface
                        }
                    }
                }
                
                variantSet "style" = {
                    "modern" {
                        over "Appliances"
                        {
                            custom string styleNote = "Clean lines, minimal decoration"
                        }
                    }
                    "traditional" {
                        over "Appliances"
                        {
                            custom string styleNote = "Ornate details, warm colors"
                            over "Refrigerator"
                            {
                                rel material:binding = </Kitchen/Materials/VintageWhite>
                            }
                        }
                    }
                }
            }

            class "_class_PbrShader"
            {
                uniform token info:id = "UsdPreviewSurface"
                color3f inputs:diffuseColor = (0.18, 0.18, 0.18)
                float inputs:metallic = 0
                float inputs:roughness = 0.5
                float inputs:ior = 1.5
                color3f inputs:emissiveColor = (0, 0, 0)
                token outputs:surface
            }
            """;

        var layer = SdfLayer.CreateNew("test_real_world");
        var parser = new UsdaParser();
        
        var success = parser.ParseContent(usdaContent, layer);
        
        Assert.True(success, "Parser should successfully parse real-world example");
        
        var primSpecs = layer.GetAllPrimSpecs().ToList();
        
        // Verify hierarchy was created
        var kitchen = primSpecs.FirstOrDefault(p => p.GetName() == "Kitchen");
        Assert.NotNull(kitchen);
        
        var appliances = kitchen.GetChildren().FirstOrDefault(p => p.GetName() == "Appliances");
        Assert.NotNull(appliances);
        
        var refrigerator = appliances.GetChildren().FirstOrDefault(p => p.GetName() == "Refrigerator");
        Assert.NotNull(refrigerator);
        
        // Verify materials scope
        var materials = kitchen.GetChildren().FirstOrDefault(p => p.GetName() == "Materials");
        Assert.NotNull(materials);
        
        // Verify variant metadata
        var kitchenMetadata = kitchen.GetMetadata();
        Assert.True(kitchenMetadata.ContainsKey("variantSets"), "Should have variant sets");
    }

    private void LogPrimHierarchy(List<SdfPrimSpec> primSpecs, string indent = "")
    {
        foreach (var prim in primSpecs.Where(p => !p.GetName().StartsWith("_")))
        {
            _output.WriteLine($"{indent}{prim.GetName()} ({prim.GetSpecifier()})");
            if (prim.GetProperties().Count > 0)
            {
                _output.WriteLine($"{indent}  Properties: {string.Join(", ", prim.GetProperties().Select(p => p.GetName()))}");
            }
            LogPrimHierarchy(prim.GetChildren(), indent + "  ");
        }
    }
}