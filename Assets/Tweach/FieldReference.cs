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
    }
}