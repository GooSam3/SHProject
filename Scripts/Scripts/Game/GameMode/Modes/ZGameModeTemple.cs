using System.Collections.Generic;
using GameDB;
using System;
using ZNet.Data;

/// <summary> 유적 게임 모드 </summary>
public class ZGameModeTemple : ZGameModeBase
{
    public override E_GameModeType GameModeType { get { return E_GameModeType.Temple; } }
    
    protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;

    public Temple_Table TempleTable { get { return DBTemple.GetTempleTableByStageTid(Parent.StageTid); } }

    /// <summary> 서버상 현재 유적 정보 </summary>
    public TempleData TempleNetData { get; private set; }

    public IList<ZGA_Chest> ChestList { get { return ZGimmickManager.Instance.AllChest(); } }

	protected override void ExitGameMode()
	{
		TempleNetData = null;
	}

    protected override void StartGameMode()
    {
        TempleNetData = Me.FindCurCharData?.TempleInfo.GetStage(Parent.StageTid);

        //보물상자 갱신
        UpdateChest();
    }

    private void UpdateChest()
	{
        foreach(var chest in ChestList)
		{
            chest.UpdateChest(IsOpened(chest.ChestIndex));
        }
	}

    /// <summary> 열린 상자 여부 </summary>
    public bool IsOpened(ushort index)
    {
        if (null == TempleNetData)
            return false;

        return TempleNetData.rewardGachaOpens.Contains(index);
    }
}

