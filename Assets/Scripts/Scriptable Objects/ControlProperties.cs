using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Tetris/ControlProperties")]
public class ControlProperties : ScriptableObject
{
    public float DAS;

    public float ARR;

    public float SDF;

    public static ControlProperties Instance;

    void OnEnable()
    {
        Instance = this;
    }


}
