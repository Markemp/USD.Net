using System;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Xform", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomXform : UsdGeomXformable
{
    #region Construction
    
    public UsdGeomXform(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdGeomXform() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Xform");
    protected override TfToken GetTypeName() => new TfToken("Xform");
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdGeomXform Get(UsdStage stage, SdfPath path)
    {
        return Get<UsdGeomXform>(stage, path);
    }
    
    public new static UsdGeomXform Define(UsdStage stage, SdfPath path)
    {
        return Define<UsdGeomXform>(stage, path);
    }
    
    #endregion
}