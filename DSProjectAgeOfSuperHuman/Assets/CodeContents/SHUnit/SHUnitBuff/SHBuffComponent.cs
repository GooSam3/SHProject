using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NBuff;
using UnityEngine.Events;
public class SHBuffComponent : CBuffComponentBase
{
	private UnityAction<bool, uint> m_delBuffNotify = null;
	//------------------------------------------------------------------
	protected override void OnBuffComponentBuffStart(CBuffBase pBuff)
	{
		base.OnBuffComponentBuffStart(pBuff);
		m_delBuffNotify?.Invoke(true, pBuff.GetBuffID());
	}

	protected override void OnBuffComponentBuffRemove(CBuffBase pBuff)
	{
		base.OnBuffComponentBuffRemove(pBuff);
		m_delBuffNotify?.Invoke(false, pBuff.GetBuffID());
	}


	//------------------------------------------------------------------
	public void DoBuffComponentTo(uint hBuffID, SHUnitBase pUnitTo)
	{
		SHBuffInstance pBuff = SHManagerScriptData.Instance.DoLoadBuff(hBuffID);
		ProtBuffComponentTaskEvent((uint)EBuffTaskEventType.BuffTo, pBuff, pUnitTo);
	}

	public void DoBuffComponentFrom(uint hBuffID, SHBuffComponent pUnitFrom, float fDuration, float fPower)
	{
		SHBuffInstance pBuff = SHManagerScriptData.Instance.DoLoadBuff(hBuffID);
		ProtBuffComponentBuffStart(pBuff, pUnitFrom , fDuration, fPower);
		ProtBuffComponentTaskEvent((uint)EBuffTaskEventType.BuffFrom, pBuff, pUnitFrom);
	}

	public void DoBuffComponentEnd(uint hBuffID)
	{
		ProtBuffComponentBuffEnd(hBuffID);
	}

	public void DoBuffComponentClear()
	{
		ProtBuffComponentClearAll();
	}

	public void DoBuffComponentNotify(UnityAction<bool, uint> delNotify)
	{
		m_delBuffNotify = delNotify;

		LinkedList<CBuffBase>.Enumerator it = m_listBuffInstance.GetEnumerator();
		while (it.MoveNext())
		{
			m_delBuffNotify?.Invoke(true, it.Current.GetBuffID());
		}
	}
	
	public EBuffType HasBuffCrowdControll()
	{
		EBuffType eBuffType = EBuffType.None;
		LinkedList<CBuffBase>.Enumerator it = m_listBuffInstance.GetEnumerator();
		while(it.MoveNext())
		{
			SHBuffInstance pBuff = it.Current as SHBuffInstance;
			if (pBuff.GetSHBuffType() == EBuffType.CrowdControl)
			{
				eBuffType = EBuffType.CrowdControl;
				break;
			}
		}

		return eBuffType;
	}

}
