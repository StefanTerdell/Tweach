using System.Collections.Generic;

namespace Tweach
{
    public interface IFieldCollection
    {
        string GetName();
        List<FieldReference> GetFields();
    }
}