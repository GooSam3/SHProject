using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHUnitRageComboCount : CMonoBase
{
	private const float c_fRageMax = 1000f;			// �ִ� �г� ������ 
	private const float c_fRageDecreaseSpeed = 400f; // �ʴ� ���ҷ�

	private const int	  c_iComboCountMax = 3;
	private const int	  c_iComboCountCancleMin = 2;

	private const float c_fComboCountDecreaseSpeed = 4f;		    // �Է��� ���� �� ���� �پ��� ��
	private const float c_fComboCountChargeTime =	1f;		// 1ȸ �Է��� �ɶ����� �����Ǵ� �ð�  �Է��� ������ �����ϸ� 0�̵Ǹ� ī��Ʈ �ٿ��� �Ͼ��.

	private int m_iCurrentCombo = 0;
	private float m_fCurrentRage = 0;
	private float m_fCurrentCountTime = 0f;
	private bool m_bComboStart = false;				public bool IsComboStart() { return m_bComboStart; }
	private UnityAction m_delComboReady = null;
	private UnityAction m_delComboEnd = null;
	//-----------------------------------------------------------
	private void Update()
	{
		float fDelta = Time.deltaTime;
		UpdateComboCount(fDelta); // �Է��� �����ð� ������ ī��Ʈ�� ��������.
		UpdateComboEnd(fDelta);
		UpdateRageBurst(fDelta);  // �г�������� ���������� �پ���.
	}

	//------------------------------------------------------------
	public void InitializeUnitRageCombo(UnityAction delComboReady, UnityAction delComboEnd)
	{
		m_delComboReady = delComboReady;
		m_delComboEnd = delComboEnd;
	}

	public void DoUnitComboCountResetAll()
	{
		m_fCurrentCountTime = 0;
		m_iCurrentCombo = 0;
		m_fCurrentRage = 0;
	}

	public void DoUnitComboCountReset()
	{
		m_fCurrentCountTime = c_fComboCountChargeTime;
		m_iCurrentCombo = 0;
	}
    
    public void DoUnitComboCountHit(int iCount = 1)
	{
		m_iCurrentCombo += iCount;
		m_fCurrentCountTime = c_fComboCountChargeTime;
		if (m_iCurrentCombo >= c_iComboCountMax)
		{
			m_iCurrentCombo = c_iComboCountMax;
			if (m_fCurrentRage >= c_fRageMax)
			{
				m_delComboReady?.Invoke();
			}
		} 
	}

	public void DoUnitRageGain(float fRagePoint)
	{
		if (m_bComboStart) return;

		m_fCurrentRage += fRagePoint;
		if (m_fCurrentRage > c_fRageMax)
		{
			m_fCurrentRage = c_fRageMax;
		}
	}

	public void DoUnitRageStart()
	{
		if (m_bComboStart) return;
		m_bComboStart = true;
	}

	public float GetUnitHeroRage()
	{
		return m_fCurrentRage;
	}

	public int GetUnitHeroComboCount()
	{
		return m_iCurrentCombo;
	}
	//-------------------------------------------------------------
	private void UpdateComboCount(float fDelta)
	{
		float fDecreaseValue = c_fComboCountDecreaseSpeed * fDelta;
		m_fCurrentCountTime -= fDecreaseValue;

		if (m_fCurrentCountTime <= 0)
		{
			m_iCurrentCombo--;
			if (m_iCurrentCombo < 0)
			{
				m_iCurrentCombo = 0;
			}
			m_fCurrentCountTime = c_fComboCountChargeTime;
		}
	}

	private void UpdateComboEnd(float fDelta)
	{
		if (m_bComboStart == false) return;

		if (m_iCurrentCombo <= c_iComboCountCancleMin)
		{
			PrivComboEnd(); 
		}
	}

	private void UpdateRageBurst(float fDelta)  
	{
		if (m_bComboStart == false) return;

		float fDecreaseValue = c_fRageDecreaseSpeed * fDelta;
		m_fCurrentRage -= fDecreaseValue;

		if (m_fCurrentRage <= 0)
		{
			m_fCurrentRage = 0f;
			PrivComboEnd();
		}
	}
	//---------------------------------------------------------------------
	private void PrivComboEnd()
	{
		DoUnitComboCountReset();
		m_bComboStart = false;
		m_delComboEnd?.Invoke();
	}
}
