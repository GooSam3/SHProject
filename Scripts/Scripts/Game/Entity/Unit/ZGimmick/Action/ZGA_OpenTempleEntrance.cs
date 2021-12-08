/// <summary> 사당 입구를 오픈 시킨다. </summary>
public class ZGA_OpenTempleEntrance : ZGimmickActionBase
{
    /// <summary> 오픈할 사당 입구 </summary>
    public uint OpenTempleTableId;

    protected override void InvokeImpl()
    {
        if (false == ZGimmickManager.Instance.TryGetEntranceValue(OpenTempleTableId, out var entrance))
        {
            return;
        }

        entrance.EnableTempleEntranceForGimmick();
    }

    protected override void CancelImpl()
    {
    }
}