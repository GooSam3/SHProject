using UnityEngine;
/// <summary> 사당용 조작 불가 상태 </summary>
public class TempleCharacterControlState_MiniGame : TempleCharacterControlStateBase
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.MiniGame; } }
    private ZTempleMiniGameBase MiniGame;

    protected override void BeginStateImpl(params object[] args)
    {
        if(null == args || args.Length <= 0)
        {
            Cancel();
            return;
        }
        StopMove(mOwner.Position);
        mOwner.MoveAnim(false);

        MiniGame = args[0] as ZTempleMiniGameBase;

        SetControlUI();
    }

    protected override void EndStateImpl()
    {
        ResetControlUI();
    }

    protected override void CancelImpl()
    {
        ChangeState(E_TempleCharacterControlState.Default);
    }

    private void SetControlUI()
    {
        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.SetControlGimmick(MiniGame.ControlType, OnClickCancel, OnClickAction, OnForwardButton, OnBackwardButton);
    }

    public void SetControlUI(E_TempleUIType controlType)
    {
        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.SetControlGimmick(controlType, OnClickCancel, OnClickAction, OnForwardButton, OnBackwardButton);
    }

    private void ResetControlUI()
    {
        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.ResetControlGimmick();
    }

    /// <summary> 취소 버튼 클릭 </summary>
    private void OnClickCancel()
    {
        MiniGame.OnClickCancel();
    }

    /// <summary> 액션 버튼 클릭 </summary>
    private void OnClickAction()
    {
        MiniGame.OnClickAction();
    }

    private void OnForwardButton(bool bPress)
    {
        MiniGame.OnForwadButton(bPress);
    }

    private void OnBackwardButton(bool bPress)
    {
        MiniGame.OnBackwardButton(bPress);
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        MiniGame.MoveJoystick(joystickDir);
        return null;
    }


#if UNITY_EDITOR
    protected override void UpdateStateImpl()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnClickCancel();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            OnClickAction();
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            OnForwardButton(true);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {            
            OnForwardButton(false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnBackwardButton(true);
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            OnBackwardButton(false);
        }
    }
#endif
}
