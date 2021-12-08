using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIFriendScrollAdapter : OSA<BaseParamsWithPrefab, UIFriendViewsHolder>
{
	public SimpleDataHelper<ScrollFriendData> Data
	{
		get; private set;
	}

	protected override UIFriendViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIFriendViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UIFriendViewsHolder newOrRecycled)
	{
		ScrollFriendData model = Data[newOrRecycled.ItemIndex];
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
			Data = new SimpleDataHelper<ScrollFriendData>(this);

		Init();
	}

	public void SetScrollData(List<Friend> _friendList, Action _callback = null)
	{
		Data.List.Clear();

		List<ScrollFriendData> DataList = new List<ScrollFriendData>();

		for (int i = 0; i < _friendList.Count; i++)
		{
			DataList.Add(new ScrollFriendData() { FriendData = _friendList[i] });
		}

		Data.InsertItemsAtEnd(DataList);

		Data.NotifyListChangedExternally();
		_callback?.Invoke();
	}
}

public class ScrollFriendData
{
	public Friend FriendData;
}

public class UIFriendViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Text Name;
	private Text Time;
	private RectTransform SelectList;
	private ZButton SelectButton;
	private ZButton DeleteButton;
	private ZButton WhisperButton;
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
		root.GetComponentAtPath("SelectButton/Img_SelectList/Bt_Cancel", out DeleteButton);
		root.GetComponentAtPath("SelectButton/Img_SelectList/Bt_Whisper", out WhisperButton);

		SelectButton.onClick.AddListener(OnClickSelectButton);
		DeleteButton.onClick.AddListener(OnClickDelete);
		WhisperButton.onClick.AddListener(OnClickWhisper);
	}

	public void UpdateViews(ScrollFriendData _model)
	{
		ResetData();

		FriendData = _model.FriendData;

		if (FriendData == null)
			return;

		Name.text = FriendData.Nick;
		Time.text = FriendData.IsLogin ?
			DBLocale.GetText("FriendState_Connecting") :
			DBConfig.Friend_Time_Check ?
				DBLocale.GetText("FriendState_Logoff") :
				TimeHelper.CompareNow(FriendData.logoutDt, "", DBLocale.GetText("FriendState_ConnectTime"), 60, 864000/*10일*/, DBLocale.GetText("FriendState_ConnectSoon"), DBLocale.GetText("FriendState_ConnectOld"));
	}

	private void OnClickSelectButton()
	{
		SelectList.gameObject.SetActive(!SelectList.gameObject.activeSelf);
	}

	private void OnClickDelete()
	{
		UIManager.Instance.Find<UIFrameFriend>().ClickHolderAction(UIFrameFriend.ActionType.DELETE, FriendData);
	}

	private void OnClickWhisper()
	{
		UIManager.Instance.Find<UIFrameFriend>().ClickHolderAction(UIFrameFriend.ActionType.WHISPER, FriendData);
	}

	private void ResetData()
	{
		SelectList.gameObject.SetActive(false);
	}
}