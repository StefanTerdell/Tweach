using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Tweach
{
    public class Tweach : MonoBehaviour
    {
        public bool _tweach;
        public Vector2 _gridSize;
        public float _headerHeigth = .1f;
        public bool _onlyMarked;
        public bool _privateFields;
        public bool _publicFields;
        public bool _hideFieldlessObjectsAndComponents;

        public static BindingFlags bindingFlags => GetBindingFlags();

        public static string baseTweachAssetPath;
        public static Transform uiComponentParentTransform => instance.transform;
        public static Vector2 gridSize => instance._gridSize;
        public static float headerHeigth => instance._headerHeigth;
        public static Tweach instance;
        private void Awake()
        {
            baseTweachAssetPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this));
            baseTweachAssetPath = baseTweachAssetPath.Substring(0, baseTweachAssetPath.LastIndexOf('/'));
            
            instance = this;
        }
     
        void Update()
        {
            if (_tweach)
            {
                _tweach = false;

                //Ui.InstantiateUiElements(GetRootReferenceList(flags));
                //Debug.Log(Utilities.GetRootReferenceListDebugString(ReferenceMapper.GetRootReferenceList(bindingFlags)));
                var list = ReferenceMapper.GetRootGameObjectReferences(_onlyMarked, _hideFieldlessObjectsAndComponents);
                Debug.Log(Utilities.DebugString(list));
            }
        }

        static BindingFlags GetBindingFlags() {
            BindingFlags flags = BindingFlags.Instance;

            if (instance._privateFields)
                flags |= BindingFlags.NonPublic;
            
            if (instance._publicFields)
                flags |= BindingFlags.Public;

            return flags;
        }
    }
}