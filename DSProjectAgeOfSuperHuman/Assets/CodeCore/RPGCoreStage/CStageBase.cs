using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
abstract public class CStageBase : CMonoBase
{
	[SerializeField]  // 로드되면 자동으로 선택되는 스테이지 
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

	public void DoStageEnd() // 스테이지 종료 후 다음 스테이지 스타트 
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
	internal void ImportStageInitialize()  // 모든 스테이지가 메모리에 로딩될따
	{
		OnStageInitialize();
	}

	internal void ImportStageEnter(uint hStageID)		// 스테이지가 시작될때
	{
		m_hStageID = hStageID;
		SetMonoActive(true);
		PrivStageLoading(hStageID);
		OnStageEnter(hStageID);
	}

	internal void ImportStageExit()			// 스테이지가 끝날때. 사용한 리소스등을 반납
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
