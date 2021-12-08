using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using ZNet.Data;
using System;

public class UIFrameGodLandMap : ZUIFrameBase
{
	private enum E_MapMode
	{
		World,
		Local,
	}

	[SerializeField] private UIGodLandBattleRecord uiBattleRecord;
	[SerializeField] private UIGodLandLocalViewer uiLocalViewer;
	[SerializeField] private UIGodLandLocalDetail uiLocalDetail;
	[SerializeField] private UIGodLandMapMyOccupy uiMyOccupy;
	[SerializeField] private UIPopupGodLandResult uiResultPopup;
	[SerializeField] private ZText MapTitle;
	[SerializeField] private GameObject productionObj;
	[SerializeField] private ZText productionName;
	[SerializeField] private ZImage productionImage;
	[SerializeField] private ZText productionAvg;
	[SerializeField] private RawImage ImageWorld;
	[SerializeField] private RawImage ImageLocal;
	[SerializeField] private GameObject RootWorld;
	[SerializeField] private GameObject RootLocal;
	[SerializeField] private UIGodLandMapWorldItem worldMapItemPf;
	[SerializeField] private UIGodLandMapLocalItem localMaptemPf;
	[SerializeField] private Transform worldMapPointRoot;
	[SerializeField] private Transform localMapPointRoot;
	[SerializeField] private GameObject localViewerButton;
	[SerializeField] private GameObject battleRecordRedDot;
	[SerializeField] private ZScrollRect ScrollWorldMap;
	[SerializeField] private ZScrollRect ScrollLocalMap;

	private const string WORLD_MAP_IMG_NAME = "img_worldmap_01";
	private const int GOD_LAND_STAGE_ID = 5000002;

	private List<UIGodLandMapWorldItem> worldMapItemList = new List<UIGodLandMapWorldItem>();
	private List<UIGodLandMapLocalItem> localMapItemList = new List<UIGodLandMapLocalItem>();
	private E_MapMode curMapMode = E_MapMode.World;
	private uint selectGroupId;
	private Dictionary<uint, uint> selectLocalIds = new Dictionary<uint, uint>();
	public override bool IsBackable => true;

	protected override void OnInitialize()
	{
		base.OnInitialize();

		productionName.text = DBLocale.GetText("GodLand_Time_Production");

		uiBattleRecord.Initialize();
		uiLocalViewer.Initialize(OnClickedLocalViwerItem);
		uiLocalDetail.Initialize(OnClickedLocalDetailAction, OnClickedLocalDetailClose);
		uiMyOccupy.Initialize(OnClickedMyInfoQuickMove);

		worldMapItemPf.gameObject.SetActive(false);
		localMaptemPf.gameObject.SetActive(false);
	}

	public bool CheckRedDot()
	{
		return uiMyOccupy.IsActiveRedDot() || uiBattleRecord.IsActiveRedDot();
	}

	protected override void OnRemove()
	{
		base.OnRemove();
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.IncludeSubScene);

		UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
			new List<uint>() { DBConfig.Crown_ID, DBConfig.Godland_Production_ID });

		OpenWorldMap();
	}

	protected override void OnHide()
	{
		base.OnHide();

		uiBattleRecord.Hide();
		uiLocalViewer.Hide();
		uiLocalDetail.Hide();
		uiResultPopup.Hide();

		UIManager.Instance.Find<UISubHUDCurrency>().ShowBaseCurrency();
		UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
	}

	/// <summary> 월드맵 오픈 </summary>
	private void OpenWorldMap()
	{
		curMapMode = E_MapMode.World;

		Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
			uiMyOccupy.Show(isHave, myData);
		});

		Me.CurCharData.GodLandContainer.REQ_GetGodLandFightRecord(false, (list) => {
			bool bChanged = Me.CurCharData.GodLandContainer.IsBattleRecordChanged(list);
			battleRecordRedDot.SetActive(bChanged);
		});

		//월드맵에서 열으면 안되는 로컬정보 닫어주기
		uiLocalViewer.Hide();
		uiLocalDetail.Hide();

		MapTitle.text = DBLocale.GetText("Holy_Ground_Text");
		productionObj.SetActive(false);

		RootWorld.SetActive(true);
		RootLocal.SetActive(false);
		localViewerButton.SetActive(false);

		Vector2 curScrollPos = ScrollWorldMap.normalizedPosition;
		if (curScrollPos == Vector2.one / 2f) {
			curScrollPos = new Vector2(0.2f, 0.21f);
		}

		LoadMapBackgroundImage(true, ImageWorld, WORLD_MAP_IMG_NAME, curScrollPos);

		RefreshWorldMap();
	}

	private void RefreshWorldMap()
	{
		var worldTableList = DBGodLand.GetGroupMainList();
		for (int i = 0; i < worldMapItemList.Count; i++) {
			worldMapItemList[i].gameObject.SetActive(false);
		}

		int loadItemCount = worldTableList.Count - worldMapItemList.Count;

		for (int i = 0; i < loadItemCount; ++i) {
			var obj = Instantiate(worldMapItemPf);
			obj.transform.SetParent(worldMapPointRoot);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			obj.gameObject.SetActive(false);
			obj.Initialize();
			worldMapItemList.Add(obj);
		}

		for (int i = 0; i < worldTableList.Count; i++) {
			var info = worldTableList[i];
			var item = worldMapItemList[i];
			bool toggle = (selectGroupId == i) ? true : false;
			item.DoUpdate(info, OnClickedWorldListItem, toggle);
		}
	}

	/// <summary> 로컬맵 오픈 </summary>

	private void OpenLocalMap(uint _selectGroupID, Action callback=null)
	{
		var prepareList = new List<GodLandSpotInfoConverted>();
		var tables = DBGodLand.GetListByGroupID(_selectGroupID);
		for (int i = 0; i < tables.Count; ++i) {
			prepareList.Add(new GodLandSpotInfoConverted(tables[i]));
		}

		// 서버 요청없이 먼저 맵을 생성하고
		RefreshLocalMap(prepareList, false);

		// 서버 요청 후 남은 데이타 세팅한다
		Me.CurCharData.GodLandContainer.REQ_GetGodLandInfo(_selectGroupID, true, (list) => {
			RefreshLocalMap(list, true);

			localViewerButton.SetActive(true);

			callback?.Invoke();
		});
	}

	private void RefreshLocalMap(List<GodLandSpotInfoConverted> localMapInfoList, bool fromServer)
	{
		curMapMode = E_MapMode.Local;

		if (fromServer == false) {
			RootWorld.SetActive(false);
			RootLocal.SetActive(true);

			var table = DBGodLand.GetByGroupID(selectGroupId);
			MapTitle.text = table.GodLandUpperTextID;

			productionObj.SetActive(false);

			LoadMapBackgroundImage(false, ImageLocal, table.MapFileName, Vector2.one / 2f);
		}
		else {
			productionObj.SetActive(true);

			var firstInfo = localMapInfoList[0];

			var itemTable = DBItem.GetItem(firstInfo.GodLandTable.ProductionItemID);
			productionImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);

			float amount = ((float)3600 / firstInfo.GodLandTable.ProductionTime) *
				firstInfo.GodLandTable.ProductionItemCount;

			productionAvg.text = string.Format("{0:0}", amount);
		}

		for (int i = 0; i < localMapItemList.Count; i++) {
			localMapItemList[i].gameObject.SetActive(false);
		}

		int loadItemCount = localMapInfoList.Count - localMapItemList.Count;

		for (int i = 0; i < loadItemCount; ++i) {
			var obj = Instantiate(localMaptemPf);
			obj.transform.SetParent(localMapPointRoot);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			obj.gameObject.SetActive(false);
			obj.Initialize();
			localMapItemList.Add(obj);
		}

		for (int i = 0; i < localMapInfoList.Count; i++) {
			var info = localMapInfoList[i];
			var item = localMapItemList[i];
			item.DoUpdate(info, fromServer, OnClickedLocalListItem);
		}
	}

	public void SetSelectLocalID(uint _selectGroupId, uint _selectLocalGodLandTid)
	{
		if (selectLocalIds.ContainsKey(_selectGroupId) == false) {
			selectLocalIds.Add(_selectGroupId, _selectLocalGodLandTid);
		}
		else {
			selectLocalIds[_selectGroupId] = _selectLocalGodLandTid;
		}
	}

	public uint GetSelectLocalID(uint _selectGroupId)
	{
		if (selectLocalIds.ContainsKey(_selectGroupId) == false) {
			selectLocalIds.Add(_selectGroupId, 0);
		}
		return selectLocalIds[_selectGroupId];
	}

	private void LoadMapBackgroundImage( bool isWorldMap, RawImage _TargetImage, string _ImageName, Vector2 scrollPosition)
	{
		_TargetImage.CrossFadeAlpha(0, 0, true);

		if (_TargetImage.texture == null || _TargetImage.texture.name != _ImageName) {
			if (_TargetImage.texture != null) {
				Addressables.Release<Texture>(_TargetImage.texture);
			}

			Addressables.LoadAssetAsync<Texture>(_ImageName).Completed += (AsyncOperationHandle<Texture> _Result) => {
				if (_Result.Status == AsyncOperationStatus.Succeeded) {
					_TargetImage.texture = _Result.Result;
					_TargetImage.CrossFadeAlpha(1, 0.45f, true);

					if (isWorldMap) {
						ScrollWorldMap.normalizedPosition = scrollPosition;
					}
					else {
						ScrollLocalMap.normalizedPosition = scrollPosition;
					}
				}
			};
		}
		else {
			_TargetImage.CrossFadeAlpha(1, 0.45f, true);

			if (isWorldMap) {
				ScrollWorldMap.normalizedPosition = scrollPosition;
			}
			else {
				ScrollLocalMap.normalizedPosition = scrollPosition;
			}
		}
	}

	// 버튼 이벤트 /////////////////////////////////////////////////////////

	/// <summary> 최상단 월드 or 로컬 나가기 버튼 클릭 </summary>
	public void OnClickExit()
	{
		if (curMapMode == E_MapMode.World) {
			Close();
		}
		else {
			OpenWorldMap();
		}
	}

	/// <summary> 전투기록 버튼 클릭 </summary>
	public void OnClickBattleRecord()
	{
		uiBattleRecord.Show();
	}

	/// <summary> 점유지 리스트 버튼 클릭 </summary>
	public void OnClickLocalViwer()
	{
		uiLocalViewer.Show(selectGroupId, GetSelectLocalID(selectGroupId));
	}

	/// <summary> 월드 아이템 클릭해 로컬로 이동 </summary>
	private void OnClickedWorldListItem(uint _selectGroupId)
	{
		Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
			uiMyOccupy.Show(isHave, myData);
		});

		selectGroupId = _selectGroupId;

		OpenLocalMap(_selectGroupId);
	}

	/// <summary> 로컬 아이템 클릭해 상세정보 보기 </summary>
	private void OnClickedLocalListItem(uint godLandTid)
	{
		Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
			uiMyOccupy.Show(isHave, myData);
			uiLocalDetail.Show(godLandTid, isHave, myData);
		});

		SetSelectLocalID(selectGroupId, godLandTid);
	}

	/// <summary> 점유지 리스트 아이템 클릭 ( +로컬아이템 클릭효과 ) </summary>
	private void OnClickedLocalViwerItem(uint godLandTid)
	{
		//뷰어리스트 갱신
		uiLocalViewer.Refresh(godLandTid);

		//로컬아이템을 클릭
		OnClickedLocalListItem(godLandTid);
	}

	/// <summary> 나의 점유지 바로가기 클릭 </summary>
	private void OnClickedMyInfoQuickMove(uint godLandTid)
	{
		Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
			uiMyOccupy.Show(isHave, myData);
			uiLocalDetail.Show(godLandTid, isHave, myData);
		});

		var table = DBGodLand.Get(godLandTid);

		selectGroupId = table.SlotGroupID;

		OpenLocalMap(table.SlotGroupID);
	}

	/// <summary> 점유지 상세보기 액션(획득 및 강탈) 클릭 </summary>
	private void OnClickedLocalDetailAction(E_TargetType ownerType, uint godLandTid)
	{
		var container = Me.CurCharData.GodLandContainer;
		if (ownerType == E_TargetType.Self) {
			// 재화획득
			container.REQ_GetGodLandSpotGatheringItem(godLandTid, (getItemTid, getCnt, timeCnt) => {
				uiResultPopup.ShowCompletePopup(getItemTid, getCnt, timeCnt);
			});
		}
		else {
			// 강탈하러 맵이동
			container.REQ_GetMatchGodLandSpot(godLandTid, () => {
				var stageTable = DBStage.Get(GOD_LAND_STAGE_ID);
				var portalTable = DBPortal.Get(stageTable.DefaultPortal);
				var invenItem = Me.CurCharData.GetInvenItemUsingMaterial(portalTable.UseItemID);
				ulong ivenItemId = (invenItem != null) ? invenItem.item_id : 0;

				AudioManager.Instance.PlaySFX(30004); //입장사운드
				ZGameManager.Instance.TryEnterStage(
					portalTable.PortalID, false, ivenItemId, portalTable.UseItemID);

				Close();
			}, () => {
				//포기콜백오면 새롭게 갱신
				OpenLocalMap(selectGroupId, ()=> {
					Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
						uiMyOccupy.Show(isHave, myData);
						uiLocalDetail.Show(godLandTid, isHave, myData);
					});
				});
			} );
		}
	}

	/// <summary> 점유지 상세보기 닫기 클릭 </summary>
	private void OnClickedLocalDetailClose()
	{
		uiLocalViewer.Hide();

		uiLocalDetail.Hide();
	}

	/// <summary> 점유지 획득 팝업 확인시 새롭게 정보갱신 </summary>
	public void OnClickGatheringCompleted()
	{
		uiResultPopup.Hide();

		Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
			uiMyOccupy.Show(isHave, myData);
			uiLocalDetail.Show(myData.GodLandTid, isHave, myData);
		});

		OpenLocalMap(selectGroupId);
	}
}
