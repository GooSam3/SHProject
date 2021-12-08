using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

public sealed class UIFrameColosseum : ZUIFrameBase
{
	private enum E_TabType
	{
		Info,
		Shop,
		Ranking,
	}

	[SerializeField] private UIColosseumInfo uiColosseumInfo;
	[SerializeField] private UIColosseumShop uiColosseumShop;
	[SerializeField] private UIColosseumRanking uiColosseumRanking;
	[SerializeField] private ZText uiFrameTitle;
	[SerializeField] private ZText[] uiTabTitle;
	[SerializeField] private GameObject matchingTextObj;
	[SerializeField] ZToggle firstTabToggle;

	private List<ITabContents> uiTabContentsList = new List<ITabContents>();
	private UIPopupItemInfo infoPopup;
	private E_TabType curTabType = E_TabType.Info;
	public override bool IsBackable => true;
	protected override void OnInitialize()
	{
		base.OnInitialize();

		uiFrameTitle.text = DBLocale.GetText("Colosseum_Field_01_Portal_Name");
		uiTabTitle[(int)E_TabType.Info].text = DBLocale.GetText("COLOSSEUM_BATTLE");
		uiTabTitle[(int)E_TabType.Shop].text = DBLocale.GetText("COLOSSEUM_SHOP");
		uiTabTitle[(int)E_TabType.Ranking].text = DBLocale.GetText("COLOSSEUM_RANKING");
		matchingTextObj.FindChildComponent<ZText>("Txt_Matching").text = DBLocale.GetText("WPvP_Duel_Matching");

		uiTabContentsList.Add(uiColosseumInfo);
		uiTabContentsList.Add(uiColosseumShop);
		uiTabContentsList.Add(uiColosseumRanking);

		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			uiTabContentsList[i].Initialize();
			uiTabContentsList[i].Index = i;
		}
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		firstTabToggle.SelectToggle(false);
		OpenTabContents(E_TabType.Info);

		RefreshSelf();

		UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);
	}

	protected override void OnHide()
	{
		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			if (uiTabContentsList[i].Index == (int)curTabType) {
				uiTabContentsList[i].Close();
			}
		}

		// 이전에 썼던 hud로!!
		UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
	}

	public void RefreshAll()
	{
		if (gameObject.activeSelf == false) {
			return;
		}

		RefreshSelf();

		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			if (uiTabContentsList[i].Index == (int)curTabType) {
				uiTabContentsList[i].Refresh();
			}
		}
	}

	private void RefreshSelf()
	{
		matchingTextObj.SetActive(Me.CurCharData.ColosseumContainer.IsMachingNow);
	}

	private void OpenTabContents(E_TabType tabType)
	{
		curTabType = tabType;

		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			if (uiTabContentsList[i].Index == (int)curTabType) {
				uiTabContentsList[i].Open();
			}
			else {
				uiTabContentsList[i].Close();
			}
		}
	}

	public void RemoveInfoPopup()
	{
		if (infoPopup != null) {
			Destroy(infoPopup.gameObject);
			infoPopup = null;
		}
	}

	public void SetInfoPopup(UIPopupItemInfo popup)
	{
		if (infoPopup) {
			Destroy(infoPopup.gameObject);
			infoPopup = null;
		}

		infoPopup = popup;
	}

	// 버튼 이벤트

	public void OnToggleValueChanged(int tabType)
	{
		if (curTabType == (E_TabType)tabType) {
			return;
		}

		OpenTabContents((E_TabType)tabType);
	}

	public void OnClose()
	{
		// 매칭중이면 경고팝업
		if (Me.CurCharData.ColosseumContainer.IsMachingNow) {
			UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("WPvP_DuelPopup_MatchingCancle"), () => {
				ZGameModeColosseum.REQ_LeaveColosseumQueue(uiColosseumInfo.SelectStageTid);
			}, null);

			return;
		}

		Close();
	}

}
