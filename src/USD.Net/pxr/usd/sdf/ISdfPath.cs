using Pxr.Base.Tf;

namespace Pxr.Usd.Sdf;

// AIDEV-NOTE: DO NOT MODIFY WITHOUT PERMISSION.  Interface extracted from OpenUSD path.h
// - comprehensive path API for C# implementation

/// <summary>
/// Interface for a path value used to locate objects in layers or scenegraphs.
/// 
/// SdfPath is used in several ways:
/// - As a storage key for addressing and accessing values held in a SdfLayer
/// - As a namespace identity for scenegraph objects  
/// - As a way to refer to other scenegraph objects through relative paths
/// 
/// The paths represented by an SdfPath class may be either relative or
/// absolute. Relative paths are relative to the prim object that contains them
/// (that is, if an SdfRelationshipSpec target is relative, it is relative to
/// the SdfPrimSpec object that owns the SdfRelationshipSpec object).
/// 
/// SdfPath objects can be readily created from and converted back to strings,
/// but as SdfPath objects, they have behaviors that make it easy and efficient
/// to work with them. The SdfPath class provides a full range of methods for
/// manipulating scene paths by appending a namespace child, appending a
/// relationship target, getting the parent path, and so on. Since the SdfPath
/// class uses a node-based representation internally, you should use the editing
/// functions rather than converting to and from strings if possible.
/// 
/// Path Syntax:
/// Like a filesystem path, an SdfPath is conceptually just a sequence of
/// path components. Unlike a filesystem path, each component has a type,
/// and the type is indicated by the syntax.
/// 
/// Two separators are used between parts of a path. A slash ("/") following an
/// identifier is used to introduce a namespace child. A period (".") following
/// an identifier is used to introduce a property. A property may also have
/// several non-sequential colons (':') in its name to provide a rudimentary
/// namespace within properties but may not end or begin with a colon.
/// 
/// A leading slash in the string representation of an SdfPath object indicates
/// an absolute path. Two adjacent periods indicate the parent namespace.
/// 
/// Brackets ("[" and "]") are used to indicate relationship target paths for
/// relational attributes.
/// 
/// Examples:
/// - "/Foo" is an absolute path specifying the root prim Foo.
/// - "/Foo/Bar" is an absolute path specifying namespace child Bar of root prim Foo.
/// - "/Foo/Bar.baz" is an absolute path specifying property baz of namespace child Bar of root prim Foo.
/// - "Foo" is a relative path specifying namespace child Foo of the current prim.
/// - ".foo" is a relative path specifying the property foo of the current prim.
/// - "/Foo.bar[/Foo.baz].attrib" is a relational attribute path.
/// 
/// Thread-Safety:
/// SdfPath is strongly thread-safe, in the sense that zero additional
/// synchronization is required between threads creating or using SdfPath
/// values. Just like TfToken, SdfPath values are immutable. Internally,
/// SdfPath uses a global prefix tree to efficiently share representations
/// of paths, and provide fast equality/hashing operations, but
/// modifications to this table are internally synchronized.
/// </summary>
public interface ISdfPath : IEquatable<ISdfPath>, IComparable<ISdfPath>
{
    /// <summary>
    /// Returns the number of path elements in this path.
    /// </summary>
    int GetPathElementCount();

    /// <summary>
    /// Returns whether the path is absolute.
    /// </summary>
    bool IsAbsolutePath();

    /// <summary>
    /// Return true if this path is the AbsoluteRootPath().
    /// </summary>
    bool IsAbsoluteRootPath();

    /// <summary>
    /// Returns whether the path identifies a prim.
    /// </summary>
    bool IsPrimPath();

    /// <summary>
    /// Returns whether the path identifies a prim or the absolute root.
    /// </summary>
    bool IsAbsoluteRootOrPrimPath();

    /// <summary>
    /// Returns whether the path identifies a root prim.
    /// The path must be absolute and have a single element (for example "/foo").
    /// </summary>
    bool IsRootPrimPath();

    /// <summary>
    /// Returns whether the path identifies a property.
    /// A relational attribute is considered to be a property, so this
    /// method will return true for relational attributes as well
    /// as properties of prims.
    /// </summary>
    bool IsPropertyPath();

    /// <summary>
    /// Returns whether the path identifies a prim's property.
    /// A relational attribute is not a prim property.
    /// </summary>
    bool IsPrimPropertyPath();

    /// <summary>
    /// Returns whether the path identifies a namespaced property.
    /// A namespaced property has colon embedded in its name.
    /// </summary>
    bool IsNamespacedPropertyPath();

    /// <summary>
    /// Returns whether the path identifies a variant selection for a prim.
    /// </summary>
    bool IsPrimVariantSelectionPath();

    /// <summary>
    /// Return true if this path is a prim path or is a prim variant selection path.
    /// </summary>
    bool IsPrimOrPrimVariantSelectionPath();

    /// <summary>
    /// Returns whether the path or any of its parent paths identifies
    /// a variant selection for a prim.
    /// </summary>
    bool ContainsPrimVariantSelection();

    /// <summary>
    /// Return true if this path contains any property elements, false otherwise.
    /// A false return indicates a prim-like path, specifically a root path,
    /// a prim path, or a prim variant selection path. A true return indicates
    /// a property-like path: a prim property path, a target path, a relational
    /// attribute path, etc.
    /// </summary>
    bool ContainsPropertyElements();

    /// <summary>
    /// Return true if this path is or has a prefix that's a target path or a mapper path.
    /// </summary>
    bool ContainsTargetPath();

    /// <summary>
    /// Returns whether the path identifies a relational attribute.
    /// If this is true, IsPropertyPath() will also be true.
    /// </summary>
    bool IsRelationalAttributePath();

    /// <summary>
    /// Returns whether the path identifies a relationship or connection target.
    /// </summary>
    bool IsTargetPath();

    /// <summary>
    /// Returns whether the path identifies a connection mapper.
    /// </summary>
    bool IsMapperPath();

    /// <summary>
    /// Returns whether the path identifies a connection mapper arg.
    /// </summary>
    bool IsMapperArgPath();

    /// <summary>
    /// Returns whether the path identifies a connection expression.
    /// </summary>
    bool IsExpressionPath();

    /// <summary>
    /// Returns true if this is the empty path (SdfPath::EmptyPath()).
    /// </summary>
    bool IsEmpty();

    /// <summary>
    /// Return the string representation of this path as a TfToken.
    /// This function is recommended only for human-readable or diagnostic
    /// output. Use the SdfPath API to manipulate paths. It is less
    /// error-prone and has better performance.
    /// </summary>
    TfToken GetAsToken();

    /// <summary>
    /// Return the string representation of this path as a TfToken lvalue.
    /// This function returns a persistent lvalue. If an rvalue will suffice,
    /// call GetAsToken() instead. That avoids populating internal data
    /// structures to hold the persistent token.
    /// This function is recommended only for human-readable or diagnostic
    /// output. Use the SdfPath API to manipulate paths. It is less
    /// error-prone and has better performance.
    /// </summary>
    TfToken GetToken();

    /// <summary>
    /// Return the string representation of this path as a string.
    /// This function is recommended only for human-readable or diagnostic
    /// output. Use the SdfPath API to manipulate paths. It is less
    /// error-prone and has better performance.
    /// </summary>
    string GetAsString();

    /// <summary>
    /// Return the string representation of this path as a string.
    /// This function returns a persistent lvalue. If an rvalue will suffice,
    /// call GetAsString() instead. That avoids populating internal data
    /// structures to hold the persistent string.
    /// This function is recommended only for human-readable or diagnostic
    /// output. Use the SdfPath API to manipulate paths. It is less
    /// error-prone and has better performance.
    /// </summary>
    string GetString();

    /// <summary>
    /// Returns the string representation of this path as a c string.
    /// This function returns a pointer to a persistent c string. If a
    /// temporary c string will suffice, call GetAsString() instead.
    /// That avoids populating internal data structures to hold the persistent string.
    /// This function is recommended only for human-readable or diagnostic
    /// output. Use the SdfPath API to manipulate paths. It is less
    /// error-prone and has better performance.
    /// </summary>
    string GetText();

    /// <summary>
    /// Returns the prefix paths of this path.
    /// Prefixes are returned in order of shortest to longest. The path
    /// itself is returned as the last prefix.
    /// Note that if the prefix order does not need to be from shortest to
    /// longest, it is more efficient to use GetAncestorsRange, which
    /// produces an equivalent set of paths, ordered from longest to shortest.
    /// </summary>
    IReadOnlyList<ISdfPath> GetPrefixes();

    /// <summary>
    /// Return up to numPrefixes prefix paths of this path.
    /// Prefixes are returned in order of shortest to longest. The path itself
    /// is returned as the last prefix. Note that if the prefix order does not
    /// need to be from shortest to longest, it is more efficient to use
    /// GetAncestorsRange, which produces an equivalent set of paths, ordered
    /// from longest to shortest. If numPrefixes is 0 or greater than the
    /// number of this path's prefixes, fill all prefixes.
    /// </summary>
    IReadOnlyList<ISdfPath> GetPrefixes(int numPrefixes);

    /// <summary>
    /// Fills prefixes with prefixes of this path.
    /// This avoids copy constructing the return value.
    /// Prefixes are returned in order of shortest to longest. The path
    /// itself is returned as the last prefix.
    /// Note that if the prefix order does not need to be from shortest to
    /// longest, it is more efficient to use GetAncestorsRange(), which
    /// produces an equivalent set of paths, ordered from longest to shortest.
    /// </summary>
    void GetPrefixes(IList<ISdfPath> prefixes);

    /// <summary>
    /// Fill prefixes with up to numPrefixes prefixes of this path.
    /// Prefixes are filled in order of shortest to longest. The path itself is
    /// included as the last prefix. Note that if the prefix order does not
    /// need to be from shortest to longest, it can be more efficient to use
    /// GetAncestorsRange(), which produces an equivalent set of paths, ordered
    /// from longest to shortest. If numPrefixes is 0 or greater than the
    /// number of this path's prefixes, fill all prefixes.
    /// </summary>
    void GetPrefixes(IList<ISdfPath> prefixes, int numPrefixes);

    /// <summary>
    /// Return a range for iterating over the ancestors of this path.
    /// The range provides iteration over the prefixes of a path, ordered
    /// from longest to shortest (the opposite of the order of the prefixes
    /// returned by GetPrefixes).
    /// </summary>
    IEnumerable<ISdfPath> GetAncestorsRange();

    /// <summary>
    /// Returns the name of the prim, property or relational attribute identified by the path.
    /// Returns EmptyPath if this path is a target or mapper path.
    /// - Returns "" for EmptyPath.
    /// - Returns "." for ReflexiveRelativePath.
    /// - Returns ".." for a path ending in ParentPathElement.
    /// </summary>
    string GetName();

    /// <summary>
    /// Returns the name of the prim, property or relational attribute identified by the path, as a token.
    /// </summary>
    TfToken GetNameToken();

    /// <summary>
    /// Returns an ascii representation of the "terminal" element of this path,
    /// which can be used to reconstruct the path using AppendElementString() on its parent.
    /// EmptyPath(), AbsoluteRootPath(), and ReflexiveRelativePath() are not considered
    /// elements (one of the defining properties of elements is that they have a parent),
    /// so GetElementString() will return the empty string for these paths.
    /// Unlike GetName() and GetTargetPath(), which provide you "some" information about
    /// the terminal element, this provides a complete representation of the element,
    /// for all element types.
    /// Also note that whereas GetName(), GetNameToken(), GetText(), GetString(), and
    /// GetTargetPath() return cached results, GetElementString() always performs some
    /// amount of string manipulation, which you should keep in mind if performance is a concern.
    /// </summary>
    string GetElementString();

    /// <summary>
    /// Like GetElementString() but return the value as a TfToken.
    /// </summary>
    TfToken GetElementToken();

    /// <summary>
    /// Return a copy of this path with its final component changed to newName.
    /// This path must be a prim or property path.
    /// This method is shorthand for path.GetParentPath().AppendChild(newName) for prim paths,
    /// path.GetParentPath().AppendProperty(newName) for prim property paths, and
    /// path.GetParentPath().AppendRelationalAttribute(newName) for relational attribute paths.
    /// Note that only the final path component is ever changed. If the name of
    /// the final path component appears elsewhere in the path, it will not be modified.
    /// Examples:
    /// ReplaceName('/chars/MeridaGroup', 'AngusGroup') -> '/chars/AngusGroup'
    /// ReplaceName('/Merida.tx', 'ty') -> '/Merida.ty'
    /// ReplaceName('/Merida.tx[targ].tx', 'ty') -> '/Merida.tx[targ].ty'
    /// </summary>
    ISdfPath ReplaceName(TfToken newName);

    /// <summary>
    /// Returns the relational attribute or mapper target path for this path.
    /// Returns EmptyPath if this is not a target, relational attribute or mapper path.
    /// Note that it is possible for a path to have multiple "target" paths.
    /// For example a path that identifies a connection target for a relational attribute
    /// includes the target of the connection as well as the target of the relational attribute.
    /// In these cases, the "deepest" or right-most target path will be returned
    /// (the connection target in this example).
    /// </summary>
    ISdfPath GetTargetPath();

    /// <summary>
    /// Returns all the relationship target or connection target paths contained in this path,
    /// and recursively all the target paths contained in those target paths in reverse depth-first order.
    /// For example, given the path: '/A/B.a[/C/D.a[/E/F.a]].a[/A/B.a[/C/D.a]]'
    /// this method produces: '/A/B.a[/C/D.a]', '/C/D.a', '/C/D.a[/E/F.a]', '/E/F.a'
    /// </summary>
    void GetAllTargetPathsRecursively(IList<ISdfPath> result);

    /// <summary>
    /// Returns the variant selection for this path, if this is a variant selection path.
    /// Returns a pair of empty strings if this path is not a variant selection path.
    /// </summary>
    (string variantSet, string variant) GetVariantSelection();

    /// <summary>
    /// Return true if both this path and prefix are not the empty path and this path
    /// has prefix as a prefix. Return false otherwise.
    /// </summary>
    bool HasPrefix(ISdfPath prefix);

    /// <summary>
    /// Return the path that identifies this path's namespace parent.
    /// For a prim path (like '/foo/bar'), return the prim's parent's path ('/foo').
    /// For a prim property path (like '/foo/bar.property'), return the prim's path ('/foo/bar').
    /// For a target path (like '/foo/bar.property[/target]') return the property path ('/foo/bar.property').
    /// For a mapper path (like '/foo/bar.property.mapper[/target]') return the property path ('/foo/bar.property).
    /// For a relational attribute path (like '/foo/bar.property[/target].relAttr') return the
    /// relationship target's path ('/foo/bar.property[/target]').
    /// For a prim variant selection path (like '/foo/bar{var=sel}') return the prim path ('/foo/bar').
    /// For a root prim path (like '/rootPrim'), return AbsoluteRootPath() ('/').
    /// For a single element relative prim path (like 'relativePrim'), return ReflexiveRelativePath() ('.').
    /// For ReflexiveRelativePath(), return the relative parent path ('..').
    /// Note that the parent path of a relative parent path ('..') is a relative grandparent path ('../..').
    /// Use caution writing loops that walk to parent paths since relative paths have infinitely many ancestors.
    /// To more safely traverse ancestor paths, consider iterating over an SdfPathAncestorsRange instead,
    /// as returned by GetAncestorsRange().
    /// </summary>
    ISdfPath GetParentPath();

    /// <summary>
    /// Creates a path by stripping all relational attributes, targets, properties,
    /// and variant selections from the leafmost prim path, leaving the nearest path
    /// for which IsPrimPath() returns true.
    /// See GetPrimOrPrimVariantSelectionPath also.
    /// If the path is already a prim path, the same path is returned.
    /// </summary>
    ISdfPath GetPrimPath();

    /// <summary>
    /// Creates a path by stripping all relational attributes, targets, and properties,
    /// leaving the nearest path for which IsPrimOrPrimVariantSelectionPath() returns true.
    /// See GetPrimPath also.
    /// If the path is already a prim or a prim variant selection path, the same path is returned.
    /// </summary>
    ISdfPath GetPrimOrPrimVariantSelectionPath();

    /// <summary>
    /// Creates a path by stripping all properties and relational attributes from this path,
    /// leaving the path to the containing prim.
    /// If the path is already a prim or absolute root path, the same path is returned.
    /// </summary>
    ISdfPath GetAbsoluteRootOrPrimPath();

    /// <summary>
    /// Create a path by stripping all variant selections from all components of this path,
    /// leaving a path with no embedded variant selections.
    /// </summary>
    ISdfPath StripAllVariantSelections();

    /// <summary>
    /// Creates a path by appending a given relative path to this path.
    /// If the newSuffix is a prim path, then this path must be a prim path or a root path.
    /// If the newSuffix is a prim property path, then this path must be a prim path or the ReflexiveRelativePath.
    /// </summary>
    ISdfPath AppendPath(ISdfPath newSuffix);

    /// <summary>
    /// Creates a path by appending an element for childName to this path.
    /// This path must be a prim path, the AbsoluteRootPath or the ReflexiveRelativePath.
    /// </summary>
    ISdfPath AppendChild(TfToken childName);

    /// <summary>
    /// Creates a path by appending an element for propName to this path.
    /// This path must be a prim path or the ReflexiveRelativePath.
    /// </summary>
    ISdfPath AppendProperty(TfToken propName);

    /// <summary>
    /// Creates a path by appending an element for variantSet and variant to this path.
    /// This path must be a prim path.
    /// </summary>
    ISdfPath AppendVariantSelection(string variantSet, string variant);

    /// <summary>
    /// Creates a path by appending an element for targetPath.
    /// This path must be a prim property or relational attribute path.
    /// </summary>
    ISdfPath AppendTarget(ISdfPath targetPath);

    /// <summary>
    /// Creates a path by appending an element for attrName to this path.
    /// This path must be a target path.
    /// </summary>
    ISdfPath AppendRelationalAttribute(TfToken attrName);

    /// <summary>
    /// Replaces the relational attribute's target path.
    /// The path must be a relational attribute path.
    /// </summary>
    ISdfPath ReplaceTargetPath(ISdfPath newTargetPath);

    /// <summary>
    /// Creates a path by appending a mapper element for targetPath.
    /// This path must be a prim property or relational attribute path.
    /// </summary>
    ISdfPath AppendMapper(ISdfPath targetPath);

    /// <summary>
    /// Creates a path by appending an element for argName.
    /// This path must be a mapper path.
    /// </summary>
    ISdfPath AppendMapperArg(TfToken argName);

    /// <summary>
    /// Creates a path by appending an expression element.
    /// This path must be a prim property or relational attribute path.
    /// </summary>
    ISdfPath AppendExpression();

    /// <summary>
    /// Creates a path by extracting and appending an element from the given ascii element encoding.
    /// Attempting to append a root or empty path (or malformed path) or attempting to append
    /// to the EmptyPath will raise an error and return the EmptyPath.
    /// May also fail and return EmptyPath if this path's type cannot possess a child of the type encoded in element.
    /// </summary>
    ISdfPath AppendElementString(string element);

    /// <summary>
    /// Like AppendElementString() but take the element as a TfToken.
    /// </summary>
    ISdfPath AppendElementToken(TfToken elementTok);

    /// <summary>
    /// Returns a path with all occurrences of the prefix path oldPrefix replaced
    /// with the prefix path newPrefix.
    /// If fixTargetPaths is true, any embedded target paths will also have their paths replaced.
    /// This is the default.
    /// If this is not a target, relational attribute or mapper path this will do zero or one
    /// path prefix replacements, if not the number of replacements can be greater than one.
    /// </summary>
    ISdfPath ReplacePrefix(ISdfPath oldPrefix, ISdfPath newPrefix, bool fixTargetPaths = true);

    /// <summary>
    /// Returns a path with maximal length that is a prefix path of both this path and path.
    /// </summary>
    ISdfPath GetCommonPrefix(ISdfPath path);

    /// <summary>
    /// Find and remove the longest common suffix from two paths.
    /// Returns this path and otherPath with the longest common suffix removed (first and second, respectively).
    /// If the two paths have no common suffix then the paths are returned as-is.
    /// If the paths are equal then this returns empty paths for relative paths and absolute roots for absolute paths.
    /// The paths need not be the same length.
    /// If stopAtRootPrim is true then neither returned path will be the root path.
    /// That, in turn, means that some common suffixes will not be removed.
    /// For example, if stopAtRootPrim is true then the paths /A/B and /B will be returned as is.
    /// Were it false then the result would be /A and /. Similarly paths /A/B/C and /B/C would return
    /// /A/B and /B if stopAtRootPrim is true but /A and / if it's false.
    /// </summary>
    (ISdfPath first, ISdfPath second) RemoveCommonSuffix(ISdfPath otherPath, bool stopAtRootPrim = false);

    /// <summary>
    /// Returns the absolute form of this path using anchor as the relative basis.
    /// anchor must be an absolute prim path.
    /// If this path is a relative path, resolve it using anchor as the relative basis.
    /// If this path is already an absolute path, just return a copy.
    /// </summary>
    ISdfPath MakeAbsolutePath(ISdfPath anchor);

    /// <summary>
    /// Returns the relative form of this path using anchor as the relative basis.
    /// anchor must be an absolute prim path.
    /// If this path is an absolute path, return the corresponding relative path
    /// that is relative to the absolute path given by anchor.
    /// If this path is a relative path, return the optimal relative path to the
    /// absolute path given by anchor. (The optimal relative path from a given prim path
    /// is the relative path with the least leading dot-dots.
    /// </summary>
    ISdfPath MakeRelativePath(ISdfPath anchor);

    /// <summary>
    /// Get the hash code for this path.
    /// </summary>
    int GetHashCode();
}

