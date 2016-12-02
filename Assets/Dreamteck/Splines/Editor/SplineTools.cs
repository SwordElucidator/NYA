#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class SplineTools
    {

        [MenuItem("Tools/Dreamteck/Splines/Update Nodes %q")]
        private static void UpdateNodes()
        {
            Node[] nodes = GameObject.FindObjectsOfType<Node>();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].UpdateConnectedComputers();
            }
        }
    }
}
#endif