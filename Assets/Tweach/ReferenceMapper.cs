using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tweach
{
    public static class ReferenceMapper
    {
        public static List<GameObjectReference> GetRootGameObjectReferences(bool onlyMarkedWithTweachAttribute, bool hideFieldlessObjectsAndComponents)
        {
            var transforms = GameObject.FindObjectsOfType<Transform>();

            var gameObjectReferenceDictionary = new Dictionary<GameObject, GameObjectReference>();

            foreach (var t in transforms)
            {
                gameObjectReferenceDictionary.Add(t.gameObject, new GameObjectReference() {
                        value = t.gameObject
                    });
            }

            foreach (var t in transforms)
            {
                if (t.parent != null)
                    gameObjectReferenceDictionary[t.gameObject].parentReference = gameObjectReferenceDictionary[t.parent.gameObject];
                
                gameObjectReferenceDictionary[t.gameObject].childReferences = t.GetComponentsInChildren<Transform>()
                    .Where(ct => ct.parent == t)
                    .Select(ct => gameObjectReferenceDictionary[ct.gameObject])
                    .ToList();
            }

            var componentReferenceDictionary = new Dictionary<Component, ComponentReference>();

            foreach (var gameObjectReference in gameObjectReferenceDictionary.Values)
            {
                gameObjectReference.componentReferences = gameObjectReference.value.GetComponents<Component>()
                    .Select(c => new ComponentReference() {
                        value = c,
                        gameObjectReference = gameObjectReferenceDictionary[c.gameObject]
                    })
                    .ToList();

                foreach (var componentReference in gameObjectReference.componentReferences)
                {
                    componentReferenceDictionary.Add(componentReference.value, componentReference);
                }
            }

            foreach (var componentReference in componentReferenceDictionary.Values)
            {
                if (componentReference.fieldReferences == null)
                    componentReference.fieldReferences = new List<FieldReference>();

                GetFields(onlyMarkedWithTweachAttribute, componentReference.fieldReferences, componentReference.value, componentReferenceDictionary, gameObjectReferenceDictionary);
            }

            var rootGameObjectReferences = gameObjectReferenceDictionary.Values.Where(g => g.parentReference == null).ToList();

            return hideFieldlessObjectsAndComponents 
                ? rootGameObjectReferences.Where(gameObjectReference => HasFieldsRecursive(gameObjectReference, true)).ToList()
                : rootGameObjectReferences;
        }

        static bool HasFieldsRecursive(GameObjectReference item, bool removeComponentsWithoutFields)
        {
            var count = item.componentReferences.Count;
            for (int i = 0; i < count; i++)
            {
                if (item.componentReferences[i].fieldReferences.Count > 0)
                {
                    return true;
                }
                else if (removeComponentsWithoutFields)
                {
                    item.componentReferences.RemoveAt(i);
                    i--;
                    count--;
                }
            }

            foreach (var gameObjectReferenceChild in item.childReferences)
            {
                if (HasFieldsRecursive(gameObjectReferenceChild, removeComponentsWithoutFields))
                    return true;
            }

            return false;
        }

        static void GetFields(bool onlyMarkedWithTweachAttribute, List<FieldReference> siblings, object parent, Dictionary<Component, ComponentReference> componentReferenceDictionary, Dictionary<GameObject, GameObjectReference> gameObjectReferenceDictionary, int depth = 0)
        {
            depth++;
            if (depth > 10)
                throw new System.Exception("Serialization depth limit reached. Recursive reference?");

            var fieldInfos = parent
                .GetType()
                .GetFields(Tweach.bindingFlags)
                .Where(f => !f.GetCustomAttributes<HideInInspector>().Any())
                .Where(f => !onlyMarkedWithTweachAttribute || f.GetCustomAttributes<TweachAttribute>().Any());

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldValue = fieldInfo.GetValue(parent);

                if (fieldValue == null)
                    continue;

                var fieldReference = new FieldReference()
                {
                    owner = parent,
                    fieldInfo = fieldInfo
                };

                if (fieldValue is Component)
                {
                    if ((Component)fieldValue != null)
                        fieldReference.value = componentReferenceDictionary[(Component)fieldValue];
                }
                else if (fieldValue is GameObject)
                {
                    if ((GameObject)fieldValue != null)
                        fieldReference.value = gameObjectReferenceDictionary[(GameObject)fieldValue];
                }
                else
                {
                    fieldReference.value = fieldValue;
                    
                    if (!fieldInfo.FieldType.IsPrimitive && fieldInfo.FieldType != typeof(string))
                    {
                        if (fieldReference.children == null)
                            fieldReference.children = new List<FieldReference>();

                        GetFields(onlyMarkedWithTweachAttribute, fieldReference.children, fieldValue, componentReferenceDictionary, gameObjectReferenceDictionary, depth);
                    }
                }

                siblings.Add(fieldReference);
            }
        }

        public static Reference GetRootReferenceList(BindingFlags bindingFlags)
        {
            var rootReference = new Reference();

            rootReference.name = "Root";

            var gameObjects = GameObject.FindObjectsOfType<GameObject>();

            var references = new Dictionary<object, Reference>();

            foreach (var gameObject in gameObjects)
            {
                
                // if (gameObject == this.gameObject)
                //     continue;

                var gameObjectReference = new Reference();

                gameObjectReference.name = gameObject.name;
                gameObjectReference.value = gameObject;
                gameObjectReference.owner = rootReference;

                rootReference.children.Add(gameObjectReference);

                if (references.ContainsKey(gameObject))
                    references[gameObject] = gameObjectReference;
                else
                    references.Add(gameObject, gameObjectReference);

                foreach (var component in gameObject.GetComponents<Component>())
                {
                    if (references.ContainsKey(component))
                    {
                        references[component].name = $"{component.GetType().Name}";
                        references[component].owner = gameObjectReference;

                        gameObjectReference.children.Add(references[component]);
                    }
                    else
                    {
                        var componentReference = new Reference();

                        componentReference.name = $"{component.GetType().Name}";
                        componentReference.value = component;
                        componentReference.owner = gameObjectReference;

                        gameObjectReference.children.Add(componentReference);
                        references.Add(component, componentReference);

                        GetSubMembers(componentReference, bindingFlags, references);
                    }
                }

            }

            return rootReference;
        }

        static void GetSubMembers(Reference owner, BindingFlags bindingFlags, Dictionary<object, Reference> references, int depth = 0)
        {
            depth++;
            if (depth > 10)
                throw new System.Exception("Serialization depth limit reached. Recursive reference?");

            var fields = owner.value
                .GetType()
                .GetFields(bindingFlags)
                .Where(f => !f.CustomAttributes.Any(a => a.GetType() == typeof(HideInInspector)));

            foreach (var fieldInfo in fields)
            {
                var fieldValue = fieldInfo.GetValue(owner.value);

                if (fieldValue == null)
                    continue;

                if (references.ContainsKey(fieldValue))
                {   
                    owner.children.Add(references[fieldValue]);
                }
                else
                {
                    var reference = new Reference()
                    {
                        name = fieldInfo.Name,
                        value = fieldValue,
                        owner = owner,
                        fieldInfo = fieldInfo
                    };

                    references.Add(fieldValue, reference);

                    owner.children.Add(reference);

                    var refType = reference.value.GetType();

                    if (!refType.IsPrimitive
                        && refType != typeof(string))
                        GetSubMembers(reference, bindingFlags, references, depth);
                }
            }
        }
    }
}