using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Dreamteck.Splines
{
    public class BakeTool : SplineTool, ISplineTool
    {
        public enum BakeGroup { All, Selected, AllExcluding }
        BakeGroup bakeGroup = BakeGroup.All;
        MeshGenerator[] found = new MeshGenerator[0];
        List<MeshGenerator> selected = new List<MeshGenerator>();
        List<MeshGenerator> excluded = new List<MeshGenerator>();

        bool isStatic = true;
        bool lightmapUV = true;
        bool saveMesh = false;
        string savePath = "";
        bool removeComputer = false;
        bool permanent = false;

        DirectoryInfo dirInfo;

        Vector2 scroll1, scroll2;

        public string GetName()
        {
            return "Bake Meshes";
        }

        public void Close()
        {
            isOpen = false;
        }

        public void Draw(Rect windowRect)
        {
            if (!isOpen) Init();
            isOpen = true;
            bakeGroup = (BakeGroup)EditorGUILayout.EnumPopup("Bake Mode", bakeGroup);
            if (bakeGroup == BakeGroup.Selected)
            {
                MeshGenSelector(ref selected, "Selected");
            } else if(bakeGroup == BakeGroup.AllExcluding)
            {
                MeshGenSelector(ref excluded, "Excluded");
            }


            saveMesh = EditorGUILayout.Toggle("Save OBJs", saveMesh);
            if (saveMesh)
            {
                EditorGUILayout.LabelField("Save Path: " + savePath);
                if (GUILayout.Button("Browse Path"))
                {
                    savePath = EditorUtility.OpenFolderPanel("Save Directory", Application.dataPath, "folder");
                    dirInfo = new DirectoryInfo(savePath);
                }
            }


            isStatic = EditorGUILayout.Toggle("Make Static", isStatic);
            lightmapUV = EditorGUILayout.Toggle("Generate Lightmap UVs", lightmapUV);
            removeComputer = EditorGUILayout.Toggle("Remove SplineComputers", removeComputer);
            permanent = EditorGUILayout.Toggle("Permanent", permanent);
           
            if (GUILayout.Button("Bake"))
            {
                string suff = "all";
                if (bakeGroup == BakeGroup.Selected) suff = "selected";
                if (bakeGroup == BakeGroup.AllExcluding) suff = "all excluding";
                if(EditorUtility.DisplayDialog("Bake " + suff, "This operation cannot be undone. Are you sure you want to bake the meshes?", "Yes", "No"))
                {
                    switch (bakeGroup)
                    {
                        case BakeGroup.All: BakeAll(); break;
                        case BakeGroup.Selected: BakeSelected(); break;
                        case BakeGroup.AllExcluding: BakeExcluding(); break;
                    }
                }
            }
        }

        private void BakeAll()
        {
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < found.Length; i++)
            {
                float percent = (float)i / (found.Length - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(found[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        private void BakeSelected()
        {
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < selected.Count; i++)
            {
                float percent = (float)i / (selected.Count - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(selected[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        private void BakeExcluding()
        {
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < found.Length; i++)
            {
                float percent = (float)i / (found.Length - 1);
                EditorUtility.DisplayProgressBar("Baking progress", "Baking generator " + i, percent);
                Bake(found[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        private void Bake(MeshGenerator gen)
        {
            gen.Bake(isStatic, lightmapUV);
            MeshRenderer renderer = gen.GetComponent<MeshRenderer>();
            MeshFilter filter = gen.GetComponent<MeshFilter>();
            if (saveMesh)
            {
                FileInfo[] files = dirInfo.GetFiles(filter.sharedMesh.name + "*.obj");
                string meshName = filter.sharedMesh.name;
                if (files.Length > 0) meshName += "_" + files.Length;
                string path = savePath + "/" + meshName + ".obj";
                string relativepath = "Assets" + path.Substring(Application.dataPath.Length);
                string objString = MeshUtility.ToOBJString(filter.sharedMesh, renderer.sharedMaterials);
                File.WriteAllText(path, objString);
                AssetDatabase.ImportAsset(relativepath, ImportAssetOptions.ForceSynchronousImport);
#if UNITY_5_0
                filter.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(relativepath, typeof(Mesh));
#else
                filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(relativepath);
#endif
            }
            if (removeComputer) GameObject.DestroyImmediate(gen.computer);
            if (permanent) GameObject.DestroyImmediate(gen);
        }

        private void Refresh()
        {
            found = GameObject.FindObjectsOfType<MeshGenerator>();
        }

        void OnFocus()
        {
            Refresh();
        }

        void OnDestroy()
        {
            EditorPrefs.SetBool("BakeTool_isStatic", isStatic);
            EditorPrefs.SetBool("BakeTool_lightmapUV", lightmapUV);
            EditorPrefs.SetBool("BakeTool_saveMesh", saveMesh);
            EditorPrefs.SetBool("BakeTool_removeComputer", removeComputer);
        }

        private void Init()
        {
            if (EditorPrefs.HasKey("BakeTool_isStatic")) isStatic = EditorPrefs.GetBool("BakeTool_isStatic");
            if (EditorPrefs.HasKey("BakeTool_lightmapUV")) lightmapUV = EditorPrefs.GetBool("BakeTool_lightmapUV");
            if (EditorPrefs.HasKey("BakeTool_saveMesh")) saveMesh = EditorPrefs.GetBool("BakeTool_saveMesh");
            if (EditorPrefs.HasKey("BakeTool_removeComputer")) removeComputer = EditorPrefs.GetBool("BakeTool_removeComputer");
            Refresh();
        }

        private void MeshGenSelector(ref List<MeshGenerator> list, string title)
        {
            List<MeshGenerator> availalbe = new List<MeshGenerator>(found);
            for (int i = availalbe.Count-1; i >= 0; i--)
            {
                for (int n = 0; n < list.Count; n++)
                {
                    if (list[n] == availalbe[i])
                    {
                        availalbe.RemoveAt(i);
                        break;
                    }
                }
            }
            GUILayout.Box("Available", GUILayout.Width(Screen.width - 15 - Screen.width/3f), GUILayout.Height(100));
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y += 15;
            rect.height -= 15;
            scroll1 = GUI.BeginScrollView(rect, scroll1, new Rect(0, 0, rect.width, 22 * availalbe.Count));
            for (int i = 0; i < availalbe.Count; i++)
            {
                GUI.Label(new Rect(5, 22 * i, rect.width - 30, 22), availalbe[i].name);
                if (GUI.Button(new Rect(rect.width - 29, 22 * i, 22, 22), "+"))
                {
                    list.Add(availalbe[i]);
                    availalbe.RemoveAt(i);
                    break;
                }
            }
                GUI.EndScrollView();
            EditorGUILayout.Space();
            GUILayout.Box(title, GUILayout.Width(Screen.width - 15 - Screen.width / 3f), GUILayout.Height(100));

            rect = GUILayoutUtility.GetLastRect();
            rect.y += 15;
            rect.height -= 15;
            scroll2 = GUI.BeginScrollView(rect, scroll2, new Rect(0, 0, rect.width, 22 * list.Count));
            for (int i = list.Count-1; i >= 0; i--)
            {
                GUI.Label(new Rect(5, 22 * i, rect.width - 30, 22), list[i].name);
                if (GUI.Button(new Rect(rect.width - 29, 22 * i, 22, 22), "x"))
                {
                    list.RemoveAt(i);
                }
            }
            GUI.EndScrollView();
        }
    }
}