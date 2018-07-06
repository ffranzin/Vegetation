using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MoistureDistribuition))]
public class MoistureDistribuitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        //if (EditorGUI.EndChangeCheck())
        //{
        //    Debug.Log("CHANGED");
        //}

        MoistureDistribuition myScript = (MoistureDistribuition)target;
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

        if (GUILayout.Button("Calculate Moisture"))
        {
            myScript.CalculateMoisture();
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