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
        public static List<GameObject> instantiatedHierarchyObjects = new List<GameObject>();
        public static List<GameObject> instantiatedComponentsAndFieldsObjects = new List<GameObject>();

        public static void FillHierarchy(List<GameObjectReference> gameObjectReferences)
        {
            var hierarchyMemberPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/HierarchyMember.prefab", typeof(GameObject)) as GameObject;

            foreach (var instantiatedObject in instantiatedHierarchyObjects)
                GameObject.Destroy(instantiatedObject);

            instantiatedHierarchyObjects.Clear();

            foreach (var gameObjectReference in gameObjectReferences)
                FillHierarchy(gameObjectReferences, gameObjectReference, hierarchyMemberPrefab);
        }

        static void FillHierarchy(List<GameObjectReference> gameObjectReferences, GameObjectReference gameObjectReference, GameObject hierarchyMemberPrefab, int depth = -1)
        {
            depth++;

            var hierarchyMemberGameObject = GameObject.Instantiate(hierarchyMemberPrefab, Tweach.hierarchyContentTransform);
            instantiatedHierarchyObjects.Add(hierarchyMemberGameObject);

            var hierarchyMemberText = hierarchyMemberGameObject.GetComponent<Text>();
            hierarchyMemberText.text = $" {DebugHelper.Indent(depth)}{gameObjectReference.value.name}";

            var hierarchyMemberButton = hierarchyMemberGameObject.GetComponent<Button>();
            hierarchyMemberButton.onClick.AddListener(() =>
            {
                gameObjectReference.expanded = !gameObjectReference.expanded;
                FillHierarchy(gameObjectReferences);
                InstantiateComponents(gameObjectReference);
            });

            if (gameObjectReference.expanded)
                foreach (var child in gameObjectReference.childGameObjectReferences)
                    FillHierarchy(gameObjectReferences, child, hierarchyMemberPrefab, depth);
        }

        static void ClearComponentsAndFieldsContent(INamedChild namedChild)
        {
            foreach (var instantiatedObject in instantiatedComponentsAndFieldsObjects)
                GameObject.Destroy(instantiatedObject);

            instantiatedComponentsAndFieldsObjects.Clear();

            Tweach.backButton.interactable = false;

            if (namedChild != null)
                Tweach.pathText.text = GetPathString(namedChild, null);
            else
                Tweach.pathText.text = "";
        }

        static string GetPathString(INamedChild namedChild, string path)
        {
            if (path == null)
                path = namedChild.GetName();
            else
                path = namedChild.GetName() + " / " + path;

            if (namedChild.GetParentAsINamedChild() != null)
                path = GetPathString(namedChild.GetParentAsINamedChild(), path);

            return path;
        }

        public static void InstantiateComponents(GameObjectReference gameObjectReference)
        {
            ClearComponentsAndFieldsContent(gameObjectReference);

            Tweach.backButton.interactable = true;
            Tweach.backButton.onClick.RemoveAllListeners();

            if (gameObjectReference.parentGameObjectReference != null)
                Tweach.backButton.onClick.AddListener(() => InstantiateComponents(gameObjectReference.parentGameObjectReference));
            else
                Tweach.backButton.onClick.AddListener(() => ClearComponentsAndFieldsContent(null));

            var classPrefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/Class.prefab", typeof(GameObject)) as GameObject;

            foreach (var componentReference in gameObjectReference.childComponentReferences)
            {
                var instantiatedComponentObject = GameObject.Instantiate(classPrefab, Tweach.componentsAndFieldsContentTransform);
                instantiatedComponentsAndFieldsObjects.Add(instantiatedComponentObject);

                var instantiatedComponentButton = instantiatedComponentObject.GetComponentInChildren<Button>();
                instantiatedComponentButton.onClick.AddListener(() => InstantiateFieldCollection(componentReference));

                var instantiatedComponentLabel = instantiatedComponentButton.GetComponentInChildren<Text>();
                instantiatedComponentLabel.text = componentReference.GetName();
            }
        }

        public static void InstantiateFieldCollection(IFieldCollection fieldCollection)
        {
            ClearComponentsAndFieldsContent(fieldCollection);

            Tweach.backButton.interactable = true;
            Tweach.backButton.onClick.RemoveAllListeners();

            if (fieldCollection is ComponentReference)
                Tweach.backButton.onClick.AddListener(() => InstantiateComponents((fieldCollection as ComponentReference).parentGameObjectReference));
            else
                Tweach.backButton.onClick.AddListener(() => InstantiateFieldCollection((fieldCollection as FieldReference).parentIFieldCollection));

            foreach (var fieldReference in fieldCollection.GetFields())
            {
                if (UiInitialization.Registry.ContainsKey(fieldReference.fieldInfo.FieldType))
                {
                    var uiComponent = InstantiateUiComponent(UiInitialization.Registry[fieldReference.fieldInfo.FieldType].prefabName);
                    uiComponent.nameLabel.text = fieldReference.fieldInfo.Name;
                    UiInitialization.Registry[fieldReference.fieldInfo.FieldType].init.Invoke(fieldReference, uiComponent);
                }
                else if (fieldReference.childFieldReferences != null && fieldReference.childFieldReferences.Count > 0)
                {
                    var uiComponent = InstantiateUiComponent("Class");
                    uiComponent.nameLabel.text = "BÖG";//fieldReference.fieldInfo.Name;
                    uiComponent.valueLabel.text = fieldReference.fieldInfo.FieldType.Name;
                    uiComponent.action = (v) => InstantiateFieldCollection(fieldReference);
                }
                else
                {
                    var uiComponent = InstantiateUiComponent("Unknown");
                    uiComponent.nameLabel.text = fieldReference.fieldInfo.Name;
                    uiComponent.valueLabel.text = fieldReference.fieldInfo.FieldType.Name;
                }
            }
        }

        static UiComponent InstantiateUiComponent(string prefabName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath($"{Tweach.baseTweachAssetPath}/UiPrefabs/Types/{prefabName}.prefab", typeof(GameObject)) as GameObject;

            var instantiatedObject = GameObject.Instantiate(prefab, Tweach.componentsAndFieldsContentTransform);
            
            instantiatedComponentsAndFieldsObjects.Add(instantiatedObject);

            var uiComponent = instantiatedObject.GetComponent<UiComponent>();

            return uiComponent;
        }
    }
}
