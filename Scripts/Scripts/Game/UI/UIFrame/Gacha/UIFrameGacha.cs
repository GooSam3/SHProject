using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Video;
using WebNet;
using ZNet.Data;

public class UIFrameGacha : ZUIFrameBase
{
	#region External
	public UIGachaVideo GachaVideo { get; private set; } = null;
	public UIGachaTimeLine GachaTimeLine { get; private set; } = null;
	#endregion

	#region Variable
	[field: SerializeField] public ZToggle SkipBtn { get; private set; } = null;
	[field: SerializeField] public ZButton TurnAllCardBtn { get; private set; } = null;
	[field: SerializeField] public ZButton ReplayGachaBtn { get; private set; } = null;
	[field: SerializeField] public ZButton CloseBtn { get; private set; } = null;

	[field: SerializeField] public Animation animSkillSelect { get; private set; } = null;

	[field: SerializeField] public ZButton NextClassBtn { get; private set; } = null;

	[field: SerializeField] public GameObject[] StateObj { get; private set; } = null;

	// 연출 Scene
	[field: SerializeField] public GameObject Scene { get; private set; } = null;

	[field: SerializeField] public Image Bg { get; private set; } = null;
	[field: SerializeField] public VideoPlayer VideoPlayer { get; private set; } = null;
	[field: SerializeField] public RawImage VideoImage { get; private set; } = null;

	[field: SerializeField] public PlayableDirector TimeLine { get; private set; } = null;

	[field: SerializeField] public UIGachaCardLinker CardLinker { get; private set; } = null;

	public UIGachaEnum.E_GachaStyle CurrentGachaStyle { get; private set; } = UIGachaEnum.E_GachaStyle.None;
	public UIGachaEnum.E_TimeLineType CurrentTimeLineType { get; private set; } = UIGachaEnum.E_TimeLineType.None;

	public GachaViewController GachaSceneController { get; private set; } = null;

	public bool IsHidden { get; private set; } = false;
	public bool SkipMode { get; private set; } = false;
	[SerializeField] private bool isInit = false;

	public List<uint> ListGachaResult { get; private set; } = new List<uint>();

	private string SkipPrefsKey;// 타입별 저장키

	/// <summary>
	/// 다시뽑기용 플래그 겸 테이블 id
	/// 0이면 스페셜상점으로 진입한거아님 -> 다시뽑기 ㄴ
	/// 0!=n 이면 해당 아이디로 재접근
	/// </summary>
	public uint SpecialShopTid { get; private set; } = 0;

	#endregion

	protected override void OnInitialize()
	{
		base.OnInitialize();

		GachaVideo = gameObject.AddComponent<UIGachaVideo>();
		GachaTimeLine = gameObject.AddComponent<UIGachaTimeLine>();

		GachaVideo.Initialize(this);
		GachaTimeLine.Initialize(this);

		isInit = true;

		// test Value
		uint[] CardValue = null;
		// Test
		//	CurrentGachaStyle = UIGachaEnum.E_GachaStyle.Ride;
		//	CurrentTimeLineType = UIGachaEnum.E_TimeLineType.Ride_11_Start;

		// Item
		//CardValue = new uint[] { 611010, 612010, 613010, 614010, 615010, 616010, 611010, 612010, 613010, 614010, 615010, };

		// Pet
		//CardValue = new uint[] { 210005, 210006, 210003, 220001, 220002, 220003, 230001, 230002, 230003, 240001, 615010, };

		// Ride
		//CardValue = new uint[] { 130009, 130010, 130011, 130012, 160007, 130014, 130015, 130016, 160008, 130018, 130019, };

		// Change
		// CardValue = new uint[] { 10013, 20014, 30010, 40022, 50023, 60017, 10013, 20014, 30010, 40022, 50023 };

		// == 외부노출시 삭제
		//SetGachaData(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_TimeLineType.Class_11_Start, CardValue);
		//SetGachaData(UIGachaEnum.E_GachaStyle.Item, UIGachaEnum.E_TimeLineType.Item_11_Start, CardValue);
		//SetGachaData(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_TimeLineType.Pet_11_Start, CardValue);
		//SetGachaData(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_TimeLineType.Ride_11_Start, CardValue);
	}

	protected override void OnRemove()
	{
		base.OnRemove();
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		SpecialShopTid = 0;

		if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame(E_UIStyle.FullScreen);

		if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
			gainSystem.SetPlayState(false);

		UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.Gacha);
		UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
		UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
	}

	protected override void OnHide()
	{
		base.OnHide();

		UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
		UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, false);
		UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, false);

		OnClose();

		if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame();

		if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
			gainSystem.SetPlayState(true);
	}

	#region Function

	// 외부 노출(세팅)용
	public void SetGachaData(UIGachaEnum.E_GachaStyle style, UIGachaEnum.E_TimeLineType timelineType, List<uint> listTid, uint spShopTid = 0)
	{
		SpecialShopTid = spShopTid;

		ListGachaResult.Clear();
		ListGachaResult.AddRange(listTid);

		CurrentGachaStyle = style;
		CurrentTimeLineType = timelineType;

		SkipPrefsKey = string.Format(ZUIConstant.PREFS_KEY_GACHA_SKIP_FORMAT, CurrentGachaStyle);

		SkipMode = Convert.ToBoolean(PlayerPrefs.GetInt(SkipPrefsKey, 0));

		// 스킵버튼 갱신-------------

		if (SkipMode != SkipBtn.isOn)
		{
			SkipMode = !SkipMode;
			//SkipBtn.SelectToggle();
			SkipBtn.isOn = !SkipBtn.isOn;
			//OnSelectSkip();
		}

		//-------------------------

		StartGacha();
	}

	public void StartGacha()
	{
		if (!isInit)
			return;

		Initialze();

		GachaVideo.StartPlayVideo();
		LoadGachaScene();
	}

	private void LoadGachaScene()
	{
		// Scene
		switch (CurrentGachaStyle)
		{
			case UIGachaEnum.E_GachaStyle.Class:
				{
					if (SkipMode || GachaSceneController != null)
						return;

					ZSceneManager.Instance.OpenAdditive(ZUIConstant.SUB_SCENE_GACHA_VIEW,
						null,
						(sceneName) =>
						{
							var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(ZUIConstant.SUB_SCENE_GACHA_VIEW);

							if (scene.isLoaded == false)
							{
								UICommon.OpenSystemPopup_One(ZUIString.ERROR,
															 "씬 로드 실패.", ZUIString.LOCALE_OK_BUTTON);

								return;
							}

							foreach (var iter in scene.GetRootGameObjects())
							{
								if (iter.name.Equals("Root"))
								{
									GachaSceneController = iter.GetComponent<GachaViewController>();
									break;
								}
							}

							if (GachaSceneController == null)
							{
								UICommon.OpenSystemPopup_One(ZUIString.ERROR,
															 "씬 로드 실패.", ZUIString.LOCALE_OK_BUTTON);

								return;
							}

							GachaSceneController.Initialize(ListGachaResult, null);
							GachaSceneController.gameObject.SetActive(false);

						});
				}
				break;
		}
	}

	public void CheckTimeLine()
	{
		switch (CurrentGachaStyle)
		{
			case UIGachaEnum.E_GachaStyle.Class:
			case UIGachaEnum.E_GachaStyle.Pet:
			case UIGachaEnum.E_GachaStyle.Ride:
			case UIGachaEnum.E_GachaStyle.Item:
				{
					GachaTimeLine.PlayTimeLine(CurrentTimeLineType);
				}
				break;
		}
	}

	public bool IsRareSequence()
	{
		foreach (var iter in ListGachaResult)
		{
			switch (CurrentGachaStyle)
			{
				case UIGachaEnum.E_GachaStyle.Pet:
				case UIGachaEnum.E_GachaStyle.Ride:
					{
						if (DBPet.TryGet(iter, out var table) == false)
							continue;

						if (table.Grade >= ZUIConstant.RARE_SEQUENCE_GRADE)
							return true;
					}
					break;
				case UIGachaEnum.E_GachaStyle.Class:
					{
						if (DBChange.TryGet(iter, out var table) == false)
							continue;

						if (table.Grade >= ZUIConstant.RARE_SEQUENCE_GRADE)
							return true;
					}
					break;
				case UIGachaEnum.E_GachaStyle.Item:
					{
						if (DBItem.GetItem(iter, out var table) == false)
							continue;

						if (table.Grade >= ZUIConstant.RARE_SEQUENCE_GRADE)
							return true;
					}
					break;
			}
		}

		return false;
	}
	#endregion

	#region Input
	/// <summary>Skip 버튼 클릭 콜백</summary>
	public void OnSelectSkip()
	{
		if (!isInit)
			return;

		SkipMode = !SkipMode;
		PlayerPrefs.SetInt(SkipPrefsKey, Convert.ToInt32(SkipMode));
	}

	/// <summary>카드 모두 뒤집기 버튼 클릭 콜백</summary>
	public void OnTurnAllCard()
	{
		CardLinker?.TurnAll();
	}

	/// <summary>다시 소환 버튼 클릭 콜백</summary>
	public void OnReplayGacha()
	{
		if (SpecialShopTid <= 0)
		{
			ZLog.Log(ZLogChannel.UI, "샵아이디가 없는데 버튼이 켜짐");
			return;
		}

		if (DBSpecialShop.TryGet(SpecialShopTid, out var table) == false)
		{
			ZLog.Log(ZLogChannel.System, $"샵아이디가 존재하지 않음 tid : {SpecialShopTid}");
			return;
		}

		if (table.GoodsListGetType != GameDB.E_GoodsListGetType.Rate)
		{
			ZLog.Log(ZLogChannel.System, $"가챠연출이 필요없는 애한테 호출됨 : {SpecialShopTid}");
			return;
		}

		string txtTitleLocale = DBLocale.GetText(table.ShopTextID);
		UIMessagePopup.ShowCostPopup(txtTitleLocale, DBLocale.GetText("Gacha_Notice_Retry", txtTitleLocale),
			table.BuyItemID, table.BuyItemCount, () => {
				if (ConditionHelper.CheckCompareCost(table.BuyItemID, table.BuyItemCount) == false)
				{
					OnClose();
					return;
				}

				GachaTimeLine.ClearAllTimeLine();

				ReplayGachaBtn.interactable = false;

				DBSpecialShop.ItemBuyInfo buyInfo = new DBSpecialShop.ItemBuyInfo(
											SpecialShopTid,
											1,
											Me.CurCharData.GetItem(table.BuyItemID)?.item_id ?? 0,
											table.BuyItemID, 0);

				ZWebManager.Instance.WebGame.REQ_SpecialShopBuy(new List<DBSpecialShop.ItemBuyInfo>() { buyInfo }, (recvPacket, recvMsgPacket) =>
				{
					ZWebManager.Instance.WebGame.REQ_GetCashMailList(delegate
					{
						SetGachaData(CurrentGachaStyle, CurrentTimeLineType, GetGachaResultTidList(recvMsgPacket), SpecialShopTid);
					});
				});
			});
	}

	// 연출 가능한 경우만 온전한 리스트 넘겨줌
	public static List<uint> GetGachaResultTidList(ResSpecialShopBuy resList)
	{
		List<uint> listTid = new List<uint>();

		UIGachaEnum.E_TimeLineType targetTimeline = UIGachaEnum.E_TimeLineType.None;

		if (targetTimeline == UIGachaEnum.E_TimeLineType.None && resList.ResultGetPetsLength > 0)
		{
			listTid.Clear();
			bool isPet = false;

			for (int i = 0; i < resList.ResultGetPetsLength; i++)
			{
				listTid.Add(resList.ResultGetPets(i).Value.PetTid);
			}

			isPet = DBPet.GetPetData(listTid[0]).PetType == E_PetType.Pet;

			targetTimeline = GetPossibleGachaTimeline(isPet ? UIGachaEnum.E_GachaStyle.Pet : UIGachaEnum.E_GachaStyle.Ride, listTid);
		}

		if (targetTimeline == UIGachaEnum.E_TimeLineType.None && resList.ResultGetChangesLength > 0)
		{
			listTid.Clear();
			for (int i = 0; i < resList.ResultGetChangesLength; i++)
			{
				listTid.Add(resList.ResultGetChanges(i).Value.ChangeTid);
			}

			targetTimeline = GetPossibleGachaTimeline(UIGachaEnum.E_GachaStyle.Class, listTid);
		}

		if (targetTimeline == UIGachaEnum.E_TimeLineType.None && resList.ResultGetItemsLength > 0)
		{
			listTid.Clear();
			for (int i = 0; i < resList.ResultGetItemsLength; i++)
			{
				if (DBItem.GetItem(resList.ResultGetItems(i).Value.ItemTid).ItemType == E_ItemType.Goods)
					continue;

				listTid.Add(resList.ResultGetItems(i).Value.ItemTid);
			}

			targetTimeline = GetPossibleGachaTimeline(UIGachaEnum.E_GachaStyle.Item, listTid);
		}

		return listTid;
	}
	
	// 연출 가능한 타임라인 넘겨줌
	public static UIGachaEnum.E_TimeLineType GetPossibleGachaTimeline(UIGachaEnum.E_GachaStyle style, List<uint> listTid)
	{
		UIGachaEnum.E_TimeLineType timelineType = UIGachaEnum.E_TimeLineType.None;

		if (listTid.Count != 1 && listTid.Count != 11)
		{
			ZLog.LogError(ZLogChannel.System, $"리스트 갯수 다시확인바람! style : {style}, count : {listTid.Count}");
			return timelineType;
		}

		bool isOne = listTid.Count == 1;

		switch (style)
		{
			case UIGachaEnum.E_GachaStyle.Pet:
				timelineType = isOne ? UIGachaEnum.E_TimeLineType.Pet_1_Start : UIGachaEnum.E_TimeLineType.Pet_11_Start;
				break;
			case UIGachaEnum.E_GachaStyle.Class:
				timelineType = isOne ? UIGachaEnum.E_TimeLineType.Class_1_Start : UIGachaEnum.E_TimeLineType.Class_11_Start;
				break;
			case UIGachaEnum.E_GachaStyle.Ride:
				timelineType = isOne ? UIGachaEnum.E_TimeLineType.Ride_1_Start : UIGachaEnum.E_TimeLineType.Ride_11_Start;
				break;
			case UIGachaEnum.E_GachaStyle.Item:
				timelineType = isOne ? UIGachaEnum.E_TimeLineType.Item_1_Start : UIGachaEnum.E_TimeLineType.Item_11_Start;
				break;
		}

		return timelineType;
	}

	// 스킬 버튼 클릭 콜백
	public void OnClickSkill(int i)
	{
		if (i <= 0)
		{
			GachaVideo.PlayVideo(GachaVideo.NextVideo);
		}
		else
		{
			GachaVideo.PlayVideo(GachaVideo.SubVideo);
		}

		SetSkillInputBtn(false);
	}

	// 강림씬뷰 다음모델 출력버튼
	public void OnClickNextClass()
	{
		bool result = GachaSceneController.SetNext();

		if (result == true)
		{
			SetClassViewSequence(false);
			CheckTimeLine();
		}
	}

	public void OnClose()
	{
		Clear();

		UIManager.Instance.Close<UIFrameGacha>();
	}
	#endregion


	private void Initialze()
	{
		IsHidden = false;

		for (int i = 0; i < StateObj.Length; i++)
			StateObj[i].SetActive(false);

		GachaVideo.Clear();

		SetSkillInputBtn(false);
		SetClassViewSequence(false);
		TurnAllCardBtn.gameObject.SetActive(false);
		CloseBtn.gameObject.SetActive(false);
		ReplayGachaBtn.gameObject.SetActive(false);
		ReplayGachaBtn.interactable = true;

		// 처음엔 null이나 다시뽑기시 있을수있음
		GachaSceneController?.Initialize(ListGachaResult, null);
	}

	public void SetSkillInputBtn(bool state)
	{
		animSkillSelect.gameObject.SetActive(state);
		if (state)
			animSkillSelect.Play();
		else
		{
			animSkillSelect.Stop();
			animSkillSelect.Rewind();
		}
	}

	public void SetClassViewSequence(bool state)
	{
		Bg.gameObject.SetActive(!state);
		NextClassBtn.gameObject.SetActive(state);
	}

	private void Clear()
	{
		SetVideoClip(null);
		GachaVideo.SetNextVideoClip(null);
		GachaVideo.SetDefaultVideoClip(null);

		GachaTimeLine.ClearAllTimeLine();

		// 씬은 무조건해제해야하므로..(임시)

		if (GachaSceneController != null)
		{
			GachaSceneController.Clear();
			ZSceneManager.Instance.CloseAdditive(ZUIConstant.SUB_SCENE_GACHA_VIEW, null);
			GachaSceneController = null;
		}
	}

	#region Data
	public void SetVideoClip(VideoClip _clip) { VideoPlayer.clip = _clip; }
	public void SetSceneObject(GameObject _scene) { Scene = _scene; }
	public void SetTimeLine(PlayableDirector _timeLine) { TimeLine = _timeLine; }
	public void SetCardLinker(UIGachaCardLinker _linker) { CardLinker = _linker; }
	public void SetCurrentTimeLineType(UIGachaEnum.E_TimeLineType _type) { CurrentTimeLineType = _type; }
	public void SetIsHidden(bool _isHidden) { IsHidden = _isHidden; }
	#endregion

	protected virtual void Initialize(ZUIFrameBase _frame) { }
}