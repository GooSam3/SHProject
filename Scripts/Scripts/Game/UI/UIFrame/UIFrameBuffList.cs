using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

// 버프(디버프) | 패시브 | 마법발동
public class UIFrameBuffList : ZUIFrameBase
{
    public class C_CustomAbilityAction
    {
        public E_BuffListType filterFlag;

        // 사용한 순서
        public ulong sortOrder;

        // 버프 끝나는 시간
        public ulong endTime = 0;

        /// <summary> 버프 시작시 남은 시간. isnotconsume 일 경우 사용 </summary>
        public ulong addedRemainTime = 0;

        public bool IsNotConsume = false;

        public Dictionary<E_AbilityType, float> dicAbiliyActionValue;

        // init 시점의 테이블 정보
        // 그룹은 여러개의 abilityaction이 존재하는 관계로 init시점의 abilityaction을 사용
        // 로케일 및 tid참조시에만 사용
        public AbilityAction_Table uniqueTable;

        public bool isInitialized = false;

        public UIBuffInfoListItem instance;

        public C_CustomAbilityAction(ulong _sortOrder = 0)
        {
            isInitialized = false;
            sortOrder = _sortOrder;
        }

        public void InitCommonData(AbilityAction_Table table)
        {
            uniqueTable = table;
            dicAbiliyActionValue = new Dictionary<E_AbilityType, float>();
            filterFlag = E_BuffListType.None;
            isInitialized = true;

            FilterByBuff();
            FilterByPassive();
            FilterByMagic();
        }

        public void AddAbilityAction(uint abilityActionID)
        {
            if (isInitialized == false)
            {
                if (DBAbilityAction.TryGet(abilityActionID, out AbilityAction_Table abTable) == false)
                    return;

                InitCommonData(abTable);
            }

            List<UIAbilityData> listDataPair = new List<UIAbilityData>();
            DBAbilityAction.GetAbilityTypeList(abilityActionID, ref listDataPair);

            foreach (var iter in listDataPair)
            {
                if (dicAbiliyActionValue.ContainsKey(iter.type) == false)
                    dicAbiliyActionValue.Add(iter.type, 0f);

                dicAbiliyActionValue[iter.type] += iter.value;
            }
        }

        private void FilterByBuff()
        {
            if (uniqueTable.MagicSignType != GameDB.E_MagicSignType.Not)
                return;
            if (uniqueTable.AbilityActionType == GameDB.E_AbilityActionType.Passive)
                return;

            filterFlag |= E_BuffListType.Buff;
        }

        private void FilterByPassive()
        {
            if (uniqueTable.MagicSignType != GameDB.E_MagicSignType.Not)
                return;
            if (uniqueTable.AbilityActionType != GameDB.E_AbilityActionType.Passive)
                return;

            filterFlag |= E_BuffListType.Passive;
        }

        private void FilterByMagic()
        {
            if (uniqueTable.MagicSignType == GameDB.E_MagicSignType.Not)
                return;
            if (uniqueTable.AbilityActionType != GameDB.E_AbilityActionType.Passive)
                return;

            filterFlag |= E_BuffListType.Magic;
        }
    }

    [System.Flags]
    public enum E_BuffListType
    {
        None = 0,
        Buff = 1 << 0,
        Passive = 1 << 1,
        Magic = 1 << 2,
        All = int.MaxValue
    }

    // 버프상세창의 남은시간은 어빌리티 액션이 업데이트가 되던 안되던 자체적으로 처리
    // ** 어빌리티 액션 업데이트시에도 남은시간 갱신해줌
    private const float SEC_UPDATE_BUFF_REMAIN = 1f;

    [SerializeField] private UIToolTipBuffList popupToolTip;

    private List<C_CustomAbilityAction> allAbilityAction = new List<C_CustomAbilityAction>();

    //패시브 데이터 리스트
    private List<C_CustomAbilityAction> allPassiveActionGroup = new List<C_CustomAbilityAction>();

    // 전체 탭 타입 그룹 리스트, 갱신시점은 탭 업데이트시(시간제한 없는관계 및 빈번한 변경 없는관꼐로..)
    private List<C_CustomAbilityAction> allAbilityActionGroup = new List<C_CustomAbilityAction>();

    // 현재 탭 타입 정렬된 리스트
    private List<C_CustomAbilityAction> curAbilityActionList = new List<C_CustomAbilityAction>();

    private E_BuffListType curListType = E_BuffListType.Buff;

    // 초기화시 onHide가 먼저 호출됨에따라 체크
    private bool isAddedEvent = false;

    private int tendency = 0;

    [SerializeField] private ZToggle toggle;

    public void Init(System.Action onEndInit = null)
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIBuffInfoListItem), (objBuffListItem)=>
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIAbilitySlot), (objAbilitySlot)=>
            {
                Initialize();
                onEndInit?.Invoke();

                ZPoolManager.Instance.Return(objBuffListItem);
                ZPoolManager.Instance.Return(objAbilitySlot);
            });
        });
    }
    private void Initialize()
    {
        base.OnInitialize();

        scroller.Initialize(OnClickSlot);
        popupToolTip.Initialize(OnClickToolTipClose);
    }

    protected override void OnRefreshOrder(int _LayerOrder)
    {
        base.OnRefreshOrder(_LayerOrder);

        //[박윤성] 절전모드일때 버프리스트 터치시 해당 레이어를 바꿔주며 나오게 한다.
        if (UIManager.Instance.ScreenSaver != null)
        {
            var targetCanvas = UIManager.Instance.GetCanvas(CManagerUIFrameFocusBase.E_UICanvas.Front);

            this.transform.SetParent(targetCanvas.gameObject.transform, false);
            UIManager.Instance.SetLayer(this.gameObject, LayerMask.NameToLayer("UIFront"));

        }
    }

    // 패시브는 열릴때만 체크
    protected override void OnShow(int _LayerOrder)
    {
        /// TODO : 추후 시스템화 되어야함 . 현재는 우선 강제로 꺼줌 
        var hudSubMenu = UIManager.Instance.Find<UISubHUDCharacterState>();
        if (hudSubMenu != null)
        {
            hudSubMenu.OnActiveInfoPopup(false);
        }

        var friendFrame = UIManager.Instance.Find<UIFrameFriend>();
        if (friendFrame != null)
        {
            friendFrame.Close();
        }

        var chatFrame = UIManager.Instance.Find<UIFrameChatting>();
        if(chatFrame!=null)
		{
            chatFrame.Close();
		}

        ZPawnManager.Instance.MyEntity.DoAddOnAblityActionChanged(OnAblityActionChanged);
        ZPawnManager.Instance.MyEntity.DoAddEventChangeTendency(OnUpdateTendency);
        tendency = ZPawnManager.Instance.MyEntity.Tendency;

        RefreshPassiveData();

        InvokeRepeating(nameof(RefreshBuffRemainTime), SEC_UPDATE_BUFF_REMAIN, SEC_UPDATE_BUFF_REMAIN);

        toggle.SelectToggle(false);
        OnClickBuffTab((int)E_BuffListType.Buff);

        isAddedEvent = true;

        

        base.OnShow(_LayerOrder);
    }

    protected override void OnHide()
    {
        CancelInvoke();

        if (isAddedEvent)
        {
            ZPawnManager.Instance.MyEntity.DoRemoveOnAblityActionChanged(OnAblityActionChanged);
            ZPawnManager.Instance.MyEntity.DoRemoveEventChangeTendency(OnUpdateTendency);
        }
        isAddedEvent = false;

        popupToolTip.SetActiveState(false);

        //[박윤성] 절전모드일때 레이어 교체
        if (UIManager.Instance.ScreenSaver != null)
        {
            var targetCanvas = UIManager.Instance.GetCanvas(CManagerUIFrameFocusBase.E_UICanvas.Back);
            this.transform.SetParent(targetCanvas.gameObject.transform, false);
            UIManager.Instance.SetLayer(this.gameObject, LayerMask.NameToLayer("UI"));
        }

        base.OnHide();
    }
    
    private void OnUpdateTendency(int newValue)
    {
        tendency = newValue;

        RefreshGroupList();
    }



    private void OnAblityActionChanged(Dictionary<uint, EntityAbilityAction> dicEntity)
    {
        allAbilityAction.Clear();

        foreach (var iter in dicEntity.Values)
        {
            if (iter.Table == null)
                continue;//혹시모르니 널체크
            if (iter.Table.BuffType == GameDB.E_BuffType.None)
                continue;

            C_CustomAbilityAction act = new C_CustomAbilityAction(iter.AddedServerTime);

            act.AddAbilityAction(iter.Table.AbilityActionID);
            act.endTime = iter.EndServerTime;
            act.addedRemainTime = iter.AddedRemainTime;
            act.IsNotConsume = iter.IsNotCunsume;
            allAbilityAction.Add(act);
        }

        RefreshList(curListType);
    }

    private void RefreshPassiveData()
    {
        allPassiveActionGroup.Clear();

        List<uint> gainSkills = Me.CurCharData.GetGainSkills();

        bool isChange = Me.CurCharData.CurrentMainChange != 0;

        GameDB.Change_Table changeTable = null;

        if (isChange)
        {
            if (DBChange.TryGet(Me.CurCharData.MainChange, out changeTable) == false)
            {
                ZLog.LogError(ZLogChannel.Stat, "강림중이나, 강림 데이터가 없음!!");
            }
        }

        for (int i = 0; i < gainSkills.Count; i++)
        {
            if (DBSkill.TryGet(gainSkills[i], out Skill_Table table) == false)
            {
                continue;
            }

            if (table.SkillType != E_SkillType.PassiveSkill)
                continue;

            // 현재 강림중이고, 해당 강림체 타입의 패시브가 아니라면 패스
            if (isChange && (EnumHelper.CheckFlag(table.CharacterType, changeTable.UseAttackType)) == false)
                continue;

            // 강림중이 아닐시 타입체크
            if (!isChange && (EnumHelper.CheckFlag(table.CharacterType, DBCharacter.GetClassTypeByTid(Me.CurCharData.TID))) == false)
                continue;

            // 1인이유 : 그룹 다음으로 출력되야함
            C_CustomAbilityAction abilityData = new C_CustomAbilityAction(1);

            abilityData.InitCommonData(DBAbilityAction.Get(table.AbilityActionID_01));

            abilityData.AddAbilityAction(table.AbilityActionID_02);
            abilityData.AddAbilityAction(table.AbilityActionID_03);

            allPassiveActionGroup.Add(abilityData);
        }
    }

    private void RefreshGroupList()
    {
        allAbilityActionGroup.Clear();

        //  그룹

        //강림
        CollectionType type = CollectionType.TYPE_CHANGE;

        var list = ZNet.Data.Me.CurCharData.GetCompleteCollectItems(type);
        if (list != null)
        {
            C_CustomAbilityAction groupChange = new C_CustomAbilityAction();
            foreach (var iter in list)
            {
                if (DBChangeCollect.GetCollection(iter.CollectTid, out ChangeCollection_Table table) == false)
                    continue;

                groupChange.AddAbilityAction(table.AbilityActionID_01);
                groupChange.AddAbilityAction(table.AbilityActionID_02);
            }
            if (groupChange.isInitialized)
                allAbilityActionGroup.Add(groupChange);
        }
        //펫
        type = CollectionType.TYPE_PET;

        list = ZNet.Data.Me.CurCharData.GetCompleteCollectItems(type);
        if (list != null)
        {
            C_CustomAbilityAction groupPet = new C_CustomAbilityAction() { isInitialized = false };

            foreach (var iter in list)
            {
                if (DBPetCollect.GetPetCollection(iter.CollectTid, out PetCollection_Table table) == false)
                    continue;

                groupPet.AddAbilityAction(table.AbilityActionID_01);
                groupPet.AddAbilityAction(table.AbilityActionID_02);
            }
            if (groupPet.isInitialized)
                allAbilityActionGroup.Add(groupPet);
        }

        //탈것
        type = CollectionType.TYPE_RIDE;

        list = ZNet.Data.Me.CurCharData.GetCompleteCollectItems(type);
        if (list != null)
        {
            C_CustomAbilityAction groupRide = new C_CustomAbilityAction() { isInitialized = false };

            foreach (var iter in list)
            {
                if (DBPetCollect.GetRideCollection(iter.CollectTid, out PetCollection_Table table) == false)
                    continue;

                groupRide.AddAbilityAction(table.AbilityActionID_01);
                groupRide.AddAbilityAction(table.AbilityActionID_02);
            }
            if (groupRide.isInitialized)
                allAbilityActionGroup.Add(groupRide);
        }
        type = CollectionType.TYPE_ITEM;


        //아이템
        list = ZNet.Data.Me.CurCharData.GetCompleteCollectItems(type);
        if (list != null)
        {
            C_CustomAbilityAction groupItem = new C_CustomAbilityAction() { isInitialized = false };

            foreach (var iter in ZNet.Data.Me.CurCharData.GetCompleteCollectItems(type))
            {
                if (DBItemCollect.GetItemCollection(iter.CollectTid, out ItemCollection_Table table) == false)
                    continue;

                groupItem.AddAbilityAction(table.AbilityActionID_01);
                groupItem.AddAbilityAction(table.AbilityActionID_02);
            }
            if (groupItem.isInitialized)
                allAbilityActionGroup.Add(groupItem);
        }

        // 성향
        if(DBPKBuff.GetTableByTendencyValue(tendency, out PKBuff_Table tendencyTable))
        {
            if (tendencyTable.CharacterLevel <= Me.CurCharData.Level)
            {
                C_CustomAbilityAction groupTendency = new C_CustomAbilityAction();

                groupTendency.AddAbilityAction(tendencyTable.AbilityActionID_01);
                groupTendency.AddAbilityAction(tendencyTable.AbilityActionID_02);

                allAbilityActionGroup.Add(groupTendency);
            }
        }

        // 문장
        if(DBConfig.Mark_Use_Level <= Me.CurCharData.Level)
        {
            AddMarkAbility();
        }

        // 속성 & 속성연계
        if (DBConfig.Attribute_Require_Level <= Me.CurCharData.Level)
        {
            // 속성
            AddAttributeAbillity();

            // 속성연계
            AddAttributeChainAbility();
        }
        // 아티펙트 링크
        AddArtifactLink();

        RefreshList(curListType, false);
    }
    
    private void AddMarkAbility()
    {
        C_CustomAbilityAction groupMarkAbility = new C_CustomAbilityAction();

        foreach (var iter in Me.CurCharData.GetMarkDataList())
        {
            if (iter.Step <= 0)
                continue;

            List<uint> listAbilityAction = new List<uint>();

            DBMark.AddAbilityActionsStackedAscending(iter.MarkTid, ref listAbilityAction);

            foreach (var abilityAction in listAbilityAction)
                groupMarkAbility.AddAbilityAction(abilityAction);
        }

        allAbilityActionGroup.Add(groupMarkAbility);
    }

    private void AddAttributeAbillity()
    {
        
        E_UnitAttributeType attributeType = E_UnitAttributeType.None;

        //클래스 중이다
        if (Me.CurCharData.CurrentMainChange != 0)
        {
            attributeType = DBChange.Get(Me.CurCharData.MainChange).AttributeType;
        }
        else// 일반상태
        {
            attributeType = DBCharacter.Get(Me.CurCharData.TID).AttributeType;
        }

        if (attributeType == E_UnitAttributeType.None)
            return;

        var attributeLevel = Me.CurCharData.GetAttributeLevelByType(attributeType);
        var attributeTable = DBAttribute.GetAttributeByLevel(attributeType, attributeLevel);


        C_CustomAbilityAction groupAttribute = new C_CustomAbilityAction();

        groupAttribute.AddAbilityAction(attributeTable.AbilityActionID_01);
        groupAttribute.AddAbilityAction(attributeTable.AbilityActionID_02);

        allAbilityActionGroup.Add(groupAttribute);
    }

    private void AddAttributeChainAbility()
    {
        var chainLevel = Me.CurCharData.GetAttributeChainEffectLevel();

        if (chainLevel <= 0)
            return;

        var chainTable = DBAttribute.GetAttributeChainByLevel(chainLevel);


        C_CustomAbilityAction groupChain = new C_CustomAbilityAction();

        groupChain.AddAbilityAction(chainTable.AbilityActionID_01);
        groupChain.AddAbilityAction(chainTable.AbilityActionID_02);

        allAbilityActionGroup.Add(groupChain);
    }

    private void AddArtifactLink()
    {
        /*
         * 아티펙트 링크 : 오픈스펙에서 제외됨
         */
    }

    private void RefreshBuffRemainTime()
    {
        for (int i = 0; i < scroller.VisibleItemsCount; i++)
        {
            scroller.GetItemViewsHolder(i).RefreshTime();
        }
        //osarefresh
    }

    public void OnClickBuffTab(int i)
    {
        curListType = (E_BuffListType)i;

        SetFocusDefault();

        RefreshGroupList();

        RefreshList(curListType, true);
    }

    private void RefreshList(E_BuffListType type, bool setStartPos = false)
    {
        curAbilityActionList.Clear();

        // 그룹 
        foreach (var iter in allAbilityActionGroup)
        {
            if (iter.filterFlag.HasFlag(type))
                curAbilityActionList.Add(iter);
        }

        //그룹, 패시브는 정렬조건없음

        if (type == E_BuffListType.Passive)
            curAbilityActionList.AddRange(allPassiveActionGroup);

        // sortorder(시작시간)기준으로 정렬
        // 그룹과 정렬이 바뀔수있어 따로정렬 후 붙여줌
        List<C_CustomAbilityAction> temp = new List<C_CustomAbilityAction>();

        // 버프
        foreach (var iter in allAbilityAction)
        {
            if (iter.filterFlag.HasFlag(type))
                temp.Add(iter);
        }

        temp.Sort((a, b) =>
        {
            if (a.sortOrder < b.sortOrder)
                return -1;
            else
                return 1;
        });

        curAbilityActionList.AddRange(temp);

        RefreshScroller();

        if (setStartPos)
        {
            scroller.SetNormalizedPosition(0);
        }
    }

    private void OnClickSlot(C_CustomAbilityAction data)
    {
        print("슬롯 클릭됨!");
        scroller.UpdateSelectedID(data.uniqueTable.AbilityActionID);
        scroller.Refresh();

        data.instance.SetFocusState(true);

        popupToolTip.SetToolTipData(data);
        popupToolTip.SetActiveState(true);
    }

    private void OnClickToolTipClose()
    {
        SetFocusDefault();
        scroller.Refresh();
    }

    public void OnClickClose()
    {
        SetFocusDefault();
        UIManager.Instance.Close<UIFrameBuffList>();
    }

    private void SetFocusDefault()
    {
        scroller.UpdateSelectedID(0);
        popupToolTip.SetActiveState(false);
    }

    #region # Sroller

    [SerializeField]
    private UIBuffListScrollerAdapter scroller;

    private void RefreshScroller()
    {
        scroller.RefreshListData(curAbilityActionList);
    }

    #endregion Scroller #
}
