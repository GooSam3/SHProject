using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary> 해당 튜토리얼 유적을 오픈한다 </summary>
public class TutorialSequence_OpenTutorialTemple : TutorialSequence_None
{
	private uint TempleStageTid = 0;
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		//유적 클리어 체크
		//필드일 경우에만 체크하자.
		CancelInvoke(nameof(StartGuide));
		if(ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Field)
		{
			Invoke(nameof(StartGuide), 0.5f);
			return;
		}

		//유적 상태 체크
		var type = ZNet.Data.Me.FindCurCharData.TempleInfo.GetTempleStartType(TempleStageTid);
		if (type == E_TempleInfoState.Close|| type == E_TempleInfoState.Enter)
		{
			var templeTable = DBTemple.GetTempleTableByStageTid(TempleStageTid);

			if (null != templeTable)
			{
				if (true == ZGimmickManager.Instance.TryGetEntranceValue(templeTable.TempleID, out var temple))
				{
					//튜토리얼용 오픈!
					temple.ForceOpenByTutorial();
				}
			}
		}

		EndSequence(false);
	}

	protected override void SetParams(List<string> args)
	{
		uint.TryParse(args[0], out TempleStageTid);
	}
}