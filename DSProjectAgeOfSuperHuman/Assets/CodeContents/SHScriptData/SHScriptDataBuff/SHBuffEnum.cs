using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NBuff
{

	public enum EBuffStatFirst
	{
		None,
	
	}

	public enum EBuffStatSecond
	{
		None,
	}

	public enum EBuffStatThird
	{
		None,
	}

	public enum EDamageType
	{
		None,
	}

	public enum EBuffTaskTarget
	{
		None,
		BuffOwner,
		BuffOrigin,
	}

	public enum EBuffUIType
	{
		None,
		BuffShow,
		BuffGlobalShow,
		BuffHide,
		DebuffShow,
		DebuffGlobalShow,
		DebuffHide,
	}

	public enum EBuffType
	{
		None,
		CrowdControl,
		KnockBack,
		Stun,

		Damage,
		Heal,
		StatUp,
		StatDown,
	}

	public abstract class SHBuffEnum { 
		public static bool HasCrowdControl(EBuffType eBuffType)
		{
			if (eBuffType == EBuffType.CrowdControl)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool HasCrowdControl(SHUnitBase pUnit)
		{
			return HasCrowdControl(pUnit.GetUnitCrowdControll());
		}
	
	}
}
