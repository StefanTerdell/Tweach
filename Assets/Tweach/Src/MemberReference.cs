using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tweach
{
    public enum PushValueResult
    {
        Unchanged,
        Succeded,
        Failed
    }

    public class MemberReference : IReference
    {
        object value;
        object parentValue;
        MemberInfo memberInfo;
        List<MemberReference> childMemberReferences;
        IReference parentReference;
        bool isArrayMember;
        int arrayIndex;

        public MemberReference(IReference parentFieldCollection, MemberInfo memberInfo)
        {
            this.parentReference = parentFieldCollection;
            this.parentValue = parentFieldCollection.GetValue();
            this.memberInfo = memberInfo;
        }

        public MemberReference(IReference parentFieldCollection, int arrayIndex)
        {
            this.parentReference = parentFieldCollection;
            this.parentValue = parentFieldCollection.GetValue();
            this.arrayIndex = arrayIndex;
            isArrayMember = true;
        }

        public PushValueResult PushValue(object newValue, bool ignoreObjectEquals = false)
        {
            if (!ignoreObjectEquals && object.Equals(value, newValue))
            {
                return PushValueResult.Unchanged;
            }

            try //This trycatch seems to do exactly nothing. Value setting is delayed somehow and exceptions go uncaught
            {
                if (isArrayMember)
                {
                    var pv = (IList)parentValue;
                    pv[arrayIndex] = newValue;
                    value = newValue;
                }
                else if (memberInfo is FieldInfo)
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
                else
                {
                    return PushValueResult.Failed;
                }
            }
            catch (Exception exc)
            {
                Debug.Log(exc.Message);
                return PushValueResult.Failed;
            }

            if (parentValue.GetType().IsValueType)
                (parentReference as MemberReference).PushValue(parentValue, true);

            return PushValueResult.Succeded;
        }

        public void AddMember(MemberReference memberReference)
        {
            if (childMemberReferences == null)
                childMemberReferences = new List<MemberReference>();

            childMemberReferences.Add(memberReference);
        }

        public Type GetMemberType()
        {
            return value != null ? value.GetType() : (memberInfo is FieldInfo ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType);
            //Not getting value.GetType here as it can be set as a GameObjectReference, while the memberinfo will always reflect the member itself
            // return isArrayMember ? value.GetType() : memberInfo is FieldInfo ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;
        }

        public List<MemberReference> GetMembers() => childMemberReferences;
        public string GetName() => isArrayMember ? arrayIndex.ToString() : memberInfo.Name;
        public string GetTypeName() => GetMemberType().Name;
        public IReference GetParentReference() => parentReference;
        public object GetValue() => value;
        public T GetValue<T>() => (T)value;
        public void SetValue(object value) => this.value = value;
    }
}