#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(ObjectBender), true)]
    public class ObjectBenderEditor : SplineUserEditor
    {
        private bool showProperties = false;
        private int selectedProperty = -1;
        private Transform drawTransform = null;

        protected override void Awake() 
        {
            ObjectBender bender = (ObjectBender)target;
            if(!Application.isPlaying) bender.UpdateReferences();
            base.Awake();
        }

        public override void BaseGUI()
        {
            EditorGUILayout.LabelField("ObjectBender is in BETA. Contact support@dreamteck-hq.com for support");
            ObjectBender bender = (ObjectBender)target;
            showAveraging = false;
            base.BaseGUI();

            if (bender.bendProperties.Length - 1 != bender.transform.childCount && !Application.isPlaying) bender.UpdateReferences();
            bender.axis = (ObjectBender.Axis)EditorGUILayout.EnumPopup("Axis", bender.axis);
            bender.autoNormals = EditorGUILayout.Toggle("Auto Normals", bender.autoNormals);
           if (bender.autoNormals) bender.upVector = EditorGUILayout.Vector3Field("Up Vector3", bender.upVector);
            showProperties = EditorGUILayout.Foldout(showProperties, "Object Properties");
            if (showProperties)
            {
                BendPropertiesGUI(bender.bendProperties);
                if (selectedProperty >= 0)
                {
                    if (Event.current.type == EventType.keyDown)
                    {
                        if (Event.current.keyCode == KeyCode.DownArrow) selectedProperty++;
                        if (Event.current.keyCode == KeyCode.UpArrow) selectedProperty--;
                        if (selectedProperty < 0) selectedProperty = 0;
                        if (selectedProperty >= bender.bendProperties.Length) selectedProperty = bender.bendProperties.Length - 1;
                        drawTransform = bender.bendProperties[selectedProperty].transform.transform;
                        Repaint();
                        SceneView.RepaintAll();
                        Event.current.Use();
                    }
                }
            }
            else drawTransform = null;
            string editModeText = "Enter Edit Mode";
            if (!bender.bend) editModeText = "Bend";
            if (GUILayout.Button(editModeText))
            {
                if (bender.bend) bender.bend = false;
                else bender.bend = true;
            }
        }

        void BendPropertiesGUI(ObjectBender.BendProperty[] properties)
        {
            for(int i = 0; i < properties.Length; i++)
            {
                if(selectedProperty == i)
                {
                    GUI.color = Color.white;
                    GUILayout.Box("", GUILayout.Height(85), GUILayout.Width(Screen.width - 20));
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.height = 18;
                    GUI.BeginGroup(lastRect);
                    GUI.Label(new Rect(5f, 1f, Screen.width * 0.7f, 22), properties[i].transform.transform.name);
                    properties[i].enabled = EditorGUI.Toggle(new Rect(Screen.width - 40f - 22f, 1f, 22, 22), properties[i].enabled);
                    GUI.EndGroup();
                    GUI.BeginGroup(GUILayoutUtility.GetLastRect());
                    if (!properties[i].enabled)
                    {
                        EditorGUI.LabelField(new Rect(220, 40, 200, 30), "BENDING DISABLED");
                    }
                    else
                    {
                        properties[i].applyRotation = EditorGUI.Toggle(new Rect(5, 22, Screen.width / 2f - 25, 20), "Apply rotation", properties[i].applyRotation);
                        properties[i].applyScale = EditorGUI.Toggle(new Rect(5, 40, Screen.width / 2f - 25, 20), "Apply scale", properties[i].applyScale);
                        if (properties[i].splineComputer != null) properties[i].bendSpline = EditorGUI.Toggle(new Rect(5, 58, Screen.width / 2f - 25, 20), "Bend Spline", properties[i].bendSpline);
                        if (properties[i].filter != null)
                        {
                            properties[i].bendMesh = EditorGUI.Toggle(new Rect(Screen.width / 2f - 20, 22, Screen.width / 2f - 25, 20), "Bend Mesh", properties[i].bendMesh);
                            if (properties[i].bendMesh)
                            {
                                if (properties[i].collider != null)
                                {
                                    properties[i].bendCollider = EditorGUI.Toggle(new Rect(Screen.width / 2f - 20, 40, Screen.width / 2f - 25, 20), "Bend Collider", properties[i].bendCollider);
                                    if (properties[i].bendCollider)
                                    {
                                        properties[i].colliderUpdateRate = EditorGUI.FloatField(new Rect(Screen.width / 2f - 20, 58, Screen.width / 2f - 25, 18), "  Update Rate", properties[i].colliderUpdateRate);
                                    }
                                }
                                else GUI.Label(new Rect(Screen.width / 2f - 20, 40, Screen.width / 2f - 25, 22), "No Mesh Collider Available");
                            }
                        }
                        else GUI.Label(new Rect(Screen.width / 2f - 20, 22, Screen.width / 2f - 25, 22), "No Mesh Available");
                    }
                    GUI.EndGroup();
                } else 
                {
                    if (!properties[i].enabled) GUI.color = Color.gray;
                    else GUI.color = Color.white;
                    GUILayout.Box("", GUILayout.Height(18), GUILayout.Width(Screen.width - 20));
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.width -= 30;
                    if (lastRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.mouseDown)
                    {
                        selectedProperty = i;
                        drawTransform = properties[i].transform.transform;
                        Repaint();
                        SceneView.RepaintAll();
                    }
                    lastRect.width += 30;
                    GUI.BeginGroup(lastRect);
                    GUI.Label(new Rect(5f, 1f, Screen.width * 0.7f, 22), properties[i].transform.transform.name);
                    properties[i].enabled = EditorGUI.Toggle(new Rect(Screen.width- 40f - 22f, 1f, 22, 22), properties[i].enabled);
                    GUI.EndGroup();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            BaseGUI();
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            ObjectBender bender = (ObjectBender)target;
            if (drawTransform != null)
            {
                Handles.BeginGUI();
                Vector2 screenPosition = HandleUtility.WorldToGUIPoint(drawTransform.position);
                SplineEditorGUI.Label(new Rect(screenPosition.x - 120 + drawTransform.name.Length * 4, screenPosition.y, 120, 25), drawTransform.name);
                Handles.EndGUI();
            }
            for(int i = 0; i < bender.bendProperties.Length; i++)
            {
                if(bender.bendProperties[i].bendSpline && bender.bendProperties[i].splineComputer != null)
                {
                    SplineEditor.DrawSplineComputer(bender.bendProperties[i].splineComputer, SceneView.lastActiveSceneView.camera, false, false, 0.2f);
                }
            }

            //Draw bounds
            if (bender.bend) return;
            TS_Bounds bound = bender.GetBounds();
            Vector3 a = bender.transform.TransformPoint(bound.min);
            Vector3 b = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.min.y, bound.min.z));
            Vector3 c = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.min.y, bound.max.z));
            Vector3 d = bender.transform.TransformPoint(new Vector3(bound.min.x, bound.min.y, bound.max.z));

            Vector3 e = bender.transform.TransformPoint(new Vector3(bound.min.x, bound.max.y, bound.min.z));
            Vector3 f = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.max.y, bound.min.z));
            Vector3 g = bender.transform.TransformPoint(new Vector3(bound.max.x, bound.max.y, bound.max.z));
            Vector3 h = bender.transform.TransformPoint(new Vector3(bound.min.x, bound.max.y, bound.max.z));

            Handles.color = Color.gray;
            Handles.DrawLine(a, b);
            Handles.DrawLine(b, c);
            Handles.DrawLine(c, d);
            Handles.DrawLine(d, a);

            Handles.DrawLine(e, f);
            Handles.DrawLine(f, g);
            Handles.DrawLine(g, h);
            Handles.DrawLine(h, e);

            Handles.DrawLine(a, e);
            Handles.DrawLine(b, f);
            Handles.DrawLine(c, g);
            Handles.DrawLine(d, h);

            Vector3 r = bender.transform.right;
            Vector3 fr = bender.transform.forward;

            switch (bender.axis)
            {
                case ObjectBender.Axis.Z: Handles.color = Color.blue; Handles.DrawLine(r + b, r + c);  break;
                case ObjectBender.Axis.X: Handles.color = Color.red; Handles.DrawLine(b - fr, a - fr); break;
                case ObjectBender.Axis.Y: Handles.color = Color.green; Handles.DrawLine(b- fr + r, f - fr + r); break;
            }
        }
    }
}
#endif
