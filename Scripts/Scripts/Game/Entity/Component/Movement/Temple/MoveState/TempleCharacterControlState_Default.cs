using UnityEngine;

using UnityEngine.AI;

/// <summary> 사당용 캐릭터 컨트롤 상태 </summary>
public class TempleCharacterControlState_Default : TempleCharacterControlStateBase
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.Default; } }

    /// <summary> 점프 관련 flag </summary>    
    private bool mJumpKeyWasPressed = false;
    private bool mJumpKeyWasLetGo = false;
    private bool mJumpKeyIsPressed = false;

    /// <summary> 점프 속도 </summary>
    public float JumpSpeed = 10f;
    public float JumpDuration = 0.5f;
    private float mCurrentJumpStartTime = 0f;

    /// <summary> 공중 이동  </summary>
    [Range(0f, 1f)]
    public float AirControl = 1f;

    /// <summary> 공중 마찰 </summary>
    public float AirFriction = 0.5f;
    /// <summary> 지면 마찰 </summary>
    public float GroundFriction = 100f;

    /// <summary> 현재 가속도 </summary>
    protected Vector3 mMomentum = Vector3.zero;

    /// <summary> 마지막 속력 </summary>
    private Vector3 mSavedVelocity = Vector3.zero;

    /// <summary> 마지막 가속도 </summary>
    private Vector3 mSavedMovementVelocity = Vector3.zero;

    /// <summary> 중력 </summary>
    public float Gravity = 20f;

    public float SlideGravity = 5f;

    /// <summary> 추가 가속도를 받고 있을때 발동하는 중력 </summary>
    public float AddMomentumGravity = 2f;

    /// <summary> 추가 가속도  </summary>
    private Vector3? AddMomentum = null;
    /// <summary> 실제 적용되는 추가 가속도 </summary>
    private Vector3 AddMomentumValue = Vector3.zero;
    /// <summary> 추가 가속도를 받고 지난 시간 </summary>
    private float AddMomentumElapsedTime = 0f;
    /// <summary> 추가 가속도를 적요할 시간 </summary>
    private float AddMomentumDuration = 0;

    /// <summary> 경사각 제한 </summary>
    public float SlopeLimit = 45f;

    public E_TempleCharacterState CurrentCharacterState { get; protected set; } = E_TempleCharacterState.Grounded;

    /// <summary> NavAgent - 클릭 이동 및 전투를 위해 추가 </summary>
    private NavMeshAgent mNavAgent = null;

    /// <summary> NavAgent활성화 여부 </summary>
    private bool IsEnableNavAgent { get { return mNavAgent?.enabled ?? false; } }

    public override bool IsPossibleRide { get { return CurrentCharacterState == E_TempleCharacterState.Grounded; } }

    protected override void BeginStateImpl(params object[] args)
    {
        CurrentCharacterState = E_TempleCharacterState.Grounded;
        
        //NavAgent 체크 및 추가.
        if (null == mNavAgent)
        {           
            mNavAgent = mOwner.gameObject.GetOrAddComponent<NavMeshAgent>();            
            mNavAgent.enabled = false;
            mNavAgent.updateRotation = true;
            mNavAgent.angularSpeed = 720;
            mNavAgent.acceleration = 50f;
            mNavAgent.stoppingDistance = 0.1f;
            mNavAgent.autoBraking = true;
            mNavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        }
    }

    protected override void EndStateImpl()
    {
        DisableNavAgent();
        AddMomentum = null;
        AddMomentumValue = Vector3.zero;
        mMomentum = Vector3.zero;
        mDir = Vector3.zero;
        mMoveSpeed = 0f;
    }

    protected override void CancelImpl()
    {
    }

    protected override void LateUpdateStateImpl()
    {
        mOwner.MoveAnim(false == mOwner.IsBlockMove() && (default != mDir || IsEnableNavAgent));
        if(CurrentCharacterState != E_TempleCharacterState.Grounded)
        {
            mOwner.StopCombat();
        }
    }

    #region ===== :: NavMeshAgent 관련 처리 :: =====
    /// <summary> NavMeshAgent를 활성화 한다. </summary>
    private void EnableNavAgent()
    {
        if (null != mNavAgent)
        {
            if (NavMesh.SamplePosition(mOwner.Position, out var hit, 0.2f, NavMesh.AllAreas))
            {

                mNavAgent.enabled = true;
            }
        }   
    }

    /// <summary> NavMeshAgent를 비활성화 한다. </summary>
    private void DisableNavAgent()
    {
        if(null != mNavAgent)
            mNavAgent.enabled = false;
    }
    #endregion

    protected override void FixedUpdateStateImpl()
    {
        //네비메시 이동중이면 패스
        if (true == IsEnableNavAgent)
        {
            return;
        }

        //이동 중지 상태 처리
  //      if (mOwner.IsBlockMove())
		//{
  //          mMover.SetVelocity(Vector3.zero);
  //          return;
  //      }   

        mMover.CheckForGround();

        CurrentCharacterState = DetermineCharacterState();
                
        if (true == IsEnableNavAgent)
        {
            //네비메시 이동중이면 패스
            if (CurrentCharacterState == E_TempleCharacterState.Grounded)
            {
                return;
            }
            else
            {
                DisableNavAgent();
            }
        }

        //Apply friction and gravity to 'momentum';
        HandleMomentum();

        //Check if the player has initiated a jump;
        HandleJumping();

        //Calculate movement velocity;
        Vector3 _velocity = CalculateMovementVelocity();

        //If local momentum is used, transform momentum into world space first;
        Vector3 _worldMomentum = mMomentum + AddMomentumValue;

        //Add current momentum to velocity;
        _velocity += _worldMomentum;

        //If player is grounded or sliding on a slope, extend mover's sensor range;
        //This enables the player to walk up/down stairs and slopes without losing ground contact;
        mMover.SetExtendSensorRange(IsGrounded() && AddMomentumValue == Vector3.zero);

        //Set mover velocity;		
        mMover.SetVelocity(_velocity);

        //Store velocity for next frame;
        mSavedVelocity = _velocity;
        mSavedMovementVelocity = _velocity - _worldMomentum;

        //Reset jump key booleans;
        mJumpKeyWasLetGo = false;
        mJumpKeyWasPressed = false;

        if (default != mDir)
        {
            Quaternion lerpRot = Quaternion.LookRotation(mDir);
            CachedTransform.rotation = Quaternion.Lerp(Rotation, lerpRot, 9f * Time.smoothDeltaTime);
        }
    }

    //Check if player has initiated a jump;
    void HandleJumping()
    {
        if (CurrentCharacterState == E_TempleCharacterState.Grounded)
        {
            if (mJumpKeyIsPressed == true || mJumpKeyWasPressed)
            {
                //Call events;
                OnGroundContactLost();
                OnJumpStart();

                CurrentCharacterState = E_TempleCharacterState.Jumping;
            }
        }
    }

    //Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
    //Handle sliding down steep slopes;
    void HandleMomentum()
    {
        Vector3 _verticalMomentum = Vector3.zero;
        Vector3 _horizontalMomentum = Vector3.zero;

        //Split momentum into vertical and horizontal components;
        if (mMomentum != Vector3.zero)
        {
            _verticalMomentum = VectorHelper.ExtractDotVector(mMomentum, CachedTransform.up);            
            _horizontalMomentum = mMomentum - _verticalMomentum;
        }

        float gravity = null == AddMomentum ? Gravity : AddMomentumGravity;
        //Add gravity to vertical momentum;
        _verticalMomentum -= CachedTransform.up * gravity * Time.deltaTime;

        //Remove any downward force if the controller is grounded;
        if (CurrentCharacterState == E_TempleCharacterState.Grounded)
            _verticalMomentum = Vector3.zero;

        //Apply friction to horizontal momentum based on whether the controller is grounded;
        if (CurrentCharacterState == E_TempleCharacterState.Grounded)
            _horizontalMomentum = VectorHelper.IncrementVectorLengthTowardTargetLength(_horizontalMomentum, GroundFriction, Time.deltaTime, 0f);
        else
            _horizontalMomentum = VectorHelper.IncrementVectorLengthTowardTargetLength(_horizontalMomentum, AirFriction, Time.deltaTime, 0f);

        //Add horizontal and vertical momentum back together;
        mMomentum = _horizontalMomentum + _verticalMomentum;

        //Project the current momentum onto the current ground normal if the controller is sliding down a slope;
        if (CurrentCharacterState == E_TempleCharacterState.Sliding)
        {
            mMomentum = Vector3.ProjectOnPlane(mMomentum, mMover.GetGroundNormal());
        }

        //Apply slide gravity along ground normal, if controller is sliding;
        if (CurrentCharacterState == E_TempleCharacterState.Sliding)
        {
            Vector3 _slideDirection = Vector3.ProjectOnPlane(-CachedTransform.up, mMover.GetGroundNormal()).normalized;
            mMomentum += _slideDirection * SlideGravity * Time.deltaTime;
        }

        //If controller is jumping, override vertical velocity with jumpSpeed;
        if (CurrentCharacterState == E_TempleCharacterState.Jumping)
        {
            mMomentum = VectorHelper.RemoveDotVector(mMomentum, CachedTransform.up);
            mMomentum += CachedTransform.up * JumpSpeed;
        }
    }

    //Calculate and return movement velocity based on player input, controller state, ground normal [...];
    protected Vector3 CalculateMovementVelocity()
    {
        //Calculate (normalized) movement direction;
        Vector3 _velocity = mDir;

        //Save movement direction for later;
        Vector3 _velocityDirection = _velocity;

        //Multiply (normalized) velocity with movement speed;
        _velocity *= mMoveSpeed;

        //If controller is not grounded, multiply movement velocity with 'airControl';
        if (CurrentCharacterState != E_TempleCharacterState.Grounded)
            _velocity = _velocityDirection * mMoveSpeed * AirControl;

        return _velocity;
    }

    private void AnimLanding()
    {
        mOwner.SetAnimParameter(E_AnimParameter.FallDown_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Jump_001, false);

        mOwner.SetAnimParameter(E_AnimParameter.Landing_001);
    }

    private void AnimRising()
    {
        mOwner.SetAnimParameter(E_AnimParameter.FallDown_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Jump_Start_001);
        mOwner.SetAnimParameter(E_AnimParameter.Jump_001, true);
    }

    private void AnimFalling()
    {
        mOwner.SetAnimParameter(E_AnimParameter.Jump_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.FallDown_001, true);
    }

    private void AnimJump()
    {
        mOwner.SetAnimParameter(E_AnimParameter.FallDown_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Jump_Start_001);
        mOwner.SetAnimParameter(E_AnimParameter.Jump_001, true);
    }

    E_TempleCharacterState DetermineCharacterState()
    {
        //Check if vertical momentum is pointing upwards;
        bool _isRising = IsRisingOrFalling() && (VectorHelper.GetDotProduct(mMomentum + AddMomentumValue, CachedTransform.up) > 0f);

        bool _isSliding = mMover.IsGrounded && IsGroundTooSteep();

        //Grounded;
        if (CurrentCharacterState == E_TempleCharacterState.Grounded)
        {
            if (_isRising)
            {
                OnGroundContactLost();
                AnimRising();
                return E_TempleCharacterState.Rising;
            }
            if (!mMover.IsGrounded)
            {
                OnGroundContactLost();
                AnimFalling();
                return E_TempleCharacterState.Falling;
            }
            if (_isSliding)
            {
                return E_TempleCharacterState.Sliding;
            }
            return E_TempleCharacterState.Grounded;
        }

        //Falling;
        if (CurrentCharacterState == E_TempleCharacterState.Falling)
        {
            if (_isRising)
            {
                AnimRising();
                return E_TempleCharacterState.Rising;
            }
            if (mMover.IsGrounded && !_isSliding)
            {
                OnGroundContactRegained(mMomentum);
                AnimLanding();
                return E_TempleCharacterState.Grounded;
            }
            if (_isSliding)
            {
                OnGroundContactRegained(mMomentum);
                AnimLanding();
                return E_TempleCharacterState.Sliding;
            }

            return E_TempleCharacterState.Falling;
        }

        //Sliding;
        if (CurrentCharacterState == E_TempleCharacterState.Sliding)
        {
            if (_isRising)
            {
                OnGroundContactLost();
                AnimRising();
                return E_TempleCharacterState.Rising;
            }
            if (!mMover.IsGrounded)
            {
                OnGroundContactLost();
                AnimFalling();
                return E_TempleCharacterState.Falling;
            }
            if (mMover.IsGrounded && !_isSliding)
            {
                OnGroundContactRegained(mMomentum);
                return E_TempleCharacterState.Grounded;
            }
            return E_TempleCharacterState.Sliding;
        }

        //Rising;
        if (CurrentCharacterState == E_TempleCharacterState.Rising)
        {
            if (!_isRising)
            {
                if (mMover.IsGrounded)
                {
                    OnGroundContactRegained(mMomentum);
                    AnimLanding();

                    return E_TempleCharacterState.Grounded;
                }
                if (_isSliding)
                {
                    AnimLanding();
                    return E_TempleCharacterState.Sliding;
                }
                if (!mMover.IsGrounded)
                {
                    AnimFalling();
                    return E_TempleCharacterState.Falling;
                }
            }

            return E_TempleCharacterState.Rising;
        }

        //Jumping;
        if (CurrentCharacterState == E_TempleCharacterState.Jumping)
        {
            //Check for jump timeout;
            if ((Time.time - mCurrentJumpStartTime) > JumpDuration)
                return E_TempleCharacterState.Rising;

            //Check if jump key was let go;
            if (mJumpKeyWasLetGo)
            {
                AnimJump();
                return E_TempleCharacterState.Rising;
            }

            return E_TempleCharacterState.Jumping;
        }

        return E_TempleCharacterState.Falling;
    }

    //Returns 'true' if vertical momentum is above a small threshold;
    private bool IsRisingOrFalling()
    {
        //Calculate current vertical momentum;
        Vector3 _verticalMomentum = VectorHelper.ExtractDotVector(mMomentum + AddMomentumValue, CachedTransform.up);

        //Setup threshold to check against;
        //For most applications, a value of '0.001f' is recommended;
        float _limit = 0.001f;

        //Return true if vertical momentum is above '_limit';
        return (_verticalMomentum.magnitude > _limit);
    }

    //Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
    private bool IsGroundTooSteep()
    {
        if (!mMover.IsGrounded)
            return true;

        return (Vector3.Angle(mMover.GetGroundNormal(), CachedTransform.up) > SlopeLimit);
    }

    //This function is called when the player has initiated a jump;
    public void OnJumpStart(float jumpPower = 10f, bool bForce = false)
    {
        DisableNavAgent();
        JumpSpeed = jumpPower;
        //Add jump force to momentum;
        if(bForce)
        {
            mMomentum = CachedTransform.up * JumpSpeed;
        }
        else
        {
            mMomentum += CachedTransform.up * JumpSpeed;
        }
        //Set jump start time;
        mCurrentJumpStartTime = Time.time;

        //Call event;
        //if (OnJump != null)
        //    OnJump(mMomentum);
    }

    public void DoAddMomentum(Vector3 momentum, float duration)
    {
        //if(CurrentCharacterState == E_TempleCharacterState.Falling)
        {
            mMomentum = Vector3.zero;
        }

        AddMomentum = momentum;
        AddMomentumElapsedTime = 0f;
        AddMomentumDuration = Mathf.Max(duration, 0.1f);        
    }

    protected override void UpdateStateImpl()
    {
        if (null != AddMomentum)
        {
            AddMomentumElapsedTime += Time.deltaTime;
            float durationRate = (AddMomentumElapsedTime / AddMomentumDuration);

            if (durationRate >= 1f)
            {
                AddMomentum = null;
                AddMomentumValue = Vector3.zero;
                return;
            }

            AddMomentumValue = -AddMomentum.Value * durationRate + AddMomentum.Value;            
        }
        else
        {
            if (false == IsEnableNavAgent)
            {
                return;
            }

            if (false == mNavAgent.isOnNavMesh || mDir != Vector3.zero)
            {
                DisableNavAgent();
                return;
            }

            if (mNavAgent.pathPending)
            {
                return;
            }

            if (mNavAgent.remainingDistance > mOwnerComp.StoppingDistance)
            {
                return;
            }

            DisableNavAgent();
        }
    }

    //This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
    void OnGroundContactLost()
    {
        //Calculate current velocity;
        //If velocity would exceed the controller's movement speed, decrease movement velocity appropriately;
        //This prevents unwanted accumulation of velocity;
        float _horizontalMomentumSpeed = VectorHelper.RemoveDotVector(mMomentum, CachedTransform.up).magnitude;
        Vector3 _currentVelocity = mMomentum + Vector3.ClampMagnitude(mSavedMovementVelocity, Mathf.Clamp(mMoveSpeed - _horizontalMomentumSpeed, 0f, mMoveSpeed));

        //Calculate length and direction from '_currentVelocity';
        float _length = _currentVelocity.magnitude;

        //Calculate velocity direction;
        Vector3 _velocityDirection = Vector3.zero;
        if (_length != 0f)
            _velocityDirection = _currentVelocity / _length;

        //Subtract from '_length', based on 'movementSpeed' and 'airControl', check for overshooting;
        if (_length >= mMoveSpeed * AirControl)
            _length -= mMoveSpeed * AirControl;
        else
            _length = 0f;

        mMomentum = _velocityDirection * _length;
    }

    //This function is called when the controller has landed on a surface after being in the air;
    void OnGroundContactRegained(Vector3 _collisionVelocity)
    {
        //Call 'OnLand' event;
        //if (OnLand != null)
        //    OnLand(_collisionVelocity);
    }

    //This function is called when the controller has collided with a ceiling while jumping or moving upwards;
    void OnCeilingContact()
    {
        //Remove all vertical parts of momentum;
        mMomentum = VectorHelper.RemoveDotVector(mMomentum, CachedTransform.up);
    }

    //Returns 'true' if controller is grounded (or sliding down a slope);
    public bool IsGrounded()
    {
        return (CurrentCharacterState == E_TempleCharacterState.Grounded || CurrentCharacterState == E_TempleCharacterState.Sliding);
    }

    public bool IsSliding()
    {
        return (CurrentCharacterState == E_TempleCharacterState.Sliding);
    }

    public override bool IsMoving()
    {
        return RBody.velocity.magnitude > 0;
    }

    public override bool IsMovingDir()
    {
        return mDir != Vector3.zero;
    }

    public override Vector3? MoveTo(Vector3 destPosition, float speed)
    {
        //mRBody.MovePosition(path);
        if(CurrentCharacterState == E_TempleCharacterState.Grounded && null == AddMomentum)
        {
            StopMove(mOwner.Position);
            EnableNavAgent();

            if(IsEnableNavAgent)
            {
                //네비메시 이동 처리
                mOwnerComp.SetSpeed(speed);
                mNavAgent.speed = speed;
                mNavAgent.SetDestination(destPosition);
            }
        }

        return null;
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        if (mOwner.IsBlockMove())
		{
            mDir = default;
            mMoveSpeed = 0f;
            return null;
        }   

        mDir = dir.normalized;
        mMoveSpeed = speed;

        if (IsEnableNavAgent)
        {
            DisableNavAgent();
        }

        return null;
    }

    public override void Warp(Vector3 position)
    {
        base.Warp(position);
        DisableNavAgent();
    }

    public override void StopMove(Vector3 curPosition)
    {
        mDir = default;
        mMoveSpeed = 0f;

        //RBody.position = curPosition;
        //RBody.velocity = Vector3.zero;
        //RBody.angularVelocity = Vector3.zero;

        DisableNavAgent();
    }

    protected override void HandleEventChangeAnimController()
    {
        switch(CurrentCharacterState)
        {
            case E_TempleCharacterState.Falling:
                AnimFalling();
                break;
            case E_TempleCharacterState.Jumping:
                AnimJump();
                break;
            case E_TempleCharacterState.Rising:
                AnimRising();
                break;
        }
    }
}
