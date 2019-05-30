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
                transforms.AddRange(g.GetComponentsInChildren<Transform>()); //This may need work. Seems to cap at 100 objects
            }

            var gameObjectReferenceDictionary = new Dictionary<GameObject, GameObjectReference>();

            foreach (var t in transforms)
            {
                gameObjectReferenceDictionary.Add(t.gameObject, new GameObjectReference(t.gameObject));
            }

            foreach (var t in transforms)
            {
                if (t.parent != null)
                    gameObjectReferenceDictionary[t.gameObject].parentGameObjectReference = gameObjectReferenceDictionary[t.parent.gameObject];

                gameObjectReferenceDictionary[t.gameObject].childGameObjectReferences = t.GetComponentsInChildren<Transform>()
                    .Where(ct => ct.parent == t)
                    .Select(ct => gameObjectReferenceDictionary[ct.gameObject])
                    .ToList();
            }

            var componentReferenceDictionary = new Dictionary<Component, ComponentReference>();

            foreach (var gameObjectReference in gameObjectReferenceDictionary.Values)
            {
                gameObjectReference.childComponentReferences = gameObjectReference.value.GetComponents<Component>()
                    .Select(c => new ComponentReference(c, gameObjectReferenceDictionary[c.gameObject]))
                    .ToList();

                foreach (var componentReference in gameObjectReference.childComponentReferences)
                {
                    componentReferenceDictionary.Add(componentReference.value, componentReference);
                }
            }

            foreach (var componentReference in componentReferenceDictionary.Values)
            {
                if (componentReference.childFieldReferences == null)
                    componentReference.childFieldReferences = new List<MemberReference>();

                MapMembers(onlyMarkedWithTweachAttribute, componentReference, componentReference.value, componentReferenceDictionary, gameObjectReferenceDictionary);
            }

            var rootGameObjectReferences = gameObjectReferenceDictionary.Values.Where(g => g.parentGameObjectReference == null).ToList();

            return hideFieldlessObjectsAndComponents
                ? rootGameObjectReferences.Where(gameObjectReference => HasMembersRecursive(gameObjectReference, true)).ToList()
                : rootGameObjectReferences;
        }

        static bool HasMembersRecursive(GameObjectReference gameObjectReference, bool removeComponentsWithoutFields)
        {
            var count = gameObjectReference.childComponentReferences.Count;

            for (int i = 0; i < count; i++)
            {
                if (gameObjectReference.childComponentReferences[i].childFieldReferences.Count > 0)
                {
                    return true;
                }
                else if (removeComponentsWithoutFields)
                {
                    gameObjectReference.childComponentReferences.RemoveAt(i);
                    i--;
                    count--;
                }
            }

            foreach (var gameObjectReferenceChild in gameObjectReference.childGameObjectReferences)
                if (HasMembersRecursive(gameObjectReferenceChild, removeComponentsWithoutFields))
                    return true;

            return false;
        }

        static bool TryGetMemberValue(MemberInfo memberInfo, object parentValue, out object value)
        {
            try
            {
                if (memberInfo is FieldInfo)
                    value = (memberInfo as FieldInfo).GetValue(parentValue);
                else
                    value = (memberInfo as PropertyInfo).GetValue(parentValue);
                    
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        static void MapMembers(bool onlyMarkedWithTweachAttribute, IMemberCollection parentIMemberCollection, object parentValue, Dictionary<Component, ComponentReference> componentReferenceDictionary, Dictionary<GameObject, GameObjectReference> gameObjectReferenceDictionary, int depth = 0)
        {
            depth++;
            if (depth > 20)
                throw new System.Exception("Serialization depth limit reached. Recursive reference?");

            var memberInfos = new List<MemberInfo>();

            if (Tweach.mapFields)
                memberInfos.AddRange(parentValue
                    .GetType()
                    .GetFields(Tweach.GetBindingFlags())
                    .Where(f => !f.GetCustomAttributes<HideInInspector>().Any())
                    .Where(f => !onlyMarkedWithTweachAttribute || f.GetCustomAttributes<TweachAttribute>().Any())
                    .Select(f => f as MemberInfo));

            if (Tweach.mapProperties)
                memberInfos.AddRange(parentValue
                    .GetType()
                    .GetProperties(Tweach.GetBindingFlags())
                    .Where(p => p.GetAccessors().Any(a => a.Name == $"get_{p.Name}")
                             && p.GetAccessors().Any(a => a.Name == $"set_{p.Name}"))
                    .Where(p => !p.GetCustomAttributes<HideInInspector>().Any())
                    .Where(p => !onlyMarkedWithTweachAttribute || p.GetCustomAttributes<TweachAttribute>().Any())
                    .Select(p => p as MemberInfo));

            foreach (var memberInfo in memberInfos)
            {
                if (!TryGetMemberValue(memberInfo, parentValue, out var memberValue))
                    return;

                var memberReference = new MemberReference(parentIMemberCollection, parentValue, memberInfo);

                if (memberValue is Component && (Component)memberValue != null)
                {
                    memberReference.value = componentReferenceDictionary[(Component)memberValue];
                }
                else if (memberValue is GameObject && (GameObject)memberValue != null)
                {
                    memberReference.value = gameObjectReferenceDictionary[(GameObject)memberValue];
                }
                else
                {
                    memberReference.value = memberValue;

                    if (memberValue != null && !memberReference.GetMemberType().IsPrimitive && memberReference.GetMemberType() != typeof(string))
                    {
                        if (memberReference.childMemberReferences == null)
                            memberReference.childMemberReferences = new List<MemberReference>();

                        MapMembers(onlyMarkedWithTweachAttribute, memberReference, memberValue, componentReferenceDictionary, gameObjectReferenceDictionary, depth);
                    }
                }

                parentIMemberCollection.GetMembers().Add(memberReference);
            }
        }
    }
}