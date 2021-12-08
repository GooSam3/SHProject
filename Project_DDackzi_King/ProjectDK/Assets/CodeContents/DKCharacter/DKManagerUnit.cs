using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKManagerUnit : CManagerUnitBase
{	public static new DKManagerUnit Instance { get { return CManagerUnitBase.Instance as DKManagerUnit; } }
	
	public class SUnitDeckInfo
	{
		public string				strDeckName = "None";
		public List<uint>			HeroSID = new List<uint>();
		public List<DKUnitBase>	HeroInstance = new List<DKUnitBase>(10);
	}

	private List<SUnitDeckInfo>	m_listDeckList = new List<SUnitDeckInfo>(5);
	private SUnitDeckInfo			m_pDeckCurrent = new SUnitDeckInfo();
	//---------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	//----------------------------------------------------
	public void DoMgrUnitGetFriend(List<DKUnitBase> pOutListFriend)
	{

	}	

	public void DoMgrUnitGetEnemy(List<DKUnitBase> pOutListEnemy)
	{

	}

	public void DoMgrUnitSpawnByDeck(uint iIndex, Vector3 vecLocation)
	{

	}

	public void DoMgrUnitLeave(DKUnitBase pUnitRemove)
	{
		ProtMgrUnitUnRegist(pUnitRemove);
	}

	// 시뮬레이션에서만 호출되는 함수로 정적 할당된 유닛을 입력한다. 
	public void DoMgrUnitSpawnByInstance(EUnitType eUnitType, int iIndex, DKUnitBase pInstance)
	{
		ProtMgrUnitRegist((uint)eUnitType, pInstance);
		if (eUnitType == EUnitType.Firend)
		{
			if (iIndex < m_pDeckCurrent.HeroInstance.Count)
			{
				m_pDeckCurrent.HeroInstance[iIndex] = pInstance;
			}
		}		
	}
	//-------------------------------------------------------

}
