using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIFriendAlertScrollAdapter : OSA<BaseParamsWithPrefab, UIFriendAlertViewsHolder>
{
	public SimpleDataHelper<ScrollFriendAlertData> Data
	{
		get; private set;
	}

	protected override UIFriendAlertViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIFriendAlertViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UIFriendAlertViewsHolder newOrRecycled)
	{
		ScrollFriendAlertData model = Data[newOrRecycled.ItemIndex];
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
			Data = new SimpleDataHelper<ScrollFriendAlertData>(this);

		Init();
	}

	public void SetScrollData(List<Friend> _friendList, Action _callback = null)
	{
		Data.List.Clear();

		List<ScrollFriendAlertData> DataList = new List<ScrollFriendAlertData>();

		for (int i = 0; i < _friendList.Count; i++)
		{
			DataList.Add(new ScrollFriendAlertData() { FriendData = _friendList[i] });
		}

		Data.InsertItemsAtEnd(DataList);

		Data.NotifyListChangedExternally();
		_callback?.Invoke();
	}
}

public class ScrollFriendAlertData
{
	public Friend FriendData;
}

public class UIFriendAlertViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Text Name;
	private Text Time;
	private RectTransform SelectList;
	private ZButton SelectButton;
	private ZButton AlertDeleteButton;
	#endregion

	#region OSA System Variable
	private Friend FriendData;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		root.GetComponentAtPath("Txt_Name", out Name);
		root.GetComponentAtPath("Txt_Time", out Time);
		root.GetComponentAtPath("SelectButton/Img_SelectList", out SelectList);
		root.GetComponentAtPath("SelectButton", out SelectButton);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Bt_Cancel", out AlertDeleteButton);

		SelectButton.onClick.AddListener(OnClickSelectButton);
		AlertDeleteButton.onClick.AddListener(OnClickAlertDelete);
	}

	public void UpdateViews(ScrollFriendAlertData _model)
	{
		ResetData();

		FriendData = _model.FriendData;

		if (FriendData == null)
			return;

		Name.text = FriendData.Nick;
		Time.text = FriendData.IsLogin ?
			DBLocale.GetText("FriendState_Connecting") :
			DBConfig.Alert_Time_Check ?
				DBLocale.GetText("FriendState_Logoff") :
				TimeHelper.CompareNow(FriendData.logoutDt, "", DBLocale.GetText("FriendState_ConnectTime"), 60, 864000/*10일*/, DBLocale.GetText("FriendState_ConnectSoon"), DBLocale.GetText("FriendState_ConnectOld"));
	}

	private void OnClickSelectButton()
	{
		SelectList.gameObject.SetActive(!SelectList.gameObject.activeSelf);
	}

	private void OnClickAlertDelete()
	{
		UIManager.Instance.Find<UIFrameFriend>().ClickHolderAction(UIFrameFriend.ActionType.ALERT_DELETE, FriendData);
	}

	private void ResetData()
	{
		SelectList.gameObject.SetActive(false);
	}
}