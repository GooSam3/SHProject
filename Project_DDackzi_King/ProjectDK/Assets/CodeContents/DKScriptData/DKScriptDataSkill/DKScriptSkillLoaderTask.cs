using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

namespace NSkill
{
	public class DKScriptSkillLoaderTask {}

	public class SkillTaskEventList : DKScriptSkillLoaderBase
	{
		public CTaskEventReceiver MakeInstance()
		{
			CTaskEventReceiver pTaskEventList = new CTaskEventReceiver();
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillTaskEventBase pTaskEvent = m_listChildElement[i] as SkillTaskEventBase;
				if (pTaskEvent != null)
				{
					pTaskEvent.ReadTaskEvent(pTaskEventList.ExportTaskMap());
				}
			}
			return pTaskEventList;
		}
	}

	public abstract class SkillTaskEventBase : DKScriptSkillLoaderBase
	{
		protected void SetSkillTaskEvent(CTaskEventConditionBase pTaskEventCondition)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillTaskList pSkillTaskList = m_listChildElement[i] as SkillTaskList;
				if (pSkillTaskList != null)
				{
					pSkillTaskList.ReadSkillTaskList(pTaskEventCondition);
				}
			}
		}

		public void ReadTaskEvent(CMultiSortedDictionary<int, CTaskEventConditionBase> mapTaskMap)
		{
			OnReadTaskEvent(mapTaskMap);
		}

		protected virtual void OnReadTaskEvent(CMultiSortedDictionary<int, CTaskEventConditionBase> mapTaskMap){}
	}

	public class SkillTaskEventEnter : SkillTaskEventBase
	{		
		protected override void OnReadTaskEvent(CMultiSortedDictionary<int, CTaskEventConditionBase> mapTaskMap)
		{
			DKTaskEventEnter pTaskEventEnter = new DKTaskEventEnter();
			mapTaskMap.Add((int)ETaskEventType.TaskEvent_Enter, pTaskEventEnter);
			SetSkillTaskEvent(pTaskEventEnter);
		}
	}

	public class SkillTaskEventExit : SkillTaskEventBase
	{
		protected override void OnReadTaskEvent(CMultiSortedDictionary<int, CTaskEventConditionBase> mapTaskMap)
		{
			DKTaskEventExit pTaskEventExit = new DKTaskEventExit();
			mapTaskMap.Add((int)ETaskEventType.TaskEvent_Exit, pTaskEventExit);
			SetSkillTaskEvent(pTaskEventExit);
		}
	}

	public class SkillTaskEventAnimation : SkillTaskEventBase
	{
		public EAnimEventType EAnimEventType = EAnimEventType.None;
		public uint iIndex = 0;
		protected override void OnReadTaskEvent(CMultiSortedDictionary<int, CTaskEventConditionBase> mapTaskMap)
		{
			DKTaskEventAnimation pTaskEventAnimation = new DKTaskEventAnimation();
			pTaskEventAnimation.SetTaskEventAnimation(EAnimEventType, iIndex);
			mapTaskMap.Add((int)ETaskEventType.TaskEvent_Animation, pTaskEventAnimation);
			SetSkillTaskEvent(pTaskEventAnimation);
		}
	}

	public class SkillTaskEventAutoCast : SkillTaskEventBase
	{
		public bool bAutoCastOn = false;

		protected override void OnReadTaskEvent(CMultiSortedDictionary<int, CTaskEventConditionBase> mapTaskMap)
		{
			DKTaskEventAutoCast pTaskEventAutoCast = new DKTaskEventAutoCast();			
			mapTaskMap.Add((int)ETaskEventType.TaskEvent_AutoCast, pTaskEventAutoCast);
			SetSkillTaskEvent(pTaskEventAutoCast);
		}
	}

	//------------------------------------------------------------------------
	public class SkillTaskList : DKScriptSkillLoaderBase
	{
		public void ReadSkillTaskList(CTaskEventConditionBase pTaskEventCondition)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskCategoryBase pCategory = m_listChildElement[i] as TaskCategoryBase;
				pCategory.ReadSkillTask(pTaskEventCondition);
			}
		}
	}

	public abstract class TaskCategoryBase : DKScriptSkillLoaderBase
	{
		public void ReadSkillTask(CTaskEventConditionBase pTaskEventCondition)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskBase pTaskBase = m_listChildElement[i] as TaskBase;
				pTaskEventCondition.SetSkillTask(pTaskBase.MakeInstance());				
			}
		}
	}

	public class TaskCategoryAnim : TaskCategoryBase{}
	public class TaskCategoryBuff : TaskCategoryBase{}
	public class TaskCategorySkill : TaskCategoryBase{}
	public class TaskCategoryDamage : TaskCategoryBase{}
	public class TaskCategorySpawn : TaskCategoryBase{}
	public class TaskCategoryProjectile : TaskCategoryBase{}
	//----------------------------------------------------------------------
	public class TaskConditionList : DKScriptSkillLoaderBase
	{
		public CTaskConditionBase MakeInstance()
		{
			CTaskConditionBase pNewCondition = null;
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskConditionBase pTaskCondition = m_listChildElement[i] as TaskConditionBase;
				if (pTaskCondition != null)
				{								
					pNewCondition = pTaskCondition.MakeInstance();
					break;
				}
			}
			return pNewCondition;
		}
	}

	public abstract class TaskConditionBase : DKScriptSkillLoaderBase
	{
		public CTaskConditionBase MakeInstance()
		{
			return OnMakeInstance();
		}

		protected virtual CTaskConditionBase OnMakeInstance() { return null; }
	}

	public class TaskConditionNone : TaskConditionBase
	{
		protected override CTaskConditionBase OnMakeInstance()
		{
			return new DKTaskConditionNone();
		}
	}

	public class TaskConditionRandom : TaskConditionBase
	{
		public uint iChance = 0;
		protected override CTaskConditionBase OnMakeInstance()
		{
			DKTaskConditionRandom pTaskCondition = new DKTaskConditionRandom();
			pTaskCondition.SetConditionChance(iChance);
			return pTaskCondition;
		}
	}

	public class TaskConditionLessHP : TaskConditionBase
	{
		public float fHPRate = 0;
		protected override CTaskConditionBase OnMakeInstance()
		{
			DKTaskConditionLessHP pTaskCondition = new DKTaskConditionLessHP();
			pTaskCondition.SetConditionHPRate(fHPRate);
			return pTaskCondition;
		}
	}

	//---------------------------------------------------------------------
	public abstract class TaskBase : DKScriptSkillLoaderBase
	{
		public CSkillTaskBase MakeInstance()
		{
			CSkillTaskBase pNewInstance = OnTaskMakeInstance();

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskConditionList pTaskConditionList = m_listChildElement[i] as TaskConditionList;
				if (pTaskConditionList != null)
				{
					pNewInstance.SetSkillTaskCondition(pTaskConditionList.MakeInstance());
				}

				TaskTarget pTaskTarget = m_listChildElement[i] as TaskTarget;
				if (pTaskTarget != null)
				{
					pNewInstance.SetSkillTaskTarget(pTaskTarget.MakeInstance());
				}

				TaskEvent pTaskEvent = m_listChildElement[i] as TaskEvent;
				if (pTaskEvent != null)
				{
					pNewInstance.SetSkillTaskEvent(pTaskEvent.MakeInstance());
				}
			}
			return pNewInstance;
		}

		protected virtual CSkillTaskBase OnTaskMakeInstance() { return null;}
	}

	//--------------------------------------------------------------------
	public class TaskAnimEffect : TaskBase
	{
		public EUnitSocket EUnitSocket = EUnitSocket.None;
		public string PrefabName;
		public float fDuration = 0;

		protected override CSkillTaskBase OnTaskMakeInstance() 
		{
			DKSkillTaskEffect pEffect = new DKSkillTaskEffect();
			pEffect.SetSkillTaskEffect(EUnitSocket, PrefabName, fDuration);
			return pEffect; 
		}
	}

	public class TaskAnimPlay : TaskBase
	{
		public EAnimationType EAmimationType = EAnimationType.None;
		public bool bLoop = false;
		public float fDuration = 0;
		public float fAniSpeed = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			DKSkillTaskAnimation pTaskAnimation = new DKSkillTaskAnimation();
			pTaskAnimation.SetSkillTaskAnimation(EAmimationType, bLoop, fDuration, fAniSpeed);
			return pTaskAnimation;
		}
	}

	//------------------------------------------------------------------------
	public class TaskTarget : DKScriptSkillLoaderBase
	{
		public CTaskTargetBase MakeInstance()
		{
			CTaskTargetBase pNewInstance = null;
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskTargetBase pTaskTarget = m_listChildElement[i] as TaskTargetBase;
				if (pTaskTarget != null)
				{
					pNewInstance = pTaskTarget.MakeInstance();
					break;
				}
			}
			return pNewInstance;
		}
	}

	public abstract class TaskTargetBase : DKScriptSkillLoaderBase
	{
		public CTaskTargetBase MakeInstance()
		{
			return OnTaskTarget();
		}

		protected virtual CTaskTargetBase OnTaskTarget() { return null; }
	}

	public class TaskTargetRange : TaskTargetBase
	{
		public bool  bNearbyMe = false;
		public float fRange = 0;
		public int iTargetCount = 0;

		protected override CTaskTargetBase OnTaskTarget() 
		{
			DKTaskTargetRange pTaskTargetRange = new DKTaskTargetRange();
			pTaskTargetRange.SetTaskTargetRange(bNearbyMe, fRange, iTargetCount);
			return pTaskTargetRange; 
		}
	}

	public class TaskTargetHP : TaskTargetBase
	{
		public ERelationType ERelationType = ERelationType.None;
		public bool bHPLess = false;
		public uint iTargetCount = 0;
	}

	public class TaskTargetRandom : TaskTargetBase
	{
		public ERelationType ERelationType = ERelationType.None;
		public uint iTargetCount = 0;
	}

	public class TaskTargetDefault : TaskTargetBase
	{
		public ERelationType ERelationType = ERelationType.None;
		protected override CTaskTargetBase OnTaskTarget()
		{
			DKTaskTargetDefault pTaskTarget = new DKTaskTargetDefault();
			pTaskTarget.SetTaskTargetDefault(ERelationType);
			return pTaskTarget;
		}
	}

	//-------------------------------------------------------------------------
	public class TaskEvent : DKScriptSkillLoaderBase
	{
		public ETaskEventType ETaskEventType = ETaskEventType.None;
		public int iArg;
		public float fArg;

		public CTaskEventGenerator MakeInstance()
		{
			CTaskEventGenerator pEventGen = new CTaskEventGenerator();
			pEventGen.eEventType = (int)ETaskEventType;
			pEventGen.iArg = iArg;
			pEventGen.fArg = fArg;
			return pEventGen;
		}
	}

	//------------------------------------------------------------------------
	public class TaskBuffUse : TaskBase
	{
		public uint hBuffID = 0;
		public float fDuration = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			DKSkillTaskBuff pTaskBuff = new DKSkillTaskBuff();
			pTaskBuff.SetSkillTaskBuff(hBuffID, fDuration);
			return pTaskBuff;
		}
	}

	public class TaskSkillUse : TaskBase
	{
		public uint hSkillID = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			DKSkillTaskSkill pTaskSkill = new DKSkillTaskSkill();
			pTaskSkill.SetSkillTaskSkill(hSkillID);
			return pTaskSkill;
		}
	}
	//-------------------------------------------------------------------------
	public class TaskProjectileMissile : TaskBase
	{
		public string ProjectileName;
		public float fDirectionOffset;
		public float fSpeed;
		public float fPower;
		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			DKSkillTaskProjectileMissile pMissile = new DKSkillTaskProjectileMissile();
			return pMissile;
		}
	}

	public class TaskProjectileDirection : TaskBase
	{
		public string ProjectileName;
		public float fDirectionOffset;
		public float fSpeed;
		public float fPower;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			DKSkillTaskProjectileDirection pDirection = new DKSkillTaskProjectileDirection();
			return pDirection;
		}
	}

	public class TaskSkillDamage : TaskBase
	{
		public bool bDamageOrHeal = false;
		public EDamageType EDamageType = EDamageType.None;
		public float fPower = 0;
		public float fAggro = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			DKSkillTaskDamage pDamage = new DKSkillTaskDamage();
			pDamage.SetSkillTaskDamage(EDamageType, bDamageOrHeal, fPower, fAggro);
			return pDamage;
		}
	}

	//---------------------------------------------------------------------------
}
