using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Tweach
{
    public static class UiInitializationDelegates
    {
        public static Dictionary<Type, Action<Reference, UiComponent>> Registry = new Dictionary<Type, Action<Reference, UiComponent>>()
        {
            {
                typeof(GameObject),
                    (Reference reference, UiComponent uiComponent) =>
                        {
                            uiComponent.action = (v) => Ui.InstantiateUiElements(reference);
                        }
            },
            {
                typeof(string),
                    (Reference reference, UiComponent uiComponent) =>
                        {
                            uiComponent.action = (newValue) => 
                            {
                                reference.fieldInfo.SetValue(reference.owner.value, newValue);
                                reference.value = reference.fieldInfo.GetValue(reference.owner.value);
                            };

                            uiComponent.inputField.text = reference.value.ToString();
                        }
            },
            {
                typeof(int),
                    (Reference reference, UiComponent uiComponent) =>
                        {
                            uiComponent.action = (newValue) =>
                            {
                                reference.fieldInfo.SetValue(reference.owner.value, int.Parse((string)newValue, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat));
                                reference.value = reference.fieldInfo.GetValue(reference.owner.value);
                            };

                            uiComponent.inputField.text = ((int)reference.value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                        }
            },
            {
                typeof(float),
                    (Reference reference, UiComponent uiComponent) =>
                        {
                            uiComponent.action = (newValue) =>
                            {
                                reference.fieldInfo.SetValue(reference.owner.value, float.Parse(((string)newValue).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat));
                                reference.value = reference.fieldInfo.GetValue(reference.owner.value);
                            };

                            uiComponent.inputField.text = ((float)reference.value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                        }
            }
        };
    }
}
