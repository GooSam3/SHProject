using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHUnitTagTween : CMonoBase
{
	[SerializeField]
	private Jun_TweenRuntime TweenInstance;

	[SerializeField]
	private Vector3 TweenPositionLeft = Vector3.zero;

	[SerializeField]
	private Vector3 TweenPositionRight = Vector3.zero;

	[SerializeField]
	private Vector3 TweenPositionCenter = Vector3.zero;

	private UnityAction m_delFinish = null;
	private EUnitTagPosition m_eTagTypeCurrent = EUnitTagPosition.Center;   public EUnitTagPosition GetUnitTagTweenCurrentPosition() { return m_eTagTypeCurrent; } 
	private bool m_bTweenStart = false;
	//--------------------------------------------------------------------------
	public void DoUnitTagTweenStart(Transform pTweenObject, EUnitTagPosition eTagType, bool bForce, UnityAction delFinish)
	{
		if (m_bTweenStart) return;
		m_bTweenStart = true;
		m_delFinish = delFinish;
		
		bool bFoward = true;

		Vector3 vecStart = TweenPositionCenter;
		Vector3 vecEnd = Vector3.zero;

		if (m_eTagTypeCurrent == EUnitTagPosition.Center)
		{
			vecStart = TweenPositionCenter;
			bFoward = true;
			if (eTagType == EUnitTagPosition.Center)
			{
				HandleUnitTagTweenFinish();
				return;
			}
			else if (eTagType == EUnitTagPosition.Left)
			{
				vecEnd = TweenPositionLeft;
			}
			else if (eTagType == EUnitTagPosition.Right)
			{
				vecEnd = TweenPositionRight;
			}
		}
		else if (m_eTagTypeCurrent == EUnitTagPosition.Left)
		{
			vecStart = TweenPositionLeft;
			bFoward = false;

			if (eTagType == EUnitTagPosition.Center)
			{
				vecEnd = TweenPositionCenter;
			}
			else if (eTagType == EUnitTagPosition.Left)
			{
				HandleUnitTagTweenFinish();
				return;
			}
			else if (eTagType == EUnitTagPosition.Right)
			{
				HandleUnitTagTweenFinish();
				return;
			}
		}
		else if (m_eTagTypeCurrent == EUnitTagPosition.Right)
		{
			vecStart = TweenPositionRight;
			bFoward = false;

			if (eTagType == EUnitTagPosition.Center)
			{
				vecEnd = TweenPositionCenter;
			}
			else if (eTagType == EUnitTagPosition.Left)
			{
				HandleUnitTagTweenFinish();
				return;
			}
			else if (eTagType == EUnitTagPosition.Right)
			{
				HandleUnitTagTweenFinish();
				return;
			}
		}
		PrivTagTweenObject(pTweenObject);
		PrivTagTweenStart(vecStart, vecEnd, bFoward, bForce);
		m_eTagTypeCurrent = eTagType;
	}

	public void DoUnitTagTweenForceEnd()
	{
		TweenInstance.StopPlay();
		m_delFinish = null;
	}

	//--------------------------------------------------------------------------
	private void PrivTagTweenStart(Vector3 vecPositionStart, Vector3 vecPositionEnd, bool bForward, bool bForce)
	{
		if (bForward)
		{
			TweenInstance.firstTween.fromVector = vecPositionStart;
			TweenInstance.firstTween.toVector = vecPositionEnd;

			if (bForce)
			{
				TweenInstance.PlayAtTime(1);
			}
			else
			{
				TweenInstance.Play();
			}
		}
		else
		{
			TweenInstance.firstTween.fromVector = vecPositionEnd;
			TweenInstance.firstTween.toVector = vecPositionStart;

			if (bForce)
			{
				TweenInstance.PlayAtTime(0);
			}
			else
			{
				TweenInstance.Rewind();
			}
		}
	}

	private void PrivTagTweenObject(Transform pTweenObject)
	{
		int Count = 0;
	
		while(true)
		{
			Jun_Tween pTween = TweenInstance.GetTween(Count++);
			if (pTween == null)
			{
				break;
			}
			else
			{
				pTween.Init(pTweenObject);
			}
		}
	}

	//--------------------------------------------------------------------------
	public void HandleUnitTagTweenFinish()
	{
		m_bTweenStart = false;
		m_delFinish?.Invoke();
	}
}
