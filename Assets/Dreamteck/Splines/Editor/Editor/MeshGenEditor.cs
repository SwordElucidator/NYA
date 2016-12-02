#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(MeshGenerator))]
    public class MeshGenEditor : SplineUserEditor
    {
        protected bool showSize = true;
        protected bool showColor = true;
        protected bool showDoubleSided = true;
        protected bool showFlipFaces = true;
        protected bool showRotation = true;
        protected bool showInfo = false;
        protected bool showOffset = true;
        protected bool showTangents = true;
        protected bool showOptimize = true;
        protected bool showNormalMethod = true;
        protected string[] normalMethods = new string[] { "Recalculate", "Spline normals" };
        private int framesPassed = 0;

        BakeMeshWindow bakeWindow = null;

        public virtual void ShowInfo()
        {
            MeshGenerator generator = (MeshGenerator)target;
            EditorGUILayout.Space();
            showInfo = EditorGUILayout.Foldout(showInfo, "Info & Components");
            if (showInfo)
            {
                MeshFilter filter = generator.GetComponent<MeshFilter>();
                if (filter == null) return;
                MeshRenderer renderer = generator.GetComponent<MeshRenderer>();
                string str = "";
                if (filter.sharedMesh != null) str = "Vertices: " + filter.sharedMesh.vertexCount + "\r\nTriangles: " + (filter.sharedMesh.triangles.Length / 3);
                else str = "No info available in prefab mode";
                EditorGUILayout.HelpBox(str, MessageType.Info);
                bool showFilter = filter.hideFlags == HideFlags.None;
                bool last = showFilter;
                showFilter = EditorGUILayout.Toggle("Show Mesh Filter", showFilter);
                if(last != showFilter)
                {
                    if (showFilter) filter.hideFlags = HideFlags.None;
                    else filter.hideFlags = HideFlags.HideInInspector;
                }
                bool showRenderer = renderer.hideFlags == HideFlags.None;
                last = showRenderer;
                showRenderer = EditorGUILayout.Toggle("Show Mesh Renderer", showRenderer);
                if (last != showRenderer)
                {
                    if (showRenderer) renderer.hideFlags = HideFlags.None;
                    else renderer.hideFlags = HideFlags.HideInInspector;
                }
            }

        }
        
        public override void BaseGUI()
        {
            MeshGenerator generator = (MeshGenerator)target;
            base.BaseGUI();
            if (showTangents || showOptimize)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh", EditorStyles.boldLabel);
                if(showOptimize) generator.optimize = EditorGUILayout.Toggle("Optimize", generator.optimize);
                if (showTangents) generator.calculateTangents = EditorGUILayout.Toggle("Calculate Tangents", generator.calculateTangents);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vertices", EditorStyles.boldLabel);
            if (showSize) generator.size = EditorGUILayout.FloatField("Size", generator.size);
            if(showColor) generator.color = EditorGUILayout.ColorField("Color", generator.color);
            if(showNormalMethod)generator.normalMethod = EditorGUILayout.Popup("Normal Method", generator.normalMethod, normalMethods);
            if(showOffset) generator.offset = EditorGUILayout.Vector3Field("Offset", generator.offset);
            if(showRotation) generator.rotation = EditorGUILayout.Slider("Rotation", generator.rotation, -180f, 180f);

            if (showDoubleSided || showFlipFaces)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Faces", EditorStyles.boldLabel);
                if (showDoubleSided) generator.doubleSided = EditorGUILayout.Toggle("Double-sided", generator.doubleSided);
                if (!generator.doubleSided && showFlipFaces) generator.flipFaces = EditorGUILayout.Toggle("Flip faces", generator.flipFaces);
            }

            if(generator.GetComponent<MeshCollider>() != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh Collider", EditorStyles.boldLabel);
                generator.colliderUpdateRate = EditorGUILayout.FloatField("Collider Update Iterval", generator.colliderUpdateRate);
            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            MeshGenerator generator = (MeshGenerator)target;
            if (Application.isPlaying) return;
            framesPassed++;
            if(framesPassed >= 100)
            {
                framesPassed = 0;
                if (generator != null && generator.GetComponent<MeshCollider>() != null) generator.UpdateCollider();
            }
        }
 
        public override void OnInspectorGUI()
        {
            MeshGenerator generator = (MeshGenerator)target;
            if (generator.baked)
            {
                GUILayout.Label("BAKED. DOES NOT UPDATE.");
                if (GUILayout.Button("Revert Bake"))
                {
                    generator.Unbake();
                    EditorUtility.SetDirty(target);
                }
                return;
            }
            EditorGUI.BeginChangeCheck();
            BaseGUI();
            ShowInfo();
            if (GUILayout.Button("Bake Mesh"))
            {
                bakeWindow = EditorWindow.GetWindow<BakeMeshWindow>();
                bakeWindow.Init(generator);
            }
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);
        }
        
        protected override void Awake()
        {
            MeshGenerator generator = (MeshGenerator)target;
            MeshRenderer rend = generator.GetComponent<MeshRenderer>();
            if (rend == null) return;
            if (rend.sharedMaterial == null) rend.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            base.Awake();
        }
        
        protected override void OnDestroy()
        {
            MeshGenerator generator = (MeshGenerator)target;
            base.OnDestroy();
            MeshGenerator gen = (MeshGenerator)target;
            if (gen == null) return;
            if (gen.GetComponent<MeshCollider>() != null) generator.UpdateCollider();
            if (bakeWindow != null) bakeWindow.Close();
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            MeshGenerator generator = (MeshGenerator)target;
            if (generator == null) return;
            MeshFilter filter = generator.GetComponent<MeshFilter>();
            if (filter != null) filter.hideFlags = HideFlags.None;
            MeshRenderer renderer = generator.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.hideFlags = HideFlags.None;
        }

        protected virtual void UVControls(MeshGenerator generator)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Uv Coordinates", EditorStyles.boldLabel);
            generator.uvMode = (MeshGenerator.UVMode)EditorGUILayout.EnumPopup("UV Mode", generator.uvMode);
            generator.uvOffset = EditorGUILayout.Vector2Field("UV Offset", generator.uvOffset);
            generator.uvScale = EditorGUILayout.Vector2Field("UV Scale", generator.uvScale);
        }
        
    }
}
#endif