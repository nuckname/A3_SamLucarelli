using System;
using UnityEditor;
using UnityEngine;

public class TerrainWindow : ExtendedWindow
{
    public static void Open(MapTerrain mapTerrain)
    {
        TerrainWindow window = GetWindow<TerrainWindow>("Game Data Editor");
        window.serializedObject = new SerializedObject(mapTerrain);
    }

    private void OnGUI()
    {
        currnetProperty = serializedObject.FindProperty("m_CurrentTerrain");
        DrawProperties(currnetProperty, true);
    }
}

