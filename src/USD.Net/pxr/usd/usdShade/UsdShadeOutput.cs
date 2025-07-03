using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdShade;

public readonly struct UsdShadeOutput : IEquatable<UsdShadeOutput>
{
    private readonly UsdAttribute _attr;

    internal UsdShadeOutput(UsdAttribute attr)
    {
        _attr = attr;
    }

    public static UsdShadeOutput CreateOutput(UsdPrim prim, TfToken name, string typeName)
    {
        if (!prim.IsValid() || name.IsEmpty)
            return new UsdShadeOutput();

        var attrName = new TfToken($"outputs:{name}");
        var attr = prim.CreateAttribute(attrName.GetText(), typeName, false);
        return new UsdShadeOutput(attr);
    }

    public static UsdShadeOutput GetOutput(UsdPrim prim, TfToken name)
    {
        if (!prim.IsValid() || name.IsEmpty)
            return new UsdShadeOutput();

        var attrName = new TfToken($"outputs:{name}");
        var attr = prim.GetAttribute(attrName);
        return new UsdShadeOutput(attr);
    }

    public bool IsValid() => _attr.IsValid();

    public UsdAttribute GetAttr() => _attr;

    public TfToken GetBaseName()
    {
        if (!IsValid())
            return new TfToken();

        var attrName = _attr.GetName();
        if (attrName.StartsWith("outputs:"))
            return new TfToken(attrName.Substring(8));
        
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

    public bool Equals(UsdShadeOutput other) => _attr.Equals(other._attr);

    public override bool Equals(object? obj) => obj is UsdShadeOutput other && Equals(other);

    public override int GetHashCode() => _attr.GetHashCode();

    public static bool operator ==(UsdShadeOutput left, UsdShadeOutput right) => left.Equals(right);

    public static bool operator !=(UsdShadeOutput left, UsdShadeOutput right) => !left.Equals(right);

    public override string ToString()
    {
        if (!IsValid())
            return "invalid";
        
        return $"UsdShadeOutput({GetAttr().GetPath()})";
    }
}