using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Spline Positioner")]
    public class SplinePositioner : SplineUser
    {
        public enum Mode { Percent, Distance }
        
        public Transform applyTransform
        {
            get
            {
                if (_applyTransform == null) return this.transform;
                return _applyTransform;
            }

            set
            {
                if(value != _applyTransform)
                {
                    _applyTransform = value;
                    if (value != null) Rebuild(false);
                }
            }
        }
        public double position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value != _position)
                {
                    animPosition = (float)value;
                    _position = value;
                    Rebuild(false);
                }
            }
        }

        public Mode mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (value != _mode)
                {
                    _mode = value;
                    Rebuild(false);
                }
            }
        }

        public SplineResult positionResult
        {
            get
            {
                return _positionResult;
            }
        }

        public Vector2 offset
        {
            get
            {
                return _offset;
            }
            set
            {
                if (value != _offset)
                {
                    _offset = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 rotationOffset
        {
            get
            {
                return _rotationOffset;
            }
            set
            {
                if (value != _rotationOffset)
                {
                    _rotationOffset = value;
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private Transform _applyTransform;
        [SerializeField]
        [HideInInspector]
        private double _position = 0.0;
        [SerializeField]
        [HideInInspector]
        private float animPosition = 0f;
        [SerializeField]
        [HideInInspector]
        private Mode _mode = Mode.Percent;
        [SerializeField]
        [HideInInspector]
        private SplineResult _positionResult;

        [SerializeField]
        [HideInInspector]
        public bool applyPosition = true;
        [SerializeField]
        [HideInInspector]
        public bool applyRotation = true;
        [SerializeField]
        [HideInInspector]
        public bool applyScale = true;
        [SerializeField]
        [HideInInspector]
        public Vector3 baseScale = Vector3.one;

        [SerializeField]
        [HideInInspector]
        public Vector2 _offset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        public Vector3 _rotationOffset = Vector3.zero;

        void Start()
        {
            //Write initialization code here
        }

        protected override void LateRun()
        {
            base.LateRun();
            //Code to run every Update/FixedUpdate/LateUpdate
        }

        protected override void OnDidApplyAnimationProperties()
        {
            if (animPosition != _position) position = animPosition;
            base.OnDidApplyAnimationProperties();
        }

        protected override void Build()
        {
            base.Build();
            //Build is called after the spline has been sampled. 
            //Use it for calculations (example: generate mesh geometry, calculate object positions)
            double percent = _position;
            if (mode == Mode.Distance)
            {
                percent = 1.0;
                double p = clipFrom;
                double prevP = p;
                float distance = 0f;
                while (true)
                {
                    Vector3 prev = EvaluatePosition(p);
                    p = DMath.Move(p, clipTo, _address.root.moveStep);
                    Vector3 current = EvaluatePosition(p);
                    float distAdd = Vector3.Distance(current, prev);
                    distance += distAdd;
                    if (distance >= _position)
                    {
                        percent = DMath.Lerp(prevP, p, Mathf.InverseLerp(distance - distAdd, distance, (float)_position));
                        break;
                    }
                    prevP = p;
                    if (p == clipTo) break;
                }
            } else percent = DMath.Lerp(clipFrom, clipTo, _position);
            _positionResult = Evaluate(percent);
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (_positionResult == null) return;
            if (applyPosition)
            {
                applyTransform.position = positionResult.position;
                if (_offset != Vector2.zero) applyTransform.position += positionResult.right * offset.x + positionResult.normal * offset.y;
            }
            if (applyRotation)
            {
                applyTransform.rotation = positionResult.rotation;
                if (_rotationOffset != Vector3.zero) applyTransform.rotation = applyTransform.rotation * Quaternion.Euler(_rotationOffset);
            }
            if (applyScale) applyTransform.localScale = baseScale * positionResult.size;
        }

    }
}
