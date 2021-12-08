using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CColliderEventHandler : CMonoBase
{
	private CUnitAIBase m_pAIOwner = null;
	//--------------------------------------------------------
	public void DoColliderEventHandler(CUnitAIBase pUnitAI)
	{
		m_pAIOwner = pUnitAI;
	}

	private void OnTriggerEnter(Collider other)
	{
		
	}

	private void OnTriggerStay(Collider other)
	{
		
	}

	private void OnTriggerExit(Collider other)
	{
		
	}

}
