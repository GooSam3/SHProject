using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NBuff
{
	public class DKScriptBuffLoader : CScriptDataLoaderXmlBase { }
	public class RootBuffData : DKScriptBuffLoader { }
	public class BuffHero : DKScriptBuffLoader { }
	public class BuffEnemy : DKScriptBuffLoader { }

	public class BuffData : DKScriptBuffLoader 
	{
		public uint hBuffID;
		public float fEventOverTime;
		public bool bExclusive;
		public bool bCountUp;
		public bool bTimeReset;
		public bool bPowerUp;


		public DKBuffInstance MakeBuffInstance()
		{
			DKBuffInstance pNewBuff = new DKBuffInstance();
		//	pNewBuff.SetBuffInitializeInfo(hBuffID, fEventOverTime, 1, bExclusive, bCountUp, bTimeReset, bPowerUp);

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				BuffEventBase pEventTask = m_listChildElement[i] as BuffEventBase;
				if (pEventTask != null)
				{
					pEventTask.ReadBuffEventTask(pNewBuff);
				}

			}

			return pNewBuff;
		}
	}

	//-------------------------------------------------------------
	public abstract class BuffEventBase : DKScriptBuffLoader
	{
		public void ReadBuffEventTask(DKBuffInstance pBuffInstance)
		{

		}

		
		protected virtual EBuffTaskEventType OnBuffEventType() { return EBuffTaskEventType.None; }
	}

	public class BuffEventStart : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventEnd : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventOverTime : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventDamageTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventDamageFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventHealTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventHealFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventCCTo : BuffEventBase
	{
		public bool bStun;
		public bool bSlience;

		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.Start;
		}
	}

	public class BuffEventCCFrom : BuffEventBase
	{
		public bool bStun;
		public bool bSlience;

	}

	//------------------------------------------------------------------------------
	public class BuffTaskCategoryList : DKScriptBuffLoader
	{

	}

	public class BuffTaskCategoryDamageHeal : DKScriptBuffLoader
	{

	}

	public class BuffTaskCategoryCC : DKScriptBuffLoader
	{

	}

	public class BuffTaskCategoryStat : DKScriptBuffLoader
	{

	}

	//-----------------------------------------------------------------------------
	public class BuffTaskDamage : DKScriptBuffLoader
	{
		public bool bDamage;
		public EDamageType EDamageType = EDamageType.None;
		public EBuffTaskTarget EBuffTaskTarget = EBuffTaskTarget.None;
		public float fPowerRate;
		
	}

	public class BuffTaskDamageRange : DKScriptBuffLoader
	{
		public bool bDamage;
		public EDamageType EDamageType = EDamageType.None;
		public EBuffTaskTarget EBuffTaskTarget = EBuffTaskTarget.None;
		public float fPowerRate;
		public float fRange;
		public float iTargetCount;
	}

	public class BuffTaskStun : DKScriptBuffLoader
	{

	}

	public class BuffTaskSilence : DKScriptBuffLoader
	{

	}

	public class BuffTaskStatConst : DKScriptBuffLoader
	{
		public EDKStatType EDKStatType = EDKStatType.Strength;
		public float fConstValue = 0;
	}

	public class BuffTaskStatPercent : DKScriptBuffLoader
	{
		public EDKStatType EDKStatType = EDKStatType.Strength;
		public float fRateValue = 0;
	}



	//-----------------------------------------------------------------
}

