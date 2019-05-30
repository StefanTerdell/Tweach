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
            if (!gameObjectReference.matchesSearchQuery)
                return;

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

        static void ClearComponentsAndFieldsContent(IReference reference)
        {
            var viewPortRect = Tweach.componentsAndFieldsContentTransform.GetComponentInParent<Mask>().rectTransform.rect;
            Tweach.componentsAndFieldsContentTransform.GetComponentInParent<GridLayoutGroup>().cellSize =
                new Vector2(viewPortRect.width / Tweach.gridSize.x, viewPortRect.height / Tweach.gridSize.y);

            foreach (var instantiatedObject in instantiatedComponentsAndFieldsObjects)
                GameObject.Destroy(instantiatedObject);

            instantiatedComponentsAndFieldsObjects.Clear();

            Tweach.backButton.interactable = false;

            if (reference != null)
                Tweach.pathText.text = GetPathString(reference, null);
            else
                Tweach.pathText.text = "";
        }

        static string GetPathString(IReference reference, string path)
        {
            if (path == null)
                path = reference.GetName();
            else
                path = reference.GetName() + " / " + path;

            if (reference.GetParentReference() != null)
                path = GetPathString(reference.GetParentReference(), path);

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

            foreach (var componentReference in gameObjectReference.childComponentReferences)
            {
                var uiComponent = InstantiateUiComponent("Component");
                uiComponent.valueLabel.text = componentReference.value.GetType().Name;
                uiComponent.action = (v) => InstantiateMemberCollection(componentReference);
            }

            InstantiateMemberCollection(gameObjectReference, false);
        }

        public static void InstantiateMemberCollection(IReference fieldCollection, bool clearContent = true)
        {
            if (clearContent)
            {
                ClearComponentsAndFieldsContent(fieldCollection);

                Tweach.backButton.interactable = true;
                Tweach.backButton.onClick.RemoveAllListeners();

                if (fieldCollection is ComponentReference)
                    Tweach.backButton.onClick.AddListener(() => InstantiateComponents((fieldCollection as ComponentReference).parentGameObjectReference));
                else
                    Tweach.backButton.onClick.AddListener(() => InstantiateMemberCollection((fieldCollection as MemberReference).parentIReference));
            }

            foreach (var memberReference in fieldCollection.GetMembers())
            {
                if (memberReference.value != null && UiInitialization.Registry.ContainsKey(memberReference.value.GetType()))
                {
                    var uiComponent = InstantiateUiComponent(UiInitialization.Registry[memberReference.value.GetType()].prefabName);
                    uiComponent.nameLabel.text = memberReference.GetName();
                    UiInitialization.Registry[memberReference.value.GetType()].init.Invoke(memberReference, uiComponent);
                }
                else if (memberReference.childMemberReferences != null && memberReference.childMemberReferences.Count > 0)
                {
                    var uiComponent = InstantiateUiComponent("Class");
                    uiComponent.nameLabel.text = memberReference.GetName();
                    uiComponent.valueLabel.text = memberReference.GetMemberType().Name;
                    uiComponent.action = (v) => InstantiateMemberCollection(memberReference);
                }
                else
                {
                    var uiComponent = InstantiateUiComponent("Unknown");
                    uiComponent.nameLabel.text = memberReference.GetName();
                    uiComponent.valueLabel.text = memberReference.GetMemberType().Name;
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
