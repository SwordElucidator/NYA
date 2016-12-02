using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines;
namespace Dreamteck.Splines
{
    public class SpiralPrimitive : SplinePrimitive, ISplinePrimitive
    {
        
        private float startRadius = 1f;
        private float endRadius = 1f;
        private int axis = 1;
        private float offset = 1f;
        private int iterations = 3;
        private AnimationCurve curve;
        private string[] axisText = new string[] {"X", "Y", "Z"};

        public string GetName()
        {
            return "Spiral";
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
            axis = EditorGUILayout.Popup("Axis", axis, axisText);
            startRadius = EditorGUILayout.FloatField("Start Radius", startRadius);
            endRadius = EditorGUILayout.FloatField("End Radius", endRadius);
            offset = EditorGUILayout.FloatField("Offset", offset);
            iterations = EditorGUILayout.IntField("Iterations", iterations);
            if (curve == null) curve = new AnimationCurve();
            if (curve.keys.Length == 0)
            {
                float tan45 = Mathf.Tan(Mathf.Deg2Rad * 45);
                curve.AddKey(new Keyframe(0, 0, tan45, tan45));
                curve.AddKey(new Keyframe(1, 1, tan45, tan45));
            }
            curve = EditorGUILayout.CurveField("Radius Curve", curve);
            if (iterations < 1) iterations = 1;
            SplinePoint[] generated = GetPoints(axis, startRadius, endRadius, offset, iterations, curve);
            OffsetPoints(generated, origin);
            computer.Break();
            computer.type = Spline.Type.Bezier;
            computer.SetPoints(generated, SplineComputer.Space.Local);
            if (GUI.changed)
            {
                UpdateUsers();
                SceneView.RepaintAll();
            }
        }

        public static SplinePoint[] GetPoints(int axis, float startRadius, float endRadius, float offset, int iterations, AnimationCurve curveControl) 
        {
            Vector3 look = Vector3.right;
            if (axis == 1) look = Vector3.up;
            if (axis == 2) look = Vector3.forward;
            SplinePoint[] points = CreatePoints(iterations*4+1, 1f, look, Color.white);
            float radiusDelta = Mathf.Abs(endRadius - startRadius);
            float radiusDeltaPercent = radiusDelta / Mathf.Max(Mathf.Abs(endRadius), Mathf.Abs(startRadius));
            float multiplier = 1f;
            if (endRadius > startRadius) multiplier = -1;
            float angle = 0f;
            float off = 0f;
            for(int i = 0; i <= iterations * 4; i++)
            {
                float percent = curveControl.Evaluate((float)i / (iterations*4));
                float radius = Mathf.Lerp(startRadius, endRadius, percent);
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
                points[i].position = rot * Vector3.up / 2f * radius + Vector3.forward * off;
                Quaternion tangentRot = Quaternion.identity;
                if (multiplier > 0) tangentRot = Quaternion.AngleAxis(Mathf.Lerp(0f, 90f * 0.16f, radiusDeltaPercent*percent), Vector3.forward);
                else tangentRot = Quaternion.AngleAxis(Mathf.Lerp(0f, -90f * 0.16f, (1f - percent) * radiusDeltaPercent), Vector3.forward);
                points[i].tangent = points[i].position + (tangentRot*rot*Vector3.right * radius - Vector3.forward * offset/4f) * 2 * (Mathf.Sqrt(2f) - 1f) / 3f;
                points[i].tangent2 = points[i].position + (points[i].tangent - points[i].position);
                off += offset / 4f;
                angle += 90f;
            }

            if (look != Vector3.forward)
            {
                Quaternion lookRot = Quaternion.LookRotation(look);
                RotatePoints(points, lookRot);
            }
            return points;
        }

        public void Cancel()
        {
            Revert();
        }

    }
}
