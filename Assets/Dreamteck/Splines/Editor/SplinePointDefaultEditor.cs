using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class SplinePointDefaultEditor : SplinePointEditor
    {
        public bool additive = false;
        public bool excludeSelected = false;
        public bool selectOnMove = true;

        public bool deleteMode = false;
        public bool click = false;

        public int minimumRectSize = 5;
        private Vector2 rectStart = Vector2.zero;
        private Vector2 rectEnd = Vector2.zero;
        private Rect rect;
        private bool drag = false;
        private bool finalize = false;

        public bool isDragging
        {
            get
            {
                return drag && rect.width >= minimumRectSize && rect.height >= minimumRectSize;
            }
        }


        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            if (!drag)
            {
                if (finalize)
                {
                    if (rect.width > 0f && rect.height > 0f)
                    {
                        if (!additive) ClearSelection(ref selected);
                        for (int i = 0; i < points.Length; i++)
                        {
                            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(points[i].position);
                            if (rect.Contains(guiPoint))
                            {
                                AddPointSelection(i, ref selected);
                                change = true;
                            }
                        }
                    }
                    finalize = false;
                }
            }
            else
            {
                rectEnd = Event.current.mousePosition;
                rect = new Rect(Mathf.Min(rectStart.x, rectEnd.x), Mathf.Min(rectStart.y, rectEnd.y), Mathf.Abs(rectEnd.x - rectStart.x), Mathf.Abs(rectEnd.y - rectStart.y));
                if (rect.width >= minimumRectSize && rect.height >= minimumRectSize)
                {
                    Color col = SplineEditorGUI.selectionColor;
                    if (deleteMode) col = Color.red;
                    col.a = 0.4f;
                    GUI.color = col;
                    Handles.BeginGUI();
                    GUI.Box(rect, "", SplineEditorGUI.whiteBox);
                    Handles.EndGUI();
                    SceneView.RepaintAll();
                }
            }
           
            for (int i = 0; i < points.Length; i++)
            {
                if (computer.isClosed && i == points.Length - 1) break;
                bool moved = false;
                bool isSelected = selected.Contains(i);
                Vector3 lastPos = points[i].position;
                Handles.color = Color.clear;
                if (excludeSelected && isSelected) Handles.FreeMoveHandle(points[i].position, Quaternion.identity, HandleUtility.GetHandleSize(points[i].position) * 0.1f, Vector3.zero, Handles.RectangleCap);
               else  points[i].SetPosition(Handles.FreeMoveHandle(points[i].position, Quaternion.identity, HandleUtility.GetHandleSize(points[i].position) * 0.1f, Vector3.zero, Handles.RectangleCap));
                
                if (!change)
                {
                    if (lastPos != points[i].position)
                    {
                        moved = true;
                        change = true;
                        if (isSelected)
                        {
                            for (int n = 0; n < selected.Count; n++)
                            {
                                if (selected[n] == i) continue;
                                points[selected[n]].SetPosition(points[selected[n]].position + (points[i].position - lastPos));
                            }
                        }
                        else if (selectOnMove)
                        {
                            selected.Clear();
                            selected.Add(i);
                            SceneView.RepaintAll();
                        }
                    }
                }

                 if (!moved && !isSelected)
                 {
                    if(SplineEditorHandles.HoverArea(points[i].position, 0.12f) && click)
                    {
                        if (additive) AddPointSelection(i, ref selected);
                        else SelectPoint(i, ref selected);
                        SceneView.RepaintAll();
                        change = true;
                    }
               }
                if (!excludeSelected || !isSelected)
                {
                    Handles.color = computer.editorPathColor;
                    if (deleteMode) Handles.color = Color.red;
                    else if (isSelected) Handles.color = SplineEditorGUI.selectionColor;
                    Handles.RectangleCap(i, points[i].position, Quaternion.LookRotation(-SceneView.currentDrawingSceneView.camera.transform.forward), HandleUtility.GetHandleSize(points[i].position) * 0.1f);
                }
                moved = false;
            }

            if (computer.type == Spline.Type.Bezier)
            {
                Handles.color = computer.editorPathColor;
                for (int i = 0; i < selected.Count; i++)
                {
                    Handles.DrawDottedLine(points[selected[i]].position, points[selected[i]].tangent, 6f);
                    Handles.DrawDottedLine(points[selected[i]].position, points[selected[i]].tangent2, 6f);
                    Vector3 lastPos = points[selected[i]].tangent;
                    points[selected[i]].SetTangentPosition(Handles.FreeMoveHandle(points[selected[i]].tangent, Quaternion.identity, HandleUtility.GetHandleSize(points[selected[i]].tangent) * 0.1f, Vector3.zero, Handles.CircleCap));
                    if (lastPos != points[selected[i]].tangent) change = true;
                    lastPos = points[selected[i]].tangent2;
                    points[selected[i]].SetTangent2Position(Handles.FreeMoveHandle(points[selected[i]].tangent2, Quaternion.identity, HandleUtility.GetHandleSize(points[selected[i]].tangent2) * 0.1f, Vector3.zero, Handles.CircleCap));
                    if (lastPos != points[selected[i]].tangent2) change = true;

                }
            }
            return change;
        }


        public void ClearSelection(ref List<int> selected)
        {
            selected.Clear();
            SceneView.RepaintAll();
        }

        public void SelectPoint(int index, ref List<int> selected)
        {
            if (computer.isClosed && index == computer.pointCount - 1) return;
            selected.Clear();
            selected.Add(index);
            SceneView.RepaintAll();
        }

        public void SelectPoints(List<int> indices, ref List<int> selected)
        {
            selected.Clear();
            for (int i = 0; i < indices.Count; i++)
            {
                if (computer.isClosed && i == computer.pointCount - 1) continue;
                selected.Add(indices[i]);
            }
            SceneView.RepaintAll();
        }

        public void AddPointSelection(int index, ref List<int> selected)
        {
            if (computer.isClosed && index == computer.pointCount - 1) return;
            if (selected.Contains(index)) return;
            selected.Add(index);
            SceneView.RepaintAll();
        }

        public void StartDrag(Vector2 position)
        {
            rectStart = position;
            drag = true;
        }

        public void FinishDrag()
        {
            if (!drag) return;
            drag = false;
            finalize = true;
        }

        public void CancelDrag()
        {
            drag = false;
        }

    }
}
