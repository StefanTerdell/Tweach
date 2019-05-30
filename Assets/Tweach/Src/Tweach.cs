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
        public bool mapOnlyMembersMarkedWithTweach;
        public bool respectHideInInspector;
        public bool mapFields, mapProperties, mapPublic, mapNonPublic, mapStatic, mapInstance;
        public bool hideMemberlessObjectsAndComponents;
        [Header("Debugging")]
        public int maxMappingDepth = 20;
        public bool logMapTime = true;
        public bool logMap;

        [HideInInspector] public Transform _hierarchyContentTransform;
        [HideInInspector] public Transform _componentsAndFieldsContentTransform;
        [HideInInspector] public Button _backButton;
        [HideInInspector] public Button _exitButton;
        [HideInInspector] public InputField _searchField;
        [HideInInspector] public Text _pathText;

        public static List<GameObjectReference> gameObjectReferences;

        static Tweach instance;
        string baseTweachAssetPath;

        void Start()
        {
            instance = this;
            baseTweachAssetPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this));
            baseTweachAssetPath = baseTweachAssetPath.Substring(0, baseTweachAssetPath.LastIndexOf('/'));
        
            var rm = GetReferenceMapperWithSettings();

            gameObjectReferences = rm.GetRootGameObjectReferences(logMapTime);
            
            if (logMap)
                Debug.Log(DebugHelper.GetDebugString(gameObjectReferences));

            var ui = GetUiInstantiatorWithSettings();

            ui.FillHierarchy(gameObjectReferences);
        }

        public static UiInstantiator GetUiInstantiatorWithSettings()
        {
            return new UiInstantiator(instance.baseTweachAssetPath, 
                                      instance._hierarchyContentTransform, 
                                      instance._componentsAndFieldsContentTransform, 
                                      instance._gridSize, 
                                      instance._backButton, 
                                      instance._pathText);
        }

        ReferenceMapper GetReferenceMapperWithSettings()
        {
            return new ReferenceMapper(hideMemberlessObjectsAndComponents,
                                       respectHideInInspector, 
                                       mapOnlyMembersMarkedWithTweach,
                                       mapPublic, 
                                       mapNonPublic, 
                                       mapFields, 
                                       mapProperties, 
                                       mapInstance, 
                                       mapStatic);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            gameObjectReferences = null;
        }
    }
}