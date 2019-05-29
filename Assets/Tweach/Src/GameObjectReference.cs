using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    public class GameObjectReference : INamedChild
    {
        public bool expanded;
        public GameObject value;
        public GameObjectReference parentGameObjectReference;
        public List<GameObjectReference> childGameObjectReferences;
        public List<ComponentReference> childComponentReferences;
        
        public GameObjectReference(GameObject value)
        {
            this.value = value;
        }

        public string GetName()
        {
            return value.name;
        }

        public INamedChild GetParentAsINamedChild()
        {
            return parentGameObjectReference;
        }
    }
}