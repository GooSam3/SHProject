using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetSpineHero : CUIWidgetSpineController
{
	public enum EHeroAnimation
	{
		idle,
		idle2,
		idle3,
	}
	[SerializeField]
	private float AniRefresh = 5.0f;

	private EHeroAnimation m_eHeroAnimation = EHeroAnimation.idle;
	//---------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		InvokeRepeating("PrivSpineHeroRandomAnimation", AniRefresh, AniRefresh);
	}

	protected override void OnSpineControllerAnimationEnd(string strAniName)
	{
		base.OnSpineControllerAnimationEnd(strAniName);
		ProtSpineControllerAnimation(EHeroAnimation.idle.ToString());
	}

	//---------------------------------------------------------
	private EHeroAnimation ExtractSpineHeroRandomAnimation(EHeroAnimation eExclusive)
	{
		EHeroAnimation eResult = 0;
		int iMax = (int)EHeroAnimation.idle3 + 1;
		while(true)
		{
			int iRandom = Random.Range(0, iMax);			
			if (iRandom != (int)eExclusive)
			{
				eResult = (EHeroAnimation)iRandom;
				break;
			}
		}

		return eResult;
	}

	private void PrivSpineHeroRandomAnimation()
	{
		m_eHeroAnimation = ExtractSpineHeroRandomAnimation(m_eHeroAnimation);
		ProtSpineControllerAnimation(m_eHeroAnimation.ToString(), false);
	}

	//--------------------------------------------------------
}
