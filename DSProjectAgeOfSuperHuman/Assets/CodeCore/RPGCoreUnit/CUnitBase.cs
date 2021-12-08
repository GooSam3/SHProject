using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

abstract public class CUnitBase : CMonoBase
{
	public enum EUnitRelationType
	{
		None,
		Hero,
		Enemy,
		Neutral,
	}

	public enum EUnitState
	{
		None,
		StandBy,        // 인스턴스 생성 후 대기 상태이다.
		Spawn,          // 스폰중이다.
		PhaseOut,	   // 스폰은 하였으나 전투에 참가하지 않은 상태이다. (모습 보임, 업데이트 받음)
		Alive,          // 다양한 활동을 할수 있다.
		Exit,           // 게임에서 잠시 퇴장한 상태이다. (모습 보이지 않음, 업데이트는 받음)
		DeathStart,      // 죽는 과정이 시작되었다 (에니메이션등), 상호작용 불가
		Death,          // 죽은 상태로 업데이트 받지 않는다. 부활등의 상호작용이 가능하다.
		Remove,         // 게임에서 제거된 상태이다 (메모리 풀에 들어간 상태). 상호작용 완전 불가능
	}


	public enum EUnitControlType
	{
		None,
		PlayerControl,      // 플레이어 직접 조정
		PlayerAI,          // 아군 AI에 의해 조정 
		PlayerPossess,      // 아군 AI에 의해 조정, 정신지배등에 의해 아군이 된 경우
		EnemyAI,            // 적군 AI에 의해 조정 
		EnemyPossess,       // 적군 AI에 의해 조정, 정신지배등에 의해 적군이 된 경우
	}

	protected EUnitControlType m_eUnitControlType = EUnitControlType.None;	public EUnitControlType GetUnitControlType() { return m_eUnitControlType; }
	protected EUnitState m_eUnitState = EUnitState.None;					public EUnitState GetUnitState() { return m_eUnitState; }
	protected EUnitRelationType m_eUnitRelation = EUnitRelationType.None;	public EUnitRelationType GetUnitRelationForPlayer() { return m_eUnitRelation; }
	protected CUnitSocketAssistBase m_pSocketAssist = null;
	protected bool m_bAlive = false;									public bool IsAlive { get { return m_bAlive; } }
	private bool m_bDynamicLoad = false;								public bool IsDynamicLoad { get { return m_bDynamicLoad; } }    // 테이블등에 의해 동적으로 로딩된 객체. false는 Scene에서 할당됨
	private bool m_bInitialize = false;
	private uint m_hUnitID = 0;										public uint GetUnitID() { return m_hUnitID; } protected void SetUnitID(uint hUnitID, string strUnitName) { m_hUnitID = hUnitID; m_strUnitName = strUnitName; } // 테이블 식별자
	private uint m_hUnitSessionID = 0;									public uint GetUnitSessionID() { return m_hUnitSessionID; }  // DB 식별자. 유니크하다. 
	private string m_strUnitName = "None";								public string GetUnitName() { return m_strUnitName; }

	//-------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pSocketAssist = GetComponentInChildren<CUnitSocketAssistBase>();
	}

	private void Update()
	{
		OnUnitUpdate();
	}

	//--------------------------------------------------------
	public void DoUnitForceUpdate(float fDelta) // 게임 오브젝트 비활성 상태에서 업데이트를 받기 위해 
	{
		OnUnitForceUpdate(fDelta);
	}

	//-----------------------------------------------------
	protected void ProtUnitDeathStart()
	{
		m_bAlive = false;
		m_eUnitState = EUnitState.DeathStart;
		OnUnitDeathStart();
	}

	protected void ProtUnitDeathEnd()
	{
		m_bAlive = false;
		m_eUnitState = EUnitState.Death;
		OnUnitDeathEnd();
	}

	protected void ProtUnitRemove(bool bForce)
	{
		m_bAlive = false;
		SetMonoActive(false);
		m_eUnitState = EUnitState.Remove;
		OnUnitRemove(bForce);
	}

	protected void ProtUnitSpawned(UnityAction delFinish)
	{
		SetMonoActive(true);
		m_eUnitState = EUnitState.Spawn;
		OnUnitSpawned(delFinish);
	}

	protected void ProtUnitExit(UnityAction delFinish)
	{
		m_bAlive = false;
		m_eUnitState = EUnitState.Exit;
		OnUnitExit(delFinish);
	}

	protected void ProtUnitPhaseOut(UnityAction delFinish)
	{
		m_bAlive = false;
		m_eUnitState = EUnitState.PhaseOut;
		SetMonoActive(true);
		OnUnitPhaseOut(delFinish);
	}

	protected void ProtUnitAlive()
	{
		m_eUnitState = EUnitState.Alive;
		m_bAlive = true;
		OnUnitAlive();
	}

	protected void ProtUnitStandBy()
	{
		m_eUnitState = EUnitState.StandBy;
		OnUnitStandBy();
	}

	//--------------------------------------------------------------
	public void DoUnitIniailize()
	{
		if (m_bInitialize) return;

		m_bInitialize = true;
		SetMonoActive(false);
		OnUnitInitialize();
	}

	//-----------------------------------------------------------------
	public Vector3 GetUnitPosition()
	{
		return transform.position;
	}

	public Transform GetUnitSocketTransform(int hSocketID)
	{
		return m_pSocketAssist.GetUnitSocketTrasform(hSocketID);
	}

	//----------------------------------------------------------------
	public void DoUnitReset()
	{
		OnUnitReset();
	}

	//-------------------------------------------------------------
	protected virtual void OnUnitInitialize() { }
	protected virtual void OnUnitUpdate() { }
	protected virtual void OnUnitDeathStart() { }
	protected virtual void OnUnitDeathEnd() { }
	protected virtual void OnUnitRemove(bool bForce) { }
	protected virtual void OnUnitExit(UnityAction delFinish) { }
	protected virtual void OnUnitSpawned(UnityAction delFinish) { }
	protected virtual void OnUnitPhaseOut(UnityAction delFinish) { }
	protected virtual void OnUnitAlive() { }
	protected virtual void OnUnitReset() { }
	protected virtual void OnUnitStandBy() { }
	protected virtual void OnUnitForceUpdate(float fDelta) { }
	public virtual float GetUnitRadius() { return 0f; }
}
