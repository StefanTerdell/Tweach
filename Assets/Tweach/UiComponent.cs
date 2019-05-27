using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public class UiComponent : MonoBehaviour
    {
        public Image background;
        public Text label;
        public InputField inputField;

        public Action<object> action;

        public void SetChangedColor()
        {
            background.color = new Color(background.color.g, 1, background.color.b, background.color.a);
        }

        void InvokeChange(object newValue)
        {
            SetChangedColor();
            action?.Invoke(newValue);
        }

        public void ValueChangeComponentCallback(string newValue)
        {
            InvokeChange(newValue);
        }
        public void ValueChangeComponentCallback(float newValue)
        {
            InvokeChange(newValue);
        }
        public void ValueChangeComponentCallback(int newValue)
        {
            InvokeChange(newValue);
        }

        public void ButtonPressComponentCallback()
        {
            action?.Invoke(null);
        }
    }
}
