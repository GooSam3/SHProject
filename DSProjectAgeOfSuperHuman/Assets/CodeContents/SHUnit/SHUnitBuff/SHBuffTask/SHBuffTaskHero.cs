using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHBuffTaskHero {}


public class SHBuffTaskHeroChangeSkin : SHBuffTaskBase
{
	private string m_strAnimGroupName;
	private string m_strSkinName;

	//---------------------------------------------------------------------------
	public void SetBuffTaskHeroChangeSkin(string strAnimGroupName, string strSkinName)
	{
		m_strAnimGroupName = strAnimGroupName;
		m_strSkinName = strSkinName;
	}

	//---------------------------------------------------------------------------
	protected override void OnBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		ISHSkillProcessor pSkillProcessor = pBuffOwner.GetBuffOwner() as ISHSkillProcessor;
		pSkillProcessor.ISHAnimSkinChange(m_strAnimGroupName, m_strSkinName);
	}
}