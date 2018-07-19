using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MoistureDistribuition))]
public class MoistureDistribuitionEditor : Editor
{
    private Vector2Int testOrigin;

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        testOrigin = EditorGUILayout.Vector2IntField(new GUIContent("Test Origin"), testOrigin);

        if (EditorGUI.EndChangeCheck())
        {
            MoistureDistribuition md = (MoistureDistribuition)target;

            if (Application.isPlaying)
            {
                testOrigin.Clamp(
                    Vector2Int.zero,
                    md.TexManager.AtlasDimensions - md.TexManager.SplatDimensions);

                md.UpdateParameters();
                md.UpdateCurves();
                md.CalculateAll(testOrigin);
            }
        }
    }
}