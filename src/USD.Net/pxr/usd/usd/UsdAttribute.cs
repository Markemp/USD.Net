using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdAttribute is a scenegraph object for authoring and retrieving numeric, string, and array valued data,
/// sampled over time, or animated by a spline.
/// </summary>
public class UsdAttribute : UsdProperty
{
    private readonly Dictionary<UsdTimeCode, VtValue> _timeSamples = new();
    private VtValue? _defaultValue;
    private string _typeName = string.Empty;
    private UsdVariability _variability = UsdVariability.Varying;
    private string _colorSpace = string.Empty;
    private bool _isBlocked;

    #region Construction

    /// <summary>
    /// Create an invalid attribute.
    /// </summary>
    public UsdAttribute() : base()
    {
    }

    /// <summary>
    /// Create an attribute with stage and path.
    /// </summary>
    public UsdAttribute(UsdStage stage, SdfPath path) : base(stage, path)
    {
    }

    /// <summary>
    /// Create an attribute with stage, path, and type.
    /// </summary>
    public UsdAttribute(UsdStage stage, SdfPath path, string typeName) : base(stage, path)
    {
        _typeName = typeName ?? string.Empty;
    }

    #endregion

    #region Type Information

    /// <summary>
    /// Get the type name of this attribute.
    /// </summary>
    public virtual string GetTypeName()
    {
        return _typeName;
    }

    /// <summary>
    /// Set the type name of this attribute.
    /// </summary>
    public virtual bool SetTypeName(string typeName)
    {
        _typeName = typeName ?? string.Empty;
        return true;
    }

    /// <summary>
    /// Return true if this attribute has a type name.
    /// </summary>
    public virtual bool HasTypeName()
    {
        return !string.IsNullOrEmpty(_typeName);
    }

    #endregion

    #region Variability

    /// <summary>
    /// Get the variability of this attribute.
    /// </summary>
    public virtual UsdVariability GetVariability()
    {
        return _variability;
    }

    /// <summary>
    /// Set the variability of this attribute.
    /// </summary>
    public virtual bool SetVariability(UsdVariability variability)
    {
        _variability = variability;
        return true;
    }

    /// <summary>
    /// Return true if this attribute varies over time.
    /// </summary>
    public virtual bool ValueMightBeTimeVarying()
    {
        return _variability == UsdVariability.Varying && _timeSamples.Count > 1;
    }

    #endregion

    #region Value Operations

    /// <summary>
    /// Get the value of this attribute at the given time.
    /// </summary>
    public virtual bool Get<T>(out T value, UsdTimeCode time = default)
    {
        value = default!;

        if (_isBlocked)
            return false;

        // If no time specified, use default time
        if (time.IsDefault())
            time = UsdTimeCode.Default();

        // For uniform attributes or default time, return default value
        if (_variability == UsdVariability.Uniform || time.IsDefault())
        {
            if (_defaultValue != null)
            {
                try
                {
                    value = _defaultValue.Get<T>();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        // For time-varying attributes, interpolate between samples
        if (_timeSamples.Count == 0)
            return false;

        // Find closest time samples for interpolation
        var timeValue = time.GetValue();
        var closestSample = _timeSamples.Keys
            .Where(t => !t.IsDefault())
            .OrderBy(t => Math.Abs(t.GetValue() - timeValue))
            .FirstOrDefault();

        if (closestSample.IsDefault())
            return false;

        try
        {
            value = _timeSamples[closestSample].Get<T>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the value of this attribute as VtValue at the given time.
    /// </summary>
    public virtual bool Get(out VtValue value, UsdTimeCode time = default)
    {
        value = VtValue.CreateEmpty();

        if (_isBlocked)
            return false;

        if (time.IsDefault())
            time = UsdTimeCode.Default();

        // For uniform attributes or default time, return default value
        if (_variability == UsdVariability.Uniform || time.IsDefault())
        {
            if (_defaultValue != null)
            {
                value = _defaultValue;
                return true;
            }
            return false;
        }

        // For time-varying attributes
        if (_timeSamples.TryGetValue(time, out var timeValue))
        {
            value = timeValue;
            return true;
        }

        // TODO: Implement proper time interpolation between samples
        var timeDouble = time.GetValue();
        var closestSample = _timeSamples.Keys
            .Where(t => !t.IsDefault())
            .OrderBy(t => Math.Abs(t.GetValue() - timeDouble))
            .FirstOrDefault();

        if (closestSample.IsDefault())
            return false;

        value = _timeSamples[closestSample];
        return true;
    }

    /// <summary>
    /// Set the value of this attribute at the given time.
    /// </summary>
    public virtual bool Set<T>(T value, UsdTimeCode time = default)
    {
        return Set(new VtValue(value), time);
    }

    /// <summary>
    /// Set the value of this attribute as VtValue at the given time.
    /// </summary>
    public virtual bool Set(VtValue value, UsdTimeCode time = default)
    {
        if (time.IsDefault())
        {
            // Setting default value
            _defaultValue = value;
        }
        else
        {
            // Setting time sample
            _timeSamples[time] = value;
        }

        _isBlocked = false;
        return true;
    }

    /// <summary>
    /// Return true if this attribute has a value (either default or time samples).
    /// </summary>
    public virtual bool HasValue()
    {
        return !_isBlocked && (_defaultValue != null || _timeSamples.Count > 0);
    }

    /// <summary>
    /// Return true if this attribute has authored time samples.
    /// </summary>
    public virtual bool HasAuthoredTimeSamples()
    {
        return _timeSamples.Count > 0;
    }

    /// <summary>
    /// Return true if this attribute has a fallback value.
    /// </summary>
    public virtual bool HasFallbackValue()
    {
        // TODO: Check schema definition for fallback value
        return false;
    }

    #endregion

    #region Time Samples

    /// <summary>
    /// Get all authored time sample times for this attribute.
    /// </summary>
    public virtual double[] GetTimeSamples()
    {
        return _timeSamples.Keys
            .Where(t => !t.IsDefault())
            .Select(t => t.GetValue())
            .OrderBy(t => t)
            .ToArray();
    }

    /// <summary>
    /// Get time samples within a given interval.
    /// </summary>
    public virtual double[] GetTimeSamplesInInterval(UsdTimeCode startTime, UsdTimeCode endTime)
    {
        var start = startTime.GetValue();
        var end = endTime.GetValue();
        
        return _timeSamples.Keys
            .Where(t => !t.IsDefault())
            .Select(t => t.GetValue())
            .Where(t => t >= start && t <= end)
            .OrderBy(t => t)
            .ToArray();
    }

    /// <summary>
    /// Get the number of time samples authored for this attribute.
    /// </summary>
    public virtual int GetNumTimeSamples()
    {
        return _timeSamples.Count;
    }

    /// <summary>
    /// Get the bracketing time samples around the given time.
    /// </summary>
    public virtual bool GetBracketingTimeSamples(double time, out double lower, out double upper, out bool hasTimeSamples)
    {
        lower = double.NaN;
        upper = double.NaN;
        hasTimeSamples = false;

        var samples = GetTimeSamples();
        if (samples.Length == 0)
            return false;

        hasTimeSamples = true;

        // Find bracketing samples
        var lowerSamples = samples.Where(t => t <= time).ToArray();
        var upperSamples = samples.Where(t => t >= time).ToArray();

        if (lowerSamples.Length > 0)
            lower = lowerSamples.Max();

        if (upperSamples.Length > 0)
            upper = upperSamples.Min();

        return true;
    }

    #endregion

    #region Blocking and Clearing

    /// <summary>
    /// Block this attribute.
    /// </summary>
    public virtual bool Block()
    {
        _isBlocked = true;
        return true;
    }

    /// <summary>
    /// Return true if this attribute is blocked.
    /// </summary>
    public virtual bool IsBlocked()
    {
        return _isBlocked;
    }

    /// <summary>
    /// Clear all authored values for this attribute.
    /// </summary>
    public virtual bool Clear()
    {
        _defaultValue = null;
        _timeSamples.Clear();
        _isBlocked = false;
        return true;
    }

    /// <summary>
    /// Clear the default value for this attribute.
    /// </summary>
    public virtual bool ClearDefault()
    {
        _defaultValue = null;
        return true;
    }

    /// <summary>
    /// Clear all time samples for this attribute.
    /// </summary>
    public virtual bool ClearAtTime(UsdTimeCode time)
    {
        if (time.IsDefault())
        {
            _defaultValue = null;
        }
        else
        {
            _timeSamples.Remove(time);
        }
        return true;
    }

    #endregion

    #region Color Space

    /// <summary>
    /// Get the color space for this attribute.
    /// </summary>
    public virtual string GetColorSpace()
    {
        return _colorSpace;
    }

    /// <summary>
    /// Set the color space for this attribute.
    /// </summary>
    public virtual bool SetColorSpace(string colorSpace)
    {
        _colorSpace = colorSpace ?? string.Empty;
        return true;
    }

    /// <summary>
    /// Return true if this attribute has a color space.
    /// </summary>
    public virtual bool HasColorSpace()
    {
        return !string.IsNullOrEmpty(_colorSpace);
    }

    /// <summary>
    /// Clear the color space for this attribute.
    /// </summary>
    public virtual bool ClearColorSpace()
    {
        _colorSpace = string.Empty;
        return true;
    }

    #endregion

    #region Connections

    /// <summary>
    /// Add a connection to this attribute.
    /// </summary>
    public virtual bool AddConnection(SdfPath sourcePath, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        // TODO: Implement connection management
        return false;
    }

    /// <summary>
    /// Remove a connection from this attribute.
    /// </summary>
    public virtual bool RemoveConnection(SdfPath sourcePath)
    {
        // TODO: Implement connection management
        return false;
    }

    /// <summary>
    /// Get all connections for this attribute.
    /// </summary>
    public virtual SdfPath[] GetConnections()
    {
        // TODO: Implement connection management
        return Array.Empty<SdfPath>();
    }

    /// <summary>
    /// Return true if this attribute has connections.
    /// </summary>
    public virtual bool HasConnections()
    {
        // TODO: Implement connection management
        return false;
    }

    /// <summary>
    /// Clear all connections for this attribute.
    /// </summary>
    public virtual bool ClearConnections()
    {
        // TODO: Implement connection management
        return true;
    }

    #endregion
}

/// <summary>
/// Enumeration for attribute variability.
/// </summary>
public enum UsdVariability
{
    /// <summary>
    /// Attribute varies over time.
    /// </summary>
    Varying,
    
    /// <summary>
    /// Attribute is uniform (constant over time).
    /// </summary>
    Uniform
}

/// <summary>
/// Enumeration for list editing positions.
/// </summary>
public enum UsdListPosition
{
    /// <summary>
    /// Add to the front of the prepend list.
    /// </summary>
    FrontOfPrependList,
    
    /// <summary>
    /// Add to the back of the prepend list.
    /// </summary>
    BackOfPrependList,
    
    /// <summary>
    /// Add to the front of the append list.
    /// </summary>
    FrontOfAppendList,
    
    /// <summary>
    /// Add to the back of the append list.
    /// </summary>
    BackOfAppendList
}