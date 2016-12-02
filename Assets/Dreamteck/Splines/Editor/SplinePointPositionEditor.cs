#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class SplinePointPositionEditor : SplinePointEditor
    {
        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            for (int i = 0; i < selected.Count; i++)
            {
                if (computer.isClosed && selected[i] == computer.pointCount - 1) continue;
                Vector3 lastPos = points[selected[i]].position;
                points[selected[i]].SetPosition(Handles.PositionHandle(points[selected[i]].position, computer.space == SplineComputer.Space.Local ? computer.transform.rotation : Quaternion.identity));
                if (!change)
                {
                    if (lastPos != points[selected[i]].position)
                    {
                        change = true;
                        for (int n = 0; n < selected.Count; n++)
                        {
                            if (n == i) continue;
                            points[selected[n]].SetPosition(points[selected[n]].position + (points[selected[i]].position - lastPos));
                        }
                    }
                }
                if (computer.type == Spline.Type.Bezier)
                {
                    Handles.DrawDottedLine(points[selected[i]].position, points[selected[i]].tangent, 6f);
                    Handles.DrawDottedLine(points[selected[i]].position, points[selected[i]].tangent2, 6f);
                    lastPos = points[selected[i]].tangent;
                    points[selected[i]].SetTangentPosition(Handles.PositionHandle(points[selected[i]].tangent, computer.space == SplineComputer.Space.Local ? computer.transform.rotation : Quaternion.identity));
                    if (lastPos != points[selected[i]].tangent) change = true;
                    lastPos = points[selected[i]].tangent2;
                    points[selected[i]].SetTangent2Position(Handles.PositionHandle(points[selected[i]].tangent2, computer.space == SplineComputer.Space.Local ? computer.transform.rotation : Quaternion.identity));
                    if (lastPos != points[selected[i]].tangent2) change = true;
                }
            }
            return change;
        }
    }
}
#endif
