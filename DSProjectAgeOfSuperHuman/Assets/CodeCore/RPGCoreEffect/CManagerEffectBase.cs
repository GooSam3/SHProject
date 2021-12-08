using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CManagerEffectBase : CManagerTemplateBase<CManagerEffectBase>
{
	private LinkedList<CEffectBase> m_listEffect = new LinkedList<CEffectBase>();
	private List<CEffectBase> m_listDeleteNote = new List<CEffectBase>(20);
	//-------------------------------------------------------------
	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		PrivEffectUpdate();
	}

	//----------------------------------------------------------------
	protected void ProtMgrEffectRegist(CEffectBase pEffect)
	{
		if (m_listEffect.Contains(pEffect)) return;
		pEffect.transform.SetParent(transform, false);
		m_listEffect.AddLast(pEffect);
	}

	//----------------------------------------------------------------
	private void PrivEffectUpdate()
	{
		m_listDeleteNote.Clear();
		LinkedList<CEffectBase>.Enumerator it = m_listEffect.GetEnumerator();
		while(it.MoveNext())
		{
			if(it.Current.IsActive == false)
			{
				m_listDeleteNote.Add(it.Current);
			}
		}

		for (int i = 0; i < m_listDeleteNote.Count; i++)
		{
			OnEffectRemove(m_listDeleteNote[i]);
			m_listEffect.Remove(m_listDeleteNote[i]);
		}
	}
	
	//----------------------------------------------------------------
	protected virtual void OnEffectRemove(CEffectBase pEffect) { }
}
