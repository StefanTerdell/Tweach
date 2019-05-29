﻿using System.Collections.Generic;
using System.Text;

namespace Tweach
{
    public static class DebugHelper
    {
        public static string GetDebugString(List<GameObjectReference> rootList)
        {
            var debugString = "<b><i>Tweach!</i></b> \n";

            foreach (var item in rootList)
            {
                debugString += GetGameObjectDebugString(item);
            }

            return debugString;
        }

        static string GetGameObjectDebugString(GameObjectReference gameObjectReference, int i = 0)
        {
            var debugString = $"{Indent(i)}{gameObjectReference.value.name}:\n";

            if (gameObjectReference.childGameObjectReferences.Any())
            {
                debugString += $"{Indent(i + 1)}Child Objects:\n";
                foreach (var item in gameObjectReference.childGameObjectReferences)
                {
                    debugString += GetGameObjectDebugString(item, i + 2);
                }
            }

            if (gameObjectReference.childComponentReferences.Any())
            {
                debugString += $"{Indent(i + 1)}Components:\n";
                foreach (var item in gameObjectReference.childComponentReferences)
                {
                    debugString += GetComponentDebugString(item, i + 2);
                }
            }

            return debugString;
        }

        static string GetComponentDebugString(ComponentReference componentReference, int i)
        {
            var debugString = $"{Indent(i)}{componentReference.value.GetType().Name}\n";

            foreach (var item in componentReference.childFieldReferences)
            {
                debugString += GetFieldDebugString(item, i + 1);
            }

            return debugString;
        }

        static string GetFieldDebugString(FieldReference fieldReference, int i)
        {
            i++;
            var debugString = $"{Indent(i)}{fieldReference.fieldInfo.FieldType.Name} {fieldReference.fieldInfo.Name}:";

            if (fieldReference.childFieldReferences.Any())
            {
                debugString += "\n";
                foreach (var item in fieldReference.childFieldReferences)
                {
                    debugString += GetFieldDebugString(item, i + 1);
                }
            }
            else
            {
                if (fieldReference.value != null)
                    debugString += $" {fieldReference.value.ToString()}\n";
                else
                    debugString += $" null\n";
            }

            return debugString;
        }

        static bool Any<t>(this List<t> item)
        {
            return item != null && item.Count > 0;
        }

        public static string Indent(int indentation, int spacing = 4)
        {
            if (indentation > 100)
                throw new System.Exception("Recursion > 100, throwing for protection");

            return new string(' ', indentation * spacing);
        }
    }
}