using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public static class UiInitializers
    {
        public static Dictionary<Type, (string prefabName, Action<MemberReference, UiComponent> init)> Registry = new Dictionary<Type, (string prefabName, Action<MemberReference, UiComponent> init)>()
        {
            {
                typeof(GameObjectReference), (prefabName: "Class", init: (MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.valueLabel.text = fieldReference.GetTypeName();
                    uiComponent.action = (object n) => {
                        TweachMain.GetUiInstantiatorWithSettings().FillHierarchy(TweachMain.gameObjectReferences);
                        TweachMain.GetUiInstantiatorWithSettings().InstantiateComponents(fieldReference.value as GameObjectReference);
                    };
                })
            },
            {
                typeof(ComponentReference), (prefabName: "Class", init: (MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.valueLabel.text = fieldReference.GetTypeName();
                    uiComponent.action = (object n) => {
                        TweachMain.GetUiInstantiatorWithSettings().FillHierarchy(TweachMain.gameObjectReferences);
                        TweachMain.GetUiInstantiatorWithSettings().InstantiateMemberCollection(fieldReference.value as ComponentReference);
                    };
                })
            },
            {
                typeof(string), (prefabName: "String", init: (MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = fieldReference.value == null ? "" : fieldReference.value.ToString();
                    uiComponent.action = (object newValue) => 
                        uiComponent.SetChangeColor(fieldReference.PushValue(newValue));
                })
            },
            {
                typeof(int), (prefabName: "Int", init: (MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = ((int)fieldReference.value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue(int.Parse((string)newValue, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat)));
                })
            },
            {
                typeof(float), (prefabName: "Float", init: (MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = ((float)fieldReference.value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue(float.Parse(((string)newValue).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat)));
                })
            },
            {
                typeof(bool), (prefabName: "Bool", init: (MemberReference fieldReference, UiComponent uiComponent) => 
                {
                    uiComponent.toggle.isOn = (bool)fieldReference.value;
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue((bool)newValue));
                })
            }
        };
    }
}