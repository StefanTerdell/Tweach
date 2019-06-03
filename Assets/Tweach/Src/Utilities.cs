using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Tweach
{
    public static class Utilities
    {
        public static string MoveUpInFileSystemPath(this string path, int times = 1)
        {
            path = path.Substring(0, path.LastIndexOf('/'));
            times--;
            return times > 0 ? path.MoveUpInFileSystemPath(times) : path;
        }

        public static BindingFlags GetBindingFlags(bool instanceFlag, bool staticFlag, bool publicFlag, bool nonPublicFlag)
        {
            var flags = BindingFlags.Default;

            if (instanceFlag)
                flags |= BindingFlags.Instance;

            if (staticFlag)
                flags |= BindingFlags.Static;

            if (publicFlag)
                flags |= BindingFlags.Public;

            if (nonPublicFlag)
                flags |= BindingFlags.NonPublic;

            return flags;
        }

        public static bool HasMembersRecursive(GameObjectReference gameObjectReference, bool removeComponentsWithoutFields)
        {
            var count = gameObjectReference.childComponentReferences.Count;

            for (int i = 0; i < count; i++)
            {
                if (gameObjectReference.childComponentReferences[i].GetMembers() != null && gameObjectReference.childComponentReferences[i].GetMembers().Count > 0)
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

        public static string UpwardsDebugTraceRecursive(IReference reference, int depth, string value = "")
        {
            value += depth + ": " + reference.GetTypeName() + ": " + reference.GetName() + "\n";
            if (reference.GetParentReference() != null)
                value = UpwardsDebugTraceRecursive(reference.GetParentReference(), --depth, value);

            return value;
        }

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
            var debugString = $"{Indent(i)}{gameObjectReference.GetName()}:\n";

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
            var debugString = $"{Indent(i)}{componentReference.GetName()}\n";

            if (componentReference.GetMembers() != null)
            foreach (var item in componentReference.GetMembers())
            {
                debugString += GetFieldDebugString(item, i + 1);
            }

            return debugString;
        }

        static string GetFieldDebugString(MemberReference memberReference, int i)
        {
            i++;
            var debugString = $"{Indent(i)}{memberReference.GetTypeName()} {memberReference.GetName()}:";

            if (memberReference.GetMembers().Any())
            {
                debugString += "\n";
                foreach (var item in memberReference.GetMembers())
                {
                    debugString += GetFieldDebugString(item, i + 1);
                }
            }
            else
            {
                if (memberReference.GetValue() != null)
                    debugString += $" {memberReference.GetValue().ToString()}\n";
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
            return new string(' ', indentation * spacing);
        }
    }
}
