using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class AssetHandler
{
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        MapTerrain  obj = EditorUtility.InstanceIDToObject(instanceID) as MapTerrain;
        if (obj != null)
        {
            TerrainWindow.Open(obj);
            return true;
        }
        return false;
    }
}

[CustomEditor(typeof(MapTerrain))]
public class TerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate Map"))
        {
            TerrainWindow.Open((MapTerrain)target);
        }
    }
}
