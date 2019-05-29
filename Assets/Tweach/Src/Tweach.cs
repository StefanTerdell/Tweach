using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public class Tweach : MonoBehaviour
    {
        [Header("Options")]
        [Tooltip("Only Serialize Fields Marked With TweachAttribute")]
        public bool _onlyMarked;
        [Tooltip("Serialize Private Fields")]
        public bool _privateFields;
        [Tooltip("Serialize Public Fields")]
        public bool _publicFields;
        [Tooltip("Hide Components Without Serialized Fields And GameObjects Without Components Or Only Hidden Components")]
        public bool _hideFieldlessObjectsAndComponents;

        [Header("Unity UI Components")]
        public Transform _hierarchyContentTransform;
        public Transform _componentsAndFieldsContentTransform;
        public Button _backButton;
        public Button _exitButton;
        public InputField _searchField;
        public Text _pathText;

        //Static References
        public static List<GameObjectReference> gameObjectReferences;
        public static Tweach instance;
        public static string baseTweachAssetPath;
        public static Transform hierarchyContentTransform => instance._hierarchyContentTransform;
        public static Transform componentsAndFieldsContentTransform => instance._componentsAndFieldsContentTransform;
        public static Button backButton => instance._backButton;
        public static Button exitButton => instance._exitButton;
        public static InputField searchField => instance._searchField;
        public static Text pathText => instance._pathText;

        private void Awake()
        {
            if (instance != null)
            {
                UnityEngine.Debug.LogWarning("Multiple Tweach instances found! Destroying this one");
                Destroy(this.gameObject);
                return;
            }

            baseTweachAssetPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this));
            baseTweachAssetPath = baseTweachAssetPath.Substring(0, baseTweachAssetPath.LastIndexOf('/'));

            instance = this;
        }

        private void Start()
        {
            var list = ReferenceMapper.GetRootGameObjectReferences(_onlyMarked, _hideFieldlessObjectsAndComponents);
            UiInstantiation.FillHierarchy(list);
            Debug.Log(DebugHelper.GetDebugString(list));
        }

        public void Exit()
        {
            Destroy(this.gameObject);
        }

        void OnDestroy()
        {
            //This helps the garbage collector clear out the static instance of the reference map that we no longer need
            gameObjectReferences = null;
            UiInstantiation.instantiatedComponentsAndFieldsObjects.Clear();
            UiInstantiation.instantiatedHierarchyObjects.Clear();
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