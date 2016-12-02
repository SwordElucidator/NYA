using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace Dreamteck.Splines
{
    public class BakeMeshWindow : EditorWindow
    {
        public bool isStatic = true;
        public bool lightmapUV = true;
        public bool saveMesh = false;
        public string savePath = "";
        public bool copy = false;
        public bool removeComputer = false;
        public bool permanent = false;

        MeshFilter filter;
        MeshGenerator meshGen;

        public void Init(MeshGenerator generator)
        {
#if UNITY_5_0
            title = "Bake Mesh";
#else
            titleContent = new GUIContent("Bake Mesh");
#endif
            meshGen = generator;
            filter = generator.GetComponent<MeshFilter>();
            if (EditorPrefs.HasKey("BakeWindow_isStatic")) isStatic = EditorPrefs.GetBool("BakeWindow_isStatic");
            if (EditorPrefs.HasKey("BakeWindow_lightmapUV")) lightmapUV = EditorPrefs.GetBool("BakeWindow_lightmapUV");
            if (EditorPrefs.HasKey("BakeWindow_saveMesh")) saveMesh = EditorPrefs.GetBool("BakeWindow_saveMesh");
            if (EditorPrefs.HasKey("BakeWindow_copy")) copy = EditorPrefs.GetBool("BakeWindow_copy");
            if (EditorPrefs.HasKey("BakeWindow_removeComputer")) removeComputer = EditorPrefs.GetBool("BakeWindow_removeComputer");
            if (EditorPrefs.HasKey("BakeWindow_permanent")) permanent = EditorPrefs.GetBool("BakeWindow_permanent");
            minSize = new Vector2(340, 190);
            maxSize = minSize;
        }

        void OnDestroy()
        {
            EditorPrefs.SetBool("BakeWindow_isStatic", isStatic);
            EditorPrefs.SetBool("BakeWindow_lightmapUV", lightmapUV);
            EditorPrefs.SetBool("BakeWindow_saveMesh", saveMesh);
            EditorPrefs.SetBool("BakeWindow_copy", copy);
            EditorPrefs.SetBool("BakeWindow_removeComputer", removeComputer);
            EditorPrefs.SetBool("BakeWindow_permanent", permanent);
        }

        void OnGUI() {
            string bakeText = "Bake";
            EditorGUILayout.BeginHorizontal();
            GUIContent saveMeshText = new GUIContent("Save as OBJ [?]", "Saves the mesh as an OBJ file which can then be used in other scenes and prefabs. OBJ files do not support vertex colors and secondary UV sets.");
            saveMesh = EditorGUILayout.Toggle(saveMeshText, saveMesh);
            if (saveMesh)
            {
                copy = EditorGUILayout.Toggle("Save as copy", copy);
                if (copy) bakeText = "Save Copy";
            }
            EditorGUILayout.EndHorizontal();
            bool hold = false;
            if (saveMesh)
            {
                EditorGUILayout.LabelField("Save Path: " + savePath);
                if (GUILayout.Button("Browse Path"))
                {
                    string meshName = "mesh";
                    if (filter != null) meshName = filter.sharedMesh.name;
                    savePath = EditorUtility.SaveFilePanel("Save " + meshName + ".obj", Application.dataPath, meshName + ".obj", "obj");
                }
            }

            EditorGUILayout.Space();
            bool isCopy = saveMesh && copy;
            if(!isCopy) isStatic = EditorGUILayout.Toggle("Make Static", isStatic);
            if(!saveMesh) lightmapUV = EditorGUILayout.Toggle("Generate Lightmap UVs", lightmapUV);
            SplineUser[] users = meshGen.GetComponents<SplineUser>();
            if (users.Length == 1 && !isCopy) removeComputer = EditorGUILayout.Toggle("Remove SplineComputer", removeComputer);
            if (!isCopy) permanent = EditorGUILayout.Toggle("Permanent", permanent);
            bool _removeComputer = removeComputer;
            if (users.Length != 1) _removeComputer = false;
            if (_removeComputer && meshGen.computer.subscriberCount > 1 && !isCopy) EditorGUILayout.HelpBox("WARNING: Removing the SplineComputer from this object may cause other SplineUsers to malfunction!", MessageType.Warning);
           
            if (saveMesh)
            {
                if(savePath == "") hold = true;
                else if(!Directory.Exists(Path.GetDirectoryName(savePath))) hold = true;
            }
            if (hold) GUI.color = new Color(1f, 1f, 1f, 0.5f);
            if (GUILayout.Button(bakeText))
            {
                Undo.RecordObject(meshGen.gameObject, "Bake mesh");
                if (hold) return;
                if (!isCopy) meshGen.Bake(isStatic, lightmapUV);
                else if (lightmapUV) Unwrapping.GenerateSecondaryUVSet(filter.sharedMesh);
                MeshRenderer renderer = meshGen.GetComponent<MeshRenderer>();
                if (saveMesh)
                {
                    string relativepath = "Assets" + savePath.Substring(Application.dataPath.Length);
                    string objString = MeshUtility.ToOBJString(filter.sharedMesh, renderer.sharedMaterials);
                    File.WriteAllText(savePath, objString);
                    AssetDatabase.ImportAsset(relativepath, ImportAssetOptions.ForceSynchronousImport);
#if UNITY_5_0
                   if(!isCopy) filter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(relativepath, typeof(Mesh));
#else
                   if (!isCopy) filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(relativepath);
#endif
                }
                if (removeComputer && !isCopy) DestroyImmediate(meshGen.computer);
                if (permanent && !isCopy) DestroyImmediate(meshGen);
                Close();
            }
            string add = "";
            if (removeComputer) add += "It will also remove the SplineComputer component from the object.";
            EditorGUILayout.HelpBox("This operation will remove the mesh generator component and will make the mesh uneditable."+add, MessageType.Info);
        }
    }
}
