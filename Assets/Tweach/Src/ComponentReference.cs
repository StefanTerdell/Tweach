using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    public class ComponentReference : IReference
    {
        Component value;
        public GameObjectReference parentGameObjectReference;
        List<MemberReference> childMemberReferences;

        public ComponentReference(Component value, GameObjectReference gameObjectReference)
        {
            this.value = value;
            this.parentGameObjectReference = gameObjectReference;
        }

        public void AddMember(MemberReference memberReference)
        {
            if (childMemberReferences == null)
                childMemberReferences = new List<MemberReference>();

            childMemberReferences.Add(memberReference);
        }

        public void SetValue(object value)
        {
            this.value = value as Component;
        }

        public Component GetComponentValue() => value;

        public List<MemberReference> GetMembers() => childMemberReferences;
        public string GetName() => value.GetType().Name;
        public string GetTypeName() => GetName();
        public IReference GetParentReference() => parentGameObjectReference;
        public object GetValue() => value;
    }
}