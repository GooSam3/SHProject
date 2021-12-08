using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBuffTaskEventType
{
	None,
	Start,
	End,
	EventOverTime,
	DamageTo,
	DamageFrom,
	HealTo,
	HealFrom,
	CrowdControlTo,
	CrowdControlFrom,
	SkillTo,
	SkillFrom,
	BuffTo,
	BuffFrom,
	BuffExpire,
}

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

	private uint m_hBuffID = 0;			public uint GetBuffID() { return m_hBuffID; }
	private uint m_hBuffInstanceID = 0;		public uint GetBuffInstanceID() { return m_hBuffInstanceID; }
	private uint m_eBuffType = 0;			public uint GetBuffType() { return m_eBuffType; }
	private string m_strBuffName;			public string GetBuffName() { return m_strBuffName; }
	private string m_strBuffIcon;			public string GetBuffIcon() { return m_strBuffIcon; }
	private int	m_iStackCount = 0;		public int GetBuffStackCount() { return m_iStackCount; }
	private float m_fEventOverTime = 0;
	private float m_fDuration = 0;
	private EBuffUIType m_eBuffUIType = EBuffUIType.None;  public EBuffUIType GetBuffUIType() { return m_eBuffUIType; }

	private float m_fEventOverTimeCurrent = 0;
	private float m_fDurationCurrent = 0;
	private int m_iEventOverTimeTickCount = 0; public int GetBuffEevenOverTimeTickCount() { return m_iEventOverTimeTickCount; }

	private bool m_bExclusive = false;		public bool IsExclusive { get { return m_bExclusive; } }
	private bool m_bCountUp = false;		public bool IsCountUp { get { return m_bCountUp; } }
	private bool m_bTimeReset = false;		public bool IsTimeReset { get { return m_bTimeReset; } }
	private bool m_bPowerUp = false;		public bool IsPowerUp { get { return m_bPowerUp; } }
	private bool m_bBuffActive = false;		public bool IsBuffActive { get { return m_bBuffActive; } }
	private bool m_bTimeOverUp = false;		public bool IsTimeOverUp { get { return m_bTimeOverUp; } }

	protected float m_fBuffPower = 0;		public float GetBuffPower() { return m_fBuffPower; }
	protected CBuffComponentBase m_pBuffOwner = null;
	protected CBuffComponentBase m_pBuffOrigin = null;
	private CMultiSortedDictionary<uint, CBuffTaskConditionBase> m_mapEventTaskInstance = new CMultiSortedDictionary<uint, CBuffTaskConditionBase>();
	//----------------------------------------------------------------------
	public void UpdateBuff(float fDeltaTime)
	{
		if (m_bBuffActive == false) return;

		UpdateDuration(fDeltaTime);
		OnBuffUpdate(fDeltaTime);
	}

	public void DoBuffStart(CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin, float fDuration, float fPower)
	{
		m_pBuffOwner = pBuffOwner;
		m_pBuffOrigin = pBuffOrigin;
		m_fBuffPower = fPower;
		m_bBuffActive = true;
		m_iEventOverTimeTickCount = 0;
		m_fEventOverTimeCurrent = 0;
		m_fDuration = fDuration;
		PrivBuffResetBuff();
		
		DoBuffEvent((uint)EBuffTaskEventType.Start);
		OnBuffStart();
	}

	public void DoBuffEnd()
	{
		m_bBuffActive = false;
		IEnumerator<List<CBuffTaskConditionBase>> it = m_mapEventTaskInstance.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CBuffTaskConditionBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				pList[i].DoBuffTaskEnd(this, m_pBuffOwner, m_pBuffOrigin);
			}
		}
		DoBuffEvent((uint)EBuffTaskEventType.End);
		OnBuffEnd();
	}

	public void DoBuffEvent(uint eBuffTaskType, params object [] aParams)
	{
		if (m_mapEventTaskInstance.ContainsKey(eBuffTaskType))
		{
			List<CBuffTaskConditionBase> pListTaskCondition = m_mapEventTaskInstance[eBuffTaskType];
			for (int i = 0; i < pListTaskCondition.Count; i++)
			{
				pListTaskCondition[i].DoBuffTaskCondition(this, m_pBuffOwner, m_pBuffOrigin, aParams);
			}
		}
	}

	internal void ImportBuffTimeReset(float fDuration)
	{
		m_fDuration = fDuration;
		m_fDurationCurrent = 0;
		m_fEventOverTimeCurrent = 0;
		OnBuffTimeReset(fDuration);
	}

	internal void ImportBuffPowerUp(float fPower)
	{
		m_fBuffPower += fPower;
		OnBuffPowerUP(m_fBuffPower);
	}

	internal void ImportBuffCountUp(int iStackCount)
	{
		m_iStackCount += iStackCount;
		OnBuffStackCount(m_iStackCount);

		if (m_iStackCount <= 0)
		{
			m_pBuffOwner.ImportBuffComponentStackLower(this);
		}
	}


	//----------------------------------------------------------------------
	protected void ProtBuffInitialize(uint hBuffID, uint hBuffInstanceID, string strBuffName, string strBuffIcon, uint eBuffType, EBuffUIType eBuffUIType, float fEventOverTime, int iStackCount, bool bExclusive, bool bCountUp, bool bTimeReset, bool bPowerUp, bool bTimeOverUp)
	{
		m_hBuffID = hBuffID;
		m_hBuffInstanceID = hBuffInstanceID;
		m_strBuffName = strBuffName;
		m_strBuffIcon = strBuffIcon;
		m_eBuffType = eBuffType;
		m_fEventOverTime = fEventOverTime;
		m_bExclusive = bExclusive;
		m_bCountUp = bCountUp;
		m_bTimeReset = bTimeReset;
		m_bPowerUp = bPowerUp;
		m_bTimeOverUp = bTimeOverUp;
		m_eBuffUIType = eBuffUIType;
		m_iStackCount = iStackCount;

		if (m_iStackCount < 1)
		{
			m_iStackCount = 1;
		}
	}

	protected void ProtBuffTaskConditionAdd(uint eTaskEventType, CBuffTaskConditionBase pBuffTaskCondition)
	{
		m_mapEventTaskInstance.Add(eTaskEventType, pBuffTaskCondition);
	}

	protected void ProtBuffTaskConditionExcute(uint hBuffTaskEvent, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin, params object [] aParams)
	{
		if (m_mapEventTaskInstance.ContainsKey(hBuffTaskEvent))
		{
			List<CBuffTaskConditionBase> pList = m_mapEventTaskInstance[hBuffTaskEvent];

			for (int i = 0; i < pList.Count; i++)
			{
				pList[i].DoBuffTaskCondition(this, pBuffOwner, pBuffOrigin, m_fBuffPower, m_iEventOverTimeTickCount, aParams);
			}
		}
	}

	//----------------------------------------------------------------------
	private void PrivBuffResetBuff()
	{
		m_fDurationCurrent = 0;
		m_iEventOverTimeTickCount = 0;
		m_fEventOverTimeCurrent = 0;
	}

	private void PrivBuffExpire()
	{
		m_pBuffOwner.ImportBuffComponentExpire(this);
		DoBuffEvent((uint)EBuffTaskEventType.BuffExpire);
		OnBuffExpire();
	}

	private void UpdateDuration(float fDeltaTime)
	{
		if (m_fDuration != 0)
		{
			m_fDurationCurrent += fDeltaTime;
			if (m_fDurationCurrent >= m_fDuration)
			{
				PrivBuffExpire();
			}
		}

		if (m_fEventOverTime != 0)
		{
			UpdateEventOverTime(fDeltaTime);
		}
	}

	private void UpdateEventOverTime(float fDeltaTime)
	{
		m_fEventOverTimeCurrent += fDeltaTime;
		if (m_fEventOverTimeCurrent > m_fEventOverTime)
		{
			m_fEventOverTimeCurrent = 0;
			DoBuffEvent((uint)EBuffTaskEventType.EventOverTime, m_iEventOverTimeTickCount);
			OnBuffEventOverTime(m_iEventOverTimeTickCount);
			m_iEventOverTimeTickCount++;
		}
	}

	//-----------------------------------------------------------------------
	protected virtual void OnBuffUpdate(float fDeltaTime) { }
	protected virtual void OnBuffStart() { }
	protected virtual void OnBuffExpire() { }
	protected virtual void OnBuffEnd() { }
	protected virtual void OnBuffEventOverTime(int iTickCount) { }

	protected virtual void OnBuffTimeReset(float fDuration) { }
	protected virtual void OnBuffPowerUP(float fTotalPower) { }
	protected virtual void OnBuffStackCount(int iTotalStack) { }

}
