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

        public void ValueChangeComponentCallback(string newValue)
        {
            action.Invoke(newValue);
        }
        public void ValueChangeComponentCallback(float newValue)
        {
            action.Invoke(newValue);
        }
        public void ValueChangeComponentCallback(int newValue)
        {
            action.Invoke(newValue);
        }
        public void ValueChangeComponentCallback(bool newValue)
        {
            action.Invoke(newValue);
        }

        public void ButtonPressComponentCallback()
        {
            action?.Invoke(null);
        }
    }
}
