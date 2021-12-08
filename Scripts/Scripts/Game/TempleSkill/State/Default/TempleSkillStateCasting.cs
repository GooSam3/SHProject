using System.Collections;
using UnityEngine;


/// <summary> 기본 캐스팅 처리 </summary>
public class TempleSkillStateCasting : TempleSkillStateBase
{
	[SerializeField]
	private float LookDuration;

	/// <summary> 시작 구현 </summary>
	protected override void StartStateImpl()
	{

	}

	/// <summary> 종료 구현 </summary>
	protected override void EndStateImpl()
	{

	}

	/// <summary> 취소 구현 </summary>
	protected override void CancelStateImpl()
	{

	}
	protected override IEnumerator Co_Update()
	{
		if (false)
			yield return null;
	}
}