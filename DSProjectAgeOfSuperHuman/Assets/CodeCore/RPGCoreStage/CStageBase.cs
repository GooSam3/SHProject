using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
abstract public class CStageBase : CMonoBase
{
	[SerializeField]  // �ε�Ǹ� �ڵ����� ���õǴ� �������� 
	private bool DefaultStage = false;  public bool IsDefaultStage { get { return DefaultStage; } }
	private uint m_hStageID = 0;		public uint pStageID { get { return m_hStageID; } }
	//----------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		if (CManagerStageBase.Instance != null)
		{
			CManagerStageBase.Instance.ImportStageRegist(this, true);
		}
	}

	protected override void OnUnityDestroy()
	{
		base.OnUnityDestroy();
		if (CManagerStageBase.Instance != null)
		{
			CManagerStageBase.Instance.ImportStageRegist(this, false);
		}
	}
	//-------------------------------------------------------------
	public void DoStageStart(bool bReset = false)
	{
		if (bReset)
		{
			PrivStageReset();
		}
	}

	public void DoStageEnd() // �������� ���� �� ���� �������� ��ŸƮ 
	{
		OnStageEnd();
	}

	//--------------------------------------------------------------
	private void PrivStageReset()
	{
		OnStageReset();
	}

	private void PrivStageLoading(uint hStageID)
	{
		OnStageLoading(hStageID, () =>
		{
			OnStageStart();
		});
	}

	//---------------------------------------------------------------
	internal void ImportStageInitialize()  // ��� ���������� �޸𸮿� �ε��ɵ�
	{
		OnStageInitialize();
	}

	internal void ImportStageEnter(uint hStageID)		// ���������� ���۵ɶ�
	{
		m_hStageID = hStageID;
		SetMonoActive(true);
		PrivStageLoading(hStageID);
		OnStageEnter(hStageID);
	}

	internal void ImportStageExit()			// ���������� ������. ����� ���ҽ����� �ݳ�
	{
		SetMonoActive(false);
		OnStageExit();
	}

	protected virtual void OnStageInitialize() { }
	protected virtual void OnStageEnter(uint hStageID) { }
	protected virtual void OnStageExit() { }
	protected virtual void OnStageReset() { }
	protected virtual void OnStageLoading(uint hStageID, UnityAction delFinish) { }
	protected virtual void OnStageStart() { }
	protected virtual void OnStageEnd() { }
}
