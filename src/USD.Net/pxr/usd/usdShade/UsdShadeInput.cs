using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdShade;

public readonly struct UsdShadeInput : IEquatable<UsdShadeInput>
{
    private readonly UsdAttribute _attr;

    internal UsdShadeInput(UsdAttribute attr)
    {
        _attr = attr;
    }

    public static UsdShadeInput CreateInput(UsdPrim prim, TfToken name, string typeName)
    {
        if (!prim.IsValid() || name.IsEmpty)
            return new UsdShadeInput();

        var attrName = new TfToken($"inputs:{name}");
        var attr = prim.CreateAttribute(attrName.GetText(), typeName, false);
        return new UsdShadeInput(attr);
    }

    public static UsdShadeInput GetInput(UsdPrim prim, TfToken name)
    {
        if (!prim.IsValid() || name.IsEmpty)
            return new UsdShadeInput();

        var attrName = new TfToken($"inputs:{name}");
        var attr = prim.GetAttribute(attrName);
        return new UsdShadeInput(attr);
    }

    public bool IsValid() => _attr.IsValid();

    public UsdAttribute GetAttr() => _attr;

    public TfToken GetBaseName()
    {
        if (!IsValid())
            return new TfToken();

        var attrName = _attr.GetName();
        if (attrName.StartsWith("inputs:"))
            return new TfToken(attrName.Substring(7));
        
        return new TfToken();
    }

    public string GetTypeName() => _attr.GetTypeName();

    public UsdStage? GetStage() => _attr.GetStage();

    public SdfPath GetPath() => _attr.GetPath();

    public UsdPrim GetPrim()
    {
        var stage = GetStage();
        if (stage == null)
            return new UsdPrim();

        // Get the prim path by removing the property name from the attribute path
        var attrPath = GetPath();
        var primPath = attrPath.GetParentPath();
        return stage.GetPrimAtPath(primPath);
    }

    public T Get<T>(UsdTimeCode time = default)
    {
        _attr.Get(out T value, time);
        return value;
    }

    public bool Set<T>(T value, UsdTimeCode time = default)
    {
        return _attr.Set(value, time);
    }

    public bool ConnectToSource(SdfPath sourcePath)
    {
        return _attr.AddConnection(sourcePath);
    }

    public bool ConnectToSource(UsdShadeInput sourceInput)
    {
        if (!sourceInput.IsValid())
            return false;

        return ConnectToSource(sourceInput.GetAttr().GetPath());
    }

    public bool ConnectToSource(UsdShadeOutput sourceOutput)
    {
        if (!sourceOutput.IsValid())
            return false;

        return ConnectToSource(sourceOutput.GetAttr().GetPath());
    }

    public bool ClearSources()
    {
        return _attr.ClearConnections();
    }

    public SdfPath[] GetConnectedSources()
    {
        return _attr.GetConnections();
    }

    public bool HasConnectedSource()
    {
        return GetConnectedSources().Length > 0;
    }

    public bool SetRenderType(TfToken renderType)
    {
        if (!IsValid())
            return false;

        return _attr.SetMetadata(UsdShadeTokens.RenderType, renderType);
    }

    public TfToken GetRenderType()
    {
        if (!IsValid())
            return new TfToken();

        var renderType = _attr.GetMetadata<TfToken?>(UsdShadeTokens.RenderType);
        return renderType ?? TfToken.Empty;
    }

    public bool SetSdrMetadata(Dictionary<TfToken, object> sdrMetadata)
    {
        if (!IsValid())
            return false;

        return _attr.SetMetadata(UsdShadeTokens.SdrMetadata, sdrMetadata);
    }

    public Dictionary<TfToken, object> GetSdrMetadata()
    {
        if (!IsValid())
            return new Dictionary<TfToken, object>();

        var sdrMetadata = _attr.GetMetadata<Dictionary<TfToken, object>>(UsdShadeTokens.SdrMetadata);
        return sdrMetadata ?? new Dictionary<TfToken, object>();
    }

    public bool SetConnectability(UsdShadeAttributeType connectability)
    {
        if (!IsValid())
            return false;

        var connectabilityStr = connectability switch
        {
            UsdShadeAttributeType.Input => "full",
            UsdShadeAttributeType.Output => "interfaceOnly",
            _ => "full"
        };

        return _attr.SetMetadata(UsdShadeTokens.Connectability, new TfToken(connectabilityStr));
    }

    public UsdShadeAttributeType GetConnectability()
    {
        if (!IsValid())
            return UsdShadeAttributeType.Invalid;

        var connectability = _attr.GetMetadata<TfToken?>(UsdShadeTokens.Connectability);
        if (connectability.HasValue)
        {
            return connectability.Value.GetText() switch
            {
                "full" => UsdShadeAttributeType.Input,
                "interfaceOnly" => UsdShadeAttributeType.Output,
                _ => UsdShadeAttributeType.Invalid
            };
        }

        return UsdShadeAttributeType.Input; // Default connectability
    }

    public bool Equals(UsdShadeInput other) => _attr.Equals(other._attr);

    public override bool Equals(object? obj) => obj is UsdShadeInput other && Equals(other);

    public override int GetHashCode() => _attr.GetHashCode();

    public static bool operator ==(UsdShadeInput left, UsdShadeInput right) => left.Equals(right);

    public static bool operator !=(UsdShadeInput left, UsdShadeInput right) => !left.Equals(right);

    public override string ToString()
    {
        if (!IsValid())
            return "invalid";
        
        return $"UsdShadeInput({GetAttr().GetPath()})";
    }
}