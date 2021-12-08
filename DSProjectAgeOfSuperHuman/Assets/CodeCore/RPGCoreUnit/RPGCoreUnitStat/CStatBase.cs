using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStatOwner
{
	public void			IStatUpdate(uint hStatType, float fStatValue);
	public uint			IStatLevel();
	public CStatBase		IStatFind(uint hStatType);
	public float			IStatValue(uint hStatType);
}

abstract public class CStatBase
{
	protected uint m_hStatType = 0;
	private float m_fStatBasic = 0;   // 레벨 테이블 기본 값 + 장비 값등 	
	private float m_fStatValue = 0;   // %버프등이 합산된 최종 적용 값
	private float m_fStatConstant = 0;  // 계산시 주어지는 상수 값. 변경하면 안되는 상수 값(기본 명중률 95%등)
	protected IStatOwner m_pStatOwner = null;

	private List<CStatBase> m_listChainStat = new List<CStatBase>(); // 이 스텟과 연관된 다른 스텟 
	private LinkedList<CStatModifierBase> m_listModifier = new LinkedList<CStatModifierBase>(); // 장비나 버프등에 의한 수정자 

	//------------------------------------------------------------------
	public void DoStatModifierRemove(CStatModifierBase pModifier)
	{
		m_listModifier.Remove(pModifier);
		PrivStatValueRefresh();
	}

	public void DoStatModifierAdd(CStatModifierBase pModifier)
	{
		if (m_listModifier.Find(pModifier) != null) return;

		m_listModifier.AddLast(pModifier);
		PrivStatValueRefresh();
	}

	public void DoStatValueBasicAdd(float fValueAdd) // 레벨업등으로 기본 스텟 증가시
	{
		if (fValueAdd == 0) return;
		m_fStatBasic += fValueAdd;
		PrivStatValueRefresh();
	}

	public void DoStatValueBasicReset(float fValueReset, IStatOwner pStatOwner = null) // 특정 값으로 초기화
	{
		m_fStatBasic = fValueReset;
		if (pStatOwner != null)
		{
			m_pStatOwner = pStatOwner;
		}
		m_listModifier.Clear();
		PrivStatValueRefresh();
	}

	public void DoStatValueConstant(float fValue)
	{
		m_fStatConstant = fValue;
		PrivStatValueRefresh();
	}

	public void DoStatValueRefresh()
	{
		PrivStatValueRefresh();
	}

	public uint GetStatType() { return m_hStatType; }

	public float GetStatValueBasic() { return m_fStatBasic + m_fStatConstant; }

	public float GetStatValue() { return m_fStatValue; }

	public void ImportStatValueChain(uint hStatID, float fValue)
	{
		OnStatValueChain(hStatID, fValue);
	}

	public void SetStatValueChain(CStatBase pStat)
	{
		m_listChainStat.Add(pStat);
	}

	public void DoStatValueClearChain()
	{
		m_listChainStat.Clear();
	}

	static public implicit operator float(CStatBase pStat)
	{
		return pStat.m_fStatValue;
	}
	//-----------------------------------------------------------------
	private void PrivStatValueRefresh()
	{
		float fValue = m_fStatBasic + m_fStatConstant;

		foreach (CStatModifierBase pModifier in m_listModifier)
		{
			fValue += pModifier.Value;
		}

		PrivStatValueApply(fValue);
	}

	private void PrivStatValueApply(float fValue)
	{
		m_fStatValue = fValue;
		PrivStatValueNotifyChain(m_hStatType, m_fStatValue);
		m_pStatOwner?.IStatUpdate(m_hStatType, m_fStatValue);
	}

	private void PrivStatValueNotifyChain(uint hStatID, float fValue)
	{
		for (int i = 0; i < m_listChainStat.Count; i++)
		{
			m_listChainStat[i].ImportStatValueChain(hStatID, fValue);
		}
		OnStatValueRefresh(fValue);
	}

	//------------------------------------------------------------------
	protected virtual void OnStatValueRefresh(float fStatValue)			    { } // 나의 값이 변경되었을때 하부 통지 
	protected virtual void OnStatValueChain(uint hStatType, float fApplyValue) { } // 나와 연관된 스텟이 변경되었을때 통지. 여기서 나의 스텟을 계산
	
}
