namespace Pxr.Usd.Sdf;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates if an operation is allowed and, if not, why not.
/// 
/// A SdfAllowed either evaluates to true in a boolean context
/// or evaluates to false and has a string annotation.
/// </summary>
public readonly struct SdfAllowed : IEquatable<SdfAllowed>
{
    private readonly string? _whyNot;

    /// <summary>
    /// Constructs a successful (allowed) result.
    /// </summary>
    public SdfAllowed()
    {
        _whyNot = null;
    }

    /// <summary>
    /// Constructs a successful result if condition is true.
    /// </summary>
    /// <param name="condition">Must be true</param>
    /// <exception cref="ArgumentException">Thrown if condition is false</exception>
    public SdfAllowed(bool condition)
    {
        if (!condition)
            throw new ArgumentException("Cannot construct SdfAllowed(false) - use constructor with reason", nameof(condition));
        _whyNot = null;
    }

    /// <summary>
    /// Constructs a failed (not allowed) result with the given reason.
    /// </summary>
    /// <param name="whyNot">Reason why the operation is not allowed</param>
    public SdfAllowed(string whyNot)
    {
        _whyNot = whyNot ?? throw new ArgumentNullException(nameof(whyNot));
    }

    /// <summary>
    /// Constructs a result based on condition with annotation if false.
    /// </summary>
    /// <param name="condition">Whether the operation is allowed</param>
    /// <param name="whyNot">Reason if not allowed</param>
    public SdfAllowed(bool condition, string whyNot)
    {
        _whyNot = condition ? null : (whyNot ?? throw new ArgumentNullException(nameof(whyNot)));
    }

    /// <summary>
    /// Constructs from a bool,string pair.
    /// </summary>
    /// <param name="result">Tuple of (success, error message)</param>
    public SdfAllowed((bool Success, string Error) result) : this(result.Success, result.Error)
    {
    }

    /// <summary>
    /// Gets whether the operation is allowed.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_whyNot))]
    public bool IsAllowed => _whyNot is null;

    /// <summary>
    /// Gets the reason why the operation is not allowed.
    /// Returns empty string if the operation is allowed.
    /// </summary>
    public string WhyNot => _whyNot ?? string.Empty;

    /// <summary>
    /// Returns true if allowed, otherwise fills whyNot and returns false.
    /// </summary>
    /// <param name="whyNot">Will contain the error message if operation is not allowed</param>
    /// <returns>True if allowed, false otherwise</returns>
    public bool TryGetError([NotNullWhen(true)] out string? whyNot)
    {
        whyNot = _whyNot;
        return _whyNot is not null;
    }

    /// <summary>
    /// Implicit conversion to bool - returns true if allowed.
    /// </summary>
    /// <param name="allowed">The SdfAllowed instance</param>
    /// <returns>True if allowed, false otherwise</returns>
    public static implicit operator bool(SdfAllowed allowed) => allowed.IsAllowed;

    /// <summary>
    /// Implicit conversion from bool - creates allowed result if true.
    /// </summary>
    /// <param name="success">Must be true</param>
    /// <returns>Successful SdfAllowed instance</returns>
    /// <exception cref="ArgumentException">Thrown if success is false</exception>
    public static implicit operator SdfAllowed(bool success) => new(success);

    /// <summary>
    /// Implicit conversion from string - creates not allowed result.
    /// </summary>
    /// <param name="whyNot">Reason why not allowed</param>
    /// <returns>Failed SdfAllowed instance</returns>
    public static implicit operator SdfAllowed(string whyNot) => new(whyNot);

    /// <summary>
    /// Implicit conversion to string - returns the error message or empty string.
    /// </summary>
    /// <param name="allowed">The SdfAllowed instance</param>
    /// <returns>Error message or empty string if allowed</returns>
    public static implicit operator string(SdfAllowed allowed) => allowed.WhyNot;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>Successful SdfAllowed instance</returns>
    public static SdfAllowed Success() => new();

    /// <summary>
    /// Creates a failed result with the given reason.
    /// </summary>
    /// <param name="whyNot">Reason why the operation failed</param>
    /// <returns>Failed SdfAllowed instance</returns>
    public static SdfAllowed Failure(string whyNot) => new(whyNot);

    /// <summary>
    /// Combines multiple SdfAllowed results. Returns success only if all are successful.
    /// If any fail, returns the first failure reason.
    /// </summary>
    /// <param name="results">Results to combine</param>
    /// <returns>Combined result</returns>
    public static SdfAllowed Combine(params SdfAllowed[] results)
    {
        foreach (var result in results)
        {
            if (!result.IsAllowed)
                return result;
        }
        return Success();
    }

    /// <summary>
    /// Combines multiple SdfAllowed results. Returns success only if all are successful.
    /// If any fail, returns the first failure reason.
    /// </summary>
    /// <param name="results">Results to combine</param>
    /// <returns>Combined result</returns>
    public static SdfAllowed Combine(IEnumerable<SdfAllowed> results)
    {
        foreach (var result in results)
        {
            if (!result.IsAllowed)
                return result;
        }
        return Success();
    }

    public bool Equals(SdfAllowed other) => _whyNot == other._whyNot;

    public override bool Equals(object? obj) => obj is SdfAllowed other && Equals(other);

    public override int GetHashCode() => _whyNot?.GetHashCode() ?? 0;

    public override string ToString() => IsAllowed ? "Allowed" : $"Not allowed: {_whyNot}";

    public static bool operator ==(SdfAllowed left, SdfAllowed right) => left.Equals(right);

    public static bool operator !=(SdfAllowed left, SdfAllowed right) => !left.Equals(right);

    /// <summary>
    /// Deconstructs the SdfAllowed into its components.
    /// </summary>
    /// <param name="isAllowed">Whether the operation is allowed</param>
    /// <param name="whyNot">Reason if not allowed, null if allowed</param>
    public void Deconstruct(out bool isAllowed, out string? whyNot)
    {
        isAllowed = IsAllowed;
        whyNot = _whyNot;
    }
}