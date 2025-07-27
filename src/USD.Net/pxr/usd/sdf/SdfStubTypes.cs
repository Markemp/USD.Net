namespace Pxr.Usd.Sdf;

using System.Collections.Generic;
using Pxr.Base.Tf;

/// <summary>
/// Stub types for SdfLayer interface compatibility
/// These are minimal implementations to allow compilation.
/// TODO: Implement full functionality when needed.
/// </summary>

/// <summary>
/// Vector of prim spec handles.
/// </summary>
public class SdfPrimSpecHandleVector : List<SdfPrimSpec>
{
}

/// <summary>
/// Vector of layer offsets.
/// </summary>
public class SdfLayerOffsetVector : List<SdfLayerOffset>
{
}

/// <summary>
/// Proxy for editing name order fields.
/// </summary>
public class SdfNameOrderProxy
{
    private readonly List<TfToken> _names = new();
    
    public int Count => _names.Count;
    public TfToken this[int index] => _names[index];
    
    public void Clear() => _names.Clear();
    public void Add(TfToken name) => _names.Add(name);
    public void Insert(int index, TfToken name) => _names.Insert(index, name);
    public void RemoveAt(int index) => _names.RemoveAt(index);
    public bool Remove(TfToken name) => _names.Remove(name);
}

/// <summary>
/// Proxy for editing sublayer paths.
/// </summary>
public class SdfSubLayerProxy
{
    private readonly List<string> _paths = new();
    
    public int Count => _paths.Count;
    public string this[int index] => _paths[index];
    
    public void Clear() => _paths.Clear();
    public void Add(string path) => _paths.Add(path);
    public void Insert(int index, string path) => _paths.Insert(index, path);
    public void RemoveAt(int index) => _paths.RemoveAt(index);
    public bool Remove(string path) => _paths.Remove(path);
}

/// <summary>
/// Map of namespace relocations.
/// </summary>
public class SdfRelocatesMap : Dictionary<SdfPath, SdfPath>
{
}

/// <summary>
/// Proxy for editing relocates map.
/// </summary>
public class SdfRelocatesMapProxy
{
    private readonly SdfRelocatesMap _map = new();
    
    public int Count => _map.Count;
    public SdfPath this[SdfPath from] => _map.TryGetValue(from, out var to) ? to : SdfPath.EmptyPath();
    
    public void Clear() => _map.Clear();
    public void Add(SdfPath from, SdfPath to) => _map[from] = to;
    public bool Remove(SdfPath from) => _map.Remove(from);
    public bool ContainsKey(SdfPath from) => _map.ContainsKey(from);
}

/// <summary>
/// Batch namespace edit operation.
/// </summary>
public class SdfBatchNamespaceEdit
{
    public List<SdfNamespaceEdit> Edits { get; } = new();
    
    public void Add(SdfNamespaceEdit edit) => Edits.Add(edit);
}

/// <summary>
/// Individual namespace edit operation.
/// </summary>
public class SdfNamespaceEdit
{
    public SdfPath CurrentPath { get; set; } = SdfPath.EmptyPath();
    public SdfPath NewPath { get; set; } = SdfPath.EmptyPath();
    public int Index { get; set; } = -1;
}

/// <summary>
/// Result details for namespace edit operations.
/// </summary>
public static class SdfNamespaceEditDetail
{
    public enum Result
    {
        Okay,
        Unbatched,
        Error
    }
    
    public class EditResult
    {
        public Result Result { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SdfNamespaceEdit Edit { get; set; } = new();
    }
}