using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    [System.Serializable]
    public class GameObjectReference : INamedChild
    {
        public GameObject value;
        [HideInInspector] public GameObjectReference parentReference;
        public List<GameObjectReference> childReferences;
        public List<ComponentReference> componentReferences;
        public GameObjectReference(GameObject value)
        {
            this.value = value;
        }
        public bool expanded;

        public string GetName()
        {
            return value.name;
        }

        public INamedChild GetParentWithName()
        {
            return parentReference;
        }
    }
}