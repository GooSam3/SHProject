/// <summary> 사당 퇴장시 연출 </summary>
public class ZGA_ExitTemple : ZGimmickActionBase
{
    protected override void InvokeImpl()
    {
        var table = DBTemple.GetTempleTableByStageTid(ZGameModeManager.Instance.StageTid);

        if (null == table)
        {            
            return;
        }

        if (false == ZWebManager.hasInstance)
            return;

        ZWebManager.Instance.WebGame.REQ_TempleClearReward(ZGameModeManager.Instance.StageTid, (packet, res) =>
        {
            //TODO :: 일단 퇴장 처리
            ZGameManager.Instance.TryEnterStage(table.ExitPortalID, false, 0, 0);
        });
    }

    protected override void CancelImpl()
    {

    }
}