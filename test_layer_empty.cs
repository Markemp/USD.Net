using Pxr.Usd.Sdf;
using System;

// Simple test to validate SdfLayer.IsEmpty() behavior
class Program 
{
    static void Main()
    {
        // Create a new empty layer
        var layer = new SdfLayer("test://empty");
        
        Console.WriteLine($"Empty layer IsEmpty(): {layer.IsEmpty()}");
        Console.WriteLine($"Root prims count: {layer.GetRootPrims().Count}");
        Console.WriteLine($"Root prim order count: {layer.GetRootPrimOrder().Count}");
        Console.WriteLine($"Sub layers count: {layer.GetSubLayerPaths().Count}");
        Console.WriteLine($"Relocates count: {layer.GetRelocates().Count}");
    }
}