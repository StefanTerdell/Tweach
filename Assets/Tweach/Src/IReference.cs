using System.Collections.Generic;

namespace Tweach
{
    public interface IReference
    {
        string GetName();
        string GetTypeName();
        IReference GetParentReference();
        List<MemberReference> GetMembers();
        object GetValue();
        void SetValue(object value);
        void AddMember(MemberReference nemberReference);
    }
}