using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

namespace NSkill
{
	public class SHScriptSkillLoaderTask {}

	public class SkillTaskEventList : SHScriptSkillLoaderBase
	{
		public CSkillTaskEventList MakeInstance()
		{
			CSkillTaskEventList pTaskEventList = new CSkillTaskEventList();
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillTaskEventBase pTaskEvent = m_listChildElement[i] as SkillTaskEventBase;
				if (pTaskEvent != null)
				{
					pTaskEvent.ReadTaskEvent(pTaskEventList);
				}
			}
			return pTaskEventList;
		}
	}

	public abstract class SkillTaskEventBase : SHScriptSkillLoaderBase
	{
		protected void ReadSkillTaskList(CSkillTaskEventConditionBase pTaskEventCondition)
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

		public void ReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
			OnReadTaskEvent(pTaskEventList);
		}

		protected virtual void OnReadTaskEvent(CSkillTaskEventList pTaskEventList) {}
	}

	public class SkillTaskEventEnter : SkillTaskEventBase
	{		
		protected override void OnReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
			SHSkillTaskEventConditionEnter pTaskEventCondition = new SHSkillTaskEventConditionEnter();
			ReadSkillTaskList(pTaskEventCondition);
			pTaskEventList.SetTaskEventCondition((int)ETaskEventType.State_Enter, pTaskEventCondition);
		}
	}

	public class SkillTaskEventExit : SkillTaskEventBase
	{
		protected override void OnReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
			SHSkillTaskEventConditionExit pTaskEventCondition = new SHSkillTaskEventConditionExit();
			ReadSkillTaskList(pTaskEventCondition);
			pTaskEventList.SetTaskEventCondition((int)ETaskEventType.State_Exit, pTaskEventCondition);
		}
	}

	public class SkillTaskEventAnimation : SkillTaskEventBase
	{
		public EAnimEventType EAnimEventType = EAnimEventType.None;
		protected override void OnReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
			SHSkillTaskEventConditionAnimation pTaskEventCondition = new SHSkillTaskEventConditionAnimation();
			pTaskEventCondition.eAnimEventType = EAnimEventType;
			ReadSkillTaskList(pTaskEventCondition);
			pTaskEventList.SetTaskEventCondition((int)ETaskEventType.Animation_Event, pTaskEventCondition);
		}
	}

	public class SkillTaskEventAnimationEnd : SkillTaskEventBase
	{
		protected override void OnReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
			SHSkillTaskEventConditionAnimationEnd pTaskEventCondition = new SHSkillTaskEventConditionAnimationEnd();
			ReadSkillTaskList(pTaskEventCondition);
			pTaskEventList.SetTaskEventCondition((int)ETaskEventType.Animation_End, pTaskEventCondition);
		}
	}


	public class SkillTaskEventAutoCast : SkillTaskEventBase
	{
		public bool bAutoCastOn = false;

		protected override void OnReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
		}
	}

	public class SkillTaskEventCustom : SkillTaskEventBase
	{
		public ETaskEventCustomType ETaskEventCustomType = ETaskEventCustomType.None;
		protected override void OnReadTaskEvent(CSkillTaskEventList pTaskEventList)
		{
			SHSkillTaskEventConditionCustom pTaskEventCondition = new SHSkillTaskEventConditionCustom();
			pTaskEventCondition.eTaskEventType = ETaskEventCustomType;
			ReadSkillTaskList(pTaskEventCondition);
			pTaskEventList.SetTaskEventCondition((int)ETaskEventType.CustomEvent, pTaskEventCondition);
		}

	}

	//------------------------------------------------------------------------
	public class SkillTaskList : SHScriptSkillLoaderBase
	{
		public void ReadSkillTaskList(CSkillTaskEventConditionBase pTaskEventCondition)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskCategoryBase pCategory = m_listChildElement[i] as TaskCategoryBase;
				pCategory.ReadSkillTask(pTaskEventCondition);
			}
		}
	}

	public abstract class TaskCategoryBase : SHScriptSkillLoaderBase
	{
		public void ReadSkillTask(CSkillTaskEventConditionBase pTaskEventCondition)
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
	public class TaskCategoryCommon : TaskCategoryBase {}
	public class TaskCategoryEffect : TaskCategoryBase {}
	//----------------------------------------------------------------------
	public class TaskFactorConditionList : SHScriptSkillLoaderBase
	{
		public CSkillTaskFactorConditionBase MakeInstance()
		{
			CSkillTaskFactorConditionBase pNewCondition = null;
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskFactorConditionBase pTaskCondition = m_listChildElement[i] as TaskFactorConditionBase;
				if (pTaskCondition != null)
				{								
					pNewCondition = pTaskCondition.MakeInstance();
					break;
				}
			}
			return pNewCondition;
		}
	}

	public abstract class TaskFactorConditionBase : SHScriptSkillLoaderBase
	{
		public CSkillTaskFactorConditionBase MakeInstance()
		{
			return OnMakeInstance();
		}

		protected virtual CSkillTaskFactorConditionBase OnMakeInstance() { return null; }
	}

	public class TaskFactorConditionNone : TaskFactorConditionBase
	{
		protected override CSkillTaskFactorConditionBase OnMakeInstance()
		{
			return new SHSkillTaskFactorConditionNone();
		}
	}

	public class TaskFactorConditionRandom : TaskFactorConditionBase
	{
		public uint iChance = 0;
		protected override CSkillTaskFactorConditionBase OnMakeInstance()
		{
			SHSkillTaskFactorConditionRandom pRandom = new SHSkillTaskFactorConditionRandom();

			return null;
		}
	}

	public class TaskFactorConditionLessHP : TaskFactorConditionBase
	{
		public float fHPRate = 0;
		protected override CSkillTaskFactorConditionBase OnMakeInstance()
		{
			return null;
			//DKTaskConditionLessHP pTaskCondition = new DKTaskConditionLessHP();
			//pTaskCondition.SetConditionHPRate(fHPRate);
			//return pTaskCondition;
		}
	}

	//---------------------------------------------------------------------
	public abstract class TaskBase : SHScriptSkillLoaderBase
	{
		public CSkillTaskBase MakeInstance()
		{
			CSkillTaskBase pNewInstance = OnTaskMakeInstance();

			for (int i = 0; i < m_listChildElement.Count; i++)
			{

				TaskFactorTargetList pTaskTarget = m_listChildElement[i] as TaskFactorTargetList;
				if (pTaskTarget != null)
				{
					pNewInstance.SetSkillTaskTarget(pTaskTarget.MakeInstance());
				}

				TaskFactorConditionList pTaskConditionList = m_listChildElement[i] as TaskFactorConditionList;
				if (pTaskConditionList != null)
				{
					pNewInstance.SetSkillTaskCondition(pTaskConditionList.MakeInstance());
				}

				TaskEvent pTaskEvent = m_listChildElement[i] as TaskEvent;
				if (pTaskEvent != null)
				{
					
				}
			}
			return pNewInstance;
		}

		protected virtual CSkillTaskBase OnTaskMakeInstance() { return null;}
	}

	//--------------------------------------------------------------------
	public class TaskEffectCameraShake : TaskBase
	{
		public string		EffectName;
		public float		fDuration = 0;

		protected override CSkillTaskBase OnTaskMakeInstance() 
		{
			SHSkillTaskEffectCameraShake pTaskEffect = new SHSkillTaskEffectCameraShake();
			pTaskEffect.SetTaskEffectCameraShake(EffectName, fDuration);
			return pTaskEffect;
		}
	}

	public class TaskEffectStage : TaskBase
	{
		public EStageEffectType EStageEffectType = EStageEffectType.None;
		public int iArg = 0;
		public float fArg = 0;
		public float fDuration = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskEffectStage pTaskEffectStage = new SHSkillTaskEffectStage();
			pTaskEffectStage.SetTaskEffectStage(EStageEffectType, iArg, fArg, fDuration);
			return pTaskEffectStage;
		}
	}

	public class TaskEffectStageEnd : TaskBase
	{
		public EStageEffectType EStageEffectType = EStageEffectType.None;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskEffectStageEnd pTaskEffectStageEnd = new SHSkillTaskEffectStageEnd();
			pTaskEffectStageEnd.SetTaskEffectStageEnd(EStageEffectType);
			return pTaskEffectStageEnd;
		}
	}

	public class TaskEffectCharacter : TaskBase
	{
		public EUnitSocket EUnitSocket = EUnitSocket.None;
		public string EffectName;
		public float fDuration = 0;
		public float fOffsetX = 0;
		public float fOffsetY = 0;
		public float fOffsetZ = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskEffectCharacter pTaskEffectChar = new SHSkillTaskEffectCharacter();
			pTaskEffectChar.SetTaskEffectCharacter(EUnitSocket, new Vector3(fOffsetX, fOffsetY, fOffsetZ), EffectName, fDuration);
			return pTaskEffectChar;
		}
	}

	public class TaskEffectTransform : TaskBase
	{
		public string EffectName;
		public float fDuration = 0;
		public float fDestX = 0;
		public float fDestY = 0;
		public float fDestZ = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskEffectTransform pTaskEffectChar = new SHSkillTaskEffectTransform();
			pTaskEffectChar.SetTaskEffectTransform(new Vector3(fDestX, fDestY, fDestZ), EffectName, fDuration);
			return pTaskEffectChar;
		}
	}

	public class TaskEffectCharShake : TaskBase
	{
		public string EffectName;
		public float fDuration = 0;
		public float fStrength = 0;
		public float fRightAngle = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskEffectCharShake pTaskEffectChar = new SHSkillTaskEffectCharShake();
			pTaskEffectChar.SetTaskEffectCharShake(EffectName, fDuration, fStrength, fRightAngle);
			return pTaskEffectChar;
		}
	}

	//----------------------------------------------------------------------

	public class TaskAnimPlay : TaskBase
	{
		public EAnimationType EAnimationType = EAnimationType.None;
		public bool bLoop = false;
		public float fDuration = 0;
		public float fAniSpeed = 0;
		public string SoundName;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskAnimation pTaskAnimation = new SHSkillTaskAnimation();
			pTaskAnimation.SetSkillTaskAnimation(EAnimationType, bLoop, fDuration, fAniSpeed, SoundName);
			return pTaskAnimation;
		}
	}

	public class TaskEvent : TaskBase
	{
		public ETaskEventCustomType ETaskEventCustomType = ETaskEventCustomType.None;
		public int iArg;
		public float fArg;
		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskEvent pTaskEvent = new SHSkillTaskEvent();
			pTaskEvent.SetTaskEvent(ETaskEventCustomType, iArg, fArg);
			return pTaskEvent;
		}
	}

	public class TaskBuffUse : TaskBase
	{
		public uint hBuffID;
		public float fDuration;
		public string PropertyName;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskUseBuff pTaskBuff = new SHSkillTaskUseBuff();
			pTaskBuff.SetTaskUseBuff(hBuffID, fDuration, PropertyName);
			return pTaskBuff;
		}
	}

	public class TaskSkillUse : TaskBase
	{
		public uint hSkillID = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskUseSkill pTaskSkillUse = new SHSkillTaskUseSkill();
			pTaskSkillUse.SetTaskUseSkill(hSkillID);
			return pTaskSkillUse;
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
			return null;
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
			return null;		
		}
	}

	public class TaskSkillDamage : TaskBase
	{
		public EDamageType EDamageType = EDamageType.None;
		public EUnitSocket EUnitSocket = EUnitSocket.None;
		public uint PropertyID;
		public string SoundName;
		public string HitEffectName = "";
		public float fDamageRate = 1;
		public float fGaugeLockDelay = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskDamage pTaskDamage = new SHSkillTaskDamage();
			pTaskDamage.SetTaskDamage(EDamageType, EUnitSocket, PropertyID, fDamageRate, fGaugeLockDelay, SoundName, HitEffectName);
			return pTaskDamage;
		}
	}

	public class TaskSkillDamageDecrescence : TaskBase
	{
		public EDamageType EDamageType = EDamageType.None;
		public EUnitSocket EUnitSocket = EUnitSocket.None;
		public uint PropertyID;
		public string SoundName;
		public float fDamagePrime = 0;
		public float fDamageSecond = 0;
		public float fDamageThird = 0;
		public float fGaugeLockDelay = 0;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskDamageDecrescence pTaskDamage = new SHSkillTaskDamageDecrescence();
			pTaskDamage.SetTaskDamage(EDamageType, EUnitSocket, PropertyID, fDamagePrime, fDamageSecond, fDamageThird, fGaugeLockDelay, SoundName);
			return pTaskDamage;
		}

	}

	public class TaskReduceCoolTime : TaskBase
	{
		public string CoolTimeName;
		public float fCoolTime;

		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskReduceCoolTime pTaskReduceCoolTime = new SHSkillTaskReduceCoolTime();
			pTaskReduceCoolTime.SetTaskReduceCoolTime(CoolTimeName, fCoolTime);
			return pTaskReduceCoolTime;
		}
	}

	public class TaskReduceCoolTimeRandom : TaskBase
	{
		public int iCoolTimeMin = 0;
		public int iCoolTimeMax = 0;
		//-----------------------------------------------------------
		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskReduceCoolTimeRandom pTaskReduceCoolTime = new SHSkillTaskReduceCoolTimeRandom();
			pTaskReduceCoolTime.SetTaskReduceCoolTimeRandom(iCoolTimeMin, iCoolTimeMax);
			return pTaskReduceCoolTime;
		}
	}

	public class TaskStateEnd : TaskBase
	{
		protected override CSkillTaskBase OnTaskMakeInstance()
		{
			SHSkillTaskStateEnd pTask = new SHSkillTaskStateEnd();
			return pTask;
		}
	}

	//---------------------------------------------------------------------------
	public class TaskFactorTargetList : SHScriptSkillLoaderBase
	{
		public CSkillTaskFactorTargetBase MakeInstance()
		{
			CSkillTaskFactorTargetBase pNewInstance = null;
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				TaskFactorTargetBase pTaskTarget = m_listChildElement[i] as TaskFactorTargetBase;
				if (pTaskTarget != null)
				{
					pNewInstance = pTaskTarget.MakeInstance();
					break;
				}
			}
			return pNewInstance;
		}
	}

	public abstract class TaskFactorTargetBase : SHScriptSkillLoaderBase
	{
		public CSkillTaskFactorTargetBase MakeInstance()
		{
			return OnTaskTarget();
		}

		protected virtual CSkillTaskFactorTargetBase OnTaskTarget() { return null; }
	}

	public class TaskFactorTargetRange : TaskFactorTargetBase
	{
		public bool bNearbyMe = false;
		public float fRange = 0;
		public int iTargetCount = 0;
	}

	public class TaskFactorTargetHP : TaskFactorTargetBase
	{
		public ERelationType ERelationType = ERelationType.None;
		public bool bHPLess = false;
		public uint iTargetCount = 0;
	}

	public class TaskFactorTargetRandom : TaskFactorTargetBase
	{
		public ERelationType ERelationType = ERelationType.None;
		public uint iTargetCount = 0;
	}

	public class TaskFactorTargetDefault : TaskFactorTargetBase
	{
		public ERelationType ERelationType = ERelationType.None;
		protected override CSkillTaskFactorTargetBase OnTaskTarget()
		{
			SHSkillTaskFactorTargetDefault pTaskDefault = new SHSkillTaskFactorTargetDefault();
			pTaskDefault.SetTaskTargetDefault(ERelationType);
			return pTaskDefault;
		}
	}

}
