using System.Collections;
using UnityEngine;


/// <summary> 기본 스킬 사용 처리 </summary>
public class TempleSkillStateAction : TempleSkillStateBase
{
	[Header("AbilityAction 적용 타이밍")]
	[SerializeField]
	private float InvokeTime = 0f;

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