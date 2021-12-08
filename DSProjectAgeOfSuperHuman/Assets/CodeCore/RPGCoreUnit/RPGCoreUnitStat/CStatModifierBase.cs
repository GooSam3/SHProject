using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatModifierOwner
{

}


abstract public class CStatModifierBase 
{
	private uint m_hStatID = 0;				public uint hStatID { get { return m_hStatID; } }
	private uint m_eStatModifierType = 0;		public uint eStatModifierType { get { return m_eStatModifierType; } }
	private float m_fStatValue = 0;			public float Value { get { return m_fStatValue; } }
	private IStatModifierOwner m_pStatModifierOwner = null;
	//----------------------------------------------------------------
	internal void ImportStatModifierRefresh(float fOriginStat)
	{
		OnStatModifierRefresh(fOriginStat);
	}
	//-----------------------------------------------------------------
	protected void ProtStatModifierConst(uint hStatID, uint eStatModifierType, float fStatValue, IStatModifierOwner pStatModifierOwner)
	{
		m_hStatID = hStatID;
		m_eStatModifierType = eStatModifierType;
		m_fStatValue = fStatValue;
		m_pStatModifierOwner = pStatModifierOwner;	
	}

	protected void ProtStatModifierPercent(uint hStatID, uint eStatModifierType, float fStatValue, IStatModifierOwner pStatModifierOwner)
	{
	}


	//------------------------------------------------------------------
	protected virtual void OnStatModifierRefresh(float fOriginStat) { }
}
