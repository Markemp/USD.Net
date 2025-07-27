using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public abstract class SdfAbstractDataSpecVisitor
{
    public abstract bool VisitSpec(ISdfAbstractData data, SdfPath path);
    
    public virtual void Done(ISdfAbstractData data)
    {
    }
}

//public abstract class SdfAbstractDataValue
//{
//    public abstract bool StoreValue(VtValue value);
//}

public abstract class SdfAbstractData : ISdfAbstractData
{
    public SdfAbstractData() { }
    
    ~SdfAbstractData() { }

    public abstract bool StreamsData();
    
    public abstract void CreateSpec(SdfPath path, SdfSpecType specType);
    public abstract bool HasSpec(SdfPath path);
    public bool Equals(ISdfAbstractData? data)
    {
        if (data is null) return false;

        var rhsHasAllSpecsInThis = new CheckAllSpecsExist(data);
        VisitSpecs(rhsHasAllSpecsInThis);
        if (!rhsHasAllSpecsInThis.Passed)
            return false;

        var thisHasAllSpecsInRhs = new CheckAllSpecsExist(this);
        data.VisitSpecs(thisHasAllSpecsInRhs);
        if (!thisHasAllSpecsInRhs.Passed)
            return false;

        var thisSpecsMatchRhsSpecs = new CheckAllSpecsMatch(data);
        VisitSpecs(thisSpecsMatchRhsSpecs);
        return thisSpecsMatchRhsSpecs.Passed;
    }

    public abstract void EraseSpec(SdfPath path);
    public abstract void MoveSpec(SdfPath oldPath, SdfPath newPath);
    public abstract SdfSpecType GetSpecType(SdfPath path);

    public abstract bool Has(SdfPath path, TfToken fieldName, SdfAbstractDataValue? value = null);
    public abstract bool Has(SdfPath path, TfToken fieldName, VtValue? value = null);
    public abstract bool HasSpecAndField(SdfPath path, TfToken fieldName, SdfAbstractDataValue? value, out SdfSpecType specType);
    public abstract bool HasSpecAndField(SdfPath path, TfToken fieldName, VtValue? value, out SdfSpecType specType);

    public abstract VtValue Get(SdfPath path, TfToken fieldName);
    public abstract void Set(SdfPath path, TfToken fieldName, VtValue value);
    public abstract void Set(SdfPath path, TfToken fieldName, SdfAbstractDataConstValue value);
    public abstract void Erase(SdfPath path, TfToken fieldName);
    public abstract List<TfToken> List(SdfPath path);

    public abstract HashSet<double> ListAllTimeSamples();
    public abstract HashSet<double> ListTimeSamplesForPath(SdfPath path);
    public abstract bool GetBracketingTimeSamples(double time, out double tLower, out double tUpper);
    public abstract int GetNumTimeSamplesForPath(SdfPath path);
    public abstract bool GetBracketingTimeSamplesForPath(SdfPath path, double time, out double tLower, out double tUpper);
    public virtual bool GetPreviousTimeSampleForPath(SdfPath path, double time, out double tPrevious)
    {
        tPrevious = 0;
        bool result = GetBracketingTimeSamplesForPath(path, time, out double lower, out double upper);
        if (result)
        {
            if (time < lower)
                return false;

            if (time == lower)
            {
                double prevTime = Math.BitDecrement(time);
                result = GetBracketingTimeSamplesForPath(path, prevTime, out lower, out upper);
                if (!result || time == lower)
                    return false;
            }
            tPrevious = lower;
        }
        return result;
    }
    public abstract bool QueryTimeSample(SdfPath path, double time, SdfAbstractDataValue? optionalValue = null);
    public abstract bool QueryTimeSample(SdfPath path, double time, VtValue? value = null);
    public abstract void SetTimeSample(SdfPath path, double time, VtValue value);
    public abstract void EraseTimeSample(SdfPath path, double time);

    public void WriteToStream(TextWriter stream)
    {
        var collector = new SortedPathCollector();
        VisitSpecs(collector);

        foreach (var path in collector.Paths)
        {
            var specType = GetSpecType(path);
            stream.WriteLine($"{path} {specType}");

            var fields = List(path);
            var fieldSet = new SortedSet<TfToken>(fields);

            foreach (var fieldName in fieldSet)
            {
                var value = Get(path, fieldName);
                stream.WriteLine($"    {fieldName} {value.GetType().Name} {value}");
            }
        }
    }

    public bool IsEmpty()
    {
        var checker = new IsEmptyChecker();
        VisitSpecs(checker);
        return checker.IsEmpty;
    }

    protected abstract void _VisitSpecs(SdfAbstractDataSpecVisitor visitor);
    
    public void VisitSpecs(SdfAbstractDataSpecVisitor visitor)
    {
        _VisitSpecs(visitor);
        visitor.Done(this);
    }

    public void CopyFrom(ISdfAbstractData source)
    {
        var copySpecs = new CopySpecs(this);
        source.VisitSpecs(copySpecs);
    }

    public T GetAs<T>(SdfPath path, TfToken fieldName, T defaultValue = default!)
    {
        var value = Get(path, fieldName);
        if (value.IsHolding<T>())
            return value.Get<T>();
        return defaultValue;
    }

    public bool HasDictKey(SdfPath path, TfToken fieldName, TfToken keyPath, SdfAbstractDataValue? value)
    {
        var tmp = new VtValue();
        bool result = HasDictKey(path, fieldName, keyPath, value != null ? tmp : null);
        
        if (result && value is not null)
            result = value.StoreValue(tmp);
        
        return result;
    }

    public bool HasDictKey(SdfPath path, TfToken fieldName, TfToken keyPath, VtValue? value = null)
    {
        var dictVal = new VtValue();
        if (Has(path, fieldName, dictVal) && dictVal.IsHolding<VtDictionary>())
        {
            var dict = dictVal.Get<VtDictionary>();
            if (dict.TryGetValue(keyPath.ToString(), out var v))
            {
                if (value is not null)
                    return true;
            }
        }
        return false;
    }

    public VtValue GetDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath)
    {
        var result = new VtValue();
        HasDictKey(path, fieldName, keyPath, result);
        return result;
    }

    public void SetDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath, VtValue value)
    {
        if (value.IsEmpty())
        {
            EraseDictValueByKey(path, fieldName, keyPath);
            return;
        }

        var dictVal = Get(path, fieldName);
        var dict = dictVal.IsHolding<VtDictionary>() ? dictVal.Get<VtDictionary>() : [];
        dict[keyPath.ToString()] = (VtValue)value.GetValue()!;
        Set(path, fieldName, VtValue.Create(dict));
    }

    public void SetDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath, SdfAbstractDataConstValue value)
    {
        var vtval = value.GetValue();
        SetDictValueByKey(path, fieldName, keyPath, vtval);
    }

    public void EraseDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath)
    {
        var dictVal = Get(path, fieldName);
        if (dictVal.IsHolding<VtDictionary>())
        {
            var dict = dictVal.Get<VtDictionary>();
            dict.Remove(keyPath.ToString());

            if (dict.Count == 0)
                Erase(path, fieldName);
            else
                Set(path, fieldName, VtValue.Create(dict));
        }
    }

    public List<TfToken> ListDictKeys(SdfPath path, TfToken fieldName, TfToken keyPath)
    {
        var result = new List<TfToken>();
        var dictVal = GetDictValueByKey(path, fieldName, keyPath);
        if (dictVal.IsHolding<VtDictionary>())
        {
            var dict = dictVal.Get<VtDictionary>();
            foreach (var key in dict.Keys)
                result.Add(new TfToken(key));
        }
        return result;
    }

    #region Nested Helper Classes

    private class IsEmptyChecker : SdfAbstractDataSpecVisitor
    {
        public bool IsEmpty { get; private set; } = true;

        public override bool VisitSpec(ISdfAbstractData data, SdfPath path)
        {
            IsEmpty = false;
            return false;
        }
    }

    private class CheckAllSpecsExist : SdfAbstractDataSpecVisitor
    {
        private readonly ISdfAbstractData _data;
        public bool Passed { get; private set; } = true;

        public CheckAllSpecsExist(ISdfAbstractData data) => _data = data;

        public override bool VisitSpec(ISdfAbstractData data, SdfPath path)
        {
            if (!_data.HasSpec(path))
                Passed = false;
            return Passed;
        }
    }

    private class CheckAllSpecsMatch : SdfAbstractDataSpecVisitor
    {
        private readonly ISdfAbstractData _rhs;
        public bool Passed { get; private set; } = true;

        public CheckAllSpecsMatch(ISdfAbstractData rhs) => _rhs = rhs;

        public override bool VisitSpec(ISdfAbstractData lhs, SdfPath path)
        {
            Passed = AreSpecsAtPathEqual(lhs, _rhs, path);
            return Passed;
        }

        private static bool AreSpecsAtPathEqual(ISdfAbstractData lhs, ISdfAbstractData rhs, SdfPath path)
        {
            var lhsFields = lhs.List(path);
            var rhsFields = rhs.List(path);
            var lhsFieldSet = new HashSet<TfToken>(lhsFields);
            var rhsFieldSet = new HashSet<TfToken>(rhsFields);

            if (lhs.GetSpecType(path) != rhs.GetSpecType(path))
                return false;
            if (!lhsFieldSet.SetEquals(rhsFieldSet))
                return false;

            foreach (var field in lhsFields)
            {
                if (!lhs.Get(path, field).Equals(rhs.Get(path, field)))
                    return false;
            }

            return true;
        }
    }

    private class CopySpecs : SdfAbstractDataSpecVisitor
    {
        private readonly ISdfAbstractData _dest;

        public CopySpecs(ISdfAbstractData dest) => _dest = dest;

        public override bool VisitSpec(ISdfAbstractData src, SdfPath path)
        {
            var keys = src.List(path);
            _dest.CreateSpec(path, src.GetSpecType(path));
            foreach (var key in keys)
                _dest.Set(path, key, src.Get(path, key));
            return true;
        }
    }

    private class SortedPathCollector : SdfAbstractDataSpecVisitor
    {
        public SortedSet<SdfPath> Paths { get; } = new SortedSet<SdfPath>();

        public override bool VisitSpec(ISdfAbstractData data, SdfPath path)
        {
            Paths.Add(path);
            return true;
        }
    }

    #endregion
}