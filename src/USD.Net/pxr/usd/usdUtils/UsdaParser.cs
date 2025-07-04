using System.Globalization;
using System.Text.RegularExpressions;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdUtils;

/// <summary>
/// UsdaParser provides functionality to parse USD ASCII (.usda) files and populate USD layers.
/// This enables reading standard USD files created by other applications.
/// </summary>
public class UsdaParser
{
    private readonly List<string> _lines = new();
    private int _currentLine = 0;
    private int _indentLevel = 0;

    /// <summary>
    /// Parse a USDA file and populate the given layer.
    /// </summary>
    public bool ParseFile(string filePath, SdfLayer layer)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var content = File.ReadAllText(filePath);
            return ParseContent(content, layer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing USDA file {filePath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Parse USDA content string and populate the given layer.
    /// </summary>
    public bool ParseContent(string content, SdfLayer layer)
    {
        try
        {
            _lines.Clear();
            _lines.AddRange(content.Split('\n').Select(line => line.TrimEnd('\r')));
            _currentLine = 0;
            _indentLevel = 0;

            if (!ParseHeader(layer))
                return false;

            ParseLayerMetadata(layer);
            ParsePrims(layer);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing USDA content: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Parse the USDA header (version info).
    /// </summary>
    private bool ParseHeader(SdfLayer layer)
    {
        if (_currentLine >= _lines.Count)
            return false;

        var line = _lines[_currentLine].Trim();
        if (!line.StartsWith("#usda"))
        {
            Console.WriteLine("Invalid USDA file: missing #usda header");
            return false;
        }

        _currentLine++;
        return true;
    }

    /// <summary>
    /// Parse layer metadata (the section in parentheses after the header).
    /// </summary>
    private void ParseLayerMetadata(SdfLayer layer)
    {
        SkipWhitespaceAndComments();

        if (_currentLine >= _lines.Count || !_lines[_currentLine].Trim().StartsWith("("))
            return;

        _currentLine++; // Skip opening paren

        while (_currentLine < _lines.Count)
        {
            var line = _lines[_currentLine].Trim();

            if (line.StartsWith(")"))
            {
                _currentLine++;
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                _currentLine++;
                continue;
            }

            // Parse metadata key-value pairs
            ParseMetadataEntry(line, layer);
            _currentLine++;
        }
    }

    /// <summary>
    /// Parse a single metadata entry.
    /// </summary>
    private void ParseMetadataEntry(string line, SdfLayer layer)
    {
        var match = Regex.Match(line, @"(\w+)\s*=\s*(.+)");
        if (!match.Success)
            return;

        var key = match.Groups[1].Value;
        var valueStr = match.Groups[2].Value.Trim();

        // Remove quotes and parse value
        var value = ParseValue(valueStr);
        layer.SetMetadata(new TfToken(key), value);
    }

    /// <summary>
    /// Parse all prims in the layer.
    /// </summary>
    private void ParsePrims(SdfLayer layer)
    {
        SkipWhitespaceAndComments();

        while (_currentLine < _lines.Count)
        {
            var line = _lines[_currentLine].Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                _currentLine++;
                continue;
            }

            if (line.StartsWith("def"))
            {
                ParsePrimDefinition(layer, null);
            }
            else
            {
                _currentLine++;
            }
        }
    }

    /// <summary>
    /// Parse a prim definition starting with "def".
    /// </summary>
    private SdfPrimSpec? ParsePrimDefinition(SdfLayer layer, SdfPrimSpec? parentPrim)
    {
        var line = _lines[_currentLine].Trim();
        var match = Regex.Match(line, @"def\s+(?:(\w+)\s+)?""([^""]+)""");
        
        if (!match.Success)
        {
            _currentLine++;
            return null;
        }

        var typeName = match.Groups[1].Value;
        var primName = match.Groups[2].Value;

        // Create prim spec
        var primPath = parentPrim?.GetPath().AppendChild(primName) ?? new SdfPath($"/{primName}");
        var primSpec = SdfPrimSpec.New(layer, primPath, primName);
        
        if (!string.IsNullOrEmpty(typeName))
            primSpec.SetTypeName(typeName);

        // Add the prim spec to the layer
        layer.AddPrimSpec(primSpec);

        _currentLine++; // Move past the def line

        // Look for opening brace
        SkipWhitespaceAndComments();
        if (_currentLine < _lines.Count && _lines[_currentLine].Trim() == "{")
        {
            _currentLine++; // Skip opening brace
            _indentLevel++;

            // Parse prim contents
            ParsePrimContents(layer, primSpec);

            _indentLevel--;
        }

        return primSpec;
    }

    /// <summary>
    /// Parse the contents of a prim (attributes and child prims).
    /// </summary>
    private void ParsePrimContents(SdfLayer layer, SdfPrimSpec primSpec)
    {
        while (_currentLine < _lines.Count)
        {
            SkipWhitespaceAndComments();
            
            if (_currentLine >= _lines.Count)
                break;

            var line = _lines[_currentLine].Trim();

            if (line == "}")
            {
                _currentLine++; // Skip closing brace
                break;
            }

            if (line.StartsWith("def"))
            {
                var childPrimSpec = ParsePrimDefinition(layer, primSpec);
                if (childPrimSpec != null)
                {
                    primSpec.AddChild(childPrimSpec);
                }
            }
            else if (IsAttributeDefinition(line))
            {
                ParseAttributeDefinition(line, layer, primSpec);
                _currentLine++;
            }
            else
            {
                _currentLine++;
            }
        }
    }

    /// <summary>
    /// Check if a line defines an attribute.
    /// </summary>
    private bool IsAttributeDefinition(string line)
    {
        // Match patterns like: "type name = value" or "uniform type name = value"
        return Regex.IsMatch(line, @"^(uniform\s+)?\w+\s+\w+\s*=");
    }

    /// <summary>
    /// Parse an attribute definition.
    /// </summary>
    private void ParseAttributeDefinition(string line, SdfLayer layer, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(line, @"^(uniform\s+)?(\w+)\s+(\w+)\s*=\s*(.+)");
        if (!match.Success)
            return;

        var isUniform = !string.IsNullOrEmpty(match.Groups[1].Value);
        var typeName = match.Groups[2].Value;
        var attrName = match.Groups[3].Value;
        var valueStr = match.Groups[4].Value.Trim();

        // Create attribute spec
        var attrPath = primSpec.GetPath().AppendProperty(attrName);
        var attrSpec = SdfPropertySpec.New(layer, attrPath, attrName, typeName);

        // Parse and set the value
        var value = ParseValue(valueStr);
        attrSpec.SetDefaultValue(value);

        if (isUniform)
            attrSpec.SetVariability(UsdVariability.Uniform);
    }

    /// <summary>
    /// Parse a value from a string representation.
    /// </summary>
    private VtValue ParseValue(string valueStr)
    {
        valueStr = valueStr.Trim();

        // Remove trailing comma if present
        if (valueStr.EndsWith(","))
            valueStr = valueStr[..^1].Trim();

        // String values (quoted)
        if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
        {
            return new VtValue(valueStr[1..^1]);
        }

        // Boolean values
        if (valueStr == "true")
            return new VtValue(true);
        if (valueStr == "false")
            return new VtValue(false);

        // Tuple values like (1.0, 2.0, 3.0)
        if (valueStr.StartsWith("(") && valueStr.EndsWith(")"))
        {
            return ParseTupleValue(valueStr);
        }

        // Array values like [1, 2, 3]
        if (valueStr.StartsWith("[") && valueStr.EndsWith("]"))
        {
            return ParseArrayValue(valueStr);
        }

        // Numeric values
        if (int.TryParse(valueStr, out var intVal))
            return new VtValue(intVal);

        if (float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal))
            return new VtValue(floatVal);

        if (double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleVal))
            return new VtValue(doubleVal);

        // Default to string
        return new VtValue(valueStr);
    }

    /// <summary>
    /// Parse tuple values like (1.0, 2.0, 3.0).
    /// </summary>
    private VtValue ParseTupleValue(string valueStr)
    {
        var content = valueStr[1..^1].Trim(); // Remove parentheses
        var parts = content.Split(',').Select(p => p.Trim()).ToArray();

        if (parts.Length == 3)
        {
            // Try to parse as float3
            if (parts.All(p => float.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out _)))
            {
                var values = parts.Select(p => float.Parse(p, CultureInfo.InvariantCulture)).ToArray();
                return new VtValue(new List<float> { values[0], values[1], values[2] });
            }
        }

        // Fallback: return as string array
        return new VtValue(parts);
    }

    /// <summary>
    /// Parse array values like [1, 2, 3].
    /// </summary>
    private VtValue ParseArrayValue(string valueStr)
    {
        var content = valueStr[1..^1].Trim(); // Remove brackets
        if (string.IsNullOrEmpty(content))
            return new VtValue(new List<object>());

        var parts = content.Split(',').Select(p => p.Trim()).ToArray();

        // Try to determine array type
        if (parts.All(p => int.TryParse(p, out _)))
        {
            var values = parts.Select(int.Parse).ToList();
            return new VtValue(values);
        }

        if (parts.All(p => float.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out _)))
        {
            var values = parts.Select(p => float.Parse(p, CultureInfo.InvariantCulture)).ToList();
            return new VtValue(values);
        }

        // String array (remove quotes)
        var stringValues = parts.Select(p =>
        {
            p = p.Trim();
            if (p.StartsWith("\"") && p.EndsWith("\""))
                return p[1..^1];
            return p;
        }).ToArray();

        return new VtValue(stringValues);
    }

    /// <summary>
    /// Skip whitespace lines and comments.
    /// </summary>
    private void SkipWhitespaceAndComments()
    {
        while (_currentLine < _lines.Count)
        {
            var line = _lines[_currentLine].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                _currentLine++;
            }
            else
            {
                break;
            }
        }
    }
}