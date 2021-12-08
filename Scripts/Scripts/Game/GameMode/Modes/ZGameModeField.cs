
using System;
/// <summary> 필드 게임 모드 </summary>
public class ZGameModeField : ZGameModeBase
{
    public override E_GameModeType GameModeType { get { return E_GameModeType.Field; } }

	protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;

	protected override void ExitGameMode()
	{
		ZGameModeManager.Instance.RemoveBattleInfoEvent();		
	}

	protected override void StartGameMode()
	{
		ZGameModeManager.Instance.AddBattleInfoEvent();
	}
}

