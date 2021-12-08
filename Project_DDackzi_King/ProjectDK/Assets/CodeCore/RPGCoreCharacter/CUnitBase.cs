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
		Alive,			// �پ��� Ȱ���� �Ҽ� �ִ�.
		WaitingForDeath,	// �״� ������ ���۵Ǿ��� (���ϸ��̼ǵ�)
		Death,			// �׾����Ƿ� �Է��� �Ұ����ϴ�.
		Leave,			// ���� ���� ��Ȱ�� ���¿��� �����
		Remove,			// ���ӿ��� ���ŵ� �����̴� (�޸� Ǯ�� �� ����)
	}


	public enum EUnitControlType
	{
		None,
		PlayerControl,		// �÷��̾� ���� ����
		PlayerAI,			// �Ʊ� AI�� ���� ���� 
		PlayerPossess,		// �Ʊ� AI�� ���� ����, �������� ���� �Ʊ��� �� ���
		EnemyAI,				// ���� AI�� ���� ���� 
		EnemyPossess,			// ���� AI�� ���� ����, �������� ���� ������ �� ���
	}

	protected EUnitControlType m_eUnitControlType = EUnitControlType.None;  public EUnitControlType GetUnitControlType() { return m_eUnitControlType; }
	protected EUnitState m_eUnitState = EUnitState.None;  public EUnitState GetUnitState() { return m_eUnitState; }
	protected CUnitSocketAssistBase m_pSocketAssist = null;
	protected bool m_bAlive = true;			public bool IsAlive { get { return m_bAlive; } }
	private bool m_bDynamicLoad = false;		public bool IsDynamicLoad { get { return m_bDynamicLoad; } } // ���̺� ���� �������� �ε��� ��ü. false�� Scene���� �Ҵ��
	private uint m_hUnitID = 0;				public uint GetUnitID() { return m_hUnitID; }			   // ���̺� �ĺ���
	private uint m_hUnitSessionID = 0;			public uint GetUnitSessionID() { return m_hUnitSessionID; }  // DB �ĺ���. ����ũ�ϴ�. 
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
