using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public abstract class SdfAbstractDataSpecVisitor
{
    public abstract bool VisitSpec(SdfAbstractData data, SdfPath path);
}

public abstract class SdfAbstractDataValue
{
    public abstract bool StoreValue(VtValue value);
}

public abstract class SdfAbstractDataConstValue
{
    public abstract void GetValue(VtValue value);
}

public abstract class SdfAbstractData
{
    public SdfAbstractData() { }
    
    ~SdfAbstractData() { }

    public virtual void CopyFrom(SdfAbstractData source)
    {
        if (source == null) return;
        
        source._VisitSpecs(new CopySpecVisitor(this));
    }

    public abstract bool StreamsData();
    
    public virtual bool IsDetached()
    {
        return !StreamsData();
    }

    public abstract void CreateSpec(SdfPath path, SdfSpecType specType);
    public abstract bool HasSpec(SdfPath path);
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
    public abstract bool GetPreviousTimeSampleForPath(SdfPath path, double time, out double tPrevious);
    public abstract bool QueryTimeSample(SdfPath path, double time, SdfAbstractDataValue? optionalValue = null);
    public abstract bool QueryTimeSample(SdfPath path, double time, VtValue? value = null);
    public abstract void SetTimeSample(SdfPath path, double time, VtValue value);
    public abstract void EraseTimeSample(SdfPath path, double time);

    protected abstract void _VisitSpecs(SdfAbstractDataSpecVisitor visitor);

    private class CopySpecVisitor : SdfAbstractDataSpecVisitor
    {
        private readonly SdfAbstractData _target;
        
        public CopySpecVisitor(SdfAbstractData target)
        {
            _target = target;
        }
        
        public override bool VisitSpec(SdfAbstractData data, SdfPath path)
        {
            var specType = data.GetSpecType(path);
            _target.CreateSpec(path, specType);
            
            var fields = data.List(path);
            foreach (var field in fields)
            {
                var value = data.Get(path, field);
                _target.Set(path, field, value);
            }
            
            return true;
        }
    }
}