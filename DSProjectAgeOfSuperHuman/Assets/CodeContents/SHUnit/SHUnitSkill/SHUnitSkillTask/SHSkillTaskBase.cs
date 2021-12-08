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
            // ���̺��� ������ �����̶� ���� ���������� ��ȯ
            fResult = iUpgradeValue / 10000f;
            // �⺻���� 1���̴�. = �������� 1�� 
            fResult += 1f;
		}

        return fResult;
	}
}
