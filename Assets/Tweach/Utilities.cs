using System.Collections.Generic;
using System.Text;

namespace Tweach
{
    public static class Utilities
    {
        public static string DebugString(List<GameObjectReference> rootList)
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

            if (gameObjectReference.childReferences.Any())
            {
                debugString += $"{Indent(i + 1)}Child Objects:\n";
                foreach (var item in gameObjectReference.childReferences)
                {
                    debugString += GetGameObjectDebugString(item, i + 2);
                }
            }

            if (gameObjectReference.componentReferences.Any())
            {
                debugString += $"{Indent(i + 1)}Components:\n";
                foreach (var item in gameObjectReference.componentReferences)
                {
                    debugString += GetComponentDebugString(item, i + 2);
                }
            }

            return debugString;
        }

        static string GetComponentDebugString(ComponentReference componentReference, int i)
        {
            var debugString = $"{Indent(i)}{componentReference.value.GetType().Name}\n";

            foreach (var item in componentReference.fieldReferences)
            {
                debugString += GetFieldDebugString(item, i + 1);
            }

            return debugString;
        }

        static string GetFieldDebugString(FieldReference fieldReference, int i)
        {
            i++;
            var debugString = $"{Indent(i)}{fieldReference.fieldInfo.FieldType.Name} {fieldReference.fieldInfo.Name}:";

            if (fieldReference.children.Any())
            {
                debugString += "\n";
                foreach (var item in fieldReference.children)
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

        static string Indent(int indentation)
        {
            if (indentation > 10)
                throw new System.Exception("Recursion > 10, throwing for protection");

            return new string(' ', indentation * 4);
        }
    }
}
