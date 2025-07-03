using Pxr.Base.Tf;

namespace Pxr.Usd.UsdShade;

public enum UsdShadeAttributeType
{
    Invalid,
    Input,
    Output
}

public static class UsdShadeTokens
{
    public static readonly TfToken ConnectableAPI = new("ConnectableAPI");
    public static readonly TfToken Connectability = new("connectability");
    public static readonly TfToken RenderType = new("renderType");
    public static readonly TfToken SdrMetadata = new("sdrMetadata");
    public static readonly TfToken Inputs = new("inputs");
    public static readonly TfToken Outputs = new("outputs");
    public static readonly TfToken Interface = new("interface");
    
    // Material terminals
    public static readonly TfToken Surface = new("surface");
    public static readonly TfToken Displacement = new("displacement");
    public static readonly TfToken Volume = new("volume");
    
    // Shader info
    public static readonly TfToken InfoId = new("info:id");
    public static readonly TfToken InfoImplementationSource = new("info:implementationSource");
    
    // Material binding
    public static readonly TfToken MaterialBinding = new("material:binding");
    public static readonly TfToken MaterialBindingCollection = new("material:binding:collection");
    
    // Material purposes
    public static readonly TfToken AllPurpose = new("allPurpose");
    public static readonly TfToken Preview = new("preview");
    public static readonly TfToken Full = new("full");
    
    // Binding strength
    public static readonly TfToken WeakerThanDescendants = new("weakerThanDescendants");
    public static readonly TfToken StrongerThanDescendants = new("strongerThanDescendants");
    
    // Connectability values
    public static readonly TfToken ConnectabilityFull = new("full");
    public static readonly TfToken ConnectabilityInterfaceOnly = new("interfaceOnly");
}

public static class UsdShadeUtils
{
    public static string GetFullName(TfToken baseName, UsdShadeAttributeType attributeType)
    {
        return attributeType switch
        {
            UsdShadeAttributeType.Input => $"inputs:{baseName}",
            UsdShadeAttributeType.Output => $"outputs:{baseName}",
            _ => baseName.GetText()
        };
    }

    public static bool IsShadeAttribute(TfToken attributeName)
    {
        var name = attributeName.GetText();
        return name.StartsWith("inputs:") || name.StartsWith("outputs:");
    }

    public static UsdShadeAttributeType GetAttributeType(TfToken attributeName)
    {
        var name = attributeName.GetText();
        if (name.StartsWith("inputs:"))
            return UsdShadeAttributeType.Input;
        if (name.StartsWith("outputs:"))
            return UsdShadeAttributeType.Output;
        return UsdShadeAttributeType.Invalid;
    }

    public static TfToken GetBaseName(TfToken attributeName)
    {
        var name = attributeName.GetText();
        if (name.StartsWith("inputs:"))
            return new TfToken(name.Substring(7));
        if (name.StartsWith("outputs:"))
            return new TfToken(name.Substring(8));
        return attributeName;
    }
}