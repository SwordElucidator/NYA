#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplinePositioner), true)]
    public class SplinePositionerEditor : SplineUserEditor
    {
        public override void BaseGUI()
        {
            base.BaseGUI();
            SplinePositioner positioner = (SplinePositioner)target;
            positioner.mode = (SplinePositioner.Mode)EditorGUILayout.EnumPopup("Mode", positioner.mode);
            if(positioner.mode == SplinePositioner.Mode.Distance) positioner.position = EditorGUILayout.FloatField("Distance", (float)positioner.position);
            else positioner.position = EditorGUILayout.Slider("Percent", (float)positioner.position, 0f, 1f);
            positioner.applyTransform = (Transform)EditorGUILayout.ObjectField("Apply transform", positioner.applyTransform, typeof(Transform), true);
            positioner.applyPosition = EditorGUILayout.Toggle("Apply position", positioner.applyPosition);
            if (positioner.applyPosition) positioner.offset = EditorGUILayout.Vector2Field("Offset", positioner.offset);
            positioner.applyRotation = EditorGUILayout.Toggle("Apply rotation", positioner.applyRotation);
            if (positioner.applyRotation) positioner.rotationOffset = EditorGUILayout.Vector3Field("Offset rotation", positioner.rotationOffset);
            positioner.applyScale = EditorGUILayout.Toggle("Apply scale", positioner.applyScale);
            if (positioner.applyScale) positioner.baseScale = EditorGUILayout.Vector3Field("Base scale", positioner.baseScale);
        }

        public override void OnInspectorGUI()
        {
            BaseGUI();
        }
    }
}
#endif
