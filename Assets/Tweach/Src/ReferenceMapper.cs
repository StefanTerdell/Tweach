using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tweach
{
    public class ReferenceMapper
    {
        public ReferenceMapper(bool hideMemberlessObjectsAndComponents,
                               bool respectHideInInspector,
                               bool mapOnlyMembersMarkedWithTweach,
                               bool mapPublic,
                               bool mapNonPublic,
                               bool mapFields,
                               bool mapProperties,
                               bool mapInstance,
                               bool mapStatic,
                               int maxDepth)
        {
            componentReferenceDictionary = new Dictionary<Component, ComponentReference>();
            gameObjectReferenceDictionary = new Dictionary<GameObject, GameObjectReference>();

            this.hideMemberlessObjectsAndComponents = hideMemberlessObjectsAndComponents;
            this.respectHideInInspector = respectHideInInspector;
            this.mapOnlyMembersMarkedWithTweach = mapOnlyMembersMarkedWithTweach;
            this.mapPublic = mapPublic;
            this.mapNonPublic = mapNonPublic;
            this.mapFields = mapFields;
            this.mapProperties = mapProperties;
            this.mapInstance = mapInstance;
            this.mapStatic = mapStatic;
            this.maxDepth = maxDepth;
        }

        readonly bool hideMemberlessObjectsAndComponents;
        readonly bool respectHideInInspector;
        readonly bool mapOnlyMembersMarkedWithTweach;
        readonly bool mapPublic;
        readonly bool mapNonPublic;
        readonly bool mapFields;
        readonly bool mapProperties;
        readonly bool mapInstance;
        readonly bool mapStatic;
        readonly int maxDepth;

        Dictionary<Component, ComponentReference> componentReferenceDictionary;
        Dictionary<GameObject, GameObjectReference> gameObjectReferenceDictionary;

        int mappedGameObjectCount;
        int mappedComponentCount;
        int mappedMemberCount;
        int failedMemberCount;

        public List<GameObjectReference> GetRootGameObjectReferences(bool logMapTime)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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

                mappedGameObjectCount++;
            }

            foreach (var componentReference in componentReferenceDictionary.Values)
            {
                MapMembers(componentReference);

                mappedComponentCount++;
            }

            var rootGameObjectReferences = gameObjectReferenceDictionary.Values
                .Where(g => (!hideMemberlessObjectsAndComponents || Utilities.HasMembersRecursive(g, true)) && g.parentGameObjectReference == null).ToList();

            sw.Stop();

            if (logMapTime)
                Debug.Log($"Mapped {mappedGameObjectCount} GameObjects, {mappedComponentCount} Components and {mappedMemberCount} Members in {sw.ElapsedMilliseconds}ms. Failed to get value of {failedMemberCount} Members.");

            return rootGameObjectReferences;
        }

        bool TryGetMemberValue(MemberInfo memberInfo, object parentValue, out object value)
        {
            try
            {
                if (memberInfo is FieldInfo)
                    value = (memberInfo as FieldInfo).GetValue(parentValue);
                else
                    value = (memberInfo as PropertyInfo).GetValue(parentValue);

                mappedMemberCount++;

                return true;
            }
            catch
            {
                value = null;

                failedMemberCount++;

                return false;
            }
        }

        void MapMembers(IReference parentReference, int depth = 0)
        {
            depth++;
            if (depth > maxDepth)
                throw new System.Exception("Serialization depth limit reached. Recursive reference?\nTrace:\n" + Utilities.UpwardsDebugTraceRecursive(parentReference, depth));

            var memberInfos = parentReference.GetValue()
                .GetType()
                .GetMembers(Utilities.GetBindingFlags(mapInstance, mapStatic, mapPublic, mapNonPublic))
                .Where(m => EvaluateMemberInfo(m));

            foreach (var memberInfo in memberInfos)
            {
                if (!TryGetMemberValue(memberInfo, parentReference.GetValue(), out var memberValue))
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

                    if (memberValue != null)
                    {
                        var type = memberReference.GetMemberType();
                        
                        if (!type.IsPrimitive && type != typeof(string) && !type.IsEnum)
                        {
                            MapMembers(memberReference, depth);
                        }
                    }
                }

                parentReference.AddMember(memberReference);
            }
        }

        bool EvaluateMemberInfo(MemberInfo m)
        {
            if (m is FieldInfo)
            {
                if (!mapFields)
                    return false;

                var f = m as FieldInfo;

                if (f.Attributes.HasFlag(FieldAttributes.InitOnly) || f.Attributes.HasFlag(FieldAttributes.Literal))
                    return false;
            }
            else if (m is PropertyInfo)
            {
                if (!mapProperties)
                    return false;

                var p = m as PropertyInfo;

                if (!p.CanRead || !p.CanWrite)
                    return false;

                if (p.GetGetMethod() == null || p.GetSetMethod() == null)
                    return false;
            }
            else
            {
                return false;
            }

            var c = m.GetCustomAttributes();

            if (c.Any(a => a.GetType() == typeof(TweachAttribute)))
                return true;

            if (mapOnlyMembersMarkedWithTweach)
                return false;

            if (c.Any(a => a.GetType() == typeof(SerializeField)))
                return true;

            if (respectHideInInspector && c.Any(a => a.GetType() == typeof(HideInInspector)))
                return false;

            return true;
        }
    }
}