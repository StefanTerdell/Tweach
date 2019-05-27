using System.Collections.Generic;
using UnityEngine;

namespace Tweach
{
    [System.Serializable]
    public class ComponentReference : IFieldCollection
    {
        public Component value;
        [HideInInspector] public GameObjectReference gameObjectReference;
        public List<FieldReference> fieldReferences;
        public List<FieldReference> GetFields() => fieldReferences;
        public string GetName() => value.GetType().Name;
    }    
}