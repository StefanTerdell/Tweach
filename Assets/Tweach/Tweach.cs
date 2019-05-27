using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tweach
{
    public class Tweach : MonoBehaviour
    {
        public bool _onlyMarked;
        public bool _privateFields;
        public bool _publicFields;
        public bool _hideFieldlessObjectsAndComponents;

        public static Transform uiComponentParentTransform => instance.transform; //Remove
        public static Vector2 gridSize => Vector2.zero; //Remove
        public static float headerHeigth => 1; //Remove



        public static Tweach instance;
        private void Awake()
        {
            instance = this;
        }

        public bool Go;
        void Update()
        {
            if (Go)
            {
                Go = false;
                var list = ReferenceMapper.GetRootGameObjectReferences(_onlyMarked, _hideFieldlessObjectsAndComponents);
                UiInstantiation.FillHierarchy(list);
                Debug.Log(Utilities.DebugString(list));
            }
        }

        public static BindingFlags GetBindingFlags()
        {
            BindingFlags flags = BindingFlags.Instance;

            if (instance._privateFields)
                flags |= BindingFlags.NonPublic;

            if (instance._publicFields)
                flags |= BindingFlags.Public;

            return flags;
        }
    }
}