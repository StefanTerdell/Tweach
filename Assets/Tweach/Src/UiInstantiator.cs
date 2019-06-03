using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tweach
{
    public partial class UiInstantiator
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
            hierarchyMemberText.text = $" {Utilities.Indent(depth)}{gameObjectReference.GetName()}";

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

            pathText.text = GetPathString(reference);

            SetBackButton(reference);
        }

        static string GetPathString(IReference reference, string path = null)
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
                var uiComponent = InstantiateUiComponent("Component", componentReference.GetTypeName());
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
                    var name = memberReference.GetName();

                    if (memberReference.GetValue() != null && UiInitializers.Registry.ContainsKey(type))
                    {
                        var uiComponent = InstantiateUiComponent(UiInitializers.Registry[memberReference.GetMemberType()].prefabName, name);
                        UiInitializers.Registry[type].init.Invoke(this, memberReference, uiComponent);
                    }
                    else if (memberReference.GetMembers() != null && memberReference.GetMembers().Count > 0)
                    {
                        var uiComponent = InstantiateUiComponent("Class", name, type.Name);
                        uiComponent.action = (v) => InstantiateMemberCollection(memberReference);
                    }
                    else if (type.IsEnum && type.GetEnumUnderlyingType() == typeof(int))
                    {
                        var uiComponent = InstantiateUiComponent("Class", name, type.Name);
                        uiComponent.action = (v) => InstantiateEnumValueCollection(memberReference);
                    }
                    else
                    {
                        InstantiateUiComponent("Unknown", name, memberReference.GetValue() == null ? type.Name + " (Null)" : type.Name);
                    }
                }
            }
        }

        void SetBackButton(IReference reference)
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
                        backButton.onClick.AddListener(() => InstantiateMemberCollection(parentReference as ComponentReference));
                    else
                        backButton.onClick.AddListener(() => InstantiateMemberCollection(parentReference as MemberReference));
                }
            }
        }

        void InstantiateEnumValueCollection(MemberReference enumMember)
        {
            ClearComponentsAndFieldsContent(enumMember);

            var names = Enum.GetNames(enumMember.GetMemberType());
            var value = enumMember.GetValue();
            var hasFlags = enumMember.GetMemberType().GetCustomAttributes().Any(a => a.GetType() == typeof(FlagsAttribute));

            for (int i = 0; i < names.Length; i++)
            {
                var uiComponent = InstantiateUiComponent("bool", names[i]);
                var enumValueOfI = Enum.GetValues(value.GetType()).GetValue(i);
                var isDefault = (int)enumValueOfI == 0;
                var flagged = ((Enum)value).HasFlag((Enum)enumValueOfI) && !(isDefault && (int)value != 0);

                uiComponent.toggle.isOn = hasFlags ? flagged : (int)enumValueOfI == (int)value;

                uiComponent.action = (v) =>
                {
                    if (hasFlags && !isDefault)
                        if (flagged)
                            enumMember.PushValue((int)value - (int)enumValueOfI);
                        else
                            enumMember.PushValue((int)value + (int)enumValueOfI);
                    else
                        enumMember.PushValue((int)enumValueOfI);

                    InstantiateEnumValueCollection(enumMember);
                };
            }
        }

        UiComponent InstantiateUiComponent(string prefabName, string name, string valueLabel = null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath($"{baseTweachAssetPath}/UiPrefabs/Types/{prefabName}.prefab", typeof(GameObject)) as GameObject;

            var instantiatedObject = GameObject.Instantiate(prefab, componentsAndFieldsContentTransform);

            instantiatedComponentsAndFieldsObjects.Add(instantiatedObject);

            var uiComponent = instantiatedObject.GetComponent<UiComponent>();

            uiComponent.nameLabel.text = name;

            if (valueLabel != null)
                uiComponent.valueLabel.text = valueLabel;

            return uiComponent;
        }
    }
}
