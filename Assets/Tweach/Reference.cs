using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tweach
{
    [Serializable]
    public class Reference
    {
        public string name;
        public object value;
        public FieldInfo fieldInfo;
        public Reference owner;
        public List<Reference> children = new List<Reference>();
        public bool valueChanged;
    }
}
