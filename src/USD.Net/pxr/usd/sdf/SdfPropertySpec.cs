namespace Pxr.Usd.Sdf;

/// <summary>
/// Placeholder class for SdfPropertySpec.
/// Represents a property specification in an SDF layer.
/// </summary>
public class SdfPropertySpec
{
    // TODO: Implement SdfPropertySpec functionality
}

/// <summary>
/// Placeholder struct for SdfLayerOffset.
/// Represents a layer offset for time remapping.
/// </summary>
public struct SdfLayerOffset
{
    public double Offset { get; set; }
    public double Scale { get; set; }
    
    public SdfLayerOffset(double offset = 0.0, double scale = 1.0)
    {
        Offset = offset;
        Scale = scale;
    }
}

