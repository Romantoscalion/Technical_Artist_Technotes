// Copyright Elliot Bentine, 2018-
#if (UNITY_EDITOR)

using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools
{
    [CustomEditor(typeof(SteppedAnimation))]
    public class SteppedAnimationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SteppedAnimation p = (SteppedAnimation)target;

            EditorGUILayout.LabelField("ProPixelizer | Stepped Animation Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(SHORT_HELP, MessageType.Info);
            EditorGUILayout.LabelField("");

            serializedObject.Update();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SourceClips"));

            EditorGUILayout.LabelField("Keyframes", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("KeyframeMode"));
            switch (p.KeyframeMode)
            {
                case SteppedAnimation.StepMode.FixedRate:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("KeyframeRate"));
                    break;
                case SteppedAnimation.StepMode.FixedTimeDelay:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FixedTimeDelay"));
                    break;
                case SteppedAnimation.StepMode.Manual:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ManualKeyframeTimes"));
                    break;
            }

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Output", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Output clips will be generated in the same folder as this asset, and given the same name as the source clip with the \"_stepped\" suffix.\nOutput clips will also be given the \"Stepped\" asset label.", MessageType.Info);
            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Generate"))
            {
                p.Generate();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public const string SHORT_HELP = "This asset can be used to create stepped versions of source animation clips. Stepped animations can be used to produce a convincing 'flipbook' effect.";
    }
}

#endif