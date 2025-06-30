using System;

namespace Pxr.Usd;

/// <summary>
/// UsdTimeCode represents a time coordinate used for time-varying data in USD.
/// It can represent specific time values or special time codes like Default and EarliestTime.
/// </summary>
public readonly struct UsdTimeCode : IEquatable<UsdTimeCode>, IComparable<UsdTimeCode>
{
    private readonly double _value;
    private readonly bool _isDefault;

    private UsdTimeCode(double value, bool isDefault = false)
    {
        _value = value;
        _isDefault = isDefault;
    }

    /// <summary>
    /// Create a time code for a specific time value.
    /// </summary>
    public static UsdTimeCode Create(double time) => new(time);

    /// <summary>
    /// Get the default time code, representing the default time-varying value.
    /// </summary>
    public static UsdTimeCode Default() => new(0.0, true);

    /// <summary>
    /// Get the earliest time code, representing the earliest time sample.
    /// </summary>
    public static UsdTimeCode EarliestTime() => new(double.NegativeInfinity);

    /// <summary>
    /// Get the safe step value for creating distinct time samples.
    /// This provides a reasonable epsilon for floating-point time comparisons.
    /// </summary>
    public static double SafeStep() => 1e-6;

    /// <summary>
    /// Return true if this is the default time code.
    /// </summary>
    public bool IsDefault() => _isDefault;

    /// <summary>
    /// Return true if this represents a numeric time value.
    /// </summary>
    public bool IsNumeric() => !_isDefault && !double.IsInfinity(_value);

    /// <summary>
    /// Return true if this is the earliest time.
    /// </summary>
    public bool IsEarliestTime() => !_isDefault && double.IsNegativeInfinity(_value);

    /// <summary>
    /// Get the numeric time value.
    /// </summary>
    public double GetValue()
    {
        if (_isDefault)
            throw new InvalidOperationException("Cannot get numeric value from default time code");
        return _value;
    }

    public static implicit operator UsdTimeCode(double time) => Create(time);
    public static explicit operator double(UsdTimeCode timeCode) => timeCode.GetValue();

    /// <summary>
    /// Add a time offset to a time code.
    /// </summary>
    public static UsdTimeCode operator +(UsdTimeCode timeCode, double offset)
    {
        if (timeCode._isDefault || timeCode.IsEarliestTime())
            return timeCode; // Special values are unchanged by arithmetic
        return new UsdTimeCode(timeCode._value + offset);
    }

    /// <summary>
    /// Subtract a time offset from a time code.
    /// </summary>
    public static UsdTimeCode operator -(UsdTimeCode timeCode, double offset)
    {
        if (timeCode._isDefault || timeCode.IsEarliestTime())
            return timeCode; // Special values are unchanged by arithmetic
        return new UsdTimeCode(timeCode._value - offset);
    }

    /// <summary>
    /// Get the difference between two time codes.
    /// </summary>
    public static double operator -(UsdTimeCode left, UsdTimeCode right)
    {
        if (left._isDefault || right._isDefault)
            return 0.0; // Default time differences are zero
        return left._value - right._value;
    }

    public bool Equals(UsdTimeCode other)
    {
        if (_isDefault && other._isDefault)
            return true;
        if (_isDefault != other._isDefault)
            return false;
        return _value.Equals(other._value);
    }

    public override bool Equals(object? obj) => obj is UsdTimeCode other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_value, _isDefault);

    public int CompareTo(UsdTimeCode other)
    {
        if (_isDefault && other._isDefault)
            return 0;
        if (_isDefault)
            return -1;
        if (other._isDefault)
            return 1;
        return _value.CompareTo(other._value);
    }

    public override string ToString()
    {
        if (_isDefault)
            return "default";
        if (double.IsNegativeInfinity(_value))
            return "earliest";
        return _value.ToString();
    }

    public static bool operator ==(UsdTimeCode left, UsdTimeCode right) => left.Equals(right);
    public static bool operator !=(UsdTimeCode left, UsdTimeCode right) => !left.Equals(right);
    public static bool operator <(UsdTimeCode left, UsdTimeCode right) => left.CompareTo(right) < 0;
    public static bool operator >(UsdTimeCode left, UsdTimeCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(UsdTimeCode left, UsdTimeCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(UsdTimeCode left, UsdTimeCode right) => left.CompareTo(right) >= 0;
}