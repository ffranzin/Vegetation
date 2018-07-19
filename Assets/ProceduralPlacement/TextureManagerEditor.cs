using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TextureManager))]
public class TextureManagerEditor : Editor
{
    private string sufix = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextureManager tm = (TextureManager)target;

        EditorGUILayout.Space();

        sufix = EditorGUILayout.TextField("File Sufix: ", sufix);

        if (GUILayout.Button("Save All Textures"))
        {
            if (Application.isPlaying)
                tm.SaveAllTextures(sufix);
            else
                Debug.LogError("Splats can only be saved in Play Mode");
        }
    }
}