
using UnityEngine;


public enum ecotype {A, B, C, NONE};


public class EnvironmentConditions
{
    [Range(0, 1)] public float globalHeight;
    [Range(0, 1)] public float localHeight;
    [Range(0, 1)] public float temperature;
    [Range(0, 1)] public float moisture;
    [Range(0, 1)] public float inclination;
    [Range(0, 1)] public float slope;

    public ecotype ecotype;
}

