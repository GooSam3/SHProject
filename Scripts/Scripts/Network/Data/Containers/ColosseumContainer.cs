using System;
using System.Collections.Generic;
using ZNet.Data;
using GameDB;
using static SpecialShopCategoryDescriptor;

public class ColosseumRankInfoConverted
{
	public readonly uint Rank;
	public readonly uint BeforRank;
	public readonly uint ServerIdx;
	public readonly ulong CharId;
	public readonly uint CharTid;
	public readonly ulong Score;
	public readonly string Nick;
	public readonly ulong GuildId;
	public readonly string GuildName;
	public readonly byte GuildMarkTid;

	public ColosseumRankInfoConverted(WebNet.RankInfo info)
	{
		Rank = info.Rank;
		BeforRank = info.BeforRank;
		ServerIdx = info.ServerIdx;
		CharId = info.CharId;
		CharTid = info.CharTid;
		Score = info.Score;
		Nick = info.Nick;
		GuildId = info.GuildId;
		GuildName = info.GuildName;
		GuildMarkTid = info.GuildMarkTid;
	}
}

public class ColosseumRoomUserConverted
{
	public readonly ulong CharId;
	public readonly string Name;
	public readonly byte TeamNo;
	public readonly byte TeamOrder;
	public readonly uint Grade;

	public ColosseumRoomUserConverted(MmoNet.RoomUser info)
	{
		CharId = info.CharId;
		Name = info.Name;
		TeamNo = info.TeamNo;
		TeamOrder = info.TeamOrder;
		Grade = info.Greade;
	}
}

public sealed class ColosseumContainer : ContainerBase
{
	public bool IsMachingNow { get; set; }
	public ulong CountDownEndTime { get; set; }
	public ulong RemainTimeTargetTime { get; set; }

	public List<ColosseumRoomUserConverted> RoomUserList { get; } = new List<ColosseumRoomUserConverted>();

	private List<ColosseumRankInfoConverted> rankingInfoList = new List<ColosseumRankInfoConverted>();
	private List<SingleDataInfo> shopSmallInfoList = new List<SingleDataInfo>();
	private List<SingleDataInfo> shopMidiumInfoList = new List<SingleDataInfo>();
	private List<SingleDataInfo> shopLargeInfoList = new List<SingleDataInfo>();

	public override void Clear()
	{
		RoomUserList.Clear();
		shopSmallInfoList.Clear();
		shopMidiumInfoList.Clear();
		shopLargeInfoList.Clear();
		rankingInfoList.Clear();

		IsMachingNow = false;
		CountDownEndTime = 0;
		RemainTimeTargetTime = 0;
	}

	/// <summary> 랭킹은 자주 갱신될 필요는 없을듯 </summary>
	public void REQ_GetColosseumRankList(Action<List<ColosseumRankInfoConverted>> callback, bool forceRequest = false)
	{
		if (forceRequest == false && rankingInfoList.Count > 0) {
			callback?.Invoke(rankingInfoList);
			return;
		}

		ZWebManager.Instance.WebGame.REQ_GetColosseumRankList((res, msg) => {
			rankingInfoList.Clear();

			for (int i = 0; i < msg.RanksLength; ++i) {
				rankingInfoList.Add(new ColosseumRankInfoConverted(msg.Ranks(i).Value));
			}

			rankingInfoList.Sort((a, b) => {
				return a.Rank.CompareTo(b.Rank);
			});

			callback?.Invoke(rankingInfoList);
		});
	}

	public void SortRoomUserList()
	{
		RoomUserList.Sort((a, b) => {
			int result = a.TeamNo.CompareTo(b.TeamNo);
			if (result != 0) {
				return result;
			}
			return a.TeamOrder.CompareTo(b.TeamOrder);
		});
	}

	public void GetShopItemInfoList(E_SpecialSubTapType subTabType, Action<List<SingleDataInfo>> callback, bool forceReset = false)
	{
		if (subTabType != E_SpecialSubTapType.Colosseum_Small &&
			subTabType != E_SpecialSubTapType.Colosseum_Medium &&
			subTabType != E_SpecialSubTapType.Colosseum_Large) {
			ZLog.LogError(ZLogChannel.Default, $"스페셜상점 탭이 아니다{subTabType}");
			callback?.Invoke(new List<SingleDataInfo>());
			return;
		}

		var shopTableList = DBSpecialShop.GetShopList(E_SpecialShopType.Colosseum);
		switch (subTabType) {
			case E_SpecialSubTapType.Colosseum_Small: {
				if (forceReset == false && shopSmallInfoList.Count > 0) {
					callback?.Invoke(shopSmallInfoList);
					return;
				}
				break;
			}
			case E_SpecialSubTapType.Colosseum_Medium: {
				if (forceReset == false && shopMidiumInfoList.Count > 0) {
					callback?.Invoke(shopMidiumInfoList);
					return;
				}
				break;
			}
			case E_SpecialSubTapType.Colosseum_Large: {
				if (forceReset == false && shopLargeInfoList.Count > 0) {
					callback?.Invoke(shopLargeInfoList);
					return;
				}
				break;
			}
		}

		var infoList = CreateShopItemInfoList(subTabType);
		callback?.Invoke(infoList);
	}

	private List<SingleDataInfo> CreateShopItemInfoList(E_SpecialSubTapType subTabType)
	{
		var shopTableList = DBSpecialShop.GetShopList(E_SpecialShopType.Colosseum, subTabType);
		for (int i = 0; i < shopTableList.Count; ++i) {
			var shopTable = shopTableList[i];
			switch (subTabType) {
				case E_SpecialSubTapType.Colosseum_Small: shopSmallInfoList.Add(CreateSingleDataInfo(shopTable)); break;
				case E_SpecialSubTapType.Colosseum_Medium: shopMidiumInfoList.Add(CreateSingleDataInfo(shopTable)); break;
				case E_SpecialSubTapType.Colosseum_Large: shopLargeInfoList.Add(CreateSingleDataInfo(shopTable)); break;
			}
		}

		switch (subTabType) {
			case E_SpecialSubTapType.Colosseum_Small: {
				shopSmallInfoList.Sort((a, b) => {
					return a.specialShopId.CompareTo(b.specialShopId);
				});
				return shopSmallInfoList;
			}
			case E_SpecialSubTapType.Colosseum_Medium: {
				shopMidiumInfoList.Sort((a, b) => {
					return a.specialShopId.CompareTo(b.specialShopId);
				});
				return shopMidiumInfoList;
			}
			case E_SpecialSubTapType.Colosseum_Large: {
				shopLargeInfoList.Sort((a, b) => {
					return a.specialShopId.CompareTo(b.specialShopId);
				});
				return shopLargeInfoList;
			}
		}
		return null;
	}

	private SingleDataInfo CreateSingleDataInfo(SpecialShop_Table shopTable)
	{
		var buyLimitState = new BuyLimitStateInfo();

		UpdateBuyLimitInfo(ref buyLimitState, shopTable.SpecialShopID);

		string dummyString = string.Empty;
		DBSpecialShop.E_SpecialShopDisplayGoodsTarget targetGoodsType = DBSpecialShop.E_SpecialShopDisplayGoodsTarget.None;
		uint externalGoodsTid = 0;
		E_CashType cashType = E_CashType.None;
		byte grade = 0;

		DBSpecialShop.GetGoodsPropsBySwitching(
			shopTable.SpecialShopID, ref cashType, ref targetGoodsType, ref dummyString, ref dummyString, ref grade, ref externalGoodsTid);

		SingleDataInfo info = new SingleDataInfo(
				shopTable.SpecialShopID, externalGoodsTid, buyLimitState, false, shopTable.PositionNumber, targetGoodsType);

		return info;
	}

	private void UpdateBuyLimitInfo(ref BuyLimitStateInfo info, uint shopID)
	{
		var limitInfo = Me.CurCharData.GetBuyLimitInfo(shopID);
		var shopData = DBSpecialShop.Get(shopID);

		if (shopData.BuyLimitType == E_BuyLimitType.Infinite) {
			info.Reset();
		}
		else {
			info.Set(shopData.BuyLimitType, shopData.BuyLimitCount, limitInfo == null ? 0 : limitInfo.BuyCnt);
		}
	}

}