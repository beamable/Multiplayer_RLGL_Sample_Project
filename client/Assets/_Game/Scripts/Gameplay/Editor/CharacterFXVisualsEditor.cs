#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace BeamableExample.RedlightGreenLight
{

    [CustomEditor(typeof(CharacterFXVisuals))]
    public class CharacterFXVisualsEditor : Editor
    {
        private string[] effectTypes;
        private int selectedEffectType = -1;
        private List<CharacterEffect> effects = new List<CharacterEffect>();
        private void GetEffectTypes()
        {
            if (effectTypes == null)
            {
                effectTypes = System.Enum.GetNames(typeof(CharacterEffectType));
            }
        }

        public CharacterEffectType GetEffectType(string typeName)
        {
            return (CharacterEffectType)Enum.Parse(typeof(CharacterEffectType), typeName);
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // var t = target as CharacterFXVisuals;

            var effects = serializedObject.FindProperty("effects");

            GetEffectTypes();

            GUILayout.BeginHorizontal();
            selectedEffectType = EditorGUILayout.Popup(selectedEffectType, effectTypes);
            GUI.enabled = selectedEffectType != -1;
            if (GUILayout.Button("Add"))
            {
                CharacterEffectType type = GetEffectType(effectTypes[selectedEffectType]);

                effects.InsertArrayElementAtIndex(effects.arraySize);
                serializedObject.ApplyModifiedProperties();

                var newEffect = effects.GetArrayElementAtIndex(effects.arraySize - 1);
                newEffect.FindPropertyRelative("type").intValue = (int)type;
                newEffect.FindPropertyRelative("foldout").boolValue = true;

                selectedEffectType = -1;

            }
            GUI.enabled = true;
            if (GUILayout.Button("Clear"))
            {
                effects.ClearArray();
            }
            GUILayout.EndHorizontal();

            // t.newEffects = effects;
            UpdateList(effects);


            serializedObject.ApplyModifiedProperties();


        }
        async void UpdateList(SerializedProperty effects)
        {
            if (effects == null) return;
            for (int i = 0; i < effects.arraySize; i++)
            {
                var effect = effects.GetArrayElementAtIndex(i);

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle(EditorStyles.foldout);
                style.fixedWidth = 5;
                effect.FindPropertyRelative("foldout").boolValue = EditorGUILayout.Foldout(effect.FindPropertyRelative("foldout").boolValue, "", style);
                EditorGUILayout.LabelField(((CharacterEffectType)effect.FindPropertyRelative("type").intValue).ToString(), EditorStyles.boldLabel);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    effects.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    if (i == effects.arraySize) return;
                    return;
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                if (effect.FindPropertyRelative("foldout").boolValue)
                {
                    DrawEffectGUI(effect);
                }

                GUILayout.EndVertical();
            }
        }


        private void DrawEffectGUI(SerializedProperty effect)
        {
            CharacterEffectType type = (CharacterEffectType)effect.FindPropertyRelative("type").intValue;
            switch (type)
            {
                case CharacterEffectType.TrailEffect:

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Effect ID");
                    effect.FindPropertyRelative("id").stringValue = EditorGUILayout.TextField(effect.FindPropertyRelative("id").stringValue);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Target Object");
                    effect.FindPropertyRelative("targetObject").objectReferenceValue = EditorGUILayout.ObjectField(effect.FindPropertyRelative("targetObject").objectReferenceValue, typeof(GameObject), true) as GameObject;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    effect.FindPropertyRelative("targetOffset").vector3Value = EditorGUILayout.Vector3Field("Target Offset", effect.FindPropertyRelative("targetOffset").vector3Value);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Trail Renderer");
                    effect.FindPropertyRelative("trailRenderer").objectReferenceValue = EditorGUILayout.ObjectField(effect.FindPropertyRelative("trailRenderer").objectReferenceValue, typeof(TrailRenderer), true) as TrailRenderer;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    SerializedProperty colorGradient = effect.FindPropertyRelative("colorGradient");
                    try
                    {
                        EditorGUILayout.PropertyField(colorGradient, false, null);
                    }
                    catch (Exception e)
                    {
                        if (ExitGUIUtility.ShouldRethrowException(e))
                        {
                            throw;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    SerializedProperty trailWidth = effect.FindPropertyRelative("trailWidth");
                    try
                    {
                        EditorGUILayout.PropertyField(trailWidth, false, null);
                    }
                    catch (Exception e)
                    {
                        if (ExitGUIUtility.ShouldRethrowException(e))
                        {
                            throw;
                        }
                    }
                    GUILayout.EndHorizontal();

                    break;
                case CharacterEffectType.HitEffect:

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Effect ID");
                    effect.FindPropertyRelative("id").stringValue = EditorGUILayout.TextField(effect.FindPropertyRelative("id").stringValue);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Particle System");
                    effect.FindPropertyRelative("particleSystem").objectReferenceValue = EditorGUILayout.ObjectField(effect.FindPropertyRelative("particleSystem").objectReferenceValue, typeof(ParticleSystem), true) as ParticleSystem;
                    GUILayout.EndHorizontal();

                    break;
            }
        }
    }

#endif



    public static class ExitGUIUtility
    {
        public static bool ShouldRethrowException(Exception exception)
        {
            return IsExitGUIException(exception);
        }

        public static bool IsExitGUIException(Exception exception)
        {
            while (exception is TargetInvocationException && exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            return exception is ExitGUIException;
        }
    }
}