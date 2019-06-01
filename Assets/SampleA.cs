using System;
using UnityEngine;

[System.Serializable]
public class SamplASubClass
{
    [Tweach]public int A = 4;

    public string Info = "I am in a subclass";
}

public enum IamEnum
{
    one = 3,
    two = 6,
    three = 100,
    cunt = 23
}

[Flags]
public enum FlagEnum
{
    zerost = 0,
    first = 1,
    second = 2,
    third = 4,
    fourth = 8
}

public class SampleA : MonoBehaviour
{
    public LayerMask layerMask;
    public FlagEnum flagEnum = FlagEnum.first | FlagEnum.fourth;
    public IamEnum enuuuuM;
    public CameraScriptSettings cameraScriptSettings;
    public string StringProperty { get; set; }
    public bool BoolProperty { get; set; }
    public float floatA = 10, floatB = 20;
    public Vector2 hedfkljsldkfj;
    public Vector3 lajsdoiad = new Vector3();
    public bool IamBool;
    static public bool SoAmI = true;

    public Color color = new Color();

    [Tweach] public string stringA = "I am a string";

    [Tweach] public SamplASubClass samplASubClass;

    private float privateFloatA = 25;

    public SampleA other;

    public GameObject otherGo;
}
