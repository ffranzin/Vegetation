using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(HumidityDistribuition))]
public class HumidityDistribuitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HumidityDistribuition myScript = (HumidityDistribuition)target;
        if (GUILayout.Button("Print values"))
        {
            myScript.PrintHeightMapValues();
        }

        if (GUILayout.Button("Clean All Data"))
        {
            myScript.CleanData();
        }

        if (GUILayout.Button("Load Data"))
        {
            myScript.LoadDataFromFiles();
        }

        if (GUILayout.Button("Calculate Mean Height"))
        {
            myScript.CalculateMeanHeight();
        }

        if (GUILayout.Button("Calculate Relative Height"))
        {
            myScript.CalculateRelativeHeight();
        }

        if (GUILayout.Button("Calculate Slope"))
        {
            myScript.CalculateSlope();
        }

        if (GUILayout.Button("Calculate Water Spread"))
        {
            myScript.CalculateWaterSpread();
        }

        if (GUILayout.Button("Calculate Humidity"))
        {
            myScript.CalculateHumidity();
        }

        if (GUILayout.Button("Reload Textures"))
        {
            myScript.UpdateTextures();
        }

        if (GUILayout.Button("Save Textures"))
        {
            myScript.SaveTextures();
        }
    }
}