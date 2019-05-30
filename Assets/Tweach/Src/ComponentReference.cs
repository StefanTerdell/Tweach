using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    public class ComponentReference : IMemberCollection
    {
        public Component value;
        public GameObjectReference parentGameObjectReference;
        public List<MemberReference> childFieldReferences;

        public ComponentReference(Component value, GameObjectReference gameObjectReference)
        {
            this.value = value;
            this.parentGameObjectReference = gameObjectReference;
        }

        public List<MemberReference> GetMembers()
        {
            return childFieldReferences;
        }

        public string GetName()
        {
            return value.GetType().Name;
        }

        public string GetTypeName()
        {
            return GetName();
        }

        public IMemberCollection GetParentAsIFieldCollection()
        {
            return null;
        }

        public INamedChild GetParentAsINamedChild()
        {
            return parentGameObjectReference;
        }
    }
}