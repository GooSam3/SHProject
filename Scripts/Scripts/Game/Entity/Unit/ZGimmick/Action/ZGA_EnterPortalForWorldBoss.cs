using UnityEngine;
using ZNet;

/// <summary> 월드 보스 맵에서 포탈 탈 때 사용 </summary>
public class ZGA_EnterPortalForWorldBoss : ZGimmickActionBase
{
    [Header("발동한 포탈 id")]
    [SerializeField]
    private uint PortalId;

    protected override void InvokeImpl()
    {
        if (0 >= PortalId)
            return;

        // TODO :: 보스 게임 모드 체크해야함
        //if(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.)

        ZGameManager.Instance.TryEnterBossWarField(PortalId, () =>
        {
            
        });
    }

    protected override void CancelImpl()
    {
    }
}