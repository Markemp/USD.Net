using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Gprim", UsdSchemaKind.AbstractTyped, IsAbstract = true)]
public abstract class UsdGeomGprim : UsdGeomBoundable
{
    #region Construction
    
    protected UsdGeomGprim(UsdPrim prim) : base(prim)
    {
    }
    
    protected UsdGeomGprim() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.AbstractTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Gprim");
    protected override TfToken GetTypeName() => TfToken.Empty;
    
    #endregion
    
    #region Display Color Attribute
    
    /// <summary>
    /// Get the display color attribute. This is useful as a fallback color
    /// for rendering when no shader is bound to the gprim.
    /// </summary>
    public UsdAttribute GetDisplayColorAttr()
    {
        return GetAttribute(new TfToken("primvars:displayColor"));
    }
    
    /// <summary>
    /// Create the display color attribute with default value.
    /// </summary>
    public UsdAttribute CreateDisplayColorAttr()
    {
        return CreateAttribute(
            new TfToken("primvars:displayColor"), 
            "color3f[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Get the display color.
    /// </summary>
    public List<GfVec3Color> DisplayColor
    {
        get
        {
            var attr = GetDisplayColorAttr();
            if (attr.IsValid() && attr.Get(out List<GfVec3Color> value))
                return value;
            return new List<GfVec3Color>();
        }
        set
        {
            var attr = CreateDisplayColorAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Display Opacity Attribute
    
    /// <summary>
    /// Get the display opacity attribute. Companion to displayColor that specifies opacity.
    /// </summary>
    public UsdAttribute GetDisplayOpacityAttr()
    {
        return GetAttribute(new TfToken("primvars:displayOpacity"));
    }
    
    /// <summary>
    /// Create the display opacity attribute with default value.
    /// </summary>
    public UsdAttribute CreateDisplayOpacityAttr()
    {
        return CreateAttribute(
            new TfToken("primvars:displayOpacity"), 
            "float[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Get the display opacity.
    /// </summary>
    public List<float> DisplayOpacity
    {
        get
        {
            var attr = GetDisplayOpacityAttr();
            if (attr.IsValid() && attr.Get(out List<float> value))
                return value;
            return new List<float>();
        }
        set
        {
            var attr = CreateDisplayOpacityAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Double Sided Attribute
    
    /// <summary>
    /// Get the double sided attribute. When true, instructs renderers to disable
    /// backface culling and provide forward-facing normals on both sides.
    /// </summary>
    public UsdAttribute GetDoubleSidedAttr()
    {
        return GetAttribute(new TfToken("doubleSided"));
    }
    
    /// <summary>
    /// Create the double sided attribute with default value.
    /// </summary>
    public UsdAttribute CreateDoubleSidedAttr()
    {
        return CreateAttribute(
            new TfToken("doubleSided"), 
            "bool", 
            false, 
            SdfVariability.Uniform, 
            new VtValue(false));
    }
    
    /// <summary>
    /// Get or set whether this gprim is double sided.
    /// </summary>
    public bool DoubleSided
    {
        get
        {
            var attr = GetDoubleSidedAttr();
            if (attr.IsValid() && attr.Get(out bool value))
                return value;
            return false; // Default value
        }
        set
        {
            var attr = CreateDoubleSidedAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Orientation Attribute
    
    /// <summary>
    /// Get the orientation attribute. Specifies whether the gprim's surface normal
    /// should be computed using the right hand rule or left hand rule.
    /// </summary>
    public UsdAttribute GetOrientationAttr()
    {
        return GetAttribute(new TfToken("orientation"));
    }
    
    /// <summary>
    /// Create the orientation attribute with default value.
    /// </summary>
    public UsdAttribute CreateOrientationAttr()
    {
        return CreateAttribute(
            new TfToken("orientation"), 
            "token", 
            false, 
            SdfVariability.Uniform, 
            new VtValue("rightHanded"));
    }
    
    /// <summary>
    /// Get or set the orientation (winding order).
    /// </summary>
    public string Orientation
    {
        get
        {
            var attr = GetOrientationAttr();
            if (attr.IsValid() && attr.Get(out string value))
                return value;
            return "rightHanded"; // Default value
        }
        set
        {
            var attr = CreateOrientationAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    /// <summary>
    /// Check if this gprim uses right-handed orientation.
    /// </summary>
    public bool IsRightHanded => Orientation == "rightHanded";
    
    /// <summary>
    /// Check if this gprim uses left-handed orientation.
    /// </summary>
    public bool IsLeftHanded => Orientation == "leftHanded";
    
    #endregion
    
    #region Convenience Methods
    
    /// <summary>
    /// Set a single display color for the entire gprim.
    /// </summary>
    public void SetDisplayColor(GfVec3Color color)
    {
        DisplayColor = new List<GfVec3Color> { color };
    }
    
    /// <summary>
    /// Set a single display color using RGB values (0-1 range).
    /// </summary>
    public void SetDisplayColor(float r, float g, float b)
    {
        SetDisplayColor(new GfVec3Color(r, g, b));
    }
    
    /// <summary>
    /// Set a single display color using RGB values (0-255 range).
    /// </summary>
    public void SetDisplayColorRGB(byte r, byte g, byte b)
    {
        SetDisplayColor(GfVec3Color.FromRGB(r, g, b));
    }
    
    /// <summary>
    /// Set a single display opacity for the entire gprim.
    /// </summary>
    public void SetDisplayOpacity(float opacity)
    {
        DisplayOpacity = new List<float> { opacity };
    }
    
    /// <summary>
    /// Set display properties (color and opacity) for the gprim.
    /// </summary>
    public void SetDisplayProperties(GfVec3Color color, float opacity = 1.0f)
    {
        SetDisplayColor(color);
        SetDisplayOpacity(opacity);
    }
    
    /// <summary>
    /// Clear all display color values.
    /// </summary>
    public bool ClearDisplayColor()
    {
        var attr = GetDisplayColorAttr();
        if (attr.IsValid())
        {
            return attr.Clear();
        }
        return true;
    }
    
    /// <summary>
    /// Clear all display opacity values.
    /// </summary>
    public bool ClearDisplayOpacity()
    {
        var attr = GetDisplayOpacityAttr();
        if (attr.IsValid())
        {
            return attr.Clear();
        }
        return true;
    }
    
    /// <summary>
    /// Check if this gprim has any display attributes authored.
    /// </summary>
    public bool HasDisplayAttributes()
    {
        var colorAttr = GetDisplayColorAttr();
        var opacityAttr = GetDisplayOpacityAttr();
        var doubleSidedAttr = GetDoubleSidedAttr();
        var orientationAttr = GetOrientationAttr();
        
        return (colorAttr.IsValid() && colorAttr.HasValue()) ||
               (opacityAttr.IsValid() && opacityAttr.HasValue()) ||
               (doubleSidedAttr.IsValid() && doubleSidedAttr.HasValue()) ||
               (orientationAttr.IsValid() && orientationAttr.HasValue());
    }
    
    /// <summary>
    /// Get all display-related attributes for this gprim.
    /// </summary>
    public IEnumerable<UsdAttribute> GetDisplayAttributes()
    {
        var attributes = new List<UsdAttribute>();
        
        var colorAttr = GetDisplayColorAttr();
        if (colorAttr.IsValid())
            attributes.Add(colorAttr);
            
        var opacityAttr = GetDisplayOpacityAttr();
        if (opacityAttr.IsValid())
            attributes.Add(opacityAttr);
            
        var doubleSidedAttr = GetDoubleSidedAttr();
        if (doubleSidedAttr.IsValid())
            attributes.Add(doubleSidedAttr);
            
        var orientationAttr = GetOrientationAttr();
        if (orientationAttr.IsValid())
            attributes.Add(orientationAttr);
            
        return attributes;
    }
    
    #endregion
    
    #region Common Display Colors
    
    public static readonly GfVec3Color White = GfVec3Color.White;
    public static readonly GfVec3Color Black = GfVec3Color.Black;
    public static readonly GfVec3Color Red = GfVec3Color.Red;
    public static readonly GfVec3Color Green = GfVec3Color.Green;
    public static readonly GfVec3Color Blue = GfVec3Color.Blue;
    
    #endregion
}