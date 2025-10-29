using UnityEngine;
using UnityEditor; 


public class GameDataCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate Map"))
        {
            GameDataObjectEditorWindow.Open();
        }
    }
}
