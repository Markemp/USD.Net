using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;
using Xunit.Abstractions;

namespace USD.Net.Tests;

public class UsdDebugTests
{
    private readonly ITestOutputHelper _output;

    public UsdDebugTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Debug_BasicStageTraversal_ShouldShowAllPrims()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        
        // Create a simple test prim
        var testPrim = stage.DefinePrim("/TestPrim");
        testPrim.SetTypeName("Mesh");
        
        _output.WriteLine($"Created prim: {testPrim.GetPath().GetString()}");
        _output.WriteLine($"Prim is valid: {testPrim.IsValid()}");
        _output.WriteLine($"Prim type name: '{testPrim.GetTypeName()}'");
        _output.WriteLine($"Prim is active: {testPrim.IsActive()}");
        _output.WriteLine($"Prim is defined: {testPrim.IsDefined()}");
        _output.WriteLine($"Prim is loaded: {testPrim.IsLoaded()}");
        _output.WriteLine($"Prim is abstract: {testPrim.IsAbstract()}");
        
        // Test each predicate individually
        _output.WriteLine($"Default predicate result: {UsdPrimPredicates.Default(testPrim)}");
        //_output.WriteLine($"OfType('Mesh') result: {UsdPrimPredicates.OfType(\"Mesh\")(testPrim)}");
        
        // Check what TraverseAll returns
        var allPrims = stage.TraverseAll().ToList();
        _output.WriteLine($"TraverseAll found {allPrims.Count} prims:");
        foreach (var prim in allPrims)
        {
            _output.WriteLine($"  - {prim.GetPath().GetString()} (type: '{prim.GetTypeName()}')");
            _output.WriteLine($"    IsAbsoluteRootPath: {prim.GetPath().IsAbsoluteRootPath()}");
        }
        
        // Check if GetPseudoRoot adds anything to the index
        var pseudoRoot = stage.GetPseudoRoot();
        _output.WriteLine($"Pseudo root path: {pseudoRoot.GetPath().GetString()}");
        _output.WriteLine($"Pseudo root IsAbsoluteRootPath: {pseudoRoot.GetPath().IsAbsoluteRootPath()}");
        
        var allPrimsAfterPseudo = stage.TraverseAll().ToList();
        _output.WriteLine($"TraverseAll after GetPseudoRoot found {allPrimsAfterPseudo.Count} prims:");
        foreach (var prim in allPrimsAfterPseudo)
        {
            _output.WriteLine($"  - {prim.GetPath().GetString()} (type: '{prim.GetTypeName()}')");
        }
        
        // Test the query method directly
        try
        {
            var count = UsdQuery.CountPrims(stage, UsdPrimPredicates.OfType("Mesh"));
            _output.WriteLine($"UsdQuery.CountPrims result: {count}");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"UsdQuery.CountPrims threw exception: {ex.Message}");
        }
        
        // Assert basic functionality
        Assert.True(allPrims.Count > 0, "TraverseAll should find at least one prim");
    }
    
    [Fact]
    public void Debug_ComprehensiveStage_ShouldShowStructure()
    {
        // Use the same test stage creation as the failing tests
        var stage = CreateComprehensiveTestStage();
        
        var allPrims = stage.TraverseAll().ToList();
        _output.WriteLine($"Comprehensive stage has {allPrims.Count} total prims:");
        
        foreach (var prim in allPrims)
        {
            _output.WriteLine($"Prim: {prim.GetPath().GetString()}");
            _output.WriteLine($"  Type: '{prim.GetTypeName()}'");
            _output.WriteLine($"  Valid: {prim.IsValid()}");
            _output.WriteLine($"  Active: {prim.IsActive()}");
            _output.WriteLine($"  Defined: {prim.IsDefined()}");
            _output.WriteLine($"  Loaded: {prim.IsLoaded()}");
            _output.WriteLine($"  Abstract: {prim.IsAbstract()}");
            _output.WriteLine($"  Default predicate: {UsdPrimPredicates.Default(prim)}");
            if (prim.GetTypeName() == "Mesh")
            {
                _output.WriteLine($"  OfType('Mesh'): {UsdPrimPredicates.OfType("Mesh")(prim)}");
            }
            _output.WriteLine("");
        }
        
        // Count mesh prims manually
        var meshCount = allPrims.Count(p => p.GetTypeName() == "Mesh");
        _output.WriteLine($"Manual count of Mesh prims: {meshCount}");
        
        // Count using predicate
        var predicateCount = allPrims.Count(UsdPrimPredicates.OfType("Mesh"));
        _output.WriteLine($"Predicate count of Mesh prims: {predicateCount}");
        
        // Count using combined predicate
        var combined = UsdPrimPredicates.And(UsdPrimPredicates.Default, UsdPrimPredicates.OfType("Mesh"));
        var combinedCount = allPrims.Count(combined);
        _output.WriteLine($"Combined predicate count: {combinedCount}");
    }
    
    private UsdStage CreateComprehensiveTestStage()
    {
        var stage = UsdStage.CreateInMemory();
        
        // World root
        var world = stage.DefinePrim("/World");
        
        // Environment
        var environment = stage.DefinePrim("/World/Environment");
        var sky = stage.DefinePrim("/World/Environment/Sky");
        sky.SetTypeName("SkyDome");
        var ground = stage.DefinePrim("/World/Environment/Ground");
        ground.SetTypeName("Plane");
        
        // Characters
        var characters = stage.DefinePrim("/World/Characters");
        characters.SetMetadata("kind", "group");
        
        var hero = stage.DefinePrim("/World/Characters/Hero");
        hero.SetMetadata("kind", "component");
        hero.SetTypeName("Mesh");
        
        var heroBody = stage.DefinePrim("/World/Characters/Hero/Body");
        heroBody.SetTypeName("Mesh");
        
        var heroClothing = stage.DefinePrim("/World/Characters/Hero/Clothing");
        heroClothing.SetTypeName("Mesh");
        heroClothing.SetActive(false); // Inactive
        
        var npcGuard1 = stage.DefinePrim("/World/Characters/NPC_Guard1");
        npcGuard1.SetMetadata("kind", "component");
        npcGuard1.SetTypeName("Mesh");
        
        var npcGuard2 = stage.DefinePrim("/World/Characters/NPC_Guard2");
        npcGuard2.SetMetadata("kind", "component");
        npcGuard2.SetTypeName("Mesh");
        
        // Props
        var props = stage.DefinePrim("/World/Props");
        props.SetMetadata("kind", "group");
        
        var table = stage.DefinePrim("/World/Props/Table");
        table.SetMetadata("kind", "component");
        table.SetTypeName("Mesh");
        
        var chair01 = stage.DefinePrim("/World/Props/Chair_01");
        chair01.SetMetadata("kind", "component");
        chair01.SetTypeName("Mesh");
        
        var chair02 = stage.DefinePrim("/World/Props/Chair_02");
        chair02.SetMetadata("kind", "component");
        chair02.SetTypeName("Mesh");
        
        return stage;
    }
}