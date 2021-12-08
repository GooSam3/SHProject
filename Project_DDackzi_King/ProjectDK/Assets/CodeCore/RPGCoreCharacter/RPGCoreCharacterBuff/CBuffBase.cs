using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBuffBase
{
	public enum EBuffUIType
	{
		None,
		BuffShow,		// UI 출력 버프
		BuffGlobalShow,	// 인게임이 아닌 전역 버프 (스테이지 버프등)
		BuffHide,		// 숨겨지는 버프 

		DebuffShow,		// UI 출력 디버프
		DebuffGlobalShow, 
		DebuffHide,
	}

	private uint m_hBuffID = 0; public uint GetBuffID() { return m_hBuffID; }
	private uint m_iStackCount = 0; public uint GetBuffStackCount() { return m_iStackCount; }
	private int m_iStackAccumulate = 1;
	private float m_fEventOverTime = 0;
	private float m_fDuration = 0;
	private EBuffUIType m_eBuffUIType = EBuffUIType.None;  public EBuffUIType GetBuffUIType() { return m_eBuffUIType; }

	private float m_fEventOverTimeCurrent = 0;
	private float m_fDurationCurrent = 0;
	private int m_iTickCount = 0;

	private bool m_bExclusive = false;		public bool IsExclusive { get { return m_bExclusive; } }
	private bool m_bCountUp = false;		public bool IsCountUp { get { return m_bCountUp; } }
	private bool m_bTimeReset = false;		public bool IsTimeReset { get { return m_bTimeReset; } }
	private bool m_bPowerUp = false;		public bool IsPowerUp { get { return m_bPowerUp; } }
	private bool m_bBuffActive = false;		public bool IsBuffActive { get { return m_bBuffActive; } }

	protected float m_fBuffPower = 0;
	protected uint m_iBuffAttribute = 0;
	protected CBuffProcessorBase m_pBuffOwner = null;
	protected CBuffProcessorBase m_pBuffOrigin = null;
	private CMultiSortedDictionary<uint, CBuffTaskConditionBase> m_mapEventTaskInstance = new CMultiSortedDictionary<uint, CBuffTaskConditionBase>();
	//----------------------------------------------------------------------
	public void UpdateBuff(float fDeltaTime)
	{
		if (m_bBuffActive == false) return;

		UpdateDuration(fDeltaTime);
		OnBuffUpdate(fDeltaTime);
	}

	public void DoBuffStart(CBuffProcessorBase pBuffOwner, CBuffProcessorBase pBuffOrigin, float fDuration, float fPower)
	{
		m_pBuffOwner = pBuffOwner;
		m_pBuffOrigin = pBuffOrigin;
		m_fBuffPower = fPower;
		m_iStackCount = 0;
		m_bBuffActive = true;
		PrivBuffResetBuff();
	}

	//----------------------------------------------------------------------
	protected void ProtBuffInitialize(uint hBuffID, float fEventOverTime, EBuffUIType eBuffUIType, int iStackAccumulate, bool bExclusive, bool bCountUp, bool bTimeReset, bool bPowerUp)
	{
		m_hBuffID = hBuffID;
		m_fEventOverTime = fEventOverTime;
		m_bExclusive = bExclusive;
		m_bCountUp = bCountUp;
		m_bTimeReset = bTimeReset;
		m_bPowerUp = bPowerUp;
		m_eBuffUIType = eBuffUIType;
		m_iStackAccumulate = iStackAccumulate;
	}

	protected void ProtBuffTaskAdd(uint eTaskEventType, CBuffTaskConditionBase pBuffTaskCondition)
	{
		m_mapEventTaskInstance.Add(eTaskEventType, pBuffTaskCondition);
	}

	protected void ProtBuffTaskEvent(uint hBuffTaskEvent, CBuffProcessorBase pBuffOwner, CBuffProcessorBase pBuffOrigin, params object [] aParams)
	{
		if (m_mapEventTaskInstance.ContainsKey(hBuffTaskEvent))
		{
			List<CBuffTaskConditionBase> pList = m_mapEventTaskInstance[hBuffTaskEvent];

			for (int i = 0; i < pList.Count; i++)
			{
				pList[i].DoBuffTaskCondition(pBuffOwner, pBuffOrigin, m_fBuffPower, m_iTickCount, aParams);
			}
		}
	}

	//----------------------------------------------------------------------
	private void PrivBuffResetBuff()
	{
		m_fDurationCurrent = 0;
		m_iTickCount = 0;
		m_fEventOverTimeCurrent = 0;
	}

	private void PrivBuffEnd()
	{
		m_pBuffOwner.ImportBuffEnd(this);
		m_bBuffActive = false;
		OnBuffEnd();
	}

	private void UpdateDuration(float fDeltaTime)
	{
		if (m_fDuration == 0)
		{
			m_fDurationCurrent += fDeltaTime;
		}

		if (m_fDurationCurrent >= m_fDuration)
		{
			PrivBuffEnd();
		}
		else
		{
			UpdateEventOverTime(fDeltaTime);
		}
	}

	private void UpdateEventOverTime(float fDeltaTime)
	{

	}


	//-----------------------------------------------------------------------
	protected virtual void OnBuffUpdate(float fDeltaTime) { }
	protected virtual void OnBuffStart() { }
	protected virtual void OnBuffEnd() { }
}
