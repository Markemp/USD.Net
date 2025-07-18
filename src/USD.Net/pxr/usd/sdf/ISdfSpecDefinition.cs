using Pxr.Base.Tf;

namespace Pxr.Usd.Sdf;

public interface ISdfSpecDefinition
{
    List<TfToken> GetFields();
    bool IsMetadataField(TfToken fieldName);
    TfToken GetMetadataFieldDisplayGroup(TfToken fieldName);
}
