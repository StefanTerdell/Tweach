using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tweach
{
    public class SpecialComponentVariable
    {
        public string name;
        public string displayName;
    }
    public class SpecialComponent
    {
        public string displayName;
        public SpecialComponentVariable[] properties;
        public SpecialComponentVariable[] fields;
    }

    public static class SpecialComponents
    {
        public static Dictionary<Type, SpecialComponent> specialComponents = new Dictionary<Type, SpecialComponent>() {
            {
                typeof(CharacterController), new SpecialComponent() {

                }
            }
        };

        public static void LoadSpecialComponents()
        {
            // specialComponents = new SpecialComponent[] {
            //     new SpecialComponent() {
            //         name = "myName",
            //         displayName = "myDisplayName",
            //         properties = new SpecialComponentVariable[] {
            //             new SpecialComponentVariable() {
            //                 name = "propName",
            //                 displayName = "propDisplayName"
            //             }
            //         },
            //         fields = new SpecialComponentVariable[] {
            //             new SpecialComponentVariable() {
            //                 name = "fieldName",
            //                 displayName = "fieldDisplayName"
            //             }
            //         }
            //     }
            // };

            // var sp = new SpecialComponent()
            // {
            //     name = "saldjkf",
            //     displayName = "aöslksld"
            // };

            Debug.Log(JsonUtility.ToJson(specialComponents, true));

            //  TextAsset json = (TextAsset) AssetDatabase.LoadAssetAtPath(Tweach.baseTweachAssetPath+"/Json/specialComponents.json", typeof(TextAsset));
            //  Debug.Log(json.ToString());
            //  specialComponents = JsonUtility.FromJson<Dictionary<string, SpecialComponent>>(json.ToString());

            //  foreach (var item in specialComponents)
            //  {
            //     Debug.Log(item.Key + ": " + item.Value.displayName);

            //     foreach (var prop in item.Value.properties)
            //     {
            //         Debug.Log("   " + prop.Key + ": " + prop.Value);    
            //     }

            //     foreach (var field in item.Value.fields)
            //     {
            //         Debug.Log("   " + field.Key + ": " + field.Value);    
            //     }
            //  }
        }
    }
}