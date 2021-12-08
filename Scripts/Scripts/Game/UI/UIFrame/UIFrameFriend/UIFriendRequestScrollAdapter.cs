using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFriendRequestScrollAdapter : OSA<BaseParamsWithPrefab, UIFriendRequestViewsHolder>
{
	public SimpleDataHelper<ScrollFriendRequestData> Data
	{
		get; private set;
	}

	protected override UIFriendRequestViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIFriendRequestViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UIFriendRequestViewsHolder newOrRecycled)
	{
		ScrollFriendRequestData model = Data[newOrRecycled.ItemIndex];
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
			Data = new SimpleDataHelper<ScrollFriendRequestData>(this);

		Init();
	}

	public void SetScrollData(List<Friend> _friendList, Action _callback = null)
	{
		Data.List.Clear();

		List<ScrollFriendRequestData> DataList = new List<ScrollFriendRequestData>();

		for (int i = 0; i < _friendList.Count; i++)
		{
			DataList.Add(new ScrollFriendRequestData() { FriendData = _friendList[i] });
		}

		Data.InsertItemsAtEnd(DataList);

		Data.NotifyListChangedExternally();
		_callback?.Invoke();
	}
}

public class ScrollFriendRequestData
{
	public Friend FriendData;
}

public class UIFriendRequestViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Text Name;
	private Text Time;
	private RectTransform Request;
	private RectTransform Send;
	private RectTransform SelectList;
	private RectTransform RequestButtonList;
	private RectTransform CancelButtonList;
	private ZButton SelectButton;
	private ZButton CancelButton;
	private ZButton RefuseButton;
	private ZButton AcceptButton;
	#endregion

	#region OSA System Variable
	private Friend FriendData;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		root.GetComponentAtPath("Txt_Name", out Name);
		root.GetComponentAtPath("Txt_Time", out Time);
		root.GetComponentAtPath("Request", out Request);
		root.GetComponentAtPath("Send", out Send);
		root.GetComponentAtPath("SelectButton/Img_SelectList", out SelectList);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Request_Bts", out RequestButtonList);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Cancel_Bts", out CancelButtonList);
		root.GetComponentAtPath("SelectButton", out SelectButton);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Cancel_Bts/Bt_Cancel", out CancelButton);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Request_Bts/Bt_Refuse", out RefuseButton);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Request_Bts/Bt_OK", out AcceptButton);

		SelectButton.onClick.AddListener(OnClickSelectButton);
		CancelButton.onClick.AddListener(OnClickCancel);
		RefuseButton.onClick.AddListener(OnClickDeny);
		AcceptButton.onClick.AddListener(OnClickAccept);
	}

	public void UpdateViews(ScrollFriendRequestData _model)
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

		switch (FriendData.friendReqState)
		{
			case WebNet.E_FriendRequestState.Request:
				{
					Request.gameObject.SetActive(false);
					Send.gameObject.SetActive(true);
				}
				break;
			case WebNet.E_FriendRequestState.Receive:
				{
					Request.gameObject.SetActive(true);
					Send.gameObject.SetActive(false);
				}
				break;
		}
	}

	private void OnClickSelectButton()
	{
		SelectList.gameObject.SetActive(!SelectList.gameObject.activeSelf);
		switch (FriendData.friendReqState)
		{
			case WebNet.E_FriendRequestState.Request:
				{
					CancelButtonList.gameObject.SetActive(true);
					RequestButtonList.gameObject.SetActive(false);
				}
				break;
			case WebNet.E_FriendRequestState.Receive:
				{
					CancelButtonList.gameObject.SetActive(false);
					RequestButtonList.gameObject.SetActive(true);
				}
				break;
		}
	}

	private void OnClickCancel()
	{
		UIManager.Instance.Find<UIFrameFriend>().ClickHolderAction(UIFrameFriend.ActionType.CANCEL, FriendData);
	}

	private void OnClickDeny()
	{
		UIManager.Instance.Find<UIFrameFriend>().ClickHolderAction(UIFrameFriend.ActionType.DENY, FriendData);
	}

	private void OnClickAccept()
	{
		UIManager.Instance.Find<UIFrameFriend>().ClickHolderAction(UIFrameFriend.ActionType.ACCEPT, FriendData);
	}

	private void ResetData()
	{
		SelectList.gameObject.SetActive(false);
	}
}
