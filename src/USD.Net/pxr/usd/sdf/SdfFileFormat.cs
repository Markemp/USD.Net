namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pxr.Base.Tf;

using FileFormatArguments = Dictionary<string, string>;

/// <summary>
/// Abstract base class for file format implementations in USD.
/// </summary>
/// <remarks>
/// SdfFileFormat is the base class for all file format readers and writers in USD.
/// It provides the interface between USD's in-memory representation and external
/// file formats. Concrete implementations handle specific formats like .usda, .usdc, etc.
/// </remarks>
public abstract class SdfFileFormat : ISdfFileFormat
{
    #region Protected Fields
    
    protected readonly ISdfSchemaBase _schema;
    protected readonly TfToken _formatId;
    protected readonly TfToken _target;
    protected readonly string _cookie;
    protected readonly TfToken _versionString;
    protected readonly List<string> _extensions;
    protected readonly bool _isPrimaryFormat;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Protected constructor for derived file format classes.
    /// </summary>
    /// <param name="formatId">Unique identifier for this format (e.g., "sdf", "usda").</param>
    /// <param name="versionString">Version string for this format.</param>
    /// <param name="target">Target platform/variant identifier.</param>
    /// <param name="cookie">File magic string/cookie for format identification.</param>
    /// <param name="extensions">Supported file extensions (without leading dots).</param>
    /// <param name="schema">Schema that this format adheres to.</param>
    /// <param name="isPrimaryFormat">Whether this is the primary format for its extensions.</param>
    protected SdfFileFormat(
        TfToken formatId,
        TfToken versionString,
        TfToken target,
        string cookie,
        IEnumerable<string> extensions,
        ISdfSchemaBase? schema = null,
        bool isPrimaryFormat = true)
    {
        _formatId = formatId;
        _versionString = versionString;
        _target = target;
        _cookie = cookie ?? string.Empty;
        _extensions = extensions?.ToList() ?? new List<string>();
        _schema = schema ?? SdfSchema.Instance;
        _isPrimaryFormat = isPrimaryFormat;
        
        // Register this format
        SdfFileFormatRegistry.RegisterFormat(this);
    }
    
    #endregion
    
    #region ISdfFileFormat Implementation - Properties
    
    /// <summary>
    /// Returns the format identifier token for this file format.
    /// </summary>
    public virtual TfToken GetFormatId() => _formatId;
    
    /// <summary>
    /// Returns the target platform/variant for this file format.
    /// </summary>
    public virtual TfToken GetTarget() => _target;
    
    /// <summary>
    /// Returns the file format version string.
    /// </summary>
    public virtual TfToken GetVersionString() => _versionString;
    
    /// <summary>
    /// Returns the file extensions supported by this format.
    /// </summary>
    public virtual IReadOnlyList<string> GetFileExtensions() => _extensions.AsReadOnly();
    
    /// <summary>
    /// Returns true if this is the primary file format for its extensions.
    /// </summary>
    public virtual bool IsPrimaryFormatForExtensions() => _isPrimaryFormat;
    
    /// <summary>
    /// Returns the file format cookie string.
    /// </summary>
    public virtual string GetFileCookie() => _cookie;
    
    /// <summary>
    /// Returns the schema that this file format adheres to.
    /// </summary>
    public virtual ISdfSchemaBase GetSchema() => _schema;
    
    #endregion
    
    #region ISdfFileFormat Implementation - Abstract Methods
    
    /// <summary>
    /// Returns true if this file format can read the specified file.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="file">The file path to check.</param>
    /// <returns>true if this format can read the file, false otherwise.</returns>
    public abstract bool CanRead(string file);
    
    /// <summary>
    /// Reads scene description data from the specified file into a layer.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="layer">The layer to populate with data from the file.</param>
    /// <param name="resolvedPath">The resolved file path to read from.</param>
    /// <param name="metadataOnly">If true, only read metadata, not full content.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public abstract bool Read(SdfLayer layer, string resolvedPath, bool metadataOnly);
    
    #endregion
    
    #region ISdfFileFormat Implementation - Virtual Methods
    
    /// <summary>
    /// Reads scene description data from a string into a layer.
    /// Default implementation returns false (not supported).
    /// </summary>
    /// <param name="layer">The layer to populate with data from the string.</param>
    /// <param name="str">The string containing the scene description data.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public virtual bool ReadFromString(SdfLayer layer, string str)
    {
        // Default implementation - not supported
        return false;
    }
    
    /// <summary>
    /// Writes the layer's data to the specified file path.
    /// Default implementation returns false (not supported).
    /// </summary>
    /// <param name="layer">The layer to write.</param>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="comment">Optional comment to include in the file.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public virtual bool WriteToFile(SdfLayer layer, string filePath, string comment = "", FileFormatArguments? args = null)
    {
        // Default implementation - not supported
        return false;
    }
    
    /// <summary>
    /// Saves the layer to its current file location.
    /// Default implementation delegates to WriteToFile.
    /// </summary>
    /// <param name="layer">The layer to save.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="comment">Optional comment to include in the file.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>true if successful, false otherwise.</returns>
    public virtual bool SaveToFile(SdfLayer layer, string filePath, string comment = "", FileFormatArguments? args = null)
    {
        return WriteToFile(layer, filePath, comment, args);
    }
    
    /// <summary>
    /// Writes the layer's data to a string.
    /// Default implementation returns false (not supported).
    /// </summary>
    /// <param name="layer">The layer to write.</param>
    /// <param name="result">Output parameter to receive the serialized data.</param>
    /// <param name="comment">Optional comment to include.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>true if successful and result is set, false otherwise.</returns>
    public virtual bool WriteToString(SdfLayer layer, out string result, string comment = "", FileFormatArguments? args = null)
    {
        result = string.Empty;
        return false;
    }
    
    /// <summary>
    /// Returns true if this file format represents a package (directory-based format).
    /// Default implementation returns false.
    /// </summary>
    public virtual bool IsPackage()
    {
        return false;
    }
    
    /// <summary>
    /// Returns the path to the root layer within a package.
    /// Default implementation returns the input path unchanged.
    /// </summary>
    /// <param name="resolvedPath">The resolved path to the package.</param>
    /// <returns>The path to the root layer file within the package.</returns>
    public virtual string GetPackageRootLayerPath(string resolvedPath)
    {
        return resolvedPath;
    }
    
    /// <summary>
    /// Creates and initializes a new data container for this file format.
    /// Default implementation creates a basic SdfData instance.
    /// </summary>
    /// <param name="args">Format-specific initialization arguments.</param>
    /// <returns>A new data container, or null if creation failed.</returns>
    public virtual ISdfAbstractData? InitData(FileFormatArguments? args = null)
    {
        return new SdfData();
    }
    
    /// <summary>
    /// Returns the default file format arguments for this format.
    /// Default implementation returns an empty dictionary.
    /// </summary>
    /// <returns>A dictionary of default format-specific arguments.</returns>
    public virtual FileFormatArguments GetDefaultFileFormatArguments()
    {
        return new FileFormatArguments();
    }
    
    /// <summary>
    /// Returns the external asset dependencies for a layer in this format.
    /// Default implementation returns an empty set.
    /// </summary>
    /// <param name="layer">The layer to analyze for dependencies.</param>
    /// <returns>A set of asset paths that this layer depends on.</returns>
    public virtual ISet<string> GetExternalAssetDependencies(SdfLayer layer)
    {
        return new HashSet<string>();
    }
    
    #endregion
    
    #region Protected Virtual Methods
    
    /// <summary>
    /// Creates a new layer instance for this file format.
    /// Can be overridden by derived classes to provide custom layer types.
    /// </summary>
    /// <param name="identifier">The layer identifier.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>A new layer instance, or null if creation failed.</returns>
    protected virtual SdfLayer? InstantiateNewLayer(string identifier, FileFormatArguments? args = null)
    {
        var data = InitData(args);
        if (data == null)
            return null;
            
        return new SdfLayer(identifier, GetSchema());
    }
    
    /// <summary>
    /// Returns true if anonymous layer reload should be skipped for this format.
    /// Default implementation returns false.
    /// </summary>
    /// <returns>true if anonymous reload should be skipped, false otherwise.</returns>
    protected virtual bool ShouldSkipAnonymousReload()
    {
        return false;
    }
    
    /// <summary>
    /// Returns true if this format should read anonymous layers.
    /// Default implementation returns true.
    /// </summary>
    /// <returns>true if anonymous layers should be read, false otherwise.</returns>
    protected virtual bool ShouldReadAnonymousLayers()
    {
        return true;
    }
    
    /// <summary>
    /// Creates and initializes a detached data container for this file format.
    /// Default implementation delegates to InitData.
    /// </summary>
    /// <param name="args">Format-specific initialization arguments.</param>
    /// <returns>A new detached data container, or null if creation failed.</returns>
    protected virtual ISdfAbstractData? InitDetachedData(FileFormatArguments? args = null)
    {
        return InitData(args);
    }
    
    /// <summary>
    /// Reads data into a detached layer (not in the layer registry).
    /// Default implementation delegates to the regular Read method.
    /// </summary>
    /// <param name="layer">The layer to populate with data.</param>
    /// <param name="resolvedPath">The resolved file path to read from.</param>
    /// <param name="metadataOnly">If true, only read metadata, not full content.</param>
    /// <returns>true if successful, false otherwise.</returns>
    protected virtual bool ReadDetached(SdfLayer layer, string resolvedPath, bool metadataOnly)
    {
        return Read(layer, resolvedPath, metadataOnly);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Returns true if the given file path has an extension supported by this format.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>true if the extension is supported, false otherwise.</returns>
    protected bool IsSupportedExtension(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;
            
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            return false;
            
        // Remove leading dot
        extension = extension.Substring(1);
        
        return _extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Checks if a file exists and can be read.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>true if the file exists and is readable, false otherwise.</returns>
    protected static bool IsFileReadable(string filePath)
    {
        try
        {
            return File.Exists(filePath) && ((File.GetAttributes(filePath) & FileAttributes.Directory) == 0);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Reads the first few bytes of a file to check for a format cookie.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="maxBytes">Maximum number of bytes to read (default 1024).</param>
    /// <returns>The file header as a string, or empty string if reading failed.</returns>
    protected static string ReadFileHeader(string filePath, int maxBytes = 1024)
    {
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[Math.Min(maxBytes, fileStream.Length)];
            var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        catch
        {
            return string.Empty;
        }
    }
    
    #endregion
}