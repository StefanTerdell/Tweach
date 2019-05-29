using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public class UiInstantiation
    {
        static List<GameObject> instantiatedHierarchyObjects = new List<GameObject>();
        static List<GameObject> instantiatedComponentsAndFieldsObjects = new List<GameObject>();

        public static void FillHierarchy(List<GameObjectReference> gameObjectReferences)
        {
            var hierarchyMemberPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/HierarchyMember.prefab", typeof(GameObject)) as GameObject;

            foreach (var instantiatedObject in instantiatedHierarchyObjects)
            {
                GameObject.Destroy(instantiatedObject);
            }

            instantiatedHierarchyObjects.Clear();

            foreach (var gameObjectReference in gameObjectReferences)
            {
                FillHierarchy(gameObjectReferences, gameObjectReference, hierarchyMemberPrefab);
            }
        }

        static void FillHierarchy(List<GameObjectReference> gameObjectReferences, GameObjectReference gameObjectReference, GameObject hierarchyMemberPrefab, int depth = -1)
        {
            depth++;

            var hierarchyMemberGameObject = GameObject.Instantiate(hierarchyMemberPrefab, Tweach.hierarchyContentTransform);
            instantiatedHierarchyObjects.Add(hierarchyMemberGameObject);

            var hierarchyMemberText = hierarchyMemberGameObject.GetComponent<Text>();
            hierarchyMemberText.text = $"{Utilities.Indent(depth)}{gameObjectReference.value.name}";

            var hierarchyMemberButton = hierarchyMemberGameObject.GetComponent<Button>();
            hierarchyMemberButton.onClick.AddListener(() =>
            {
                gameObjectReference.expanded = !gameObjectReference.expanded;
                FillHierarchy(gameObjectReferences);
                InstantiateComponents(gameObjectReference);
            });

            if (gameObjectReference.expanded)
            {
                foreach (var child in gameObjectReference.childReferences)
                {
                    FillHierarchy(gameObjectReferences, child, hierarchyMemberPrefab, depth);
                }
            }
        }

        static void ClearComponentsAndFieldsContent(INamedChild namedChild)
        {
            foreach (var instantiatedObject in instantiatedComponentsAndFieldsObjects)
            {
                GameObject.Destroy(instantiatedObject);
            }

            instantiatedComponentsAndFieldsObjects.Clear();

            Tweach.backButton.interactable = false;

            Tweach.pathText.text = GetPathString(namedChild, null);

        }

        static string GetPathString(INamedChild namedChild, string path)
        {
            if (path == null)
                path = namedChild.GetName();
            else
                path = namedChild.GetName() + " / " + path;

            if (namedChild.GetParentWithName() != null)
                path = GetPathString(namedChild.GetParentWithName(), path);

            return path;
        }

        public static void InstantiateComponents(GameObjectReference gameObjectReference)
        {
            ClearComponentsAndFieldsContent(gameObjectReference);

            Tweach.backButton.interactable = false;

            var classPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

            foreach (var componentReference in gameObjectReference.componentReferences)
            {
                var instantiatedComponentObject = GameObject.Instantiate(classPrefab, Tweach.componentsAndFieldsContentTransform);
                instantiatedComponentsAndFieldsObjects.Add(instantiatedComponentObject);

                var instantiatedComponentButton = instantiatedComponentObject.GetComponentInChildren<Button>();
                instantiatedComponentButton.onClick.AddListener(() =>
                {
                    InstantiateFieldCollection(componentReference);
                });

                var instantiatedComponentLabel = instantiatedComponentButton.GetComponentInChildren<Text>();
                instantiatedComponentLabel.text = componentReference.GetName();
            }
        }

        public static void InstantiateFieldCollection(IFieldCollection fieldCollection)
        {
            ClearComponentsAndFieldsContent(fieldCollection);

            if (fieldCollection.GetParentIFieldCollection() != null)
            {
                Tweach.backButton.interactable = true;
                Tweach.backButton.onClick.RemoveAllListeners();
                Tweach.backButton.onClick.AddListener(() =>
                {
                    InstantiateFieldCollection(fieldCollection.GetParentIFieldCollection());
                });

                Debug.Log(fieldCollection.GetName() + " has parent " + fieldCollection.GetParentIFieldCollection().GetName());
            }
            else
            {
                Debug.Log(fieldCollection.GetName() + " has no parent");
            }

            foreach (var fieldReference in fieldCollection.GetFields())
            {
                if (fieldReference.children != null && fieldReference.children.Count > 0)
                {
                    var classPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

                    var instantiatedComponentObject = GameObject.Instantiate(classPrefab, Tweach.componentsAndFieldsContentTransform);
                    instantiatedComponentsAndFieldsObjects.Add(instantiatedComponentObject);

                    var instantiatedComponentButton = instantiatedComponentObject.GetComponentInChildren<Button>();
                    instantiatedComponentButton.onClick.AddListener(() =>
                    {
                        InstantiateFieldCollection(fieldReference);
                    });

                    var instantiatedComponentLabel = instantiatedComponentButton.GetComponentInChildren<Text>();
                    instantiatedComponentLabel.text = fieldReference.fieldInfo.Name;
                }
                else if (Types.Registry.ContainsKey(fieldReference.fieldInfo.FieldType)) 
                {
                    var tweachType = Types.Registry[fieldReference.fieldInfo.FieldType];

                    var classPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/{tweachType.prefabName}.prefab", typeof(GameObject)) as GameObject;

                    var instantiatedObject = GameObject.Instantiate(classPrefab, Tweach.componentsAndFieldsContentTransform);
                    instantiatedComponentsAndFieldsObjects.Add(instantiatedObject);

                    var uiComponent = instantiatedObject.GetComponent<UiComponent>();
                    uiComponent.label.text = fieldReference.GetName();
                    tweachType.init.Invoke(fieldReference, uiComponent);
                }
                else
                {
                    var classPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

                    var instantiatedComponentObject = GameObject.Instantiate(classPrefab, Tweach.componentsAndFieldsContentTransform);
                    instantiatedComponentsAndFieldsObjects.Add(instantiatedComponentObject);

                    var instantiatedComponentButton = instantiatedComponentObject.GetComponentInChildren<Button>();

                    var instantiatedComponentLabel = instantiatedComponentButton.GetComponentInChildren<Text>();
                    instantiatedComponentLabel.text = fieldReference.fieldInfo.Name;
                }
            }
        }
    }
}
