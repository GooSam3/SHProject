using System;

/// <summary> 사당 입장용 컴포넌트에 알림 </summary>
public class ZGA_NotifyForTempleEntrance : ZGimmickActionBase
{
    [NonSerialized]
    public uint TempleTableId;

    protected override void InvokeImpl()
    {
        if(false == ZGimmickManager.Instance.TryGetEntranceValue(TempleTableId, out var entrance))
        {
            return;
        }

        entrance.OnGimmickActionNotify();
    }

    protected override void CancelImpl()
    {
    }
}