using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public static class Types
    {
        public static Dictionary<Type, (string prefabName, Action<FieldReference, UiComponent> init)> Registry = new Dictionary<Type, (string prefabName, Action<FieldReference, UiComponent> init)>()
        {
            {
                typeof(GameObject), (prefabName: "Class", init: (FieldReference fieldReference, UiComponent uiComponent) => 
                {
                    uiComponent.action = (object n) => {
                        
                    };
                })
            },
            {
                typeof(string), (prefabName: "String", init: (FieldReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = fieldReference.value.ToString();
                    uiComponent.action = (object newValue) =>
                    {
                        fieldReference.fieldInfo.SetValue(fieldReference.owner, newValue);
                        fieldReference.value = fieldReference.fieldInfo.GetValue(fieldReference.owner);
                    };
                })
            },
            {
                typeof(int), (prefabName: "Int", init: (FieldReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = ((int)fieldReference.value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    uiComponent.action = (object newValue) =>
                    {
                        fieldReference.fieldInfo.SetValue(fieldReference.owner, int.Parse((string)newValue, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat));
                        fieldReference.value = fieldReference.fieldInfo.GetValue(fieldReference.owner);
                    };
                })
            },
            {
                typeof(float), (prefabName: "Float", init: (FieldReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = ((float)fieldReference.value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    uiComponent.action = (object newValue) =>
                    {
                        fieldReference.fieldInfo.SetValue(fieldReference.owner, int.Parse(((string)newValue).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat));
                        fieldReference.value = fieldReference.fieldInfo.GetValue(fieldReference.owner);
                    };
                })
            }
        };
    }
}