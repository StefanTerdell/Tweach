﻿using UnityEngine;

[System.Serializable]
public class SamplASubClass
{
    public int A = 4;

    public string Info = "I am in a subclass";
}

public class SampleA : MonoBehaviour
{
    public string StringProperty { get; set; }
    public float floatA = 10, floatB = 20;
    public Vector2 hedfkljsldkfj;
    public Vector3 lajsdoiad = new Vector3();
    public bool IamBool;
    public bool SoAmI = true;

    public Color color = new Color();

    [Tweach] public string stringA = "I am a string";

    public SamplASubClass samplASubClass;

    private float privateFloatA = 25;

    public SampleA other;

    public GameObject otherGo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
