using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines;
namespace Dreamteck.Splines
{
    public class LinePrimitive : SplinePrimitive, ISplinePrimitive
    {
        private bool mirror = true;
        private float distance = 1f;
        private Vector3 rotation = Vector3.zero;
        private int segments = 1;

        public string GetName()
        {
            return "Line";
        }

        public override void Init(SplineComputer comp)
        {
            base.Init(comp);
        }

        public void SetOrigin(Vector3 o)
        {
            origin = o;
        }

        public void Draw()
        {
            distance = EditorGUILayout.FloatField("Length", distance);
            rotation = EditorGUILayout.Vector3Field("Rotation", rotation);
            segments = EditorGUILayout.IntField("Segments", segments);
            mirror = EditorGUILayout.Toggle("Mirror", mirror);
            if (segments < 1) segments = 1;
            SplinePoint[] generated = GetPoints(distance, segments, rotation, mirror);
            OffsetPoints(generated, origin);
            computer.type = Spline.Type.Linear;
            computer.SetPoints(generated, SplineComputer.Space.Local);
            computer.Break();
            if (GUI.changed)
            {
                UpdateUsers();
                SceneView.RepaintAll();
            }
        }

        public static SplinePoint[] GetPoints(float dist, int segs, Vector3 rot, bool mir) 
        {
            Quaternion quaternion = Quaternion.Euler(rot);
            Vector3 look = quaternion*Vector3.up;
            Vector3 direction = quaternion * Vector3.forward;
            SplinePoint[] points = CreatePoints(segs + 1, 1f, look, Color.white);
            Vector3 origin = Vector3.zero;
            if (mir) origin = -direction * dist * 0.5f;
            for (int i = 0; i < points.Length; i++)
            {
               points[i].position = origin + direction * dist * ((float)i/(points.Length-1));
            }
            return points;
        }

        public void Cancel()
        {
            Revert();
        }

    }
}
