using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UISubHUDCharacterState : ZUIFrameBase
{
    /// <summary>클래스변신, 팻, 탈것 UI 클래스</summary>
    [Serializable]
    private class C_PetRideSlot
    {
        public Text PetChangeNameTxt;
        public UIAbilityListAdapter AbilityScroll;
        public UIPetChangeListItem PetChangeSlot;
        public GameObject AddIconObj;
        public GameObject RegistNoticeObj;
        public GameObject ConfirmBtn;

       public void Initialize()
        {
            AbilityScroll.Initialize();
        }

        public void Refresh(bool isOn)
        {
            PetChangeSlot.gameObject.SetActive(isOn);
            AddIconObj.gameObject.SetActive(!isOn);
            PetChangeNameTxt.gameObject.SetActive(isOn);
            RegistNoticeObj.gameObject.SetActive(!isOn);
            AbilityScroll.gameObject.SetActive(isOn);
            ConfirmBtn.SetActive(isOn);
        }
    }

    #region ENUM
    // 임시
    public enum TopUITextType
    {
        HP,
        MP,
        Attack,
        Defenc,
        MagicDefenc
    }

    public enum InfoWindowType
    {
        Equip,
        Status
    }

    public enum EquipPageType
    {
        Equipment,
        Accessory,
        PetRide
    }
    #endregion

    #region UI Variable
    [Header("Top UI"), Space(10)]
    [SerializeField] private Slider HpBar = null;
    [SerializeField] private Slider MpBar = null;
    /// <summary>상단 UI 오브젝트(공격력, 방어력, 마법방어력)</summary>
    [SerializeField] private GameObject StatsArea;
    /// <summary>상단 UI 텍스트 배열(HP, MP, 공격력, 방어력, 마법방어력)</summary>
    [SerializeField] private Text[] TopUITexts = new Text[5]; // 5 상수로 변환할것.
    
    [Header("Info UI"), Space(10)]
    /// <summary>상세정보창</summary>
    [SerializeField] private GameObject StateWindow = null;

    [Header("Equip Tap"), Space(10)]
    /// <summary>장비 탭 페이지 배열(장비, 악세서리, 변신탈것)</summary>
    [SerializeField] private GameObject[] EquipPages = new GameObject[3]; // 3 상수로 변환할것.
    /// <summary>장비 탭 페이지 토글 배열(장비, 악세서리, 변신탈것)</summary>
    [SerializeField] private ZToggle[] EquipPageToggles = new ZToggle[ZUIConstant.CHARACTER_STATE_EQUIP_PAGE_COUNT];
    /// <summary>장비 탭 페이지 토글 이미지 배열(장비, 악세서리, 변신탈것)</summary>
    [SerializeField] private Image[] EquipPageImg = new Image[ZUIConstant.CHARACTER_STATE_EQUIP_PAGE_COUNT];
    /// <summary>장비 탭 프리셋 버튼 배열</summary>
    [SerializeField] private ZUIButtonRadio[] Preset = new ZUIButtonRadio[ZUIConstant.CHARACTER_PRESET_COUNT];

    [Header("Stat Tap"), Space(10)]
    /// <summary>능력치 탭 UI 클래스 아이콘 이미지</summary>
    [SerializeField] private RawImage StatClassIcon = null;
    /// <summary>능력치 탭 UI 스탯 포인트 텍스트</summary>
    [SerializeField] private Text StatPointTxt = null;
    /// <summary>능력치 탭 UI 스탯 타이틀 배열(str, dex, int, wiz, hp</summary>
    [SerializeField] private Text[] StatPointTitles = new Text[ZUIConstant.CHARACTER_STATUS_COUNT];
    /// <summary>능력치 탭 UI 스탯 수치 배열(str, dex, int, wiz, hp</summary>
	[SerializeField] private Text[] StatPointNum = new Text[ZUIConstant.CHARACTER_STATUS_COUNT];
    /// <summary>능력치 탭 UI 프리뷰(변환) 스탯 수치 배열(str, dex, int, wiz, hp</summary>
    [SerializeField] private Text[] StatPointNumChange = new Text[ZUIConstant.CHARACTER_STATUS_COUNT];
    /// <summary>능력치 탭 UI 스탯 버튼 리스트</summary>
    [SerializeField] private List<ZUIButtonRadio> StatusRadioList = new List<ZUIButtonRadio>();
    /// <summary>능력치 탭 리셋 버튼</summary>
    [SerializeField] private ZButton ResetButton;
    /// <summary>능력치 탭 적용 버튼</summary>
    [SerializeField] private ZButton ApplyButton;
    
    [Header("DetailStats"), Space(10)]
    /// <summary>상세보기 팝업</summary>
    [SerializeField] private GameObject DetailPopup = null;
    [SerializeField] private ZText AttackSpeedText, MoveSpeedText, CastSpeed;
    [SerializeField] private UICharDetailInfoScrollAdapter DetailInfoScrollAdapter;

    [Header("Change-Pet-Ride"), Space(10)]
    [SerializeField] private C_PetRideSlot[] PetRideSlot = null;

    [Header("Buff & Debuff List"), Space(10)]
    [SerializeField] private List<Image> BuffList = new List<Image>();
    [SerializeField] private List<Image> DebuffList = new List<Image>();

    [Header("Artifact Slots"), Space(10)]
    [SerializeField] private UIInvenArtifactEquipSlot ArtifactSlot_Pet;
    [SerializeField] private UIInvenArtifactEquipSlot ArtifactSlot_Vehicle;
    #endregion

    #region System Variable
    //Dictionary<E_AbilityType, float> stats = new Dictionary<E_AbilityType, float>();

    /// <summary> 임시 보너스 스탯 수치 변수 </summary>
    private int RemainStat = 0;
    private int SelectStat = 0;
    private int ChangeStat = 0;

    private int[] StatPoint = new int[ZUIConstant.CHARACTER_STATUS_COUNT];

    private bool ChangeEquipTimeCheck = true;
    private float ChangeEquipDelayTime = 5.0f;

    private UIPopupItemInfo InfoPopup;
    private UIPopupArtifactItemInfo InfoPopup_Artifact;

    private ZPawnMyPc MyEntity = null;
    private E_CharacterStateEquip SelectPageIdx = E_CharacterStateEquip.Equipment;
    #endregion

    #region Another Variable
    [Space(10)]
    [SerializeField] private GameObject[] EquipSlot = new GameObject[ZUIConstant.CHARACTER_STATE_EQUIP_SLOT];
    [SerializeField] private List<UIInvenEquipSlot> EquipList = new List<UIInvenEquipSlot>();
    #endregion

    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
                
        ZPawnManager.Instance.DoAddEventRemoveEntity(OnRemoveEntity);
        ZPawnManager.Instance.DoAddEventEquipRideVehicle(OnEquipVehicle);
        ZPawnManager.Instance.DoAddEventStatPreviewUpdated(OnUpdatePreviewStats);

        ZGameOption.Instance.OnOptionChanged += UpdateOption;
    }

    protected override void OnUnityDestroy()
    {
        base.OnUnityDestroy();

        if (ZPawnManager.hasInstance)
        {
            ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);
            ZPawnManager.Instance.DoRemoveEventRemoveEntity(OnRemoveEntity);
            ZPawnManager.Instance.DoRemoveEquipRideVehicle(OnEquipVehicle);
            ZPawnManager.Instance.DoRemoveEventStatPreviewUpdated(OnUpdatePreviewStats);
        }

        ZGameOption.Instance.OnOptionChanged -= UpdateOption;
    }

	protected override void OnInitialize()
	{
		base.OnInitialize();

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIAbilitySlot), (obj)=>
        {
            for (int i = 0; i < PetRideSlot.Length; i++)
            {
                PetRideSlot[i].Initialize();
            }
            ZPoolManager.Instance.Return(obj);
        });
    }

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

        StateWindow.SetActive(false);
        HideAllStatSelectUI();

        RemoveInfoPopup_Artifact();
    }

    // ljh : onshow hud 속성변할때(뎁스바뀔때) 초기화해줌, onshow와 동일
	protected override void OnRefreshOrder(int _LayerOrder)
	{
		base.OnRefreshOrder(_LayerOrder);

        StateWindow.SetActive(false);
        HideAllStatSelectUI();

        RemoveInfoPopup_Artifact();
    }

    protected override void OnHide()
    {
        base.OnHide();  

        if(UIManager.Instance.Find<UIFramePetChangeSelect>(out var frame))
        {
            frame.Close();
        }

        HideAllStatSelectUI();
    }

    public void Init()
    {
        //InfoWindows[(int)InfoWindowType.Equip].SetActive(true);
        Initialize();
    }

    private void Initialize()
    {
        // 장비 프리셋 설정.
        ChangeSet( Me.CurCharData.SelectEquipSetNo );

        // Buff & Debuff List SetActive
        BuffList.ForEach((img) => img.gameObject.SetActive(false));
        DebuffList.ForEach((img) => img.gameObject.SetActive(false));

        // 스텟 셋팅.
        InitStatPoint();

        // 상세 스탯 설정.
        DetailInfoScrollAdapter.Init();

        // 케릭터 정보창 클래스 아이콘 : 별도의 테이블 정보가 없어서 임시로 처리
        switch (DBCharacter.GetClassTypeByTid(Me.CurCharData.TID))
        {
            case E_CharacterType.Knight: ZResourceManager.Instance.Load<Texture>("img_equip_base_class_gladiator", (stringName, tex) => {StatClassIcon.texture = tex;}); break;
            case E_CharacterType.Assassin: ZResourceManager.Instance.Load<Texture>("img_equip_base_class_assassin", (stringName, tex) => { StatClassIcon.texture = tex; }); break;
            case E_CharacterType.Archer: ZResourceManager.Instance.Load<Texture>("img_equip_base_class_archer", (stringName, tex) => { StatClassIcon.texture = tex; }); break;
            case E_CharacterType.Wizard: ZResourceManager.Instance.Load<Texture>("img_equip_base_class_magician", (stringName, tex) => { StatClassIcon.texture = tex; }); break;
        }

        // 프리셋 토글 셋팅.
        Preset[Me.CurCharData.SelectEquipSetNo - 1].DoRadioButtonToggleOn();

        // OnUnityAwake() 쪽으로 이동해야하지 않나?
        ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);
    }

    private void UpdateOption(ZGameOption.OptionKey key)
    {
        if(key == ZGameOption.OptionKey.Option_ShowDefaultCharacterText)
        {
            StatsArea.SetActive(ZGameOption.Instance.bShowDefaultCharacterText);
        }
    }

    private void HandleCreateMyEntity()
    {
        if (null != MyEntity)
            MyEntity.DoRemoveEventStatUpdated(OnUpdateStat);

        MyEntity = ZPawnManager.Instance.MyEntity;

        MyEntity.DoAddEventStatUpdated(OnUpdateStat);
        MyEntity.DoAddEventStatUpdated(OnUpdateStatPoint);
        MyEntity.DoAddEventHpUpdated(UpdateHP);
        MyEntity.DoAddEventMpUpdated(UpdateMP);
        MyEntity.DoAddOnAblityActionChanged(OnAblityActionChanged);
        MyEntity.DoAddEventStatUpdated(OnUpdateDetailStat);
        MyEntity.DoAddEventWeightUpdated(OnUpdateWeight);

        // 여기서 이거 호출할 필요없을 듯. 함수 제거 생각해보기.
        SetDetailStat();

        // 재사용하는곳 찾아서 함수로 빼기.
        for (int i = 0; i < ZUIConstant.CHARACTER_STATUS_COUNT; i++)
        {
            switch ((E_Stat)i)
            {
                case E_Stat.STR:
                    StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_STR)).ToString();
                    StatPointTitles[i].text = DBLocale.GetText(E_AbilityType.FINAL_STR.ToString());
                    break;
                case E_Stat.DEX:
                    StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_DEX)).ToString();
                    StatPointTitles[i].text = DBLocale.GetText(E_AbilityType.FINAL_DEX.ToString());
                    break;
                case E_Stat.INT:
                    StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_INT)).ToString();
                    StatPointTitles[i].text = DBLocale.GetText(E_AbilityType.FINAL_INT.ToString());
                    break;
                case E_Stat.WIS:
                    StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_WIS)).ToString();
                    StatPointTitles[i].text = DBLocale.GetText(E_AbilityType.FINAL_WIS.ToString());
                    break;
                case E_Stat.VIT:
                    StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_VIT)).ToString();
                    StatPointTitles[i].text = DBLocale.GetText(E_AbilityType.FINAL_VIT.ToString());
                    break;
            }
        }


        RefreshStatInfo();

        StatsArea.SetActive(ZGameOption.Instance.bShowDefaultCharacterText);
    }

    private void OnRemoveEntity(uint entityId)
    {
        if (null == MyEntity || MyEntity.EntityId != entityId)
        {
            return;
        }

        MyEntity.DoRemoveEventStatUpdated(OnUpdateStat);
        MyEntity.DoRemoveEventStatUpdated(OnUpdateStatPoint);
        MyEntity.DoRemoveEventHpUpdated(UpdateHP);
        MyEntity.DoRemoveEventMpUpdated(UpdateMP);
        MyEntity.DoRemoveOnAblityActionChanged(OnAblityActionChanged);
        MyEntity.DoRemoveEventStatUpdated(OnUpdateDetailStat);
        MyEntity.DoRemoveEventWeightUpdated(OnUpdateWeight);
    }

    // 비싼 함수. 수정하라.
    private void RefreshStatInfo()
    {
        if (null == MyEntity)
            return;

        UpdateHP(MyEntity.CurrentHp, MyEntity.MaxHp);
        UpdateMP(MyEntity.CurrentMp, MyEntity.MaxMp);

        SetStatInfo();
        //UpdateAtk();
        //UpdateDef();
        //UpdateMDef();
    }

    
    private void UpdateHP(float cur, float max)
    {
        HpBar.value = Mathf.Clamp01(cur / max);
        TopUITexts[(int)TopUITextType.HP].text = $"{cur:N0}/{max:N0}";

        UpdateWarnMessageHP(cur, max);
    }

    /// <summary>[박윤성] 체력 패널티 관련 메세지 처리 시간 </summary>
    float HealthWarnMessageLastTime = 0;
    /// <summary>[박윤성] 체력이 낮을때 표시할 메세지</summary>
    public void UpdateWarnMessageHP(float _cur, float _max)
    {
        //카오스랑 같은 시간
        if (Time.time - HealthWarnMessageLastTime <= 3f)
            return;

        HealthWarnMessageLastTime = Time.time;

        if((_cur/_max) <= ZGameOption.Instance.AlramHPPer)
        {
            //로케일로 바꿔야됨
            string noticeStr = DBLocale.GetText("Alert_Warning_Low_Hp");
            UICommon.SetNoticeMessage(noticeStr, Color.red, 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
        }
    }

    private void UpdateMP(float cur, float max)
    {
        MpBar.value = Mathf.Clamp01(cur / max);
        TopUITexts[(int)TopUITextType.MP].text = $"{cur:N0}/{max:N0}";
    }

    private void UpdateAtk()
    {
        switch (MyEntity.CharacterType)
        {
            case GameDB.E_CharacterType.Knight:
            case GameDB.E_CharacterType.Assassin:
                TopUITexts[(int)TopUITextType.Attack].text = $"{MyEntity.GetAbility(E_AbilityType.FINAL_MAX_SHORT_ATTACK):N0}";
                break;

            case GameDB.E_CharacterType.Archer:
                TopUITexts[(int)TopUITextType.Attack].text = $"{MyEntity.GetAbility(E_AbilityType.FINAL_MAX_LONG_ATTACK):N0}";
                break;

            case GameDB.E_CharacterType.Wizard:
                TopUITexts[(int)TopUITextType.Attack].text = $"{MyEntity.GetAbility(E_AbilityType.FINAL_MAX_MAGIC_ATTACK):N0}";
                break;
        }
    }

    private void UpdateDef()
    {
        float curDefence = MyEntity.GetAbility(E_AbilityType.FINAL_MELEE_DEFENCE);
        TopUITexts[(int)TopUITextType.Defenc].text = $"{curDefence:N0}";
    }

    private void UpdateMDef()
    {
        float curMagicDefence = MyEntity.GetAbility(E_AbilityType.FINAL_MAGIC_DEFENCE);
        TopUITexts[(int)TopUITextType.MagicDefenc].text = $"{curMagicDefence:N0}";
    }

    private void SetStatInfo()
    {
        switch (MyEntity.CharacterType)
        {
            case GameDB.E_CharacterType.Knight:
            case GameDB.E_CharacterType.Assassin:
                TopUITexts[(int)TopUITextType.Attack].text = $"{MyEntity.GetAbility(E_AbilityType.FINAL_MAX_SHORT_ATTACK):N0}";
                break;

            case GameDB.E_CharacterType.Archer:
                TopUITexts[(int)TopUITextType.Attack].text = $"{MyEntity.GetAbility(E_AbilityType.FINAL_MAX_LONG_ATTACK):N0}";
                break;

            case GameDB.E_CharacterType.Wizard:
                TopUITexts[(int)TopUITextType.Attack].text = $"{MyEntity.GetAbility(E_AbilityType.FINAL_MAX_MAGIC_ATTACK):N0}";
                break;
        }

        float curDefence = MyEntity.GetAbility(E_AbilityType.FINAL_MELEE_DEFENCE);
        TopUITexts[(int)TopUITextType.Defenc].text = $"{curDefence:N0}";

        float curMagicDefence = MyEntity.GetAbility(E_AbilityType.FINAL_MAGIC_DEFENCE);
        TopUITexts[(int)TopUITextType.MagicDefenc].text = $"{curMagicDefence:N0}";
    }

    /// <summary>스탯 갱신</summary>
    /// <param name="stats">변경 스탯's</param>
    private void OnUpdateStat(Dictionary<E_AbilityType, float> stats)
    {
        // 함수 3개로 나누어져 있는데 이럴필요가 있나? 하나로 함치는거 생각해볼것.
        //    foreach (var stat in stats)
        //    {
        //        switch (stat.Key)
        //        {
        //case E_AbilityType.FINAL_MAX_SHORT_ATTACK:
        //case E_AbilityType.FINAL_MAX_LONG_ATTACK:
        //case E_AbilityType.FINAL_MAX_MAGIC_ATTACK:
        //	UpdateAtk();
        //	break;

        //case E_AbilityType.FINAL_MELEE_DEFENCE:
        //            case E_AbilityType.FINAL_PET_MELEE_DEFENCE:
        //                UpdateDef();
        //                break;

        //            case E_AbilityType.FINAL_MAGIC_DEFENCE:
        //            case E_AbilityType.FINAL_PET_MAGIC_DEFENCE:
        //                UpdateMDef();
        //                break;
        //        }
        //    }
        SetStatInfo();

        // 상세정보 어데이트
        stats.Add(E_AbilityType.FINAL_MAX_HP, MyEntity.GetAbility(E_AbilityType.FINAL_MAX_HP));
        stats.Add(E_AbilityType.FINAL_MAX_MP, MyEntity.GetAbility(E_AbilityType.FINAL_MAX_MP));
        OnUpdateDetailStat(stats);
    }


	//=============================================================================== 수정영역.

	#region 버튼 콜백.
	/// <summary>상세 정보창 팝업 활성화 함수</summary>
	/// <param name="_active">활성화 여부</param>
	public void OnActiveInfoPopup(bool _active)
    {
        if (UIManager.Instance.Find<UIFrameHUD>().CurUIType != E_UIStyle.Normal)
            return;

        if (_active != false)
            CloseOtherPopups();
        else
            HideAllStatSelectUI();

        RemoveInfoPopup();
        RemoveInfoPopup_Artifact();
        DetailPopup.SetActive(false);
        StateWindow.SetActive(_active);

        InitStatPoint();

        /// 이윤선 : 슬롯창 다시 오픈했을때 이 로직을 타므로 여기서 아티팩트 업데이트 실행 
        if (StateWindow.activeSelf)
        {
            UpdateArtifactSlots();
            RefreshPetChangeRidePreset(Me.CurCharData.SelectEquipSetNo);
        }

        if (_active == false)
        {
            StatPreview();
        }
    }

    /// <summary>상세보기 탭(장비창, 스탯창) 활성화 버튼콜백</summary>
    /// <param name="_Idx">탭 인덱스</param>
    public void OnActiveInfoWindow(int _Idx)
    {
        RemoveInfoPopup();
        RemoveInfoPopup_Artifact();
        DetailPopup.SetActive(false);

        //InfoWindows[(int)InfoWindowType.Equip].SetActive((E_CharacterState)_Idx == E_CharacterState.Equip);
        //InfoWindows[(int)InfoWindowType.Status].SetActive((E_CharacterState)_Idx == E_CharacterState.Status);
        HideAllStatSelectUI();
    }

    /// <summary>장비창 페이지 버튼(<, >) 버튼콜백</summary>
    public void OnChangeEquipPage(bool _next)
    {
        if (_next)
            switch (SelectPageIdx)
            {
                case E_CharacterStateEquip.Equipment:
                    EquipPageToggles[(int)E_CharacterStateEquip.Accessory].GetComponent<ZToggle>().isOn = true;
                    break;
                case E_CharacterStateEquip.Accessory:
                    EquipPageToggles[(int)E_CharacterStateEquip.PetRide].GetComponent<ZToggle>().isOn = true;
                    break;
                case E_CharacterStateEquip.PetRide:
                    EquipPageToggles[(int)E_CharacterStateEquip.Equipment].GetComponent<ZToggle>().isOn = true;
                    break;
            }
        else
            switch (SelectPageIdx)
            {
                case E_CharacterStateEquip.Equipment:
                    EquipPageToggles[(int)E_CharacterStateEquip.PetRide].GetComponent<ZToggle>().isOn = true;
                    break;
                case E_CharacterStateEquip.Accessory:
                    EquipPageToggles[(int)E_CharacterStateEquip.Equipment].GetComponent<ZToggle>().isOn = true;
                    break;
                case E_CharacterStateEquip.PetRide:
                    EquipPageToggles[(int)E_CharacterStateEquip.Accessory].GetComponent<ZToggle>().isOn = true;
                    break;
            }
    }

    /// <summary>장비창 페이지 활성화 버튼콜백</summary>
    /// <param name="_Idx">페이지 인덱스</param>
    public void OnActiveEquipPage(int _Idx)
    {
        EquipPages[(int)EquipPageType.Equipment].SetActive((E_CharacterStateEquip)_Idx == E_CharacterStateEquip.Equipment);
        EquipPages[(int)EquipPageType.Accessory].SetActive((E_CharacterStateEquip)_Idx == E_CharacterStateEquip.Accessory);
        EquipPages[(int)EquipPageType.PetRide].SetActive((E_CharacterStateEquip)_Idx == E_CharacterStateEquip.PetRide);

        switch (_Idx)
        {
            case (int)EquipPageType.Equipment:
                UpdateEquipSlot();
                /// 장비 페이지로 바뀌었을시 아티팩트 업데이트 .
                UpdateArtifactSlots();
                break;
            case (int)EquipPageType.Accessory:
                UpdateEquipSlot();
                break;
            case (int)EquipPageType.PetRide:
                RefreshPetRide();
                break;
        }

        // 레디오 버튼으로 수정하면 없어질 코드..
        SelectPageIdx = (E_CharacterStateEquip)_Idx;

        for (int i = 0; i < EquipPageImg.Length; i++)
            if (i == _Idx)
                EquipPageImg[i].color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
            else
                EquipPageImg[i].color = new Color(112f / 255f, 116f / 255f, 128f / 255f);

        EquipPageToggles[(int)SelectPageIdx].Select();
        EquipPageToggles[(int)SelectPageIdx].group.NotifyToggleOn(EquipPageToggles[(int)SelectPageIdx]);
        // ......................................
    }

    /// <summary>상세 팝업 활성화 버튼</summary>
    public void OnActiveStatusDetailPopup()
    {
        if (!DetailPopup.activeSelf)
        {
            // 수정 예정.
            bool check = false;
            for (int i = 0; i < StatPoint.Length; i++)
            {
                if (StatPoint[i] != 0)
                {
                    check = true;
                }
            }
            if(check)
                StatPreview();
            DetailPopup.SetActive(true);
        }
        else
        {
            DetailPopup.SetActive(false);
        }
    }
    #endregion

    // 장비 관련 코드 정리 필요.
    #region 장비 관련
    /// <summary>장비 슬롯 갱신</summary>
    public void UpdateEquipSlot()
    {
        RemoveInfoPopup();

        List<ZItem> slotItem = Me.CurCharData.GetEquipSlotList();
        ZItem equipItem = null;

        for (int i = 0; i < ZUIConstant.CHARACTER_STATE_EQUIP_SLOT; i++)
        {
            switch ((E_CharacterStateEquipSlot)i)
            {
                case E_CharacterStateEquipSlot.Helmet:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Helmet == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Armor:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Armor == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Gloves:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Gloves == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Shoes:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Shoes == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.MainWeapon:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Weapon == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Cape:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Cape == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Belt:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Pants == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Necklace:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Necklace == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.SubWeapon:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.SideWeapon == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Artifact:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Artifact == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Earring1:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Earring == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Earring2:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Earring_2 == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Bracelet1:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Bracelet == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Bracelet2:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Bracelet_2 == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Ring1:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Ring == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Ring2:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Ring_2 == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Ring3:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Ring_3 == item.slot_idx);
                    break;
                case E_CharacterStateEquipSlot.Ring4:
                    equipItem = slotItem.Find(item => (int)E_EquipSlotType.Ring_4 == item.slot_idx);
                    break;
            }

            if (equipItem != null)
                EquipList[i].Initialize(equipItem);

            EquipList[i].gameObject.SetActive(equipItem != null);
            equipItem = null;

        }

    }

    /// <summary>장비 프리셋 변경</summary>
    /// <param name="_equipSetIdx">프리셋 인덱스</param>
    public void OnChangeEquipPreset(int _equipSetIdx)
    {
        ChangeEquipDelayTime = 3;
        CancelInvoke();
        Invoke(nameof(WaitForChangeEquipTime), ChangeEquipDelayTime);
        if (ChangeEquipTimeCheck)
        {
            if (Me.CurCharData.SelectEquipSetNo != _equipSetIdx)
                ChangeSet(_equipSetIdx);
        }
        else
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("프리셋 변경을 잠시 후에 다시 시도해주세요."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
                Debug.Log(Me.CurCharData.SelectEquipSetNo);
                for (int i = 0; i < Preset.Length; i++)
                {
                    Preset[i].DoToggleAction(false);
                }
                Preset[Me.CurCharData.SelectEquipSetNo - 1].DoToggleAction(true);
            });
        }
        ChangeEquipTimeCheck = false;
    }

    private void WaitForChangeEquipTime()
    {
        ChangeEquipTimeCheck = true;
    }

    /// <summary>장비 프리셋 설정</summary>
    /// <param name="SetNo">프리셋 인덱스</param>
    private void ChangeSet(int SetNo)
    {
        List<OptionEquipInfo> resultEquipItemList = Me.CurCharData.GetEquipSetItemList(SetNo);
        List<ZItem> resultUnEquipItemList = Me.CurCharData.GetEquipItems();

        if (resultEquipItemList.Count <= 0)
        {
            var currecntEquipValue = Me.CurCharData.GetEquipSetValue(Me.CurCharData.SelectEquipSetNo);

            if (resultEquipItemList.Count <= 0)
            {
                foreach (var equip in resultUnEquipItemList)
                {
                    currecntEquipValue = Me.CurCharData.AddEquipItemCurrentSet(equip.item_id, equip.slot_idx);
                }
            }

            //unequip
            UnEquipSet();

            if (resultUnEquipItemList.Count > 0)
                ZWebManager.Instance.WebGame.REQ_UnEquipItems(resultUnEquipItemList, (x, y) =>
                {

                });


            switch (SetNo)
            {
                case 1:
                    ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET1, currecntEquipValue, (x, y) => { SetPresetOption(SetNo); });
                    break;
                case 2:
                    ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET2, currecntEquipValue, (x, y) => { SetPresetOption(SetNo); });
                    break;
                case 3:
                    ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET3, currecntEquipValue, (x, y) => { SetPresetOption(SetNo); });
                    break;
            }

            return;
        }

        void changeItemSet()
        {
            //equip
            if (resultEquipItemList.Count > 0)
                ZWebManager.Instance.WebGame.REQ_EquipItems(resultEquipItemList, (x, y) => { });

        }

        //unequip
        void UnEquipSet()
        {
            for (int i = 0; i < resultUnEquipItemList.Count; i++)
            {
                for (int j = 0; j < resultEquipItemList.Count; j++)
                {
                    if (resultUnEquipItemList[i].item_id == resultEquipItemList[j].UniqueID && resultUnEquipItemList[i].slot_idx == resultEquipItemList[j].SlotIdx)
                    {
                        resultUnEquipItemList.RemoveAt(i);
                        i--;

                        resultEquipItemList.RemoveAt(j);
                        j--;

                        break;
                    }
                }
            }
        }

        //unequip
        UnEquipSet();

        if (resultUnEquipItemList.Count > 0)
            ZWebManager.Instance.WebGame.REQ_UnEquipItems(resultUnEquipItemList, (x, y) =>
            {
                changeItemSet();
            });
        else
            changeItemSet();

        SetPresetOption(SetNo);
    }
    #endregion

    #region Item Info Popup
    /// <summary>아이템 정보창 UI 활성화 함수</summary>
    public void SetInfoPopup(UIPopupItemInfo _infoPopup)
    {
        RemoveInfoPopup_Artifact();

        if (InfoPopup)
        {
            Destroy(InfoPopup.gameObject);
            InfoPopup = null;
        }

        InfoPopup = _infoPopup;
    }

    /// <summary>아이템 정보창 UI 비활성화 함수</summary>
    public void RemoveInfoPopup()
    {
        if (InfoPopup != null)
        {
            Destroy(InfoPopup.gameObject);
            InfoPopup = null;
        }
    }
	#endregion

	#region Artifact Info Popup
	public void SetInfoPopup_Artifact(UIPopupArtifactItemInfo _infoPopup)
    {
        RemoveInfoPopup();

        if (InfoPopup_Artifact)
        {
            Destroy(InfoPopup_Artifact.gameObject);
            InfoPopup_Artifact = null;
        }

        InfoPopup_Artifact = _infoPopup;
    }

    public void RemoveInfoPopup_Artifact()
    {
        if (InfoPopup_Artifact)
        {
            Destroy(InfoPopup_Artifact.gameObject);
            InfoPopup_Artifact = null;
        }
    }

    /// <summary>
    ///  아티팩트 Set UI 처리 
    /// </summary>
    public void UpdateArtifactSlots()
    {
        uint tid_pet = Me.CurCharData.GetMyArtifactPetTid();
        uint tid_vehicle = Me.CurCharData.GetMyArtifactVehicleTid();

        ArtifactSlot_Pet.SetUI(tid_pet);
        ArtifactSlot_Vehicle.SetUI(tid_vehicle);
    }
    #endregion

    /// <summary>연관된(겹치는) 팝업창들 일과 적으로 닫아줌.</summary>
    private void CloseOtherPopups()
    {
        var friendFrame = UIManager.Instance.Find<UIFrameFriend>();
        if (friendFrame != null && friendFrame.Show)
        {
            friendFrame.Close();
        }

        var buffFrame = UIManager.Instance.Find<UIFrameBuffList>();
        if (buffFrame != null && buffFrame.Show)
        {
            buffFrame.Close();
        }

        var chatFrame = UIManager.Instance.Find<UIFrameChatting>();
        if (chatFrame != null && chatFrame.Show)
        {
            chatFrame.Close();
        }
    }

    private void RefreshPetChangeRidePreset(int SetNo)
    {
        var equipChange = Me.CurCharData.GetEquipChangeSet(SetNo);
        var equipPet = Me.CurCharData.GetEquipPetSet(SetNo);
        var equipRide = Me.CurCharData.GetEquipRideSet(SetNo);

        // 프리셋에 등록은 되있음
        if (equipChange != null)
        {
            var myChange = Me.CurCharData.GetChangeDataByTID((uint)equipChange.UniqueID);

            // 등록된 강림 소유중
            if (myChange != null)
            {
                // 현재 장착중이지 않음
                if (Me.CurCharData.MainChange != myChange.ChangeTid)
                {
                    Me.CurCharData.UpdateMainChange(myChange.ChangeTid, 0);
                }
            }
            else
            {
                Me.CurCharData.UpdateMainChange(0, 0);
            }
        }
        else
            Me.CurCharData.UpdateMainChange(0, 0);

        // 프리셋에 등록은 되있음
        if (equipPet != null)
        {
            var myPet = Me.CurCharData.GetPetData((uint)equipPet.UniqueID);

            // 등록된 강림 소유중
            if (myPet != null)
            {
                // 현재 장착중이지 않음
                if (Me.CurCharData.MainPet != myPet.PetTid)
                {
                    Me.CurCharData.UpdateMainPet(myPet.PetTid, 0);
                }
            }
            else
            {
                Me.CurCharData.UpdateMainPet(0, 0);
            }
        }
        else
            Me.CurCharData.UpdateMainPet(0, 0);

        // 프리셋에 등록은 되있음
        if (equipRide != null)
        {
            var myRide = Me.CurCharData.GetRideData((uint)equipRide.UniqueID);

            // 등록된 강림 소유중
            if (myRide != null)
            {
                // 현재 장착중이지 않음
                if (Me.CurCharData.MainVehicle != myRide.PetTid)
                {
                    Me.CurCharData.UpdateMainVehicle(myRide.PetTid);
                }
            }
            else
            {
                Me.CurCharData.UpdateMainVehicle(0);
            }
        }
        else
            Me.CurCharData.UpdateMainVehicle(0);


        RefreshPetRide();
    }

    private void SetPresetOption(int _setNo)
    {
        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SELECT_SET, _setNo.ToString(), (x, y) => 
        {
            RefreshPetChangeRidePreset(_setNo);
        });
    }

    #region Stat Point
    private void InitStatPoint()
    {
        var stat = Me.CurCharData.GetItem(DBConfig.GetStatPoint_ItemID, NetItemType.TYPE_STACK);
        RemainStat = stat != null ? (int)stat.cnt : 0;
        ChangeStat = 0;
        StatPointTxt.text = RemainStat.ToString();
        for (int i = 0; i < ZUIConstant.CHARACTER_STATUS_COUNT; i++)
        {
            StatPointNumChange[i].text = string.Empty;
            StatPoint[i] = 0;
        }

        UpdateStatButton();
    }

    private void UpdateStatButton()
    {
        if (ChangeStat > 0)
        {
            ResetButton.interactable = true;
            ApplyButton.interactable = true;
        }
        else
        {
            ResetButton.interactable = false;
            ApplyButton.interactable = false;
        }

        if (RemainStat == 0 && ChangeStat == 0)
        {
            for (int i = 0; i < StatusRadioList.Count; i++)
            {
                StatusRadioList[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < StatusRadioList.Count / 2; i++)
            {
                StatusRadioList[i].gameObject.SetActive(true);
            }
        }
    }

    private void HideAllStatSelectUI()
    {
        for (int i = 0; i < StatusRadioList.Count; i++)
            StatusRadioList[i].DoToggleAction(false);
    }

    public void OnStatBoard()
    {
        HideAllStatSelectUI();
    }

    public void OnStatPoint(int _statIndex)
    {
        SelectStat = _statIndex;  
    }

    private void OnUpdateStatPoint(Dictionary<E_AbilityType, float> stats)
    {
        for (int i = 0; i < stats.Count; i++)
        {
            switch ((E_Stat)i)
            {
                case E_Stat.STR: StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_STR)).ToString(); break;
                case E_Stat.DEX: StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_DEX)).ToString(); break;
                case E_Stat.INT: StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_INT)).ToString(); break;
                case E_Stat.WIS: StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_WIS)).ToString(); break;
                case E_Stat.VIT: StatPointNum[i].text = ((int)MyEntity.GetAbility(E_AbilityType.FINAL_VIT)).ToString(); break;
            }
        }
    }



    public void OnSetStatPoint(int _point)
    {
        var statData = Me.CurCharData.GetItem(DBConfig.GetStatPoint_ItemID, NetItemType.TYPE_STACK);
        
        if (statData == null || statData.cnt == 0 || ChangeStat + _point > (int)statData.cnt || (_point < 0 && StatPoint[SelectStat] + _point < 0)/* || (RemainStat + _point < 0)*/)
            return;

        RemainStat -= _point;
        ChangeStat += _point;
        StatPoint[SelectStat] += _point; 
        StatPointTxt.text = RemainStat.ToString();

        if(StatPoint[SelectStat] != 0)
            StatPointNumChange[SelectStat].text = "+ " + StatPoint[SelectStat].ToString();
        else
            StatPointNumChange[SelectStat].text = string.Empty;

        UpdateStatButton();

        StatPreview();
    }

    public void ResetAllStatPoint()
    {
        var hasResetItem = Me.CurCharData.GetItem(DBConfig.StatUp_ResetItem_ID, NetItemType.TYPE_STACK);

        if (hasResetItem != null)
            ZWebManager.Instance.WebGame.REQ_ResetStatPoint(hasResetItem.item_id, hasResetItem.item_tid, (_recvPacket, _onError) =>
            {
                InitStatPoint();
            });
        else
        {
            UICommon.OpenCostConfirmPopup((UIPopupCostConfirm _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, "스탯을 초기화하시겠습니까?", DBConfig.StatPoint_ResetCount.ToString(), "item_gem", new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON }, new Action[]
                {
                        delegate { _popup.Close(); },
                        delegate
                        {
                            if (Me.CurUserData.Cash >= DBConfig.StatPoint_ResetCount)
                            {
                                ZWebManager.Instance.WebGame.REQ_ResetStatPoint(0, DBConfig.Diamond_ID, (recvPacket, _onError) =>
                                {
                                    if(UIManager.Instance.Find(out UIFrameHUD _hud)) _hud.RefreshCurrency(); InitStatPoint(); _popup.Close();

                                    StatPreview();

                                    if ((WebNet.ERROR)recvPacket.ErrCode == WebNet.ERROR.STAT_RESET_POINT_ZERO)
                                    {
                                            UICommon.OpenSystemPopup((UIPopupSystem _popupS) => {
                                            _popupS.Open(ZUIString.WARRING, DBLocale.GetText("No_Initialization_Stats"), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate { _popupS.Close(); } });});
                                    }
                                });
                            }
                            else
							{
                                 UICommon.OpenSystemPopup((UIPopupSystem _popupS) => {
                                _popupS.Open(ZUIString.WARRING, DBLocale.GetText("NOT_ENOUGH_DIAMOND"), new string[] { ZUIString.LOCALE_CANCEL_BUTTON }, new Action[] { delegate { _popupS.Close(); } });
                                });
                            }

                            _popup.Close();
                        }
                });
            });
        }  
    }

    public void ReSetStatPoint()
    {
        InitStatPoint();

        StatPreview();
    }

    public void SetStatPoint()
    {
        Dictionary<uint, uint> dicStat = new Dictionary<uint, uint>();
        for(int i = 0; i < StatPoint.Length; i++)
            if(StatPoint[i] > 0)
            {
                switch((E_Stat)i)
                {
                    case E_Stat.STR: dicStat.Add((uint)E_AbilityType.BASE_STR, (uint)StatPoint[i]); break;
                    case E_Stat.DEX: dicStat.Add((uint)E_AbilityType.BASE_DEX, (uint)StatPoint[i]); break;
                    case E_Stat.INT: dicStat.Add((uint)E_AbilityType.BASE_INT, (uint)StatPoint[i]); break;
                    case E_Stat.WIS: dicStat.Add((uint)E_AbilityType.BASE_WIS, (uint)StatPoint[i]); break;
                    case E_Stat.VIT: dicStat.Add((uint)E_AbilityType.BASE_VIT, (uint)StatPoint[i]); break;
                }
            }

        bool changeStatCheck = false;
        for (int i = 0; i < StatPoint.Length; i++)
        {
            if (StatPoint[i] != 0)
                changeStatCheck = true;
        }

        if (changeStatCheck)
        {
            ZWebManager.Instance.WebGame.REQ_UseStatPoint(dicStat, Me.CurCharData.GetItem(DBConfig.GetStatPoint_ItemID, NetItemType.TYPE_STACK).item_id, (_recvPacket, _onError) =>
            {
                InitStatPoint();

                StatPreview();
            });
        }
        else
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("적용된 스탯포인트이 없습니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
        }
    }
    #endregion

    public void StatPreview()
    {
        Dictionary<E_AbilityType, float> stats = new Dictionary<E_AbilityType, float>();
        stats.Clear();
        stats.Add(E_AbilityType.FINAL_STR, StatPoint[0]);
        stats.Add(E_AbilityType.FINAL_DEX, StatPoint[1]);
        stats.Add(E_AbilityType.FINAL_INT, StatPoint[2]);
        stats.Add(E_AbilityType.FINAL_WIS, StatPoint[3]);
        stats.Add(E_AbilityType.FINAL_VIT, StatPoint[4]);
        ZMmoManager.Instance.Field.REQ_StatPreview(stats);
    }

    #region # Change, Pet, Ride
    /// <summary>탈것 정보 갱신 함수</summary>
    public void RefreshPetRide()
    {
        for (int i = 0; i < PetRideSlot.Length; i++)
        {
            RefreshPetRideSlotData((E_PetChangeViewType)i);
        }
    }

    // 강림(change)와 변경(change)의 단어가 같아 변경을 뜻하는 단어는 swap으로 쓰겠습니다.
    ///<see cref="E_PetChangeViewType"/>
    public void OnClickSwapButton(int i)
    {
        Action onConfirm = null;

        switch ((E_PetChangeViewType)i)
        {
            case E_PetChangeViewType.Change:
                onConfirm = () => RefreshPetChangeRidePreset(Me.CurCharData.SelectEquipSetNo);
                break;
            case E_PetChangeViewType.Pet:
                onConfirm = () => RefreshPetChangeRidePreset(Me.CurCharData.SelectEquipSetNo);
                break;
            case E_PetChangeViewType.Ride:
                break;
            default:
                return;
        }

        UIFramePetChangeSelect selectPopup = UIManager.Instance.Find<UIFramePetChangeSelect>();

        if(selectPopup == null)
        {
            UIManager.Instance.Load<UIFramePetChangeSelect>(nameof(UIFramePetChangeSelect), (loadName, loadFrame) =>
            {
                loadFrame.Init(
                    () => UIManager.Instance.Open<UIFramePetChangeSelect>(
                        delegate { UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType((E_PetChangeViewType)i, onConfirm); }));

                loadFrame.SetOnCloseAction(() => {
                    OnActiveInfoPopup(true);
                });
            });
        }
        else
        {
            if(selectPopup.Show == false)
            {
                UIManager.Instance.Open<UIFramePetChangeSelect>(
                    delegate {
                        var uiFrame = UIManager.Instance.Find<UIFramePetChangeSelect>();
                        uiFrame.SetViewType((E_PetChangeViewType)i, onConfirm);
                        uiFrame.SetOnCloseAction(()=> {
                            OnActiveInfoPopup(true);
                        });
                    }
                );
            }
        }
    }

    private void OnEquipVehicle(uint tid)
    {
        var frame = UIManager.Instance.Find<UIFramePetChangeSelect>();
        if (frame != null && frame.isActiveAndEnabled)
        {
            frame.Refresh();
        }
        //정보갱신
        RefreshPetRideSlotData(E_PetChangeViewType.Ride);

        Me.CurCharData.SetEquipPetChangeRide(tid, OptionEquipType.TYPE_RIDE);
        ZWebManager.Instance.WebGame.REQ_SetCharacterCurrentPreset(null, null);
    }

    private void RefreshPetRideSlotData(E_PetChangeViewType type)
    {
        int index = (int)type;
        string name = string.Empty;
        C_PetChangeData data = null;
        List<UIAbilityData> listAbility = new List<UIAbilityData>();

        switch (type)
        {
            case E_PetChangeViewType.Change:
				{
					if (Me.CurCharData.MainChange != 0)
					{
						data = new C_PetChangeData(DBChange.Get(Me.CurCharData.MainChange));

						Change_Table changeData = data.changeData;

						name = DBLocale.GetText(changeData.ChangeTextID);

						foreach (var abilId in changeData.AbilityActionIDs)
						{
							if (DBAbilityAction.TryGet(abilId, out var abilTable))
								DBAbilityAction.GetAbilityTypeList(abilTable, ref listAbility);
						}
					}
				}
				break;

            case E_PetChangeViewType.Pet:
				{
					if (Me.CurCharData.MainPet != 0)
					{
						data = new C_PetChangeData(DBPet.GetPetData(Me.CurCharData.MainPet));

						Pet_Table petData = data.petData;

						name = DBLocale.GetText(petData.PetTextID);

						if (DBAbilityAction.TryGet(petData.GrowthAbility, out AbilityAction_Table growthAbility))
							DBAbilityAction.GetAbilityTypeList(growthAbility, ref listAbility);
					}
				}
                break;
            case E_PetChangeViewType.Ride:
                if (Me.CurCharData.MainVehicle != 0)
                {
                    data = new C_PetChangeData(DBPet.GetPetData(Me.CurCharData.MainVehicle));

                    Pet_Table petData = data.petData;

                    name = DBLocale.GetText(petData.PetTextID);

                    if (DBAbilityAction.TryGet(petData.GrowthAbility, out AbilityAction_Table growthAbility))
                        DBAbilityAction.GetAbilityTypeList(growthAbility, ref listAbility);
                }
                break;
        }

        bool isOn = (data != null);

        PetRideSlot[index].Refresh(isOn);
        
        if (isOn == false)
            return;

        PetRideSlot[index].PetChangeNameTxt.text = name;
        PetRideSlot[index].PetChangeSlot.SetSlotSimple(data);
        PetRideSlot[index].AbilityScroll.RefreshListData(listAbility);
    }
    #endregion Change, Pet, Ride #

    #region # Buff & Debuff

    public void OnClickBuffList()
    {
        // ljh : 타운, 필드 아닐시 열지않음
        if (UIManager.Instance.Find<UIFrameHUD>().CurUIType != E_UIStyle.Normal)
            return;

        ZLog.Log(ZLogChannel.UI, "버프리스트클릭!");

        UIFrameBuffList bufflist = UIManager.Instance.Find<UIFrameBuffList>();

        if (bufflist == null)
        {
            UIManager.Instance.Load<UIFrameBuffList>(nameof(UIFrameBuffList), (_loadName, _loadFrame) => {
                _loadFrame.Init(()=> UIManager.Instance.Open<UIFrameBuffList>(null));
            });
        }
        else
        {
            if (!bufflist.Show)
            {
                if (StateWindow.activeSelf)
                    StateWindow.SetActive(!StateWindow.activeSelf);

                UIManager.Instance.Open<UIFrameBuffList>();
            }
            else
                UIManager.Instance.Close<UIFrameBuffList>();
        }
    }

    public void OnAblityActionChanged(Dictionary<uint, EntityAbilityAction> dicAblityAction)
    {
        //각 15개까지 출력, 오버시 우선순위로 판단(같을시 인덱스)

        List<EntityAbilityAction> lbuff = new List<EntityAbilityAction>();
        List<EntityAbilityAction> ldebuff = new List<EntityAbilityAction>();

        foreach (var iter in dicAblityAction.Values)
        {
            AbilityAction_Table table = iter.Table;

            //테이블정보 못받아오면 패스
            if (table == null)
                continue;

            //hud 미출력 패스
            if (table.HudBuffSignType == E_HudBuffSignType.Not)
                continue;

            switch (table.BuffType)
            {
                case E_BuffType.Buff:
                    lbuff.Add(iter);
                    break;
                case E_BuffType.DeBuff:
                    ldebuff.Add(iter);
                    break;

                case E_BuffType.None://버프 디버프 제외 패스
                default:
                    break;
            }
        }

        SetBuffHUD(E_BuffType.Buff, lbuff);
        SetBuffHUD(E_BuffType.DeBuff, ldebuff);
    }

    private void SortAbilityActionList(ref List<EntityAbilityAction> lbuff)
    {
        // 정렬기준 : 우선순위 > 등록시간
        lbuff.Sort((a, b) =>
        {
            if (a.Table.BuffNumber != b.Table.BuffNumber)
            {
                if (a.Table.BuffNumber > b.Table.BuffNumber)
                    return -1;
                else
                    return 1;
            }
            else
            {
                if (a.AddedServerTime < b.AddedServerTime)
                    return -1;
                else
                    return 1;
            }
        });
    }

    private void SetBuffHUD(E_BuffType buffType, List<EntityAbilityAction> lBuff)
    {
        SortAbilityActionList(ref lBuff);

        List<Image> targetImgList = null;

        switch (buffType)
        {
            case E_BuffType.Buff:
                targetImgList = BuffList;
                break;
            case E_BuffType.DeBuff:
                targetImgList = DebuffList;
                break;
            case E_BuffType.None:
                return;
        }

        if (targetImgList == null) 
            return;

        for (int i = 0; i < targetImgList.Count; i++)
        {
            bool isOn = i < lBuff.Count;

            targetImgList[i].gameObject.SetActive(isOn);

            if (isOn == false)
                break;

            targetImgList[i].sprite = ZManagerUIPreset.Instance.GetSprite(lBuff[i].Table.BuffIconString);
        }
    }
    #endregion Buff & Debuff #

    #region DetailInfo
    public void SetDetailStat()
    {
        DetailInfoScrollAdapter.SetScrollData(MyEntity);

        UpdateBaseStat(true, true, true);        
    }

    private void OnUpdateDetailStat(Dictionary<E_AbilityType, float> stats)
    {
        DetailInfoScrollAdapter.UpdateDetailStats(stats);

        bool bAttackSpeedDirty = false;
        bool bMoveSpeedDirty = false;
        bool bSkillSpeedDirty = false;

        foreach (var stat in stats)
        {
            float value = MyEntity.GetAbility(stat.Key);

            switch (stat.Key)
            {
                case E_AbilityType.FINAL_PET_ATTACK_SPEED:
                case E_AbilityType.ATTACK_SPEED_PER:
					{
                        bAttackSpeedDirty = true;
                    }
                    break;
                case E_AbilityType.MOVE_SPEED_PER:
                case E_AbilityType.FINAL_PET_MOVE_SPEED:
					{
                        bMoveSpeedDirty = true;
                    }
                    break;
                case E_AbilityType.SKILL_SPEED_PER:
					{
                        bSkillSpeedDirty = true;
                    }
                    break;
                case E_AbilityType.SPEED_PER:
                    bAttackSpeedDirty = true;
                    bMoveSpeedDirty = true;
                    bSkillSpeedDirty = true;                    
                    break;
            }
        }
        UpdateBaseStat(bAttackSpeedDirty, bMoveSpeedDirty, bSkillSpeedDirty);
    }

	private void UpdateBaseStat(bool bAttack, bool bMove, bool bCast)
	{
        if(bAttack)
		{
            float value = MyEntity.GetAbility(E_AbilityType.ATTACK_SPEED_PER) + MyEntity.GetAbility(E_AbilityType.FINAL_PET_ATTACK_SPEED) + MyEntity.GetAbility(E_AbilityType.SPEED_PER);
            AttackSpeedText.text = string.Format("{0}", DBAbility.ParseAbilityValue(DBAbility.GetAbility(E_AbilityType.ATTACK_SPEED_PER).AbilityID, value));
        }
            
        if(bMove)
		{
            float value = MyEntity.GetAbility(E_AbilityType.MOVE_SPEED_PER) + MyEntity.GetAbility(E_AbilityType.FINAL_PET_MOVE_SPEED) + MyEntity.GetAbility(E_AbilityType.SPEED_PER);
            MoveSpeedText.text = string.Format("{0}", DBAbility.ParseAbilityValue(DBAbility.GetAbility(E_AbilityType.MOVE_SPEED_PER).AbilityID, value));
        }
            
        if(bCast)
		{
            float value = MyEntity.GetAbility(E_AbilityType.SKILL_SPEED_PER) + MyEntity.GetAbility(E_AbilityType.SPEED_PER);
            CastSpeed.text = string.Format("{0}", DBAbility.ParseAbilityValue(DBAbility.GetAbility(E_AbilityType.SKILL_SPEED_PER).AbilityID, value));
        }   
    }


	private void OnUpdateWeight()
    {
        if (false == DetailPopup.activeSelf)
            return;

        DetailInfoScrollAdapter.UpdateDetailStats(new Dictionary<E_AbilityType, float>() { { E_AbilityType.FINAL_MAX_WEIGH, MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH) } });
    }

    private void OnUpdatePreviewStats(Dictionary<E_AbilityType, float> _stats)
    {
        DetailInfoScrollAdapter.UpdatePreviewStats(_stats);
    }
    #endregion
}