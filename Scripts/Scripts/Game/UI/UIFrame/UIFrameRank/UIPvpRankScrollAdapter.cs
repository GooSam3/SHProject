using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using ZDefine;

public class UIPvpRankScrollAdapter : OSA<BaseParamsWithPrefab, UIPvpRankViewsHolder>
{
	public SimpleDataHelper<ScrollPvpRankData> Data
	{
		get; private set;
	}

	protected override UIPvpRankViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIPvpRankViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UIPvpRankViewsHolder newOrRecycled)
	{
		ScrollPvpRankData model = Data[newOrRecycled.ItemIndex];
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
			Data = new SimpleDataHelper<ScrollPvpRankData>(this);

		Init();
	}

	public void SetScrollData(List<RankingUser> _userList, Action _callback = null)
	{
		Data.List.Clear();

		List<ScrollPvpRankData> Datalist = new List<ScrollPvpRankData>();

		for (int i = 0; i < _userList.Count; i++)
		{
			Datalist.Add(new ScrollPvpRankData() { UserData = _userList[i] });
		}

		Data.InsertItemsAtEnd(Datalist);

		Data.NotifyListChangedExternally();
		RefreshData();
		_callback?.Invoke();
	}
}

public class ScrollPvpRankData
{
	public RankingUser UserData;
}

public class UIPvpRankViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image RankIcon;
	private Image ClassIcon;
	private Image GuildIcon;
	private Image BufferIcon01;
	private Image BufferIcon02;
	private Image BufferIcon03;
	private ZButton BufferButton01;
	private ZButton BufferButton02;
	private ZButton BufferButton03;
	private Text RankText;
	private Text KillCount;
	private Text PlayerName;
	private Text GuildName;
	#endregion

	#region OSA System Variable
	private RankingUser UserData;
	private GameDB.RankBuff_Table rewardBuffData;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		root.GetComponentAtPath("Icon_Rank", out RankIcon);
		root.GetComponentAtPath("Chr_Name/Icon_Class", out ClassIcon);
		root.GetComponentAtPath("Guild_Name/Icon_Guild", out GuildIcon);
		root.GetComponentAtPath("Benefit/Icon_Buff_01", out BufferIcon01);
		root.GetComponentAtPath("Benefit/Icon_Buff_02", out BufferIcon02);
		root.GetComponentAtPath("Benefit/Icon_Buff_03", out BufferIcon03);
		root.GetComponentAtPath("Benefit/Icon_Buff_01", out BufferButton01);
		root.GetComponentAtPath("Benefit/Icon_Buff_02", out BufferButton02);
		root.GetComponentAtPath("Benefit/Icon_Buff_03", out BufferButton03);
		root.GetComponentAtPath("Txt_Rank", out RankText);
		root.GetComponentAtPath("Kill_Num/Txt_KillNum", out KillCount);
		root.GetComponentAtPath("Chr_Name/Txt_Name", out PlayerName);
		root.GetComponentAtPath("Guild_Name/Txt_GuildName", out GuildName);

		BufferButton01.onClick.AddListener(ClickBuff01);
		BufferButton02.onClick.AddListener(ClickBuff02);
	}

	public void UpdateViews(ScrollPvpRankData _model)
	{
		ResetData();

		UserData = _model.UserData;

		if (UserData == null)
			return;

		// 랭킹, 랭킹 이미지
		RankText.text = UserData.Rank == 0 ? "-" : UserData.Rank.ToString();
		RankIcon.gameObject.SetActive(UserData.Rank <= 3 && UserData.Rank > 0);
		if (UserData.Rank <= 3 && UserData.Rank > 0)
		{
			RankText.text = "";
			switch (UserData.Rank)
			{
				case 1:
					RankIcon.sprite = ZManagerUIPreset.Instance.GetSprite("img_txt_rank_1");
					break;
				case 2:
					RankIcon.sprite = ZManagerUIPreset.Instance.GetSprite("img_txt_rank_2");
					break;
				case 3:
					RankIcon.sprite = ZManagerUIPreset.Instance.GetSprite("img_txt_rank_3");
					break;
			}
		}

		// 킬카운트
		//KillCount.text = MathHelper.CountString(UserData.Score);
		KillCount.text = UserData.Score.ToString();

		// 클래스이미지, 유저 이름
		ClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBCharacter.GetClassIconName(UserData.CharTid));
		PlayerName.text = UserData.Nick;
		ClassIcon.gameObject.SetActive(true);

		// 길드이미지, 길드 이름
		if (UserData.GuildId != 0)
		{
			GuildIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(UserData.GuildMarkTid));
			GuildName.text = UserData.GuildName;
			GuildIcon.gameObject.SetActive(true);
		}
		else
		{
			GuildIcon.gameObject.SetActive(false);
			GuildName.text = "-";
		}

		// 버프
		BufferIcon01.gameObject.SetActive(false);
		BufferIcon02.gameObject.SetActive(false);
		BufferIcon03.gameObject.SetActive(false);

		rewardBuffData = DBRank.GetPkRank(UserData.Rank);

		if (rewardBuffData != null)
		{
			if (rewardBuffData.AbilityActionID_01 != 0)
			{
				BufferIcon01.sprite = ZManagerUIPreset.Instance.GetSprite(DBAbility.GetActionIcon(rewardBuffData.AbilityActionID_01));
				BufferIcon01.gameObject.SetActive(true);
			}

			if (rewardBuffData.AbilityActionID_02 != 0)
			{
				BufferIcon02.sprite = ZManagerUIPreset.Instance.GetSprite(DBAbility.GetActionIcon(rewardBuffData.AbilityActionID_02));
				BufferIcon02.gameObject.SetActive(true);
			}
		}
	}

	private void ResetData()
	{
		RankIcon.gameObject.SetActive(false);
		ClassIcon.gameObject.SetActive(false);
		GuildIcon.gameObject.SetActive(false);
		BufferIcon01.gameObject.SetActive(false);
		BufferIcon02.gameObject.SetActive(false);
		BufferIcon03.gameObject.SetActive(false);
		RankText.text = string.Empty;
		KillCount.text = string.Empty;
		PlayerName.text = string.Empty;
		GuildName.text = string.Empty;
	}

	private void ClickBuff01()
	{
		if (rewardBuffData == null)
			return;

		uint AbilityActionTid = rewardBuffData.AbilityActionID_01;

		string strTitle = "";
		string strDesc = "";
		strTitle = string.Format("{0}", DBLocale.GetText(DBAbility.GetActionName(AbilityActionTid)));
		strDesc += DBAbility.ParseAbilityActions(" ", "\n", AbilityActionTid);

		UIManager.Instance.Find<UIFrameRank>().OpenBufferInfoPopup(BufferIcon01.sprite, strTitle, strDesc);
	}

	private void ClickBuff02()
	{
		if (rewardBuffData == null)
			return;

		uint AbilityActionTid = rewardBuffData.AbilityActionID_02;

		string strTitle = "";
		string strDesc = "";
		strTitle = string.Format("{0}", DBLocale.GetText(DBAbility.GetActionName(AbilityActionTid)));
		strDesc += DBAbility.ParseAbilityActions(" ", "\n", AbilityActionTid);

		UIManager.Instance.Find<UIFrameRank>().OpenBufferInfoPopup(BufferIcon02.sprite, strTitle, strDesc);
	}
}
