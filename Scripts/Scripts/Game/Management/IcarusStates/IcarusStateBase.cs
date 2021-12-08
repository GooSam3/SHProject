using UnityEngine;

/// <summary>
/// 
/// </summary>
public class IcarusStateBase : FSM.BaseState<ZGameManager>
{
	protected string mUIDesc = string.Empty;
	/// <summary> [FSM] Next 이벤트가 존재할때, 표시되는 UI버튼 보일지 여부 </summary>
	protected bool mShowNextButton = true;

	public override void Dev_OnGUI()
	{
		GUILayout.Label($"Current State: {Parent.FSM.Current_State}");
		bool existNextState = Parent.FSM.CheckEvent(E_GameEvent.Next);
		if (mShowNextButton && existNextState && GUILayout.Button("[Next State]"))
		{
			Parent.FSM.ChangeState(E_GameEvent.Next);
		}
		GUILayout.Label(mUIDesc);
		GUILayout.Space(50f);
	}

	protected bool GoNextState()
	{
		bool existNextState = Parent.FSM.CheckEvent(E_GameEvent.Next);
		if (!existNextState)
		{
			Debug.LogError($"{Parent.FSM.Current_State} State에서 {E_GameEvent.Next} 이벤트에 의해 전이될 State가 존재하지 않습니다.");
			return false;
		}

		return Parent.FSM.ChangeState(E_GameEvent.Next);
	}

	protected bool GoInGameState(params object[] args)
	{
		return Parent.FSM.ChangeState(E_GameState.InGame, false, args);
	}
}