using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tweach
{
    [System.Serializable]
    public class FieldReference : IFieldCollection
    {
        public object value;
        [HideInInspector] public object owner;
        public FieldInfo fieldInfo;
        public List<FieldReference> children;
        public List<FieldReference> GetFields() => children;
        public string GetName() => fieldInfo.Name;
        public IFieldCollection parentFieldCollection;
        public IFieldCollection GetParent() => parentFieldCollection;
        public FieldReference(IFieldCollection parentFieldCollection, object owner, FieldInfo fieldInfo)
        {
            this.parentFieldCollection = parentFieldCollection;
            this.owner = owner;
            this.fieldInfo = fieldInfo;
        }
    }
}