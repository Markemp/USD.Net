using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdShade;

public readonly struct UsdShadeConnectableAPI : IEquatable<UsdShadeConnectableAPI>
{
    private readonly UsdPrim _prim;

    public UsdShadeConnectableAPI(UsdPrim prim)
    {
        _prim = prim;
    }

    public static UsdShadeConnectableAPI Apply(UsdPrim prim)
    {
        if (!prim.IsValid())
            return new UsdShadeConnectableAPI();

        // Note: In a full implementation, this would apply the ConnectableAPI schema
        // For now, we'll just return the API wrapper
        return new UsdShadeConnectableAPI(prim);
    }

    public static UsdShadeConnectableAPI Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdShadeConnectableAPI(prim);
    }

    public bool IsValid() => _prim.IsValid();

    public UsdPrim GetPrim() => _prim;

    public UsdSchemaKind GetSchemaType() => UsdSchemaKind.SingleApplyAPI;

    public UsdShadeInput CreateInput(TfToken name, string typeName)
    {
        return UsdShadeInput.CreateInput(_prim, name, typeName);
    }

    public UsdShadeInput GetInput(TfToken name)
    {
        return UsdShadeInput.GetInput(_prim, name);
    }

    public UsdShadeInput[] GetInputs(bool onlyAuthored = true)
    {
        var inputs = new List<UsdShadeInput>();
        
        foreach (var attr in _prim.GetAttributes())
        {
            if (onlyAuthored && !attr.IsAuthored())
                continue;

            var attrName = attr.GetName();
            if (attrName.StartsWith("inputs:"))
            {
                inputs.Add(new UsdShadeInput(attr));
            }
        }

        return inputs.ToArray();
    }

    public UsdShadeOutput CreateOutput(TfToken name, string typeName)
    {
        return UsdShadeOutput.CreateOutput(_prim, name, typeName);
    }

    public UsdShadeOutput GetOutput(TfToken name)
    {
        return UsdShadeOutput.GetOutput(_prim, name);
    }

    public UsdShadeOutput[] GetOutputs(bool onlyAuthored = true)
    {
        var outputs = new List<UsdShadeOutput>();
        
        foreach (var attr in _prim.GetAttributes())
        {
            if (onlyAuthored && !attr.IsAuthored())
                continue;

            var attrName = attr.GetName();
            if (attrName.StartsWith("outputs:"))
            {
                outputs.Add(new UsdShadeOutput(attr));
            }
        }

        return outputs.ToArray();
    }

    public static bool ConnectToSource(UsdAttribute shadingAttr, SdfPath source)
    {
        if (!shadingAttr.IsValid() || source.IsEmpty())
            return false;

        return shadingAttr.AddConnection(source);
    }

    public static bool ConnectToSource(UsdAttribute shadingAttr, UsdShadeConnectableAPI source, TfToken sourceName, UsdShadeAttributeType sourceType = UsdShadeAttributeType.Output, string typeName = "")
    {
        if (!shadingAttr.IsValid() || !source.IsValid())
            return false;

        var sourcePath = source.GetPrim().GetPath();
        var sourceAttrName = sourceType == UsdShadeAttributeType.Input 
            ? new TfToken($"inputs:{sourceName}")
            : new TfToken($"outputs:{sourceName}");

        var targetPath = sourcePath.AppendProperty(sourceAttrName);
        return ConnectToSource(shadingAttr, targetPath);
    }

    public static bool ConnectToSource(UsdAttribute shadingAttr, UsdShadeInput sourceInput)
    {
        if (!sourceInput.IsValid())
            return false;

        return ConnectToSource(shadingAttr, sourceInput.GetAttr().GetPath());
    }

    public static bool ConnectToSource(UsdAttribute shadingAttr, UsdShadeOutput sourceOutput)
    {
        if (!sourceOutput.IsValid())
            return false;

        return ConnectToSource(shadingAttr, sourceOutput.GetAttr().GetPath());
    }

    public static bool DisconnectSource(UsdAttribute shadingAttr, SdfPath sourceAttr = default)
    {
        if (!shadingAttr.IsValid())
            return false;

        if (sourceAttr.IsEmpty())
        {
            // Clear all connections
            return shadingAttr.ClearConnections();
        }
        else
        {
            // Remove specific connection - for now, just clear all since USD.Net doesn't have RemoveConnection yet
            return shadingAttr.ClearConnections();
        }
    }

    public static bool ClearSources(UsdAttribute shadingAttr)
    {
        return DisconnectSource(shadingAttr);
    }

    public static SdfPath[] GetConnectedSources(UsdAttribute shadingAttr)
    {
        if (!shadingAttr.IsValid())
            return Array.Empty<SdfPath>();

        return shadingAttr.GetConnections();
    }

    public static bool GetConnectedSource(UsdAttribute shadingAttr, out UsdShadeConnectableAPI source, out TfToken sourceName, out UsdShadeAttributeType sourceType)
    {
        source = new UsdShadeConnectableAPI();
        sourceName = new TfToken();
        sourceType = UsdShadeAttributeType.Invalid;

        var sources = GetConnectedSources(shadingAttr);
        if (sources.Length == 0)
            return false;

        var sourcePath = sources[0];
        var stage = shadingAttr.GetStage();
        if (stage == null)
            return false;

        var sourcePrim = stage.GetPrimAtPath(sourcePath.GetParentPath());
        
        if (!sourcePrim.IsValid())
            return false;

        source = new UsdShadeConnectableAPI(sourcePrim);
        
        var propertyName = sourcePath.GetName();
        if (propertyName.StartsWith("inputs:"))
        {
            sourceName = new TfToken(propertyName.Substring(7));
            sourceType = UsdShadeAttributeType.Input;
        }
        else if (propertyName.StartsWith("outputs:"))
        {
            sourceName = new TfToken(propertyName.Substring(8));
            sourceType = UsdShadeAttributeType.Output;
        }
        else
        {
            return false;
        }

        return true;
    }

    public static bool CanConnect(UsdAttribute shadingAttr, UsdAttribute sourceAttr)
    {
        if (!shadingAttr.IsValid() || !sourceAttr.IsValid())
            return false;

        // Basic type compatibility check
        var shadingType = shadingAttr.GetTypeName();
        var sourceType = sourceAttr.GetTypeName();
        
        // Allow same types
        if (shadingType == sourceType)
            return true;

        // Allow compatible numeric types (float->color, etc.)
        return IsCompatibleType(shadingType, sourceType);
    }

    public static bool CanConnect(UsdShadeInput shadingInput, UsdShadeOutput sourceOutput)
    {
        if (!shadingInput.IsValid() || !sourceOutput.IsValid())
            return false;

        return CanConnect(shadingInput.GetAttr(), sourceOutput.GetAttr());
    }

    public static bool CanConnect(UsdShadeOutput shadingOutput, UsdShadeOutput sourceOutput)
    {
        if (!shadingOutput.IsValid() || !sourceOutput.IsValid())
            return false;

        return CanConnect(shadingOutput.GetAttr(), sourceOutput.GetAttr());
    }

    private static bool IsCompatibleType(string targetType, string sourceType)
    {
        // Simplified type compatibility check
        // In a full implementation, this would check USD's type compatibility rules
        
        // Same type is always compatible
        if (targetType == sourceType)
            return true;

        // Float can connect to color/vector types
        if (sourceType == "float" && (targetType.Contains("color") || targetType.Contains("vector")))
            return true;

        // Allow color3f -> vector3f and vice versa
        if ((sourceType.Contains("color3") && targetType.Contains("vector3")) ||
            (sourceType.Contains("vector3") && targetType.Contains("color3")))
            return true;

        return false;
    }

    public bool HasConnectableAPI()
    {
        // For now, assume all prims can be connectable
        return _prim.IsValid();
    }

    public bool Equals(UsdShadeConnectableAPI other) => _prim.Equals(other._prim);

    public override bool Equals(object? obj) => obj is UsdShadeConnectableAPI other && Equals(other);

    public override int GetHashCode() => _prim.GetHashCode();

    public static bool operator ==(UsdShadeConnectableAPI left, UsdShadeConnectableAPI right) => left.Equals(right);

    public static bool operator !=(UsdShadeConnectableAPI left, UsdShadeConnectableAPI right) => !left.Equals(right);

    public override string ToString()
    {
        if (!IsValid())
            return "invalid";
        
        return $"UsdShadeConnectableAPI({GetPrim().GetPath()})";
    }
}