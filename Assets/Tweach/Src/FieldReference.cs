using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tweach
{
    public class FieldReference : IFieldCollection
    {
        public object value;
        public object parentValue;
        public FieldInfo fieldInfo;
        public List<FieldReference> childFieldReferences;
        public FieldReference parentFieldReference;
        public IFieldCollection parentIFieldCollection;

        public FieldReference(IFieldCollection parentFieldCollection, object owner, FieldInfo fieldInfo)
        {
            if (parentFieldCollection is FieldReference)
                parentFieldReference = parentFieldCollection as FieldReference;

            this.parentIFieldCollection = parentFieldCollection;
            this.parentValue = owner;
            this.fieldInfo = fieldInfo;
        }

        public void PushValue(object newValue)
        {
            fieldInfo.SetValue(parentValue, newValue);
            value = fieldInfo.GetValue(parentValue);

            if (parentValue.GetType().IsValueType)
                parentFieldReference.fieldInfo.SetValue(parentFieldReference.parentValue, parentValue);
        }

        public List<FieldReference> GetFields()
        {
            return childFieldReferences;
        }

        public string GetName()
        {
            return fieldInfo.Name;
        }

        public INamedChild GetParentAsINamedChild()
        {
            return parentIFieldCollection;
        }
    }
}