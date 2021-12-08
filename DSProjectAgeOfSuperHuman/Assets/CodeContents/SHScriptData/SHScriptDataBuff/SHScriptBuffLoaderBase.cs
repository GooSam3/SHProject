using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NBuff
{
	public class SHScriptBuffLoaderBase : CScriptDataLoaderXmlBase { }
	public class RootBuffData : SHScriptBuffLoaderBase { }
	public class BuffHero : SHScriptBuffLoaderBase { }
	public class BuffEnemy : SHScriptBuffLoaderBase { }

	public class BuffData : SHScriptBuffLoaderBase
	{
		public uint hBuffID;
		public string BuffName;
		public string BuffIcon;
		public EBuffType EBuffType = EBuffType.None;
		public EBuffUIType EBuffUIType = EBuffUIType.None;
		public int iStackCount;
		public float fEventOverTime;
		public bool bExclusive;
		public bool bCountUp;
		public bool bTimeReset;
		public bool bPowerUp;
		public bool bTimeOverUp;

		public SHBuffInstance MakeBuffInstance()
		{
			SHBuffInstance pNewBuff = new SHBuffInstance();
			pNewBuff.SetBuffInitialize(hBuffID, 0, BuffName, BuffIcon, EBuffType, (CBuffBase.EBuffUIType)EBuffUIType, iStackCount, fEventOverTime, bExclusive, bCountUp, bTimeReset, bPowerUp, bTimeOverUp);

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
	public abstract class BuffEventBase : SHScriptBuffLoaderBase
	{
		public void ReadBuffEventTask(SHBuffInstance pBuffInstance)
		{
			EBuffTaskEventType eEventType = OnBuffEventType();
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				BuffTaskCategoryList pTaskCategory = m_listChildElement[i] as BuffTaskCategoryList;
				if (pTaskCategory != null)
				{
					CBuffTaskConditionBase pTaskCondition = pTaskCategory.MakeInstance();
					if (pTaskCondition != null)
					{
						pBuffInstance.SetBuffTaskCondition(eEventType, pTaskCondition);
					}
				}
			}
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
			return EBuffTaskEventType.End;
		}
	}

	public class BuffEventOverTime : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.EventOverTime;
		}
	}

	public class BuffEventDamageTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.DamageTo;
		}
	}

	public class BuffEventDamageFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.DamageFrom;
		}
	}

	public class BuffEventHealTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.HealTo;
		}
	}

	public class BuffEventHealFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.HealFrom;
		}
	}

	public class BuffEventCCTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.CrowdControlTo;
		}
	}

	public class BuffEventCCFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.CrowdControlFrom;
		}
	}

	public class BuffEventSkillTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.SkillTo;
		}
	}

	public class BuffEventSkillFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.SkillFrom;
		}
	}

	public class BuffEventBuffTo : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.BuffTo;
		}
	}

	public class BuffEventBuffFrom : BuffEventBase
	{
		protected override EBuffTaskEventType OnBuffEventType()
		{
			return EBuffTaskEventType.BuffFrom;
		}
	}

	//------------------------------------------------------------------------------
	public class BuffTaskCategoryList : SHScriptBuffLoaderBase
	{
		public CBuffTaskConditionBase MakeInstance()
		{
			CBuffTaskConditionBase pBuffTaskCondition = null;
			List<CBuffTaskBase> pListTask = new List<CBuffTaskBase>();

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				BuffTaskConditionList pConditionList = m_listChildElement[i] as BuffTaskConditionList;
				if (pConditionList != null)
				{
					pBuffTaskCondition = pConditionList.MakeInstance();
				}

				BuffTaskCategoryBase pTaskCategory = m_listChildElement[i] as BuffTaskCategoryBase;
				if (pTaskCategory != null)
				{
					List<CBuffTaskBase> pListTaskCategory = pTaskCategory.MakeInstance();

					for (int j = 0; j < pListTaskCategory.Count; j++)
					{
						pListTask.Add(pListTaskCategory[j]);
					}
				}
			}

			if (pBuffTaskCondition != null)
			{
				for (int i = 0; i < pListTask.Count; i++)
				{
					pBuffTaskCondition.ImportBuffTaskAdd(pListTask[i]);
				}
			}

			return pBuffTaskCondition;
		}
	}

	public abstract class BuffTaskCategoryBase : SHScriptBuffLoaderBase
	{
		public List<CBuffTaskBase> MakeInstance()
		{
			List<CBuffTaskBase> pListBuffTask = new List<CBuffTaskBase>();

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				BuffTaskBase pBuffTask = m_listChildElement[i] as BuffTaskBase;
				if (pBuffTask != null)
				{
					pListBuffTask.Add(pBuffTask.MakeInstance());
				}
			}

			return pListBuffTask;
		}
	}

	public class BuffTaskCategoryDamageHeal : BuffTaskCategoryBase
	{

	}

	public class BuffTaskCategoryCC : BuffTaskCategoryBase
	{

	}

	public class BuffTaskCategoryStat : BuffTaskCategoryBase
	{

	}

	public class BuffTaskCategoryUtility : BuffTaskCategoryBase
	{

	}

	public class BuffTaskCategoryEffect : BuffTaskCategoryBase
	{

	}

	public class BuffTaskCategoryHero : BuffTaskCategoryBase
	{

	}

	//-----------------------------------------------------------------------------
	public class BuffTaskConditionList : SHScriptBuffLoaderBase
	{
		public CBuffTaskConditionBase MakeInstance()
		{
			CBuffTaskConditionBase pCondition = null;
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				BuffTaskConditionBase pConditionTask = m_listChildElement[i] as BuffTaskConditionBase;
				pCondition = pConditionTask.MakeInstance();
			}

			return pCondition;
		}
	}

	public abstract class BuffTaskConditionBase : SHScriptBuffLoaderBase
	{
		public CBuffTaskConditionBase MakeInstance()
		{
			return OnBuffTaskConditionMakeInstance();
		}

		protected virtual CBuffTaskConditionBase OnBuffTaskConditionMakeInstance() { return null; }
	}

	public class BuffTaskConditionNone : BuffTaskConditionBase
	{
		protected override CBuffTaskConditionBase OnBuffTaskConditionMakeInstance()
		{
			return new SHBuffTaskConditionNone();
		}
	}

	public class BuffTaskConditionStackCount : BuffTaskConditionBase
	{
		public int iStackCount = 0;
		protected override CBuffTaskConditionBase OnBuffTaskConditionMakeInstance() 
		{
			SHBuffTaskConditionStackCount pStackCount = new SHBuffTaskConditionStackCount();
			pStackCount.SetBuffTaskConditionStackCount(iStackCount);
			return pStackCount; 
		}
	}

	public class BuffTaskConditionRandom : BuffTaskConditionBase
	{
		public int iChance = 0;
		//------------------------------------------------------------------
		protected override CBuffTaskConditionBase OnBuffTaskConditionMakeInstance()
		{
			SHBuffTaskConditionRandom pRandom = new SHBuffTaskConditionRandom();
			pRandom.SetBuffTaskConditionRandom(iChance);
			return pRandom;
		}
	}
}