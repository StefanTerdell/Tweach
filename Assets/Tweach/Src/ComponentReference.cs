using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    public class ComponentReference : IReference
    {
        public Component value;
        public GameObjectReference parentGameObjectReference;
        public List<MemberReference> childMemberReferences;

        public ComponentReference(Component value, GameObjectReference gameObjectReference)
        {
            childMemberReferences = new List<MemberReference>();
            this.value = value;
            this.parentGameObjectReference = gameObjectReference;
        }

        public List<MemberReference> GetMembers() => childMemberReferences;
        public string GetName() => value.GetType().Name;
        public string GetTypeName() => GetName();
        public IReference GetParentReference() => parentGameObjectReference;
        public object GetValue() => value;
    }
}