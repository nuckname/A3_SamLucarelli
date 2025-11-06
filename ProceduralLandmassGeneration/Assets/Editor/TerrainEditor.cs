using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

//Based on https://www.youtube.com/watch?v=c_3DXBrH-Is
public class AssetHandler
{
    /// <summary>
    /// Called by Unity whenever an asset is opened.
    /// If the asset is a MapTerrain, opens the TerrainWindow and prevents the default behaviour.
    /// </summary>
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

/// <summary>
/// Adds a simple custom Inspector for MapTerrain that shows a "Generate Map" button.
/// This button opens the TerrainWindow for editing.
/// </summary>
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
