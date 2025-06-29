using System;
using Pxr.Usd.Sdf;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing SdfPath behavior:");
        
        var emptyPath = SdfPath.EmptyPath();
        Console.WriteLine($"EmptyPath().IsEmpty(): {emptyPath.IsEmpty()}");
        Console.WriteLine($"EmptyPath().IsAbsolutePath(): {emptyPath.IsAbsolutePath()}");
        Console.WriteLine($"EmptyPath().GetString(): '{emptyPath.GetString()}'");
        
        var relativePath = new SdfPath("relativePath");
        Console.WriteLine($"new SdfPath(\"relativePath\").IsEmpty(): {relativePath.IsEmpty()}");
        Console.WriteLine($"new SdfPath(\"relativePath\").IsAbsolutePath(): {relativePath.IsAbsolutePath()}");
        Console.WriteLine($"new SdfPath(\"relativePath\").GetString(): '{relativePath.GetString()}'");
        
        var absolutePath = new SdfPath("/validAbsolutePath");
        Console.WriteLine($"new SdfPath(\"/validAbsolutePath\").IsEmpty(): {absolutePath.IsEmpty()}");
        Console.WriteLine($"new SdfPath(\"/validAbsolutePath\").IsAbsolutePath(): {absolutePath.IsAbsolutePath()}");
        Console.WriteLine($"new SdfPath(\"/validAbsolutePath\").GetString(): '{absolutePath.GetString()}'");
    }
}