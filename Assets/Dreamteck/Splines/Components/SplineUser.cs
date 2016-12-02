using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamteck.Splines {
    //SplineUser _samples SplineComputer and supports multithreading.
    public class SplineUser : MonoBehaviour {
        public enum UpdateMethod { Update, FixedUpdate, LateUpdate }
        [SerializeField]
        [HideInInspector]
        private SplineUser[] subscribers = new SplineUser[0];
        [HideInInspector]
        public UpdateMethod updateMethod = UpdateMethod.Update;
        [HideInInspector]
        [SerializeField]
        private SplineUser _user = null;
        public SplineUser user
        {
            get
            {
                return _user;
            }
            set
            {
                if (Application.isPlaying && value != null && value.rootUser == this) return;
                if(value != _user)
                {
                    if (value != null && computer != null)
                    {
                        computer.Unsubscribe(this);
                        computer = null;
                    }
                    if (_user != null) _user.Unsubscribe(this);
                    _user = value;
                    if (_user != null)
                    {
                        _user.Subscribe(this);
                        sampleUser = true;
                    }
                    if (computer == null)
                    {
                        _samples = new SplineResult[0];
                        _clippedSamples = new SplineResult[0];
                    }
                    Rebuild(false);
                }
            }
        }
        public SplineUser rootUser
        {
            get
            {
                SplineUser root = _user;
                while (root != null)
                {
                    if (root._user == null) break;
                    root = root._user;
                    if (root == this) break;
                }
                return root;
            }
        }

        public SplineComputer computer
        {
            get {
                if (_address == null) _address = new SplineAddress((SplineComputer)null);
                return address.root;
            }
            set
            {
                if (_address == null)
                {
                    _address = new SplineAddress(value);
                    value.Subscribe(this);
                    if(value != null) RebuildImmediate(true);
                    return;
                }
                if (value != _address.root)
                {
                    if (value != null && sampleUser)
                    {
                        _user.Unsubscribe(this);
                        _user = null;
                    }
                    if (_address.root != null) _address.root.Unsubscribe(this);
                    _address.root = value;
                    if (value != null)
                    {
                        value.Subscribe(this);
                        sampleUser = false;
                    }
                    if (_address.root != null) RebuildImmediate(true);
                }
            }
        }

        public double resolution
        {
            get
            {
                return _resolution;
            }
            set
            {
                if (value != _resolution)
                {
                    animResolution = (float)_resolution;
                    _resolution = value;
                    if (sampleUser) return;
                    Rebuild(true);
                }
            }
        }

        public double clipFrom
        {
            get
            {
               return _clipFrom; 
            }
            set
            {
                if (value != _clipFrom)
                {
                    animClipFrom = (float)_clipFrom;
                    _clipFrom = DMath.Clamp01(value);
                    if (_clipFrom > _clipTo) _clipTo = _clipFrom;
                    getClippedSamples = true;
                    Rebuild(false);
                }
            }
        }

        public double clipTo
        {
            get
            {
                return _clipTo;
            }
            set
            {
                
                if (value != _clipTo)
                {
                    animClipTo = (float)_clipTo;
                    _clipTo = DMath.Clamp01(value);
                    if (_clipTo < _clipFrom) _clipFrom = _clipTo;
                    getClippedSamples = true;
                    Rebuild(false);
                }
            }
        }


        public bool averageResultVectors
        {
            get
            {
                return _averageResultVectors; 
            }
            set
            {
                if (value != _averageResultVectors)
                {
                    _averageResultVectors = value;
                    if (sampleUser) return;
                    Rebuild(true);
                }
            }
        }

        //The percent of the spline that we're traversing
        public double span
        {
            get
            {
                return _clipTo - _clipFrom;
            }
        }

        public SplineAddress address
        {
            get
            {
                if (_address == null) _address = new SplineAddress((SplineComputer)null);
                return _address;
            }
        }
        [SerializeField]
        [HideInInspector]
        public SplineAddress _address = null;
        //Serialized values
        [SerializeField]
        [HideInInspector]
        private double _resolution = 1f;
        [SerializeField]
        [HideInInspector]
        private double _clipTo = 1f;
        [SerializeField]
        [HideInInspector]
        private double _clipFrom = 0f;
        [SerializeField]
        [HideInInspector]
        private bool _averageResultVectors = true;
        [SerializeField]
        [HideInInspector]
        private SplineResult[] _samples = new SplineResult[0];
        public SplineResult[] samples
        {
            get
            {
                if(sampleUser) return _user.samples;
                else return _samples;
            }
        }
        [SerializeField]
        [HideInInspector]
        protected SplineResult[] _clippedSamples = new SplineResult[0];
        public SplineResult[] clippedSamples
        {
            get
            {
                return _clippedSamples;
            }
        }

        //float values used for making animations
        [SerializeField]
        [HideInInspector]
        private float animClipFrom = 0f;
        [SerializeField]
        [HideInInspector]
        private float animClipTo = 1f;
        [SerializeField]
        [HideInInspector]
        private double animResolution = 1.0;
        [SerializeField]
        [HideInInspector]
        protected bool sampleUser = false;

        private bool rebuild = false;
        private bool sample = false;
        private volatile bool getClippedSamples = false;

        protected bool willRebuild
        {
            get
            {
                return rebuild;
            }
        }

        //Threading values
        [HideInInspector]
        public volatile bool multithreaded = false;
        [HideInInspector]
        public bool buildOnAwake = false;
        private Thread buildThread = null;
        private volatile bool postThread = false;
        private volatile bool threadSample = false;
        private volatile bool threadWork = false;
        private bool _threadWorking = false;
        public bool threadWorking
        {
            get { return _threadWorking; }
        }
        protected object locker = new object();

#if UNITY_EDITOR
        /// <summary>
        /// USE THIS ONLY IN A COMPILER DIRECTIVE REQUIRING UNITY_EDITOR!!!
        /// </summary>
        protected bool isPlaying = false;
#endif


#if UNITY_EDITOR
        /// <summary>
        /// Used by the custom editor. DO NO CALL THIS METHOD IN YOUR RUNTIME CODE
        /// </summary>
        public virtual void EditorAwake()
        {
            //Create a new instance of the address. Otherwise it would be a reference
            _address = new SplineAddress(_address);   
            if (sampleUser)
            {
                if (!user.IsSubscribed(this)) user.Subscribe(this);
            }
            else
            {
                if (computer == null) computer = GetComponent<SplineComputer>();
                else if (!computer.IsSubscribed(this)) computer.Subscribe(this);
            }
            RebuildImmediate(true);
        }
#endif

        protected virtual void Awake() {
#if UNITY_EDITOR
            isPlaying = true;
#endif
            if (sampleUser)
            {
                if (!user.IsSubscribed(this)) user.Subscribe(this);
            }
            else
            {
                if (computer == null) computer = GetComponent<SplineComputer>();
                else if (!computer.IsSubscribed(this)) computer.Subscribe(this);
            }
            if (buildOnAwake) RebuildImmediate(true);
        }

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            EditorAwake();
#endif
        }

        protected virtual void OnEnable()
        {
            if (computer != null)  computer.Subscribe(this);
        }

        protected virtual void OnDisable()
        {
            if (computer != null) computer.Unsubscribe(this);
            threadWork = false;
        }

        protected virtual void OnDestroy()
        {
            if (computer != null) computer.Unsubscribe(this);
            if (buildThread != null)
            {
                threadWork = false;
                buildThread.Abort();
                buildThread = null;
                _threadWorking = false;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            if (buildThread != null)
            {
                threadWork = false;
                buildThread.Abort();
                buildThread = null;
                _threadWorking = false;
            }
        }

        protected virtual void OnDidApplyAnimationProperties()
        {
            bool clip = false;
            if (_clipFrom != animClipFrom || _clipTo != animClipTo) clip = true;
            bool resample = false;
            if (_resolution != animResolution) resample = true;
            _clipFrom = animClipFrom;
            _clipTo = animClipTo;
            _resolution = animResolution;
            Rebuild(resample);
            if (!resample && clip) GetClippedSamples();
        }

        /// <summary>
        /// Rebuild the SplineUser. This will cause Build and Build_MT to be called.
        /// </summary>
        /// <param name="sampleComputer">Should the SplineUser sample the SplineComputer</param>
        public virtual void Rebuild(bool sampleComputer)
        {
            if (sampleUser)
            {
                sampleComputer = false;
                getClippedSamples = true;
            }
#if UNITY_EDITOR
            //If it's the editor and it's not playing, then rebuild immediate
            if (Application.isPlaying)
            {
                rebuild = true;
                if (sampleComputer)
                {
                    sample = true;
                    if (threadWorking) StartCoroutine(UpdateSubscribersRoutine());
                }
            } else RebuildImmediate(sampleComputer);
#else
             rebuild = true;
             if (sampleComputer)
             {
                sample = true;
                if (threadWorking) StartCoroutine(UpdateSubscribersRoutine());
             }
#endif
        }

        IEnumerator UpdateSubscribersRoutine()
        {
            while (rebuild) yield return null;
            UpdateSubscribers();
        }

        /// <summary>
        /// Rebuild the SplineUser immediate. This method will call sample samples and call Build as soon as it's called even if the component is disabled.
        /// </summary>
        /// <param name="sampleComputer">Should the SplineUser sample the SplineComputer</param>
        public virtual void RebuildImmediate(bool sampleComputer)
        {
            if (sampleUser)
            {
                sampleComputer = false;
                GetClippedSamples();
            }
#if UNITY_EDITOR
            if (PrefabUtility.GetPrefabType(this.gameObject) == PrefabType.Prefab) return;
#endif
            if (threadWork)
            {
                if(sampleComputer) threadSample = true;
                buildThread.Interrupt();
                StartCoroutine(UpdateSubscribersRoutine());
            }
            else
            {
                if (sampleComputer) SampleComputer();
                else if (getClippedSamples) GetClippedSamples();
                UpdateSubscribers();
                Build();
                PostBuild();
            }
            rebuild = false;
            sampleComputer = false;
            getClippedSamples = false;
        }

        /// <summary>
        /// Enter a junction address.
        /// </summary>
        /// <param name="element">The address element to add to the address</param>
        public virtual void EnterAddress(Node node, int pointIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            address.Enter(node, pointIndex, direction);
            if (_address.depth != lastDepth) Rebuild(true);
        }

        /// <summary>
        /// Clear the junction address.
        /// </summary>
        public virtual void ClearAddress()
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            _address.Clear();
            if (_address.depth != lastDepth) RebuildImmediate(true);
        }

        /// <summary>
        /// Exit junction address.
        /// </summary>
        /// <param name="depth">How many address elements to exit</param>
        public virtual void ExitAddress(int depth)
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            _address.Exit(depth);
            if (_address.depth != lastDepth) Rebuild(true);
        }

        private void Update()
        {
            if (updateMethod == UpdateMethod.Update) RunMain();
        }

        private void LateUpdate()
        {
            if (updateMethod == UpdateMethod.LateUpdate) RunMain();
        }

        private void FixedUpdate()
        {
            if (updateMethod == UpdateMethod.FixedUpdate) RunMain();
        }

        void UpdateSubscribers()
        {
            for (int i = subscribers.Length - 1; i >= 0; i--)
            {
                if (subscribers[i] == null) RemoveSubscriber(i);
                else subscribers[i].RebuildImmediate(false);
            }
        }

        //Update logic for handling threads and rebuilding
        private void RunMain()
        {
            Run();
            //Handle threading
#if UNITY_EDITOR
            if (multithreaded) threadWork = Application.isPlaying && System.Environment.ProcessorCount > 1;
            else threadWork = postThread = false;
#else
            if (multithreaded) threadWork = System.Environment.ProcessorCount > 1; //Don't check Application.isplaying if it's not the UnityEditor
            else threadWork = postThread = false;
#endif
            //Handle multithreading
            if (threadWork)
            {
                if (postThread)
                {
                    lock (locker)
                    {
                        PostBuild();
                    }
                    postThread = false;
                }
                if (buildThread == null)
                {
                    buildThread = new Thread(RunThread);
                    buildThread.Start();
                } else if (!buildThread.IsAlive)
                {
                    Debug.Log("Thread died - unknown error");
                    buildThread = new Thread(RunThread);
                    buildThread.Start();
                }
            }
            else if (_threadWorking)
            {
                buildThread.Abort();
                buildThread = null;
                _threadWorking = false;
            }

            //Handle rebuilding
            if (rebuild && this.enabled)
            {
                if (threadWorking)
                {
                    threadSample = sample;
                    buildThread.Interrupt();
                    sample = false;
                }
                else
                {
                    if (sample)
                    {
                        SampleComputer();
                        sample = false;
                        UpdateSubscribers();
                    }
                    else if (getClippedSamples)
                    {
                        GetClippedSamples();
                        UpdateSubscribers();
                    }
                    Build();
                    PostBuild();
                }
                rebuild = false;
            }
            LateRun();
        }

        //Update logic for threads.
        private void RunThread()
        {
            lock (locker)
            {
                _threadWorking = true;
            }
            while (true)
            {
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                    lock (locker)
                    {
                        if (threadSample)
                        {
                            SampleComputer();
                            threadSample = false;
                        } else if (getClippedSamples) GetClippedSamples();
                        Build();
                        postThread = true;
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
            }
        }

        /// Code to run every Update/FixedUpdate/LateUpdate before any building has taken place
        protected virtual void Run()
        {

        }

        /// Code to run every Update/FixedUpdate/LateUpdate after any rabuilding has taken place
        protected virtual void LateRun()
        {

        }

        //Used for calculations. Called on the main or the worker thread.
        protected virtual void Build()
        {
        }

        //Called on the Main thread only - used for applying the results from Build
        protected virtual void PostBuild()
        {

        }

        //Sample the computer
        private void SampleComputer()
        {
            if (computer == null) return;
            if (computer.pointCount < 1) return;
            double moveStep = _address.moveStep / _resolution;
            int fullIterations = DMath.CeilInt(1.0 / moveStep) + 1;
            double _span = span;
            if (_span != span) fullIterations = DMath.CeilInt(_span / moveStep) + 1;
            if(_samples.Length != fullIterations) _samples = new SplineResult[fullIterations];
            for(int i = 0; i < fullIterations; i ++)
            {
                double eval = (double)i/(fullIterations-1);
                if (computer.isClosed && i == fullIterations - 1) eval = 0.0;
                SplineResult result = _address.Evaluate(eval);
                result.percent = eval;
                _samples[i] = result;
            }
            if (_samples.Length == 0)
            {
                _clippedSamples = new SplineResult[0];
                return;
            }
            if (_samples.Length > 1)
            {
                if (_averageResultVectors)
                {
                    //Average directions
                    Vector3 lastDir = _samples[1].position - _samples[0].position;
                    for (int i = 0; i < _samples.Length - 1; i++)
                    {
                        Vector3 dir = (_samples[i + 1].position - _samples[i].position).normalized;
                        _samples[i].direction = (lastDir + dir).normalized;
                        _samples[i].normal = (_samples[i].normal + _samples[i + 1].normal).normalized;
                        lastDir = dir;
                    }

                    if (computer.isClosed) _samples[_samples.Length - 1].direction = _samples[0].direction = Vector3.Slerp(_samples[0].direction, lastDir, 0.5f);
                    else _samples[_samples.Length - 1].direction = lastDir;
                }
            }
            if (computer.isClosed && _span == 1f)
            {
                //Handle closed splines
                _samples[_samples.Length - 1] = new SplineResult(_samples[0]);
                _samples[_samples.Length - 1].percent = clipTo;
            }
            GetClippedSamples();
        }

        /// <summary>
        /// Gets the clipped _samples defined by clipFrom and clipTo
        /// </summary>
        private void GetClippedSamples()
        {
            double clipFromValue = clipFrom * (samples.Length - 1);
            double clipToValue = clipTo * (samples.Length - 1);
            int clippedIterations = DMath.CeilInt(clipToValue) - DMath.FloorInt(clipFromValue) + 1;
            if (span == 1.0)
            {
                //if (_clippedSamples.Length != samples.Length) _clippedSamples = new SplineResult[samples.Length];
                //samples.CopyTo(_clippedSamples, 0);
                _clippedSamples = samples;
                return;
            }
            else if(_clippedSamples.Length != clippedIterations) _clippedSamples = new SplineResult[clippedIterations];
            int clipFromIndex = DMath.FloorInt(clipFromValue);
            int clipToIndex = DMath.CeilInt(clipToValue);
            if (clipFromIndex + 1 < samples.Length) _clippedSamples[0] = SplineResult.Lerp(samples[clipFromIndex], samples[clipFromIndex + 1], clipFromValue - clipFromIndex);
            for (int i = 1; i < _clippedSamples.Length - 1; i++)
            {
                _clippedSamples[i] = samples[clipFromIndex + i];
            }
            if (clipToIndex - 1 >= 0) _clippedSamples[_clippedSamples.Length - 1] = SplineResult.Lerp(samples[clipToIndex], samples[clipToIndex - 1], clipToIndex - clipToValue);
            getClippedSamples = false;
        }

        /// <summary>
        /// Evaluate the sampled samples
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public SplineResult Evaluate(double percent)
        {
            if (samples.Length == 0) return null;
            percent = DMath.Clamp01(percent);
            int index = GetSampleIndex(percent);
            double percentExcess = (samples.Length - 1) * percent - index;
            if (percentExcess > 0.0 && index < samples.Length - 1) return SplineResult.Lerp(samples[index], samples[index + 1], percentExcess);
            else return new SplineResult(samples[index]);
        }

        /// <summary>
        /// Evaluate the sampled samples' positions
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public Vector3 EvaluatePosition(double percent)
        {
            if (samples.Length == 0) return Vector3.zero;
            percent = DMath.Clamp01(percent);
            int index = GetSampleIndex(percent);
            double percentExcess = (samples.Length - 1) * percent - index;
            if (percentExcess > 0.0 && index < samples.Length - 1) return Vector3.Lerp(samples[index].position, samples[index + 1].position, (float)percentExcess);
            else return samples[index].position;
        }

        /// <summary>
        /// Evaluate the sampled samples and pass the result to an existing result reference
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public void Evaluate(double percent, ref SplineResult result)
        {
            if (samples.Length == 0) return;
            percent = DMath.Clamp01(percent);
            int index = GetSampleIndex(percent);
            double percentExcess = (samples.Length - 1) * percent - index;
            if (result == null) result = new SplineResult(samples[index]);
            else  result.Absorb(samples[index]);
            if (percentExcess > 0.0 && index < samples.Length - 1) result.Lerp(samples[index + 1], percentExcess);
        }

        /// <summary>
        /// Get the index of the sampled result at percent
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public int GetSampleIndex(double percent)
        {
            return DMath.FloorInt(percent * (samples.Length - 1));
        }

        /// <summary>
        /// Project a point onto the sampled SplineComputer
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <param name="from">Start check from</param>
        /// <param name="to">End check at</param>
        /// <returns></returns>
        public SplineResult Project(Vector3 point, double from = 0.0, double to = 1.0)
        {
            if (samples.Length == 0) return new SplineResult();
            if (computer == null) return new SplineResult();
            //First make a very rough sample of the from-to region 
            int steps = (computer.pointCount - 1) * 6; //Sampling six points per segment is enough to find the closest point range
            int step = samples.Length / steps;
            if (step < 1) step = 1;
            float minDist = (point - samples[0].position).sqrMagnitude;
            int fromIndex = 0;
            int toIndex = samples.Length - 1;
            if (from != 0.0) fromIndex = GetSampleIndex(from);
            if (to != 1.0) toIndex = Mathf.CeilToInt((float)to * (samples.Length - 1));
            int checkFrom = fromIndex;
            int checkTo = toIndex;

            //Find the closest point range which will be checked in detail later
            for (int i = fromIndex; i <= toIndex; i += step)
            {
                if (i > toIndex) i = toIndex;
                float dist = (point - samples[i].position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    checkFrom = Mathf.Max(i - step, 0);
                    checkTo = Mathf.Min(i + step, samples.Length - 1);
                }
                if (i == toIndex) break;
            }
            minDist = (point - samples[checkFrom].position).sqrMagnitude;

            int index = checkFrom;
            //Find the closest result within the range
            for (int i = checkFrom + 1; i <= checkTo; i++)
            {
                float dist = (point - samples[i].position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }
            //Project the point on the line between the two closest samples
            int backIndex = index - 1;
            if (backIndex < 0) backIndex = 0;
            int frontIndex = index + 1;
            if (frontIndex > samples.Length - 1) frontIndex = samples.Length - 1;
            Vector3 back = Dreamteck.Utils.ProjectOnLine(samples[backIndex].position, samples[index].position, point);
            Vector3 front = Dreamteck.Utils.ProjectOnLine(samples[index].position, samples[frontIndex].position, point);
            float backLength = (samples[index].position - samples[backIndex].position).magnitude;
            float frontLength = (samples[index].position - samples[frontIndex].position).magnitude;
            float backProjectDist = (back - samples[backIndex].position).magnitude;
            float frontProjectDist = (front - samples[frontIndex].position).magnitude;
            if (backIndex < index && index < frontIndex)
            {
                if ((point - back).sqrMagnitude < (point - front).sqrMagnitude)  return SplineResult.Lerp(samples[backIndex], samples[index], backProjectDist / backLength);
                else return SplineResult.Lerp(samples[frontIndex], samples[index], frontProjectDist / frontLength);
            } else if (backIndex < index)  return SplineResult.Lerp(samples[backIndex], samples[index], backProjectDist / backLength);
            else return SplineResult.Lerp(samples[frontIndex], samples[index], frontProjectDist / frontLength);
        }

        /// <summary>
        /// Returns the percent from the spline at a given distance from the start point
        /// </summary>
        /// <param name="start">The start point</param>
        /// /// <param name="distance">The distance to travel</param>
        /// <param name="direction">The direction towards which to move</param>
        /// <returns></returns>
        public double Travel(double start, float distance, Spline.Direction direction)
        {
            float moved = 0f;
            float lastMoved = 0f;
            Vector3 lastPoint = EvaluatePosition(start);
            Vector3 currentPoint = lastPoint;
            int step = GetSampleIndex(start);
            while (moved < distance)
            {
                if (direction == Spline.Direction.Forward) step++;
                else step--;
                if (step < 0 || step >= samples.Length) break;
                currentPoint = samples[step].position;
                lastMoved = moved;
                moved += Vector3.Distance(currentPoint, lastPoint);
                lastPoint = currentPoint;
            }
            double distancePercent = DMath.InverseLerp(lastMoved, moved, distance);
            return DMath.Lerp(samples[Mathf.Max(0, step-1)].percent, samples[step].percent, distancePercent);
        }

        //-----------Subscribing logic for users that reference a SplineUser instad of a SplineComputer

        /// <summary>
        /// Subscribe a SplineUser to this User. This will rebuild the user automatically when there are changes.
        /// </summary>
        /// <param name="input">The SplineUser to subscribe</param>
        private void Subscribe(SplineUser input)
        {
            if (input == this) return;
            int emptySlot = -1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == input) return;
                else if (subscribers[i] == null && emptySlot < 0) emptySlot = i;
            }
            if (emptySlot >= 0) subscribers[emptySlot] = input;
            else
            {
                SplineUser[] newSubscribers = new SplineUser[subscribers.Length + 1];
                subscribers.CopyTo(newSubscribers, 0);
                newSubscribers[subscribers.Length] = input;
                subscribers = newSubscribers;
            }
        }

        /// <summary>
        /// Unsubscribe a SplineUser from this computer's updates
        /// </summary>
        /// <param name="input">The SplineUser to unsubscribe</param>
        private void Unsubscribe(SplineUser input)
        {
            int removeSlot = -1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == input)
                {
                    removeSlot = i;
                    break;
                }
            }
            if (removeSlot < 0) return;
            SplineUser[] newSubscribers = new SplineUser[subscribers.Length - 1];
            int index = subscribers.Length - 1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (index == removeSlot) continue;
                else if (i < index) newSubscribers[i] = subscribers[i];
                else newSubscribers[i - 1] = subscribers[i - 1];
            }
            subscribers = newSubscribers;
        }

        private void RemoveSubscriber(int index)
        {
            SplineUser[] newSubscribers = new SplineUser[subscribers.Length - 1];
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (i == index) continue;
                else if (i < index) newSubscribers[i] = subscribers[i];
                else newSubscribers[i - 1] = subscribers[i];
            }
            subscribers = newSubscribers;
        }

        private bool IsSubscribed(SplineUser user)
        {
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == user)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
