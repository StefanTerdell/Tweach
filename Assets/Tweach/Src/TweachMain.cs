using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tweach
{
    public class TweachMain : MonoBehaviour
    {
        public Action runOnExit; //This runs whenever Tweach is exited or disabled. Use it to unpause your game! :)

        [Header("Options")]
        public Vector2Int gridSize = new Vector2Int(3, 4);
        public bool mapOnlyMembersMarkedWithTweach;
        public bool respectHideInInspector;
        public bool mapFields, mapProperties, mapPublic, mapNonPublic, mapStatic, mapInstance;
        public bool hideMemberlessObjectsAndComponents;
        
        [Header("Debugging")]
        public int maxDepth = 20;
        public bool logMapTime = true;
        public bool logMap;

        [Header("Unity UI Components")]
        public Transform hierarchyContentTransform;
        public Transform componentsAndFieldsContentTransform;
        public Button backButton;
        public Text pathText;

        public static List<GameObjectReference> gameObjectReferences;
        public static UiInstantiator uiInstantiator;
        static TweachMain instance;
        string baseTweachAssetPath;
        internal static object run;

        void Awake() {
            if (instance != null)
                Destroy(instance.gameObject);

            instance = this;
            baseTweachAssetPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this)).MoveUpInFileSystemPath(2);
        }

        void OnEnable()
        {
            if (mapNonPublic && mapStatic && !mapOnlyMembersMarkedWithTweach)
                Debug.LogWarning("Mapping all non public static fields can be dangerous!");

            gameObjectReferences = GetReferenceMapperWithSettings().GetRootGameObjectReferences(logMapTime);
            
            if (logMap) 
                Debug.Log(Utilities.GetDebugString(gameObjectReferences));

            uiInstantiator = GetUiInstantiatorWithSettings(); //For search function
            uiInstantiator.FillHierarchy(gameObjectReferences);
        }

        void OnDisable() {
            gameObjectReferences = null; //Let GC collect the static list
            uiInstantiator = null; //and the uiInstatiator
            runOnExit?.Invoke();
        }

        public void Destroy() //Delegate for exit button
        {
            Destroy(gameObject);
        }

        UiInstantiator GetUiInstantiatorWithSettings()
        {
            return new UiInstantiator(instance.baseTweachAssetPath, 
                                      instance.hierarchyContentTransform, 
                                      instance.componentsAndFieldsContentTransform, 
                                      instance.gridSize, 
                                      instance.backButton, 
                                      instance.pathText);
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
                                       mapStatic,
                                       maxDepth);
        }
    }
}