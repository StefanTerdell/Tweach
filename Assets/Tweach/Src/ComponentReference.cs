using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    public class ComponentReference : IFieldCollection
    {
        public Component value;
        public GameObjectReference parentGameObjectReference;
        public List<FieldReference> childFieldReferences;

        public ComponentReference(Component value, GameObjectReference gameObjectReference)
        {
            this.value = value;
            this.parentGameObjectReference = gameObjectReference;
        }

        public List<FieldReference> GetFields()
        {
            return childFieldReferences;
        }

        public string GetName()
        {
            return value.GetType().Name;
        }

        public IFieldCollection GetParentAsIFieldCollection()
        {
            return null;
        }

        public INamedChild GetParentAsINamedChild()
        {
            return parentGameObjectReference;
        }
    }
}