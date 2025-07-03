using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdShade;

public class UsdShadeShader : UsdTyped
{
    public UsdShadeShader() : base()
    {
    }

    public UsdShadeShader(UsdPrim prim) : base(prim)
    {
    }

    public static UsdShadeShader Define(UsdStage stage, SdfPath path)
    {
        var prim = stage.DefinePrim(path, "Shader");
        return new UsdShadeShader(prim);
    }

    public static UsdShadeShader Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdShadeShader(prim);
    }

    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Shader");
    protected override TfToken GetTypeName() => new TfToken("Shader");

    #region Connectability

    public UsdShadeConnectableAPI ConnectableAPI => new UsdShadeConnectableAPI(Prim);

    public UsdShadeInput CreateInput(TfToken name, string typeName)
    {
        return ConnectableAPI.CreateInput(name, typeName);
    }

    public UsdShadeInput GetInput(TfToken name)
    {
        return ConnectableAPI.GetInput(name);
    }

    public UsdShadeInput[] GetInputs(bool onlyAuthored = true)
    {
        return ConnectableAPI.GetInputs(onlyAuthored);
    }

    public UsdShadeOutput CreateOutput(TfToken name, string typeName)
    {
        return ConnectableAPI.CreateOutput(name, typeName);
    }

    public UsdShadeOutput GetOutput(TfToken name)
    {
        return ConnectableAPI.GetOutput(name);
    }

    public UsdShadeOutput[] GetOutputs(bool onlyAuthored = true)
    {
        return ConnectableAPI.GetOutputs(onlyAuthored);
    }

    #endregion

    #region Shader Info Attributes

    public UsdAttribute GetIdAttr()
    {
        return Prim.GetAttribute(UsdShadeTokens.InfoId);
    }

    public UsdAttribute CreateIdAttr(VtValue? defaultValue = null, bool writeSparsely = false)
    {
        return CreateAttribute(UsdShadeTokens.InfoId, "token", false, SdfVariability.Uniform, defaultValue);
    }

    public bool SetShaderId(TfToken shaderId)
    {
        var attr = CreateIdAttr();
        return attr.Set(shaderId);
    }

    public TfToken GetShaderId()
    {
        var attr = GetIdAttr();
        if (attr.IsValid())
        {
            attr.Get(out TfToken value);
            return value;
        }
        return new TfToken();
    }

    public UsdAttribute GetImplementationSourceAttr()
    {
        return Prim.GetAttribute(UsdShadeTokens.InfoImplementationSource);
    }

    public UsdAttribute CreateImplementationSourceAttr(VtValue? defaultValue = null, bool writeSparsely = false)
    {
        return CreateAttribute(UsdShadeTokens.InfoImplementationSource, "token", false, SdfVariability.Uniform, defaultValue);
    }

    public bool SetImplementationSource(TfToken source)
    {
        var attr = CreateImplementationSourceAttr();
        return attr.Set(source);
    }

    public TfToken GetImplementationSource()
    {
        var attr = GetImplementationSourceAttr();
        if (attr.IsValid())
        {
            attr.Get(out TfToken value);
            return value;
        }
        return new TfToken("id"); // Default implementation source
    }

    #endregion

    #region Source Asset and Code

    public UsdAttribute GetSourceAssetAttr()
    {
        return Prim.GetAttribute(new TfToken("info:sourceAsset"));
    }

    public UsdAttribute CreateSourceAssetAttr(VtValue? defaultValue = null, bool writeSparsely = false)
    {
        return CreateAttribute(new TfToken("info:sourceAsset"), "asset", false, SdfVariability.Uniform, defaultValue);
    }

    public bool SetSourceAsset(SdfAssetPath assetPath, string subIdentifier = "")
    {
        var attr = CreateSourceAssetAttr();
        var success = attr.Set(assetPath);
        
        if (!string.IsNullOrEmpty(subIdentifier))
        {
            var subIdAttr = CreateSourceAssetSubIdentifierAttr();
            success &= subIdAttr.Set(new TfToken(subIdentifier));
        }
        
        return success;
    }

    public SdfAssetPath GetSourceAsset()
    {
        var attr = GetSourceAssetAttr();
        if (attr.IsValid())
        {
            attr.Get(out SdfAssetPath value);
            return value;
        }
        return new SdfAssetPath();
    }

    public UsdAttribute GetSourceAssetSubIdentifierAttr()
    {
        return Prim.GetAttribute(new TfToken("info:sourceAsset:subIdentifier"));
    }

    public UsdAttribute CreateSourceAssetSubIdentifierAttr(VtValue? defaultValue = null, bool writeSparsely = false)
    {
        return CreateAttribute(new TfToken("info:sourceAsset:subIdentifier"), "token", false, SdfVariability.Uniform, defaultValue);
    }

    public UsdAttribute GetSourceCodeAttr()
    {
        return Prim.GetAttribute(new TfToken("info:sourceCode"));
    }

    public UsdAttribute CreateSourceCodeAttr(VtValue? defaultValue = null, bool writeSparsely = false)
    {
        return CreateAttribute(new TfToken("info:sourceCode"), "string", false, SdfVariability.Uniform, defaultValue);
    }

    public bool SetSourceCode(string sourceCode, string language = "")
    {
        var attr = CreateSourceCodeAttr();
        var success = attr.Set(sourceCode);
        
        if (!string.IsNullOrEmpty(language))
        {
            var langAttr = CreateSourceCodeLanguageAttr();
            success &= langAttr.Set(new TfToken(language));
        }
        
        return success;
    }

    public string GetSourceCode()
    {
        var attr = GetSourceCodeAttr();
        if (attr.IsValid())
        {
            attr.Get(out string value);
            return value;
        }
        return string.Empty;
    }

    public UsdAttribute GetSourceCodeLanguageAttr()
    {
        return Prim.GetAttribute(new TfToken("info:sourceCode:language"));
    }

    public UsdAttribute CreateSourceCodeLanguageAttr(VtValue? defaultValue = null, bool writeSparsely = false)
    {
        return CreateAttribute(new TfToken("info:sourceCode:language"), "token", false, SdfVariability.Uniform, defaultValue);
    }

    #endregion

    #region Factory Methods for Common Shaders

    public static UsdShadeShader CreateUsdPreviewSurface(UsdStage stage, SdfPath path)
    {
        var shader = Define(stage, path);
        shader.SetShaderId(new TfToken("UsdPreviewSurface"));
        shader.SetImplementationSource(new TfToken("id"));
        
        // Create common PBR inputs
        shader.CreateInput(new TfToken("diffuseColor"), "color3f");
        shader.CreateInput(new TfToken("emissiveColor"), "color3f");
        shader.CreateInput(new TfToken("metallic"), "float");
        shader.CreateInput(new TfToken("roughness"), "float");
        shader.CreateInput(new TfToken("clearcoat"), "float");
        shader.CreateInput(new TfToken("clearcoatRoughness"), "float");
        shader.CreateInput(new TfToken("opacity"), "float");
        shader.CreateInput(new TfToken("opacityThreshold"), "float");
        shader.CreateInput(new TfToken("ior"), "float");
        shader.CreateInput(new TfToken("normal"), "normal3f");
        shader.CreateInput(new TfToken("displacement"), "float");
        shader.CreateInput(new TfToken("occlusion"), "float");
        
        // Create outputs
        shader.CreateOutput(new TfToken("surface"), "token");
        shader.CreateOutput(new TfToken("displacement"), "token");
        
        return shader;
    }

    public static UsdShadeShader CreateUsdUVTexture(UsdStage stage, SdfPath path)
    {
        var shader = Define(stage, path);
        shader.SetShaderId(new TfToken("UsdUVTexture"));
        shader.SetImplementationSource(new TfToken("id"));
        
        // Create texture inputs
        shader.CreateInput(new TfToken("file"), "asset");
        shader.CreateInput(new TfToken("st"), "texCoord2f");
        shader.CreateInput(new TfToken("wrapS"), "token");
        shader.CreateInput(new TfToken("wrapT"), "token");
        shader.CreateInput(new TfToken("fallback"), "float4");
        shader.CreateInput(new TfToken("scale"), "float4");
        shader.CreateInput(new TfToken("bias"), "float4");
        
        // Create outputs
        shader.CreateOutput(new TfToken("r"), "float");
        shader.CreateOutput(new TfToken("g"), "float");
        shader.CreateOutput(new TfToken("b"), "float");
        shader.CreateOutput(new TfToken("a"), "float");
        shader.CreateOutput(new TfToken("rgb"), "float3");
        shader.CreateOutput(new TfToken("rgba"), "float4");
        
        return shader;
    }

    public static UsdShadeShader CreateUsdPrimvarReader(UsdStage stage, SdfPath path, string primvarType = "float2")
    {
        var shader = Define(stage, path);
        shader.SetShaderId(new TfToken($"UsdPrimvarReader_{primvarType}"));
        shader.SetImplementationSource(new TfToken("id"));
        
        // Create inputs
        shader.CreateInput(new TfToken("varname"), "token");
        shader.CreateInput(new TfToken("fallback"), primvarType);
        
        // Create output
        shader.CreateOutput(new TfToken("result"), primvarType);
        
        return shader;
    }

    #endregion

    #region Utility Methods

    public bool IsShader()
    {
        return IsValid && Prim.GetTypeName() == "Shader";
    }

    public TfToken[] GetShaderInputNames()
    {
        if (!IsValid)
            return Array.Empty<TfToken>();
        return GetInputs().Select(input => input.GetBaseName()).ToArray();
    }

    public TfToken[] GetShaderOutputNames()
    {
        if (!IsValid)
            return Array.Empty<TfToken>();
        return GetOutputs().Select(output => output.GetBaseName()).ToArray();
    }

    public bool HasShaderInput(TfToken name)
    {
        if (!IsValid)
            return false;
        return GetInput(name).IsValid();
    }

    public bool HasShaderOutput(TfToken name)
    {
        if (!IsValid)
            return false;
        return GetOutput(name).IsValid();
    }

    public Dictionary<TfToken, object> GetShaderMetadata()
    {
        if (!IsValid)
            return new Dictionary<TfToken, object>();
            
        var metadata = new Dictionary<TfToken, object>();
        
        var shaderId = GetShaderId();
        if (!shaderId.IsEmpty)
            metadata[new TfToken("id")] = shaderId;
        
        var implSource = GetImplementationSource();
        if (!implSource.IsEmpty)
            metadata[new TfToken("implementationSource")] = implSource;
        
        var sourceAsset = GetSourceAsset();
        if (!string.IsNullOrEmpty(sourceAsset.GetAssetPath()))
            metadata[new TfToken("sourceAsset")] = sourceAsset;
        
        var sourceCode = GetSourceCode();
        if (!string.IsNullOrEmpty(sourceCode))
            metadata[new TfToken("sourceCode")] = sourceCode;
        
        return metadata;
    }

    #endregion
}