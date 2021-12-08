
using System;
/// <summary> 시공의틈 게임 모드 </summary>
public class ZGameModeTower : ZGameModeField
{
    public override E_GameModeType GameModeType { get { return E_GameModeType.Tower; } }
    protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;
}