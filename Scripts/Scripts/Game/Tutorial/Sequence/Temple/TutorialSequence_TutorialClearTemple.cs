using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary> 유적 클리어 체크 </summary>
public class TutorialSequence_TutorialClearTemple : TutorialSequence_None
{
	private uint TempleStageTid = 0;
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		SetBlockScereen(false, false);
		InvokeClearTemple();
	}

	private void InvokeClearTemple()
	{
		//유적 클리어 체크
		//필드일 경우에만 체크하자.
		//로딩중일경우 패스
		CancelInvoke(nameof(InvokeClearTemple));
		if (ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Field || ZGameManager.Instance.IsEnterGameLoading)
		{
			Invoke(nameof(InvokeClearTemple), 0.5f);
			return;
		}

		//유적 상태 체크
		var type = ZNet.Data.Me.FindCurCharData.TempleInfo.GetTempleStartType(TempleStageTid);
		if (type == E_TempleInfoState.Clear || type == E_TempleInfoState.Replay)
		{
			EndSequence(false);
		}
		else
		{
			Invoke(nameof(InvokeClearTemple), 0.5f);
		}
	}

	protected override void SetParams(List<string> args)
	{
		uint.TryParse(args[0], out TempleStageTid);
	}
}