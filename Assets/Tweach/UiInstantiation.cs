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
            var hierarchyMemberPrefab = AssetDatabase.LoadAssetAtPath($"{TweachComponents.baseTweachAssetPath}/UiPrefabs/HierarchyMember.prefab", typeof(GameObject)) as GameObject;

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

            var hierarchyMemberGameObject = GameObject.Instantiate(hierarchyMemberPrefab, TweachComponents.hierarchyContentTransform);
            instantiatedHierarchyObjects.Add(hierarchyMemberGameObject);

            var hierarchyMemberText = hierarchyMemberGameObject.GetComponent<Text>();
            hierarchyMemberText.text = $"{Utilities.Indent(depth)}{gameObjectReference.value.name}";

            var hierarchyMemberButton = hierarchyMemberGameObject.GetComponent<Button>();
            hierarchyMemberButton.onClick.AddListener(() =>
            {
                gameObjectReference.expanded = !gameObjectReference.expanded;
                FillHierarchy(gameObjectReferences);
                InstantiateComponents(gameObjectReference.componentReferences);
            });

            if (gameObjectReference.expanded)
            {
                foreach (var child in gameObjectReference.childReferences)
                {
                    FillHierarchy(gameObjectReferences, child, hierarchyMemberPrefab, depth);
                }
            }
        }

        static void ClearComponentsAndFieldsContent()
        {
            foreach (var instantiatedObject in instantiatedComponentsAndFieldsObjects)
            {
                GameObject.Destroy(instantiatedObject);
            }

            instantiatedComponentsAndFieldsObjects.Clear();
        }

        static void InstantiateComponents(List<ComponentReference> componentReferences)
        {
            ClearComponentsAndFieldsContent();
            
            TweachComponents.backButton.interactable = false;

            var classPrefab = AssetDatabase.LoadAssetAtPath($"{TweachComponents.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

            foreach (var componentReference in componentReferences)
            {
                var instantiatedComponentObject = GameObject.Instantiate(classPrefab, TweachComponents.componentsAndFieldsContentTransform);
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

        static void InstantiateFieldCollection(IFieldCollection fieldCollection)
        {
            ClearComponentsAndFieldsContent();

            if (fieldCollection.GetParent() == null)
            {
                TweachComponents.backButton.interactable = false;
            }
            else
            {
                TweachComponents.backButton.onClick.RemoveAllListeners();
                TweachComponents.backButton.onClick.AddListener(() =>
                {
                    InstantiateFieldCollection(fieldCollection.GetParent());
                });
            }

            foreach (var fieldReference in fieldCollection.GetFields())
            {
                if (fieldReference.children != null && fieldReference.children.Count > 0)
                {
                    var classPrefab = AssetDatabase.LoadAssetAtPath($"{TweachComponents.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

                    var instantiatedComponentObject = GameObject.Instantiate(classPrefab, TweachComponents.componentsAndFieldsContentTransform);
                    instantiatedComponentsAndFieldsObjects.Add(instantiatedComponentObject);

                    var instantiatedComponentButton = instantiatedComponentObject.GetComponentInChildren<Button>();
                    instantiatedComponentButton.onClick.AddListener(() =>
                    {
                        InstantiateFieldCollection(fieldReference);
                    });

                    var instantiatedComponentLabel = instantiatedComponentButton.GetComponentInChildren<Text>();
                    instantiatedComponentLabel.text = fieldReference.fieldInfo.Name;
                }
                else
                {
                    var classPrefab = AssetDatabase.LoadAssetAtPath($"{TweachComponents.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

                    var instantiatedComponentObject = GameObject.Instantiate(classPrefab, TweachComponents.componentsAndFieldsContentTransform);
                    instantiatedComponentsAndFieldsObjects.Add(instantiatedComponentObject);

                    var instantiatedComponentButton = instantiatedComponentObject.GetComponentInChildren<Button>();

                    var instantiatedComponentLabel = instantiatedComponentButton.GetComponentInChildren<Text>();
                    instantiatedComponentLabel.text = fieldReference.fieldInfo.Name;
                }
            }
        }
    }
}
