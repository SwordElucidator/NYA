#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineRenderer), true)]
    public class SplineRendererEditor : MeshGenEditor
    {
        public override void BaseGUI()
        {
            base.BaseGUI();
            EditorGUI.BeginChangeCheck();
            SplineRenderer user = (SplineRenderer)target;


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
            user.slices = EditorGUILayout.IntField("Slices", user.slices);
            if (user.slices < 1) user.slices = 1;
            UVControls(user);

            if (EditorGUI.EndChangeCheck())  EditorUtility.SetDirty(user);
            
        }

        public override void OnInspectorGUI()
        {
            showDoubleSided = false;
            showFlipFaces = false;
            showRotation = false;
            showNormalMethod = false;
            showOptimize = false;
            base.OnInspectorGUI();
        }

    }
}
#endif
