using UnityEngine;

//This script handles all physics, collision detection and ground detection;
//It expects a movement velocity (via 'SetVelocity') every 'FixedUpdate' frame from an external script (like a controller script) to work;
//It also provides several getter methods for important information (whether the mover is grounded, the current surface normal [...]);
public class Mover : MonoBehaviour
{
    //Collider variables;
    [Header("Mover Options :")]
    [Range(0f, 1f)]
    public float StepHeightRatio = 0.25f;
    [Header("Collider Options :")]
    [SerializeField]
    public float ColliderHeight = 2f;
    [SerializeField]
    public float ColliderThickness = 1f;
    [SerializeField]
    public Vector3 ColliderOffset = new Vector3(0f, 0.5f, 0f);

    private CapsuleCollider mCapsuleCollider;

    //Sensor variables;
    [Header("Sensor Options :")]
    [SerializeField]
    public Sensor.CastType SensorType = Sensor.CastType.Spherecast;

    private float mSensorRadiusModifier = 0.8f;
    private int mCurrentLayer;

    [SerializeField]
    public bool IsInDebugMode = false;
    [Header("Sensor Array Options :")]
    [SerializeField]
    [Range(1, 5)]
    public int SensorArrayRows = 1;
    [SerializeField]
    [Range(3, 10)]
    public int SensorArrayRayCount = 6;
    [SerializeField]
    public bool SensorArrayRowsAreOffset = false;

    [HideInInspector]
    public Vector3[] RaycastArrayPreviewPositions;

    //Ground detection variables;
    public bool IsGrounded { get; private set; }

    //Sensor range variables;
    private bool mIsUsingExtendedSensorRange = true;
    private float mBaseSensorRange = 0f;

    //Current upwards (or downwards) velocity necessary to keep the correct distance to the ground;
    private Vector3 mCurrentGroundAdjustmentVelocity = Vector3.zero;

    //References to attached components;
    public Rigidbody Rbody { get; private set; }

    private Collider mCollider;    
    private Transform mTransform;
    private Sensor mSensor;

    private bool IsDestory = false;

    public void Initialize(float colliderRadius, float colliderHeight)
    {
        ColliderThickness = colliderRadius * 2f;
        ColliderHeight = colliderHeight;

        Setup();

        //Initialize sensor;
        mSensor = new Sensor(this.mTransform, mCollider);
        RecalculateColliderDimensions();
        RecalibrateSensor();
    }

    public void DestroyMover()
    {
        if (null == this || true == IsDestory)
            return;
        
        if(null != Rbody)
            Rbody.isKinematic = true;

        IsDestory = true;

        GameObject.Destroy(this);
    }
    void Reset()
    {
        Setup();
    }

    void OnValidate()
    {
        //Recalculate collider dimensions;
        if (this.gameObject.activeInHierarchy)
            RecalculateColliderDimensions();

        //Recalculate raycast array preview positions;
        if (SensorType == Sensor.CastType.RaycastArray)
            RaycastArrayPreviewPositions =
                Sensor.GetRaycastStartPositions(SensorArrayRows, SensorArrayRayCount, SensorArrayRowsAreOffset, 1f);
    }

    //Setup references to components;
    void Setup()
    {
        mTransform = transform;
        mCollider = GetComponent<Collider>();

        //If no collider is attached to this gameobject, add a collider;
        if (mCollider == null)
        {
            mTransform.gameObject.AddComponent<CapsuleCollider>();
            mCollider = GetComponent<Collider>();
        }

        Rbody = GetComponent<Rigidbody>();

        //If no rigidbody is attached to this gameobject, add a rigidbody;
        if (Rbody == null)
        {
            mTransform.gameObject.AddComponent<Rigidbody>();
            Rbody = GetComponent<Rigidbody>();
        }

        mCapsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();

        //Freeze rigidbody rotation and disable rigidbody gravity;
        Rbody.freezeRotation = true;
        Rbody.useGravity = false;
        Rbody.isKinematic = ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Temple;
        Rbody.interpolation = RigidbodyInterpolation.Interpolate;        
    }

    //Draw debug information if debug mode is enabled;
    //void LateUpdate()
    //{
    //    if (IsInDebugMode)
    //        mSensor.DrawDebug();
    //}

    //Recalculate collider height/width/thickness;
    public void RecalculateColliderDimensions()
    {
        //Check if a collider is attached to this gameobject;
        if (mCollider == null)
        {
            //Try to get a reference to the attached collider by calling Setup();
            Setup();

            //Check again;
            if (mCollider == null)
            {
                Debug.LogWarning("There is no collider attached to " + this.gameObject.name + "!");
                return;
            }
        }

        if (mCapsuleCollider)
        {
            mCapsuleCollider.height = ColliderHeight;
            mCapsuleCollider.center = ColliderOffset * ColliderHeight;
            mCapsuleCollider.radius = ColliderThickness / 2f;

            mCapsuleCollider.center = mCapsuleCollider.center + new Vector3(0f, StepHeightRatio * mCapsuleCollider.height / 2f, 0f);
            mCapsuleCollider.height *= (1f - StepHeightRatio);

            if (mCapsuleCollider.height / 2f < mCapsuleCollider.radius)
                mCapsuleCollider.radius = mCapsuleCollider.height / 2f;
        }

        //Recalibrate sensor variables to fit new collider dimensions;
        if (mSensor != null)
            RecalibrateSensor();
    }

    //Recalibrate sensor variables;
    void RecalibrateSensor()
    {
        //Set sensor ray origin and direction;
        mSensor.SetCastOrigin(GetColliderCenter());
        mSensor.SetCastDirection(Sensor.CastDirection.Down);

        //Calculate sensor layermask;
        RecalculateSensorLayerMask();

        //Set sensor cast type;
        mSensor.castType = SensorType;

        //Calculate sensor radius/width;
        float _radius = ColliderThickness / 2f * mSensorRadiusModifier;

        //Multiply all sensor lengths with '_safetyDistanceFactor' to compensate for floating point errors;
        float _safetyDistanceFactor = 0.001f;

        //Fit collider height to sensor radius;
        _radius = Mathf.Clamp(_radius, _safetyDistanceFactor, (mCapsuleCollider.height / 2f) * (1f - _safetyDistanceFactor));

        //Set sensor variables;

        //Set sensor radius;
        mSensor.sphereCastRadius = _radius;

        //Calculate and set sensor length;
        float _length = 0f;
        _length += (ColliderHeight * (1f - StepHeightRatio)) * 0.5f;
        _length += ColliderHeight * StepHeightRatio;
        mBaseSensorRange = _length * (1f + _safetyDistanceFactor);
        mSensor.castLength = _length;

        //Set sensor array variables;
        mSensor.ArrayRows = SensorArrayRows;
        mSensor.arrayRayCount = SensorArrayRayCount;
        mSensor.offsetArrayRows = SensorArrayRowsAreOffset;
        mSensor.isInDebugMode = IsInDebugMode;

        //Set sensor spherecast variables;
        mSensor.calculateRealDistance = true;
        mSensor.calculateRealSurfaceNormal = true;

        //Recalibrate sensor to the new values;
        mSensor.RecalibrateRaycastArrayPositions();
    }

    //Recalculate sensor layermask based on current physics settings;
    void RecalculateSensorLayerMask()
    {
        int _layerMask = 0;
        int _objectLayer = this.gameObject.layer;

        //Calculate layermask;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(_objectLayer, i))
                _layerMask = _layerMask | (1 << i);
        }

        //Make sure that the calculated layermask does not include the 'Ignore Raycast' layer;
        if (_layerMask == (_layerMask | (1 << LayerMask.NameToLayer("Ignore Raycast"))))
        {
            _layerMask ^= (1 << LayerMask.NameToLayer("Ignore Raycast"));
        }

        //Set sensor layermask;
        mSensor.layermask = _layerMask;

        //Save current layer;
        mCurrentLayer = _objectLayer;
    }

    //Returns the collider's center in world coordinates;
    Vector3 GetColliderCenter()
    {
        if (mCollider == null)
            Setup();

        return mCollider.bounds.center;
    }

    //Check if mover is grounded;
    //Store all relevant collision information for later;
    //Calculate necessary adjustment velocity to keep the correct distance to the ground;
    void Check()
    {
        //Reset ground adjustment velocity;
        mCurrentGroundAdjustmentVelocity = Vector3.zero;

        //Set sensor length;
        if (mIsUsingExtendedSensorRange)
            mSensor.castLength = mBaseSensorRange + ColliderHeight * StepHeightRatio;
        else
            mSensor.castLength = mBaseSensorRange;

        mSensor.Cast();

        //If sensor has not detected anything, set flags and return;
        if (!mSensor.HasDetectedHit())
        {
            IsGrounded = false;
            return;
        }

        //Set flags for ground detection;
        IsGrounded = true;

        //Get distance that sensor ray reached;
        float _distance = mSensor.GetDistance();

        //Calculate how much mover needs to be moved up or down;
        float _upperLimit = (ColliderHeight * (1f - StepHeightRatio)) * 0.5f;
        float _middle = _upperLimit + ColliderHeight * StepHeightRatio;
        float _distanceToGo = _middle - _distance;

        //Set new ground adjustment velocity for the next frame;
        mCurrentGroundAdjustmentVelocity = mTransform.up * (_distanceToGo / Time.fixedDeltaTime);
    }

    //Check if mover is grounded;
    public void CheckForGround()
    {
        //Check if object layer has been changed since last frame;
        //If so, recalculate sensor layer mask;
        if (mCurrentLayer != this.gameObject.layer)
            RecalculateSensorLayerMask();

        Check();
    }

    //Set mover velocity;
    public void SetVelocity(Vector3 _velocity)
    {
        Rbody.velocity = _velocity + mCurrentGroundAdjustmentVelocity;
    }
    
    //Setters;

    //Set whether sensor range should be extended;
    public void SetExtendSensorRange(bool _isExtended)
    {
        mIsUsingExtendedSensorRange = _isExtended;
    }

    //Set height of collider;
    public void SetColliderHeight(float _newColliderHeight)
    {
        if (ColliderHeight == _newColliderHeight)
            return;

        ColliderHeight = _newColliderHeight;
        RecalculateColliderDimensions();
    }

    //Set acceptable step height;
    public void SetStepHeightRatio(float _newStepHeightRatio)
    {
        _newStepHeightRatio = Mathf.Clamp(_newStepHeightRatio, 0f, 1f);
        StepHeightRatio = _newStepHeightRatio;
        RecalculateColliderDimensions();
    }

    //Getters;

    public Vector3 GetGroundNormal()
    {
        return mSensor.GetNormal();
    }

    public Vector3 GetGroundPoint()
    {
        return mSensor.GetPosition();
    }

    public Collider GetGroundCollider()
    {
        return mSensor.GetCollider();
    }

}