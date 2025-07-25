namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pxr.Base.Tf;

/// <summary>
/// Registry for file format implementations in USD.
/// </summary>
/// <remarks>
/// The SdfFileFormatRegistry manages the registration and discovery of file formats.
/// It provides static methods for finding appropriate file formats based on file
/// extensions, format identifiers, and other criteria.
/// </remarks>
public static class SdfFileFormatRegistry
{
    #region Private Fields
    
    private static readonly ConcurrentDictionary<TfToken, ISdfFileFormat> _formatsByToken = new();
    private static readonly ConcurrentDictionary<string, List<ISdfFileFormat>> _formatsByExtension = new();
    private static readonly object _registrationLock = new object();
    
    #endregion
    
    #region Registration
    
    /// <summary>
    /// Registers a file format with the registry.
    /// </summary>
    /// <param name="format">The file format to register.</param>
    /// <remarks>
    /// This method is typically called automatically when file format instances
    /// are created. It registers the format by its identifier token and supported
    /// file extensions.
    /// </remarks>
    public static void RegisterFormat(ISdfFileFormat format)
    {
        if (format is null)
            throw new ArgumentNullException(nameof(format));
            
        lock (_registrationLock)
        {
            // Register by format token
            var formatId = format.GetFormatId();
            _formatsByToken.TryAdd(formatId, format);
            
            // Register by extensions
            foreach (var extension in format.GetFileExtensions())
            {
                if (!_formatsByExtension.ContainsKey(extension))
                    _formatsByExtension[extension] = new List<ISdfFileFormat>();
                
                var formatList = _formatsByExtension[extension];
                if (!formatList.Contains(format))
                {
                    // Primary formats go first
                    if (format.IsPrimaryFormatForExtensions())
                        formatList.Insert(0, format);
                    else
                        formatList.Add(format);
                }
            }
        }
    }
    
    /// <summary>
    /// Unregisters a file format from the registry.
    /// </summary>
    /// <param name="format">The file format to unregister.</param>
    public static void UnregisterFormat(ISdfFileFormat format)
    {
        if (format is null)
            return;
            
        lock (_registrationLock)
        {
            // Remove from token registry
            var formatId = format.GetFormatId();
            _formatsByToken.TryRemove(formatId, out _);
            
            // Remove from extension registry
            foreach (var extension in format.GetFileExtensions())
            {
                if (_formatsByExtension.TryGetValue(extension, out var formatList))
                {
                    formatList.Remove(format);
                    if (formatList.Count == 0)
                        _formatsByExtension.TryRemove(extension, out _);
                }
            }
        }
    }
    
    #endregion
    
    #region Discovery by ID
    
    /// <summary>
    /// Finds a file format by its format identifier.
    /// </summary>
    /// <param name="formatId">The format identifier token to search for.</param>
    /// <returns>The file format if found, null otherwise.</returns>
    public static ISdfFileFormat? FindById(TfToken formatId)
        => _formatsByToken.TryGetValue(formatId, out var format) ? format : null;
    
    /// <summary>
    /// Returns all registered format identifiers.
    /// </summary>
    /// <returns>A collection of all registered format identifier tokens.</returns>
    public static IReadOnlyCollection<TfToken> GetAllFormatIds()
        => _formatsByToken.Keys.ToList().AsReadOnly();
    
    #endregion
    
    #region Discovery by Extension
    
    /// <summary>
    /// Finds a file format by file extension and optional target.
    /// </summary>
    /// <param name="path">The file path or extension to match.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>The best matching file format, or null if none found.</returns>
    public static ISdfFileFormat? FindByExtension(string path, string target = "")
    {
        if (string.IsNullOrEmpty(path))
            return null;
            
        var extension = ExtractExtension(path);
        if (string.IsNullOrEmpty(extension))
            return null;
            
        if (!_formatsByExtension.TryGetValue(extension, out var formats) || formats.Count == 0)
            return null;
            
        // If target is specified, try to find a format with matching target
        if (!string.IsNullOrEmpty(target))
        {
            var targetToken = new TfToken(target);
            var targetFormat = formats.FirstOrDefault(f => f.GetTarget().Equals(targetToken));
            if (targetFormat is not null)
                return targetFormat;
        }
        
        // Return the first (primary) format for the extension
        return formats[0];
    }
    
    /// <summary>
    /// Finds all file formats that support the given extension.
    /// </summary>
    /// <param name="extension">The file extension to search for.</param>
    /// <returns>A list of file formats that support the extension.</returns>
    public static IReadOnlyList<ISdfFileFormat> FindAllByExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return Array.Empty<ISdfFileFormat>();
            
        extension = NormalizeExtension(extension);
        
        if (_formatsByExtension.TryGetValue(extension, out var formats))
            return formats.AsReadOnly();
        
        return Array.Empty<ISdfFileFormat>();
    }
    
    /// <summary>
    /// Returns all file extensions supported by registered file formats.
    /// </summary>
    /// <returns>A set of all supported file extensions.</returns>
    public static ISet<string> FindAllFileFormatExtensions()
        => new HashSet<string>(_formatsByExtension.Keys);
    
    #endregion
    
    #region Support Queries
    
    /// <summary>
    /// Returns true if any registered format supports reading the given extension.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>true if reading is supported, false otherwise.</returns>
    public static bool FormatSupportsReading(string extension, string target = "")
    {
        var format = FindByExtension(extension, target);
        // For now, assume all registered formats support reading
        // (In full implementation, would check format capabilities)
        return format is not null;
    }
    
    /// <summary>
    /// Returns true if any registered format supports writing the given extension.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>true if writing is supported, false otherwise.</returns>
    public static bool FormatSupportsWriting(string extension, string target = "")
    {
        var format = FindByExtension(extension, target);
        // For now, assume all registered formats support writing
        // (In full implementation, would check format capabilities)
        return format is not null;
    }
    
    /// <summary>
    /// Returns true if any registered format supports editing the given extension.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>true if editing is supported, false otherwise.</returns>
    public static bool FormatSupportsEditing(string extension, string target = "")
    {
        var format = FindByExtension(extension, target);
        // For now, assume all registered formats support editing
        // (In full implementation, would check format capabilities)
        return format is not null;
    }
    
    #endregion
    
    #region File Format Discovery by File Content
    
    /// <summary>
    /// Finds the best file format for reading a specific file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="target">Optional target platform/variant.</param>
    /// <returns>The best file format for reading the file, or null if none found.</returns>
    /// <remarks>
    /// This method first tries to find formats by extension, then tests each
    /// format's CanRead method to find the best match.
    /// </remarks>
    public static ISdfFileFormat? FindFormatForFile(string filePath, string target = "")
    {
        if (string.IsNullOrEmpty(filePath))
            return null;
            
        var extension = ExtractExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            return null;
            
        var candidates = FindAllByExtension(extension);
        if (candidates.Count == 0)
            return null;
            
        // Filter by target if specified
        if (!string.IsNullOrEmpty(target))
        {
            var targetToken = new TfToken(target);
            candidates = candidates.Where(f => f.GetTarget().Equals(targetToken)).ToList();
        }
        
        // Test each candidate to see if it can read the file
        foreach (var format in candidates)
        {
            try
            {
                if (format.CanRead(filePath))
                    return format;
            }
            catch
            {
                // Continue trying other formats if one fails
                continue;
            }
        }
        
        // If no format can read the file, return the first candidate
        return candidates.FirstOrDefault();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Extracts the file extension from a path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The normalized file extension, or empty string if none.</returns>
    private static string ExtractExtension(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
            
        var extension = Path.GetExtension(path);
        return NormalizeExtension(extension);
    }
    
    /// <summary>
    /// Normalizes a file extension by removing the leading dot and converting to lowercase.
    /// </summary>
    /// <param name="extension">The file extension to normalize.</param>
    /// <returns>The normalized extension.</returns>
    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return string.Empty;
            
        if (extension.StartsWith("."))
            extension = extension.Substring(1);
            
        return extension.ToLowerInvariant();
    }
    
    #endregion
    
    #region Diagnostics
    
    /// <summary>
    /// Returns information about all registered file formats for debugging.
    /// </summary>
    /// <returns>A dictionary mapping format IDs to format information.</returns>
    public static Dictionary<string, object> GetRegistryInfo()
    {
        var info = new Dictionary<string, object>();
        
        foreach (var kvp in _formatsByToken)
        {
            var format = kvp.Value;
            info[kvp.Key.GetText()] = new
            {
                FormatId = format.GetFormatId().GetText(),
                Target = format.GetTarget().GetText(),
                Version = format.GetVersionString().GetText(),
                Extensions = format.GetFileExtensions().ToList(),
                IsPrimary = format.IsPrimaryFormatForExtensions(),
                Cookie = format.GetFileCookie(),
                IsPackage = format.IsPackage()
            };
        }
        
        return info;
    }
    
    #endregion
}