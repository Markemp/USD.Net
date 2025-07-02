using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

namespace Pxr.Usd.UsdUtils;

/// <summary>
/// UsdaWriter provides functionality to serialize USD stages and layers to USDA (ASCII) format.
/// This enables exporting USD.Net created content to standard USD files that can be imported
/// into DCC applications like Blender, Maya, Houdini, etc.
/// </summary>
public class UsdaWriter
{
    private readonly StringBuilder _output;
    private int _indentLevel;
    private const string IndentString = "    "; // 4 spaces per USD convention

    public UsdaWriter()
    {
        _output = new StringBuilder();
        _indentLevel = 0;
    }

    /// <summary>
    /// Write a complete USD stage to USDA format.
    /// </summary>
    public string WriteStage(UsdStage stage)
    {
        _output.Clear();
        _indentLevel = 0;

        // Write file header
        WriteHeader(stage);

        // Write all prims starting from pseudoroot
        var pseudoRoot = stage.GetPseudoRoot();
        foreach (var child in pseudoRoot.GetChildren())
        {
            WritePrim(child);
        }

        return _output.ToString();
    }

    /// <summary>
    /// Write USDA file header with version and optional layer metadata.
    /// </summary>
    private void WriteHeader(UsdStage stage)
    {
        WriteLine("#usda 1.0");

        // Get the root layer and write any metadata
        var rootLayer = stage.GetRootLayer();
        var hasMetadata = false;

        // Check for common layer metadata
        var defaultPrim = stage.GetDefaultPrim();
        if (defaultPrim?.IsValid() == true)
        {
            if (!hasMetadata)
            {
                WriteLine("(");
                _indentLevel++;
                hasMetadata = true;
            }
            WriteLine($"defaultPrim = \"{defaultPrim.GetName()}\"");
        }

        // Add upAxis if we can determine it (assume Z for now)
        if (!hasMetadata)
        {
            WriteLine("(");
            _indentLevel++;
            hasMetadata = true;
        }
        WriteLine("upAxis = \"Z\"");

        if (hasMetadata)
        {
            _indentLevel--;
            WriteLine(")");
        }

        WriteLine(""); // Empty line after header
    }

    /// <summary>
    /// Write a single prim and all its children recursively.
    /// </summary>
    private void WritePrim(UsdPrim prim)
    {
        if (!prim.IsValid())
            return;

        var typeName = prim.GetTypeName();
        var primName = prim.GetName();

        // Write prim definition
        if (string.IsNullOrEmpty(typeName))
        {
            WriteLine($"def Xform \"{primName}\"");
        }
        else
        {
            WriteLine($"def {typeName} \"{primName}\"");
        }

        WriteLine("{");
        _indentLevel++;

        // Write all attributes
        WriteAttributes(prim);

        // Write children
        foreach (var child in prim.GetChildren())
        {
            WritePrim(child);
        }

        _indentLevel--;
        WriteLine("}");
        WriteLine(""); // Empty line between prims
    }

    /// <summary>
    /// Write all attributes for a prim.
    /// </summary>
    private void WriteAttributes(UsdPrim prim)
    {
        var attributes = prim.GetAttributes();
        
        // Sort attributes for consistent output
        var sortedAttrs = attributes.OrderBy(attr => attr.GetName()).ToList();

        foreach (var attr in sortedAttrs)
        {
            WriteAttribute(attr);
        }
    }

    /// <summary>
    /// Write a single attribute with its value and optional time samples.
    /// </summary>
    private void WriteAttribute(UsdAttribute attr)
    {
        if (!attr.IsValid())
            return;

        var attrName = attr.GetName();
        var typeName = attr.GetTypeName();

        // Skip internal USD attributes that shouldn't be serialized
        if (attrName.StartsWith("__") || attrName == "apiSchemas")
            return;

        // Handle special xformOp attributes
        if (attrName == "xformOpOrder")
        {
            WriteXformOpOrder(attr);
            return;
        }

        // Get default value
        var hasDefaultValue = attr.Get(out VtValue defaultValue);
        var hasTimeSamples = attr.GetNumTimeSamples() > 0;

        if (!hasDefaultValue && !hasTimeSamples)
            return;

        // Write attribute with type and default value
        if (hasDefaultValue)
        {
            var valueStr = FormatValue(defaultValue, typeName);
            var typeStr = GetUsdTypeName(typeName);
            
            if (attr.GetVariability() == UsdVariability.Uniform)
            {
                WriteLine($"uniform {typeStr} {attrName} = {valueStr}");
            }
            else if (attrName.StartsWith("xformOp:"))
            {
                WriteLine($"custom {typeStr} {attrName} = {valueStr}");
            }
            else
            {
                WriteLine($"{typeStr} {attrName} = {valueStr}");
            }
        }

        // Write time samples if they exist
        if (hasTimeSamples)
        {
            WriteTimeSamples(attr);
        }
    }

    /// <summary>
    /// Write xformOpOrder as a special uniform token array.
    /// </summary>
    private void WriteXformOpOrder(UsdAttribute attr)
    {
        if (attr.Get(out string[] opOrder))
        {
            var formattedTokens = opOrder.Select(op => $"\"{op}\"").ToArray();
            var tokensStr = "[" + string.Join(", ", formattedTokens) + "]";
            WriteLine($"uniform token[] xformOpOrder = {tokensStr}");
        }
    }

    /// <summary>
    /// Write time samples for an attribute.
    /// </summary>
    private void WriteTimeSamples(UsdAttribute attr)
    {
        var times = attr.GetTimeSamples();
        if (times.Length == 0)
            return;

        var attrName = attr.GetName();
        var typeName = attr.GetTypeName();

        WriteLine($"{attrName}.timeSamples = {{");
        _indentLevel++;

        for (int i = 0; i < times.Length; i++)
        {
            var time = times[i];
            if (attr.Get(out VtValue value, time))
            {
                var valueStr = FormatValue(value, typeName);
                var comma = i < times.Length - 1 ? "," : "";
                WriteLine($"{time}: {valueStr}{comma}");
            }
        }

        _indentLevel--;
        WriteLine("}");
    }

    /// <summary>
    /// Format a VtValue according to USD ASCII conventions.
    /// </summary>
    private string FormatValue(VtValue value, string typeName)
    {
        if (value.IsEmpty())
            return "None";

        // Handle different value types
        if (value.IsHolding<bool>())
        {
            return value.Get<bool>() ? "true" : "false";
        }
        else if (value.IsHolding<int>())
        {
            return value.Get<int>().ToString();
        }
        else if (value.IsHolding<float>())
        {
            return value.Get<float>().ToString("G");
        }
        else if (value.IsHolding<double>())
        {
            return value.Get<double>().ToString("G");
        }
        else if (value.IsHolding<string>())
        {
            return $"\"{value.Get<string>()}\"";
        }
        else if (value.IsHolding<TfToken>())
        {
            return $"\"{value.Get<TfToken>().GetText()}\"";
        }
        else if (value.IsHolding<GfVec3f>())
        {
            var vec = value.Get<GfVec3f>();
            return $"({vec.X:G}, {vec.Y:G}, {vec.Z:G})";
        }
        else if (value.IsHolding<GfVec3Color>())
        {
            var color = value.Get<GfVec3Color>();
            return $"({color.R:G}, {color.G:G}, {color.B:G})";
        }
        else if (value.IsHolding<GfMatrix4d>())
        {
            var matrix = value.Get<GfMatrix4d>();
            var elements = new List<string>();
            for (int i = 0; i < 16; i++)
            {
                elements.Add(matrix[i].ToString("G"));
            }
            return "( (" + string.Join(", ", elements.Take(4)) + "), " +
                   "(" + string.Join(", ", elements.Skip(4).Take(4)) + "), " +
                   "(" + string.Join(", ", elements.Skip(8).Take(4)) + "), " +
                   "(" + string.Join(", ", elements.Skip(12).Take(4)) + ") )";
        }
        else if (value.IsHolding<List<GfVec3f>>())
        {
            var vectors = value.Get<List<GfVec3f>>();
            var formattedVecs = vectors.Select(v => $"({v.X:G}, {v.Y:G}, {v.Z:G})");
            return "[" + string.Join(", ", formattedVecs) + "]";
        }
        else if (value.IsHolding<List<GfVec3Color>>())
        {
            var colors = value.Get<List<GfVec3Color>>();
            var formattedColors = colors.Select(c => $"({c.R:G}, {c.G:G}, {c.B:G})");
            return "[" + string.Join(", ", formattedColors) + "]";
        }
        else if (value.IsHolding<List<int>>())
        {
            var ints = value.Get<List<int>>();
            return "[" + string.Join(", ", ints) + "]";
        }
        else if (value.IsHolding<List<float>>())
        {
            var floats = value.Get<List<float>>();
            return "[" + string.Join(", ", floats.Select(f => f.ToString("G"))) + "]";
        }
        else if (value.IsHolding<string[]>())
        {
            var strings = value.Get<string[]>();
            var quoted = strings.Select(s => $"\"{s}\"");
            return "[" + string.Join(", ", quoted) + "]";
        }

        // Fallback for unknown types
        return $"\"{value.ToString()}\"";
    }

    /// <summary>
    /// Get the USD type name for a given type.
    /// </summary>
    private string GetUsdTypeName(string typeName)
    {
        // Map internal type names to USD type names
        return typeName switch
        {
            "bool" => "bool",
            "int" => "int",
            "float" => "float",
            "double" => "double",
            "string" => "string",
            "token" => "token",
            "float3" => "float3",
            "double3" => "double3",
            "color3f" => "color3f",
            "point3f" => "point3f",
            "normal3f" => "normal3f",
            "matrix4d" => "matrix4d",
            "float3[]" => "float3[]",
            "color3f[]" => "color3f[]",
            "point3f[]" => "point3f[]",
            "int[]" => "int[]",
            "float[]" => "float[]",
            "token[]" => "token[]",
            _ => typeName // Pass through unknown types
        };
    }

    /// <summary>
    /// Write a line with proper indentation.
    /// </summary>
    private void WriteLine(string text = "")
    {
        if (!string.IsNullOrEmpty(text))
        {
            for (int i = 0; i < _indentLevel; i++)
            {
                _output.Append(IndentString);
            }
            _output.AppendLine(text);
        }
        else
        {
            _output.AppendLine();
        }
    }

    /// <summary>
    /// Save a stage to a USDA file.
    /// </summary>
    public static bool SaveStageToFile(UsdStage stage, string filePath)
    {
        try
        {
            var writer = new UsdaWriter();
            var usdaContent = writer.WriteStage(stage);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, usdaContent, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving USDA file: {ex.Message}");
            return false;
        }
    }
}