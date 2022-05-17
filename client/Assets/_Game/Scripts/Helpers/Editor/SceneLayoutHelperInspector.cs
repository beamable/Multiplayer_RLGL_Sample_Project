//This next script shows how to call upon variables from the "MyGameObject" Script (the first script) to make custom fields in the Inspector for these variables.

using UnityEngine;
using UnityEditor;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(SceneLayoutHelper))]
[CanEditMultipleObjects]
public class EditorGUILayoutPropertyField : Editor
{
    SerializedProperty m_sourceToTargets;
    SerializedProperty m_VectorProp;
    SerializedProperty m_GameObjectProp;
    SceneLayoutHelper script;
    private bool folded = true;
    bool toggle;
    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        m_sourceToTargets = serializedObject.FindProperty("sourceToTargets");
        m_VectorProp = serializedObject.FindProperty("m_MyVector");
        m_GameObjectProp = serializedObject.FindProperty("m_MyGameObject");
        script = (SceneLayoutHelper)target;
    }

    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Source To Target"))
        {
            script.ReplaceSourceWithTarget();
        }
        if (GUILayout.Button("Target To Source"))
        {
            script.ReplaceTargetWithSource();
        }
        
        
        DrawPropertyArray(m_sourceToTargets, ref folded);

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPropertyArray(SerializedProperty property, ref bool fold)
    {
        fold = EditorGUILayout.Foldout(fold, new GUIContent(
            property.displayName,
            ""), true);

        if (!fold) return;
        var arraySizeProp = property.FindPropertyRelative("Array.size");
        EditorGUILayout.PropertyField(arraySizeProp);

        EditorGUI.indentLevel++;

        for (var i = 0; i < arraySizeProp.intValue; i++)
        {
            EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
        }

        EditorGUI.indentLevel--;
    }

}