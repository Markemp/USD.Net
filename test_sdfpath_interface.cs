using Pxr.Usd.Sdf;
using System;

// Simple test to validate ISdfPath interface compatibility
class Program 
{
    static void Main()
    {
        // Test that SdfPath can be used as ISdfPath
        SdfPath concretePath = new SdfPath("/test/path");
        ISdfPath interfacePath = concretePath;
        
        Console.WriteLine($"Concrete path: {concretePath.GetAsString()}");
        Console.WriteLine($"Interface path: {interfacePath.GetAsString()}");
        Console.WriteLine($"Are equal: {concretePath.Equals(interfacePath)}");
        
        // Test static methods work on SdfPath
        bool isValid = SdfPath.IsValidIdentifier("validName");
        Console.WriteLine($"Is 'validName' valid: {isValid}");
        
        // Test method that accepts ISdfPath
        TestMethod(concretePath);
        TestMethod(interfacePath);
    }
    
    static void TestMethod(ISdfPath path)
    {
        Console.WriteLine($"TestMethod received: {path.GetAsString()}");
        Console.WriteLine($"Is prim path: {path.IsPrimPath()}");
        Console.WriteLine($"Is empty: {path.IsEmpty()}");
    }
}