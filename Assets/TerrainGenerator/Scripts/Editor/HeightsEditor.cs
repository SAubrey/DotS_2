using UnityEngine;
using System;
using UnityEditor;
using Assets.Scripts.MapGenerator.Generators;
using System.Collections.Generic;

[CustomEditor(typeof(HeightsGenerator))]
class HeightsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var gen = (HeightsGenerator)target;

        if (DrawDefaultInspector())
        {
            if (gen.AutoUpdate)
            {
                gen.Generate();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            gen.Generate();
        }

        if (GUILayout.Button("Clear"))
        {
            gen.Clear();
        }
    }
}
