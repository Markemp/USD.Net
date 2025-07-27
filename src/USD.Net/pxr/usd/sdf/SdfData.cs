using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public class SdfData : SdfAbstractData, ISdfData
{
    private readonly Dictionary<ISdfPath, SpecData> _data = [];

    public SdfData() : base() { }

    public override bool StreamsData() => false;

    public override bool HasSpec(ISdfPath path) => _data.ContainsKey(path);

    public override void EraseSpec(ISdfPath path)
    {
        if (!_data.ContainsKey(path))
            return;
        _data.Remove(path);
    }

    public override void MoveSpec(ISdfPath oldPath, ISdfPath newPath)
    {
        if (!_data.TryGetValue(oldPath, out var specData))
            return;

        if (_data.ContainsKey(newPath))
            return;

        _data[newPath] = specData;
        _data.Remove(oldPath);
    }

    public override SdfSpecType GetSpecType(ISdfPath path)
    {
        if (_data.TryGetValue(path, out var specData))
            return specData.SpecType;
        
        return SdfSpecType.Unknown;
    }

    public override void CreateSpec(ISdfPath path, SdfSpecType specType)
    {
        if (specType == SdfSpecType.Unknown)
            return;
        
        _data[path] = new SpecData { SpecType = specType };
    }

    protected override void _VisitSpecs(SdfAbstractDataSpecVisitor visitor)
    {
        foreach (var kvp in _data)
        {
            if (!visitor.VisitSpec(this, kvp.Key))
                break;
        }
    }

    public bool Has(ISdfPath path, TfToken field) => _GetFieldValue(path, field) is not null;

    public override bool Has(ISdfPath path, TfToken field, SdfAbstractDataValue? value = null)
    {
        var fieldValue = _GetFieldValue(path, field);
        if (fieldValue is not null)
        {
            if (value is not null)
                return value.StoreValue(fieldValue);
            
            return true;
        }
        return false;
    }

    public override bool Has(ISdfPath path, TfToken field, VtValue? value = null)
    {
        var fieldValue = _GetFieldValue(path, field);
        if (fieldValue is not null)
        {
            if (value is not null)
                value = fieldValue;

            return true;
        }
        return false;
    }

    public override bool HasSpecAndField(ISdfPath path, TfToken fieldName, SdfAbstractDataValue? value, out SdfSpecType specType)
    {
        var fieldValue = _GetSpecTypeAndFieldValue(path, fieldName, out specType);
        if (fieldValue is not null)
            return value is null || value.StoreValue(fieldValue);

        return false;
    }

    public override bool HasSpecAndField(ISdfPath path, TfToken fieldName, VtValue? value, out SdfSpecType specType)
    {
        var fieldValue = _GetSpecTypeAndFieldValue(path, fieldName, out specType);
        if (fieldValue is not null)
        {
            if (value is not null)
                value = fieldValue;

            return true;
        }
        return false;
    }

    private VtValue? _GetSpecTypeAndFieldValue(ISdfPath path, TfToken field, out SdfSpecType specType)
    {
        if (_data.TryGetValue(path, out var spec))
        {
            specType = spec.SpecType;
            return spec.Fields.TryGetValue(field, out var fieldValue) ? fieldValue : null;
        }
        
        specType = SdfSpecType.Unknown;
        return null;
    }

    private VtValue? _GetFieldValue(ISdfPath path, TfToken field)
    {
        if (_data.TryGetValue(path, out var spec))
            return spec.Fields.TryGetValue(field, out var fieldValue) ? fieldValue : null;

        return null;
    }

    private VtValue? _GetMutableFieldValue(ISdfPath path, TfToken field)
    {
        if (_data.TryGetValue(path, out var spec))
            return spec.Fields.TryGetValue(field, out var fieldValue) ? fieldValue : null;
        
        return null;
    }

    public override VtValue Get(ISdfPath path, TfToken field)
    {
        var value = _GetFieldValue(path, field);
        return value ?? new VtValue();
    }

    public override void Set(ISdfPath path, TfToken field, VtValue value)
    {
        if (value.IsEmpty())
        {
            Erase(path, field);
            return;
        }

        var newValue = _GetOrCreateFieldValue(path, field);
        if (newValue is not null)
            _data[path].Fields[field] = value;
    }

    public override void Set(ISdfPath path, TfToken field, SdfAbstractDataConstValue value)
    {
        var newValue = _GetOrCreateFieldValue(path, field);
        if (newValue is not null)
        {
            // Option 1: Use the parameterless GetValue()
            var vtValue = value.GetValue();
            _data[path].Fields[field] = vtValue;
        }
    }

    private VtValue? _GetOrCreateFieldValue(ISdfPath path, TfToken field)
    {
        if (!_data.TryGetValue(path, out var spec))
            return null;

        if (spec.Fields.TryGetValue(field, out var existing))
            return existing;

        spec.Fields[field] = new VtValue();
        return spec.Fields[field];
    }

    public override void Erase(ISdfPath path, TfToken field)
    {
        if (_data.TryGetValue(path, out var spec))
            spec.Fields.Remove(field);
    }

    public override List<TfToken> List(ISdfPath path)
    {
        var names = new List<TfToken>();
        if (_data.TryGetValue(path, out var spec))
            names.AddRange(spec.Fields.Keys);
        
        return names;
    }

    public override HashSet<double> ListAllTimeSamples()
    {
        var times = new HashSet<double>();
        foreach (var kvp in _data)
        {
            var timesForPath = ListTimeSamplesForPath(kvp.Key);
            times.UnionWith(timesForPath);
        }
        return times;
    }

    public override HashSet<double> ListTimeSamplesForPath(ISdfPath path)
    {
        var times = new HashSet<double>();
        var value = Get(path, new TfToken(SdfDataTokens.TimeSamples));
        if (value.IsHolding<SdfTimeSampleMap>())
        {
            var timeSampleMap = value.Get<SdfTimeSampleMap>();
            foreach (var kvp in timeSampleMap)
            {
                times.Add(kvp.Key);
            }
        }
        return times;
    }

    public override bool GetBracketingTimeSamples(double time, out double tLower, out double tUpper)
        => _GetBracketingTimeSamples(ListAllTimeSamples(), time, out tLower, out tUpper);

    public override int GetNumTimeSamplesForPath(ISdfPath path)
    {
        var fieldValue = _GetFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
            return fieldValue.Get<SdfTimeSampleMap>().Count;
        return 0;
    }

    public override bool GetBracketingTimeSamplesForPath(ISdfPath path, double time, out double tLower, out double tUpper)
    {
        var fieldValue = _GetFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
        {
            var timeSampleMap = fieldValue.Get<SdfTimeSampleMap>();
            return _GetBracketingTimeSamples(timeSampleMap, time, out tLower, out tUpper);
        }
        tLower = tUpper = 0.0;
        return false;
    }

    public override bool GetPreviousTimeSampleForPath(ISdfPath path, double time, out double tPrevious)
    {
        var fieldValue = _GetFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
        {
            var timeSampleMap = fieldValue.Get<SdfTimeSampleMap>();
            if (timeSampleMap.Count == 0 || time <= timeSampleMap.Keys.Min())
            {
                tPrevious = 0.0;
                return false;
            }
            
            if (time > timeSampleMap.Keys.Max())
            {
                tPrevious = timeSampleMap.Keys.Max();
                return true;
            }
            
            var sortedKeys = timeSampleMap.Keys.OrderBy(k => k).ToList();
            for (int i = sortedKeys.Count - 1; i >= 0; i--)
            {
                if (sortedKeys[i] < time)
                {
                    tPrevious = sortedKeys[i];
                    return true;
                }
            }
        }
        tPrevious = 0.0;
        return false;
    }

    public override bool QueryTimeSample(ISdfPath path, double time, SdfAbstractDataValue? optionalValue = null)
    {
        var fieldValue = _GetFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
        {
            var timeSampleMap = fieldValue.Get<SdfTimeSampleMap>();
            if (timeSampleMap.TryGetValue(time, out var value))
                return optionalValue is null || optionalValue.StoreValue(value);
        }
        return false;
    }

    public override bool QueryTimeSample(ISdfPath path, double time, VtValue? value = null)
    {
        var fieldValue = _GetFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
        {
            var timeSampleMap = fieldValue.Get<SdfTimeSampleMap>();
            if (timeSampleMap.TryGetValue(time, out var sampleValue))
            {
                if (value is not null)
                    value = sampleValue;
                
                return true;
            }
        }
        return false;
    }

    public override void SetTimeSample(ISdfPath path, double time, VtValue value)
    {
        if (value.IsEmpty())
        {
            EraseTimeSample(path, time);
            return;
        }

        var newSamples = new SdfTimeSampleMap();
        var fieldValue = _GetMutableFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
        {
            var existingSamples = fieldValue.Get<SdfTimeSampleMap>();
            foreach (var kvp in existingSamples)
            {
                newSamples[kvp.Key] = kvp.Value;
            }
        }
        
        newSamples[time] = value;
        Set(path, new TfToken(SdfDataTokens.TimeSamples), new VtValue(newSamples));
    }

    public override void EraseTimeSample(ISdfPath path, double time)
    {
        var fieldValue = _GetMutableFieldValue(path, new TfToken(SdfDataTokens.TimeSamples));
        if (fieldValue is not null && fieldValue.IsHolding<SdfTimeSampleMap>())
        {
            var existingSamples = fieldValue.Get<SdfTimeSampleMap>();
            var newSamples = new SdfTimeSampleMap();
            
            foreach (var kvp in existingSamples)
            {
                if (kvp.Key != time)
                    newSamples[kvp.Key] = kvp.Value;
            }
            
            if (newSamples.Count == 0)
                Erase(path, new TfToken(SdfDataTokens.TimeSamples));
            else
                Set(path, new TfToken(SdfDataTokens.TimeSamples), new VtValue(newSamples));
        }
    }

    private bool _GetBracketingTimeSamples(IEnumerable<double> samples, double time, out double tLower, out double tUpper)
    {
        var sortedSamples = samples.OrderBy(s => s).ToList();
        
        if (sortedSamples.Count == 0)
        {
            tLower = tUpper = 0.0;
            return false;
        }
        
        if (time <= sortedSamples.First())
        {
            tLower = tUpper = sortedSamples.First();
            return true;
        }
        
        if (time >= sortedSamples.Last())
        {
            tLower = tUpper = sortedSamples.Last();
            return true;
        }
        
        for (int i = 0; i < sortedSamples.Count - 1; i++)
        {
            if (sortedSamples[i] <= time && time <= sortedSamples[i + 1])
            {
                if (sortedSamples[i] == time)
                    tLower = tUpper = sortedSamples[i];
                else if (sortedSamples[i + 1] == time)
                    tLower = tUpper = sortedSamples[i + 1];
                else
                {
                    tLower = sortedSamples[i];
                    tUpper = sortedSamples[i + 1];
                }
                return true;
            }
        }
        
        tLower = tUpper = 0.0;
        return false;
    }

    private bool _GetBracketingTimeSamples(SdfTimeSampleMap samples, double time, out double tLower, out double tUpper)
        => _GetBracketingTimeSamples(samples.Keys, time, out tLower, out tUpper);

    /// <summary>
    /// Static helper method for listing fields (used by SdfLayer)
    /// This matches the C++ SdfLayer::_ListFields implementation
    /// </summary>
    public static List<TfToken> ListFields(ISdfSchemaBase schema, ISdfData data, ISdfPath path)
    {
        // Get the list from the data implementation
        var dataList = data.List(path);

        // Determine spec type. If unknown, return early.
        var specType = data.GetSpecType(path);
        if (specType == SdfSpecType.Unknown)
            return dataList;

        // Get required fields from schema
        var requiredFields = schema.GetRequiredFields(specType);

        // Union them together, but retain order of dataList since it influences
        // the output ordering in some file writers.
        var result = new List<TfToken>(dataList);

        foreach (var requiredField in requiredFields)
        {
            // If the required field name is not already present, append it
            if (!dataList.Contains(requiredField))
                result.Add(requiredField);
        }

        return result;
    }

    private class SpecData
    {
        public SdfSpecType SpecType { get; set; } = SdfSpecType.Unknown;
        public Dictionary<TfToken, VtValue> Fields { get; } = new();
    }
}