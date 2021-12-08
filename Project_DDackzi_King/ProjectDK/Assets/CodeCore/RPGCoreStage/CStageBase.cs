using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStageBase : CMonoBase
{
	[SerializeField]  // 로드되면 자동으로 선택되는 스테이지 
	private bool DefaultStage = false;  public bool IsDefaultStage { get { return DefaultStage; } }

	//----------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		if (CManagerStageBase.Instance != null)
		{
			CManagerStageBase.Instance.ImportStage(this, true);
		}
	}

	protected override void OnUnityDestroy()
	{
		base.OnUnityDestroy();
		if (CManagerStageBase.Instance != null)
		{
			CManagerStageBase.Instance.ImportStage(this, false);
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

	//--------------------------------------------------------------
	private void PrivStageReset()
	{
		CManagerUnitBase.Instance.DoMgrUnitClearAll();
	}

	//---------------------------------------------------------------
	internal void ImportStageInitialize()
	{
		OnStageInitialize();
	}

	internal void ImportStageEnter()
	{
		OnStageEnter();
	}

	internal void ImportStageOut()
	{
		OnStageExit();
	}

	protected virtual void OnStageInitialize() { }
	protected virtual void OnStageEnter() { }
	protected virtual void OnStageExit() { }
	protected virtual void OnStageReset() { }
}
