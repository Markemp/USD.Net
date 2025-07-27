namespace Pxr.Usd.Sdf;

using System.Collections.Generic;
using Pxr.Base.Tf;

using FileFormatArguments = Dictionary<string, string>;

// AIDEV-NOTE: Interface based on OpenUSD fileFormat.h - DO NOT MODIFY WITHOUT PERMISSION
/// <summary>
/// Interface for file format support in USD.
/// </summary>
/// <remarks>
/// The SdfFileFormat class is a base class for file format readers and writers.
/// File formats are responsible for translating scene description data between 
/// USD's in-memory representation and external file formats.
/// </remarks>
public interface ISdfFileFormat
{
    #region Properties
    
    /// <summary>
    /// Returns the format identifier token for this file format.
    /// </summary>
    /// <remarks>
    /// The format identifier is a token that uniquely identifies this file format,
    /// such as "sdf", "usda", or "usdc".
    /// </remarks>
    TfToken GetFormatId();
    
    /// <summary>
    /// Returns the target platform/variant for this file format.
    /// </summary>
    /// <remarks>
    /// The target token identifies the intended platform or variant for this format.
    /// For example, different platforms might have different binary representations.
    /// </remarks>
    TfToken GetTarget();
    
    /// <summary>
    /// Returns the file format version string.
    /// </summary>
    /// <remarks>
    /// The version string identifies the version of the file format specification
    /// that this implementation supports.
    /// </remarks>
    TfToken GetVersionString();
    
    /// <summary>
    /// Returns the file extensions supported by this format.
    /// </summary>
    /// <remarks>
    /// Returns a list of file extensions (without the leading dot) that this
    /// file format can handle, such as {"sdf", "menva"} for text formats.
    /// </remarks>
    IReadOnlyList<string> GetFileExtensions();
    
    /// <summary>
    /// Returns true if this is the primary file format for its extensions.
    /// </summary>
    /// <remarks>
    /// Multiple file formats may support the same extension. The primary format
    /// is preferred when multiple formats are available for an extension.
    /// </remarks>
    bool IsPrimaryFormatForExtensions();
    
    /// <summary>
    /// Returns the file format cookie string.
    /// </summary>
    /// <remarks>
    /// The cookie is typically placed at the beginning of files to identify
    /// the format, such as "#usda 1.0" for USD ASCII files.
    /// </remarks>
    string GetFileCookie();
    
    /// <summary>
    /// Returns the schema that this file format adheres to.
    /// </summary>
    /// <remarks>
    /// The schema defines the valid structure and types of data that can be
    /// stored in this file format.
    /// </remarks>
    ISdfSchemaBase GetSchema();
    
    #endregion
    
    #region File Recognition
    
    /// <summary>
    /// Returns true if this file format can read the specified file.
    /// </summary>
    /// <param name="file">The file path to check.</param>
    /// <returns>true if this format can read the file, false otherwise.</returns>
    /// <remarks>
    /// This method is used by the file format discovery system to determine
    /// which format should be used to read a particular file. Implementations
    /// typically check file extensions and/or examine file headers.
    /// </remarks>
    bool CanRead(string file);
    
    #endregion
    
    #region Reading
    
    /// <summary>
    /// Reads scene description data from the specified file into a layer.
    /// </summary>
    /// <param name="layer">The layer to populate with data from the file.</param>
    /// <param name="resolvedPath">The resolved file path to read from.</param>
    /// <param name="metadataOnly">If true, only read metadata, not full content.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// This is the primary method for reading file content into USD's in-memory
    /// representation. The metadataOnly flag can be used for performance when
    /// only layer metadata is needed.
    /// </remarks>
    bool Read(SdfLayer layer, string resolvedPath, bool metadataOnly);
    
    /// <summary>
    /// Reads scene description data from a string into a layer.
    /// </summary>
    /// <param name="layer">The layer to populate with data from the string.</param>
    /// <param name="str">The string containing the scene description data.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// This method allows reading from in-memory strings rather than files.
    /// Not all file formats may support this operation.
    /// </remarks>
    bool ReadFromString(SdfLayer layer, string str);
    
    #endregion
    
    #region Writing
    
    /// <summary>
    /// Writes the layer's data to the specified file path.
    /// </summary>
    /// <param name="layer">The layer to write.</param>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="comment">Optional comment to include in the file.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// This method writes the complete layer contents to a file. The comment
    /// parameter allows adding documentation to the output file.
    /// </remarks>
    bool WriteToFile(SdfLayer layer, string filePath, string comment = "", FileFormatArguments? args = null);
    
    /// <summary>
    /// Saves the layer to its current file location.
    /// </summary>
    /// <param name="layer">The layer to save.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="comment">Optional comment to include in the file.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// This method is similar to WriteToFile but may have different semantics
    /// for updating existing files versus creating new ones.
    /// </remarks>
    bool SaveToFile(SdfLayer layer, string filePath, string comment = "", FileFormatArguments? args = null);
    
    /// <summary>
    /// Writes the layer's data to a string.
    /// </summary>
    /// <param name="layer">The layer to write.</param>
    /// <param name="result">Output parameter to receive the serialized data.</param>
    /// <param name="comment">Optional comment to include.</param>
    /// <param name="args">Format-specific arguments.</param>
    /// <returns>true if successful and result is set, false otherwise.</returns>
    /// <remarks>
    /// This method serializes the layer to an in-memory string rather than a file.
    /// Not all file formats may support this operation.
    /// </remarks>
    bool WriteToString(SdfLayer layer, out string result, string comment = "", FileFormatArguments? args = null);
    
    #endregion
    
    #region Package Support
    
    /// <summary>
    /// Returns true if this file format represents a package (directory-based format).
    /// </summary>
    /// <remarks>
    /// Some file formats store data as packages (directories containing multiple files)
    /// rather than single files. This method identifies such formats.
    /// </remarks>
    bool IsPackage();
    
    /// <summary>
    /// Returns the path to the root layer within a package.
    /// </summary>
    /// <param name="resolvedPath">The resolved path to the package.</param>
    /// <returns>The path to the root layer file within the package.</returns>
    /// <remarks>
    /// For package-based formats, this method returns the path to the main
    /// layer file within the package directory structure.
    /// </remarks>
    string GetPackageRootLayerPath(string resolvedPath);
    
    #endregion
    
    #region Data Management
    
    /// <summary>
    /// Creates and initializes a new data container for this file format.
    /// </summary>
    /// <param name="args">Format-specific initialization arguments.</param>
    /// <returns>A new data container, or null if creation failed.</returns>
    /// <remarks>
    /// This method creates the underlying data storage that will hold the
    /// scene description data for layers using this file format.
    /// </remarks>
    ISdfAbstractData? InitData(FileFormatArguments? args = null);
    
    /// <summary>
    /// Returns the default file format arguments for this format.
    /// </summary>
    /// <returns>A dictionary of default format-specific arguments.</returns>
    /// <remarks>
    /// File formats may accept various arguments that control their behavior.
    /// This method returns the default values for those arguments.
    /// </remarks>
    FileFormatArguments GetDefaultFileFormatArguments();
    
    #endregion
    
    #region Dependencies
    
    /// <summary>
    /// Returns the external asset dependencies for a layer in this format.
    /// </summary>
    /// <param name="layer">The layer to analyze for dependencies.</param>
    /// <returns>A set of asset paths that this layer depends on.</returns>
    /// <remarks>
    /// This method identifies external assets (textures, references, etc.) that
    /// the layer depends on. This information is used for dependency tracking
    /// and asset management.
    /// </remarks>
    ISet<string> GetExternalAssetDependencies(SdfLayer layer);
    
    #endregion
    
    #region Static Discovery Methods
    
    /// <summary>
    /// Finds a file format by its format identifier.
    /// </summary>
    /// <param name="formatId">The format identifier token to search for.</param>
    /// <returns>The file format if found, null otherwise.</returns>
    /// <remarks>
    /// This static method searches the registered file formats for one with
    /// the specified identifier token.
    /// </remarks>
    static ISdfFileFormat? FindById(TfToken formatId) => SdfFileFormatRegistry.FindById(formatId);
    
    /// <summary>
    /// Finds a file format by file extension and optional target.
    /// </summary>
    /// <param name="path">The file path or extension to match.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>The best matching file format, or null if none found.</returns>
    /// <remarks>
    /// This method searches for file formats that can handle the specified
    /// file extension, optionally filtered by target platform.
    /// </remarks>
    static ISdfFileFormat? FindByExtension(string path, string target = "") => SdfFileFormatRegistry.FindByExtension(path, target);
    
    /// <summary>
    /// Returns all file extensions supported by registered file formats.
    /// </summary>
    /// <returns>A set of all supported file extensions.</returns>
    /// <remarks>
    /// This method returns the union of all file extensions supported by
    /// all registered file formats.
    /// </remarks>
    static ISet<string> FindAllFileFormatExtensions() => SdfFileFormatRegistry.FindAllFileFormatExtensions();
    
    /// <summary>
    /// Returns true if any registered format supports reading the given extension.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>true if reading is supported, false otherwise.</returns>
    static bool FormatSupportsReading(string extension, string target = "") => SdfFileFormatRegistry.FormatSupportsReading(extension, target);
    
    /// <summary>
    /// Returns true if any registered format supports writing the given extension.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>true if writing is supported, false otherwise.</returns>
    static bool FormatSupportsWriting(string extension, string target = "") => SdfFileFormatRegistry.FormatSupportsWriting(extension, target);
    
    /// <summary>
    /// Returns true if any registered format supports editing the given extension.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>true if editing is supported, false otherwise.</returns>
    static bool FormatSupportsEditing(string extension, string target = "") => SdfFileFormatRegistry.FormatSupportsEditing(extension, target);
    
    #endregion
}