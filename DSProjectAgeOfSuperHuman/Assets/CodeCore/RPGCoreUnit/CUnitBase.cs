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
		StandBy,        // �ν��Ͻ� ���� �� ��� �����̴�.
		Spawn,          // �������̴�.
		PhaseOut,	   // ������ �Ͽ����� ������ �������� ���� �����̴�. (��� ����, ������Ʈ ����)
		Alive,          // �پ��� Ȱ���� �Ҽ� �ִ�.
		Exit,           // ���ӿ��� ��� ������ �����̴�. (��� ������ ����, ������Ʈ�� ����)
		DeathStart,      // �״� ������ ���۵Ǿ��� (���ϸ��̼ǵ�), ��ȣ�ۿ� �Ұ�
		Death,          // ���� ���·� ������Ʈ ���� �ʴ´�. ��Ȱ���� ��ȣ�ۿ��� �����ϴ�.
		Remove,         // ���ӿ��� ���ŵ� �����̴� (�޸� Ǯ�� �� ����). ��ȣ�ۿ� ���� �Ұ���
	}


	public enum EUnitControlType
	{
		None,
		PlayerControl,      // �÷��̾� ���� ����
		PlayerAI,          // �Ʊ� AI�� ���� ���� 
		PlayerPossess,      // �Ʊ� AI�� ���� ����, �������� ���� �Ʊ��� �� ���
		EnemyAI,            // ���� AI�� ���� ���� 
		EnemyPossess,       // ���� AI�� ���� ����, �������� ���� ������ �� ���
	}

	protected EUnitControlType m_eUnitControlType = EUnitControlType.None;	public EUnitControlType GetUnitControlType() { return m_eUnitControlType; }
	protected EUnitState m_eUnitState = EUnitState.None;					public EUnitState GetUnitState() { return m_eUnitState; }
	protected EUnitRelationType m_eUnitRelation = EUnitRelationType.None;	public EUnitRelationType GetUnitRelationForPlayer() { return m_eUnitRelation; }
	protected CUnitSocketAssistBase m_pSocketAssist = null;
	protected bool m_bAlive = false;									public bool IsAlive { get { return m_bAlive; } }
	private bool m_bDynamicLoad = false;								public bool IsDynamicLoad { get { return m_bDynamicLoad; } }    // ���̺� ���� �������� �ε��� ��ü. false�� Scene���� �Ҵ��
	private bool m_bInitialize = false;
	private uint m_hUnitID = 0;										public uint GetUnitID() { return m_hUnitID; } protected void SetUnitID(uint hUnitID, string strUnitName) { m_hUnitID = hUnitID; m_strUnitName = strUnitName; } // ���̺� �ĺ���
	private uint m_hUnitSessionID = 0;									public uint GetUnitSessionID() { return m_hUnitSessionID; }  // DB �ĺ���. ����ũ�ϴ�. 
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
	public void DoUnitForceUpdate(float fDelta) // ���� ������Ʈ ��Ȱ�� ���¿��� ������Ʈ�� �ޱ� ���� 
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
