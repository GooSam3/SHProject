using Com.TheFallenGames.OSA.Core;
using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZDefine;
using ZNet;
using ZNet.Data;
using static UIFrameArtifactAbilityActionBuilder;

public class UIFrameArtifactManufacture : MonoBehaviour
{
    public enum MiddleTab
    {
        None = 0,
        Look, // 외관 출력 탭
        Stat // 스탯 출력 탭
    }

    public enum EquipActionCase
    {
        None = 0,
        Equip, // 장착 
        Switch, // 해제 후 새로운애 장착 
        Unequip
    }

    [Serializable]
    public class SingleMiddleTab
    {
        public MiddleTab type;
        public Toggle toggle;
        public List<GameObject> activeObj;
    }

    [Serializable]
    public class UIGroup_Look
    {
        [Serializable]
        public class ArtifactItemEffect
        {
            public byte grade;
            public GameObject obj;
        }

        public Image imgItem;
        // Artifact Grade 로 넣음 . 
        public List<ArtifactItemEffect> effects;
    }

    [Serializable]
    public class UIGroup_Stats
    {
        public ScrollRect scrollRect;

        public Image imgFirstAbilityIcon;
        public Text txtFirstAbilityTitle;
        public Text txtFirstAbilityValue;
        public Text txtGradeTitle;
        public Text txtGrade;

        public Text txtCharacterClass;
        public Text txtArtifactName;
        public Text txtArtifactItemName;

        public GameObject petCombiBuffObj;
        public Text txtPetCombiBuffTitle;

        public Button btnAdjustStepPrev;
        public Button btnAdjustStepNext;

        public Color statTitleActiveColor;

        [HideInInspector]
        public List<UIFrameArtifactSingleAbilityAction> mainAbilityActionTextsCached;
        [HideInInspector]
        public List<UIFrameArtifactSingleAbilityAction> petCombiAbilityActionTextsCached;
    }

    [Serializable]
    public class UIGroup_Material
    {
        public Text txtArtifactName;
        public RectTransform slotParent;

        public RectTransform costObjParent;

        public Color costTxtColor_enough;
        public Color costTxtColor_notEnough;
        public Color costTxtColor_requiredCost;

        public List<UIFrameArtifactManufactureCost> costItems;

        public Text txtEquipOrUnequip;
        public Button btnEquipOrUnequip;

        public Button btnReset;
        public Button btnAutoSetting;
        public Button btnManufacture;

        public Text txtManufacture;

        public Text txtSuccessRate;

        public Color inactivatedTextColor;

        public Image imgRequiredManufactureProtectorItem;
        public Text txtRequiredManufactureProtectorItemCnt;

        public List<GameObject> activeOnMaxStep;

        public Toggle toggleUseProtector;

        public UIFrameArtifactManufactureMaterialHandler materialHandler;
    }

    #region SerializedField
    #region Preference Variable
    [SerializeField] private MiddleTab defaultMiddleTab = MiddleTab.Look;
    #endregion

    [SerializeField] private ScrollArtifactManufactureItemListAdapter ScrollAdapter;
    [SerializeField] private List<GameObject> activeObjs;
    [SerializeField] private List<SingleMiddleTab> manufactureMiddleTabs;

    #region UI Variables
    [SerializeField] private UIGroup_Look uiGroup_look;
    [SerializeField] private UIGroup_Stats uiGroup_stat;
    [SerializeField] private UIGroup_Material uiGroup_material;

    /// <summary>
    /// 메인 Ability Action 게임옵젝 원본 
    /// </summary>
    [SerializeField] private UIFrameArtifactSingleAbilityAction mainAbilityActionSourceObj;
    /// <summary>
    /// 펫 콤보 Ability Action 게임옵젝 원본 
    /// </summary>
    [SerializeField] private UIFrameArtifactSingleAbilityAction petCombiAbilityActionSourceObj;
    [SerializeField] private UIFrameArtifactManufactureCost costSourceObj;

    [SerializeField] private RectTransform mainAbilityActionTextParent;
    [SerializeField] private RectTransform petCombiAbilityActionTextParent;
    #endregion
    #endregion

    #region System Variables
    private UIFrameArtifact ArtifactFrame;

    private MiddleTab prevSelectedMiddleTab;
    private MiddleTab curSelectedMiddleTab;

    private bool blockClickEvents;

    /// <summary>
    /// 슬롯 및 Stat UI 서 Select 관련 
    /// </summary>
    private uint selectedArtifactIDOnScroll; // 왼쪽 슬롯에서 클릭된 아티팩트 ID (눈에 보이는, 즉 보유중일때는 현재 내가 보유중인애가 보이고 미보유중이면 가장 낮은 Step 의 아티팩트가됨)
    private uint targetOperationArtifactIDOnScroll; // 현재 제작 대상인 아티팩트 아이디 . 보유중/미보유중 일때 다른 selectedArtifactIDOnScroll 값이 들어가기때문에 별도로 관리 
    private uint selectedArtifactGroupID;
    private byte selectedArtifactIDStep;
    private uint selectedArtifactDisplayableMinStep; // 가운데에서 보여지고 있는 아티팩트 단계의 ID 
    private bool isSelectedArtifactUpgradeable;
    private bool isSelectedArtifactLastStep;
    //    private bool isSelectedArtifactUpgradable; // 제작/승급이 가능한 상태인가? 
    /// <summary>
    ///  현재 Stat 창에서 '보여지고' 있는 Artifact 의 ID . (슬롯이 아님 주의.)
    /// </summary>
    private uint curShownArtifactIDOnStat;

    ///// <summary>
    ///// 현재 성공 확률 
    ///// </summary>
    //private uint curSuccessRawRate;
    //private uint curSuccessRateConverted;

    private List<AbilityActionTitleValuePair> curSelectedArtifactMainAbilityActionValues;
    private List<AbilityActionTitleValuePair> curSelectedArtifactPetCombiAbilityActionValues;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    private void Update()
    {

    }
    #endregion

    #region Public Methods
    public void Initialize(UIFrameArtifact artifactFrame)
    {
        uiGroup_stat.mainAbilityActionTextsCached = new List<UIFrameArtifactSingleAbilityAction>();
        uiGroup_stat.petCombiAbilityActionTextsCached = new List<UIFrameArtifactSingleAbilityAction>();
        curSelectedArtifactMainAbilityActionValues = new List<AbilityActionTitleValuePair>(5);
        curSelectedArtifactPetCombiAbilityActionValues = new List<AbilityActionTitleValuePair>(5);

        ArtifactFrame = artifactFrame;
        InitScrollAdapter();

        activeObjs.ForEach(t => t.SetActive(false));

        foreach (var tabObj in manufactureMiddleTabs)
        {
            tabObj.activeObj.ForEach(t => t.SetActive(false));
        }

        uiGroup_material.materialHandler.AddListener_OnRegisterDone(OnMaterialRegisterDone);
        uiGroup_material.materialHandler.Initialize(artifactFrame, false, true);
        uiGroup_material.materialHandler.SetGetter_SelectedArtifactID(GetTargetOperationArtifactID);
    }

    public void Open(uint? desiredFirstArtifactID, MiddleTab? desiredFirstMiddleTab)
    {
        blockClickEvents = false;

        uint firstArtifactID = desiredFirstArtifactID.HasValue ? desiredFirstArtifactID.Value : 0;
        MiddleTab firstMiddleTab = desiredFirstMiddleTab.HasValue ? desiredFirstMiddleTab.Value : defaultMiddleTab;

        if (firstMiddleTab == MiddleTab.None)
            firstMiddleTab = defaultMiddleTab;

        /// shortcut 용 분기 
        if (firstArtifactID == 0)
        {
            RefreshArtifactScroll(true, true, 1f);
        }
        else
        {
            SetSelectedArtifactIDOnScroll(firstArtifactID);
            RefreshArtifactScroll(true, false);
            ScrollAdapter.SnapTo(firstArtifactID);
        }

        SetMiddleLookTab(firstMiddleTab);

        uiGroup_material.materialHandler.ResetAll(isSelectedArtifactLastStep ? 0 : targetOperationArtifactIDOnScroll);

        UpdateUI_MaterialAndETC();

        gameObject.SetActive(true);
        activeObjs.ForEach(t => t.SetActive(true));
    }

    public void Hide()
    {
        // 탭간 이동할때 마지막으로 이동시키기 위해 none 처리 X     curSelectedMiddleTab = MiddleTab.None;
        //        prevSelectedMiddleTab = MiddleTab.None;

        gameObject.SetActive(false);
        activeObjs.ForEach(t => t.SetActive(false));

        // manufactureMiddleTabs.ForEach(t => t.toggle.isOn = false);
        // manufactureMiddleTabs.Find(t => t.type == defaultMiddleTab).toggle.isOn = true;

        uiGroup_material.activeOnMaxStep.ForEach(t => t.SetActive(false));

        uiGroup_material.toggleUseProtector.isOn = false;

        uiGroup_material.materialHandler.Release();
    }

    public void Release()
    {
        //manufactureMiddleTabs.ForEach(t => t.toggle.isOn = false);
        //manufactureMiddleTabs.Find(t => t.type == defaultMiddleTab).toggle.isOn = true;

        //curSelectedMiddleTab = MiddleTab.None;
        //prevSelectedMiddleTab = MiddleTab.None;
    }

    /// <summary>
    /// 현재 보이는 화면 그대로 갱신 
    /// </summary>
    public void RefreshCurrentUI(bool selectFirstSlot, bool forceResetScroll)
    {
        RefreshArtifactScroll(true, selectFirstSlot);
        SetMiddleLookTab(curSelectedMiddleTab);
        if (selectFirstSlot || forceResetScroll)
        {
            uiGroup_material.materialHandler.ResetAll(isSelectedArtifactLastStep ? 0 : targetOperationArtifactIDOnScroll);
        }
        UpdateUI_MaterialAndETC();
    }

    public MiddleTab GetCurSelectedMiddleTab()
    {
        return curSelectedMiddleTab;
    }

    public MiddleTab GetPrevSelectedMiddleTab()
    {
        return prevSelectedMiddleTab;
    }

    public uint GetCurSelectedArtifactID()
    {
        return selectedArtifactIDOnScroll;
    }

    public uint GetTargetOperationArtifactID()
    {
        return targetOperationArtifactIDOnScroll;
    }
    #endregion

    #region Private Methods
    private void InitScrollAdapter()
    {
        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollArtifactManufactureItemListSlot));
        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);
        ScrollAdapter.AddListener_OnClick(OnMainScrollSlotClicked);
        ScrollAdapter.Initialize();
    }

    private void SetMiddleLookTab(MiddleTab tab)
    {
        var target = manufactureMiddleTabs.Find(t => t.type == tab);

        if (target == null)
            return;

        target.toggle.isOn = true;

        prevSelectedMiddleTab = curSelectedMiddleTab;
        curSelectedMiddleTab = tab;
        UIFrameArtifact._ManufactureRuntimeShortData.middleTab = tab;

        switch (tab)
        {
            case MiddleTab.Look:
                {
                    UpdateUI_Look();
                }
                break;
            case MiddleTab.Stat:
                {
                    UpdateUI_Stat();
                }
                break;
        }

        foreach (var t in manufactureMiddleTabs)
        {
            t.activeObj.ForEach(obj => obj.SetActive(t.type == tab));
        }
    }

    /// <summary>
    /// 아티팩트 스크롤 데이터 세팅 및 Refresh List 호출 
    /// </summary>
    private void RefreshArtifactScroll(bool refreshList, bool selectFirstItem, float? scrollNormalizedPos = null)
    {
        if (refreshList)
        {
            List<ScrollArtifactManufactureItemListModel> displayList = new List<ScrollArtifactManufactureItemListModel>();

            // TableData임 수정 하면 저얼대 안댐
            var upgradeList = DBArtifact.GetUpgradeList();

            /*
             * 그룹별로 순회하며 내가 보유중이면 보유중인애를 띄어주고 
             * 보유중이 아니라면 가장 낮은 Step 의 Artifact 를 띄어준다 .
             */

            if (upgradeList != null)
            {
                foreach (var byGroup in upgradeList)
                {
                    bool foundMine = false;

                    // Step 즉 레벨별로 돌음 . 
                    // 내거를 찾으면 display list 에 해당 아티팩트를 추가하며 즉시 순회 탈출 
                    foreach (var byStep in byGroup.Value)
                    {
                        if (Me.CurCharData.IsMyArtifact(byStep.Value.ArtifactID))
                        {
                            foundMine = true;

                            displayList.Add(new ScrollArtifactManufactureItemListModel()
                            {
                                // 해당 artifact 가 내가 보유중인애이므로 더하고 탈출 로직 
                                artifactID = byStep.Value.ArtifactID,
                                isObtained = true
                            });

                            break;
                        }
                    }

                    // 해당 아티팩트 그룹이 내것이 아니라면 가장 하위 데이터로 땡겨옴. 
                    // 테이블에서 이미 step 정렬이 돼있다 가정함. 
                    if (foundMine == false)
                    {
                        displayList.Add(new ScrollArtifactManufactureItemListModel()
                        {
                            artifactID = byGroup.Value.Values.Min(t => t.ArtifactID),
                            isObtained = false
                        });
                    }
                }
            }

            uint selectedID = selectedArtifactIDOnScroll;

            if (selectFirstItem)
            {
                if (displayList.Count > 0)
                {
                    selectedID = displayList.First().artifactID;
                    SetSelectedArtifactIDOnScroll(selectedID);
                }
                else
                {
                    SetSelectedArtifactIDOnScroll(0);
                }
            }

            /// artifactID 기준 오름차순 정렬 
            displayList.Sort((t01, t02) =>
            {
                return t01.artifactID.CompareTo(t02.artifactID);
            });

            ScrollAdapter.SelectArtifactID(selectedID);
            ScrollAdapter.RefreshData(displayList);
        }
        else
        {
            if (selectFirstItem)
            {
                SetSelectedArtifactIDOnScroll(ScrollAdapter.SelectFirstArtifactID());
            }

            ScrollAdapter.RefreshData();
        }

        if (scrollNormalizedPos.HasValue)
            ScrollAdapter.SetNormalizedPosition(scrollNormalizedPos.Value);
    }

    /// <summary>
    /// 가운데 탭에 Look 탭을 누르면 뜨는 UI 업데이트 
    /// </summary>
    private void UpdateUI_Look()
    {
        var data = DBArtifact.GetArtifactByID(selectedArtifactIDOnScroll);

        if (data != null)
        {
            uiGroup_look.imgItem.sprite = ZManagerUIPreset.Instance.GetSprite(data.Icon);
            uiGroup_look.effects.ForEach(t => t.obj.SetActive(t.grade == data.Grade));
        }

        // WARNING : 여기 로직은 정상적인 경우에는 타면안된다 . 
        // 여기 타면 선택된 슬롯이 없는건데 데이터가 존재할시 일단 그런상황발생하면안댐 
        else
        {
            uiGroup_look.imgItem.sprite = null;
            uiGroup_look.effects.ForEach(t => t.obj.SetActive(false));
        }
    }

    /// <summary>
    /// 가운데 Stat 탭을 누르면 뜨는 아티팩트 아이템 스탯 정보들 출력해주는
    /// UI 들 업데이트 
    /// </summary>
    private void UpdateUI_Stat()
    {
        var data = DBArtifact.GetArtifactByID(curShownArtifactIDOnStat);

        if (data != null)
        {
            uiGroup_stat.txtArtifactItemName.text = DBLocale.GetText(data.ArtifactName);
            uiGroup_stat.txtArtifactItemName.color = UIFrameArtifact.GetColorByGrade(data.Grade);
        }
        else
        {
            uiGroup_stat.txtArtifactItemName.text = string.Empty;
        }

        UpdateUIStat_AbilityActionText(data);
        UpdateUIStat_LevelAdjustButtons();
    }

    /// <summary>
    /// 가운데 Stat 탭일때의 AbilityAction text 세팅 
    /// </summary>
    void UpdateUIStat_AbilityActionText(Artifact_Table data)
    {
        var abilityHelper = ArtifactFrame.AbilityActionBuildHelper;

        if (data != null)
        {
            var mainAbilityActionIDs = new List<uint>();
            var petCombiAbilityActionIDs = new List<uint>();
            bool isObtained = Me.CurCharData.IsArtifactObtained(data.ArtifactID);
            Color titleTxtColor = isObtained ? uiGroup_stat.statTitleActiveColor : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Color valueTxtColor = isObtained ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);

            for (int i = 0; i < data.AbilityActionID.Count; i++)
            {
                mainAbilityActionIDs.Add(data.AbilityActionID[i]);
            }

            for (int i = 0; i < data.CheckPetAbilityActionID.Count; i++)
            {
                petCombiAbilityActionIDs.Add(data.CheckPetAbilityActionID[i]);
            }

            uiGroup_stat.txtGradeTitle.color = titleTxtColor;
            uiGroup_stat.txtGrade.text = DBLocale.GetText(DBUIResouce.GetTierText(data.Grade));
            uiGroup_stat.txtGrade.color = isObtained ? UIFrameArtifact.GetColorByGrade(data.Grade) : titleTxtColor;

            curSelectedArtifactMainAbilityActionValues.Clear();
            curSelectedArtifactPetCombiAbilityActionValues.Clear();

            // 메인 abilityAction 텍스트 데이터 세팅  
            abilityHelper.BuildAbilityActionTexts(
                ref curSelectedArtifactMainAbilityActionValues
                , mainAbilityActionIDs);

            // MainAbilityAction 의 처음 데이터는 별도로 맨 위 UI 요소에 세팅함.
            if (curSelectedArtifactMainAbilityActionValues.Count > 0)
            {
                var t = curSelectedArtifactMainAbilityActionValues[0];
                var abilityData = DBAbility.GetAbility(t.type);
                var firstAbilityIconSprite = abilityData != null ? ZManagerUIPreset.Instance.GetSprite(abilityData.AbilityIcon) : null;
                uiGroup_stat.imgFirstAbilityIcon.sprite = firstAbilityIconSprite;
                uiGroup_stat.txtFirstAbilityTitle.text = t.title;
                uiGroup_stat.txtFirstAbilityTitle.color = titleTxtColor;
                uiGroup_stat.txtFirstAbilityValue.text = t.strValue;
                uiGroup_stat.txtFirstAbilityValue.color = valueTxtColor;
            }

            // 실제 SourceObj 를 생성하고 거기에 Text Pair Data 를 세팅함 . 
            ArtifactFrame.AbilityActionBuildHelper.SetAbilityActionUITexts(
                false
                , 1
                , mainAbilityActionSourceObj
                , curSelectedArtifactMainAbilityActionValues
                , mainAbilityActionTextParent
                , ref uiGroup_stat.mainAbilityActionTextsCached);

            foreach (var txt in uiGroup_stat.mainAbilityActionTextsCached)
            {
                if (txt != null
                    && txt.gameObject.activeSelf)
                {
                    txt.txtTitle.color = titleTxtColor;
                    txt.txtValue.color = valueTxtColor;
                }
            }

            /// 펫 장착 추가 효과 존재 여부 
            bool petBuffExist = data.CheckPetID.Count > 0;

            /// 현재 기획 컨펌 사항으로는 펫 추가효과가 2 개이상 들어갈 일이 없음 . 
            /// 배열이기는 하지만 두개 이상이 안들어간다 해서 하나만 있다 가정하고 구현함 
            if (data.CheckPetID.Count > 1)
            {
                ZLog.LogError(ZLogChannel.UI, " Pet Check ID count cannot be larger then 1 ");
                return;
            }

            uiGroup_stat.petCombiBuffObj.SetActive(petBuffExist);

            /// 착용 Pet 에 대한 추가 버프 존재 여부 
            if (petBuffExist)
            {
                var targetPet = DBPet.GetPetData(data.CheckPetID[0]);
                string petName = targetPet == null ? string.Empty : DBLocale.GetText(targetPet.PetTextID);
                uiGroup_stat.txtPetCombiBuffTitle.text = string.Format(DBLocale.GetText("Artifact_6Tier_Advanced_Enhance"), petName);

                /// 위와 같은 방식의 abilityAction 세팅 
                abilityHelper.BuildAbilityActionTexts(
                    ref curSelectedArtifactPetCombiAbilityActionValues
                    , petCombiAbilityActionIDs);

                ArtifactFrame.AbilityActionBuildHelper.SetAbilityActionUITexts(
                    false
                    , 0
                    , petCombiAbilityActionSourceObj
                    , curSelectedArtifactPetCombiAbilityActionValues
                    , petCombiAbilityActionTextParent
                    , ref uiGroup_stat.petCombiAbilityActionTextsCached);
            }
            else
            {
                uiGroup_stat.petCombiAbilityActionTextsCached.ForEach(t => t.gameObject.SetActive(false));
            }
        }
        else
        {
            uiGroup_stat.imgFirstAbilityIcon.gameObject.SetActive(false);
            uiGroup_stat.txtFirstAbilityTitle.gameObject.SetActive(false);
            uiGroup_stat.txtFirstAbilityValue.gameObject.SetActive(false);
            uiGroup_stat.mainAbilityActionTextsCached.ForEach(t => t.gameObject.SetActive(false));
            uiGroup_stat.petCombiAbilityActionTextsCached.ForEach(t => t.gameObject.SetActive(false));
        }

        uiGroup_stat.scrollRect.verticalNormalizedPosition = 1;
    }

    private void UpdateUIStat_LevelAdjustButtons()
    {
        bool isObtained = Me.CurCharData.IsArtifactObtained(curShownArtifactIDOnStat);
        uint myTargetArtifactStep = Me.CurCharData.GetArtifactStep(selectedArtifactGroupID);
        uint shownArtifactStep = DBArtifact.GetArtifactStep(curShownArtifactIDOnStat);

        // 최소 레벨이 아니면서 , 해당 아티팩트의 Step 을 만약 내가 소유중이라면
        // 내 Step 보다 높아야함 . 

        uiGroup_stat.btnAdjustStepPrev.interactable =
            DBArtifact.IsArtifactPrevStepExist(curShownArtifactIDOnStat) &&
            shownArtifactStep > myTargetArtifactStep;
        uiGroup_stat.btnAdjustStepNext.interactable = DBArtifact.IsArtifactNextStepExist(curShownArtifactIDOnStat);
    }

    private void UpdateUI_MaterialAndETC()
    {
        var selectedArtifactData = DBArtifact.GetArtifactByID(selectedArtifactIDOnScroll);
        var targetManufactureArtifactData = DBArtifact.GetArtifactByID(targetOperationArtifactIDOnScroll);

        if (targetManufactureArtifactData != null)
        {
            uiGroup_material.txtArtifactName.text = DBLocale.GetText(targetManufactureArtifactData.ArtifactName);
            uiGroup_material.txtArtifactName.color = UIFrameArtifact.GetColorByGrade(targetManufactureArtifactData.Grade);

            #region Cost Item Setting

            uiGroup_material.costItems.ForEach(t => t.gameObject.SetActive(false));

            for (int i = 0; i < targetManufactureArtifactData.CostItemID.Count; i++)
            {
                var costID = targetManufactureArtifactData.CostItemID[i];
                var costCnt = targetManufactureArtifactData.CostItemCount[i];

                if (DBItem.GetItem(targetManufactureArtifactData.CostItemID[i], out var itemData))
                {
                    bool enoughCurrency = ZNet.Data.Me.GetCurrency(costID) >= costCnt;
                    var ui = uiGroup_material.costItems.Find(t => t.gameObject.activeSelf == false);
                    if (ui == null)
                    {
                        ui = Instantiate(costSourceObj, uiGroup_material.costObjParent);
                        uiGroup_material.costItems.Add(ui);
                    }

                    ui.Set(
                        ZManagerUIPreset.Instance.GetSprite(itemData.IconID)
                        , ZNet.Data.Me.GetCurrency(costID)
                        , costCnt
                        , enoughCurrency ? uiGroup_material.costTxtColor_enough : uiGroup_material.costTxtColor_notEnough);
                    ui.gameObject.SetActive(true);
                }
            }

            #endregion

            #region Success Rate + Protection Item Setting 

            var successRatePercent = targetManufactureArtifactData.SuccessRate / 10000;
            uiGroup_material.txtSuccessRate.text = string.Format("{0}%", successRatePercent.ToString());

            var protectionID = targetManufactureArtifactData.ProtectItemID;
            var requiredProtectionCnt = targetManufactureArtifactData.ProtectItemCount;
            var myProtectionCnt = ZNet.Data.Me.GetCurrency(protectionID);

            if (protectionID != 0 &&
                DBItem.GetItem(protectionID, out var itemData_))
            {
                bool canUseProtection = myProtectionCnt >= requiredProtectionCnt;
                Sprite protectionItemSprite = ZManagerUIPreset.Instance.GetSprite(itemData_.IconID);
                uiGroup_material.imgRequiredManufactureProtectorItem.sprite = protectionItemSprite;
                uiGroup_material.imgRequiredManufactureProtectorItem.gameObject.SetActive(true);
                string requiredProtectionCntColorKey = myProtectionCnt >= requiredProtectionCnt ? "FFFFFF" : ArtifactFrame.ConvertColorToHex(uiGroup_material.costTxtColor_notEnough);
                uiGroup_material.txtRequiredManufactureProtectorItemCnt.text = string.Format("<color=#{0}>{1}</color> / {2}", requiredProtectionCntColorKey, myProtectionCnt, requiredProtectionCnt);
                uiGroup_material.toggleUseProtector.interactable = canUseProtection;

                //uiGroup_material.txtRequiredManufactureProtectorItemCnt.color = canUseProtection ? Color.white : uiGroup_material.inactivatedTextColor;

                /// 보호제 개수가 부족하면 아예 토글 꺼버림 
                if (canUseProtection == false)
                    uiGroup_material.toggleUseProtector.isOn = false;
            }
            else
            {
                uiGroup_material.imgRequiredManufactureProtectorItem.gameObject.SetActive(false);
                uiGroup_material.txtRequiredManufactureProtectorItemCnt.text = string.Format("<color=#{0}>{1} / 0</color>", ArtifactFrame.ConvertColorToHex(uiGroup_material.inactivatedTextColor), myProtectionCnt);
                uiGroup_material.toggleUseProtector.isOn = false;
                uiGroup_material.toggleUseProtector.interactable = false;
            }

            #endregion

            #region Material 

            uiGroup_material.activeOnMaxStep.ForEach(t => t.SetActive(isSelectedArtifactLastStep));
            #endregion

            #region Buttons 

            //bool isNextLevelExist = DBArtifact.IsArtifactNextStepExist(selectedArtifactData.ArtifactID);
            //uiGroup_material.btnManufacture.interactable = isNextLevelExist;

            bool isMaterialFull = uiGroup_material.materialHandler.IsEmptySlotExist() == false;
            bool isSlotEmpty = uiGroup_material.materialHandler.DataExistCount == 0;
            bool isNextLevelExist = DBArtifact.IsArtifactNextStepExist(selectedArtifactIDOnScroll);

            uiGroup_material.btnManufacture.interactable = isMaterialFull && isNextLevelExist;
            uiGroup_material.btnAutoSetting.interactable = isMaterialFull == false && isNextLevelExist;
            uiGroup_material.btnReset.interactable = isSlotEmpty == false;
            bool isObtained = Me.CurCharData.IsArtifactObtained(selectedArtifactData.ArtifactID);
            uiGroup_material.btnEquipOrUnequip.interactable = isObtained;

            string strEquip = DBLocale.GetText("Equip_Text");
            string strUnequip = DBLocale.GetText("Lift_Text");

            /// 눈에 보이는 선택된 슬롯 소유중 
            if (isObtained)
            {
                bool isEquipped = Me.CurCharData.IsArtifactEquippedByType(selectedArtifactData.ArtifactType);
                bool isAlreadyEquipped = false;
                string equipBtnText = string.Empty;

                if (isEquipped)
                {
                    isAlreadyEquipped = selectedArtifactData.ArtifactID == Me.CurCharData.GetMyArtifactTidByType(selectedArtifactData.ArtifactType);
                }

                /// 슬롯에 장착된 애랑 내가 장착하려는 애가 같다 
                if (isAlreadyEquipped)
                {
                    /// 해제 
                    equipBtnText = strUnequip;
                }
                /// 해당 슬롯이 비었다 
                else if (isEquipped == false)
                {
                    /// 장착
                    equipBtnText = strEquip;
                }
                /// 선택된 애랑 슬롯에 장착중인애랑 다르다 
                else
                {
                    equipBtnText = strEquip;
                }

                uiGroup_material.txtEquipOrUnequip.text = equipBtnText;
                uiGroup_material.txtManufacture.text = DBLocale.GetText("ArtifactUpgrade");
            }
            /// 소유중이 아니라면 텍스트는 무조건 장착으로 띄움  
            else
            {
                uiGroup_material.txtEquipOrUnequip.text = strEquip;
                uiGroup_material.txtManufacture.text = DBLocale.GetText("ArtifactMake");
            }

            #endregion
        }

        // 현재 내부에 세팅되있는 걸로 ScrollUpdate 함 . 
        uiGroup_material.materialHandler.RefreshScroll();
    }

    private void OnMaterialRegisterDone()
    {
        UpdateUI_MaterialAndETC();
    }

    //private void UpdateData_MaterialCostInfo()
    //{
    //    var data = DBArtifact.GetArtifactByID(selectedArtifactIDOnScroll);

    //    if (data == null)
    //        return; 

    //}

    /// <summary>
    /// 아티팩트 아이템을 클릭하였을때 해당 슬롯의 아티팩트에 대한 정보를 저장함 . 
    /// 여러상황에 대해서 데이터를 동시에 동기화해줘야하는 상황이 많음 . 조심.
    /// </summary>
    private void SetSelectedArtifactIDOnScroll(uint id)
    {
        selectedArtifactIDOnScroll = id;
        /// shortcut 용 데이터 넣어줌 
        UIFrameArtifact._ManufactureRuntimeShortData.targetArtifactTid = id;
        var data = DBArtifact.GetArtifactByID(id);
        selectedArtifactIDStep = data != null ? data.Step : (byte)0;
        selectedArtifactGroupID = DBArtifact.GetArtifactGroupIDByArtifactID(id);
        selectedArtifactDisplayableMinStep = data.Step;
        curShownArtifactIDOnStat = id;
        isSelectedArtifactLastStep = DBArtifact.IsArtifactNextStepExist(id) == false;

        if (Me.CurCharData.IsArtifactObtained(id) && isSelectedArtifactLastStep == false)
        {
            targetOperationArtifactIDOnScroll = id + 1;
            isSelectedArtifactUpgradeable = true;
        }
        else
        {
            targetOperationArtifactIDOnScroll = id;
            isSelectedArtifactUpgradeable = false;
        }
    }

    /// <summary>
    /// 좌측 메인 탭들에 있는 스크롤 슬롯 클릭 핸들링 함수 
    /// </summary>
    private void OnMainScrollSlotClicked(ScrollArtifactManufactureItemListModel obj)
    {
        var prevSelectedSlotID = selectedArtifactIDOnScroll;
        //    var prevTargetOperationSlotID = targetOperationArtifactIDOnScroll;

        SetSelectedArtifactIDOnScroll(obj.artifactID);
        ScrollAdapter.SelectArtifactID(obj.artifactID);
        RefreshArtifactScroll(false, false);

        // 슬롯이 바뀌었기에 재료 클리어함 . );

        SetMiddleLookTab(curSelectedMiddleTab);

        // 슬롯이 바뀌었기// 료 클리어함 . 
        if (prevSelectedSlotID != obj.artifactID)
        {
            // ArtifactFrame.GetArtifactPopup_SelectMaterial().OnSelectedSlotClicked(targetOperationArtifactIDOnScroll);
            uiGroup_material.materialHandler.ResetAll(isSelectedArtifactLastStep ? 0 : targetOperationArtifactIDOnScroll);
        }

        UpdateUI_MaterialAndETC();
    }

    private bool CheckCurrencyEnough(List<uint> costItem, List<uint> cost)
    {
        if (costItem.Count != cost.Count)
        {
            ZLog.LogError(ZLogChannel.UI, "These two have to match");
            return false;
        }

        for (int i = 0; i < costItem.Count; i++)
        {
            if (ConditionHelper.CheckCompareCost(costItem[i], cost[i]) == false)
                return false;
        }

        return true;
    }

    //private void OnMaterialSlotClicked(MaterialDataOnSlot data)
    //{

    //}

    #endregion

    #region Inspector Events 
    public void OnMiddleTabChanged(Toggle toggle)
    {
        if (toggle.isOn == false)
            return;

        var selected = manufactureMiddleTabs.Find(t => t.toggle == toggle);

        if (selected != null)
            SetMiddleLookTab(selected.type);
    }

    #region OnClick
    public void OnClick_StatPrevLevelBtn()
    {
        if (blockClickEvents)
            return;

        // 그냥 아이디 -1 시킴 . 이게 가능한 상황이기에 버튼이 활성화돼엇다
        // 가정함 . 
        curShownArtifactIDOnStat -= 1;
        UpdateUI_Stat();
    }

    public void OnClick_StatNextLevelBtn()
    {
        if (blockClickEvents)
            return;

        curShownArtifactIDOnStat += 1;
        UpdateUI_Stat();
    }

    public void OnClickResetBtn()
    {
        if (blockClickEvents)
            return;

        /// 이미 텅비었으면 굳이 업데이트안함 . 
        if (uiGroup_material.materialHandler.DataExistCount == 0)
            return;

        uiGroup_material.materialHandler.ResetAll(isSelectedArtifactLastStep ? 0 : targetOperationArtifactIDOnScroll);
        RefreshArtifactScroll(false, false);
        UpdateUI_MaterialAndETC();
    }

    public void OnClick_AutoSetting()
    {
        if (blockClickEvents)
            return;

        if (uiGroup_material.materialHandler.FillMaterialsAuto())
        {
            RefreshArtifactScroll(false, false);
            UpdateUI_MaterialAndETC();
        }
        else
        {
            ArtifactFrame.OpenNotiUp(DBLocale.GetText("Artifact_Material_Empty_Notice"), "알림");
        }
    }

    public void OnClick_Upgrade()
    {
        if (blockClickEvents)
            return;

        /// 현재 선택된 아티팩트가 보유중/보유중이 아님에 따라서 
        /// 아이디 세팅 해줌 
        var curSelectedArtifactOnScroll = DBArtifact.GetArtifactByID(selectedArtifactIDOnScroll);
        var targetOperationArtifact = DBArtifact.GetArtifactByID(targetOperationArtifactIDOnScroll);

        ///// 소유중이라면 지금 보이는 것의 다음것을 지정해준다 
        //if (Me.CurCharData.IsArtifactObtained(selectedArtifactIDOnScroll))
        //{
        //    targetOperationArtifact = DBArtifact.GetNextArtifact(curSelectedArtifactOnScroll.ArtifactGroupID, curSelectedArtifactOnScroll.Step);
        //}

        // TODO : locale 
        if (targetOperationArtifact == null)
        {
            ArtifactFrame.OpenNotiUp("데이터가 존재하지 않습니다.", "알림");
            return;
        }
        else if (CheckCurrencyEnough(targetOperationArtifact.CostItemID, targetOperationArtifact.CostItemCount) == false)
        {
            return;
        }
        else if (uiGroup_material.materialHandler.IsAllSlotSet == false)
        {
            ArtifactFrame.OpenNotiUp(DBLocale.GetText("NOT_ENOUGH_ARTIFACT_MAKE_MATERIAL_COUNT"), "알림");
            return;
        }
        else if (Me.CurCharData.IsThisArtifactEquipped(curSelectedArtifactOnScroll.ArtifactID))
        {
            ArtifactFrame.OpenNotiUp("장착 중인 아티팩트는 승급할 수 없습니다.", "알림");
            return;
        }

        bool isObtained = Me.CurCharData.IsArtifactObtained(selectedArtifactIDOnScroll);

        ArtifactFrame.OpenTwoButtonQueryPopUp("확인", isObtained ? DBLocale.GetText("Artifact_Upgrade_Confirm") : DBLocale.GetText("Artifact_Make_Confirm")
            , onConfirmed: () =>
            {
                List<DBArtifact.MaterialItem> matsNotInOrder = new List<DBArtifact.MaterialItem>(targetOperationArtifact.MaterialCount);
                bool error = false;

                uiGroup_material.materialHandler.ForeachData((mat) =>
                {
                    ulong itemId = 0;
                    uint itemTid = 0;
                    PetData petData = null;

                    // Pet,Vehicle 따라서 다르게 가져와야한다고한다 
                    if (mat.matType == E_ArtifactMaterialType.Pet)
                    {
                        petData = Me.CurCharData.GetPetData(mat.tid);
                    }
                    else if (mat.matType == E_ArtifactMaterialType.Vehicle)
                    {
                        petData = Me.CurCharData.GetRideData(mat.tid);
                    }

                    if (petData == null)
                    {
                        ZLog.LogError(ZLogChannel.UI, " could not get data of registered mat ");
                        return;
                    }

                    itemId = petData.PetId;
                    itemTid = petData.PetTid;

                    var targetMat = matsNotInOrder.Find(matAlreadyExist => matAlreadyExist.tid == itemTid);

                    if (targetMat == null)
                    {
                        targetMat = new DBArtifact.MaterialItem() { id = itemId, tid = itemTid, cnt = 1 };
                        matsNotInOrder.Add(targetMat);
                    }
                    else
                    {
                        targetMat.cnt++;
                    }
                });

                var upgradeOrderList = DBArtifact.BuildMaterialList(targetOperationArtifact.ArtifactID, false);
                var matsOrdered = new List<DBArtifact.MaterialItem>(matsNotInOrder.Count);

                /// 서버에서 테이블에서의 재료순서가 맞아야한단다 . 왜그렇게 체크하는지 이해가안가지만 일단 처리함.
                /// 테이블의 재료 등급 순서대로 맞추어져 있는 리스트를 순회함 
                foreach (var orderProp in upgradeOrderList)
                {
                    /// 해당 등급의 재료를 전부 가져옴 
                    var targetGradeMats = matsNotInOrder.FindAll(t =>
                    {
                        var data = DBPet.GetPetData(t.tid);
                        return data.Grade == orderProp.grade;
                    });

                    matsOrdered.AddRange(targetGradeMats);
                }

                if (error)
                    return;

                bool protectItemExist = uiGroup_material.toggleUseProtector.isOn;

                blockClickEvents = true;

                try
                {
                    ZWebManager.Instance.WebGame.REQ_MakeArtifactItem(
                        targetOperationArtifact.ArtifactID
                        , targetOperationArtifact.CostItemID.ToArray()
                        , protectItemExist ? targetOperationArtifact.ProtectItemID : 0
                        , matsOrdered
                        , (revPacket, resList) =>
                        {
                            blockClickEvents = false;

                            string resultComment = string.Empty;
                            uint newSelectedArtifactID = 0;

                            if (resList.ArtifactResult)
                            {
                                resultComment = isObtained ? DBLocale.GetText("Artifact_Upgrade_Success") : DBLocale.GetText("Artifact_Make_Success");

                                /// 선택된 스크롤 업데이트 . 강화 성공한 녀석을 계속 클릭하게끔 설정 
                                newSelectedArtifactID = targetOperationArtifactIDOnScroll;
                            }
                            else
                            {
                                /// 가지고 있는 애였다면 승급 시도한것 
                                if (isObtained)
                                {
                                    if (protectItemExist)
                                    {
                                        resultComment = DBLocale.GetText("Artifact_Upgrade_Protected");
                                    }
                                    else
                                    {
                                        resultComment = DBLocale.GetText("Artifact_Upgrade_Fail");
                                    }
                                }
                                /// 아니라면은 제작을 시도한것 
                                else
                                {
                                    resultComment = DBLocale.GetText("Artifact_Make_Fail");
                                }

                                /// 기존 아티팩트가 그대로 있다 ? 그러면 그냥 그대로 넣어줌 
                                if (Me.CurCharData.IsArtifactObtained(selectedArtifactIDOnScroll))
                                {
                                    newSelectedArtifactID = selectedArtifactIDOnScroll;
                                }
                                else
                                {
                                    /// 파괴된 아티팩트이기 때문에 처음 Step 거로 세팅. 
                                    newSelectedArtifactID = DBArtifact.GetFirstStepArtifact(selectedArtifactIDOnScroll).ArtifactID;
                                }
                            }

                            /// 새로운 슬롯을 클릭하게끔 아이디 세팅 
                            SetSelectedArtifactIDOnScroll(newSelectedArtifactID);
                            // ArtifactFrame.OpenNotiUp(popupComment, "알림");
                            UICommon.SetNoticeMessage(resultComment, Color.white, 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);
                            RefreshCurrentUI(false, true);
                        },
                        (err, req, res) =>
                        {
                            blockClickEvents = false;

                            RefreshCurrentUI(false, true);
                            ArtifactFrame.HandleError(err, req, res);
                        });
                }
                catch (Exception exp)
                {
                    blockClickEvents = false;
                }
            }
            , onCanceled: () =>
            {
                blockClickEvents = false;
            });

        //else if (DBArtifact.IsArtifactNextStepExist(curSelectedArtifactOnScroll.ArtifactID) == false)
        //{
        //    ArtifactFrame.OpenNotiUp("더 이상 강화가 불가능합니다.", "알림");
        //    return;
        //}
    }

    public void OnClick_EquipOrRemoveBtn()
    {
        var selectedData = DBArtifact.GetArtifactByID(selectedArtifactIDOnScroll);

        if (selectedData == null)
            return;

        E_PetType type = selectedData.ArtifactType;
        bool isEquipped = Me.CurCharData.IsArtifactEquippedByType(type);
        bool isSelectedAlreadyEquipped = Me.CurCharData.IsArtifactEquippedByTID(selectedData.ArtifactID);

        string askingTxt = string.Empty;
        // isEquipped ? DBLocale.GetText("Lift_Text") : DBLocale.GetText("Equip_Text");
        EquipActionCase curAction = EquipActionCase.None;
        Artifact_Table switchArtfact_curEquipped = null;
        string confirmText = string.Empty;

        /// 해당 아티팩트의 타입은 이미 장착된 상황 
        if (isEquipped)
        {
            confirmText = DBLocale.GetText("Lift_Text");

            /// 이미 장착돼 있는 아티팩트가 선택한 아티팩트인 상황 
            if (isSelectedAlreadyEquipped)
            {
                curAction = EquipActionCase.Unequip;

                askingTxt = DBLocale.GetText("Artifact_Lift_Message", DBLocale.GetText(selectedData.ArtifactName));
            }
            /// 이미 장착돼 있는 아티팩트와 현재 선택한 아티팩트가 다른 상황 
            else
            {
                curAction = EquipActionCase.Switch;

                uint curEquippedTid = Me.CurCharData.GetMyArtifactTidByType(selectedData.ArtifactType);
                switchArtfact_curEquipped = DBArtifact.GetArtifactByID(curEquippedTid);
                askingTxt = string.Format(DBLocale.GetText("Artifact_Mounting_Message"), DBLocale.GetText(switchArtfact_curEquipped.ArtifactName));
            }
        }
        /// 해당 아티팩트 타입이 비어있으므로 그냥 장착할 수 있는 상황
        else
        {
            curAction = EquipActionCase.Equip;

            askingTxt = string.Format("{0}을 장착하시겠습니까?", DBLocale.GetText(selectedData.ArtifactName));
            confirmText = DBLocale.GetText("Equip_Text");
        }

        ArtifactFrame.OpenTwoButtonQueryPopUp(
            title: "알림"
            , content: askingTxt
            , confirmText: confirmText
            , cancelText: DBLocale.GetText("Cancel_Button")
            , onConfirmed: () =>
            {
                /// 각종 상황에 따라 분기 처리. 
                switch (curAction)
                {
                    /// 장착하기 
                    case EquipActionCase.Equip:
                        {
                            DoEquip(
                                selectedData.ArtifactType
                                , Me.CurCharData.GetMyArtifactIDByTid(selectedData.ArtifactID)
                                , (revPacket, resList) =>
                                {
                                    RefreshCurrentUI(false, false);
                                },
                                (err, req, res) =>
                                {
                                    RefreshCurrentUI(false, false);
                                    ZWebManager.Instance.ProcessErrorPacket(err, req, res, false);
                                });
                        }
                        break;
                    /// 해제했다가 장착하기 
                    case EquipActionCase.Switch:
                        {
                            /// 해제 
                            DoUnEquip(
                                selectedData.ArtifactType
                                , (revPacket, resList) =>
                                {
                                    /// 장착 
                                    DoEquip(
                                        selectedData.ArtifactType
                                        , Me.CurCharData.GetMyArtifactIDByTid(selectedData.ArtifactID)
                                        , (revPacket_, resList_) =>
                                        {
                                            RefreshCurrentUI(false, false);
                                        },
                                        (err_, req_, res_) =>
                                        {
                                            RefreshCurrentUI(false, false);
                                            ZWebManager.Instance.ProcessErrorPacket(err_, req_, res_, false);
                                        });
                                },
                                (err, req, res) =>
                                {
                                    RefreshCurrentUI(false, false);
                                    ZWebManager.Instance.ProcessErrorPacket(err, req, res, false);
                                });
                        }
                        break;
                    /// 해제하기 
                    case EquipActionCase.Unequip:
                        {
                            DoUnEquip(
                                  selectedData.ArtifactType
                                  , (revPacket, resList) =>
                                  {
                                      RefreshCurrentUI(false, false);
                                  },
                                  (err, req, res) =>
                                  {
                                      RefreshCurrentUI(false, false);
                                      ZWebManager.Instance.ProcessErrorPacket(err, req, res, false);
                                  });
                        }
                        break;
                }
            }
            , onCanceled: () =>
            {

            });
    }

    void DoEquip(E_PetType artifactType, ulong artifactID,
        Action<ZWebRecvPacket, ResArtifactEquip> onFinished
        , PacketErrorCBDelegate onError)
    {
        ZWebManager.Instance.WebGame.REQ_ArtifactEquip(
            (uint)artifactType
            , artifactID
            , (revPacket, resList) =>
            {
                onFinished?.Invoke(revPacket, resList);
            },
            (err, req, res) =>
            {
                onError?.Invoke(err, req, res);
            });
    }

    void DoUnEquip(
        E_PetType artifactType
        , Action<ZWebRecvPacket, ReqArtifactUnEquip> onFinished
        , PacketErrorCBDelegate onError)
    {
        ZWebManager.Instance.WebGame.REQ_ArtifactUnEquip(
            (uint)artifactType
            , (revPacket, resList) =>
            {
                onFinished?.Invoke(revPacket, resList);
            },
            (err, req, res) =>
            {
                onError?.Invoke(err, req, res);
            });
    }

    #endregion
    #endregion
}
