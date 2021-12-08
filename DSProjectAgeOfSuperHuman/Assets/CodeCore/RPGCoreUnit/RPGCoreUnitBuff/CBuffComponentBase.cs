using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public abstract class CBuffComponentBase : CMonoBase
{
	private CUnitBuffBase m_pOwner = null; public CUnitBuffBase GetBuffOwner() { return m_pOwner; }
	private CStatComponentBase m_pOwnerStat = null;
	private List<CBuffBase> m_listBuffDelete = new List<CBuffBase>();
	private List<CBuffBase> m_listNote = new List<CBuffBase>(32); // GC를 방지하기 위한 이터레이션 전용. 테스크에 의해 이터레이션중에 배열이 변경될 수 있다.
	protected LinkedList<CBuffBase> m_listBuffInstance = new LinkedList<CBuffBase>();
	//------------------------------------------------------------------------------------------------
	private void Update()
	{
		UpdateBuffComponent(Time.deltaTime);
		OnUnityUpdate();
	}

	private void LateUpdate()
	{
		PrivBuffComponentDelete();
	}
	//--------------------------------------------------------------------------------------------------
	public void ImportBuffComponentExpire(CBuffBase pBuffExpire)
	{
		m_listBuffDelete.Add(pBuffExpire);
		OnBuffComponentBuffExpire(pBuffExpire);
	}

	public void ImportBuffComponentStackLower(CBuffBase pBuffStackLower)
	{
		m_listBuffDelete.Add(pBuffStackLower);
		OnBuffComponentBuffStackLower(pBuffStackLower);
	}

	public void ImportBuffComponentInitialize(CUnitBuffBase pOwner, CStatComponentBase pOwnerStat)
	{
		m_pOwner = pOwner;
		m_pOwnerStat = pOwnerStat;
		OnBuffComponentInitialize(pOwner, pOwnerStat);
	}

	//---------------------------------------------------------------------------------------------------
	public CStatComponentBase ExtractStatComponent() { return m_pOwnerStat; }

	//--------------------------------------------------------------------------------------------------
	protected CBuffBase FindBuff(uint hBuffID)
	{
		CBuffBase pFindBuff = null;
		LinkedList<CBuffBase>.Enumerator it = m_listBuffInstance.GetEnumerator();
		while (it.MoveNext())
		{
			if (it.Current.GetBuffID() == hBuffID)
			{
				pFindBuff = it.Current;
				break;
			}
		}
		return pFindBuff;
	}

	protected void ProtBuffComponentTaskEvent(uint eEventType, params object[] aParams)
	{
		LinkedList<CBuffBase>.Enumerator it = m_listBuffInstance.GetEnumerator();
		while (it.MoveNext())
		{
			m_listNote.Add(it.Current);
		}

		for (int i = 0; i < m_listNote.Count; i++)
		{
			m_listNote[i].DoBuffEvent(eEventType, aParams);
		}
		m_listNote.Clear();
	}

	protected CBuffBase ProtBuffComponentBuffStart(CBuffBase pBuffInstance, CBuffComponentBase pBuffOrigin, float fDuration, float fBuffPower)
	{
		CBuffBase pFinalBuff = null;
		CBuffBase pStackBuff = FindBuff(pBuffInstance.GetBuffID());
		if (pStackBuff != null)
		{
			pFinalBuff = PrivBuffComponentMergeBuff(pStackBuff, pBuffInstance, pBuffOrigin, fDuration, fBuffPower);
		}
		else
		{
			PrivBuffComponentStart(pBuffInstance, pBuffOrigin, fDuration, fBuffPower);
			pFinalBuff = pBuffInstance;
		}

		OnBuffComponentBuffStart(pFinalBuff);
		return pFinalBuff;
	}

	protected void ProtBuffComponentBuffEnd(uint hBuffID)
	{
		CBuffBase pBuff = FindBuff(hBuffID);
		if (pBuff != null)
		{
			m_listBuffDelete.Add(pBuff);
		}
	}


	protected void ProtBuffComponentClearAll()
	{
		m_listBuffDelete.Clear();
		m_listNote.Clear();
		m_listBuffInstance.Clear();
	}
	//-------------------------------------------------------------------------------------------------
	public void DoBuffComponentForceUpdate(float fDelta)
	{
		UpdateBuffComponent(fDelta);
	}

	//--------------------------------------------------------------------------------------------------
	private CBuffBase PrivBuffComponentMergeBuff(CBuffBase pBuffStack, CBuffBase pBuffNew, CBuffComponentBase pBuffOrigin, float fDuration, float fBuffPower)
	{
		if (pBuffNew.IsExclusive)
		{
			PrivBuffComponentRemove(pBuffStack);
			PrivBuffComponentStart(pBuffNew, pBuffOrigin, fDuration, fBuffPower);
			return pBuffNew;
		}

		if (pBuffNew.IsTimeReset)
		{
			pBuffStack.ImportBuffTimeReset(fDuration);
		}

		if (pBuffNew.IsPowerUp)
		{
			pBuffStack.ImportBuffPowerUp(fBuffPower);
		}

		if (pBuffNew.IsCountUp)
		{
			pBuffStack.ImportBuffCountUp(pBuffNew.GetBuffStackCount());
		}
		return pBuffStack;
	}

	private void PrivBuffComponentDelete()
	{
		for (int i = 0; i < m_listBuffDelete.Count; i++)
		{
			PrivBuffComponentRemove(m_listBuffDelete[i]);
		}
		m_listBuffDelete.Clear();
	}

	private void PrivBuffComponentRemove(CBuffBase pBuff)
	{
		m_listBuffInstance.Remove(pBuff);
		pBuff.DoBuffEnd();
		OnBuffComponentBuffRemove(pBuff);
	}

	private void PrivBuffComponentStart(CBuffBase pBuff, CBuffComponentBase pBuffOrigin, float fDuration, float fBuffPower)
	{
		m_listBuffInstance.AddLast(pBuff);
		pBuff.DoBuffStart(this, pBuffOrigin, fDuration, fBuffPower);
	}

	private void UpdateBuffComponent(float fDeltaTime)
	{
		LinkedList<CBuffBase>.Enumerator it = m_listBuffInstance.GetEnumerator();
		while (it.MoveNext())
		{
			it.Current.UpdateBuff(fDeltaTime);
		}
	}

	//---------------------------------------------------------------
	protected virtual void OnUnityUpdate() { }
	protected virtual void OnBuffComponentBuffStart(CBuffBase pBuff) { }
	protected virtual void OnBuffComponentBuffExpire(CBuffBase pBuff) { }
	protected virtual void OnBuffComponentBuffRemove(CBuffBase pBuff) { }
	protected virtual void OnBuffComponentBuffStackLower(CBuffBase pBuff) { }
	protected virtual void OnBuffProcessBuffMerge(CBuffBase pBuffOrigin, CBuffBase pBuffNew) { }
	protected virtual void OnBuffComponentInitialize(CUnitBuffBase pOwner, CStatComponentBase pOwnerStat) { }
	
}
