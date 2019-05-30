using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tweach
{
    public static class ReferenceMapper
    {
        static Dictionary<Component, ComponentReference> componentReferenceDictionary;
        static Dictionary<GameObject, GameObjectReference> gameObjectReferenceDictionary;

        public static List<GameObjectReference> GetRootGameObjectReferences()
        {
            var transforms = new List<Transform>();

            foreach (var g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                transforms.AddRange(g.GetComponentsInChildren<Transform>()); //This may need work. Seems to cap at 200 objects
            }

            gameObjectReferenceDictionary = new Dictionary<GameObject, GameObjectReference>();

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

            componentReferenceDictionary = new Dictionary<Component, ComponentReference>();

            foreach (var gameObjectReference in gameObjectReferenceDictionary.Values)
            {
                gameObjectReference.childComponentReferences = gameObjectReference.value.GetComponents<Component>()
                    .Select(c => new ComponentReference(c, gameObjectReferenceDictionary[c.gameObject]))
                    .ToList();

                foreach (var componentReference in gameObjectReference.childComponentReferences)
                {
                    componentReferenceDictionary.Add(componentReference.value, componentReference);
                }

                MapMembers(gameObjectReference);

                Tweach.mappedGameObjectCount++;
            }

            foreach (var componentReference in componentReferenceDictionary.Values)
            {
                MapMembers(componentReference);

                Tweach.mappedComponentCount++;
            }

            var rootGameObjectReferences = gameObjectReferenceDictionary.Values.Where(g => g.parentGameObjectReference == null).ToList();

            componentReferenceDictionary = null;
            gameObjectReferenceDictionary = null;

            return Tweach.hideMemberlessObjectsAndComponents
                ? rootGameObjectReferences.Where(gameObjectReference => HasMembersRecursive(gameObjectReference, true)).ToList()
                : rootGameObjectReferences;
        }

        static bool HasMembersRecursive(GameObjectReference gameObjectReference, bool removeComponentsWithoutFields)
        {
            var count = gameObjectReference.childComponentReferences.Count;

            for (int i = 0; i < count; i++)
            {
                if (gameObjectReference.childComponentReferences[i].childMemberReferences.Count > 0)
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
                value = GetMemberValue(memberInfo, parentValue);
                return true;
            }
            catch
            {
                // Debug.LogWarning($"Failed getting member {memberInfo.Name} from {parentValue}");
                Tweach.mappedMemberCount--;
                Tweach.failedMemberCount++;
                value = null;
                return false;
            }
        }

        static object GetMemberValue(MemberInfo memberInfo, object parentValue)
        {
            Tweach.mappedMemberCount++;

            if (memberInfo is FieldInfo)
                return (memberInfo as FieldInfo).GetValue(parentValue);
            else
                return (memberInfo as PropertyInfo).GetValue(parentValue);
        }

        static void MapMembers(IReference parentReference, int depth = 0)
        {
            depth++;
            if (depth > 20)
                throw new System.Exception("Serialization depth limit reached. Recursive reference?");

            var memberInfos = new List<MemberInfo>();

            if (Tweach.mapFields)
                memberInfos.AddRange(parentReference.GetValue()
                    .GetType()
                    .GetFields(Tweach.GetBindingFlags())
                    .Where(f => 
                    {
                        var att = f.GetCustomAttributes();
                        return !Tweach.respectHideInInspector || !att.Any(a => a.GetType() == typeof(HideInInspector))
                            && !Tweach.mapOnlyMembersMarkedWithTweach || att.Any(a => a.GetType() == typeof(TweachAttribute));
                    })
                    .Select(f => f as MemberInfo));

            if (Tweach.mapProperties)
                memberInfos.AddRange(parentReference.GetValue()
                    .GetType()
                    .GetProperties(Tweach.GetBindingFlags())
                    .Where(p =>
                    {
                        var acc = p.GetAccessors();
                        var att = p.GetCustomAttributes();
                        return acc.Any(a => a.Name == $"get_{p.Name}")
                            && acc.Any(a => a.Name == $"set_{p.Name}")
                            && !Tweach.respectHideInInspector || !att.Any(a => a.GetType() == typeof(HideInInspector))
                            && !Tweach.mapOnlyMembersMarkedWithTweach || att.Any(a => a.GetType() == typeof(TweachAttribute));
                    })
                    .Select(p => p as MemberInfo));

            foreach (var memberInfo in memberInfos)
            {
                object memberValue;

                if (Tweach.perilousMode)
                    memberValue = GetMemberValue(memberInfo, parentReference.GetValue());
                else if (!TryGetMemberValue(memberInfo, parentReference.GetValue(), out memberValue))
                    continue;

                var memberReference = new MemberReference(parentReference, memberInfo);

                if (memberValue is Component && (Component)memberValue != null && componentReferenceDictionary.ContainsKey((Component)memberValue))
                {
                    memberReference.value = componentReferenceDictionary[(Component)memberValue];
                }
                else if (memberValue is GameObject && (GameObject)memberValue != null && gameObjectReferenceDictionary.ContainsKey((GameObject)memberValue))
                {
                    memberReference.value = gameObjectReferenceDictionary[(GameObject)memberValue];
                }
                else
                {
                    memberReference.value = memberValue;

                    if (memberValue != null && !memberReference.GetMemberType().IsPrimitive && memberReference.GetMemberType() != typeof(string))
                    {
                        MapMembers(memberReference, depth);
                    }
                }

                parentReference.AddMember(memberReference);
            }
        }
    }
}