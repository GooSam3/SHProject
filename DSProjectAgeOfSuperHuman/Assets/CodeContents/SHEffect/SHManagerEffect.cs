using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHManagerEffect : CManagerEffectBase
{	public static new SHManagerEffect Instance { get { return CManagerEffectBase.Instance as SHManagerEffect; } }

	private List<string> m_listLoadEffect = new List<string>();
	//-------------------------------------------------------------------------------
	protected override void OnEffectRemove(CEffectBase pEffect)
	{
		base.OnEffectRemove(pEffect);
		SHManagerPrefabPool.Instance.Return(pEffect.gameObject);
	}

	//--------------------------------------------------------------------------------
	public void DoMgrEffectPreLoadListAdd(string strEffectName)
	{
		if (m_listLoadEffect.Contains(strEffectName) == false)
		{
			m_listLoadEffect.Add(strEffectName);
		}
	}

	public void DoMgrEffectPreLoadListStart(UnityAction delFinish)
	{
		if (m_listLoadEffect.Count == 0)
		{
			delFinish?.Invoke();
			return;
		}

		for (int i = 0; i < m_listLoadEffect.Count; i++)
		{
			if (i == m_listLoadEffect.Count - 1)
			{
				SHManagerPrefabPool.Instance.LoadInstance(EPoolType.Effect, m_listLoadEffect[i], ()=> {
					delFinish?.Invoke();
					m_listLoadEffect.Clear();
				});
			}
			else
			{
				SHManagerPrefabPool.Instance.LoadInstance(EPoolType.Effect, m_listLoadEffect[i], null);
			}
		}
	}

	public void DoMgrEffectRigist<INSTANCE>(string strEffectName, UnityAction<INSTANCE> delFinish) where INSTANCE : CEffectBase
	{
		SHManagerPrefabPool.Instance.LoadComponent<INSTANCE>(EPoolType.Effect, strEffectName, (INSTANCE pInstance) => {
			ProtMgrEffectRegist(pInstance);
			delFinish(pInstance);
		});
	}
}
