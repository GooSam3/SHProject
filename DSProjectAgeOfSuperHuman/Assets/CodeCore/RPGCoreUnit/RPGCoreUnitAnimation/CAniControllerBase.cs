using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CAniControllerBase : CMonoBase
{
	public enum EAnimationActionEnd
	{		
		Idle,
		IdleAllAnimation,
		Hide,
	}

	public enum EAnimationActionStart
	{
		None,
		HideOtherGroup,
	}


	[SerializeField]
	private string AniGroupName = "None"; public string GetAniGroupName() { return AniGroupName; }

	[SerializeField]
	private EAnimationActionStart AniActionStart = EAnimationActionStart.None;

	[SerializeField]
	private EAnimationActionEnd AniActionEnd = EAnimationActionEnd.Idle;

	protected CAnimationBase m_pAnimationOwner = null;
	private UnityAction<string, bool>			m_delFinish = null;
	private UnityAction<string, int, float>		m_delAniEvent = null;
	//-------------------------------------------------------------
	internal void ImportAniControllerInitialize(CAnimationBase pAnimationOwner)
	{
		m_pAnimationOwner = pAnimationOwner;
		OnAniControllerInitialize(pAnimationOwner);
	}

	internal void ImportAniControllerAnimationStart(CAnimationBase.SAnimationUsage rAnimUsage, UnityAction<string, bool> delFinish, UnityAction<string, int, float> delAniEvent)
	{
		SetMonoActive(true);
		m_delFinish = delFinish;
		m_delAniEvent = delAniEvent;

		switch(AniActionStart)
		{
			case EAnimationActionStart.HideOtherGroup:
				m_pAnimationOwner.ImportAnimationHideOther(AniGroupName);
				break;
		}

		OnAniControllerAnimationStart(rAnimUsage);
	}
	internal void ImportAniControllerShaderChange(CAnimationBase.SAnimationShaderInfo pShaderInfo)
	{
		OnAniControllerShaderChange(pShaderInfo);
	}

	internal void ImportAniControllerShaderReset()
	{
		OnAniControllerShaderReset();
	}

	internal void ImportAniControllerHide()
	{
		ProtAniControllerDelegateReset();
		SetMonoActive(false);
		OnAniControllerHide();
	}

	internal void ImportAniControllerIdle()
	{
		ProtAniControllerDelegateReset();
		SetMonoActive(true);
		OnAniControllerIdle();
	}

	internal void ImportAniControllerSkinChange(string strChangeAniName)
	{
		OnAniControllerSkinChange(strChangeAniName);
	}

	public virtual bool HasAnimation(string strAniName) { return false; } 

	private void Update()
	{
		OnUnityUpdate();
	}
	//-------------------------------------------------------------
	protected void ProtAniControllerFinish(string strAniName, bool bFinish)
	{
		m_delFinish?.Invoke(strAniName, bFinish);
		ProtAniControllerDelegateReset();

		switch(AniActionEnd)
		{
			case EAnimationActionEnd.Idle:
				ImportAniControllerIdle();
				break;
			case EAnimationActionEnd.IdleAllAnimation:
				m_pAnimationOwner.ImportAnimationAllGroupIdle();
				break;
			case EAnimationActionEnd.Hide:
				ImportAniControllerHide();
				break;
		}
	}

	protected void ProtAniControllerEvent(string strEventName, int iArg, float fArg)
	{
		m_delAniEvent?.Invoke(strEventName, iArg, fArg);
	}

	protected void ProtAniControllerDelegateReset()
	{
		m_delFinish = null;
		m_delAniEvent = null; 
	}

	//-------------------------------------------------------------
	protected virtual void OnAniControllerInitialize(CAnimationBase pAnimationOwner) { }
	protected virtual void OnAniControllerAnimationStart(CAnimationBase.SAnimationUsage rAnimUsage) { }
	protected virtual void OnAniControllerShaderChange(CAnimationBase.SAnimationShaderInfo pShaderInfo) { }
	protected virtual void OnAniControllerShaderReset() { }
	protected virtual void OnAniControllerHide() { }
	protected virtual void OnAniControllerIdle() { }
	protected virtual void OnAniControllerSkinChange(string strSkinName) { }
	protected virtual void OnUnityUpdate() { }
}
