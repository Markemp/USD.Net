using System;
using System.Collections.Generic;
using System.Linq;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfPath represents paths in the scene graph hierarchy, used for identifying prim and property locations.
/// It provides efficient path manipulation and traversal operations.
/// </summary>
public readonly struct SdfPath : IEquatable<SdfPath>, IComparable<SdfPath>
{
    private readonly string _pathString;

    public SdfPath(string pathString)
    {
        _pathString = pathString?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(pathString));
        
        if (string.IsNullOrEmpty(_pathString))
            _pathString = "/";
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
        return lastSlash >= 0 ? _pathString.Substring(lastSlash + 1) : _pathString;
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

    public bool Equals(SdfPath other) => 
        string.Equals(_pathString, other._pathString, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SdfPath other && Equals(other);

    public override int GetHashCode() => _pathString?.GetHashCode() ?? 0;

    public int CompareTo(SdfPath other) => 
        string.Compare(_pathString, other._pathString, StringComparison.Ordinal);

    public override string ToString() => _pathString ?? string.Empty;

    public static bool operator ==(SdfPath left, SdfPath right) => left.Equals(right);
    public static bool operator !=(SdfPath left, SdfPath right) => !left.Equals(right);
    public static bool operator <(SdfPath left, SdfPath right) => left.CompareTo(right) < 0;
    public static bool operator >(SdfPath left, SdfPath right) => left.CompareTo(right) > 0;
    public static bool operator <=(SdfPath left, SdfPath right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SdfPath left, SdfPath right) => left.CompareTo(right) >= 0;
}