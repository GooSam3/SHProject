using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitSpriteBase : CUnitAIBase
{
	private Animator m_pAnimator = null;
	private SpriteRenderer m_pSpriteRenderer = null;
	private AnimationClip m_pCurrentAnimClip = null;
	private AnimatorStateInfo m_sIdleStateInfo;
	private float m_fDurationUpdate = 0;
	private bool m_bDurationAnimation = false;
	private Dictionary<string, AnimationClip> m_mapAnimClip = new Dictionary<string, AnimationClip>();
	//----------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		SetNavAgenRotationSpeed(0);
		m_pSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		m_pAnimator = GetComponentInChildren<Animator>();

		AnimationClip[] aAnimation = m_pAnimator.runtimeAnimatorController.animationClips;
		for (int i = 0; i < aAnimation.Length; i++)
		{
			m_mapAnimClip.Add(aAnimation[i].name, aAnimation[i]);
		}

		m_sIdleStateInfo = m_pAnimator.GetCurrentAnimatorStateInfo(0);
	}

	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();

		if (m_bDurationAnimation)
		{
			m_fDurationUpdate -= Time.deltaTime;

			if (m_fDurationUpdate <= 0)
			{
				ProtUnitSpriteLoopEnd();
			}
		}

		m_pSpriteRenderer.sortingOrder = -(int)(transform.position.z * 100f);
	}

	protected override void OnUnitSkillAnimation(string strAnimName, bool bLoop, float fDuration, float fAniSpeed)
	{
		ProtSpritePlayAnimation(strAnimName, bLoop, fDuration, fAniSpeed);
	}

	//------------------------------------------------------
	protected void ProtSpriteFlip(bool bAxisX, bool bFlip)
	{
		if (bAxisX) //케릭터의 트랜스폼을 돌려야 소켓이 유지된다.
		{
			if (bFlip)
			{
				transform.eulerAngles = new Vector3(0f, 0f, 180f);
			}
			else
			{
				transform.eulerAngles = Vector3.zero;
			}
		}
		else
		{

		}
	}

	protected void ProtSpritePlayAnimation(string strAnimName, bool bLoop, float fDuration = 0f, float fPlaySpeed = 1f)
	{
		if (m_mapAnimClip.ContainsKey(strAnimName))
		{			
			AnimationClip pClip = m_mapAnimClip[strAnimName];
			if (pClip == m_pCurrentAnimClip && bLoop == true)
			{
				return;
			}

			m_pCurrentAnimClip = pClip;
			if (bLoop)
			{
				pClip.wrapMode = WrapMode.Loop;
			}
			else
			{
				pClip.wrapMode = WrapMode.Once;
			}

			if (fDuration > 0)
			{
				m_bDurationAnimation = true;
				m_fDurationUpdate = fDuration;
			}

			m_pAnimator.enabled = true;
			m_pAnimator.speed = fPlaySpeed;
			m_pAnimator.Play(strAnimName, -1, 0);
		}
		else
		{
			Debug.LogError($"[Animation] Invalid Animation Name {strAnimName}");
		}
	}

	protected string ProtUnitSpriteGetCurrentAnimation()
	{
		string strAnimName = "None";
		if (m_pCurrentAnimClip != null)
		{
			strAnimName = m_pCurrentAnimClip.name;
		}

		return strAnimName;
	}

	protected void ProtUnitSpriteLoopEnd(bool bAnimationReset = true)
	{
		m_fDurationUpdate = 0;
		m_bDurationAnimation = false;

		if (m_pCurrentAnimClip != null)
		{
			if (bAnimationReset)
			{
				m_pAnimator.Play(m_sIdleStateInfo.fullPathHash, 0);
			}
			OnUnitSpriteAnimationLoopEnd(m_pCurrentAnimClip.name);
		}
	}

	//-------------------------------------------------------------------------------


	//---------------------------------------------------------------------------------------------
	protected virtual void OnUnitSpriteAnimationLoopEnd(string strAnimName) { }

}
