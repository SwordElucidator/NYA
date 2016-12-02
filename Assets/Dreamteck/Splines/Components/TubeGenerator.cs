using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Tube Generator")]
    public class TubeGenerator : MeshGenerator
    {
        public int sides
        {
            get { return _sides; }
            set
            {
                if (value != _sides)
                {
                    if (value < 3) value = 3;
                    _sides = value;
                    Rebuild(false);
                }
            }
        }

        public bool cap
        {
            get { return _cap; }
            set
            {
                if (value != _cap)
                {
                    _cap = value;
                    Rebuild(false);
                }
            }
        }

        public float integrity
        {
            get { return _integrity; }
            set
            {
                if (value != _integrity)
                {
                    _integrity = value;
                    Rebuild(false);
                }
            }
        }

        public float capUVScale
        {
            get { return _capUVScale; }
            set
            {
                if (value != _capUVScale)
                {
                    _capUVScale = value;
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private int _sides = 12;
        [SerializeField]
        [HideInInspector]
        private bool _cap = false;
        [SerializeField]
        [HideInInspector]
        private float _integrity = 360f;
        [SerializeField]
        [HideInInspector]
        private float _capUVScale = 1f;

        private bool useCap
        {
            get
            {
                if (computer != null) return _cap && (!computer.isClosed || span < 1f);
                else if (sampleUser)
                {
                    SplineUser root = rootUser;
                    if (root == null) return _cap;
                    if (root.computer != null) return _cap && (!root.computer.isClosed || root.span < 1f);
                } 
                return _cap;
            }
        }

        protected override void Reset()
        {
            base.Reset();
        }

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "tube";
        }


        protected override void BuildMesh()
        {
            if (_sides <= 2) return;
            base.BuildMesh();
            GenerateVertices();
            GenerateTriangles();
            if (doubleSided) MeshUtility.MakeDoublesided(tsMesh);
            if(calculateTangents) MeshUtility.CalculateTangents(tsMesh);
        }

        void GenerateVertices()
        {
            _sides++;
            int vertexCount = _sides * clippedSamples.Length + (useCap ? _sides * 2 : 0);
            if (tsMesh.vertexCount != vertexCount)
            {
                tsMesh.vertices = new Vector3[vertexCount];
                tsMesh.normals = new Vector3[vertexCount];
                tsMesh.colors = new Color[vertexCount];
                tsMesh.uv = new Vector2[vertexCount];
            }
            int vertexIndex = 0;
            BeginUV();
            for (int i = 0; i < clippedSamples.Length; i++)
            {
                Vector3 center = clippedSamples[i].position;
                Vector3 right = clippedSamples[i].right;
                if (offset != Vector3.zero) center += offset.x * right + offset.y * clippedSamples[i].normal + offset.z * clippedSamples[i].direction;
                if (uvMode == UVMode.UniformClip || uvMode == UVMode.UniformClamp) AddUVLength(i);
                for (int n = 0; n < _sides; n++)
                {
                    float anglePercent = (float)(n) / (_sides - 1);
                    Quaternion rot = Quaternion.AngleAxis(_integrity * anglePercent + rotation + 180f, clippedSamples[i].direction);
                    tsMesh.vertices[vertexIndex] = center + rot * right * size * clippedSamples[i].size * 0.5f;
                    tsMesh.uv[vertexIndex] = GetUV(1f - anglePercent, (float)clippedSamples[i].percent);
                    tsMesh.normals[vertexIndex] = Vector3.Normalize(tsMesh.vertices[vertexIndex] - center);
                    if (flipFaces && !doubleSided) tsMesh.normals[vertexIndex] *= -1f;
                    tsMesh.colors[vertexIndex] = clippedSamples[i].color * color;
                    vertexIndex++;
                }
            }
            if (useCap)
            {
                vertexIndex = 0;
                for (int i = tsMesh.vertexCount - _sides * 2; i < tsMesh.vertexCount; i++)
                {
                    if (vertexIndex < _sides)
                    {
                        tsMesh.vertices[i] = tsMesh.vertices[vertexIndex];
                        tsMesh.normals[i] = -clippedSamples[0].direction;
                        tsMesh.colors[i] = tsMesh.colors[vertexIndex];
                        tsMesh.uv[i] = Quaternion.AngleAxis(_integrity * ((float)(vertexIndex) / (_sides - 1)), Vector3.forward) * Vector2.right * 0.5f * capUVScale + Vector3.right * 0.5f + Vector3.up * 0.5f;
                    }
                    else
                    {
                        int index = tsMesh.vertexCount - _sides * 3 + (vertexIndex - _sides);
                        tsMesh.vertices[i] = tsMesh.vertices[index];
                        tsMesh.normals[i] = clippedSamples[clippedSamples.Length - 1].direction;
                        tsMesh.colors[i] = tsMesh.colors[index];
                        tsMesh.uv[i] = Quaternion.AngleAxis(_integrity * ((float)(vertexIndex - _sides) / (_sides - 1)), Vector3.forward) * Vector2.right * 0.5f * capUVScale + Vector3.right * 0.5f + Vector3.up * 0.5f;
                    }
                    vertexIndex++;
                }
            }
            _sides--;
        }

        void GenerateTriangles()
        {
            bool closed = _integrity == 360f;
            int faces = _sides * (clippedSamples.Length - 1);
            int indexCount = faces * 2 * 3;
            int t = indexCount;
            if (useCap) indexCount += (_sides - 1) * 3 * 2;
            tsMesh.triangles = new int[indexCount];
            MeshUtility.GeneratePlaneTriangles(_sides, clippedSamples.Length, flipFaces && !doubleSided).CopyTo(tsMesh.triangles, 0);

            if (useCap)
            {
                int finalSides = closed ? _sides - 1 : _sides;
                int vertexCount = (_sides + 1) * clippedSamples.Length;
                //Start cap
                for (int i = 0; i < finalSides - 1; i++)
                {
                    if (flipFaces && !doubleSided)
                    {

                        tsMesh.triangles[t++] = vertexCount;
                        tsMesh.triangles[t++] = i + vertexCount + 1;
                        tsMesh.triangles[t++] = i + vertexCount + 2;
                    }
                    else
                    {
                        tsMesh.triangles[t++] = i + vertexCount + 2;
                        tsMesh.triangles[t++] = i + +vertexCount + 1;
                        tsMesh.triangles[t++] = vertexCount;
                    }
                }

                //End cap
                for (int i = 0; i < finalSides - 1; i++)
                {
                    if (flipFaces && !doubleSided)
                    {
                        tsMesh.triangles[t++] = i + 2 + vertexCount + (_sides + 1);
                        tsMesh.triangles[t++] = i + 1 + vertexCount + (_sides + 1);
                        tsMesh.triangles[t++] = vertexCount + (_sides + 1);
                    }
                    else
                    {
                        tsMesh.triangles[t++] = vertexCount + (_sides + 1);
                        tsMesh.triangles[t++] = i + 1 + vertexCount + (_sides + 1);
                        tsMesh.triangles[t++] = i + 2 + vertexCount + (_sides + 1);

                    }
                }
            }
        }
    }
}
