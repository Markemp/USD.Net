using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Xformable", UsdSchemaKind.AbstractTyped, IsAbstract = true)]
public abstract class UsdGeomXformable : UsdGeomImageable
{
    #region Construction
    
    public UsdGeomXformable(UsdPrim prim) : base(prim)
    {
    }
    
    protected UsdGeomXformable() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.AbstractTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Xformable");
    protected override TfToken GetTypeName() => TfToken.Empty;
    
    #endregion
    
    #region XformOpOrder Attribute
    
    public UsdAttribute GetXformOpOrderAttr()
    {
        return GetAttribute(new TfToken("xformOpOrder"));
    }
    
    public UsdAttribute CreateXformOpOrderAttr()
    {
        return CreateAttribute(
            new TfToken("xformOpOrder"), 
            "token[]", 
            false, 
            SdfVariability.Uniform);
    }
    
    #endregion
    
    #region Transform Operations Management
    
    public enum XformOpType
    {
        Invalid,
        Translate,
        TranslateX,
        TranslateY, 
        TranslateZ,
        Scale,
        ScaleX,
        ScaleY,
        ScaleZ,
        RotateX,
        RotateY,
        RotateZ,
        RotateXYZ,
        RotateXZY,
        RotateYXZ,
        RotateYZX,
        RotateZXY,
        RotateZYX,
        Orient,
        Transform
    }
    
    public enum XformOpPrecision
    {
        Float,
        Double
    }
    
    public class UsdGeomXformOp
    {
        private readonly UsdAttribute _attr;
        private readonly XformOpType _opType;
        private readonly bool _isInverseOp;
        
        public UsdGeomXformOp(UsdAttribute attr, XformOpType opType, bool isInverseOp = false)
        {
            _attr = attr;
            _opType = opType;
            _isInverseOp = isInverseOp;
        }
        
        public UsdAttribute GetAttr() => _attr;
        public XformOpType GetOpType() => _opType;
        public bool IsInverseOp() => _isInverseOp;
        public bool IsValid() => _attr.IsValid();
        
        public TfToken GetOpName()
        {
            return _attr.IsValid() ? _attr.GetName() : TfToken.Empty;
        }
        
        public bool Set<T>(T value, UsdTimeCode time = default)
        {
            if (!_attr.IsValid() || _isInverseOp)
                return false;
            return _attr.Set(new VtValue(value), time);
        }
        
        public bool Get<T>(out T value, UsdTimeCode time = default)
        {
            value = default!;
            if (!_attr.IsValid())
                return false;
                
            if (_attr.Get(out VtValue vtValue, time))
            {
                if (vtValue.IsHolding<T>())
                {
                    value = vtValue.Get<T>();
                    return true;
                }
            }
            return false;
        }
        
        public GfMatrix4d GetOpTransform(UsdTimeCode time = default)
        {
            if (!IsValid())
                return GfMatrix4d.Identity;
                
            return _opType switch
            {
                XformOpType.Translate => ComputeTranslateMatrix(time),
                XformOpType.TranslateX => ComputeTranslateXMatrix(time),
                XformOpType.TranslateY => ComputeTranslateYMatrix(time),
                XformOpType.TranslateZ => ComputeTranslateZMatrix(time),
                XformOpType.Scale => ComputeScaleMatrix(time),
                XformOpType.ScaleX => ComputeScaleXMatrix(time),
                XformOpType.ScaleY => ComputeScaleYMatrix(time),
                XformOpType.ScaleZ => ComputeScaleZMatrix(time),
                XformOpType.RotateX => ComputeRotateXMatrix(time),
                XformOpType.RotateY => ComputeRotateYMatrix(time),
                XformOpType.RotateZ => ComputeRotateZMatrix(time),
                XformOpType.RotateXYZ => ComputeRotateXYZMatrix(time),
                XformOpType.Transform => ComputeTransformMatrix(time),
                _ => GfMatrix4d.Identity
            };
        }
        
        private GfMatrix4d ComputeTranslateMatrix(UsdTimeCode time)
        {
            if (Get<GfVec3f>(out var translate, time))
                return GfMatrix4d.CreateTranslation(translate);
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeTranslateXMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var x, time))
                return GfMatrix4d.CreateTranslation(new GfVec3f(x, 0, 0));
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeTranslateYMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var y, time))
                return GfMatrix4d.CreateTranslation(new GfVec3f(0, y, 0));
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeTranslateZMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var z, time))
                return GfMatrix4d.CreateTranslation(new GfVec3f(0, 0, z));
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeScaleMatrix(UsdTimeCode time)
        {
            if (Get<GfVec3f>(out var scale, time))
                return GfMatrix4d.CreateScale(scale);
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeScaleXMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var x, time))
                return GfMatrix4d.CreateScale(new GfVec3f(x, 1, 1));
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeScaleYMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var y, time))
                return GfMatrix4d.CreateScale(new GfVec3f(1, y, 1));
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeScaleZMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var z, time))
                return GfMatrix4d.CreateScale(new GfVec3f(1, 1, z));
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeRotateXMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var degrees, time))
            {
                var radians = degrees * (float)(Math.PI / 180.0);
                return GfMatrix4d.CreateRotation(radians, 0, 0);
            }
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeRotateYMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var degrees, time))
            {
                var radians = degrees * (float)(Math.PI / 180.0);
                return GfMatrix4d.CreateRotation(0, radians, 0);
            }
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeRotateZMatrix(UsdTimeCode time)
        {
            if (Get<float>(out var degrees, time))
            {
                var radians = degrees * (float)(Math.PI / 180.0);
                return GfMatrix4d.CreateRotation(0, 0, radians);
            }
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeRotateXYZMatrix(UsdTimeCode time)
        {
            if (Get<GfVec3f>(out var degrees, time))
            {
                var radiansX = degrees.X * (float)(Math.PI / 180.0);
                var radiansY = degrees.Y * (float)(Math.PI / 180.0);
                var radiansZ = degrees.Z * (float)(Math.PI / 180.0);
                return GfMatrix4d.CreateRotation(radiansX, radiansY, radiansZ);
            }
            return GfMatrix4d.Identity;
        }
        
        private GfMatrix4d ComputeTransformMatrix(UsdTimeCode time)
        {
            if (Get<GfMatrix4d>(out var matrix, time))
                return matrix;
            return GfMatrix4d.Identity;
        }
    }
    
    #endregion
    
    #region Transform Operations API
    
    public UsdGeomXformOp AddXformOp(XformOpType opType, 
                                     XformOpPrecision precision = XformOpPrecision.Double,
                                     TfToken opSuffix = default,
                                     bool isInverseOp = false)
    {
        var opName = GetXformOpAttrName(opType, opSuffix, isInverseOp);
        var typeName = GetXformOpTypeName(opType, precision);
        
        var attr = CreateAttribute(opName, typeName, false, SdfVariability.Varying);
        if (!attr.IsValid())
            return new UsdGeomXformOp(new UsdAttribute(), XformOpType.Invalid);
            
        // Add to xformOpOrder
        var orderAttr = GetXformOpOrderAttr();
        if (!orderAttr.IsValid())
        {
            orderAttr = CreateXformOpOrderAttr();
        }
        
        var newOpToken = GetXformOpOrderToken(opType, opSuffix, isInverseOp).GetText();
        
        // Get current order if it exists, otherwise start with empty list
        var currentOrder = new List<string>();
        if (orderAttr.IsValid())
        {
            // Try to get as string array first
            if (orderAttr.Get(out string[] orderArray))
            {
                currentOrder = orderArray.ToList();
            }
            else if (orderAttr.Get(out List<string> orderList))
            {
                currentOrder = orderList;
            }
            // If neither worked, currentOrder remains empty, which is correct
        }
        
        // Add the new operation to the order
        currentOrder.Add(newOpToken);
        
        // Set the updated order
        orderAttr.Set(new VtValue(currentOrder.ToArray()));
        
        return new UsdGeomXformOp(attr, opType, isInverseOp);
    }
    
    public UsdGeomXformOp AddTranslateOp(XformOpPrecision precision = XformOpPrecision.Double,
                                         TfToken opSuffix = default,
                                         bool isInverseOp = false)
    {
        return AddXformOp(XformOpType.Translate, precision, opSuffix, isInverseOp);
    }
    
    public UsdGeomXformOp AddScaleOp(XformOpPrecision precision = XformOpPrecision.Float,
                                     TfToken opSuffix = default,
                                     bool isInverseOp = false)
    {
        return AddXformOp(XformOpType.Scale, precision, opSuffix, isInverseOp);
    }
    
    public UsdGeomXformOp AddRotateXYZOp(XformOpPrecision precision = XformOpPrecision.Float,
                                         TfToken opSuffix = default,
                                         bool isInverseOp = false)
    {
        return AddXformOp(XformOpType.RotateXYZ, precision, opSuffix, isInverseOp);
    }
    
    public UsdGeomXformOp AddTransformOp(XformOpPrecision precision = XformOpPrecision.Double,
                                         TfToken opSuffix = default,
                                         bool isInverseOp = false)
    {
        return AddXformOp(XformOpType.Transform, precision, opSuffix, isInverseOp);
    }
    
    public List<UsdGeomXformOp> GetOrderedXformOps(out bool resetsXformStack)
    {
        resetsXformStack = false;
        var ops = new List<UsdGeomXformOp>();
        
        var orderAttr = GetXformOpOrderAttr();
        if (!orderAttr.IsValid() || !orderAttr.HasValue())
            return ops;
            
        // Try to get the order as string array first, then as List<string>
        List<string>? opOrder = null;
        if (orderAttr.Get(out string[] orderArray))
        {
            opOrder = orderArray.ToList();
        }
        else if (orderAttr.Get(out List<string> orderList))
        {
            opOrder = orderList;
        }
        
        if (opOrder == null)
            return ops;
            
        foreach (var opToken in opOrder)
        {
            if (opToken == "!resetXformStack!")
            {
                resetsXformStack = true;
                ops.Clear(); // Clear any ops before reset
                continue;
            }
            
            var (opType, suffix, isInverse) = ParseXformOpToken(opToken);
            if (opType == XformOpType.Invalid)
                continue;
                
            var opName = GetXformOpAttrName(opType, suffix, isInverse);
            var attr = GetAttribute(opName);
            if (attr.IsValid())
            {
                ops.Add(new UsdGeomXformOp(attr, opType, isInverse));
            }
        }
        
        return ops;
    }
    
    public bool GetLocalTransformation(out GfMatrix4d transform, 
                                       out bool resetsXformStack,
                                       UsdTimeCode time = default)
    {
        var ops = GetOrderedXformOps(out resetsXformStack);
        return GetLocalTransformation(out transform, ops, time);
    }
    
    public static bool GetLocalTransformation(out GfMatrix4d transform,
                                              List<UsdGeomXformOp> ops,
                                              UsdTimeCode time = default)
    {
        transform = GfMatrix4d.Identity;
        
        foreach (var op in ops)
        {
            var opTransform = op.GetOpTransform(time);
            transform = transform * opTransform;
        }
        
        return true;
    }
    
    public UsdGeomXformOp MakeMatrixXform()
    {
        ClearXformOpOrder();
        return AddTransformOp();
    }
    
    public bool ClearXformOpOrder()
    {
        var orderAttr = GetXformOpOrderAttr();
        if (orderAttr.IsValid())
        {
            return orderAttr.Clear();
        }
        return true;
    }
    
    public bool SetResetXformStack(bool resetXform)
    {
        var orderAttr = CreateXformOpOrderAttr();
        
        // Get current order if it exists
        var currentOrder = new List<string>();
        if (orderAttr.IsValid() && orderAttr.HasValue())
        {
            if (orderAttr.Get(out string[] orderArray))
            {
                currentOrder = orderArray.ToList();
            }
            else if (orderAttr.Get(out List<string> orderList))
            {
                currentOrder = orderList;
            }
        }
        
        // Remove any existing reset tokens
        currentOrder = currentOrder.Where(op => op != "!resetXformStack!").ToList();
        
        // Add reset token if requested
        if (resetXform)
        {
            currentOrder.Insert(0, "!resetXformStack!");
        }
        
        return orderAttr.Set(new VtValue(currentOrder.ToArray()));
    }
    
    public bool GetResetXformStack()
    {
        var orderAttr = GetXformOpOrderAttr();
        if (!orderAttr.IsValid() || !orderAttr.HasValue())
            return false;
            
        // Try to get the order as string array first, then as List<string>
        if (orderAttr.Get(out string[] orderArray))
        {
            return orderArray.Contains("!resetXformStack!");
        }
        else if (orderAttr.Get(out List<string> orderList))
        {
            return orderList.Contains("!resetXformStack!");
        }
        
        return false;
    }
    
    #endregion
    
    #region Helper Methods
    
    private TfToken GetXformOpAttrName(XformOpType opType, TfToken suffix, bool isInverse)
    {
        var baseName = $"xformOp:{GetXformOpTypeString(opType)}";
        if (!suffix.IsEmpty)
            baseName += $":{suffix.GetText()}";
        return new TfToken(baseName);
    }
    
    private TfToken GetXformOpOrderToken(XformOpType opType, TfToken suffix, bool isInverse)
    {
        var token = GetXformOpAttrName(opType, suffix, false);
        if (isInverse)
            return new TfToken($"!invert!{token.GetText()}");
        return token;
    }
    
    private string GetXformOpTypeString(XformOpType opType) => opType switch
    {
        XformOpType.Translate => "translate",
        XformOpType.TranslateX => "translateX",
        XformOpType.TranslateY => "translateY",
        XformOpType.TranslateZ => "translateZ",
        XformOpType.Scale => "scale",
        XformOpType.ScaleX => "scaleX",
        XformOpType.ScaleY => "scaleY",
        XformOpType.ScaleZ => "scaleZ",
        XformOpType.RotateX => "rotateX",
        XformOpType.RotateY => "rotateY",
        XformOpType.RotateZ => "rotateZ",
        XformOpType.RotateXYZ => "rotateXYZ",
        XformOpType.RotateXZY => "rotateXZY",
        XformOpType.RotateYXZ => "rotateYXZ",
        XformOpType.RotateYZX => "rotateYZX",
        XformOpType.RotateZXY => "rotateZXY",
        XformOpType.RotateZYX => "rotateZYX",
        XformOpType.Orient => "orient",
        XformOpType.Transform => "transform",
        _ => "invalid"
    };
    
    private string GetXformOpTypeName(XformOpType opType, XformOpPrecision precision) => opType switch
    {
        XformOpType.Translate => precision == XformOpPrecision.Float ? "float3" : "double3",
        XformOpType.TranslateX or XformOpType.TranslateY or XformOpType.TranslateZ => 
            precision == XformOpPrecision.Float ? "float" : "double",
        XformOpType.Scale => precision == XformOpPrecision.Float ? "float3" : "double3",
        XformOpType.ScaleX or XformOpType.ScaleY or XformOpType.ScaleZ => 
            precision == XformOpPrecision.Float ? "float" : "double",
        XformOpType.RotateX or XformOpType.RotateY or XformOpType.RotateZ => 
            precision == XformOpPrecision.Float ? "float" : "double",
        XformOpType.RotateXYZ or XformOpType.RotateXZY or XformOpType.RotateYXZ or 
        XformOpType.RotateYZX or XformOpType.RotateZXY or XformOpType.RotateZYX => 
            precision == XformOpPrecision.Float ? "float3" : "double3",
        XformOpType.Orient => precision == XformOpPrecision.Float ? "quatf" : "quatd",
        XformOpType.Transform => "matrix4d", // Always double precision for matrices
        _ => "unknown"
    };
    
    private (XformOpType opType, TfToken suffix, bool isInverse) ParseXformOpToken(string token)
    {
        var isInverse = false;
        var workingToken = token;
        
        if (token.StartsWith("!invert!"))
        {
            isInverse = true;
            workingToken = token.Substring(8);
        }
        
        if (!workingToken.StartsWith("xformOp:"))
            return (XformOpType.Invalid, TfToken.Empty, false);
            
        var parts = workingToken.Substring(8).Split(':');
        var opTypeStr = parts[0];
        var suffix = parts.Length > 1 ? new TfToken(string.Join(":", parts.Skip(1))) : TfToken.Empty;
        
        var opType = opTypeStr switch
        {
            "translate" => XformOpType.Translate,
            "translateX" => XformOpType.TranslateX,
            "translateY" => XformOpType.TranslateY,
            "translateZ" => XformOpType.TranslateZ,
            "scale" => XformOpType.Scale,
            "scaleX" => XformOpType.ScaleX,
            "scaleY" => XformOpType.ScaleY,
            "scaleZ" => XformOpType.ScaleZ,
            "rotateX" => XformOpType.RotateX,
            "rotateY" => XformOpType.RotateY,
            "rotateZ" => XformOpType.RotateZ,
            "rotateXYZ" => XformOpType.RotateXYZ,
            "rotateXZY" => XformOpType.RotateXZY,
            "rotateYXZ" => XformOpType.RotateYXZ,
            "rotateYZX" => XformOpType.RotateYZX,
            "rotateZXY" => XformOpType.RotateZXY,
            "rotateZYX" => XformOpType.RotateZYX,
            "orient" => XformOpType.Orient,
            "transform" => XformOpType.Transform,
            _ => XformOpType.Invalid
        };
        
        return (opType, suffix, isInverse);
    }
    
    #endregion
    
    #region Static Factory Methods
    
    
    #endregion
}