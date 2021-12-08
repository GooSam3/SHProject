/// <summary> 사당용 조작 불가 상태 </summary>
public class TempleCharacterControlState_Empty : TempleCharacterControlStateBase
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.Empty; } }

    protected override void BeginStateImpl(params object[] args)
    {
        StopMove(mOwner.Position);
        mOwner.MoveAnim(false);
    }

    protected override void EndStateImpl()
    {
    }

    protected override void CancelImpl()
    {
    }
}
