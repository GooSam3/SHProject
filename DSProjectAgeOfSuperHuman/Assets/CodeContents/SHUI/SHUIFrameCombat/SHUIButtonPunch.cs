using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
public class SHUIButtonPunch : SHUIButtonNormal
{
	[SerializeField]
	private bool Left = false;

	[SerializeField]
	private GameObject PunchTweenObj = null;

	[SerializeField]
	private ParticleSystem PunchEffect = null;

	private List<uTweener> m_listTween = new List<uTweener>();
	//------------------------------------------------
	private void Update()
	{
		if (Left)
		{
			if (Input.GetKeyDown(KeyCode.Z))
			{
				ProtButtonActionPress();
				
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.C))
			{
				ProtButtonActionPress();
			}
		}
	}
	//----------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		PunchTweenObj.GetComponents(m_listTween);
		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].enabled = false;
		}
		PunchEffect?.Stop();
	}

	protected override void OnButtonClick()
	{
		base.OnButtonClick();
		PrivButtonPunchTweenPlay();
	}
	//---------------------------------------------------
	public void DoButtonPunch()
	{
		ProtButtonActionPress();
	}

	//-----------------------------------------------------
	private void PrivButtonPunchTweenPlay()
	{
		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].enabled = true;
			m_listTween[i].ResetPlay();
		}
		PunchEffect?.Play();
	}
	
	//--------------------------------------------------------
	public void HandleButtonPunchTweenFinish()
	{
		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].Sample(0, false);
			m_listTween[i].enabled = false;
		}
	}
}
