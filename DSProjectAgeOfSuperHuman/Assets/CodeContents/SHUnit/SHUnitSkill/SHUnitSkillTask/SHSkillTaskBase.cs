using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHSkillTaskBase : CSkillTaskBase
{
    

    //-----------------------------------------------------------------
    protected float ProtSkillTaskUpgradeValue(uint hHeroID, uint hUpgradeID)
	{
        float fResult = 1f;

        uint iPoint = SHManagerGameDB.Instance.GetGameDBHeroStatUpgradePoint(hHeroID, hUpgradeID);

        if (iPoint != 0)
		{
            uint iUpgradeValue = SHManagerScriptData.Instance.ExtractTableHeroUpgrade().GetTableHeroUpgradeValue(hUpgradeID, iPoint);
            // 테이블은 만분율 기준이라 실제 전투값으로 변환
            fResult = iUpgradeValue / 10000f;
            // 기본값은 1배이다. = 전투력의 1배 
            fResult += 1f;
		}

        return fResult;
	}
}
