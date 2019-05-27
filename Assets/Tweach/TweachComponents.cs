using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TweachComponents : MonoBehaviour
{
    public static TweachComponents instance;
    private void Awake()
    {
        baseTweachAssetPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this));
        baseTweachAssetPath = baseTweachAssetPath.Substring(0, baseTweachAssetPath.LastIndexOf('/'));

        instance = this;
    }

    public static string baseTweachAssetPath;

    public Transform _hierarchyContentTransform;
    public static Transform hierarchyContentTransform => instance._hierarchyContentTransform;
    public Transform _componentsAndFieldsContentTransform;
    public static Transform componentsAndFieldsContentTransform => instance._componentsAndFieldsContentTransform;
    public Button _backButton;
    public static Button backButton => instance._backButton;
    public Button _exitButton;
    public static Button exitButton => instance._exitButton;
    public InputField _searchField;
    public static InputField searchField => instance._searchField;
}
