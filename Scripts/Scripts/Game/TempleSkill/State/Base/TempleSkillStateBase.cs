using UnityEngine;
using System;
using System.Collections;


/// <summary> 유적용 보스 스킬 </summary>
public abstract class TempleSkillStateBase : MonoBehaviour
{
	/// <summary> 현재 상태 종료 이벤트 </summary>
	private Action mEventEndState;

	protected TempleBossSkill Skill;

	/// <summary> 스킬 소유주 </summary>
	protected ZPawn Owner { get { return Skill.Owner; } }
		
	/// <summary> 스킬 소유주의 타겟 </summary>
	protected EntityBase Target { get { return Skill.Owner?.GetTarget(); } }

	/// <summary> 초기화 </summary>
	public void Initialize(TempleBossSkill skill, Action onFinish)
	{
		mEventEndState = onFinish;
		Skill = skill;
	} 

	/// <summary> 현재 상태 시작 </summary>
	public void StartState()
	{		
		//실제 시작 처리
		StartStateImpl();

		StartCoroutine(Co_Update());
	}

	/// <summary> 현재 상태 종료 </summary>
	private void EndState()
	{
		StopAllCoroutines();

		//실제 종료 처리
		EndStateImpl();
		mEventEndState?.Invoke();
	}

	/// <summary> 현재 상태 취소 </summary>
	public void CancelState()
	{
		StopAllCoroutines();

		//실제 취소 처리
		CancelStateImpl();
	}

	protected virtual IEnumerator Co_Update()
	{
		if(false)
			yield return null;
	}

	/// <summary> 시작 구현 </summary>
	protected abstract void StartStateImpl();

	/// <summary> 종료 구현 </summary>
	protected abstract void EndStateImpl();

	/// <summary> 취소 구현 </summary>
	protected abstract void CancelStateImpl();
}