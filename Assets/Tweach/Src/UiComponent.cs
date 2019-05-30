using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public class UiComponent : MonoBehaviour
    {
        public Image background;
        public Text nameLabel;
        public Text valueLabel;
        public InputField inputField;
        public Toggle toggle;

        public Action<object> action;

        public void SetChangedColor()
        {
            background.color = new Color(background.color.r, 1, background.color.b, background.color.a);
        }

        public void ValueChangeComponentCallback(string newValue)
        {
            action?.Invoke(newValue);
        }
        public void ValueChangeComponentCallback(bool newValue)
        {
            action?.Invoke(newValue);
        }
        public void ButtonPressComponentCallback()
        {
            action?.Invoke(null);
        }
    }
}
