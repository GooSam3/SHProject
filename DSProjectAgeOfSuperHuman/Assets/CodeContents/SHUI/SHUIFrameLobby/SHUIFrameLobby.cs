using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameLobby : SHUIFrameBase
{
	[SerializeField]
	private SHUIWidgetHeroStatus HeroStatus = null;
	[SerializeField]
	private SHUIWidgetSpineHeroContainer HeroMotion = null;

	//---------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
	}

	protected override void OnUIFrameInitializePost()
	{
		base.OnUIFrameInitializePost();
	}

	protected override void OnUIFrameShow(int iOrder)
	{
		base.OnUIFrameShow(iOrder);
		UIManager.Instance.DoUIMgrShow<SHUIFrameResource>();
		DoUIFrameLobbyHeroRefresh();
	}

	//----------------------------------------------------------------
	public void DoUIFrameLobbyHeroRefresh()
	{
		uint hReaderID = SHManagerGameDB.Instance.GetGameDBHeroReaderID();
		HeroStatus.DoHeroStatusRefresh(hReaderID);
		HeroMotion.DoSpineHeroContainer(hReaderID);
	}

	//---------------------------------------------------------------
	public void HandleLobbyHero()
	{
		CloseSelf();
		UIManager.Instance.DoUIMgrShow<SHUIFrameNavigationBar>().DoUINavigationTabHero();
	}

	public void HandleLobbySummon()
	{
	//	CloseSelf();
	//	UIManager.Instance.DoUIMgrShow<SHUIFrameNavigationBar>().DoUINavigationTabSummon();
	}

	public void HandleLobbyShop()
	{
	//	CloseSelf();
	//	UIManager.Instance.DoUIMgrShow<SHUIFrameNavigationBar>().DoUINavigationTabShop();
	}

	public void HandleLobbyContents()
	{
		CloseSelf();
		UIManager.Instance.DoUIMgrShow<SHUIFrameContentsSelect>();
	}


	//--------------------------------------------------------------------------------------
	public void HandleLobbyNotice()
	{

	}

	public void HandleLobbyRanking()
	{

	}

	public void HandleLobbyMission()
	{

	}


	//---------------------------------------------------------------------------------
	public void HandleLobbyGift()
	{

	}

	public void HandleLobbyPost()
	{

	}

	public void HandleLobbyMenu()
	{

	}
}
