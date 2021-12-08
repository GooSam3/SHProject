using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHStageMovingControl : CMonoBase
{
	[SerializeField]
	private SpriteRenderer StageMoveGround = null;

	private UnityAction m_delFinish = null;
	private List<Jun_TweenRuntime> m_listTween = new List<Jun_TweenRuntime>();
	//------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		SetMonoActive(false);
		GetComponentsInChildren(true, m_listTween);
	}

	public void DoStageMovingStart(Sprite pBackImage, UnityAction delFinish)
	{
		m_delFinish = delFinish;
		SetMonoActive(true);
		StageMoveGround.sprite = pBackImage;
		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].enabled = true;
			m_listTween[i].enablePlay = true;
			m_listTween[i].Play();
		}
	}

	//--------------------------------------------------
	public void HandleMovingControllFinish()
	{
		SetMonoActive(false);
		m_delFinish?.Invoke();
	}
}
