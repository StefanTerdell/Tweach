using System.Collections.Generic;

namespace Tweach
{
    public interface IFieldCollection : INamedChild
    {
        List<FieldReference> GetFields();
    }
}