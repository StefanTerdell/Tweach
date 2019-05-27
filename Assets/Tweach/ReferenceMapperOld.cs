using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tweach
{
    public class ReferenceMapperOld {
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