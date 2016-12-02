﻿using UnityEngine;
using System.Collections;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamteck.Splines
{

    public class MeshGenerator : SplineUser
    {
        public float size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    _size = value;
                    Rebuild(false);
                } else _size = value;
            }
        }

        public Color color
        {
            get { return _color; }
            set
            {
                if (value != _color)
                {
                    _color = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 offset
        {
            get { return _offset; }
            set
            {
                if (value != _offset)
                {
                    _offset = value;
                    Rebuild(false);
                }
            }
        }

        public int normalMethod
        {
            get { return _normalMethod; }
            set
            {
                if (value != _normalMethod)
                {
                    _normalMethod = value;
                    Rebuild(false);
                }
            }
        }

        public bool calculateTangents
        {
            get { return _tangents; }
            set
            {
                if (value != _tangents)
                {
                    _tangents = value;
                    Rebuild(false);
                }
            }
        }

        public float rotation
        {
            get { return _rotation; }
            set
            {
                if (value != _rotation)
                {
                    _rotation = value;
                    Rebuild(false);
                }
            }
        }

        public bool flipFaces
        {
            get { return _flipFaces; }
            set
            {
                if (value != _flipFaces)
                {
                    _flipFaces = value;
                    Rebuild(false);
                }
            }
        }

        public bool doubleSided
        {
            get { return _doubleSided; }
            set
            {
                if (value != _doubleSided)
                {
                    _doubleSided = value;
                    Rebuild(false);
                }
            }
        }

        public UVMode uvMode
        {
            get { return _uvMode; }
            set
            {
                if (value != _uvMode)
                {
                    _uvMode = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 uvScale
        {
            get { return _uvScale; }
            set
            {
                if (value != _uvScale)
                {
                    _uvScale = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 uvOffset
        {
            get { return _uvOffset; }
            set
            {
                if (value != _uvOffset)
                {
                    _uvOffset = value;
                    Rebuild(false);
                }
            }
        }

        public bool baked
        {
            get
            {
                return _baked;
            }
        }


        public enum UVMode { Clip, UniformClip, Clamp, UniformClamp }
        [SerializeField]
        [HideInInspector]
        private bool _baked = false;
        [SerializeField]
        [HideInInspector]
        private float _size = 1f;
        [SerializeField]
        [HideInInspector]
        private Color _color = Color.white;
        [SerializeField]
        [HideInInspector]
        private Vector3 _offset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private int _normalMethod = 1;
        [SerializeField]
        [HideInInspector]
        private bool _tangents = true;
        [SerializeField]
        [HideInInspector]
        private float _rotation = 0f;
        [SerializeField]
        [HideInInspector]
        private bool _flipFaces = false;
        [SerializeField]
        [HideInInspector]
        private bool _doubleSided = false;
        [SerializeField]
        [HideInInspector]
        private UVMode _uvMode = UVMode.Clip;
        [SerializeField]
        [HideInInspector]
        private Vector2 _uvScale = Vector3.one;
        [SerializeField]
        [HideInInspector]
        private Vector2 _uvOffset = Vector2.zero;
        [SerializeField]
        [HideInInspector]
        protected MeshCollider meshCollider;
        [SerializeField]
        [HideInInspector]
        protected MeshFilter filter;
        [SerializeField]
        [HideInInspector]
        protected MeshRenderer meshRenderer;
        [SerializeField]
        [HideInInspector]
        protected TS_Mesh tsMesh = new TS_Mesh();
        [SerializeField]
        [HideInInspector]
        protected Mesh mesh;
        [HideInInspector]
        public float colliderUpdateRate = 0.2f;
        [SerializeField]
        [HideInInspector]
        public bool optimize = false;
        protected bool updateCollider = false;
        protected float lastUpdateTime = 0f;

        private float splineLength = 0f;

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            base.EditorAwake();
            //Make copies of the meshes.
            if (tsMesh != null) tsMesh = TS_Mesh.Copy(tsMesh);
            else tsMesh = new TS_Mesh();
            if (mesh != null) mesh = (Mesh)Instantiate(mesh);
            else mesh = new Mesh();
            Awake();
        }

        public void Bake(bool makeStatic, bool lightmapUV)
        {
            if (mesh == null) return;
            this.gameObject.isStatic = false;
            computer.Unsubscribe(this);
            filter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            filter.hideFlags = meshRenderer.hideFlags = HideFlags.None;
            filter.sharedMesh = mesh;
            filter.sharedMesh.Optimize();
            if (lightmapUV) Unwrapping.GenerateSecondaryUVSet(filter.sharedMesh);
            if (makeStatic) this.gameObject.isStatic = true; 
            _baked = true;
        }

        public void Unbake()
        {
            this.gameObject.isStatic = false; 
            _baked = false;
            computer.Subscribe(this);
            Rebuild(false);
        }
#endif



        protected override void Awake()
        {
            if (mesh == null) mesh = new Mesh();
            base.Awake();
            filter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }


        protected override void Reset()
        {
            base.Reset();
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null) filter.hideFlags = HideFlags.HideInInspector;
            MeshRenderer rend = GetComponent<MeshRenderer>();
            if (rend != null) rend.hideFlags = HideFlags.None;
        }

        public override void Rebuild(bool sampleComputer)
        {
            if (_baked) return;
            base.Rebuild(sampleComputer);
        }

        public override void RebuildImmediate(bool sampleComputer)
        {
            if (_baked) return;
            base.RebuildImmediate(sampleComputer);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MeshFilter filter = GetComponent<MeshFilter>();
            MeshRenderer rend = GetComponent<MeshRenderer>();
            if (filter != null)  filter.hideFlags = HideFlags.None;
            if (rend != null)  rend.hideFlags = HideFlags.None;
        }


        public void UpdateCollider()
        {
            meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null) meshCollider = this.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
        }

        protected override void LateRun()
        {
            if (_baked) return;
            base.LateRun();
            if (updateCollider)
            {
                if (meshCollider != null)
                {
                    if (Time.time - lastUpdateTime >= colliderUpdateRate)
                    {
                        lastUpdateTime = Time.time;
                        updateCollider = false;
                        meshCollider.sharedMesh = filter.sharedMesh;
                    }
                }
            }
        }

        protected override void Build()
        {
            base.Build();
            if (samples.Length > 0) BuildMesh();
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            WriteMesh();
        }

        protected virtual void BuildMesh()
        {
            //Logic for mesh generation, automatically called in the Build method
        }

        protected virtual void WriteMesh() 
        {
            MeshUtility.InverseTransformMesh(tsMesh, this.transform);
            if (doubleSided) MeshUtility.MakeDoublesided(tsMesh);
            MeshUtility.CalculateTangents(tsMesh);
            tsMesh.WriteMesh(ref mesh);
            if (_normalMethod == 0) mesh.RecalculateNormals();
            if (optimize) mesh.Optimize();
            if (filter != null) filter.sharedMesh = mesh;
            updateCollider = true;
        }

        protected float GetLengthAtClip()
        {
            float length = 0f;
            
            return length;
        }

        protected void BeginUV()
        {
            splineLength = 0f;
            if (_uvMode != UVMode.UniformClip) return;
            int i = 1;
            while (i < samples.Length)
            {
                if (samples[i].percent >= clippedSamples[0].percent)
                {
                    splineLength += Vector3.Distance(clippedSamples[0].position, samples[i - 1].position);
                    break;
                }
                else splineLength += Vector3.Distance(samples[i].position, samples[i - 1].position);
                i++;
            }
        }

        protected void AddUVLength(int sampleIndex)
        {
            if (sampleIndex == 0) return;
            splineLength += Vector3.Distance(clippedSamples[sampleIndex].position, clippedSamples[sampleIndex - 1].position);
        }

        protected Vector2 GetUV(float xPercent, float yPercent)
        {
            switch (uvMode)
            {
                case UVMode.Clip: return new Vector2(xPercent * uvScale.x, yPercent * uvScale.y) - uvOffset;
                case UVMode.Clamp: return new Vector2(xPercent * uvScale.x, (float)(yPercent - clipFrom) / ((float)span) * uvScale.y) - uvOffset; 
                case UVMode.UniformClip: return new Vector2(xPercent * uvScale.x, splineLength * uvScale.y) - uvOffset;
                case UVMode.UniformClamp: return new Vector2(xPercent * uvScale.x, splineLength * uvScale.y) - uvOffset;
            }
            return Vector2.zero;
        }
    }

  
}