using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CEffectParticleBase : CEffectBase
{
    private List<ParticleSystem> m_listParticleSystem = new List<ParticleSystem>();
	//---------------------------------------------------------------
	protected override void OnUnityUpdate()
	{
		base.OnUnityUpdate();
		UpdeteEffectParticleExpire();
		
	}


	protected override void OnEffectInitialize()
	{
		base.OnEffectInitialize();
		GetComponentsInChildren(true, m_listParticleSystem);
		for (int i = 0; i < m_listParticleSystem.Count; i++)
		{
			m_listParticleSystem[i].gameObject.SetActive(false);

		}
	}

	protected override void OnEffectStartTransform(Transform pFollow, Vector3 vecDest, float fDuration, params object[] aParams)
	{
		base.OnEffectStartTransform(pFollow, vecDest, fDuration, aParams);

		for (int i = 0; i < m_listParticleSystem.Count; i++)
		{
			m_listParticleSystem[i].gameObject.SetActive(true);
			m_listParticleSystem[i].Play();
		}
	}

	protected override void OnEffectStartWorldPosition(Vector3 vecOrigin, Vector3 vecDest, float fDuration, params object[] aParams)
	{
		base.OnEffectStartWorldPosition(vecOrigin, vecDest, fDuration, aParams);
		for (int i = 0; i < m_listParticleSystem.Count; i++)
		{
			m_listParticleSystem[i].gameObject.SetActive(true);
			m_listParticleSystem[i].Play();
		}
	}

	protected override void OnEffectEnd(bool bForce)
	{
		base.OnEffectEnd(bForce);
		for (int i = 0; i < m_listParticleSystem.Count; i++)
		{
			m_listParticleSystem[i].Stop();
		}
	}

	protected override void OnEffectStartSingle(float fDuration, params object[] aParams)
	{
		base.OnEffectStartSingle(fDuration, aParams);
		for (int i = 0; i < m_listParticleSystem.Count; i++)
		{
			m_listParticleSystem[i].gameObject.SetActive(true);
			m_listParticleSystem[i].Play();
		}
	}

	//-----------------------------------------------------------------
	private void UpdeteEffectParticleExpire()
	{
		bool bExpire = true;
		for (int i = 0; i < m_listParticleSystem.Count; i++)
		{
			if (m_listParticleSystem[i].isPlaying)
			{
				bExpire = false;
				break;
			}
		}

		if (bExpire)
		{
			DoEffectEnd();
		}
	}
}
