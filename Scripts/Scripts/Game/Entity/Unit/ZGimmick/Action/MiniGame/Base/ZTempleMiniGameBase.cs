using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary> 사당 관련 미니게임 </summary>
public abstract class ZTempleMiniGameBase : ZGimmickActionBase
{
    [Header("미니게임 완료시 요청할 액션(Optional)")]
    [SerializeField]
    private List<GimmickActionInvokeType> CompleteAction = new List<GimmickActionInvokeType>();

    [Header("오픈할 유적 입구(TempleTable ID - 테이블에서 셋팅됨)")]    
    [ReadOnly]
    public uint OpenTempleTableId;

    [Header("조작용 카메라")]
    [SerializeField]
    protected CinemachineVirtualCamera VirtualCamera;

    /// <summary> 미니게임 조작 타입 </summary>
    public abstract E_TempleUIType ControlType { get; }

    public bool IsComplete { get; private set; }
    public bool IsPlaying { get; private set; }

    protected override void InitializeImpl()
    {
        IsComplete = false;

        if(null != VirtualCamera)
            VirtualCamera.gameObject.SetActive(false);
    }

    protected override void InvokeImpl()
    {
        if(IsComplete)
        {
            ZLog.LogError(ZLogChannel.Entity, $"이미 완료된 미니게임이다.");
            return;
        }

        if(IsPlaying)
        {
            ZLog.LogError(ZLogChannel.Entity, $"이미 플레이 중이다.");
            return;
        }

        IsPlaying = true;

        ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);
    }

    protected override void CancelImpl()
    {
        IsPlaying = false;

        if (true == IsComplete)
            return;

        StopMiniGame();
    }

    private void HandleCreateMyEntity()
    {        
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);

        //이동 중지
        ZPawnManager.Instance.MyEntity.StopMove();

        //캐릭터 movement 변경(만약 사당이 아니라면)      
        if (ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Temple)
            ZPawnManager.Instance.MyEntity.ChangeMovementComponent<EntityComponentMovement_Temple>();

        //미니게임 movement 모드로 변경
        ZTempleHelper.ChangeCharacterControlState(E_TempleCharacterControlState.MiniGame, this);

        //미니게임 시작
        StartMiniGame();

        VirtualCamera.gameObject.SetActive(true);
    }

    private void StopMiniGame()
    {
        //미니게임 종료
        EndMiniGame();

        //캐릭터 컨트롤러 원복 (만약 사당이 아니라면)      
        if (ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Temple)
            ZPawnManager.Instance.MyEntity.ChangeMovementComponent<EntityComponentMovement_NavMeshForPlayer>();

        ZTempleHelper.CancelCharacterControlState();

        VirtualCamera.gameObject.SetActive(false);
    }

    /// <summary> 완료 체크 및 완료 처리 </summary>
    protected void CheckComplete()
    {
        //완료 체크
        if(false == CheckCompleteImpl())
        {
            return;
        }

        IsPlaying = false;
        IsComplete = true;

        //완료시 발동할 기믹
        foreach (var action in CompleteAction)
        {
            action.Invoke(InvokeAttributeLevel);
        }

        if (ZGimmickManager.Instance.TryGetEntranceValue(OpenTempleTableId, out var entrance))
        {
            entrance.EnableTempleEntranceForGimmick();
        }

        ZTempleHelper.CancelCharacterControlState();

        StopMiniGame();
    }

    /// <summary> 완료 체크 </summary>
    protected abstract bool CheckCompleteImpl();
    /// <summary> 미니게임 시작 </summary>
    protected abstract void StartMiniGame();
    /// <summary> 미니게임 종료 </summary>
    protected abstract void EndMiniGame();

    public virtual void MoveJoystick(Vector2 joysticDir)
    {

    }

    /// <summary> 취소 버튼 클릭 </summary>
    public void OnClickCancel()
    {
        Cancel();
    }

    /// <summary> 액션 버튼 클릭 </summary>
    public virtual void OnClickAction()
    {

    }

    /// <summary> 중력건에서 앞으로 (중력건 반대 방향) 오브젝트를 이동 시킬때 </summary>
    public virtual void OnForwadButton(bool bPress)
    {

    }

    /// <summary> 중력건에서 뒤로 (중력건 방향) 오브젝트를 이동 시킬때 </summary>
    public virtual void OnBackwardButton(bool bPress)
    {

    }

    protected virtual void OnSetCharacterController()
    {
        ZTempleHelper.ChangeCharacterControlState(E_TempleCharacterControlState.Empty);
    }
}
