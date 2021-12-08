using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ProBuilder;

public sealed class UIFrameDungeon : ZUIFrameBase
{
	/// <summary> 사용중인 탭순서와 맞어야함 </summary>
	private enum E_TabType
	{
		Tower,
		Boss,
		Trial_Sanctuary,
		Infinity_Tower,
	}

	[SerializeField] private UITowerInfo uiTowerInfo;
	[SerializeField] private UIBossWarInfo uiBossWarInfo;
	[SerializeField] private UITrialSanctuaryInfo uiTrialSanctuaryInfo;
	[SerializeField] private UIInfinityTowerInfo uiInfinityTowerInfo;

	[SerializeField] private ZText uiFrameTitle;
	[SerializeField] private ZText towerTitle;
	[SerializeField] private ZText TrialSanctuaryTitle;
	[SerializeField] private ZText InfinityTowerTitle;
	[SerializeField] private ZText BossWarTitle;

	private Camera subSceneCamera;
	[SerializeField] private GameObject BackGround;
	[SerializeField] private Transform rayTarget;
	[SerializeField] private ZToggle firstTabRadio;
	[SerializeField] private GameObject tabRoot;


	private E_TabType curTabType = E_TabType.Tower;
	private UIPopupItemInfo infoPopup;
	private Action modelLoadCompleted;

	private const string SUBSCENE_MODELVIEW = "UIModelView";
	private bool IsLoadScene = false;
	private Transform root;
	private Transform viewRoot;
	private GameObject loaded3DModel;
	private Light DirectionalLight;
	private float OriginLightIntensity;

	private Dictionary<E_ModelSocket, Transform> dicModelSocket = new Dictionary<E_ModelSocket, Transform>();
	public override bool IsBackable => true;
	private List<ITabContents> uiTabContentsList = new List<ITabContents>();

#if UNITY_EDITOR
	// 테스트코드
	public bool bAutoUpdateViewRoot = true;
	private void LateUpdate()
	{
		if (bAutoUpdateViewRoot == false || IsLoadScene == false) return;
		if (subSceneCamera != null) SetViewRootPosition();
	}
#endif

	protected override void OnInitialize()
	{
		uiTrialSanctuaryInfo.gameObject.SetActive(false);
		uiInfinityTowerInfo.gameObject.SetActive(false);

		base.OnInitialize();

		uiTabContentsList.Add(uiTowerInfo);
		uiTabContentsList.Add(uiBossWarInfo);
		uiTabContentsList.Add(uiTrialSanctuaryInfo);
		uiTabContentsList.Add(uiInfinityTowerInfo);

		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			uiTabContentsList[i].Initialize();
			uiTabContentsList[i].Index = i;
		}

		uiFrameTitle.text = DBLocale.GetText("WDungeon_Title");
		towerTitle.text = DBLocale.GetText("StageType_Tower");
		TrialSanctuaryTitle.text = DBLocale.GetText("StageType_Instance");
		InfinityTowerTitle.text = DBLocale.GetText("StageType_Infinity");
		BossWarTitle.text = DBLocale.GetText("WDungeon_BossWar");

		EnableTabButton(false);
	}

	private void EnableTabButton(bool bEnable)
	{
		foreach (var colliderObj in tabRoot.FindChildrenComponents<RectTransform>("Collider")) {
			colliderObj.gameObject.SetActive( bEnable );
		}
	}

	private void LoadSubScene()
	{
		ZSceneManager.Instance.OpenAdditive(SUBSCENE_MODELVIEW, (float _progress) => {
			ZLog.Log(ZLogChannel.Default, $"Scene loading progress: {_progress}");
		}, (temp) => {
			IsLoadScene = true;

			var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(SUBSCENE_MODELVIEW);
			if( root == null ) {
				foreach (GameObject obj in scene.GetRootGameObjects()) {
					if (obj.name == "Root") {
						root = obj.transform;
						DirectionalLight = root.GetComponentInChildren<Light>();
					}
				}
			}

			Vector3 targetPostiion = UIManager.Instance.transform.position;
			targetPostiion.z = 0;
			root.position += targetPostiion;
			OriginLightIntensity = DirectionalLight.intensity;
			DirectionalLight.intensity = 0.75f;

			if ( viewRoot == null ) {
				viewRoot = root.FindTransform("ViewRoot");
			}

			var SubSceneCameraTr = root.FindTransform("SubSceneCamera");
			if (SubSceneCameraTr != null) {
				SubSceneCameraTr.localPosition = new Vector3(0, 1.09f, -3.3f);

				subSceneCamera = SubSceneCameraTr.GetComponent<Camera>();
				if (subSceneCamera != null) {
					UIManager.Instance.DoSubCameraStack(subSceneCamera, true);
				}
			}

			OnSubSceneLoaded();
		});
	}

	private void OnSubSceneLoaded()
	{
		//방어코드인듯 로드완료했는데 frame이 꺼져있으면 다시 언로드
		if (Show == false) {
			UnloadSubScene();
			return;
		}

		CoroutineManager.Instance.NextFrame(() => {
			SetViewRootPosition();
		});

		modelLoadCompleted?.Invoke();
		modelLoadCompleted = null;
	}

	private void SetViewRootPosition()
	{
		var ray = subSceneCamera.ScreenPointToRay(subSceneCamera.WorldToScreenPoint(rayTarget.position));
		if (Physics.Raycast(ray, out var hitInfo, 100f)) {
			var vec = viewRoot.position;
			vec.x = hitInfo.point.x;
			viewRoot.position = vec;
		}
	}

	private void UnloadSubScene()
	{
		if (IsLoadScene) {
			IsLoadScene = false;

			if (DirectionalLight != null)
			{
				DirectionalLight.intensity = OriginLightIntensity;
			}

			if (subSceneCamera != null)
			{
				UIManager.Instance.DoSubCameraStack(subSceneCamera, false);
				subSceneCamera = null;
			}

			ZSceneManager.Instance.CloseAdditive(SUBSCENE_MODELVIEW, null);

		}
	}

	public void OnToggleValueChanged(int type)
	{
		if (curTabType == (E_TabType)type) {
			return;
		}
		OpenSubContents((E_TabType)type);
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		firstTabRadio.SelectToggle(false);
		OpenSubContents(E_TabType.Tower);

		UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.IncludeSubScene);
	}

    //[wisreal][2020.11.26]
    public void OpenDungeonUI(E_StageType stageType)
    {
        switch (stageType)
        {
            case E_StageType.Infinity:
                OpenSubContents(E_TabType.Infinity_Tower);
                break;
        }
    }

	private void OpenSubContents(E_TabType tabType)
	{
		curTabType = tabType;

		BackGround.SetActive(true);

		RemoveInfoPopup();

		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			if (uiTabContentsList[i].Index != (int)tabType) {
				uiTabContentsList[i].Close();
				continue;
			}

			if (tabType == E_TabType.Tower ||
				tabType == E_TabType.Trial_Sanctuary ||
				tabType == E_TabType.Boss) {

				if (IsLoadScene == false) {
					LoadSubScene();
					modelLoadCompleted += () => {
						uiTabContentsList[(int)tabType].Open();
						BackGround.SetActive(false);
					};
				}
				else {
					uiTabContentsList[i].Open();
					BackGround.SetActive(false);
				}
			}
			else {
				uiTabContentsList[i].Open();
			}
		}
	}

	protected override void OnHide()
	{
		CloseCurrentTab();

		UnloadSubScene();
		// 이전에 썼던 hud로!!
		UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
	}

	public void OnClose()
	{
		if (IsLoadScene == false) {
			return;
		}

		UIManager.Instance.Close<UIFrameDungeon>();
	}

	private void CloseCurrentTab()
	{
		for (int i = 0; i < uiTabContentsList.Count; ++i) {
			if (uiTabContentsList[i].Index == (int)curTabType) {
				uiTabContentsList[i].Close();
			}
		}
	}

	public void CloseItemInfoPopup()
	{
		uiInfinityTowerInfo?.CloseClearRewardItemPopup();
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

	public void ShowBossModel(Stage_Table stageTable)
	{
		var monsterTable = DBMonster.Get(stageTable.SummonBossID);
		if (monsterTable == null) {
			ZLog.LogError(ZLogChannel.Default, $"DBMonster가 null이다, SummonBossID:{stageTable.SummonBossID}");
			return;
		}

		var resourceTable = DBResource.Get(monsterTable.ResourceID);
		if (resourceTable == null) {
			ZLog.LogError(ZLogChannel.Default, $"DBResource가 null이다, ResourceID:{monsterTable.ResourceID}");
			return;
		}

		ShowModel(resourceTable.ResourceFile, monsterTable.ViewScale, stageTable.BossViewPosY);
	}

	private void ShowModel(string resFile, uint viewScale, float viewPosY)
	{
		EnableTabButton(false);

		if (string.IsNullOrEmpty(resFile) == false) {
			Addressables.InstantiateAsync(resFile).Completed += (obj) => {
				Action prepareAction = () => {
					if (loaded3DModel != null) {
						Addressables.ReleaseInstance(loaded3DModel);
						loaded3DModel = null;
					}

					EnableTabButton(true);

					loaded3DModel = obj.Result;
					loaded3DModel.SetLayersRecursively("UIModel");

					LODGroup lodGroup = loaded3DModel.GetComponent<LODGroup>();
					if (null != lodGroup) {
						lodGroup.ForceLOD(0);
					}

					loaded3DModel.transform.SetParent(viewRoot);

					Vector3 pos = Vector3.zero;
					pos.y = viewPosY;
					loaded3DModel.transform.localPosition = pos;
					loaded3DModel.transform.localScale = Vector3.one * viewScale * 0.01f;
					loaded3DModel.transform.localRotation = Quaternion.Euler(0, -30, 0);

					//소켓 세팅
					foreach (E_ModelSocket socket in Enum.GetValues(typeof(E_ModelSocket))) {
						Transform socketTrans = loaded3DModel.transform.FindTransform($"Socket_{socket}");

						if (null == socketTrans)
							continue;

						dicModelSocket[socket] = socketTrans;
					}
				};

				prepareAction.Invoke();
			};
		}
	}
}
