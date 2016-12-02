using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace Dreamteck.Splines
{
    public delegate void SplineReachHandler();
    [AddComponentMenu("Dreamteck/Splines/Spline Follower")]
    public class SplineFollower : SplineUser
    {
        public enum FollowMode { Uniform, Time }
        public enum Wrap { Default, Loop, PingPong}
        [HideInInspector]
        public Wrap wrapMode = Wrap.Default;
        [HideInInspector]
        public FollowMode followMode = FollowMode.Uniform;

        [Range(0f, 1f)] //Make the range to be in clipFrom-clipTo
        [HideInInspector]
        public double startPercent = 0.0;
        [HideInInspector]
        public bool findStartPoint = true;
        [HideInInspector]
        public bool applyPosition = true;
        [HideInInspector]
        public bool applyRotation = true;
        [HideInInspector]
        public bool applyDirectionRotation = true;
        [HideInInspector]
        public bool applyScale = false;
        [HideInInspector]
        public SplineTrigger[] triggers = new SplineTrigger[0];
        [HideInInspector]
        public Vector3 baseScale = Vector3.one;

        [HideInInspector]
        public bool autoFollow = true;

        public event SplineReachHandler onEndReached;
        public event SplineReachHandler onBeginningReached;

        /// <summary>
        /// Used when follow mode is set to Uniform. Defines the speed of the follower
        /// </summary>
        public float followSpeed
        {
            get { return _followSpeed; }
            set
            {
                if (_followSpeed != value)
                {
                    if (value < 0f) value = 0f;
                    _followSpeed = value;
                }
            }
        }

        /// <summary>
        /// Used when follow mode is set to Time. Defines how much time it takes for the follower to travel through the path
        /// </summary>
        public float followDuration
        {
            get { return _followDuration; }
            set
            {
                if (_followDuration != value)
                {
                    if (value < 0f) value = 0f;
                    _followDuration = value;
                }
            }
        }


        [SerializeField]
        [HideInInspector]
        private float _followSpeed = 1f;

        [SerializeField]
        [HideInInspector]
        private float _followDuration = 1f;

        private SplineResult _followResult = new SplineResult();
        [HideInInspector]
        public Spline.Direction direction = Spline.Direction.Forward;
        [SerializeField]
        [HideInInspector]
        public Vector2 offset;
        [SerializeField]
        [HideInInspector]
        public Vector3 rotationOffset = Vector3.zero;

        private bool white = false;

        private bool percentSet = false;

        public SplineResult followResult
        {
            get { return _followResult; }
        }

        public SplineResult offsettedFollowResult
        {
            get {
                SplineResult offsetted = new SplineResult(_followResult);
                offsetted.position += offsetted.right * offset.x + offsetted.normal * offset.y;
                offsetted.direction =  Quaternion.Euler(rotationOffset) * offsetted.direction;
                return offsetted;
            }
        }


        // Use this for initialization
        void Start()
        {
            if (autoFollow)
            {
                if (percentSet) return;
                Restart();
            }
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (autoFollow) AutoFollow();
        }

        void AutoFollow()
        {
            switch (followMode)
            {
                case FollowMode.Uniform: Move(Time.deltaTime * _followSpeed); break;
                case FollowMode.Time: 
                    if(_followDuration == 0.0) Move(0.0);
                    else Move((double)(Time.deltaTime / _followDuration)); break;
            }
            
        }

        private void ApplyTransformation()
        {
            if (_followResult == null) return;
            if (applyPosition)
            {
                transform.position = _followResult.position;
                if (offset != Vector2.zero) transform.position += _followResult.right * offset.x + _followResult.normal * offset.y;
            }
            if (applyRotation)
            {
                transform.rotation = Quaternion.LookRotation(applyDirectionRotation ? _followResult.direction * (direction == Spline.Direction.Forward ? 1f : -1f) : _followResult.direction, _followResult.normal);
                if (rotationOffset != Vector3.zero) transform.rotation = transform.rotation * Quaternion.Euler(rotationOffset);
            }
            if (applyScale) transform.localScale = baseScale * _followResult.size;
        }

        /// <summary>
        /// Get the available node in front or behind the follower
        /// </summary>
        /// <returns></returns>
        public Node GetNextNode()
        {
            SplineComputer comp;
            double evaluatePercent = 0.0;
            Spline.Direction dir = Spline.Direction.Forward;
            _address.GetEvaluationValues(_followResult.percent, out comp, out evaluatePercent, out dir);
            if (direction == Spline.Direction.Backward)
            {
                if (dir == Spline.Direction.Forward) dir = Spline.Direction.Backward;
                else dir = Spline.Direction.Forward;
            }
            int[] links = comp.GetAvailableNodeLinksAtPosition(evaluatePercent, dir);
            if (links.Length == 0) return null;
            //Find the closest one
            if (dir == Spline.Direction.Forward)
            {
                int min = comp.pointCount-1;
                int index = 0;
                for (int i = 0; i < links.Length; i++)
                {
                    if (comp.nodeLinks[i].pointIndex < min)
                    {
                        min = comp.nodeLinks[i].pointIndex;
                        index = i;
                    }
                }
                return comp.nodeLinks[index].node;
            }
            else
            {
                int max = 0;
                int index = 0;
                for (int i = 0; i < links.Length; i++)
                {
                    if (comp.nodeLinks[i].pointIndex > max)
                    {
                        max = comp.nodeLinks[i].pointIndex;
                        index = i;
                    }
                }
                return comp.nodeLinks[index].node;
            }
        }

        /// <summary>
        /// Get the current computer the follower is on at the moment
        /// </summary>
        /// <returns></returns>
        public void GetCurrentComputer(out SplineComputer comp, out double percent, out Spline.Direction dir)
        {
            _address.GetEvaluationValues(_followResult.percent, out comp, out percent, out dir);
        }

        public override void EnterAddress(Node node, int pointIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            int element = _address.GetElementIndex(_followResult.percent);
            double localPercent = _address.PathToLocalPercent(_followResult.percent, element);
            base.EnterAddress(node, pointIndex, direction);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            SetPercent(newPercent);
            percentSet = false;
        }

        public override void ExitAddress(int depth)
        {
            int element = _address.GetElementIndex(_followResult.percent);
            double localPercent = _address.PathToLocalPercent(_followResult.percent, element);
            base.ExitAddress(depth);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            SetPercent(newPercent);
            percentSet = false;
        }

        public void Restart()
        {
            if (findStartPoint) SetPercent(Project(this.transform.position, clipFrom, clipTo).percent);
            else SetPercent(startPercent);
            percentSet = false;
        }

        public void SetPercent(double percent)
        {
            percentSet = true;
            percent = DMath.Clamp01(percent);
            _followResult = Evaluate(percent);
            ApplyTransformation();
        }

        public void SetDistance(float distance)
        {
            _followResult = Evaluate(0.0);
            Spline.Direction dir = direction;
            direction = Spline.Direction.Forward;
            Move(distance);
            direction = dir;
        }

        private void CheckTriggers(double prevPercent, double curPercent)
        {
            for(int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null) continue;
                if(clipFrom <= triggers[i].position && clipTo >= triggers[i].position) triggers[i].Check(prevPercent, curPercent);
            }
        }

        public void Move(double percent)
        {
			if(percent == 0.0) return;
            _followResult = Evaluate(followResult.percent);
            double startPercent = followResult.percent;
            double p = startPercent + (direction == Spline.Direction.Forward ? percent : -percent);
            double trace1 = DMath.Clamp01(p);
            if(p > 1.0)
            {
                switch (wrapMode)
                {
                    case Wrap.Default:
                        if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) onEndReached(); 
                        p = trace1; 
                        CheckTriggers(startPercent, p); 
                        break;
                    case Wrap.Loop:
                        if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) onEndReached();
                        while (p > 1.0) p -= 1.0;
                        CheckTriggers(0.0, p);
                        break;
                    case Wrap.PingPong:
                        if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) onEndReached();
                        p = DMath.Clamp01(1.0-(p-1.0));
                        direction = Spline.Direction.Backward;
                        CheckTriggers(1.0, p);
                        break;
                }
            } else if(p < 0.0)
            {
                switch (wrapMode)
                {
                    case Wrap.Default:
                        if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) onBeginningReached();
                        p = trace1; 
                        CheckTriggers(startPercent, p); 
                        break;
                    case Wrap.Loop:
                        if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) onBeginningReached();
                        while (p < 0.0) p += 1.0;
                        CheckTriggers(1.0, p);
                        break;
                    case Wrap.PingPong:
                        if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)trace1)) onBeginningReached();
                        p = DMath.Clamp01(-p);
                        direction = Spline.Direction.Forward;
                        CheckTriggers(0.0, p);
                        break;
                }
            } else CheckTriggers(startPercent, p);
            _followResult = Evaluate(p);
            ApplyTransformation();
        }

        public void Move(float distance)
        {
            if (distance < 0f) distance = 0f;
			if(distance == 0f) return;
            _followResult = Evaluate(followResult.percent);
            SplineResult lastResult = _followResult;
            double tracePercent = _followResult.percent;
            SplineResult traceResult = _followResult;
            double traceTriggersFrom = traceResult.percent;
            float moved = 0f;
            while (moved < distance)
            {
                int resultIndex = GetSampleIndex(traceResult.percent);
                if (direction == Spline.Direction.Forward)
                {
                    if (tracePercent == clipTo)
                    {
                        CheckTriggers((float)traceTriggersFrom, traceResult.percent);
                        if (wrapMode == Wrap.Default)
                        {
                            if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipTo)) onEndReached();
                            _followResult = traceResult;
                            break;
                        }
                        if (wrapMode == Wrap.Loop)
                        {
                            if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipTo)) onEndReached();
                            traceResult = Evaluate(clipFrom);
                            traceTriggersFrom = traceResult.percent;
                            resultIndex = GetSampleIndex(clipFrom);
                        }
                        if (wrapMode == Wrap.PingPong)
                        {
                            if (onEndReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipTo)) onEndReached();
                            direction = Spline.Direction.Backward;
                            lastResult = traceResult;
                            traceTriggersFrom = traceResult.percent;
                            continue;
                        }
                    }
                    double nextPercent = (double)(resultIndex + 1) / (samples.Length - 1);
                    if (nextPercent <= tracePercent) nextPercent = (double)(resultIndex + 2) / (samples.Length - 1);
                    if (nextPercent > clipTo) nextPercent = clipTo;
                    tracePercent = nextPercent;
                }
                else
                {
                    if (tracePercent == clipFrom)
                    {
                        CheckTriggers((float)traceTriggersFrom, traceResult.percent);
                        if (wrapMode == Wrap.Default)
                        {
                            if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipFrom)) onBeginningReached();
                            _followResult = traceResult;
                            break;
                        }
                        if (wrapMode == Wrap.Loop)
                        {
                            if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipFrom)) onBeginningReached();
                            traceResult = Evaluate(clipTo);
                            traceTriggersFrom = traceResult.percent;
                            resultIndex = GetSampleIndex(clipTo);
                        }
                        if (wrapMode == Wrap.PingPong)
                        {
                            if (onBeginningReached != null && !Mathf.Approximately((float)_followResult.percent, (float)clipFrom)) onBeginningReached();
                            direction = Spline.Direction.Forward;
                            lastResult = traceResult;
                            traceTriggersFrom = traceResult.percent;
                            continue;
                        }
                    }
                    double nextPercent = (double)resultIndex / (samples.Length - 1);
                    if (nextPercent >= tracePercent) nextPercent = (double)(resultIndex - 1) / (samples.Length - 1);
                    if (nextPercent < clipFrom) nextPercent = clipFrom;
                    tracePercent = nextPercent;
                }
                lastResult = traceResult;
                traceResult = Evaluate(tracePercent);
                float traveled = (traceResult.position - lastResult.position).magnitude;
                moved += traveled;
                if (moved >= distance)
                {
                    float excess = moved - distance;
                    double lerpPercent = 1.0 - excess / traveled;
                    if (direction == Spline.Direction.Backward && !averageResultVectors)
                    {
                        traceResult.direction = samples[Mathf.Max(resultIndex - 1, 0)].direction;
                        float directionLerp = (float)lastResult.percent * (samples.Length - 1)-resultIndex;
                        lastResult.direction = Vector3.Slerp(samples[resultIndex].direction, traceResult.direction, 1f - directionLerp);
                    }
                    _followResult = SplineResult.Lerp(lastResult, traceResult, lerpPercent);
                    CheckTriggers((float)traceTriggersFrom, _followResult.percent);
                    break;
                }
            }
            ApplyTransformation();
            white = !white;
        }

        private void AddTrigger(SplineTrigger trigger)
        {
            SplineTrigger[] newTriggers = new SplineTrigger[triggers.Length + 1];
            triggers.CopyTo(newTriggers, 0);
            newTriggers[newTriggers.Length - 1] = trigger;
            triggers = newTriggers;
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction call, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<int> call, int value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<float> call, float value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<double> call, double value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<string> call, string value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<bool> call, bool value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<GameObject> call, GameObject value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(SplineTrigger.Type t, UnityAction<Transform> call, Transform value, double position = 0.0, SplineTrigger.Type type = SplineTrigger.Type.Double)
        {
            SplineTrigger trigger = ScriptableObject.CreateInstance<SplineTrigger>();
            trigger.Create(t, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

    }
}
