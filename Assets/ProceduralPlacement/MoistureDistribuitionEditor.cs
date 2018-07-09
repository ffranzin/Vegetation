using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MoistureDistribuition))]
public class MoistureDistribuitionEditor : Editor
{
    private Vector2 testOrigin;

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        testOrigin = EditorGUILayout.Vector2Field(new GUIContent("Test Origin"), testOrigin);


        if (EditorGUI.EndChangeCheck())
        {
            MoistureDistribuition md = (MoistureDistribuition)target;

            testOrigin.x = Mathf.Max(0f, testOrigin.x);
            testOrigin.y = Mathf.Max(0f, testOrigin.y);

            if (Application.isPlaying)
            {
                //Debug.Log("Updating...");

                md.UpdateParameters();
                md.UpdateCurves();
                md.CalculateAll(testOrigin);
            }
        }
    }
}