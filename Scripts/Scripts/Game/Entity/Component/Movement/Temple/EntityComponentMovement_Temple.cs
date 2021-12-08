using System.Collections.Generic;
using UnityEngine;

/// <summary> 사당용 이동 </summary>
public class EntityComponentMovement_Temple : EntityComponentMovementBase
{
    /// <summary> Fixed Update 등록 </summary>
    protected override bool EnableFixedUpdate => true;
    protected override bool EnableLateUpdate => true;
    protected override bool EnableUpdate => true;

    public override bool IsPossibleRide { get { return CurrentState.IsPossibleRide; } }

    //Events;
    public delegate void VectorEvent(Vector3 v);
    public VectorEvent OnJump;
    public VectorEvent OnLand;

    public Rigidbody RBody { get { return mMover.Rbody; } }    
    public Mover mMover { get; private set; }

    private Dictionary<E_TempleCharacterControlState, TempleCharacterControlStateBase> m_dicTempleControlState = new Dictionary<E_TempleCharacterControlState, TempleCharacterControlStateBase>();

    public TempleCharacterControlStateBase CurrentState { get; private set; }

    public float StoppingDistance { get { return STOPPING_DISTANCE; } }
    
    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();
        
        //mover 셋팅
        mMover = gameObject.GetOrAddComponent<Mover>();
        mMover.Initialize(0.5f, 2f);        

        //스테이트 셋팅
        m_dicTempleControlState.Clear();
        m_dicTempleControlState.Add(E_TempleCharacterControlState.Empty, new TempleCharacterControlState_Empty());
        m_dicTempleControlState.Add(E_TempleCharacterControlState.Default, new TempleCharacterControlState_Default());
        m_dicTempleControlState.Add(E_TempleCharacterControlState.PullPush, new TempleCharacterControlState_PullPush());
        m_dicTempleControlState.Add(E_TempleCharacterControlState.ControllPanel, new TempleCharacterControlState_ControlPanel());
        m_dicTempleControlState.Add(E_TempleCharacterControlState.Ladder, new TempleCharacterControlState_Ladder());
        m_dicTempleControlState.Add(E_TempleCharacterControlState.Carry, new TempleCharacterControlState_Carry());
        m_dicTempleControlState.Add(E_TempleCharacterControlState.MiniGame, new TempleCharacterControlState_MiniGame());

        //스테이트 초기화
        foreach (var controlState in m_dicTempleControlState)
        {
            controlState.Value.InitializeController(Owner.To<ZPawn>(), this);
        }

        //최초 스테이트 셋팅
        ChangeCharacterControlState(E_TempleCharacterControlState.Default);
    }

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();
        if (null != mMover)
        {
            mMover.DestroyMover();            
        }
        CurrentState?.DestroyController();
        
        CurrentState = null;
        mMover = null;
    }

    /// <summary> 조작 상태를 변경한다. </summary>
    public void ChangeCharacterControlState(E_TempleCharacterControlState state, params object[] args)
    {
        if (null != CurrentState)
        {
            CurrentState.EndState();
        }

        Owner.To<ZPawnMyPc>().SetCustomConditionControl(E_CustomConditionControl.Temple_Control, state != E_TempleCharacterControlState.Default);        

        CurrentState = m_dicTempleControlState[state];
        CurrentState.BeginState(args);
    }

    protected override void OnFixedUpdateImpl()
    {
        CurrentState.FixedUpdateState();
    }

    protected override void OnUpdateImpl()
    {
        CurrentState.UpdateState();
    }

    protected override void OnLateUpdateImpl()
    {
        CurrentState.LateUpdateState();
    }

    public override bool IsMoving()
    {
        return CurrentState.IsMoving();
    }

    public override bool IsMovingDir()
    {
        return CurrentState.IsMovingDir();
    }

    public override Vector3? MoveTo(Vector3 destPosition, float speed, bool bInputMove)
    {
        return CurrentState.MoveTo(destPosition, speed);
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        return CurrentState.MoveToDirection(curPosition, dir, speed, joystickDir);
    }
    
    public override void Warp(Vector3 position)
    {
        CurrentState.Warp(position);
    }

    public override void StopMove(Vector3 curPosition)
    {
        CurrentState.StopMove(curPosition);
    }
}
