using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

//This is a combindation of my work. ChatGpts and the video as a base foundation. 
// The video: https://www.youtube.com/watch?v=c_3DXBrH-Is
//Chat history: https://chatgpt.com/share/69094c3a-20c4-8000-affa-70f173ec01d4


//The Editor Window
public class TerrainWindow : EditorWindow
{
    private MapTerrain targetAsset;

    // UI state
    private string _selectedGroup;
    private Vector2 _leftScroll;
    private Vector2 _rightScroll;
    
    //serialsed objects
    protected SerializedObject serializedObject;
    private string selectedPropertyPath;

    /// <summary>
    /// Opens the window and loads the selected MapTerrain object
    /// into a SerializedObject for editing.
    /// </summary>
    public static void Open(MapTerrain mapTerrain)
    {
        var window = GetWindow<TerrainWindow>("Terrain Editor");
        window.targetAsset = mapTerrain;
        window.serializedObject = new SerializedObject(mapTerrain);
        window._selectedGroup = null;
        window.Repaint();
    }

    /// <summary>
    /// Ensures the SerializedObject is recreated when Unity reloads scripts
    /// or the window becomes active again.
    /// </summary>
    private void OnEnable()
    {
        if (targetAsset != null && (serializedObject == null || serializedObject.targetObject == null))
            serializedObject = new SerializedObject(targetAsset);
    }

    /// <summary>
    /// Main GUI (Graphical User Interface) function. Draws sidebar sections, property panels,
    /// live updates the MapTerrain if enabled, and includes a bottom-right "Generate Map" button.
    /// </summary>
    private void OnGUI()
    {
        if (serializedObject == null || serializedObject.targetObject == null)
        {
            EditorGUILayout.HelpBox("No MapTerrain selected. Open this window from a MapTerrain asset.", MessageType.Info);
            return;
        }

        serializedObject.Update();

        var topLevel = GetTopLevelProps(serializedObject);
        var groups = BuildGroupsByHeader(targetAsset, topLevel);

        // Default selection = first group
        if (string.IsNullOrEmpty(_selectedGroup) || !groups.ContainsKey(_selectedGroup))
        {
            string firstGroupName = groups.Keys.FirstOrDefault();

            if (!string.IsNullOrEmpty(firstGroupName))
            {
                _selectedGroup = firstGroupName;
            }
            else
            {
                _selectedGroup = "Draw Mode";
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            //Left side bar with header names
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(50)))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Sections", EditorStyles.boldLabel);
                EditorGUILayout.Space(3);

                _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
                foreach (var kvp in groups)
                {
                    bool isSelected = kvp.Key == _selectedGroup;

                    var buttonStyle = new GUIStyle(EditorStyles.miniButtonLeft)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal,
                        fixedHeight = 26
                    };

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(kvp.Key, buttonStyle))
                            _selectedGroup = kvp.Key;
                    }
                }
                EditorGUILayout.EndScrollView();

                GUILayout.FlexibleSpace();

    
            }

            //The right sidebar properities. 
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField(_selectedGroup, EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                bool changedValue = false;
                
                _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);

                if (groups.TryGetValue(_selectedGroup, out var propsInGroup))
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        foreach (var p in propsInGroup)
                            EditorGUILayout.PropertyField(p, true);
                    }
                    
                    changedValue = true;
                }
                else
                {
                    EditorGUILayout.HelpBox("No properties in this group.", MessageType.Info);
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                if (changedValue && targetAsset.autoUpdate)
                {
                    serializedObject.ApplyModifiedProperties();
                    GenerateFromWindow();

                    Debug.Log("Called");

                    //Dont display generate button.
                    return;
                }
                
                //Also put the generate button at the bottom of the right pane
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Generate Map", GUILayout.Width(140), GUILayout.Height(28)))
                        GenerateFromWindow();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Retrieves the "root-level" properties of the asset
    /// (excluding m_Script) while maintaining their drawing order.
    /// </summary>
    private static List<SerializedProperty> GetTopLevelProps(SerializedObject so)
    {
        var list = new List<SerializedProperty>();
        var it = so.GetIterator();
        var enterChildren = true;
        while (it.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (it.depth != 0) continue;
            if (it.name == "m_Script") continue;
            //Reference from the iterator using its path
            list.Add(so.FindProperty(it.propertyPath)); 
        }
        return list;
    }

    /// <summary>
    /// Groups Serialized Properties according to their Header attributes,
    /// allowing the window to create collapsible “sections” for the UI.
    /// </summary>
    private static SortedDictionary<string, List<SerializedProperty>> BuildGroupsByHeader(ScriptableObject target, List<SerializedProperty> props)
    {
        var result = new SortedDictionary<string, List<SerializedProperty>>();
        var type = target.GetType();
        const BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        string lastGroup = "Draw Mode";

        foreach (var p in props)
        {
            var field = type.GetField(p.name, bf);
            string group = lastGroup;

            if (field != null)
            {
                var header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null && !string.IsNullOrEmpty(header.header))
                    group = header.header;
            }

            lastGroup = group;

            if (!result.TryGetValue(group, out var list))
            {
                list = new List<SerializedProperty>();
                result[group] = list;
            }
            list.Add(p);
        }

        //Preserve the correct order
        foreach (var key in result.Keys.ToList())
            result[key] = result[key].OrderBy(p => FieldOrder(type, p.name)).ToList();

        return result;
    }

    /// <summary>
    /// Determines the display ordering of fields by scanning their declaration order.
    /// Ensures the editor window draws them top-to-bottom as in the script.
    /// </summary>
    private static int FieldOrder(Type t, string fieldName)
    {
        const BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var fields = new List<FieldInfo>();
        for (var type = t; type != null && type != typeof(UnityEngine.Object); type = type.BaseType)
            fields.AddRange(type.GetFields(bf));
        for (int i = 0; i < fields.Count; i++)
            if (fields[i].Name == fieldName) return i;
        return int.MaxValue;
    }

    private void GenerateFromWindow()
    {
        serializedObject.ApplyModifiedProperties();

        MapGenerator mapGen = FindObjectOfType<MapGenerator>();

        if (mapGen == null)
        {
            Debug.LogWarning("cant find object");
            return;
        }

        mapGen.GenerateMap();

        Debug.Log("Map generated successfully from TerrainWindow.");
    }

}
