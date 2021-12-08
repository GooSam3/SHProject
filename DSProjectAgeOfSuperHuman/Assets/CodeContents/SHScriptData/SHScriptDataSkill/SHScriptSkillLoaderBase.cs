using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using NSkill;

namespace NSkill
{
	public abstract class SHScriptSkillLoaderBase : CScriptDataLoaderXmlBase
	{

	}

	public class RootSkillData : SHScriptSkillLoaderBase { }
	public class SkillHero : SHScriptSkillLoaderBase { }
	public class SkillEnemy : SHScriptSkillLoaderBase { }
	public class SkillActive : SHScriptSkillLoaderBase
	{
		public uint hSkillID = 0;
		public ETargetingType ETargetingType = ETargetingType.None;
		public string SkillName;
		
		public void ReadSkillActive(SHSkillDataActive pNewSkillData)
		{
			pNewSkillData.hSkillID = hSkillID;
			pNewSkillData.TargetingType = ETargetingType;
			pNewSkillData.SkillName = SkillName;

			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillConditionList pCondition = m_listChildElement[i] as SkillConditionList;
				if (pCondition != null)
				{
					pCondition.ReadConditionList(pNewSkillData.listCondition);
					continue;
				}

				SkillState pSkillState = m_listChildElement[i] as SkillState;
				if (pSkillState != null)
				{
					pSkillState.ReadSkillState(pNewSkillData.listState);
				}

				SkillPropertyList pSkillPropertyList = m_listChildElement[i] as SkillPropertyList;
				if (pSkillPropertyList != null)
				{
					pSkillPropertyList.ReadPropertyList(pNewSkillData.PropertyList);
				}
			}
		}
	}

	public class SkillPassive : SHScriptSkillLoaderBase
	{
		public uint hSkillID = 0;
		public string SkillIconName;
	}

	public class SkillAutoCast : SHScriptSkillLoaderBase
	{
		public uint hSkillID = 0;
		public string SkillIconName;
	}
	//-------------------------------------------------------------------
	public class SkillConditionList : SHScriptSkillLoaderBase
	{
		public void ReadConditionList(List<CSkillConditionBase> pConditionList)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillConditionNone pConditionNone = m_listChildElement[i] as SkillConditionNone;
				if (pConditionNone != null)
				{
					pConditionList.Add(new SHSkillConditionNone());
					continue;
				}

				SkillConditionCC pConditionCC = m_listChildElement[i] as SkillConditionCC;
				if (pConditionCC != null)
				{
					pConditionList.Add(pConditionCC.MakeInstance());
					continue;
				}

				SkillConditionCoolTime pConditionCoolTime = m_listChildElement[i] as SkillConditionCoolTime;
				if (pConditionCoolTime != null)
				{
					pConditionList.Add(pConditionCoolTime.MakeInstance());
				}
			}
		}
	}
	public class SkillConditionNone : SHScriptSkillLoaderBase
	{
	}
	public class SkillConditionCC : SHScriptSkillLoaderBase
	{
		public bool bStun = false;
		public bool bSilence = false;

		public SHSkillConditionCC MakeInstance()
		{
			SHSkillConditionCC pInstance = new SHSkillConditionCC();
			pInstance.SetSkillCondition(bStun, bSilence);
			return pInstance;
		}
	}
	public class SkillConditionCoolTime : SHScriptSkillLoaderBase
	{
		public string CoolTimeName;
		public SHSkillConditionCoolTime MakeInstance()
		{
			SHSkillConditionCoolTime pInstance = new SHSkillConditionCoolTime();
			pInstance.CoolTimeName = CoolTimeName;		
			return pInstance;
		}
	}
	//----------------------------------------------------------------
	public class SkillPropertyList : SHScriptSkillLoaderBase
	{
		public void ReadPropertyList(CSkillPropertyListBase pPropertyList)
		{
			for (int i = 0; i < m_listChildElement.Count; i++)
			{
				SkillProperty pSkillProperty = m_listChildElement[i] as SkillProperty;
				if (pSkillProperty != null)
				{
					pSkillProperty.ReadProperty(pPropertyList);
				}
			}
		}
	}

	public class SkillProperty : SHScriptSkillLoaderBase
	{
		public string PropertyName;

		public void ReadProperty(CSkillPropertyListBase pPropertyList)
		{
			
		}
	}


	//-----------------------------------------------------------------
	public class SkillState : SHScriptSkillLoaderBase
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

	public abstract class SkillStateBase : SHScriptSkillLoaderBase
	{
		public SHSkillStateBase MakeInstance()
		{
			SHSkillStateBase pNewState = OnSkillStateMakeInstance();

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
					SHSkillStateAnimation pAnimation = pNewState as SHSkillStateAnimation;
					if (pAnimation != null)
					{
						pAnimation.SetStateAnimation(pSkillAnimation.MakeInstance());
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

		protected abstract SHSkillStateBase OnSkillStateMakeInstance();
	}


	public class SkillStateAnimation : SkillStateBase
	{
		protected override SHSkillStateBase OnSkillStateMakeInstance()
		{
			return new SHSkillStateAnimation();
		}
	}


	public class SkillAnimation : SHScriptSkillLoaderBase
	{
		public EAnimationType EAnimationType = EAnimationType.None;
		public bool bLoop = false;
		public float fDuration = 0;
		public float fAniSpeed = 1;
		public string SoundName;

		public SHSkillTaskAnimation MakeInstance()
		{
			SHSkillTaskAnimation pNewInstance = new SHSkillTaskAnimation();
			pNewInstance.SetSkillTaskCondition(new SHSkillTaskFactorConditionNone());
			SHSkillTaskFactorTargetDefault pTaskTarget = new SHSkillTaskFactorTargetDefault();
			pTaskTarget.SetTaskTargetDefault(ERelationType.Relation_Me);
			pNewInstance.SetSkillTaskTarget(pTaskTarget);
			pNewInstance.SetSkillTaskAnimation(EAnimationType, bLoop, fDuration, fAniSpeed, SoundName);
			return pNewInstance;
		}
	}

	public class SkillStateCrowdControl : SkillStateBase
	{
		protected override SHSkillStateBase OnSkillStateMakeInstance()
		{
			return new SHSkillStateCrowdControl();
		}
	}

	public class SkillStateInstance : SkillStateBase
	{
		protected override SHSkillStateBase OnSkillStateMakeInstance()
		{
			return new SHSkillStateCrowdControl();
		}
	}

	//---------------------------------------------------------------------
	public class SkillResourceList : SHScriptSkillLoaderBase
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

	public class SkillResourceNone : SHScriptSkillLoaderBase
	{
		public SHSkillResourceNone MakeInstance()
		{
			return new SHSkillResourceNone();
		}
	}

	public class SkillResourceCoolTime : SHScriptSkillLoaderBase
	{
		public string CoolTimeName;
		public float fCoolTime;
		public float fGlobalCoolTime;
		public float fFeverPoint;

		public SHSkillResourceCoolTime MakeInstance()
		{
			SHSkillResourceCoolTime pCoolTime = new SHSkillResourceCoolTime();
			pCoolTime.SetResourceCoolTime(CoolTimeName, fCoolTime, fGlobalCoolTime, fFeverPoint);
			return pCoolTime;
		}
	}

	public class SkillResourceBuff : SHScriptSkillLoaderBase
	{
		public uint hBuffID = 0;
		public int iConsumeCount = 0;
	}

	public class SkillResourceHP : SHScriptSkillLoaderBase
	{
		public int iConsumeHP = 0;
	}

	//-------------------------------------------------------------------
}




