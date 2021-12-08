using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SHManagerGameConfig : CManagerTemplateBase<SHManagerGameConfig>
{
	public enum EGameConfigKey
	{
		None,
		TagCoolTime,
		TotalDeck,
		GateURL,
		ScreenIdleTime,
		UpgradeGoldPerStat,
		UpgradeGoldPerSkill,
		PowerFactorOfBasic,
		PowerFactorOfCritical,
		PowerFactorOfHit,
		PowerFactorOfRecover,
		PowerFactorOfSkill,
	}

	public enum EGameModeType
	{
		None,
		Login,
		Lobby,
		Combat,
	}

	private EGameModeType m_eGameMode = EGameModeType.None; public EGameModeType GetGameMode() { return m_eGameMode; } public void SetGameMode(EGameModeType eGameMode) { m_eGameMode = eGameMode; } 
	private SHScriptTableGameConfig m_pTableGameConfig = null;
	//------------------------------------------------------------------------
	protected override void OnManagerScriptLoaded()
	{
		base.OnManagerScriptLoaded();
		m_pTableGameConfig = SHManagerScriptData.Instance.ExtractTableGameConfig();
	}

	public float GetGameDBTagCoolTime() 
	{
		return m_pTableGameConfig.ExtractValueFloat(EGameConfigKey.TagCoolTime.ToString()); 
	}

	public float GetGameDBConfigFloat(EGameConfigKey eGameConfigKey)
	{
		return m_pTableGameConfig.ExtractValueFloat(eGameConfigKey.ToString());
	}

	public string GetGameDBConfigString(EGameConfigKey eGameConfigKey)
	{
		return m_pTableGameConfig.ExtractValueString(eGameConfigKey.ToString());
	}

	public int GetGameDBConfigInteger(EGameConfigKey eGameConfigKey)
	{
		return m_pTableGameConfig.ExtractValueInteger(eGameConfigKey.ToString());
	}

}
