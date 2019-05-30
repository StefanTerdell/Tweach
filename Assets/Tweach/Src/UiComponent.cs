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

        public void SetChangeColor(PushValueResult pushValueResult)
        {
            switch (pushValueResult)
            {
                case PushValueResult.Failed:
                    background.color = new Color(1, 0, 0, .33f);
                    break;
                case PushValueResult.Succeded:
                    background.color = new Color(0, 1, 0, .33f); break;
            }
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
