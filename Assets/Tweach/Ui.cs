using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tweach
{
    public class Ui
    {
        static List<GameObject> instantiatedObjects = new List<GameObject>();

        public static void InstantiateUiElements(Reference reference)
        {
            foreach (var instantiatedObject in instantiatedObjects)
            {
                GameObject.Destroy(instantiatedObject);
            }

            instantiatedObjects.Clear();

            if (reference == null)
                return;

            InstantiateHeader(reference);

            int x = 0, y = 0;

            foreach (var child in reference.children)
            {
                if (SelectInstantiateAndInitializeUiComponent(x, y, child))
                {
                    x++;

                    if (x == Tweach.gridSize.x)
                    {
                        x = 0;
                        y++;
                    }
                }
            }
        }

        static void InstantiateHeader(Reference reference)
        {
            var headerObject = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/header.prefab", typeof(GameObject)), Tweach.uiComponentParentTransform) as GameObject;

            instantiatedObjects.Add(headerObject);

            var uiComponent = headerObject.GetComponent<UiComponent>();
            uiComponent.label.text = BuildPathStringRecursively(reference);
            uiComponent.action = (v) =>
            {
                InstantiateUiElements(reference.owner);
            };

        }

        static string BuildPathStringRecursively(Reference reference, string path = null)
        {
            if (path == null)
            {
                path = reference.name;
            }
            else
            {
                var newPath = reference.name + " / " + path;

                if (newPath.Length > 30)
                {
                    path = "... / " + path;
                }
                else
                {
                    path = newPath;
                }
            }

            if (reference.owner != null)
            {
                path = BuildPathStringRecursively(reference.owner, path);
            }

            return path;
        }

        static bool SelectInstantiateAndInitializeUiComponent(int x, int y, Reference reference)
        {
            Type type;

            if (reference.children.Any())
            {
                type = typeof(GameObject);
            }
            else if (UiInitializationDelegates.Registry.ContainsKey(reference.value.GetType()))
            {
                type = reference.value.GetType();
            }
            else
            {
                return false;
            }

            InstantiateAndInitializeUiComponent(x, y, reference, type);

            return true;
        }

        static void InstantiateAndInitializeUiComponent(int x, int y, Reference reference, Type type)
        {
            var prefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/{type.Name}.prefab", typeof(GameObject));

            if (prefab == null)
            {
                Debug.LogWarning($"{Tweach.baseTweachAssetPath}/UiPrefabs/{type.Name.ToLower()}.prefab");
                return;
            }

            var uiObject = GameObject.Instantiate(prefab, Tweach.uiComponentParentTransform) as GameObject;

            instantiatedObjects.Add(uiObject);

            var rect = uiObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(x / (float)Tweach.gridSize.x, 1 - (1 / (float)Tweach.gridSize.y * (y + 1)) - Tweach.headerHeigth);
            rect.anchorMax = new Vector2((x + 1) / (float)Tweach.gridSize.x, 1 - (1 / (float)Tweach.gridSize.y * y) - Tweach.headerHeigth);

            InitializeUiComponent(reference, uiObject, type);
        }

        static void InitializeUiComponent(Reference reference, GameObject uiObject, Type type)
        {
            var uiComponent = uiObject.GetComponent<UiComponent>();
            uiComponent.label.text = reference.name;

            UiInitializationDelegates.Registry[type].Invoke(reference, uiComponent);
        }
    }
}
