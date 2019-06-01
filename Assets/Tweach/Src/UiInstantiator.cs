using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public class UiInstantiator
    {
        public UiInstantiator(string baseTweachAssetPath,
                               Transform hierarchyContentTransform,
                               Transform componentsAndFieldsContentTransform,
                               Vector2Int gridSize,
                               Button backButton,
                               Text pathText)
        {
            this.baseTweachAssetPath = baseTweachAssetPath;
            this.hierarchyContentTransform = hierarchyContentTransform;
            this.componentsAndFieldsContentTransform = componentsAndFieldsContentTransform;
            this.gridSize = gridSize;
            this.backButton = backButton;
            this.pathText = pathText;
        }

        readonly string baseTweachAssetPath;
        readonly Transform hierarchyContentTransform;
        readonly Transform componentsAndFieldsContentTransform;
        readonly Vector2Int gridSize;
        readonly Button backButton;
        readonly Text pathText;

        readonly static List<GameObject> instantiatedHierarchyObjects = new List<GameObject>();
        readonly static List<GameObject> instantiatedComponentsAndFieldsObjects = new List<GameObject>();

        public void FillHierarchy(List<GameObjectReference> gameObjectReferences)
        {
            var hierarchyMemberPrefab = AssetDatabase.LoadAssetAtPath($"{baseTweachAssetPath}/UiPrefabs/HierarchyMember.prefab", typeof(GameObject)) as GameObject;

            foreach (var instantiatedObject in instantiatedHierarchyObjects)
                GameObject.Destroy(instantiatedObject);

            instantiatedHierarchyObjects.Clear();

            foreach (var gameObjectReference in gameObjectReferences)
                FillHierarchy(gameObjectReferences, gameObjectReference, hierarchyMemberPrefab);
        }

        void FillHierarchy(List<GameObjectReference> gameObjectReferences, GameObjectReference gameObjectReference, GameObject hierarchyMemberPrefab, int depth = -1)
        {
            if (!gameObjectReference.matchesSearchQuery)
                return;

            depth++;

            var hierarchyMemberGameObject = GameObject.Instantiate(hierarchyMemberPrefab, hierarchyContentTransform);
            instantiatedHierarchyObjects.Add(hierarchyMemberGameObject);

            var hierarchyMemberText = hierarchyMemberGameObject.GetComponent<Text>();
            hierarchyMemberText.text = $" {Utilities.Indent(depth)}{gameObjectReference.value.name}";

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

        void ClearComponentsAndFieldsContent(IReference reference)
        {
            var viewPortRect = componentsAndFieldsContentTransform.GetComponentInParent<Mask>().rectTransform.rect;

            componentsAndFieldsContentTransform.GetComponentInParent<GridLayoutGroup>().cellSize = new Vector2(viewPortRect.width / gridSize.x, viewPortRect.height / gridSize.y);

            foreach (var instantiatedObject in instantiatedComponentsAndFieldsObjects)
                GameObject.Destroy(instantiatedObject);

            instantiatedComponentsAndFieldsObjects.Clear();

            pathText.text = GetPathString(reference, null);

            SetBackButtonStuff(reference);
        }

        static string GetPathString(IReference reference, string path)
        {
            if (reference == null)
                return "";

            if (path == null)
                path = reference.GetName();
            else
                path = reference.GetName() + " / " + path;

            if (reference.GetParentReference() != null)
                path = GetPathString(reference.GetParentReference(), path);

            return path;
        }

        public void InstantiateComponents(GameObjectReference gameObjectReference)
        {
            ClearComponentsAndFieldsContent(gameObjectReference);

            foreach (var componentReference in gameObjectReference.childComponentReferences)
            {
                var uiComponent = InstantiateUiComponent("Component");
                uiComponent.valueLabel.text = componentReference.value.GetType().Name;
                uiComponent.action = (v) => InstantiateMemberCollection(componentReference);
            }

            InstantiateMemberCollection(gameObjectReference, false);
        }

        public void InstantiateMemberCollection(IReference fieldCollection, bool clearContent = true)
        {
            if (clearContent)
                ClearComponentsAndFieldsContent(fieldCollection);

            var memberReferences = fieldCollection.GetMembers();

            if (memberReferences != null)
            {
                foreach (var memberReference in memberReferences)
                {
                    var type = memberReference.GetMemberType();

                    if (memberReference.value != null && UiInitializers.Registry.ContainsKey(type))
                    {
                        var uiComponent = InstantiateUiComponent(UiInitializers.Registry[memberReference.value.GetType()].prefabName);
                        uiComponent.nameLabel.text = memberReference.GetName();
                        UiInitializers.Registry[type].init.Invoke(memberReference, uiComponent);
                    }
                    else if (memberReference.childMemberReferences != null && memberReference.childMemberReferences.Count > 0)
                    {
                        var uiComponent = InstantiateUiComponent("Class");
                        uiComponent.nameLabel.text = memberReference.GetName();
                        uiComponent.valueLabel.text = memberReference.GetMemberType().Name;
                        uiComponent.action = (v) => InstantiateMemberCollection(memberReference);
                    }
                    else if (type.IsEnum)
                    {
                        var uiComponent = InstantiateUiComponent("Class");
                        uiComponent.nameLabel.text = memberReference.GetName();
                        uiComponent.valueLabel.text = memberReference.GetMemberType().Name;
                        uiComponent.action = (v) => InstantiateEnumValueCollection(memberReference);
                    }
                    else
                    {
                        var uiComponent = InstantiateUiComponent("Unknown");
                        uiComponent.nameLabel.text = memberReference.GetName();
                        if (memberReference.value == null)
                            uiComponent.valueLabel.text = memberReference.GetMemberType().Name + " (Null)";
                        else
                            uiComponent.valueLabel.text = memberReference.GetMemberType().Name;
                    }
                }
            }
        }

        void SetBackButtonStuff(IReference reference)
        {
            if (reference == null)
            {
                backButton.interactable = false;
            }
            else
            {
                var parentReference = reference.GetParentReference();

                if (parentReference == null)
                {
                    ClearComponentsAndFieldsContent(null);
                }
                else
                {
                    backButton.interactable = true;

                    backButton.onClick.RemoveAllListeners();

                    if (parentReference is GameObjectReference)
                        backButton.onClick.AddListener(() => InstantiateComponents(parentReference as GameObjectReference));
                    else if (parentReference is ComponentReference)
                        backButton.onClick.AddListener(() => InstantiateComponents((parentReference as ComponentReference).parentGameObjectReference));
                    else
                        backButton.onClick.AddListener(() => InstantiateMemberCollection((parentReference as MemberReference).parentIReference));
                }
            }
        }

        void InstantiateEnumValueCollection(MemberReference enumMember)
        {
            ClearComponentsAndFieldsContent(enumMember);

            var names = Enum.GetNames(enumMember.GetMemberType());
            var value = (int)enumMember.GetValue();

            for (int i = 0; i < names.Length; i++)
            {
                var uiComponent = InstantiateUiComponent("bool");

                uiComponent.nameLabel.text = names[i];
                uiComponent.toggle.isOn = i == value;

                int valueCopyOfI = i; //this was a fun bug to find
                
                uiComponent.action = (v) =>
                {
                    enumMember.PushValue(valueCopyOfI);
                    InstantiateEnumValueCollection(enumMember);
                };
            }
        }

        UiComponent InstantiateUiComponent(string prefabName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath($"{baseTweachAssetPath}/UiPrefabs/Types/{prefabName}.prefab", typeof(GameObject)) as GameObject;

            var instantiatedObject = GameObject.Instantiate(prefab, componentsAndFieldsContentTransform);

            instantiatedComponentsAndFieldsObjects.Add(instantiatedObject);

            var uiComponent = instantiatedObject.GetComponent<UiComponent>();

            return uiComponent;
        }
    }
}
