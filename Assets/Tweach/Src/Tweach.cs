using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tweach
{
    public class Tweach : MonoBehaviour
    {
        [Header("Options")]
        public Vector2Int _gridSize = new Vector2Int(3, 4);
        [Tooltip("Only Map Members Marked With TweachAttribute")]
        public bool _mapOnlyMembersMarkedWithTweach;
        public bool _respectHideInInspector;
        [Tooltip("Map Private Members")]
        public bool _mapPrivateMembers;
        [Tooltip("Map Public Members")]
        public bool _mapPublicMembers;
        [Tooltip("Map fields")]
        public bool _mapFields;
        [Tooltip("Map Properties. Must implement both Get and Set!")]
        public bool _mapProperties;
        [Tooltip("Hide Components Without Serialized Fields And GameObjects Without Components Or Only Hidden Components")]
        public bool _hideMemberlessObjectsAndComponents;
        [Tooltip("Only change this if you know what you're doing :)")]
        public int _maxMappingDepth = 20;
        public bool _logMapTime = true;
        public bool _logMap;
        public bool _perilousMode;

        //[Header("Unity UI Components")]
        [HideInInspector] public Transform _hierarchyContentTransform;
        [HideInInspector] public Transform _componentsAndFieldsContentTransform;
        [HideInInspector] public Button _backButton;
        [HideInInspector] public Button _exitButton;
        [HideInInspector] public InputField _searchField;
        [HideInInspector] public Text _pathText;

        //Static References
        public static int mappedGameObjectCount, mappedComponentCount, mappedMemberCount, failedMemberCount;
        public static List<GameObjectReference> gameObjectReferences;
        public static string baseTweachAssetPath;
        public static Transform hierarchyContentTransform => instance._hierarchyContentTransform;
        public static Transform componentsAndFieldsContentTransform => instance._componentsAndFieldsContentTransform;
        public static Button backButton => instance._backButton;
        public static Button exitButton => instance._exitButton;
        public static InputField searchField => instance._searchField;
        public static Text pathText => instance._pathText;
        public static Vector2 gridSize  => instance._gridSize;
        public static int maxMappingDepth                   => instance._maxMappingDepth;
        public static bool mapProperties                                         => instance._mapProperties;
        public static bool mapFields                                    => instance._mapFields;
        public static bool perilousMode                     => instance._perilousMode;
        public static bool mapOnlyMembersMarkedWithTweach = instance._mapOnlyMembersMarkedWithTweach;
        public static bool respectHideInInspector           = instance._respectHideInInspector;
        public static bool hideMemberlessObjectsAndComponents => instance._hideMemberlessObjectsAndComponents;

        static Tweach instance;
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

        void Start()
        {
            Run();
        }

        public bool run;
        void Update() {
            if (run)
            {
                run = false;
                Run();
            }
        }

        void Run()
        {
            mappedGameObjectCount = mappedComponentCount = mappedMemberCount = failedMemberCount = 0;

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            gameObjectReferences = ReferenceMapper.GetRootGameObjectReferences();

            sw.Stop();

            if (_logMapTime)
                Debug.Log($"Mapped {mappedGameObjectCount} GameObjects, {mappedComponentCount} Components and {mappedMemberCount} Members in {sw.ElapsedMilliseconds}ms. Failed to get value of {failedMemberCount} Members.");

            UiInstantiation.FillHierarchy(gameObjectReferences);
            
            if (_logMap)
                Debug.Log(DebugHelper.GetDebugString(gameObjectReferences));
        }

        public void Destroy()
        {
            Destroy(gameObject);
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

            if (instance._mapPrivateMembers)
                flags |= BindingFlags.NonPublic;

            if (instance._mapPublicMembers)
                flags |= BindingFlags.Public;

            return flags;
        }
    }
}