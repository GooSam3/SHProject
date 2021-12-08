
/// <summary> 게임 모드 </summary>
public class ZGameModeEmpty : ZGameModeBase
{
    public override E_GameModeType GameModeType { get { return E_GameModeType.Empty; } }

    protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.None;

    protected override void StartGameMode()
    {
    }
}

