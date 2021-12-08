using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary> 해당 스테이지에 입장할때까지 대기 </summary>
public class TutorialSequence_CheckStage : TutorialSequence_None
{
	protected override float StartDelayTime => 3f;

	private uint StageTid = 0;
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		SetBlockScereen(false, false);
		InvokeClearTemple();
	}

	private void InvokeClearTemple()
	{	
		//스테이지 체크
		//로딩중일경우 패스
		CancelInvoke(nameof(InvokeClearTemple));
		if (ZGameModeManager.Instance.StageTid != StageTid || ZGameManager.Instance.IsEnterGameLoading)
		{
			Invoke(nameof(InvokeClearTemple), 0.5f);
			return;
		}

		EndSequence(false);
	}

	protected override void SetParams(List<string> args)
	{
		uint.TryParse(args[0], out StageTid);
	}
}