using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIPvpLogScrollAdapter : OSA<BaseParamsWithPrefab, UIPvpLogViewsHolder>
{
	public SimpleDataHelper<ScrollPvpLogData> Data
	{
		get; private set;
	}

	protected override UIPvpLogViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIPvpLogViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UIPvpLogViewsHolder newOrRecycled)
	{
		ScrollPvpLogData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
				UpdateViewsHolder(GetItemViewsHolder(i));
		}
	}

	public void Initialize()
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollPvpLogData>(this);

		Init();
	}

	public void SetScrollData(List<PkLogData> _pkLogList, Action _callback = null)
	{
		Data.List.Clear();

		List<ScrollPvpLogData> Datalist = new List<ScrollPvpLogData>();

		for (int i = 0; i < _pkLogList.Count; i++)
		{
			Datalist.Add(new ScrollPvpLogData() { PkLogData = _pkLogList[i] });
		}

		Data.InsertItemsAtEnd(Datalist);

		Data.NotifyListChangedExternally();
		RefreshData();
		_callback?.Invoke();
	}
}

public class ScrollPvpLogData
{
	public PkLogData PkLogData;
}

public class UIPvpLogViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image TargetIcon;
	private Image GuildIcon;
	private Text Time;
	private Text TargetText;
	private Text GuildText;
	private ZButton SneerButton;
	#endregion

	#region OSA System Variable
	private PkLogData PkLogData;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		root.GetComponentAtPath("Target/Icon_Class", out TargetIcon);
		root.GetComponentAtPath("Guild_Name/Icon_Guild", out GuildIcon);
		root.GetComponentAtPath("Time/Txt_Time", out Time);
		root.GetComponentAtPath("Target/Txt_TargetName", out TargetText);
		root.GetComponentAtPath("Guild_Name/Txt_GuildName", out GuildText);
		root.GetComponentAtPath("Bt_Aggro", out SneerButton);

		SneerButton.onClick.AddListener(ClickSneer);
	}

	public void UpdateViews(ScrollPvpLogData _model)
	{
		ResetData();

		PkLogData = _model.PkLogData;

		if (PkLogData == null)
			return;

		// 발생시간
		DateTime pkTime = TimeHelper.ParseTimeStamp((long)PkLogData.CreateDt);
		Time.text = string.Format("{0}-{1:00}-{2:00} {3:00}:{4:00}", pkTime.Year, pkTime.Month, pkTime.Day, pkTime.Hour, pkTime.Minute);

		// 타켓 이름, 길드
		TargetText.text = PkLogData.CharNick;
		GuildText.text = PkLogData.GuildName;

		if (string.IsNullOrEmpty(PkLogData.CharNick))
			TargetIcon.gameObject.SetActive(false);
		else
		{
			TargetIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBCharacter.GetClassIconName(PkLogData.CharTid));
			TargetIcon.gameObject.SetActive(true);
		}

		if (string.IsNullOrEmpty(PkLogData.GuildName))
			GuildIcon.gameObject.SetActive(false);
		else
		{
			GuildIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark((byte)PkLogData.GuildMarkTid));
			GuildIcon.gameObject.SetActive(true);
		}

		// 조롱 버튼
		SneerButton.interactable = !PkLogData.IsSneer;
	}

	private void ResetData()
	{
		TargetIcon.gameObject.SetActive(false);
		GuildIcon.gameObject.SetActive(false);
		Time.text = string.Empty;
		TargetText.text = string.Empty;
		GuildText.text = string.Empty;
	}

	private void ClickSneer()
	{
		// 조롱 기능 팝업 뛰우기.
		UICommon.OpenCostConfirmPopup((UIPopupCostConfirm _popup) =>
		{
			_popup.Open(DBLocale.GetText("조롱하기"),
						DBLocale.GetText(string.Format("{0}가 {1}를 처치하였습니다.\n위 메세지를 전체 유저에게 알리시겠습니까?", Me.CurCharData.Nickname, PkLogData.CharNick)),
						DBLocale.GetText(string.Format("{0}", DBConfig.PKSneer_Gold_Cost)),
						DBLocale.GetText("item_coin"),
						new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
						new Action[] {
								delegate
								{
									_popup.Close();
								},
								delegate
								{
									_popup.Close();
									// 데이터 처리.
									ZItem havItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Gold_ID); // 골드

									if (havItem == null || havItem.cnt < DBConfig.PKSneer_Gold_Cost)
									{
										UICommon.OpenSystemPopup((UIPopupSystem _popup0) =>
										{
											_popup0.Open(ZUIString.WARRING, DBLocale.GetText("골드가 부족합니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
											_popup0.Close(); } });
										});
									}
									else
									{
										ZWebManager.Instance.WebGame.REQ_SendPvpSneer(PkLogData.LogID, havItem.item_id, (recvPacket, recvPacketMsg) =>
										{
											PkLogData.IsSneer = true;
											UIManager.Instance.Find<UIFrameRank>().UpdateTab();
										});
									}
								}
						});
		});
	}
}
