using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CCoolTime 
{
	private class SCoolTimeInfo
	{
		public float fCoolTime = 0;
	}

	private Dictionary<string, SCoolTimeInfo> m_mapCoolTime = new Dictionary<string, SCoolTimeInfo>();
	private event UnityAction<string, float> m_delFinish;
	//------------------------------------------------------
	public void CooltimeReceiverAdd(UnityAction<string, float> delFinish)
	{
		m_delFinish += delFinish;
	}

	public void CoolTimeReceiverDelete(UnityAction<string, float> delFinish)
	{
		m_delFinish -= delFinish;
	}

	public float GetCoolTime(string strCoolTimeName)
	{
		float fCoolTime = 0;
		if (m_mapCoolTime.ContainsKey(strCoolTimeName))
		{
			fCoolTime = m_mapCoolTime[strCoolTimeName].fCoolTime;
		}
		return fCoolTime;
	}

	public void SetCoolTime(string strCoolTimeName, float fCoolTime)
	{
		if (m_mapCoolTime.ContainsKey(strCoolTimeName))
		{
			m_mapCoolTime[strCoolTimeName].fCoolTime = fCoolTime;
		}
		else
		{
			SCoolTimeInfo info = new SCoolTimeInfo();
			info.fCoolTime = fCoolTime;
			m_mapCoolTime[strCoolTimeName] = info;
		}
		PrivCoolTimeStart(strCoolTimeName, fCoolTime);
	}

	//------------------------------------------------------
	public void UpdateCoolTime(float fDelta)
	{
		Dictionary<string, SCoolTimeInfo>.Enumerator it = m_mapCoolTime.GetEnumerator();
		while(it.MoveNext())
		{
			float fValue = it.Current.Value.fCoolTime;
			fValue -= fDelta;
			m_mapCoolTime[it.Current.Key].fCoolTime = fValue;
		}

		it = m_mapCoolTime.GetEnumerator();
		while(it.MoveNext())
		{
			if (it.Current.Value.fCoolTime <= 0)
			{
				PrivCoolTimeEnd(it.Current.Key);
				m_mapCoolTime.Remove(it.Current.Key);
				it = m_mapCoolTime.GetEnumerator();
			}
		}
	}

	//------------------------------------------------------
	private void PrivCoolTimeEnd(string strCoolTimeName)
	{
		m_delFinish?.Invoke(strCoolTimeName, 0);
	}

	private void PrivCoolTimeStart(string strCoolTimeName, float fCoolTime)
	{
		m_delFinish?.Invoke(strCoolTimeName, fCoolTime);
	}
}
