using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary> 유적 고정 클래스 </summary>
public class TutorialSequence_TempleFixedClass : TutorialSequence_None
{
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		//바로 종료
		EndSequence(false);
	}

	protected override void SetParams(List<string> args)
	{
		foreach (var data in args)
		{
			var split = data.Split(':');

			if(ZPawnManager.Instance.MyEntity.TableId == uint.Parse(split[0]))
			{
				//고정 변신 처리
				ZPawnManager.Instance.TempleFixedChangeTid = uint.Parse(split[1]);
				return;
			}
		}
	}

	private void OnDestroy()
	{
		//고정 변신 원복
		if(ZPawnManager.hasInstance)
			ZPawnManager.Instance.TempleFixedChangeTid = 0;
	}
}