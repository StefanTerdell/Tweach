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
            var transforms = new List<Transform>();
            
            foreach (var g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                transforms.AddRange(g.GetComponentsInChildren<Transform>());
            }

            var gameObjectReferenceDictionary = new Dictionary<GameObject, GameObjectReference>();

            foreach (var t in transforms)
            {
                gameObjectReferenceDictionary.Add(t.gameObject, new GameObjectReference(t.gameObject));
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
                    .Select(c => new ComponentReference(c, gameObjectReferenceDictionary[c.gameObject]))
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

                GetFields(onlyMarkedWithTweachAttribute, componentReference, componentReference.value, componentReferenceDictionary, gameObjectReferenceDictionary);
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

        static void GetFields(bool onlyMarkedWithTweachAttribute, IFieldCollection parentIFieldCollection, object parentValue, Dictionary<Component, ComponentReference> componentReferenceDictionary, Dictionary<GameObject, GameObjectReference> gameObjectReferenceDictionary, int depth = 0)
        {
            depth++;
            if (depth > 10)
                throw new System.Exception("Serialization depth limit reached. Recursive reference?");

            var fieldInfos = parentValue
                .GetType()
                .GetFields(Tweach.GetBindingFlags())
                .Where(f => !f.GetCustomAttributes<HideInInspector>().Any())
                .Where(f => !onlyMarkedWithTweachAttribute || f.GetCustomAttributes<TweachAttribute>().Any());

            foreach (var fieldInfo in fieldInfos)
            {
                var fieldValue = fieldInfo.GetValue(parentValue);

                var fieldReference = new FieldReference(parentIFieldCollection, parentValue, fieldInfo);

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
                else if (fieldValue != null)
                {
                    fieldReference.value = fieldValue;
                    
                    if (!fieldInfo.FieldType.IsPrimitive && fieldInfo.FieldType != typeof(string))
                    {
                        if (fieldReference.children == null)
                            fieldReference.children = new List<FieldReference>();

                        GetFields(onlyMarkedWithTweachAttribute, fieldReference, fieldValue, componentReferenceDictionary, gameObjectReferenceDictionary, depth);
                    }
                }
                else
                {
                    fieldReference.value = fieldValue;
                }

                parentIFieldCollection.GetFields().Add(fieldReference);
            }
        }
    }
}