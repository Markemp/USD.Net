namespace Pxr.Usd.Sdf;

/// <summary>
/// Represents the shape/dimensions of a value type - matches C++ SdfTupleDimensions behavior.
/// </summary>
public readonly record struct SdfTupleDimensions : IEquatable<SdfTupleDimensions>
{
    public static readonly SdfTupleDimensions Scalar = new();

    private readonly int[]? _dimensions;

    public SdfTupleDimensions()
    {
        Size = 0;
        _dimensions = null;
    }

    public SdfTupleDimensions(int dimension1)
    {
        Size = 1;
        _dimensions = [dimension1];
    }

    public SdfTupleDimensions(int dimension1, int dimension2)
    {
        Size = 2;
        _dimensions = [dimension1, dimension2];
    }

    public SdfTupleDimensions(params int[] dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);
        Size = dimensions.Length;
        _dimensions = dimensions.Length > 0 ? dimensions.ToArray() : null;
    }

    public int Size { get; init; }

    public int[] Dimensions => _dimensions?.ToArray() ?? Array.Empty<int>();

    public bool IsScalar => Size == 0;
    public bool IsVector => Size == 1;
    public bool IsMatrix => Size == 2;

    /// <summary>
    /// Access dimensions by index (matches C++ d[index])
    /// </summary>
    public int this[int index] => index >= 0 && index < Size && _dimensions != null
        ? _dimensions[index]
        : throw new IndexOutOfRangeException($"Index {index} is out of range for dimensions of size {Size}");

    public bool Equals(SdfTupleDimensions other)
    {
        if (Size != other.Size) return false;

        if (Size == 0) return true;

        if (_dimensions == null || other._dimensions == null)
            return _dimensions == other._dimensions;

        for (int i = 0; i < Size; i++)
        {
            if (_dimensions[i] != other._dimensions[i])
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (Size == 0) return 0;

        var hash = new HashCode();
        hash.Add(Size);

        if (_dimensions is not null)
        {
            for (int i = 0; i < Size; i++)
            {
                hash.Add(_dimensions[i]);
            }
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return Size switch
        {
            0 => "Scalar",
            1 => $"Vector[{this[0]}]",
            2 => $"Matrix[{this[0]}, {this[1]}]",
            _ => $"Dimensions[{string.Join(", ", Dimensions)}]"
        };
    }
}