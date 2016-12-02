#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class SplinePointScaleEditor : SplinePointEditor
    {
        private PointTransformer transformer;

        public bool scaleSize = true;
        public bool scaleTangents = true;

        public override void LoadState()
        {
            base.LoadState();
            scaleSize = LoadBool("scaleSize");
            scaleTangents = LoadBool("scaleTangents");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveBool("scaleSize", scaleSize);
            SaveBool("scaleTangents", scaleTangents);
        }

        public void Reset(ref SplinePoint[] points, ref List<int> selected)
        {
            if (computer.space == SplineComputer.Space.Local) transformer = new PointTransformer(points, selected, computer.transform);
            else transformer = new PointTransformer(points, selected);
        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            if (transformer == null) Reset(ref points, ref selected);
            Vector3 lastScale = transformer.scale;
            transformer.scale = Handles.ScaleHandle(transformer.scale, transformer.center, computer.space == SplineComputer.Space.Local ? computer.transform.rotation : Quaternion.identity, HandleUtility.GetHandleSize(transformer.center));
            if (lastScale != transformer.scale)
            {
                change = true;
                points = transformer.GetScaled(scaleSize, scaleTangents);
            }
            return change;
        }
    }
}
#endif
