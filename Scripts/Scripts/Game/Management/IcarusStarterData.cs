using UnityEngine;

/// <summary>
/// 이카루스용 게임 스타터
/// </summary>
/// <remarks>
/// 미리 설정한 정보를 플레이도중 사용하고 싶다면 여기다가 선언해놓고 사용하면됨.
/// </remarks>
public class IcarusStarterData : ZStarterDataBase
{
	

	protected override void OnStart()
	{
		var FSM = Owner.FSM;

		FSM.AddState(E_GameState.Start, Owner.gameObject.AddComponent<StartState>());
		FSM.AddState(E_GameState.Logo, Owner.gameObject.AddComponent<LogoState>());
		FSM.AddState(E_GameState.Login, Owner.gameObject.AddComponent<LoginState>());
		FSM.AddState(E_GameState.CharacterSelect, Owner.gameObject.AddComponent<CharacterSelectState>());
		FSM.AddState(E_GameState.InGame, Owner.gameObject.AddComponent<InGameState>());

		// [Optional] 전이 이벤트 등록 
		FSM.RegistEvent(E_GameState.Start, E_GameEvent.Next, E_GameState.Logo);
		FSM.RegistEvent(E_GameState.Logo, E_GameEvent.Next, E_GameState.Login);
		FSM.RegistEvent(E_GameState.Login, E_GameEvent.Next, E_GameState.CharacterSelect);

		FSM.Enable(E_GameState.Start);

		if (!Debug.isDebugBuild)
			ZLog.RemoveChannel(ZLogChannel.MMO);
	}
}
