using UnityEngine;

/// <summary> 중력 조작 </summary>
public class ZTempleMiniGame_GravityControl : ZTempleMiniGameBase
{
    public override E_TempleUIType ControlType => E_TempleUIType.Joystick_CancelActionButton;

    /// <summary> 중력건 작동된 상태에서 표시될 UI </summary>
    private E_TempleUIType GravityControlType = E_TempleUIType.Joystick_AllButton;


    private void Start()
    {
        DefaultAngle = TransLauncher.rotation.eulerAngles;        
    }

    protected override void InitializeImpl()
    {
        base.InitializeImpl();

        //애니메이션은 따로 컨트롤한다.
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 0f);
        Gimmick.SetAnimParameter(E_AnimParameter.Start_001);

        //일단 밑으로 포신 이동시키자
        AnimNormalizeTime = 1f;
        Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, AnimNormalizeTime);
    }

    protected override void StartMiniGame()
    {
        IsForward = false;
        IsBackward = false;
        IsLinking = false;

        //소켓 셋팅
        SocketProjectile = Gimmick.GetSocket(GameDB.E_ModelSocket.Projectile);
                
        float maxDistance = Mathf.Sqrt(Mathf.Pow(HeightOffset, 2) + Mathf.Pow(XZMaxDistance, 2));
        Magnetic.Initialize(this, maxDistance, OnFinished);

        //내 타겟 제거. 메테리얼 프로퍼티 변경때문에
        ZPawnManager.Instance.MyEntity.SetTarget(null);

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    protected override void EndMiniGame()
    {
        Magnetic.End();

        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    private void OnDestroy()
    {
        if (ZMonoManager.hasInstance)
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    protected override bool CheckCompleteImpl()
    {
        return false;
    }
    
    [SerializeField]
    private ZTempleProjectile_Magnetic Magnetic;

    [Header("중력건이 적용될 기믹 재질")]
    [SerializeField]
    private E_TempleGimmickMeterial mApplyMeterial = E_TempleGimmickMeterial.Metal;

    public E_TempleGimmickMeterial ApplyMeterial { get { return mApplyMeterial; } }

    [Header("회전판")]
    [SerializeField]
    private Transform TransLauncher;

    [Header("감도")]
    [SerializeField]
    private float Sensitivity = 1f;
        
    [Header("높이")]
    [SerializeField]
    public float HeightOffset = 20f;

    [Header("xz 축 최소 거리")]
    [SerializeField]
    private float XZMinDistance = 5f;
    [Header("xz 축 최대 거리")]
    [SerializeField]
    private float XZMaxDistance = 20f;

    //[Header("연결된 상태라면 연결이 해제될 각도")]
    //[SerializeField]
    //[Range(30f, 180f)]
    //public float DisconnectAngle = 60f;

    public Transform SocketProjectile;
    /// <summary> 바라보는 방향 </summary>
    public Vector3 Forward { get { return SocketProjectile.forward; } }

    /// <summary> y 현재 높이 </summary>
    public float CurrentHeight { get; private set; } = 0f;

    /// <summary> xz 축 현재 거리 </summary>    
    public float CurrentDistance { get; private set; } = 0f;

    private bool IsForward = false;
    private bool IsBackward = false;

    private float AnimNormalizeTime;

    /// <summary> 기본 각도 </summary>
    private Vector3 DefaultAngle = default;

    [Header("좌우 최대 각도")]
    [SerializeField]
    private float MaxAngleY = 60f;

    private float AngleY = 0;    

    /// <summary> 중력건 발사 상태 </summary>
    private bool IsLinking = false;

    /// <summary> 중력건 회전 </summary>
    public override void MoveJoystick(Vector2 joysticDir)
    {
        //연결중 상태면 패스
        if (true == IsLinking)
            return;

        AngleY += (joysticDir.x * Sensitivity);

        AngleY = Mathf.Clamp(AngleY, -MaxAngleY, MaxAngleY);
        
        AnimNormalizeTime -= (joysticDir.y * Time.smoothDeltaTime);
        AnimNormalizeTime = Mathf.Clamp(AnimNormalizeTime, 0f, 1f);
    }

    /// <summary> 액션 버튼 클릭 </summary>
    public override void OnClickAction()
    {
        //연결중이면 대기
        if (true == IsLinking)
            return;

        //연결 상태 체크        
        if (false == Magnetic.IsConnected)
        {
            IsLinking = true;
            Magnetic.Fire();
        }
        else
        {
            Magnetic.End();
        }
    }

    private void OnFinished()
    {
        IsLinking = false;

        if(true == Magnetic.IsConnected)
        {
            CurrentDistance = (Magnetic.ConnectedRBody.worldCenterOfMass - Gimmick.Position).magnitude;
            ChangeControlUIType(GravityControlType);
        }
        else
        {
            ChangeControlUIType(ControlType);
        }
    }

    /// <summary> 중력건에서 앞으로 (중력건 반대 방향) 오브젝트를 이동 시킬때 </summary>
    public override void OnForwadButton(bool bPress)
    {
        IsForward = bPress;
    }

    /// <summary> 중력건에서 뒤로 (중력건 방향) 오브젝트를 이동 시킬때 </summary>
    public override void OnBackwardButton(bool bPress)
    {
        IsBackward = bPress;
    }

    /// <summary> ui 를 변경한다. </summary>
    private void ChangeControlUIType(E_TempleUIType controlType)
    {
        var pc = ZPawnManager.Instance.MyEntity;
        var state = pc.GetMovement<EntityComponentMovement_Temple>().CurrentState;

        var minigameState = state as TempleCharacterControlState_MiniGame;

        if (null == minigameState)
            return;

        minigameState.SetControlUI(controlType);
    }

    private void HandleLateUpdate()
    {
        Vector3 goal = DefaultAngle + new Vector3(0, AngleY, 0f);
        TransLauncher.rotation = Quaternion.Lerp(TransLauncher.rotation, Quaternion.Euler(goal), Time.smoothDeltaTime * 10f);

        Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, AnimNormalizeTime);

        if (true ==  IsForward)
        {
            CurrentDistance += Time.smoothDeltaTime * 10f;
            CurrentDistance = Mathf.Min(CurrentDistance, XZMaxDistance);
        }

        if(true == IsBackward)
        {
            CurrentDistance -= Time.smoothDeltaTime * 10f;
            CurrentDistance = Mathf.Max(CurrentDistance, XZMinDistance);
        }
    }
}
