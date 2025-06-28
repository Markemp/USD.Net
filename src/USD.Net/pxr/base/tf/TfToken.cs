using System;

namespace Pxr.Base.Tf;

/// <summary>
/// TfToken is a lightweight, efficient string-like type used for representing identifiers and names efficiently.
/// It provides fast equality comparison and low memory overhead through string interning.
/// </summary>
public readonly struct TfToken : IEquatable<TfToken>
{
    private readonly string? _value;

    public TfToken(string? value)
    {
        _value = string.IsInterned(value ?? string.Empty) ?? string.Intern(value ?? string.Empty);
    }

    public TfToken(ReadOnlySpan<char> value)
    {
        var str = value.ToString();
        _value = string.IsInterned(str) ?? string.Intern(str);
    }

    public static implicit operator TfToken(string? value) => new(value);
    public static implicit operator string(TfToken token) => token._value ?? string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(_value);
    
    public string GetText() => _value ?? string.Empty;

    public bool Equals(TfToken other) => ReferenceEquals(_value, other._value);

    public override bool Equals(object? obj) => obj is TfToken other && Equals(other);

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    public override string ToString() => _value ?? string.Empty;

    public static bool operator ==(TfToken left, TfToken right) => left.Equals(right);
    public static bool operator !=(TfToken left, TfToken right) => !left.Equals(right);

    public static readonly TfToken Empty = new(string.Empty);
}