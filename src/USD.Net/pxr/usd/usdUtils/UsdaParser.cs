using System.Globalization;
using System.Text.RegularExpressions;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;

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
    private readonly Stack<SdfPath> _pathStack = new();
    private readonly Dictionary<string, List<string>> _variantSets = new();
    private readonly Dictionary<string, string> _variantSelections = new();

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

            if (line.StartsWith("def") || line.StartsWith("over") || line.StartsWith("class"))
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
    /// Parse a prim definition starting with "def", "over", or "class".
    /// </summary>
    private SdfPrimSpec? ParsePrimDefinition(SdfLayer layer, SdfPrimSpec? parentPrim)
    {
        var line = _lines[_currentLine].Trim();
        var primType = GetPrimDefinitionType(line);
        if (primType == null)
        {
            _currentLine++;
            return null;
        }

        var (typeName, primName, metadata) = ParsePrimHeader(line);
        if (string.IsNullOrEmpty(primName))
        {
            _currentLine++;
            return null;
        }

        // Create prim spec
        var primPath = parentPrim?.GetPath().AppendChild(primName) ?? new SdfPath($"/{primName}");
        var primSpec = SdfPrimSpec.New(layer, primPath, primName);
        
        if (!string.IsNullOrEmpty(typeName))
            primSpec.SetTypeName(typeName);

        // Add the prim spec to the layer
        layer.AddPrimSpec(primSpec);

        // Parse metadata if present
        if (!string.IsNullOrEmpty(metadata))
        {
            ParsePrimMetadata(metadata, primSpec);
        }

        _currentLine++; // Move past the def line

        // Look for opening brace
        SkipWhitespaceAndComments();
        if (_currentLine < _lines.Count && _lines[_currentLine].Trim() == "{")
        {
            _currentLine++; // Skip opening brace
            _indentLevel++;
            _pathStack.Push(primPath);

            // Parse prim contents
            ParsePrimContents(layer, primSpec);

            _pathStack.Pop();
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

            if (line.StartsWith("def") || line.StartsWith("over") || line.StartsWith("class"))
            {
                var childPrimSpec = ParsePrimDefinition(layer, primSpec);
                if (childPrimSpec != null)
                {
                    primSpec.AddChild(childPrimSpec);
                }
            }
            else if (line.StartsWith("rel "))
            {
                ParseRelationshipDefinition(line, layer, primSpec);
                _currentLine++;
            }
            else if (line.StartsWith("variantSet "))
            {
                ParseVariantSetBlock(layer, primSpec);
            }
            else if (IsAttributeDefinition(line))
            {
                ParseAttributeDefinition(line, layer, primSpec);
                _currentLine++;
            }
            else if (IsTimeSampleBlock(line))
            {
                ParseTimeSampleBlock(line, layer, primSpec);
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

        // Create and add attribute spec to the prim
        var attrSpec = primSpec.CreateProperty(attrName, typeName);

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

        // None/null values
        if (valueStr == "None" || valueStr == "null")
            return new VtValue();

        // Dictionary values like {key: value, ...}
        if (valueStr.StartsWith("{") && valueStr.EndsWith("}"))
        {
            return ParseDictionaryValue(valueStr);
        }

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

        // Token values (unquoted identifiers)
        if (Regex.IsMatch(valueStr, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
        {
            return new VtValue(new TfToken(valueStr));
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
                return new VtValue(new GfVec3f(values[0], values[1], values[2]));
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
    /// Get the prim definition type (def, over, class) from a line.
    /// </summary>
    private string? GetPrimDefinitionType(string line)
    {
        if (line.StartsWith("def ")) return "def";
        if (line.StartsWith("over ")) return "over";
        if (line.StartsWith("class ")) return "class";
        return null;
    }

    /// <summary>
    /// Parse the prim header to extract type name, prim name, and metadata.
    /// </summary>
    private (string typeName, string primName, string metadata) ParsePrimHeader(string line)
    {
        // Match patterns like:
        // def "PrimName" (...)
        // def TypeName "PrimName" (...)
        // over "PrimName" (...)
        var match = Regex.Match(line, @"^(?:def|over|class)\s+(?:(\w+)\s+)?""([^""]+)""\s*(\(.*\))?\s*$");
        
        if (!match.Success)
            return (string.Empty, string.Empty, string.Empty);

        var typeName = match.Groups[1].Value;
        var primName = match.Groups[2].Value;
        var metadata = match.Groups[3].Value;

        return (typeName, primName, metadata);
    }

    /// <summary>
    /// Parse prim metadata from the parentheses section.
    /// </summary>
    private void ParsePrimMetadata(string metadataStr, SdfPrimSpec primSpec)
    {
        // Remove outer parentheses
        if (metadataStr.StartsWith("(") && metadataStr.EndsWith(")"))
        {
            metadataStr = metadataStr[1..^1].Trim();
        }

        // Parse composition arcs and other metadata
        var entries = SplitMetadataEntries(metadataStr);
        foreach (var entry in entries)
        {
            ParsePrimMetadataEntry(entry, primSpec);
        }
    }

    /// <summary>
    /// Split metadata string into individual entries, handling nested structures.
    /// </summary>
    private List<string> SplitMetadataEntries(string metadataStr)
    {
        var entries = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;
        var inQuotes = false;
        var i = 0;

        while (i < metadataStr.Length)
        {
            var ch = metadataStr[i];

            if (ch == '"' && (i == 0 || metadataStr[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes)
            {
                if (ch == '[' || ch == '(' || ch == '{')
                {
                    depth++;
                }
                else if (ch == ']' || ch == ')' || ch == '}')
                {
                    depth--;
                }
                else if (ch == ',' && depth == 0)
                {
                    entries.Add(current.ToString().Trim());
                    current.Clear();
                    i++;
                    continue;
                }
            }

            current.Append(ch);
            i++;
        }

        if (current.Length > 0)
        {
            entries.Add(current.ToString().Trim());
        }

        return entries;
    }

    /// <summary>
    /// Parse a single prim metadata entry.
    /// </summary>
    private void ParsePrimMetadataEntry(string entry, SdfPrimSpec primSpec)
    {
        // Handle composition arcs
        if (entry.StartsWith("add references") || entry.StartsWith("references"))
        {
            ParseReferences(entry, primSpec);
        }
        else if (entry.StartsWith("add payload") || entry.StartsWith("payload"))
        {
            ParsePayloads(entry, primSpec);
        }
        else if (entry.StartsWith("add inherits") || entry.StartsWith("inherits"))
        {
            ParseInherits(entry, primSpec);
        }
        else if (entry.StartsWith("add specializes") || entry.StartsWith("specializes"))
        {
            ParseSpecializes(entry, primSpec);
        }
        else if (entry.StartsWith("add variantSets") || entry.StartsWith("variantSets"))
        {
            ParseVariantSets(entry, primSpec);
        }
        else if (entry.StartsWith("variants"))
        {
            ParseVariantSelections(entry, primSpec);
        }
        else
        {
            // Regular metadata
            var match = Regex.Match(entry, @"(\w+)\s*=\s*(.+)");
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                var valueStr = match.Groups[2].Value.Trim();
                var value = ParseValue(valueStr);
                primSpec.SetMetadata(new TfToken(key), value);
            }
        }
    }

    /// <summary>
    /// Parse references metadata.
    /// </summary>
    private void ParseReferences(string entry, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(entry, @"(?:add\s+)?references\s*=\s*(.+)");
        if (!match.Success) return;

        var refsStr = match.Groups[1].Value.Trim();
        var references = ParseReferenceList(refsStr);
        
        // TODO: Add references to primSpec when SdfPrimSpec supports it
        // For now, store as metadata
        primSpec.SetMetadata(new TfToken("references"), new VtValue(references));
    }

    /// <summary>
    /// Parse a list of references.
    /// </summary>
    private List<SdfReference> ParseReferenceList(string refsStr)
    {
        var references = new List<SdfReference>();
        
        if (refsStr.StartsWith("[") && refsStr.EndsWith("]"))
        {
            refsStr = refsStr[1..^1].Trim();
        }

        var refEntries = SplitReferenceEntries(refsStr);
        foreach (var refEntry in refEntries)
        {
            var reference = ParseSingleReference(refEntry);
            if (reference.HasValue)
            {
                references.Add(reference.Value);
            }
        }

        return references;
    }

    /// <summary>
    /// Parse a single reference entry.
    /// </summary>
    private SdfReference? ParseSingleReference(string refStr)
    {
        refStr = refStr.Trim();

        // Internal reference: </Path>
        if (refStr.StartsWith("<") && refStr.EndsWith(">"))
        {
            var pathStr = refStr[1..^1];
            var path = new SdfPath(pathStr);
            return SdfReference.CreateInternal(path);
        }

        // External reference: @file.usda@ or @file.usda@</Path>
        var match = Regex.Match(refStr, @"@([^@]+)@(?:<([^>]+)>)?");
        if (match.Success)
        {
            var assetPath = match.Groups[1].Value;
            var primPathStr = match.Groups[2].Value;
            
            if (string.IsNullOrEmpty(primPathStr))
            {
                return SdfReference.CreateDefaultPrim(assetPath);
            }
            else
            {
                var primPath = new SdfPath(primPathStr);
                return SdfReference.CreateExternal(assetPath, primPath);
            }
        }

        return null;
    }

    /// <summary>
    /// Split reference entries, handling nested structures.
    /// </summary>
    private List<string> SplitReferenceEntries(string refsStr)
    {
        var entries = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;
        var inReference = false;

        for (int i = 0; i < refsStr.Length; i++)
        {
            var ch = refsStr[i];

            if (ch == '@')
            {
                inReference = !inReference;
            }
            else if (!inReference)
            {
                if (ch == '<')
                {
                    depth++;
                }
                else if (ch == '>')
                {
                    depth--;
                }
                else if (ch == ',' && depth == 0)
                {
                    entries.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }
            }

            current.Append(ch);
        }

        if (current.Length > 0)
        {
            entries.Add(current.ToString().Trim());
        }

        return entries;
    }

    /// <summary>
    /// Parse payloads metadata.
    /// </summary>
    private void ParsePayloads(string entry, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(entry, @"(?:add\s+)?payload\s*=\s*(.+)");
        if (!match.Success) return;

        var payloadsStr = match.Groups[1].Value.Trim();
        var payloads = ParsePayloadList(payloadsStr);
        
        // TODO: Add payloads to primSpec when SdfPrimSpec supports it
        // For now, store as metadata
        primSpec.SetMetadata(new TfToken("payload"), new VtValue(payloads));
    }

    /// <summary>
    /// Parse a list of payloads.
    /// </summary>
    private List<SdfPayload> ParsePayloadList(string payloadsStr)
    {
        var payloads = new List<SdfPayload>();
        
        if (payloadsStr.StartsWith("[") && payloadsStr.EndsWith("]"))
        {
            payloadsStr = payloadsStr[1..^1].Trim();
        }

        var payloadEntries = SplitReferenceEntries(payloadsStr); // Same logic as references
        foreach (var payloadEntry in payloadEntries)
        {
            var payload = ParseSinglePayload(payloadEntry);
            if (payload.HasValue)
            {
                payloads.Add(payload.Value);
            }
        }

        return payloads;
    }

    /// <summary>
    /// Parse a single payload entry.
    /// </summary>
    private SdfPayload? ParseSinglePayload(string payloadStr)
    {
        payloadStr = payloadStr.Trim();

        // Internal payload: </Path>
        if (payloadStr.StartsWith("<") && payloadStr.EndsWith(">"))
        {
            var pathStr = payloadStr[1..^1];
            var path = new SdfPath(pathStr);
            return SdfPayload.CreateInternal(path);
        }

        // External payload: @file.usda@ or @file.usda@</Path>
        var match = Regex.Match(payloadStr, @"@([^@]+)@(?:<([^>]+)>)?");
        if (match.Success)
        {
            var assetPath = match.Groups[1].Value;
            var primPathStr = match.Groups[2].Value;
            
            if (string.IsNullOrEmpty(primPathStr))
            {
                return SdfPayload.CreateDefaultPrim(assetPath);
            }
            else
            {
                var primPath = new SdfPath(primPathStr);
                return SdfPayload.CreateExternal(assetPath, primPath);
            }
        }

        return null;
    }

    /// <summary>
    /// Parse inherits metadata.
    /// </summary>
    private void ParseInherits(string entry, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(entry, @"(?:add\s+)?inherits\s*=\s*(.+)");
        if (!match.Success) return;

        var inheritsStr = match.Groups[1].Value.Trim();
        var inherits = ParsePathList(inheritsStr);
        
        // TODO: Add inherits to primSpec when SdfPrimSpec supports it
        // For now, store as metadata
        primSpec.SetMetadata(new TfToken("inherits"), new VtValue(inherits));
    }

    /// <summary>
    /// Parse specializes metadata.
    /// </summary>
    private void ParseSpecializes(string entry, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(entry, @"(?:add\s+)?specializes\s*=\s*(.+)");
        if (!match.Success) return;

        var specializesStr = match.Groups[1].Value.Trim();
        var specializes = ParsePathList(specializesStr);
        
        // TODO: Add specializes to primSpec when SdfPrimSpec supports it
        // For now, store as metadata
        primSpec.SetMetadata(new TfToken("specializes"), new VtValue(specializes));
    }

    /// <summary>
    /// Parse a list of paths.
    /// </summary>
    private List<SdfPath> ParsePathList(string pathsStr)
    {
        var paths = new List<SdfPath>();
        
        if (pathsStr.StartsWith("[") && pathsStr.EndsWith("]"))
        {
            pathsStr = pathsStr[1..^1].Trim();
        }

        // Single path
        if (pathsStr.StartsWith("<") && pathsStr.EndsWith(">") && !pathsStr.Contains(","))
        {
            var pathStr = pathsStr[1..^1];
            paths.Add(new SdfPath(pathStr));
            return paths;
        }

        // Multiple paths
        var pathEntries = SplitPathEntries(pathsStr);
        foreach (var pathEntry in pathEntries)
        {
            var trimmed = pathEntry.Trim();
            if (trimmed.StartsWith("<") && trimmed.EndsWith(">"))
            {
                var pathStr = trimmed[1..^1];
                paths.Add(new SdfPath(pathStr));
            }
        }

        return paths;
    }

    /// <summary>
    /// Split path entries.
    /// </summary>
    private List<string> SplitPathEntries(string pathsStr)
    {
        var entries = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;

        foreach (var ch in pathsStr)
        {
            if (ch == '<')
            {
                depth++;
            }
            else if (ch == '>')
            {
                depth--;
            }
            else if (ch == ',' && depth == 0)
            {
                entries.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
        {
            entries.Add(current.ToString().Trim());
        }

        return entries;
    }

    /// <summary>
    /// Parse variant sets metadata.
    /// </summary>
    private void ParseVariantSets(string entry, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(entry, @"(?:add\s+)?variantSets\s*=\s*(.+)");
        if (!match.Success) return;

        var variantSetsStr = match.Groups[1].Value.Trim();
        var variantSets = ParseStringArray(variantSetsStr);
        
        // Store variant sets for this prim
        var primPath = primSpec.GetPath().GetString();
        foreach (var variantSet in variantSets)
        {
            if (!_variantSets.ContainsKey(primPath))
            {
                _variantSets[primPath] = new List<string>();
            }
            _variantSets[primPath].Add(variantSet);
        }
        
        primSpec.SetMetadata(new TfToken("variantSets"), new VtValue(variantSets));
    }

    /// <summary>
    /// Parse variant selections metadata.
    /// </summary>
    private void ParseVariantSelections(string entry, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(entry, @"variants\s*=\s*\{(.+)\}");
        if (!match.Success) return;

        var selectionsStr = match.Groups[1].Value.Trim();
        var selections = ParseVariantSelectionDict(selectionsStr);
        
        // Store variant selections
        var primPath = primSpec.GetPath().GetString();
        foreach (var kvp in selections)
        {
            var key = $"{primPath}:{kvp.Key}";
            _variantSelections[key] = kvp.Value;
        }
        
        primSpec.SetMetadata(new TfToken("variants"), new VtValue(selections));
    }

    /// <summary>
    /// Parse variant selection dictionary.
    /// </summary>
    private Dictionary<string, string> ParseVariantSelectionDict(string dictStr)
    {
        var dict = new Dictionary<string, string>();
        var entries = SplitMetadataEntries(dictStr);
        
        foreach (var entry in entries)
        {
            var match = Regex.Match(entry, @"string\s+(\w+)\s*=\s*""([^""]+)""");
            if (match.Success)
            {
                var variantSet = match.Groups[1].Value;
                var selection = match.Groups[2].Value;
                dict[variantSet] = selection;
            }
        }
        
        return dict;
    }

    /// <summary>
    /// Parse string array value.
    /// </summary>
    private List<string> ParseStringArray(string arrayStr)
    {
        var result = new List<string>();
        
        if (arrayStr.StartsWith("[") && arrayStr.EndsWith("]"))
        {
            arrayStr = arrayStr[1..^1].Trim();
        }
        
        var parts = arrayStr.Split(',');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
            {
                result.Add(trimmed[1..^1]);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Parse a relationship definition.
    /// </summary>
    private void ParseRelationshipDefinition(string line, SdfLayer layer, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(line, @"rel\s+(\w+)(?:\s*=\s*(.+))?");
        if (!match.Success) return;

        var relName = match.Groups[1].Value;
        var targetsStr = match.Groups[2].Value;

        // Create relationship property
        var relSpec = primSpec.CreateProperty(relName, "rel");

        if (!string.IsNullOrEmpty(targetsStr))
        {
            var targets = ParsePathList(targetsStr);
            // TODO: Set targets when SdfPropertySpec supports relationships
            relSpec.SetMetadata(new TfToken("targetPaths"), targets);
        }
    }

    /// <summary>
    /// Check if a line defines a time sample block.
    /// </summary>
    private bool IsTimeSampleBlock(string line)
    {
        return line.Contains(".timeSamples") && line.Contains("=");
    }

    /// <summary>
    /// Parse a time sample block.
    /// </summary>
    private void ParseTimeSampleBlock(string line, SdfLayer layer, SdfPrimSpec primSpec)
    {
        var match = Regex.Match(line, @"(\w+)\.timeSamples\s*=\s*\{");
        if (!match.Success)
        {
            _currentLine++;
            return;
        }

        var attrName = match.Groups[1].Value;
        var timeSamples = new Dictionary<double, object>();

        _currentLine++; // Move past the opening line

        while (_currentLine < _lines.Count)
        {
            var sampleLine = _lines[_currentLine].Trim();
            if (sampleLine == "}")
            {
                _currentLine++;
                break;
            }

            var sampleMatch = Regex.Match(sampleLine, @"([\d\.\-]+):\s*(.+),?");
            if (sampleMatch.Success)
            {
                var time = double.Parse(sampleMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                var valueStr = sampleMatch.Groups[2].Value.TrimEnd(',');
                var value = ParseValue(valueStr);
                timeSamples[time] = value;
            }

            _currentLine++;
        }

        // TODO: Set time samples when SdfPropertySpec supports it
        var attrSpec = primSpec.GetProperties().FirstOrDefault(p => p.GetName() == attrName);
        if (attrSpec != null)
        {
            attrSpec.SetMetadata(new TfToken("timeSamples"), timeSamples);
        }
    }

    /// <summary>
    /// Parse a variant set block.
    /// </summary>
    private void ParseVariantSetBlock(SdfLayer layer, SdfPrimSpec primSpec)
    {
        var line = _lines[_currentLine].Trim();
        var match = Regex.Match(line, @"variantSet\s+""(\w+)""\s*=\s*\{");
        if (!match.Success)
        {
            _currentLine++;
            return;
        }

        var variantSetName = match.Groups[1].Value;
        _currentLine++; // Move past the opening line

        var variantSpecs = new Dictionary<string, SdfPrimSpec>();

        while (_currentLine < _lines.Count)
        {
            SkipWhitespaceAndComments();
            if (_currentLine >= _lines.Count) break;

            var variantLine = _lines[_currentLine].Trim();
            if (variantLine == "}")
            {
                _currentLine++;
                break;
            }

            // Parse variant definition
            var variantMatch = Regex.Match(variantLine, @"""([^""]+)""\s*\{");
            if (variantMatch.Success)
            {
                var variantName = variantMatch.Groups[1].Value;
                _currentLine++; // Move past variant opening

                // Create a temporary prim spec for the variant
                var variantPath = new SdfPath($"{primSpec.GetPath().GetString()}{{{variantSetName}={variantName}}}");
                var variantSpec = SdfPrimSpec.New(layer, variantPath, variantName);
                
                // Parse variant contents
                ParsePrimContents(layer, variantSpec);
                
                variantSpecs[variantName] = variantSpec;
            }
            else
            {
                _currentLine++;
            }
        }

        // TODO: Store variant specs when SdfPrimSpec supports it
        primSpec.SetMetadata(new TfToken($"variantSet:{variantSetName}"), new VtValue(variantSpecs));
    }

    /// <summary>
    /// Parse dictionary values like {key: value, ...}.
    /// </summary>
    private VtValue ParseDictionaryValue(string valueStr)
    {
        var content = valueStr[1..^1].Trim(); // Remove braces
        if (string.IsNullOrEmpty(content))
            return new VtValue(new Dictionary<string, object>());

        var dict = new Dictionary<string, object>();
        var entries = SplitMetadataEntries(content);
        
        foreach (var entry in entries)
        {
            var colonIndex = entry.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = entry[..colonIndex].Trim();
                var valueStr2 = entry[(colonIndex + 1)..].Trim();
                
                // Remove quotes from key if present
                if (key.StartsWith("\"") && key.EndsWith("\""))
                {
                    key = key[1..^1];
                }
                
                var value = ParseValue(valueStr2);
                dict[key] = value.GetValue() ?? new object();
            }
        }
        
        return new VtValue(dict);
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