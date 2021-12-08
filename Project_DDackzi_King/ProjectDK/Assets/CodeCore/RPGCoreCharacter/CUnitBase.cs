using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitBase : CMonoBase
{
	public enum EUnitRelationType
	{
		None,
		Ally,
		Enemy,
		Neutral, 
	}

	public enum EUnitState
	{
		None,
		Alive,			// 다양한 활동을 할수 있다.
		WaitingForDeath,	// 죽는 과정이 시작되었다 (에니메이션등)
		Death,			// 죽었으므로 입력이 불가능하다.
		Leave,			// 죽은 이후 비활성 상태에서 대기중
		Remove,			// 게임에서 제거된 상태이다 (메모리 풀에 들어간 상태)
	}


	public enum EUnitControlType
	{
		None,
		PlayerControl,		// 플레이어 직접 조정
		PlayerAI,			// 아군 AI에 의해 조정 
		PlayerPossess,		// 아군 AI에 의해 조정, 정신지배등에 의해 아군이 된 경우
		EnemyAI,				// 적군 AI에 의해 조정 
		EnemyPossess,			// 적군 AI에 의해 조정, 정신지배등에 의해 적군이 된 경우
	}

	protected EUnitControlType m_eUnitControlType = EUnitControlType.None;  public EUnitControlType GetUnitControlType() { return m_eUnitControlType; }
	protected EUnitState m_eUnitState = EUnitState.None;  public EUnitState GetUnitState() { return m_eUnitState; }
	protected CUnitSocketAssistBase m_pSocketAssist = null;
	protected bool m_bAlive = true;			public bool IsAlive { get { return m_bAlive; } }
	private bool m_bDynamicLoad = false;		public bool IsDynamicLoad { get { return m_bDynamicLoad; } } // 테이블등에 의해 동적으로 로딩된 객체. false는 Scene에서 할당됨
	private uint m_hUnitID = 0;				public uint GetUnitID() { return m_hUnitID; }			   // 테이블 식별자
	private uint m_hUnitSessionID = 0;			public uint GetUnitSessionID() { return m_hUnitSessionID; }  // DB 식별자. 유니크하다. 
	private string m_strUnitName = "None";		public string GetUnitName() { return m_strUnitName; } protected void SetUnitName(string strName) { m_strUnitName = strName; } 
	//-------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		CAnimationEventHandlerBase pEventHandler = GetComponentInChildren<CAnimationEventHandlerBase>();
		if (pEventHandler)
		{
			pEventHandler.ImportEventHandlerOwner(this);
		}

		m_pSocketAssist = GetComponentInChildren<CUnitSocketAssistBase>();
	}

	private void Update()
	{
		OnUnitUpdate();
	}
	//--------------------------------------------------------------
	protected EUnitRelationType ProtUnitExtractRelationType(CUnitBase pCompareUnit)
	{
		EUnitRelationType eRelationType = EUnitRelationType.None;

		EUnitControlType eCompareType = pCompareUnit.GetUnitControlType();
		return eRelationType;
	}

	protected void ProtUnitDeathStart()
	{
		m_bAlive = false;
		m_eUnitState = EUnitState.WaitingForDeath;
		OnUnitDeathStart();
	}

	protected void ProtUnitDeathEnd()
	{
		SetMonoActive(false);
		m_eUnitState = EUnitState.Death;		
		OnUnitDeathEnd();
	}

	protected void ProtUnitRemove()
	{
		m_eUnitState = EUnitState.Remove;
		OnUnitRemove();
	}

	//--------------------------------------------------------------
	internal void ImportUnitIniailize()
	{
		SetMonoActive(true);
		m_eUnitState = EUnitState.Alive;
		m_bAlive = true;
		OnUnitInitialize();
	}

	internal void ImportUnitLeave()
	{
		m_eUnitState = EUnitState.Leave;
		OnUnitLeave();
	}

	public Vector3 GetUnitPosition()
	{
		return transform.position;
	}
		

	//-------------------------------------------------------------
	protected virtual void OnUnitInitialize() { }
	protected virtual void OnUnitLeave() { }
	protected virtual void OnUnitUpdate() { }
	protected virtual void OnUnitDeathStart() { }
	protected virtual void OnUnitDeathEnd() { }
	protected virtual void OnUnitRemove() { }

	public virtual float GetUnitRadius() { return 0f; }

}
