using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameDB;
using ZNet.Data;
using static DBMileageShop;
using UnityEngine.AddressableAssets;

/// <summary>
/// Active 는 외부에서 조작한다 . 
/// 
/// ljh : 이카루스 방침 - 클래스, 펠로우, 탈것 클릭시(따로 인터렉션 정하지 않을시) 본 팝업 띄워줌
/// 이름은 추후 수정
/// 
/// 디폴트 세팅은 공용 및 타입별 사용 컴포넌트(버튼, 타이틀 등) off
/// 후처리로 개별제어
/// </summary>
public class MileageChangePetPopup : ZUIFrameBase
{
    private enum E_PopupType // listCommonUI index
	{
        None = 0,
        MileageExchange = 1,
        Replace = 2,

        End = 3,
	}

    [Serializable]
    private class VIewTypeUIGroup
	{
        [SerializeField] private List<GameObject> listObj;

        public void SetState(bool b)
		{
            listObj.ForEach(item => item.SetActive(b));
		}
	}

    public override bool IsBackable => true; 

	[SerializeField] private UIAbilityListAdapter ScrollAdapter;

    [SerializeField] private Image imgCharacterClass;
    [SerializeField] private Image imgElement;
    [SerializeField] private GameObject collectionObj;          // 공용
    [SerializeField] private GameObject obtainCountRoot;
    [SerializeField] private Text txtObtainCount;

    [SerializeField] private Text txtUpperExName;
    [SerializeField] private Text txtName;

    [SerializeField] private RawImage imgRawModel;
    [SerializeField] private Camera modelRenderCamera;

    [SerializeField] private Transform modelRoot;

    // 공용 컴포넌트
    [SerializeField] private Text txtTitle;
    
    [SerializeField, Header("COMMON")] private List<VIewTypeUIGroup> listViewGroup = new List<VIewTypeUIGroup>();

    // PCR - 교체
    [SerializeField, Header("PCR_REPLACE")] private Text txtReplaceCost;
    [SerializeField] private Image imgReplaceCost;
    [SerializeField] private ZButton btnReplace;

    [SerializeField] private Text txtReplaceRemainCount;
    [SerializeField] private Text txtReplaceRemainTime;
    


    bool initDone;

    public uint CurTID { get; private set; }
    public bool IsLoading { get; private set; }

    private ModelResourceData modelResourceData;

    private RenderTexture modelRenderTex;

    private GameObject modelObj;

    private MileageShopAdvSceneRoot sceneController;
    private bool isLoadScene = false;
    private bool isReserveUnload = false;

    private bool willCreateModel;

    private Action onCloseRequest;
    private Action onAdvanceExchange;

    private Action<ZDefine.GachaKeepData> onClickReplace;
    private Action<ZDefine.GachaKeepData> onClickReplaceConfirm;

    private ZDefine.GachaKeepData replaceKeepData;

    private void Update()
    {
        if(sceneController != null && willCreateModel)
        {
            willCreateModel = false;
            CreateModel(true);
        }
    }

	protected override void OnHide()
	{
		base.OnHide();

        CancelInvoke();
	}

	#region Public Methods
    // 마일리지 상점
	public void Open(MileageBaseDataIdentifier identifier, Action onCloseReqCallback, Action onTryExchange)
    {
        if (SetDefault(identifier) == false)
            return;

        onCloseRequest = onCloseReqCallback;
        onAdvanceExchange = onTryExchange;

        txtTitle.text = DBLocale.GetText("Mileage_DetailPopup_Title");

        switch (identifier.dataType)
        {
            case MileageDataEvaluateTargetDataType.Change:
                SetUI_Change(DBChange.Get(identifier.tid));
                break;
            case MileageDataEvaluateTargetDataType.Pet:
                SetUI_Pet(DBPet.GetPetData(identifier.tid));
                break;
        }

        SetCommonUI(E_PopupType.MileageExchange);
    }

    // 펠로우/탈것/클래스 교체
    public void Open(ZDefine.GachaKeepData keepData, Action<ZDefine.GachaKeepData> onReplace, Action<ZDefine.GachaKeepData> onConfirm, Action onCloseReqCallback)
	{
        var dataType = MileageDataEvaluateTargetDataType.None;
		switch (keepData.KeepType)
		{
			case ZDefine.E_GachaKeepType.Pet:
            case ZDefine.E_GachaKeepType.Ride:
                dataType = MileageDataEvaluateTargetDataType.Pet;
                break;
			case ZDefine.E_GachaKeepType.Change:
                dataType = MileageDataEvaluateTargetDataType.Change;
                break;
		}

        MileageBaseDataIdentifier identifier = new MileageBaseDataIdentifier()
        {
            dataType = dataType,
            tid = keepData.Tid
        };

        if (SetDefault(identifier) == false)
            return;

        replaceKeepData = keepData;

        onClickReplace = onReplace;
        onClickReplaceConfirm = onConfirm;
        onCloseRequest = onCloseReqCallback;

        int leftCnt = (int)DBConfig.CardChange_Change_Count - (int)keepData.ReOpenCnt;

        btnReplace.interactable = leftCnt > 0;

        if (leftCnt <= 0)
        {
            leftCnt = 0;
        }

        imgReplaceCost.sprite = UICommon.GetSprite(DBItem.GetItemIconName(DBConfig.Diamond_ID));
        txtReplaceCost.text = DBConfig.CardChange_Diamond.ToString();

        RefreshReplaceRemainTime();

        txtReplaceRemainCount.text = DBLocale.GetText("PCR_Replace_ReplaceCount", leftCnt);


        switch (identifier.dataType)
        {
            case MileageDataEvaluateTargetDataType.Change:
                txtTitle.text = DBLocale.GetText("Title_Change_Select");

                SetUI_Change(DBChange.Get(identifier.tid));
                break;
            case MileageDataEvaluateTargetDataType.Pet:
                var data = DBPet.GetPetData(identifier.tid);
                
                if(data.PetType == E_PetType.Pet)
                    txtTitle.text = DBLocale.GetText("Title_Pet_Select");
                else
                    txtTitle.text = DBLocale.GetText("Title_Ride_Select");

                SetUI_Pet(data);
                break;
        }

        SetCommonUI(E_PopupType.Replace);

        InvokeRepeating(nameof(txtReplaceRemainTime), 1f, 1f);
    }

    public void Open(MileageBaseDataIdentifier identifier, Action onCloseReqCallback)
    {
        if (SetDefault(identifier) == false)
            return;

        onCloseRequest = onCloseReqCallback;

        switch (identifier.dataType)
        {
            case MileageDataEvaluateTargetDataType.Change:
                txtTitle.text = DBLocale.GetText("Change_Name");
                SetUI_Change(DBChange.Get(identifier.tid));
                break;
            case MileageDataEvaluateTargetDataType.Pet:
                var data = DBPet.GetPetData(identifier.tid);

                if (data.PetType == E_PetType.Pet)
                    txtTitle.text = DBLocale.GetText("Pet_Name");
                else
                    txtTitle.text = DBLocale.GetText("Vehicle_Name");

                SetUI_Pet(DBPet.GetPetData(identifier.tid));
                break;
        }

        SetCommonUI(E_PopupType.None);
    }


    private void RefreshReplaceRemainTime()
	{
        if (replaceKeepData == null)
            return;

        var remainTime = ((long)replaceKeepData.CreateDt + (long)DBConfig.CardChange_ChangeTime - (long)TimeManager.NowSec);

        if (remainTime < 0)
        {
            txtReplaceRemainTime.text = DBLocale.GetText("PCR_Replace_Timeout");
            btnReplace.interactable = false;
        }
        else
            txtReplaceRemainTime.text = TimeHelper.GetRemainTime((ulong)remainTime);
    }

    private bool SetDefault(MileageBaseDataIdentifier identifier)
	{
        SetDefaultCommonUI();

        if (initDone == false)
        {
            initDone = Initialize();
        }

        if (initDone == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Issue Occured");
            return false;
        }

        if (sceneController == null)
        {
            LoadSubScene();
        }

        CurTID = identifier.tid;

        if (CheckDataType(identifier.dataType) == false)
            return false;

        return true;
    }

    private bool CheckDataType(MileageDataEvaluateTargetDataType type)
	{
        if (type != MileageDataEvaluateTargetDataType.Change &&
            type != MileageDataEvaluateTargetDataType.Pet)
        {
            ZLog.LogError(ZLogChannel.UI, "Only Change and Pet supported now");
            return false;
        }
        return true;
    }

    // 공용컴포넌트 off
    private void SetDefaultCommonUI()
	{
        txtTitle.text = string.Empty;

        for(int i = 0;i < listViewGroup.Count; i++)
		{
            listViewGroup[i].SetState(false);
		}
    }

    // 개별 컨텐츠 컴포넌트 on
    private void SetCommonUI(E_PopupType type)
	{
        int typeIdx = (int)type;
        if (typeIdx <= 0 || typeIdx >= listViewGroup.Count)
            return;

        listViewGroup[typeIdx].SetState(true);
	}

    public void OnClose()
    {
        CancelInvoke();

        ReleaseModel();
        IsLoading = false;
        CurTID = 0;
    }

    public void OnCloseBtnClicked()
    {
        CancelInvoke();

        onCloseRequest?.Invoke();
        onCloseRequest = null;
    }

    public void OnClickAdvanceObtainBtn()
    {
        onAdvanceExchange?.Invoke();
    }

    public void OnClickReplace()
	{
        onClickReplace?.Invoke(replaceKeepData);
	}

    public void OnClickReplaceConfirm()
	{
        onClickReplaceConfirm?.Invoke(replaceKeepData);
	}

    #endregion

    #region Private Methods
    private bool Initialize()
    {
        ScrollAdapter.Initialize_SkipPrefabManualLoading();

        this.modelRenderTex = new RenderTexture((int)imgRawModel.rectTransform.rect.width, (int)imgRawModel.rectTransform.rect.height, 16, RenderTextureFormat.ARGB32);
        this.modelRenderTex.Create();

        imgRawModel.texture = this.modelRenderTex;

        if (modelRenderTex != null)
        {
            this.modelRenderCamera.targetTexture = this.modelRenderTex;
        }

        return true;
    }

    private void SetUI_Change(Change_Table data)
    {
        if (data == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Data is NULL");
            return;
        }

        int obtainCnt = Me.CurCharData.GetChangeCount(data.ChangeID);

        SetUI(
            displayCharacterClassImg: true
            , displayElementImage: true
            , displayUpperExName: true
            , displayObtainCountStatus: obtainCnt > 0
            , displayCollection: false
            , characterClassSprite: ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetCharacterTypeSprite(data.UseAttackType))
            , elementSprite: ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetAttributeSpriteName(data.AttributeType))
            , mainName: DBLocale.GetText(data.ChangeTextID)
            , upperExName: string.Format("{0}, {1}", DBLocale.GetText(data.UseAttackType.ToString()), DBLocale.GetText(UIFrameMileage.GetAttributeName(data.AttributeType)))
            , obtainedCount: obtainCnt);

        ScrollAdapter.RefreshListData(UIStatHelper.GetChangeStat(data));

        RefreshModelResourceData(data);
        // CreateModel(true);
        willCreateModel = true;
    }

    private void SetUI_Pet(Pet_Table data)
    {
        if (data == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Data is NULL");
            return;
        }

        int obtainCnt = Me.CurCharData.GetPetCount(data.PetID);

        SetUI(
            displayCharacterClassImg: false
            , displayElementImage: false
            , displayUpperExName: false
            , displayObtainCountStatus: obtainCnt > 0
            , displayCollection: false
            , mainName: DBLocale.GetText(data.PetTextID)
            , obtainedCount: obtainCnt);

        ScrollAdapter.RefreshListData(UIStatHelper.GetPetStat(data, IncludeExtraStat: false));

        RefreshModelResourceData(data);
        // CreateModel(true);
        willCreateModel = true;
    }

    //private void RefreshAbilityData(List<uint> abilityActionIDs)
    //{
    //    var dataList = new List<UIAbilityData>();

    //    for (int i = 0; i < abilityActionIDs.Count; i++)
    //    {
    //        DBAbilityAction.GetAbilityTypeList(abilityActionIDs[i], ref dataList);
    //    }

    //    dataList.ForEach(t => t.viewType = E_UIAbilityViewType.Ability);

    //    ScrollAdapter.RefreshListData(dataList);
    //}

    private void SetUI(
        bool displayCharacterClassImg = false
        , bool displayElementImage = false
        , bool displayUpperExName = false
        , bool displayObtainCountStatus = false
        , bool displayCollection = false
        , Sprite characterClassSprite = null
        , Sprite elementSprite = null
        , string mainName = ""
        , string upperExName = ""
        , int obtainedCount = 0)
    {
        imgCharacterClass.gameObject.SetActive(displayCharacterClassImg);
        imgElement.gameObject.SetActive(displayElementImage);
        collectionObj.SetActive(displayCollection);
        obtainCountRoot.SetActive(displayObtainCountStatus);
        txtUpperExName.gameObject.SetActive(displayUpperExName);

        txtName.text = mainName;

        if (displayCharacterClassImg)
        {
            imgCharacterClass.sprite = characterClassSprite;
        }

        if (displayElementImage)
        {
            imgElement.sprite = elementSprite;
        }

        if (displayObtainCountStatus)
        {
            txtObtainCount.text = obtainedCount.ToString();
        }

        if (displayUpperExName)
        {
            txtUpperExName.text = upperExName;
        }

        if (displayCollection)
        {
            /// Nothing
        }
    }

    /// <summary>
    /// 모델 띄움 
    /// </summary>
    private void CreateModel(bool useLobbyModel)
    {
        if (string.IsNullOrEmpty(modelResourceData.FileName))
        {
            ZLog.LogError(ZLogChannel.UI, "fileNameNULL or empty");
            return;
        }

        string fileName = modelResourceData.FileName;

        if (useLobbyModel)
        {
            fileName += "_LOBBY";
        }

        IsLoading = true;

        Addressables.InstantiateAsync(fileName).Completed += (obj) =>
        {
            ReleaseModel();

            IsLoading = false;

            if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
            {
                if (useLobbyModel)
                {
                    CreateModel(false);
                }

                return;
            }

            this.modelObj = obj.Result;

            if (this.modelObj == null)
                return;

            //ResetModelAdjustRoot();

            this.modelObj.SetLayersRecursively("UIModel");

            LODGroup lodGroup = this.modelObj.GetComponent<LODGroup>();
            if (null != lodGroup)
                lodGroup.ForceLOD(0);

            this.modelObj.transform.SetParent(sceneController.SpawnRoot); // modelRoot);

            Vector3 pos = Vector3.zero;
            pos.y = modelResourceData.ViewPosY;
            this.modelObj.transform.SetLocalTRS(pos, Quaternion.identity, Vector3.one * modelResourceData.ViewScale * sceneController.scaleMultiplier);

            if (modelRenderTex != null && imgRawModel.gameObject.activeSelf == false)
            {
                imgRawModel.gameObject.SetActive(true);
            }
        };
    }

    private void ReleaseModel()
    {
        if (modelObj != null)
        {
            Addressables.ReleaseInstance(modelObj);
            modelObj = null;
        }
    }

    private void RefreshModelResourceData(Change_Table data)
    {
        modelResourceData = new ModelResourceData(DBResource.GetResourceFileName(data.ResourceID), data.ViewScale, data.ViewScaleLocY);
    }

    private void RefreshModelResourceData(Pet_Table data)
    {
        modelResourceData = new ModelResourceData(data.ResourceFile, data.ViewScale, data.ViewScaleLocY);
    }

    public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData)
    {
        if (modelObj == null)
            return;

        UICommon.RotateObjectDrag(eventData, modelObj);
    }

    public void Release()
    {
        willCreateModel = false;

        if (sceneController != null)
        {
            UnloadSubScene();
        }
        
        if (this.modelRenderTex)
        {
            this.modelRenderTex.Release();
            modelRenderTex = null;
        }
    }
    #endregion


    /// <summary>
    ///  +++++++++ 
    /// </summary>
    private void LoadSubScene()
    {
        ZSceneManager.Instance.OpenAdditive(ZUIConstant.SUB_SCENE_MILEAGESHOP_POPUP,
                            (float _progress) =>
                            {
                                ZLog.Log(ZLogChannel.Default, $"Scene loading progress: {_progress}");
                            },
                            (onLoadEnd) =>
                            {
                                OnLoadSubScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(ZUIConstant.SUB_SCENE_MILEAGESHOP_POPUP));
                            });
    }

    private void OnLoadSubScene(UnityEngine.SceneManagement.Scene scene)
    {
        if (isReserveUnload)
        {
            UnloadSubScene();
            return;
        }

        foreach (var iter in scene.GetRootGameObjects())
        {
            if (iter.name.Equals("Root"))
            {
                iter.transform.position = new Vector3(-9500, -9500, 0);

                sceneController = iter.GetComponent<MileageShopAdvSceneRoot>();

                Vector2 size = new Vector2(imgRawModel.rectTransform.rect.width, imgRawModel.rectTransform.rect.height);
                imgRawModel.texture = sceneController.SetRenderTexture(size);
            }
        }

        isLoadScene = true;
    }

    public void UnloadSubScene()
    {
        ZSceneManager.Instance.CloseAdditive(ZUIConstant.SUB_SCENE_MILEAGESHOP_POPUP, null);
        sceneController = null;
    }

    #region Define
    public struct ModelResourceData
    {
        public string FileName;
        public uint ViewScale;
        public float ViewPosY;

        public ModelResourceData(string fileName, uint viewScale, float viewPosY)
        {
            FileName = fileName;
            ViewScale = viewScale;
            ViewPosY = viewPosY;
        }
    }
    #endregion
}
