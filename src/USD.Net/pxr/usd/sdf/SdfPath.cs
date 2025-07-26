using Pxr.Base.Tf;
namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfPath represents paths in the scene graph hierarchy, used for identifying prim and property locations.
/// It provides efficient path manipulation and traversal operations.
/// </summary>
public readonly struct SdfPath : ISdfPath
{
    private readonly string _pathString;

    public SdfPath(string pathString)
    {
        if (pathString is null)
            throw new ArgumentNullException(nameof(pathString));
            
        _pathString = pathString.TrimEnd('/');
        
        // Only convert empty to root if it was originally just "/"
        if (pathString == "/")
            _pathString = "/";
        else if (string.IsNullOrEmpty(_pathString))
            _pathString = string.Empty; // Keep empty as empty
    }

    /// <summary>
    /// Get the absolute root path.
    /// </summary>
    public static SdfPath AbsoluteRootPath() => new("/");

    /// <summary>
    /// Get an empty/invalid path.
    /// </summary>
    public static SdfPath EmptyPath() => new(string.Empty);
    
    /// <summary>
    /// The relative path representing "self".
    /// </summary>
    public static SdfPath ReflexiveRelativePath() => new(".");

    /// <summary>
    /// Returns whether name is a legal identifier for any path component.
    /// </summary>
    public static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        
        // Basic validation - can be enhanced later
        return name.All(c => char.IsLetterOrDigit(c) || c == '_') && 
               !char.IsDigit(name[0]);
    }

    /// <summary>
    /// Returns whether name is a legal namespaced identifier.
    /// This returns true if IsValidIdentifier() does.
    /// </summary>
    public static bool IsValidNamespacedIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        
        // Split by namespace delimiter ':'
        var parts = name.Split(':');
        return parts.All(part => !string.IsNullOrEmpty(part) && IsValidIdentifier(part));
    }

    /// <summary>
    /// Tokenizes name by the namespace delimiter.
    /// Returns the empty list if name is not a valid namespaced identifier.
    /// </summary>
    public static IReadOnlyList<string> TokenizeIdentifier(string name)
    {
        if (!IsValidNamespacedIdentifier(name))
            return Array.Empty<string>();
            
        return name.Split(':');
    }

    /// <summary>
    /// Tokenizes name by the namespace delimiter.
    /// Returns the empty list if name is not a valid namespaced identifier.
    /// </summary>
    public static IReadOnlyList<TfToken> TokenizeIdentifierAsTokens(string name)
    {
        var stringTokens = TokenizeIdentifier(name);
        return stringTokens.Select(s => new TfToken(s)).ToArray();
    }

    /// <summary>
    /// Join names into a single identifier using the namespace delimiter.
    /// Any empty strings present in names are ignored when joining.
    /// </summary>
    public static string JoinIdentifier(IEnumerable<string> names)
        => string.Join(":", names.Where(n => !string.IsNullOrEmpty(n)));

    /// <summary>
    /// Join names into a single identifier using the namespace delimiter.
    /// Any empty strings present in names are ignored when joining.
    /// </summary>
    public static string JoinIdentifier(IEnumerable<TfToken> names)
        => JoinIdentifier(names.Select(t => t.GetText()));

    /// <summary>
    /// Join lhs and rhs into a single identifier using the namespace delimiter.
    /// Returns lhs if rhs is empty and vice verse.
    /// Returns an empty string if both lhs and rhs are empty.
    /// </summary>
    public static string JoinIdentifier(string lhs, string rhs)
    {
        if (string.IsNullOrEmpty(lhs)) return rhs ?? string.Empty;
        if (string.IsNullOrEmpty(rhs)) return lhs;
        return lhs + ":" + rhs;
    }

    /// <summary>
    /// Join lhs and rhs into a single identifier using the namespace delimiter.
    /// Returns lhs if rhs is empty and vice verse.
    /// Returns an empty string if both lhs and rhs are empty.
    /// </summary>
    public static string JoinIdentifier(TfToken lhs, TfToken rhs)
        => JoinIdentifier(lhs.GetText(), rhs.GetText());

    /// <summary>
    /// Returns name stripped of any namespaces.
    /// This does not check the validity of the name; it just attempts
    /// to remove anything that looks like a namespace.
    /// </summary>
    public static string StripNamespace(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        
        var lastColon = name.LastIndexOf(':');
        return lastColon >= 0 ? name.Substring(lastColon + 1) : name;
    }

    /// <summary>
    /// Returns name stripped of any namespaces.
    /// This does not check the validity of the name; it just attempts
    /// to remove anything that looks like a namespace.
    /// </summary>
    public static TfToken StripNamespace(TfToken name)
        => new TfToken(StripNamespace(name.GetText()));
    }
    /// <summary>
    /// Returns (name, true) where name is stripped of the prefix specified by matchNamespace
    /// if name indeed starts with matchNamespace. Returns (name, false) otherwise, with name unmodified.
    /// This function deals with both the case where matchNamespace contains the trailing namespace delimiter ':' or not.
    /// </summary>
    public static (string strippedName, bool wasStripped) StripPrefixNamespace(string name, string matchNamespace)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(matchNamespace))
            return (name, false);
            
        var prefix = matchNamespace.EndsWith(":") ? matchNamespace : matchNamespace + ":";
        
        if (name.StartsWith(prefix))
            return (name.Substring(prefix.Length), true);
        
        return (name, false);
    }

    /// <summary>
    /// Return true if pathString is a valid path string, meaning that passing the string
    /// to the SdfPath constructor will result in a valid, non-empty SdfPath.
    /// Otherwise, return false and if errMsg is not null, set the pointed-to string to the parse error.
    /// </summary>
    public static bool IsValidPathString(string pathString, out string? errMsg)
    {
        errMsg = null;
        
        if (string.IsNullOrEmpty(pathString))
        {
            errMsg = "Path string cannot be empty";
            return false;
        }
        
        // Basic validation - can be enhanced later
        try
        {
            var path = new SdfPath(pathString);
            return !path.IsEmpty();
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Given some vector of paths, get a vector of concise unambiguous relative paths.
    /// GetConciseRelativePaths requires a vector of absolute paths. It finds a set of relative paths
    /// such that each relative path is unique.
    /// </summary>
    public static IReadOnlyList<ISdfPath> GetConciseRelativePaths(IEnumerable<ISdfPath> paths)
    {
        // TODO: Implement concise relative path algorithm
        return paths.ToArray();
    }

    /// <summary>
    /// Remove all elements of paths that are prefixed by other elements in paths.
    /// As a side-effect, the result is left in sorted order.
    /// </summary>
    public static void RemoveDescendentPaths(IList<ISdfPath> paths)
    {
        if (paths.Count <= 1) return;
        
        var sortedPaths = paths.OrderBy(p => p.GetAsString()).ToList();
        paths.Clear();
        
        for (int i = 0; i < sortedPaths.Count; i++)
        {
            bool isDescendent = false;
            for (int j = 0; j < i; j++)
            {
                if (sortedPaths[i].HasPrefix(sortedPaths[j]))
                {
                    isDescendent = true;
                    break;
                }
            }
            
            if (!isDescendent)
                paths.Add(sortedPaths[i]);
        }
    }

    /// <summary>
    /// Remove all elements of paths that prefix other elements in paths.
    /// As a side-effect, the result is left in sorted order.
    /// </summary>
    public static void RemoveAncestorPaths(IList<ISdfPath> paths)
    {
        if (paths.Count <= 1) return;
        
        var sortedPaths = paths.OrderBy(p => p.GetAsString()).ToList();
        paths.Clear();
        
        for (int i = 0; i < sortedPaths.Count; i++)
        {
            bool isAncestor = false;
            for (int j = i + 1; j < sortedPaths.Count; j++)
            {
                if (sortedPaths[j].HasPrefix(sortedPaths[i]))
                {
                    isAncestor = true;
                    break;
                }
            }
            
            if (!isAncestor)
                paths.Add(sortedPaths[i]);
        }
    }

    /// <summary>
    /// Return true if this path is valid.
    /// </summary>
    public bool IsEmpty() => string.IsNullOrEmpty(_pathString);

    /// <summary>
    /// Return true if this is an absolute path.
    /// </summary>
    public bool IsAbsolutePath() => !IsEmpty() && _pathString.StartsWith('/');

    /// <summary>
    /// Return true if this is the absolute root path.
    /// </summary>
    public bool IsAbsoluteRootPath() => _pathString == "/";

    /// <summary>
    /// Return true if this path identifies a prim.
    /// </summary>
    public bool IsPrimPath() => IsAbsolutePath() && !_pathString.Contains('.');

    /// <summary>
    /// Return true if this path identifies a property.
    /// </summary>
    public bool IsPropertyPath() => IsAbsolutePath() && _pathString.Contains('.');

    /// <summary>
    /// Get the parent path of this path.
    /// </summary>
    public SdfPath GetParentPath()
    {
        if (IsEmpty() || IsAbsoluteRootPath())
            return EmptyPath();

        // For property paths, the parent is the prim (before the dot)
        if (IsPropertyPath())
        {
            var dotIndex = _pathString.LastIndexOf('.');
            if (dotIndex > 0)
                return new SdfPath(_pathString.Substring(0, dotIndex));
        }

        // For prim paths, the parent is the path before the last slash
        var lastSlash = _pathString.LastIndexOf('/');
        if (lastSlash <= 0)
            return AbsoluteRootPath();

        return new SdfPath(_pathString.Substring(0, lastSlash));
    }

    /// <summary>
    /// Get the name of this path (the final component).
    /// </summary>
    public string GetName()
    {
        if (IsEmpty() || IsAbsoluteRootPath())
            return string.Empty;

        var lastSlash = _pathString.LastIndexOf('/');
        var pathAfterSlash = lastSlash >= 0 ? _pathString.Substring(lastSlash + 1) : _pathString;
        
        // For property paths, return just the property name (after the dot)
        if (IsPropertyPath())
        {
            var dotIndex = pathAfterSlash.LastIndexOf('.');
            return dotIndex >= 0 ? pathAfterSlash.Substring(dotIndex + 1) : pathAfterSlash;
        }
        
        return pathAfterSlash;
    }

    /// <summary>
    /// Get the element string representation.
    /// </summary>
    public string GetElementString() => GetName();

    /// <summary>
    /// Append a child path component.
    /// </summary>
    public SdfPath AppendChild(string childName)
    {
        if (string.IsNullOrEmpty(childName))
            throw new ArgumentException("Child name cannot be null or empty", nameof(childName));

        if (IsEmpty())
            throw new InvalidOperationException("Cannot append to empty path");

        var newPath = IsAbsoluteRootPath() ? $"/{childName}" : $"{_pathString}/{childName}";
        return new SdfPath(newPath);
    }

    /// <summary>
    /// Append a property name to create a property path.
    /// </summary>
    public SdfPath AppendProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

        if (IsEmpty())
            throw new InvalidOperationException("Cannot append to empty path");

        return new SdfPath($"{_pathString}.{propertyName}");
    }

    /// <summary>
    /// Get the path string representation.
    /// </summary>
    public string GetString() => _pathString ?? string.Empty;

    /// <summary>
    /// Return true if this path has a given prefix.
    /// </summary>
    public bool HasPrefix(SdfPath prefix)
    {
        if (prefix.IsEmpty())
            return true;
        
        return GetString().StartsWith(prefix.GetString());
    }

    /// <summary>
    /// Get all ancestor paths.
    /// </summary>
    public IEnumerable<SdfPath> GetAncestorPaths()
    {
        var current = GetParentPath();
        while (!current.IsEmpty())
        {
            yield return current;
            current = current.GetParentPath();
        }
    }

    public static implicit operator SdfPath(string pathString) => new(pathString);
    public static implicit operator string(SdfPath path) => path.GetString();

    public bool Equals(ISdfPath? other) => 
        other is SdfPath otherPath && string.Equals(_pathString, otherPath._pathString, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SdfPath other && Equals(other);

    public override int GetHashCode() => _pathString?.GetHashCode() ?? 0;

    public int CompareTo(ISdfPath? other) => 
        other is SdfPath otherPath ? string.Compare(_pathString, otherPath._pathString, StringComparison.Ordinal) : 1;

    public override string ToString() => _pathString ?? string.Empty;

    public int GetPathElementCount()
    {
        if (IsEmpty()) return 0;
        if (IsAbsoluteRootPath()) return 0;
        
        var path = _pathString;
        if (path.StartsWith("/")) path = path.Substring(1);
        
        var components = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = components.Length;
        
        // Count property elements
        if (IsPropertyPath())
        {
            var lastComponent = components.LastOrDefault() ?? "";
            var dotCount = lastComponent.Count(c => c == '.');
            count += dotCount; // Each dot adds another element
        }
        
        return count;
    }

    public bool IsAbsoluteRootOrPrimPath()
        => IsAbsoluteRootPath() || IsPrimPath();

    public bool IsRootPrimPath()
    {
        if (!IsAbsolutePath() || IsAbsoluteRootPath()) return false;
        
        // Root prim path has exactly one component after the root slash
        var withoutRoot = _pathString.Substring(1);
        return !withoutRoot.Contains('/') && !withoutRoot.Contains('.');
    }

    public bool IsPrimPropertyPath()
    {
        if (!IsPropertyPath()) return false;
        
        // A prim property path is a property that's not a relational attribute
        // Relational attributes contain brackets []
        return !_pathString.Contains('[');
    }

    public bool IsNamespacedPropertyPath()
    {
        if (!IsPropertyPath()) return false;
        
        var propertyName = GetName();
        return propertyName.Contains(':');
    }

    public bool IsPrimVariantSelectionPath()
        => _pathString.Contains('{') && _pathString.Contains('}');

    public bool IsPrimOrPrimVariantSelectionPath()
        => IsPrimPath() || IsPrimVariantSelectionPath();

    public bool ContainsPrimVariantSelection()
        => _pathString.Contains('{') && _pathString.Contains('}');

    public bool ContainsPropertyElements()
        => _pathString.Contains('.');

    public bool ContainsTargetPath()
        => _pathString.Contains('[') && _pathString.Contains(']');

    public bool IsRelationalAttributePath()
    {
        // Relational attributes are properties that come after a target path []
        return IsPropertyPath() && _pathString.Contains('[') && _pathString.Contains(']');
    }

    public bool IsTargetPath()
    {
        // Target paths end with [targetPath] but don't have properties after
        return _pathString.Contains('[') && _pathString.Contains(']') && !IsRelationalAttributePath();
    }

    public bool IsMapperPath()
    {
        // Mapper paths contain .mapper syntax
        return _pathString.Contains(".mapper[");
    }

    public bool IsMapperArgPath()
    {
        // Mapper arg paths are arguments to mapper paths
        return IsMapperPath() && _pathString.Contains(".mapper[") && _pathString.EndsWith("]");
    }

    public bool IsExpressionPath()
    {
        // Expression paths contain .expression syntax
        return _pathString.Contains(".expression");
    }

    public TfToken GetAsToken() => new TfToken(_pathString);

    public TfToken GetToken() => GetAsToken();

    public string GetAsString() => _pathString ?? string.Empty;

    public string GetText() => _pathString ?? string.Empty;

    public IReadOnlyList<ISdfPath> GetPrefixes()
    {
        var prefixes = new List<ISdfPath>();
        GetPrefixes(prefixes);
        return prefixes;
    }

    public IReadOnlyList<ISdfPath> GetPrefixes(int numPrefixes)
    {
        var prefixes = new List<ISdfPath>();
        GetPrefixes(prefixes, numPrefixes);
        return prefixes;
    }

    public void GetPrefixes(IList<ISdfPath> prefixes)
    {
        prefixes.Clear();
        
        if (IsEmpty()) return;
        
        var ancestors = GetAncestorPaths().Reverse().ToList();
        foreach (var ancestor in ancestors)
        {
            prefixes.Add(ancestor);
        }
        prefixes.Add(this);
    }

    public void GetPrefixes(IList<ISdfPath> prefixes, int numPrefixes)
    {
        GetPrefixes(prefixes);
        
        if (numPrefixes > 0 && prefixes.Count > numPrefixes)
        {
            // Keep only the last numPrefixes (including self)
            var toKeep = prefixes.Skip(prefixes.Count - numPrefixes).ToList();
            prefixes.Clear();
            foreach (var item in toKeep)
                prefixes.Add(item);
        }
    }

    public IEnumerable<ISdfPath> GetAncestorsRange() => GetAncestorPaths().Cast<ISdfPath>();

    public TfToken GetNameToken() => new TfToken(GetName());

    public TfToken GetElementToken() => new TfToken(GetElementString());

    public ISdfPath ReplaceName(TfToken newName)
    {
        if (IsEmpty()) return this;
        
        var parent = GetParentPath();
        if (parent.IsEmpty()) return this;
        
        if (IsPrimPath())
            return new SdfPath(parent._pathString + "/" + newName.GetText());
        else if (IsPropertyPath())
            return new SdfPath(parent._pathString + "." + newName.GetText());
            
        return this;
    }

    public ISdfPath GetTargetPath()
    {
        // Extract target path from [targetPath] syntax
        var start = _pathString.LastIndexOf('[');
        var end = _pathString.LastIndexOf(']');
        
        if (start >= 0 && end > start)
        {
            var targetPathStr = _pathString.Substring(start + 1, end - start - 1);
            return new SdfPath(targetPathStr);
        }
        
        return EmptyPath();
    }

    public void GetAllTargetPathsRecursively(IList<ISdfPath> result)
    {
        // Simplified implementation - just get immediate target
        var target = GetTargetPath();
        if (!target.IsEmpty())
            result.Add(target);
    }

    public (string variantSet, string variant) GetVariantSelection()
    {
        // Extract variant selection from {variantSet=variant} syntax
        var start = _pathString.LastIndexOf('{');
        var end = _pathString.LastIndexOf('}');
        
        if (start >= 0 && end > start)
        {
            var variantStr = _pathString.Substring(start + 1, end - start - 1);
            var equalIndex = variantStr.IndexOf('=');
            
            if (equalIndex >= 0)
            {
                var variantSet = variantStr.Substring(0, equalIndex);
                var variant = variantStr.Substring(equalIndex + 1);
                return (variantSet, variant);
            }
        }
        
        return (string.Empty, string.Empty);
    }

    public bool HasPrefix(ISdfPath prefix)
    {
        if (prefix is SdfPath prefixPath)
            return HasPrefix(prefixPath);
        return false;
    }

    ISdfPath ISdfPath.GetParentPath() => GetParentPath();

    public ISdfPath GetPrimPath()
    {
        if (IsPrimPath()) return this;
        if (IsEmpty() || IsAbsoluteRootPath()) return this;
        
        // Strip property parts
        if (IsPropertyPath())
        {
            var dotIndex = _pathString.LastIndexOf('.');
            if (dotIndex > 0)
                return new SdfPath(_pathString.Substring(0, dotIndex));
        }
        
        return this;
    }

    public ISdfPath GetPrimOrPrimVariantSelectionPath()
    {
        if (IsPrimOrPrimVariantSelectionPath()) return this;
        return GetPrimPath();
    }

    public ISdfPath GetAbsoluteRootOrPrimPath()
    {
        if (IsAbsoluteRootPath() || IsPrimPath()) return this;
        return GetPrimPath();
    }

    public ISdfPath StripAllVariantSelections()
    {
        // Remove {variantSet=variant} patterns
        var result = _pathString;
        while (result.Contains('{') && result.Contains('}'))
        {
            var start = result.IndexOf('{');
            var end = result.IndexOf('}');
            
            if (end > start)
                result = result.Remove(start, end - start + 1);
            else break;
        }
        return new SdfPath(result);
    }

    public ISdfPath AppendPath(ISdfPath newSuffix)
    {
        if (newSuffix is SdfPath suffixPath)
        {
            if (suffixPath.IsEmpty()) return this;
            if (IsEmpty()) return suffixPath;
            
            // Simple concatenation for now
            var result = _pathString.TrimEnd('/') + "/" + suffixPath._pathString.TrimStart('/');
            return new SdfPath(result);
        }
        return this;
    }

    public ISdfPath AppendChild(TfToken childName) => AppendChild(childName.GetText());

    public ISdfPath AppendProperty(TfToken propName) => AppendProperty(propName.GetText());

    public ISdfPath AppendVariantSelection(string variantSet, string variant) 
        => new SdfPath(_pathString + $"{{{variantSet}={variant}}}");

    public ISdfPath AppendTarget(ISdfPath targetPath)
    {
        if (targetPath is SdfPath target)
            return new SdfPath(_pathString + $"[{target._pathString}]");
        return this;
    }

    public ISdfPath AppendRelationalAttribute(TfToken attrName)
        => new SdfPath(_pathString + "." + attrName.GetText());

    public ISdfPath ReplaceTargetPath(ISdfPath newTargetPath)
    {
        // TODO: Implement target path replacement
        return this;
    }

    public ISdfPath AppendMapper(ISdfPath targetPath)
    {
        if (targetPath is SdfPath target)
            return new SdfPath(_pathString + $".mapper[{target._pathString}]");
        return this;
    }

    public ISdfPath AppendMapperArg(TfToken argName)
        => new SdfPath(_pathString + "." + argName.GetText());

    public ISdfPath AppendExpression() => new SdfPath(_pathString + ".expression");

    public ISdfPath AppendElementString(string element)
    {
        // TODO: Parse element string and append appropriately
        return new SdfPath(_pathString + "/" + element);
    }

    public ISdfPath AppendElementToken(TfToken elementTok)
        => AppendElementString(elementTok.GetText());

    public ISdfPath ReplacePrefix(ISdfPath oldPrefix, ISdfPath newPrefix, bool fixTargetPaths = true)
    {
        // TODO: Implement prefix replacement
        return this;
    }

    public ISdfPath GetCommonPrefix(ISdfPath path)
    {
        // TODO: Implement common prefix finding
        return EmptyPath();
    }

    public (ISdfPath first, ISdfPath second) RemoveCommonSuffix(ISdfPath otherPath, bool stopAtRootPrim = false)
    {
        // TODO: Implement common suffix removal
        return (this, otherPath);
    }

    public ISdfPath MakeAbsolutePath(ISdfPath anchor)
    {
        // TODO: Implement absolute path resolution
        return this;
    }

    public ISdfPath MakeRelativePath(ISdfPath anchor)
    {
        // TODO: Implement relative path creation
        return this;
    }

    public static bool operator ==(SdfPath left, SdfPath right) => left.Equals(right);
    public static bool operator !=(SdfPath left, SdfPath right) => !left.Equals(right);
    public static bool operator <(SdfPath left, SdfPath right) => left.CompareTo(right) < 0;
    public static bool operator >(SdfPath left, SdfPath right) => left.CompareTo(right) > 0;
    public static bool operator <=(SdfPath left, SdfPath right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SdfPath left, SdfPath right) => left.CompareTo(right) >= 0;
    
    // Additional Equals overload for struct comparison
    public bool Equals(SdfPath other) => 
        string.Equals(_pathString, other._pathString, StringComparison.Ordinal);
        
    public int CompareTo(SdfPath other) => 
        string.Compare(_pathString, other._pathString, StringComparison.Ordinal);
}