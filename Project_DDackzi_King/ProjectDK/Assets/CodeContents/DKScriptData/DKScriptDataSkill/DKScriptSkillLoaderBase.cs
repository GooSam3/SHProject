using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using NSkill;

namespace NSkill
{
	public abstract class DKScriptSkillLoaderBase : CScriptDataLoaderXmlBase
	{

	}

	public class RootSkillData : DKScriptSkillLoaderBase { }
	public class SkillCommon : DKScriptSkillLoaderBase { }
	public class SkillHero : DKScriptSkillLoaderBase { }
	public class SkillEnemy : DKScriptSkillLoaderBase { }
	public class SkillActive : DKScriptSkillLoaderBase
	{
		public uint hSkillID = 0;
		public ETargetingType ETargetingType = ETargetingType.None;
		public float fSkillRange = 0;
		public float fSkillRadius = 0;
		public float fSkillAngle = 0;

		public void ReadSkillActive(DKSkillDataActive pSkillDataActive)
		{
			pSkillDataActive.hSkillID = hSkillID;
			pSkillDataActive.TargetingType = this.ETargetingType;
			pSkillDataActive.SkillRange = fSkillRange;
			pSkillDataActive.SkillRadius = fSkillRadius;
			pSkillDataActive.SkillAngle = fSkillAngle;

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillConditionList pCondition = m_listChildElement[i] as SkillConditionList;
				if (pCondition != null)
				{
					pCondition.ReadConditionList(pSkillDataActive.listCondition);
					continue;
				}

				SkillState pState = m_listChildElement[i] as SkillState;
				if (pState != null)
				{
					pState.ReadSkillState(pSkillDataActive.listState);
				}
			}
		}
	}

	public class SkillPassive : DKScriptSkillLoaderBase
	{
		public uint hSkillID = 0;
	}

	public class SkillAutoCast : DKScriptSkillLoaderBase
	{
		public uint hSkillID = 0;
	}
	//-------------------------------------------------------------------
	public class SkillConditionList : DKScriptSkillLoaderBase
	{
		public void ReadConditionList(List<CSkillConditionBase> pConditionList)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillConditionNone pConditionNone = m_listChildElement[i] as SkillConditionNone;
				if (pConditionNone != null)
				{
					pConditionList.Add(new DKSkillConditionNone());
					continue;
				}

				SkillConditionCC pConditionCC = m_listChildElement[i] as SkillConditionCC;
				if (pConditionCC != null)
				{
					pConditionList.Add(pConditionCC.MakeInstance());
					continue;
				}

				SkillConditionCoolTime pConditionCT = m_listChildElement[i] as SkillConditionCoolTime;
				if (pConditionCT != null)
				{
					pConditionList.Add(pConditionCT.MakeInstance());
				}
			}
		}
	}
	public class SkillConditionNone : DKScriptSkillLoaderBase
	{
	}
	public class SkillConditionCC : DKScriptSkillLoaderBase
	{
		public bool bStun = false;
		public bool bSleep = false;
		public bool bKnockBack = false;
		public bool bSilence = false;

		public DKSkillConditionCrowdControl MakeInstance()
		{
			DKSkillConditionCrowdControl pInstance = new DKSkillConditionCrowdControl();
			pInstance.Stun = bStun;
			pInstance.Sleep = bSleep;
			pInstance.KnockBack = bKnockBack;
			pInstance.Silence = bSilence;
			return pInstance;
		}
	}
	public class SkillConditionCoolTime : DKScriptSkillLoaderBase
	{
		public string CoolTimeName;
		public string GlobalCoolTime;

		public DKSkillConditionCoolTime MakeInstance()
		{
			DKSkillConditionCoolTime pInstance = new DKSkillConditionCoolTime();
			pInstance.CoolTimeName = CoolTimeName;
			pInstance.GlobalCoolTime = GlobalCoolTime;
			return pInstance;
		}
	}
	//-----------------------------------------------------------------
	public class SkillState : DKScriptSkillLoaderBase
	{
		public void ReadSkillState(List<CStateSkillBase> pListState)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillStateBase pState = m_listChildElement[i] as SkillStateBase;
				if (pState != null)
				{
					pListState.Add(pState.MakeInstance());
				}
			}
		}
	}

	public abstract class SkillStateBase : DKScriptSkillLoaderBase
	{
		public DKSkillStateBase MakeInstance()
		{
			DKSkillStateBase pNewState = OnSkillStateMakeInstance();

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillResourceList pResourceList = m_listChildElement[i] as SkillResourceList;
				if (pResourceList != null)
				{
					pResourceList.ReadSkillResource(pNewState.ExportSkilResource());
					continue;
				}

				SkillAnimation pSkillAnimation = m_listChildElement[i] as SkillAnimation;
				if (pSkillAnimation != null)
				{
					DKSkillStateAnimation pAnimation = pNewState as DKSkillStateAnimation;
					if (pAnimation != null)
					{
						pAnimation.StartAnimation = pSkillAnimation.MakeInstance();
					}
					continue;
				}

				SkillTaskEventList pSkillTaskList = m_listChildElement[i] as SkillTaskEventList;
				if (pSkillTaskList != null)
				{
					pNewState.ImportTaskEvent(pSkillTaskList.MakeInstance());
					continue;
				}

			}
			return pNewState;
		}

		protected abstract DKSkillStateBase OnSkillStateMakeInstance();
	}


	public class SkillStateAnimation : SkillStateBase
	{
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			return new DKSkillStateAnimation();
		}
	}


	public class SkillAnimation : DKScriptSkillLoaderBase
	{
		public EAnimationType EAnimationType = EAnimationType.None;
		public bool bLoop = false;
		public float fDuration = 0;
		public float fAniSpeed = 1;

		public DKSkillTaskAnimation MakeInstance()
		{
			DKSkillTaskAnimation pNewInstance = new DKSkillTaskAnimation();
			pNewInstance.SetSkillTaskCondition(new DKTaskConditionNone());
			pNewInstance.SetSkillTaskAnimation(EAnimationType, bLoop, fDuration, fAniSpeed);
			return pNewInstance;
		}
	}

	public class SkillStateCrowdControl : SkillStateBase
	{
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			return new DKSkillStateCrowdControl();
		}
	}

	public class SkillStateIdle : SkillStateBase
	{
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			return new DKSkillStateIdle();
		}
	}

	public class SkillStateInstant : SkillStateBase
	{
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			return new DKSkillStateInstant();
		}
	}

	public class SkillStateMoveDash : SkillStateBase
	{
		public float fStopRange = 0;
		public string SpeedCurveName;
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			DKSkillStateMoveDash pMoveDash = new DKSkillStateMoveDash();
			return pMoveDash;
		}
	}

	public class SkillStateMoveWarp : SkillStateBase
	{
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			return new DKSkillStateMoveWarp();
		}
	}

	public class SkillStateMoveWalk : SkillStateBase
	{
		public float fStopRange = 0;
		protected override DKSkillStateBase OnSkillStateMakeInstance()
		{
			DKSkillStateMoveWalk pMoveWalk = new DKSkillStateMoveWalk();
			pMoveWalk.SetMoveWalkStopRange(fStopRange);
			return pMoveWalk;
		}
	}

	//---------------------------------------------------------------------
	public class SkillResourceList : DKScriptSkillLoaderBase
	{
		public void ReadSkillResource(List<CSkillResourceBase> listSkillResource)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillResourceNone pResourceNone = m_listChildElement[i] as SkillResourceNone;
				if (pResourceNone != null)
				{
					listSkillResource.Add(pResourceNone.MakeInstance());
					continue;
				}

				SkillResourceCoolTime pResourceCoolTime = m_listChildElement[i] as SkillResourceCoolTime;
				if (pResourceCoolTime != null)
				{
					listSkillResource.Add(pResourceCoolTime.MakeInstance());
					continue;
				}

			}
		}
	}

	public class SkillResourceNone : DKScriptSkillLoaderBase
	{
		public DKSkillResourceBase MakeInstance()
		{
			return new DKSkillResourceNone();
		}
	}

	public class SkillResourceCoolTime : DKScriptSkillLoaderBase
	{
		public string CoolTimeName;
		public float fCoolTime;
		
		public DKSkillResourceCooltime MakeInstance()
		{
			DKSkillResourceCooltime pCoolTime = new DKSkillResourceCooltime();
			pCoolTime.SetResourceCoolTime(CoolTimeName, fCoolTime);
			return pCoolTime;
		}
	}

	public class SkillResourceBuff : DKScriptSkillLoaderBase
	{
		public uint hBuffID = 0;
		public int iConsumeCount = 0;
	}

	public class SkillResourceHP : DKScriptSkillLoaderBase
	{
		public int iConsumeHP = 0;
	}

	//-------------------------------------------------------------------
}




