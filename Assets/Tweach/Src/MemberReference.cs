using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tweach
{
    public class MemberReference : IMemberCollection
    {
        public object value;
        public object parentValue;
        public MemberInfo memberInfo;
        public List<MemberReference> childMemberReferences;
        public MemberReference parentMemberReference; //To propagate change in value types upwards
        public IMemberCollection parentIMemberCollection;
        public MemberReference(IMemberCollection parentFieldCollection, object owner, MemberInfo memberInfo)
        {
            if (parentFieldCollection is MemberReference)
                parentMemberReference = parentFieldCollection as MemberReference;

            this.parentIMemberCollection = parentFieldCollection;
            this.parentValue = owner;
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

            if (parentValue.GetType().IsValueType)
                parentMemberReference.PushValue(parentValue);
        }

        public List<MemberReference> GetMembers()
        {
            return childMemberReferences;
        }

        public string GetName()
        {
            return memberInfo.Name;
        }

        public Type GetMemberType()
        {
            //Not getting value.Type here as it can be set as a GameObjectReference, while the memberinfo will always reflect the member itself
            return memberInfo is FieldInfo ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;
        }

        public string GetTypeName()
        {
            return GetMemberType().Name;
        }

        public INamedChild GetParentAsINamedChild()
        {
            return parentIMemberCollection;
        }
    }
}