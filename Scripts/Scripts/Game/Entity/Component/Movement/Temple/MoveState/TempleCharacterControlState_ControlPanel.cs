using UnityEngine;

/// <summary> 사당용 밀기/당기기 상태 </summary>
public class TempleCharacterControlState_ControlPanel : TempleCharacterControlStateBase
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.ControllPanel; } }

    /// <summary> 조작할 패널 </summary>
    private ZGA_ControlPanel ControlPanel = null;
    protected override void BeginStateImpl(params object[] args)
    {
        if(null == args || args.Length <= 0)
        {
            ChangeState(E_TempleCharacterControlState.Default);
            return;
        }

        ControlPanel = args[0] as ZGA_ControlPanel;

        ControlPanel.SetVirtualCamera(true);

        mOwner.MoveAnim(false);
        StopMove(mOwner.Position);

        ZPawnManager.Instance.MyEntity.ChangeController<EntityComponentController_MiniGame>();

        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        if(null != subHudTemple)
        {
            subHudTemple.SetControlGimmick(E_TempleUIType.Joystick_CancelButton, () =>
            {
                Cancel();
            });
        }
    }

    protected override void UpdateStateImpl()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Cancel();
        }
#endif
    }

    protected override void EndStateImpl()
    {
        ControlPanel.SetVirtualCamera(false);
        ControlPanel?.ResetControlPanel();

        ZPawnManager.Instance.MyEntity.ChangeController<EntityComponentController>();

        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.ResetControlGimmick();
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        ControlPanel?.SetDir(dir.z, -dir.x);
        return null;
    }

    protected override void CancelImpl()
    {        
        ChangeState(E_TempleCharacterControlState.Default);
    }
}
