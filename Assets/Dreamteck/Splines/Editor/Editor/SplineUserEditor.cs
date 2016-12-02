using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineUser), true)]
    public class SplineUserEditor : Editor
    {
        protected bool showResolution = true;
        protected bool showClip = true;
        protected bool showAveraging = true;
        protected bool showUpdateMethod = true;
        protected bool showMultithreading = true;
        private PathWindow pathWindow = null;
        private bool initGUI = true;

        enum SampleTarget { Computer, User }
        private SampleTarget sampleTarget = SampleTarget.Computer;

        public virtual void BaseGUI() {
            if (initGUI)
            {
                SplineEditorGUI.Initialize();
                initGUI = false;
            }
            base.OnInspectorGUI();
            SplineUser user = (SplineUser)target;
            bool isTargetComputer = (user.user == null || sampleTarget == SampleTarget.Computer);
            if (user.computer != null && !user.computer.IsSubscribed(user)) user.computer.Subscribe(user);
            Undo.RecordObject(user, "Inspector Change");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spline User", EditorStyles.boldLabel, GUILayout.Width(85));
            GUI.color = new Color(1f, 1f, 1f, 0.75f);
            sampleTarget = (SampleTarget)EditorGUILayout.EnumPopup(sampleTarget, GUILayout.Width(75));
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            if (sampleTarget == SampleTarget.Computer) user.computer = (SplineComputer)EditorGUILayout.ObjectField("Computer", user.computer, typeof(SplineComputer), true);
            else
            {
                SplineUser lastUser = user.user;
                user.user = (SplineUser)EditorGUILayout.ObjectField("User", user.user, typeof(SplineUser), true);
                if(lastUser != user.user && user.rootUser == user)
                {
                    user.user = null;
                    EditorUtility.DisplayDialog("Cannot assign user.", "A SplineUser component cannot sample itself, please select another user to sample.", "OK");
                }
            }
            if (showUpdateMethod && isTargetComputer) user.updateMethod = (SplineUser.UpdateMethod)EditorGUILayout.EnumPopup("Update Method", user.updateMethod);
            if (user.computer == null && isTargetComputer) EditorGUILayout.HelpBox("No SplineComputer or SplineUser is referenced. Reference a SplineComputer or another SplineUser component to make this SplineUser work.", MessageType.Error);

            if (showResolution && isTargetComputer) user.resolution = (double)EditorGUILayout.Slider("Resolution", (float)user.resolution, 0f, 1f);
            if (showClip)
            {
                EditorGUILayout.BeginHorizontal();
                float clipFrom = (float)user.clipFrom;
                float clipTo = (float)user.clipTo;
                EditorGUILayout.MinMaxSlider(new GUIContent("Clip Range:"), ref clipFrom, ref clipTo, 0f, 1f);
                user.clipFrom = clipFrom;
                user.clipTo = clipTo;
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(30));
                user.clipFrom = EditorGUILayout.FloatField((float)user.clipFrom);
                user.clipTo = EditorGUILayout.FloatField((float)user.clipTo);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }
            if (showAveraging && (user.user == null || sampleTarget == SampleTarget.Computer)) user.averageResultVectors = EditorGUILayout.Toggle("Average Result Vectors", user.averageResultVectors);
            if (showMultithreading) user.multithreaded = EditorGUILayout.Toggle("Multithreading", user.multithreaded);
            
            user.buildOnAwake = EditorGUILayout.Toggle("Build on Awake", user.buildOnAwake);
            if (user.computer != null && user.computer.nodeLinks.Length > 0 && isTargetComputer)
            {
                if(GUILayout.Button("Edit junction path"))
                {
                    pathWindow = EditorWindow.GetWindow<PathWindow>();
                    pathWindow.init(this, "Junction Path", new Vector2(300, 150));
                }
            }
        }

        protected virtual void OnSceneGUI()
        {
            if (initGUI)
            {
                SplineEditorGUI.Initialize();
                initGUI = false;
            }
            SplineUser user = (SplineUser)target;
            if (user.computer == null)
            {
                SplineUser root = user.rootUser;
                if (root == null) return;
                if (root.computer == null) return;
                List<SplineComputer> allComputers = root.computer.GetConnectedComputers();
                for (int i = 0; i < allComputers.Count; i++)
                {
                    if (allComputers[i] == root.computer) continue;
                    SplineEditor.DrawSplineComputer(allComputers[i], SceneView.currentDrawingSceneView.camera, false, false, 0.4f);
                }
                for (int i = 0; i < root.address.depth; i++)
                {
                    if (user.address.elements[i].computer == root.computer) continue;
                    SplineEditor.DrawSplineComputer(root.address.elements[i].computer, SceneView.currentDrawingSceneView.camera, false, false, 1f, root.address.elements[i].startPercent, root.address.elements[i].endPercent, false);
                }
            }
            else
            {
                SplineComputer rootComputer = user.GetComponent<SplineComputer>();
                List<SplineComputer> allComputers = user.computer.GetConnectedComputers();
                for (int i = 0; i < allComputers.Count; i++)
                {
                    if (allComputers[i] == rootComputer) continue;
                    SplineEditor.DrawSplineComputer(allComputers[i], SceneView.currentDrawingSceneView.camera, false, false, 0.4f);
                }
                for (int i = 0; i < user.address.depth; i++)
                {
                    if (user.address.elements[i].computer == rootComputer) continue;
                    SplineEditor.DrawSplineComputer(user.address.elements[i].computer, SceneView.currentDrawingSceneView.camera, false, false, 1f, user.address.elements[i].startPercent, user.address.elements[i].endPercent, false);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            BaseGUI();
        }

        protected virtual void OnDestroy()
        {
            if (pathWindow != null) pathWindow.Close();
            SplineUser user = (SplineUser)target;
            if (Application.isEditor && !Application.isPlaying)
            {
                if (user == null) OnDelete(); //The object or the component is being deleted
                else if (user.computer != null) user.Rebuild(true);
            }
        }

        protected virtual void OnDelete()
        {

        }

        protected virtual void Awake()
        {
            SplineUser user = (SplineUser)target;
            if (user.user != null) sampleTarget = SampleTarget.User;
            else sampleTarget = SampleTarget.Computer;
            user.EditorAwake();
        }
    }
}
