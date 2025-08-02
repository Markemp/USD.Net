using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

public class UsdAttribute : UsdProperty, IUsdAttribute
{
    private readonly Dictionary<UsdTimeCode, VtValue> _timeSamples = new();
    private readonly List<ISdfPath> _connections = new();
    private VtValue? _defaultValue;
    private SdfValueTypeName _typeName = SdfValueTypeName.Invalid;
    private SdfVariability _variability = SdfVariability.Varying;
    private TfToken _colorSpace = TfToken.Empty;
    private bool _isBlocked;

    #region Construction

    public UsdAttribute() : base()
    {
    }

    public UsdAttribute(UsdStage stage, ISdfPath path) : base(stage, path)
    {
    }

    public UsdAttribute(UsdStage stage, ISdfPath path, SdfValueTypeName typeName) : base(stage, path)
    {
        _typeName = typeName;
    }

    #endregion

    #region Type Information

    public virtual SdfValueTypeName GetTypeName() => _typeName;

    public virtual bool SetTypeName(SdfValueTypeName typeName)
    {
        _typeName = typeName;
        return true;
    }


    #endregion

    #region Variability

    public virtual SdfVariability GetVariability() => _variability;

    public virtual bool SetVariability(SdfVariability variability)
    {
        _variability = variability;
        return true;
    }

    public virtual bool ValueMightBeTimeVarying()
        => _variability == SdfVariability.Varying && _timeSamples.Count > 1;

    #endregion

    #region Value Operations

    public virtual bool Get<T>(out T value)
    {
        return Get(out value, UsdTimeCode.Default());
    }

    public virtual bool Get<T>(out T value, UsdTimeCode time)
    {
        value = default!;

        if (_isBlocked)
            return false;

        // For uniform attributes or default time, return default value
        if (_variability == SdfVariability.Uniform || time.IsDefault())
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

        // For time-varying attributes, use proper interpolation
        if (GetInterpolatedValue(time, out var vtValue))
        {
            try
            {
                value = vtValue.Get<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public virtual bool Get(out VtValue value)
    {
        return Get(out value, UsdTimeCode.Default());
    }

    public virtual bool Get(out VtValue value, UsdTimeCode time)
    {
        value = VtValue.CreateEmpty();

        if (_isBlocked)
            return false;

        // For uniform attributes or default time, return default value
        if (_variability == SdfVariability.Uniform || time.IsDefault())
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

        // Implement proper time interpolation between samples
        return GetInterpolatedValue(time, out value);
    }

    private bool GetInterpolatedValue(UsdTimeCode time, out VtValue value)
    {
        value = VtValue.CreateEmpty();

        if (!GetBracketingTimeSamples(time.GetValue(), out var lower, out var upper, out var hasTimeSamples))
        {
            // No time samples, try default value
            if (_defaultValue != null)
            {
                value = _defaultValue;
                return true;
            }
            return false;
        }


        var lowerTime = UsdTimeCode.Create(lower);
        var upperTime = UsdTimeCode.Create(upper);

        // Get the bracketing values
        if (!_timeSamples.TryGetValue(lowerTime, out var lowerValue))
            lowerValue = VtValue.CreateEmpty();
        if (!_timeSamples.TryGetValue(upperTime, out var upperValue))
            upperValue = VtValue.CreateEmpty();


        // If we have exact time match
        if (Math.Abs(lower - time.GetValue()) < UsdTimeCode.SafeStep())
        {
            value = lowerValue;
            return !lowerValue.IsEmpty();
        }
        if (Math.Abs(upper - time.GetValue()) < UsdTimeCode.SafeStep())
        {
            value = upperValue;
            return !upperValue.IsEmpty();
        }

        // Interpolate between values
        if (Math.Abs(upper - lower) < UsdTimeCode.SafeStep())
        {
            // Same time samples, use either one
            value = lowerValue.IsEmpty() ? upperValue : lowerValue;
            return !value.IsEmpty();
        }

        // Calculate parametric time
        var alpha = (time.GetValue() - lower) / (upper - lower);

        // TODO: Use stage interpolation setting - for now default to held
        var interpolationType = UsdInterpolationType.Held;

        return UsdInterpolation.Interpolate(lowerValue, upperValue, alpha, interpolationType, out value);
    }

    public virtual bool Set<T>(T value)
    {
        return Set(new VtValue(value), UsdTimeCode.Default());
    }

    public virtual bool Set<T>(T value, UsdTimeCode time)
    {
        return Set(new VtValue(value), time);
    }

    public virtual bool Set(VtValue value)
    {
        return Set(value, UsdTimeCode.Default());
    }

    public virtual bool Set(VtValue value, UsdTimeCode time)
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

    public virtual bool HasAuthoredValue()
    {
        return !_isBlocked && (_defaultValue != null || _timeSamples.Count > 0);
    }

    public virtual bool HasFallbackValue()
    {
        // TODO: Check schema definition for fallback value
        return false;
    }

    public virtual TfToken GetRoleName()
    {
        // TODO: Extract role from type name
        return TfToken.Empty;
    }

    public virtual bool SetConnections(IEnumerable<ISdfPath> sources)
    {
        if (!IsValid())
            return false;
            
        _connections.Clear();
        if (sources != null)
        {
            _connections.AddRange(sources.Where(s => !s.IsEmpty()));
        }
        return true;
    }

    #endregion

    #region Time Samples

    public virtual bool GetTimeSamples(out IReadOnlyList<double> times)
    {
        var sampleTimes = _timeSamples.Keys
            .Where(t => !t.IsDefault())
            .Select(t => t.GetValue())
            .OrderBy(t => t)
            .ToList();
        times = sampleTimes.AsReadOnly();
        return true;
    }

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
    public virtual int GetNumTimeSamples() => _timeSamples.Count;

    /// <summary>
    /// Get the bracketing time samples around the given time.
    /// </summary>
    public virtual bool GetBracketingTimeSamples(double time, out double lower, out double upper, out bool hasTimeSamples)
    {
        lower = double.NaN;
        upper = double.NaN;
        hasTimeSamples = false;

        // Use the out parameter version of GetTimeSamples
        if (!GetTimeSamples(out IReadOnlyList<double> samples))
            return false;

        if (samples.Count == 0)
            return false;

        hasTimeSamples = true;

        // Find bracketing samples
        var lowerSamples = samples.Where(t => t <= time).ToList();
        var upperSamples = samples.Where(t => t >= time).ToList();

        if (lowerSamples.Count > 0)
            lower = lowerSamples.Max();

        if (upperSamples.Count > 0)
            upper = upperSamples.Min();

        return true;
    }

    #endregion

    #region Blocking and Clearing

    public virtual void Block()
    {
        _isBlocked = true;
    }

    public virtual bool IsBlocked() => _isBlocked;

    public virtual bool Clear()
    {
        _defaultValue = null;
        _timeSamples.Clear();
        _isBlocked = false;
        return true;
    }

    public virtual bool ClearDefault()
    {
        _defaultValue = null;
        return true;
    }

    public virtual bool ClearAtTime(UsdTimeCode time)
    {
        if (time.IsDefault())
            _defaultValue = null;
        else
            _timeSamples.Remove(time);
        
        return true;
    }

    #endregion

    #region Color Space

    public virtual TfToken GetColorSpace() => _colorSpace;

    public virtual void SetColorSpace(TfToken colorSpace)
    {
        _colorSpace = colorSpace;
    }

    public virtual bool HasColorSpace() => !_colorSpace.IsEmpty;
    
    public virtual bool ClearColorSpace()
    {
        _colorSpace = TfToken.Empty;
        return true;
    }

    #endregion

    #region Connections

    public virtual bool AddConnection(ISdfPath source, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (!IsValid() || source.IsEmpty())
            return false;
            
        // Add connection if not already present
        if (!_connections.Contains(source))
        {
            switch (position)
            {
                case UsdListPosition.FrontOfPrependList:
                case UsdListPosition.FrontOfAppendList:
                    _connections.Insert(0, source);
                    break;
                default:
                    _connections.Add(source);
                    break;
            }
        }
        return true;
    }

    public virtual bool RemoveConnection(ISdfPath source)
    {
        if (!IsValid())
            return false;
            
        return _connections.Remove(source);
    }

    public virtual bool GetConnections(out IReadOnlyList<ISdfPath> sources)
    {
        sources = _connections.AsReadOnly();
        return true;
    }

    public virtual bool HasAuthoredConnections() => _connections.Count > 0;

    public virtual bool ClearConnections()
    {
        if (!IsValid())
            return false;
            
        _connections.Clear();
        return true;
    }

    public override string GetDescription()
    {
        throw new NotImplementedException();
    }

    #endregion
}

