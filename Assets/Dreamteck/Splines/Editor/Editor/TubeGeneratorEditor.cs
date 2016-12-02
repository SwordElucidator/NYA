#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(TubeGenerator))]
    public class TubeGeneratorEditor : MeshGenEditor
    {
        
        public override void BaseGUI()
        {
            TubeGenerator tubeGenerator = (TubeGenerator)target;
            base.BaseGUI();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            tubeGenerator.sides = EditorGUILayout.IntField("Sides", tubeGenerator.sides);
            tubeGenerator.cap = EditorGUILayout.Toggle("Cap", tubeGenerator.cap);
            if (tubeGenerator.sides < 3) tubeGenerator.sides = 3;
            tubeGenerator.integrity = EditorGUILayout.Slider("Integrity", tubeGenerator.integrity, 0f, 360f);

            UVControls(tubeGenerator);
            tubeGenerator.capUVScale = EditorGUILayout.FloatField("Cap UV Scale", tubeGenerator.capUVScale);
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(tubeGenerator);
        }

    }
}
#endif