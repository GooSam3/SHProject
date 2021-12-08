using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIStepLobby : CMonoBase
{

	//----------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		SHManagerGameConfig.Instance.SetGameMode(SHManagerGameConfig.EGameModeType.Lobby);
	}
	//-----------------------------------------------
}
