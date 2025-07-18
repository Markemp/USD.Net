using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public interface ISdfFieldDefinition
{
    TfToken GetName();
    bool IsReadOnly();
    bool HoldsChildren();
    VtValue GetFallbackValue();
}
