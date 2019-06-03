using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public static class UiInitializers
    {
        public static Dictionary<Type, (string prefabName, Action<UiInstantiator, MemberReference, UiComponent> init)> Registry = new Dictionary<Type, (string prefabName, Action<UiInstantiator, MemberReference, UiComponent> init)>()
        {
            {
                typeof(GameObjectReference), (prefabName: "Class", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.valueLabel.text = fieldReference.GetValue<GameObjectReference>().GetTypeName();
                    uiComponent.action = (object n) => {
                        uiInstantiator.FillHierarchy(TweachMain.gameObjectReferences);
                        uiInstantiator.InstantiateComponents(fieldReference.GetValue<GameObjectReference>());
                    };
                })
            },
            {
                typeof(ComponentReference), (prefabName: "Class", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.valueLabel.text = fieldReference.GetValue<ComponentReference>().GetTypeName();
                    uiComponent.action = (object n) => {
                        uiInstantiator.FillHierarchy(TweachMain.gameObjectReferences);
                        uiInstantiator.InstantiateMemberCollection(fieldReference.GetValue<ComponentReference>());
                    };
                })
            },
            {
                typeof(Color), (prefabName: "Class", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    var c = fieldReference.GetValue<Color>();
                    uiComponent.valueLabel.text = $"r:{c.r}, g:{c.g}, b:{c.b}, a:{c.a}";
                    uiComponent.colorPreview.color = c;
                    uiComponent.action = (object n) => {
                        uiInstantiator.InstantiateMemberCollection(fieldReference);
                    };
                })
            },
            {
                typeof(string), (prefabName: "String", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {

                    uiComponent.inputField.text = fieldReference.GetValue() == null ? "" : fieldReference.GetValue().ToString();
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue(newValue));
                })
            },
            {
                typeof(int), (prefabName: "Int", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = fieldReference.GetValue<int>().ToString(CultureInfo.InvariantCulture.NumberFormat);
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue(int.Parse((string)newValue, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat)));
                })
            },
            {
                typeof(float), (prefabName: "Float", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.inputField.text = fieldReference.GetValue<float>().ToString(CultureInfo.InvariantCulture.NumberFormat);
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue(float.Parse(((string)newValue).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat)));
                })
            },
            {
                typeof(bool), (prefabName: "Bool", init: (UiInstantiator uiInstantiator, MemberReference fieldReference, UiComponent uiComponent) =>
                {
                    uiComponent.toggle.isOn = fieldReference.GetValue<bool>();
                    uiComponent.action = (object newValue) =>
                        uiComponent.SetChangeColor(fieldReference.PushValue((bool)newValue));
                })
            }
        };
    }
}