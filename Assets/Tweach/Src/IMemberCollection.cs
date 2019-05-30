using System.Collections.Generic;

namespace Tweach
{
    public interface IMemberCollection : INamedChild
    {
        List<MemberReference> GetMembers();
    }
}