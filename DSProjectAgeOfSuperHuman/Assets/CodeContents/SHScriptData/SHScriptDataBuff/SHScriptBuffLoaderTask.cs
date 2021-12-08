using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NBuff
{
	public class BuffTaskBase : SHScriptBuffLoaderBase
	{
		public CBuffTaskBase MakeInstance()
		{
			return OnBuffTaskMakeInstance();
		}

		protected virtual CBuffTaskBase OnBuffTaskMakeInstance() { return null; }
	}

	public class BuffTaskDamage : BuffTaskBase
	{
		public bool bDamage;
		public EDamageType EDamageType = EDamageType.None;
		public EBuffTaskTarget EBuffTaskTarget = EBuffTaskTarget.None;
		public float fPowerRate;

		protected override CBuffTaskBase OnBuffTaskMakeInstance() 
		{
			SHBuffTaskDamage pBuffTask = new SHBuffTaskDamage();
			return pBuffTask; 
		}

	}

	public class BuffTaskDamageRange : BuffTaskBase
	{
		public bool bDamage;
		public EDamageType EDamageType = EDamageType.None;
		public EBuffTaskTarget EBuffTaskTarget = EBuffTaskTarget.None;
		public float fPowerRate;
		public float fRange;
		public float iTargetCount;

		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskDamageRange pBuffTask = new SHBuffTaskDamageRange();
			return pBuffTask;
		}

	}

	public class BuffTaskStun : BuffTaskBase
	{
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskStun pBuffTask = new SHBuffTaskStun();
			return pBuffTask;
		}
	}

	public class BuffTaskSilence : BuffTaskBase
	{
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskSilence pBuffTask = new SHBuffTaskSilence();
			return pBuffTask;
		}
	}

	public class BuffTaskWeakPoint : BuffTaskBase
	{
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskWeakPoint pBuffTask = new SHBuffTaskWeakPoint();
			return pBuffTask;
		}
	}

	public class BuffTaskStatConst : BuffTaskBase
	{
		public ESHStatType ESHStatType = ESHStatType.None;
		public float fConstValue = 0;
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskStatConst pBuffTask = new SHBuffTaskStatConst();
			pBuffTask.SetBuffTaskStatConst(ESHStatType, fConstValue);
			return pBuffTask;
		}
	}

	public class BuffTaskStatPercent : BuffTaskBase
	{
		public ESHStatType ESHStatType = ESHStatType.None;
		public float fRateValue = 0;
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskStatPercent pBuffTask = new SHBuffTaskStatPercent();
			pBuffTask.SetBuffTaskStatConst(ESHStatType, fRateValue);
			return pBuffTask;
		}
	}

	public class BuffTaskClose : BuffTaskBase
	{
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskClose pBuffTask = new SHBuffTaskClose();
			return pBuffTask;
		}
	}

	public class BuffTaskBuffUse : BuffTaskBase
	{
		public EBuffTaskTarget EBuffTaskTarget = EBuffTaskTarget.None;
		public uint hBuffID;
		public float fPower;
		public float fDuration;
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskBuffUse pBuffTask = new SHBuffTaskBuffUse();
			pBuffTask.SetBuffTaskBuffUse(EBuffTaskTarget, hBuffID, fPower, fDuration);
			return pBuffTask;
		}
	}

	public class BuffTaskStack : BuffTaskBase
	{
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskStack pBuffTask = new SHBuffTaskStack();
			return pBuffTask;
		}
	}

	public class BuffTaskSkillCancle : BuffTaskBase
	{

	}
	//--------------------------------------------------------------------------
	public class BuffTaskEffectNormal : BuffTaskBase
	{
		public EUnitSocket	EUnitSocket = EUnitSocket.None;
		public string			EffectName;
		public float			fDuration;
		public float			fOffsetX;
		public float			fOffsetY;
		public float			fOffsetZ;
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskEffectNormal pBuffTask = new SHBuffTaskEffectNormal();
			pBuffTask.SetBuffTaskEffectNormal(EUnitSocket, EffectName, fDuration, new Vector3(fOffsetX, fOffsetY, fOffsetZ));
			return pBuffTask;
		}
	}

	public class BuffTaskEffectAttachCamera : BuffTaskBase
	{
		public string EffectName;
		public float fDuration;
		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskEffectAttachCamera pBuffTask = new SHBuffTaskEffectAttachCamera();
			pBuffTask.SetBuffTaskEffectAttachCamera(EffectName, fDuration);
			return pBuffTask;
		}
	}

	//----------------------------------------------------------------
	public class BuffTaskHeroChangeSkin : BuffTaskBase
	{
		public string AniGroupName;
		public string SkinName;

		protected override CBuffTaskBase OnBuffTaskMakeInstance()
		{
			SHBuffTaskHeroChangeSkin pBuffTask = new SHBuffTaskHeroChangeSkin();
			pBuffTask.SetBuffTaskHeroChangeSkin(AniGroupName, SkinName);
			return pBuffTask;
		}
	}

	public class SHScriptBuffLoaderTask{}
}

