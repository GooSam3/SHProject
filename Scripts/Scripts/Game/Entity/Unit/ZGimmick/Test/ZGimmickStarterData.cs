using UnityEngine;

/// <summary>
/// 이카루스용 게임 스타터
/// </summary>
public class ZGimmickStarterData : ZStarterDataBase
{
    [Header("대상 AssetName 셋팅 필요 -----------------")]
    public uint CharacterTableId = 100000;
    public uint ChangeTableId = 0;
    public uint StageTid = 1011001;

    protected override void OnStart()
    {
        var FSM = Owner.FSM;

        FSM.AddState(E_GameState.Start, Owner.gameObject.AddComponent<ZGimmickTestState>());
        FSM.Enable(E_GameState.Start);
    }
}
