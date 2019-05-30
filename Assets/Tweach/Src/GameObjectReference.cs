using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    public class GameObjectReference : IReference
    {
        public bool expanded;
        public bool matchesSearchQuery = true;
        public GameObject value;
        public GameObjectReference parentGameObjectReference;
        public List<GameObjectReference> childGameObjectReferences;
        public List<ComponentReference> childComponentReferences;
        public List<MemberReference> childMemberReferences;
        
        public GameObjectReference(GameObject value)
        {
            childMemberReferences = new List<MemberReference>();
            this.value = value;
        }

        public string GetName() => value.name;
        public string GetTypeName() => value.GetType().Name;
        public IReference GetParentReference() => parentGameObjectReference;
        public List<MemberReference> GetMembers() => childMemberReferences;
        public object GetValue() => value;
    }
}