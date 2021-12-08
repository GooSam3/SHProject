using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using GameDB;


/// <summary> 유적용 보스 스킬 </summary>
public class TempleBossSkill : MonoBehaviour
{
	/// <summary> 쿨 타임 체크 </summary>
	public bool IsEndCool { get; }

	/// <summary> 스킬 종료 콜백 </summary>
	private Action<E_TempleSkillFinishReason> mEventFinishSkill;

	/// <summary> 스킬 소유주 </summary>
	public ZPawn Owner { get; private set; }

	/// <summary> 스킬 하나에 대한 각 상태 (Casting, Action 등등) </summary>
	private List<TempleSkillStateBase> m_listSkillState = new List<TempleSkillStateBase>();

	private TempleSkillStateBase CurrentState = null;

	/// <summary> 스킬 테이블 </summary>
	public Skill_Table SkillTable { get; private set; }

	private int StateIndex = 0;

	/// <summary> 스킬 관련 초기화. 테이블 관련 처리. </summary>
	public void Initialize(ZPawn owner, uint skillId)
	{
		Owner = owner;

		m_listSkillState.Clear();
		m_listSkillState.AddRange(GetComponentsInChildren<TempleSkillStateBase>());

		foreach(var state in m_listSkillState)
		{
			state.Initialize(this, PlayState);
		}
	}

	/// <summary> 스킬 사용 </summary>
	public void UseSkill(Action<E_TempleSkillFinishReason> onFinishSkill)
	{
		StateIndex = 0;
		mEventFinishSkill = onFinishSkill;
		
		PlayState();
	}

	/// <summary> 스킬 종료 </summary>
	public void CancelSkill()
	{
		if (null != CurrentState)
		{
			CurrentState.CancelInvoke();
			CurrentState = null;
		}
		
		mEventFinishSkill?.Invoke(E_TempleSkillFinishReason.Cancel);
	}

	/// <summary> 스킬 정상 종료 </summary>
	private void EndSkill()
	{		
		CurrentState = null;
		mEventFinishSkill?.Invoke(E_TempleSkillFinishReason.SkillEnd);
	}

	/// <summary> 스킬 상태 시작 </summary>
	private void PlayState()
	{
		if (false == IsValidState())
		{
			//종료다!
			EndSkill();
			return;
		}	

		m_listSkillState[StateIndex].StartState();

		++StateIndex;
	}

	/// <summary> 현재 상태 유효성 체크 </summary>
	private bool IsValidState()
	{
		return m_listSkillState.Count > StateIndex;
	}
}