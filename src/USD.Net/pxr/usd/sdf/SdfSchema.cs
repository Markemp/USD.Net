using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// Class that provides information about the various scene description fields.
/// This is a singleton that inherits from SdfSchemaBase.
/// </summary>
public sealed class SdfSchema : SdfSchemaBase
{
    private static readonly Lazy<SdfSchema> _instance = new(() => new SdfSchema());
    
    /// <summary>
    /// Gets the singleton instance of SdfSchema.
    /// </summary>
    public static SdfSchema Instance => _instance.Value;
    
    // Private constructor for singleton pattern
    private SdfSchema()
    {
        RegisterStandardTypes();
        RegisterStandardFields();
        RegisterStandardSpecs();
    }
    
    /// <summary>
    /// Register standard value types used in USD.
    /// </summary>
    private void RegisterStandardTypes()
    {
        // Basic types
        RegisterType(new SdfValueTypeName(new TfToken("bool")));
        RegisterType(new SdfValueTypeName(new TfToken("uchar")));
        RegisterType(new SdfValueTypeName(new TfToken("int")));
        RegisterType(new SdfValueTypeName(new TfToken("uint")));
        RegisterType(new SdfValueTypeName(new TfToken("int64")));
        RegisterType(new SdfValueTypeName(new TfToken("uint64")));
        RegisterType(new SdfValueTypeName(new TfToken("half")));
        RegisterType(new SdfValueTypeName(new TfToken("float")));
        RegisterType(new SdfValueTypeName(new TfToken("double")));
        RegisterType(new SdfValueTypeName(new TfToken("string")));
        RegisterType(new SdfValueTypeName(new TfToken("token")));
        RegisterType(new SdfValueTypeName(new TfToken("asset")));
        RegisterType(new SdfValueTypeName(new TfToken("matrix2d")));
        RegisterType(new SdfValueTypeName(new TfToken("matrix3d")));
        RegisterType(new SdfValueTypeName(new TfToken("matrix4d")));
        RegisterType(new SdfValueTypeName(new TfToken("quatd")));
        RegisterType(new SdfValueTypeName(new TfToken("quatf")));
        RegisterType(new SdfValueTypeName(new TfToken("quath")));
        RegisterType(new SdfValueTypeName(new TfToken("double2")));
        RegisterType(new SdfValueTypeName(new TfToken("float2")));
        RegisterType(new SdfValueTypeName(new TfToken("half2")));
        RegisterType(new SdfValueTypeName(new TfToken("int2")));
        RegisterType(new SdfValueTypeName(new TfToken("double3")));
        RegisterType(new SdfValueTypeName(new TfToken("float3")));
        RegisterType(new SdfValueTypeName(new TfToken("half3")));
        RegisterType(new SdfValueTypeName(new TfToken("int3")));
        RegisterType(new SdfValueTypeName(new TfToken("point3h")));
        RegisterType(new SdfValueTypeName(new TfToken("point3f")));
        RegisterType(new SdfValueTypeName(new TfToken("point3d")));
        RegisterType(new SdfValueTypeName(new TfToken("vector3h")));
        RegisterType(new SdfValueTypeName(new TfToken("vector3f")));
        RegisterType(new SdfValueTypeName(new TfToken("vector3d")));
        RegisterType(new SdfValueTypeName(new TfToken("normal3h")));
        RegisterType(new SdfValueTypeName(new TfToken("normal3f")));
        RegisterType(new SdfValueTypeName(new TfToken("normal3d")));
        RegisterType(new SdfValueTypeName(new TfToken("color3h")));
        RegisterType(new SdfValueTypeName(new TfToken("color3f")));
        RegisterType(new SdfValueTypeName(new TfToken("color3d")));
        RegisterType(new SdfValueTypeName(new TfToken("double4")));
        RegisterType(new SdfValueTypeName(new TfToken("float4")));
        RegisterType(new SdfValueTypeName(new TfToken("half4")));
        RegisterType(new SdfValueTypeName(new TfToken("int4")));
        RegisterType(new SdfValueTypeName(new TfToken("color4h")));
        RegisterType(new SdfValueTypeName(new TfToken("color4f")));
        RegisterType(new SdfValueTypeName(new TfToken("color4d")));
    }
    
    /// <summary>
    /// Register standard fields used in USD scene description.
    /// </summary>
    private void RegisterStandardFields()
    {
        // Core fields
        RegisterFieldDefinition(SdfFieldKeys.Active, 
            new SdfFieldDefinition(this, SdfFieldKeys.Active, new VtValue(true)));
            
        RegisterFieldDefinition(SdfFieldKeys.Comment, 
            new SdfFieldDefinition(this, SdfFieldKeys.Comment, new VtValue(string.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.Documentation, 
            new SdfFieldDefinition(this, SdfFieldKeys.Documentation, new VtValue(string.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.DisplayName, 
            new SdfFieldDefinition(this, SdfFieldKeys.DisplayName, new VtValue(string.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.Hidden, 
            new SdfFieldDefinition(this, SdfFieldKeys.Hidden, new VtValue(false)));
            
        RegisterFieldDefinition(SdfFieldKeys.Kind, 
            new SdfFieldDefinition(this, SdfFieldKeys.Kind, new VtValue(TfToken.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.Permission, 
            new SdfFieldDefinition(this, SdfFieldKeys.Permission, new VtValue(SdfPermission.Public)));
            
        RegisterFieldDefinition(SdfFieldKeys.Prefix, 
            new SdfFieldDefinition(this, SdfFieldKeys.Prefix, new VtValue(string.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.Suffix, 
            new SdfFieldDefinition(this, SdfFieldKeys.Suffix, new VtValue(string.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.SymmetryFunction, 
            new SdfFieldDefinition(this, SdfFieldKeys.SymmetryFunction, new VtValue(TfToken.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.TypeName, 
            new SdfFieldDefinition(this, SdfFieldKeys.TypeName, new VtValue(TfToken.Empty)));
            
        // Children fields
        RegisterFieldDefinition(SdfChildrenKeys.PrimChildren,
            new SdfFieldDefinition(this, SdfChildrenKeys.PrimChildren, new VtValue())
                .AsChildren());
                
        RegisterFieldDefinition(SdfChildrenKeys.PropertyChildren,
            new SdfFieldDefinition(this, SdfChildrenKeys.PropertyChildren, new VtValue())
                .AsChildren());
                
        RegisterFieldDefinition(SdfChildrenKeys.VariantChildren,
            new SdfFieldDefinition(this, SdfChildrenKeys.VariantChildren, new VtValue())
                .AsChildren());
                
        RegisterFieldDefinition(SdfChildrenKeys.VariantSetChildren,
            new SdfFieldDefinition(this, SdfChildrenKeys.VariantSetChildren, new VtValue())
                .AsChildren());
                
        // Property fields
        RegisterFieldDefinition(SdfFieldKeys.Custom, 
            new SdfFieldDefinition(this, SdfFieldKeys.Custom, new VtValue(false)));
            
        RegisterFieldDefinition(SdfFieldKeys.Default, 
            new SdfFieldDefinition(this, SdfFieldKeys.Default, new VtValue()));
            
        RegisterFieldDefinition(SdfFieldKeys.TimeSamples, 
            new SdfFieldDefinition(this, SdfFieldKeys.TimeSamples, new VtValue()));
            
        RegisterFieldDefinition(SdfFieldKeys.Variability, 
            new SdfFieldDefinition(this, SdfFieldKeys.Variability, new VtValue(SdfVariability.Varying)));
            
        // Relationship fields
        RegisterFieldDefinition(SdfFieldKeys.TargetPaths, 
            new SdfFieldDefinition(this, SdfFieldKeys.TargetPaths, new VtValue(new List<SdfPath>())));
            
        // Composition fields
        RegisterFieldDefinition(SdfFieldKeys.References, 
            new SdfFieldDefinition(this, SdfFieldKeys.References, new VtValue()));
            
        RegisterFieldDefinition(SdfFieldKeys.Payload, 
            new SdfFieldDefinition(this, SdfFieldKeys.Payload, new VtValue()));
            
        RegisterFieldDefinition(SdfFieldKeys.Inherits, 
            new SdfFieldDefinition(this, SdfFieldKeys.Inherits, new VtValue()));
            
        RegisterFieldDefinition(SdfFieldKeys.Specializes, 
            new SdfFieldDefinition(this, SdfFieldKeys.Specializes, new VtValue()));
            
        RegisterFieldDefinition(SdfFieldKeys.VariantSelection, 
            new SdfFieldDefinition(this, SdfFieldKeys.VariantSelection, new VtValue()));
            
        // Layer fields
        RegisterFieldDefinition(SdfFieldKeys.DefaultPrim, 
            new SdfFieldDefinition(this, SdfFieldKeys.DefaultPrim, new VtValue(TfToken.Empty)));
            
        RegisterFieldDefinition(SdfFieldKeys.SubLayers, 
            new SdfFieldDefinition(this, SdfFieldKeys.SubLayers, new VtValue()));
    }
    
    /// <summary>
    /// Register standard spec definitions and their allowed fields.
    /// </summary>
    private void RegisterStandardSpecs()
    {
        // PseudoRoot spec (represents the layer root)
        var pseudoRootSpec = new SdfSpecDefinition()
            .AddField(SdfFieldKeys.Comment)
            .AddField(SdfFieldKeys.Documentation)
            .AddField(SdfFieldKeys.DefaultPrim)
            .AddField(SdfFieldKeys.SubLayers)
            .AddMetadataField(SdfFieldKeys.ColorConfiguration)
            .AddMetadataField(SdfFieldKeys.ColorManagementSystem);
        RegisterSpecDefinition(SdfSpecType.PseudoRoot, pseudoRootSpec);
        
        // Prim spec
        var primSpec = new SdfSpecDefinition()
            .AddField(SdfFieldKeys.TypeName, required: true)
            .AddField(SdfFieldKeys.Active)
            .AddField(SdfFieldKeys.Hidden)
            .AddField(SdfFieldKeys.Kind)
            .AddField(SdfFieldKeys.Permission)
            .AddField(SdfFieldKeys.Comment)
            .AddField(SdfFieldKeys.Documentation)
            .AddField(SdfFieldKeys.DisplayName)
            .AddField(SdfFieldKeys.Prefix)
            .AddField(SdfFieldKeys.Suffix)
            .AddField(SdfFieldKeys.References)
            .AddField(SdfFieldKeys.Payload)
            .AddField(SdfFieldKeys.Inherits)
            .AddField(SdfFieldKeys.Specializes)
            .AddField(SdfFieldKeys.VariantSelection)
            .AddField(SdfChildrenKeys.PrimChildren)
            .AddField(SdfChildrenKeys.PropertyChildren)
            .AddField(SdfChildrenKeys.VariantSetChildren)
            .AddMetadataField(SdfFieldKeys.AssetInfo)
            .AddMetadataField(SdfFieldKeys.CustomData);
        RegisterSpecDefinition(SdfSpecType.Prim, primSpec);
        
        // Attribute spec
        var attributeSpec = new SdfSpecDefinition()
            .AddField(SdfFieldKeys.TypeName, required: true)
            .AddField(SdfFieldKeys.Custom)
            .AddField(SdfFieldKeys.Default)
            .AddField(SdfFieldKeys.TimeSamples)
            .AddField(SdfFieldKeys.Variability)
            .AddField(SdfFieldKeys.Comment)
            .AddField(SdfFieldKeys.Documentation)
            .AddField(SdfFieldKeys.DisplayName)
            .AddField(SdfFieldKeys.Hidden)
            .AddMetadataField(SdfFieldKeys.DisplayUnit)
            .AddMetadataField(SdfFieldKeys.CustomData);
        RegisterSpecDefinition(SdfSpecType.Attribute, attributeSpec);
        
        // Relationship spec
        var relationshipSpec = new SdfSpecDefinition()
            .AddField(SdfFieldKeys.TargetPaths)
            .AddField(SdfFieldKeys.Custom)
            .AddField(SdfFieldKeys.Variability)
            .AddField(SdfFieldKeys.Comment)
            .AddField(SdfFieldKeys.Documentation)
            .AddField(SdfFieldKeys.DisplayName)
            .AddField(SdfFieldKeys.Hidden)
            .AddMetadataField(SdfFieldKeys.CustomData);
        RegisterSpecDefinition(SdfSpecType.Relationship, relationshipSpec);
        
        // Variant spec - copies from prim spec
        var variantSpec = new SdfSpecDefinition();
        variantSpec.CopyFrom(primSpec);
        RegisterSpecDefinition(SdfSpecType.Variant, variantSpec);
        
        // VariantSet spec
        var variantSetSpec = new SdfSpecDefinition()
            .AddField(SdfChildrenKeys.VariantChildren);
        RegisterSpecDefinition(SdfSpecType.VariantSet, variantSetSpec);
        
        // Connection spec
        var connectionSpec = new SdfSpecDefinition()
            .AddField(SdfFieldKeys.ConnectionPaths);
        RegisterSpecDefinition(SdfSpecType.Connection, connectionSpec);
        
        // Expression spec
        var expressionSpec = new SdfSpecDefinition()
            .AddField(SdfChildrenKeys.ExpressionChildren);
        RegisterSpecDefinition(SdfSpecType.Expression, expressionSpec);
        
        // Mapper spec
        var mapperSpec = new SdfSpecDefinition()
            .AddField(SdfChildrenKeys.MapperArgChildren)
            .AddField(SdfFieldKeys.TypeName);
        RegisterSpecDefinition(SdfSpecType.Mapper, mapperSpec);
        
        // MapperArg spec
        var mapperArgSpec = new SdfSpecDefinition()
            .AddField(SdfFieldKeys.Default)
            .AddField(SdfFieldKeys.TypeName);
        RegisterSpecDefinition(SdfSpecType.MapperArg, mapperArgSpec);
        
        // RelationshipTarget spec
        var relationshipTargetSpec = new SdfSpecDefinition()
            .AddField(SdfChildrenKeys.RelationshipTargetChildren);
        RegisterSpecDefinition(SdfSpecType.RelationshipTarget, relationshipTargetSpec);
    }
}
