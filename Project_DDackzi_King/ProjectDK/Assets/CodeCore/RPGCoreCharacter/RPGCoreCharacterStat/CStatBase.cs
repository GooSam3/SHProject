using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStatOwner
{
	public void IStatUpdate(int hStatType, float fStatValue);
	public void IStatDie(CUnitBase pKiller);
}

abstract public class CStatBase
{
	protected int m_hStatType = 0;			
	private float m_fStatDefault = 0; // ���۽� �־����� �⺻��
	private float m_fStatLimit = 0;   // Apply �Ѱ谪
	private float m_fStatValue = 0;   // ��� �������� �ջ�� ���� ���� ��
	private IStatOwner m_pStatOwner = null;

	private List<CStatBase> m_listChainStat = new List<CStatBase>(); // �� ���ݰ� ������ �ٸ� ���� 
	private LinkedList<CStatModifierBase> m_listModifier = new LinkedList<CStatModifierBase>(); // ��� ����� ���� ������ 

	//------------------------------------------------------------------
	public void DoStatModifierRemove(CStatModifierBase pModifier)
	{
		m_listModifier.Remove(pModifier);
		PrivStatValueRefresh();
	}

	public void DoStatModifierAdd(CStatModifierBase pModifier)
	{
		m_listModifier.AddLast(pModifier);
		PrivStatValueRefresh();
	}

	public void DoStatValueAdd(float fValueAdd) // ������������ �⺻ ���� ������
	{
		m_fStatDefault += fValueAdd;
		PrivStatValueRefresh();
	}

	public void DoStatValueReset(float fValueReset, float fLimitValue, IStatOwner pStatOwner) // Ư�� ������ �ʱ�ȭ
	{
		m_fStatDefault = fValueReset;

		m_fStatDefault = fValueReset;
		m_fStatLimit = fLimitValue;
		m_pStatOwner = pStatOwner;

		if (fValueReset > fLimitValue)
		{
			m_fStatLimit = fValueReset;
		}
		PrivStatValueRefresh();
	}

	public int GetStatType() { return m_hStatType; }

	internal void ImportStatValueChain(int hStatID, float fValue)
	{
		OnStatValueChain(hStatID, fValue);
	}

	static public implicit operator float(CStatBase pStat)
	{
		return pStat.m_fStatValue;
	}
	//-----------------------------------------------------------------
	private void PrivStatValueRefresh()
	{
		float fValue = m_fStatDefault;

		foreach(CStatModifierBase pModifier in m_listModifier)
		{
			fValue += pModifier.Value;
		}

		PrivStatValueApply(fValue);
	}

	private void PrivStatValueApply(float fValue)
	{
		if (fValue > m_fStatLimit)
		{
			fValue = m_fStatLimit;
		}
		m_fStatValue = fValue;
		PrivStatValueNotifyChain(m_hStatType, m_fStatValue);
		m_pStatOwner?.IStatUpdate(m_hStatType, m_fStatValue);
	}

	private void PrivStatValueNotifyChain(int hStatID, float fValue)
	{
		for (int i = 0; i < m_listChainStat.Count; i++)
		{
			m_listChainStat[i].ImportStatValueChain(hStatID, fValue);
		}
	}

	//------------------------------------------------------------------
	protected virtual void OnStatValueRefresh(float fStatValue) { } // ���� ���� ����Ǿ����� �Ϻ� ���� 
	protected virtual void OnStatValueChain(int hStatType, float fApplyValue) { } // ���� ������ ������ ����Ǿ����� ����. ���⼭ ���� ������ ���
}
