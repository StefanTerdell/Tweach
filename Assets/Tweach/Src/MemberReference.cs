using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tweach
{
    public class MemberReference : IReference
    {
        public object value;
        public object parentValue;
        public MemberInfo memberInfo;
        public List<MemberReference> childMemberReferences;
        public IReference parentIReference;

        public MemberReference(IReference parentFieldCollection, MemberInfo memberInfo)
        {
            childMemberReferences = new List<MemberReference>();

            this.parentIReference = parentFieldCollection;
            this.parentValue = parentFieldCollection.GetValue();
            this.memberInfo = memberInfo;
        }

        public void PushValue(object newValue)
        {
            if (memberInfo is FieldInfo)
            {
                var fieldInfo = memberInfo as FieldInfo;

                fieldInfo.SetValue(parentValue, newValue);
                value = fieldInfo.GetValue(parentValue);
            }
            else if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                propertyInfo.SetValue(parentValue, newValue);
                value = propertyInfo.GetValue(parentValue);
            }

            if (parentValue.GetType().IsValueType && parentIReference is MemberReference)
                (parentIReference as MemberReference).PushValue(parentValue);
        }

        public Type GetMemberType()
        {
            //Not getting value.GetType here as it can be set as a GameObjectReference, while the memberinfo will always reflect the member itself
            return memberInfo is FieldInfo ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;
        }
        public List<MemberReference> GetMembers() => childMemberReferences;
        public string GetName() => memberInfo.Name;
        public string GetTypeName() => GetMemberType().Name;
        public IReference GetParentReference() => parentIReference;
        public object GetValue() => value;
    }
}