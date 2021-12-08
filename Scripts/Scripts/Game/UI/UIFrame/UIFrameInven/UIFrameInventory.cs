using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameInventory : ZUIFrameBase
{
    #region Frame
    public override bool IsBackable => true;
    #endregion

    private const float LASTSORTTIME = 5f;

    public bool IsInitialize { get; private set; }

    #region UI Variable
    /// <summary>인벤토리 상단 Tab.</summary>
    public ZToggle[] TabToggle = new ZToggle[ZUIConstant.INVEN_TOP_TAB_MENU_COUNT];
    /// <summary>인벤토리 하단 아이템 갯수 ex)"6/100".</summary>
    [SerializeField] private Text InvenVolume = null;
    /// <summary>인벤토리 하단 골드.</summary>
    [SerializeField] private Text Coin = null;
    /// <summary>인벤토리 정렬 버튼.</summary>
    [SerializeField] private Button SortBtn = null;
    /// <summary>인벤토리 OSA 스크롤.</summary>
    [SerializeField] public UIInventoryScrollAdapter ScrollAdapter;

    [SerializeField] private Image AnimSkillIcon = null;
    [SerializeField] private Text AnimSkillName = null;
    [SerializeField] private Animation GainSkillAnimation = null;
    #endregion


    #region System Variable
    /// <summary>인벤토리 선택 Slot.</summary>
    [SerializeField] private object SelectObject = null;
    /// <summary>아이템 상세 정보 팝업 창.</summary>
    public UIPopupItemInfo InfoPopup = null;
    /// <summary>현재 인벤의 정렬(상단 Tab 기준) 상태.</summary>
    public E_InvenSortType CurSortType = E_InvenSortType.All;
    /// <summary>자동 정렬 버튼에서 사용하는 Sorting 체크를 위한 변수.</summary>
    private bool isSort = false;
    /// <summary>자동 정렬을 위해서 사용하는 마지막 정렬 시간.</summary>
    private float LastSortTime = 0.0f;
    /// <summary>자동 정렬 버튼 쿨타임(쿨타임 이 안되면 버튼 활성화가 안됨.)</summary>
    private float SortCoolTime = 0.0f;
    private Queue<Skill_Table> SkillAnimationQueue = new Queue<Skill_Table>();
    private Coroutine SkillAnimationCoroutine = null;
    #endregion

    /// <summary>FrameBase 초기화.</summary>
    protected override void OnInitialize()
    {
        base.OnInitialize();

        SortCoolTime = DBConfig.InvenArray_Time;

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIInvenViewsHolder), delegate
        {
            ScrollAdapter.Initialize();

            ZWebManager.Instance.WebGame.GetCharacterOption(new List<uint>() { (uint)WebNet.E_CharacterOptionKey.INVEN_SORT_LIST }, (recvPacket, recvMsgPacket) =>
            {
                if (Me.CurCharData.GetInvenSortList().Count > 0)
                {
                    isSort = true;
                }

                TabToggle[(int)E_InvenSortType.All].SelectToggleAction((ZToggle _toggle) => {
                    ShowInvenSort((int)E_InvenSortType.All);
                });

                SetInvenList();
                
                if (Me.CurCharData.GetNewGainItemList().Count > 0)
                    ShowNewList();

                IsInitialize = true;
            });
        });
    }

	protected override void OnRemove()
	{
		base.OnRemove();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIInvenViewsHolder));
    }

	/// <summary>인벤토리 스크롤 그리드 셋팅.</summary>
	public void SetInvenList()
    {
        ScrollAdapter.SetData(delegate
        {
            RefreshInvenVolume();
            RefreshInfoPopup();
            RefreshCoinText();
        });
    }

    /// <summary>인벤토리에 담기는 아이템 인지 구분을 위한 함수.</summary>
    /// <param name="_item">아이템 정보</param>
    /// <returns>인벤토리에 넣는 아이템이 아닌경우 true.</returns>
    private bool IsExceptItem(ZItem _item)
    {
        return false;
    }

    /// <summary>인벤토리 아이템 갯수 갱신</summary>
    public void RefreshInvenVolume()
    {
        int currentInvenItemCnt = 0;
        for (int i = 0; i < ScrollAdapter.Data.Count; i++)
        {
            if (ScrollAdapter.Data[i].Item != null)
                currentInvenItemCnt++;
        }
        InvenVolume.text = currentInvenItemCnt.ToString() + " / " + Me.CurCharData.InvenMaxCnt.ToString();
    }

    /// <summary>상세정보창 초기화</summary>
    private void RefreshInfoPopup()
    {
        if (InfoPopup != null)
            InfoPopup.Refresh();
    }

    public void ActiveTab()
    {
        TabToggle[(int)E_InvenSortType.All].gameObject.SetActive(CurSortType == E_InvenSortType.All || CurSortType == E_InvenSortType.Equipment || CurSortType == E_InvenSortType.ETC);
        TabToggle[(int)E_InvenSortType.Equipment].gameObject.SetActive(CurSortType == E_InvenSortType.All || CurSortType == E_InvenSortType.Equipment || CurSortType == E_InvenSortType.ETC);
        TabToggle[(int)E_InvenSortType.ETC].gameObject.SetActive(CurSortType == E_InvenSortType.All || CurSortType == E_InvenSortType.Equipment || CurSortType == E_InvenSortType.ETC);
        TabToggle[(int)E_InvenSortType.Disassemble].gameObject.SetActive(CurSortType == E_InvenSortType.Disassemble);
        TabToggle[(int)E_InvenSortType.Enhance].gameObject.SetActive(CurSortType == E_InvenSortType.Enhance);
        TabToggle[(int)E_InvenSortType.Upgrade].gameObject.SetActive(CurSortType == E_InvenSortType.Upgrade);
        TabToggle[(int)E_InvenSortType.EnhanceEquip].gameObject.SetActive(CurSortType == E_InvenSortType.EnhanceEquip);
        TabToggle[(int)E_InvenSortType.EnchantEquip].gameObject.SetActive(CurSortType == E_InvenSortType.EnchantEquip);
    }

    public void RemoveItem(ulong _itemId)
    {
        ScrollAdapter.SetData();
    }

    public void RefreshCoinText()
    {
         Coin.text = ZNet.Data.Me.GetCurrency(DBConfig.Gold_ID).ToString();
    }

    public void SetNewIcon(ulong _itemId, NetItemType _type)
    {
        for (int i = 0; i < ScrollAdapter.Data.List.Count; i++)
        {
            var data = ScrollAdapter.Data.List.Find(item => item.Item != null && item.Item.item_id == _itemId && item.Item.netType == _type);

            if(data != null)
                data.isNew = true;
        }

        ScrollAdapter.RefreshData();
    }

    // 임시조치
    public void ShowInvenSort(int _idx)
    {
        CurSortType = (E_InvenSortType)_idx;

        RemoveInfoPopup();

        ScrollAdapter.SetData();

        RefreshInvenVolume();
        ActiveTab();

        if ((E_InvenSortType)_idx != E_InvenSortType.Disassemble)
            if(ScrollAdapter.Data != null && ScrollAdapter.Data.List.Count > 0)
                ScrollAdapter.ScrollTo(0);
    }

    private void HideNewIcon()
    {
        for (int i = 0; i < ScrollAdapter.Data.List.Count; i++)
        {
            if (ScrollAdapter.Data[i].isNew)
            {
                ScrollAdapter.Data[i].isNew = false;
            }
        }
        ScrollAdapter.RefreshData();
    }

    public void Close()
    {
        UIManager.Instance.Close<UIFrameInventory>();
    }

    public void RemoveAllSelectObject()
    {
        for (int i = 0; i < ScrollAdapter.Data.List.Count; i++)
        {
            if (ScrollAdapter.Data[i].isSelected)
            {
                ScrollAdapter.Data[i].isSelected = false;
                ScrollAdapter.RefreshData();
                return;
            }
        }
    }

    public void SetSelectObject(object _obj)
    {
        if (_obj == null)
            UIManager.Instance.Find<UISubHUDQuickSlot>().SelectQuickSlotEffect(false);
        else
            UIManager.Instance.Find<UISubHUDQuickSlot>().SelectQuickSlotEffect(true);

        SelectObject = _obj;
    }

    public object GetSelectObject()
    {
        return SelectObject;
    }

    public void RemoveInfoPopup()
    {
        if(InfoPopup != null)
        {
            if(InfoPopup.gameObject != null)
                Destroy(InfoPopup.gameObject);

            InfoPopup = null;
        }
    }

    public void SetInfoPopup(UIPopupItemInfo _infoPopup)
    {
        if(InfoPopup != null)
        {
            if (InfoPopup.gameObject != null)
                Destroy(InfoPopup.gameObject);

            InfoPopup = null;
        }

        InfoPopup = _infoPopup;
    }

    private void UpdateSortCheck()
    {
        if (Time.realtimeSinceStartup - LastSortTime < SortCoolTime)
            SortBtn.interactable = false;
        else
            SortBtn.interactable = true;
    }

    public void SortList()
    {
        Me.CurCharData.InvenList.Sort((x, y) => {
            if (DBItem.IsEquipItem(x.item_tid) && !DBItem.IsEquipItem(y.item_tid))
                return -1;
            else if (!DBItem.IsEquipItem(x.item_tid) && DBItem.IsEquipItem(y.item_tid))
                return 1;
            else if (DBItem.IsEquipItem(x.item_tid) && DBItem.IsEquipItem(y.item_tid))
            {
                if (x.slot_idx != 0 && y.slot_idx == 0)
                    return -1;
                else if (x.slot_idx == 0 && y.slot_idx != 0)
                    return 1;
            }

            var xItemData = DBItem.GetItem(x.item_tid);
            var yItemData = DBItem.GetItem(y.item_tid);

            if (xItemData.ItemType < yItemData.ItemType)
                return -1;
            else if (xItemData.ItemType > yItemData.ItemType)
                return 1;

            if (xItemData.UseCharacterType < yItemData.UseCharacterType)
                return -1;
            else if (xItemData.UseCharacterType > yItemData.UseCharacterType)
                return 1;

            if (xItemData.Grade > yItemData.Grade)
                return -1;
            else if (xItemData.Grade < yItemData.Grade)
                return 1;

            if (xItemData.Step > yItemData.Step)
                return -1;
            else if (xItemData.Step < yItemData.Step)
                return 1;

            if (xItemData.ItemTextID[0] < yItemData.ItemTextID[0])
                return -1;
            else if (xItemData.ItemTextID[0] > yItemData.ItemTextID[0])
                return 1;

            if (xItemData.BelongType < yItemData.BelongType)
                return -1;
            else if (xItemData.BelongType > yItemData.BelongType)
                return 1;

            if (x.item_id < y.item_id)
                return -1;
            else if (x.item_id > y.item_id)
                return 1;

            return 0;
        });
    }

    public void SortInvenList()
    {
        LastSortTime = LASTSORTTIME;
        SortBtn.interactable = false;
        Invoke(nameof(UpdateSortCheck), SortCoolTime);

        SortList();

        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.INVEN_SORT_LIST, Me.CurCharData.ChangeInvenSortListValue(ScrollAdapter.Data.List), (x, y) =>
        {
            TabToggle[(int)CurSortType].SelectToggleAction((ZToggle _toggle) => {
                ShowInvenSort((int)CurSortType);
            });
            SetInvenList();
        });
    }

    private void ShowNewList()
	{
        var list = Me.CurCharData.GetNewGainItemList();

        for (int i = 0; i < list.Count; i++)
            SetNewIcon(list[i].item_id, list[i].netType);

        Me.CurCharData.ClearNewGainItemList();
        
        if (UIManager.Instance.Find(out UISubHUDMenu _menu))
            _menu.ActiveNewAlarm(UISubHUDMenu.E_TopMenuButton.Bag, false);
    }

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

        if (!IsInitialize)
            return;

        TabToggle[(int)E_InvenSortType.All].SelectToggleAction((ZToggle _toggle) => {
            ShowInvenSort((int)E_InvenSortType.All);
        });

        if (ScrollAdapter.IsInitialized)
            ScrollAdapter.ScrollTo(0);

        AudioManager.Instance.PlaySFX(30001);

        if (Me.CurCharData.GetNewGainItemList().Count > 0)
            ShowNewList();
    }

	protected override void OnHide()
    {
        base.OnHide();

        if(SkillAnimationCoroutine != null)
		{
            StopCoroutine(nameof(ShowGainSkillAnimation));
            SkillAnimationCoroutine = null;
		}

        if(GainSkillAnimation.gameObject.activeSelf)
		{
            GainSkillAnimation.gameObject.SetActive(false);
		}

        if (ScrollAdapter.IsInitialized)
        {
            HideNewIcon();
            Me.CurCharData.ClearNewGainItemList();
            RemoveAllSelectObject();
        }
        RemoveInfoPopup();
        SetSelectObject(null);

        if (isSort)
            SortInvenList();

        SetSelectObject(null);

        if (UIManager.Instance.Find(out UIFrameItemDisassemble _disassemble)) _disassemble.Close();
        if (UIManager.Instance.Find(out UIFrameItemEnhance _enhance)) _enhance.Close();
        if (UIManager.Instance.Find(out UIFrameItemEnchant _enchant)) _enchant.Close();
        //if (UIManager.Instance.Find(out UIFrameItemUpgrade _upgrade)) _upgrade.Close(); // 업그레이드는 재료가 1:1 대응임(기획 확인)이라고 업그레이 스크립트에서 닫아줌.(닫기 이벤트 분리시켜야함)

        if (UIManager.Instance.Find(out UISubHUDMenu _menu))
            _menu.ActiveNewAlarm(UISubHUDMenu.E_TopMenuButton.Bag, false);
    }

    public void UpdateGainSkill(Skill_Table _skillData)
    {
        SkillAnimationQueue.Enqueue(_skillData);
        
        if (SkillAnimationCoroutine == null)
            SkillAnimationCoroutine = StartCoroutine("ShowGainSkillAnimation");
    }

    IEnumerator ShowGainSkillAnimation()
    {
        while(SkillAnimationQueue.Count > 0)
        {
            Skill_Table skillTable = SkillAnimationQueue.Dequeue();

            AnimSkillIcon.sprite = ZManagerUIPreset.Instance.GetSprite(skillTable.IconID);
            AnimSkillName.text = DBLocale.GetText(skillTable.SkillTextID);

            GainSkillAnimation.gameObject.SetActive(true);
            yield return new WaitUntil(() => !GainSkillAnimation.isPlaying);

            GainSkillAnimation.gameObject.SetActive(false);
        }

        SkillAnimationCoroutine = null;
        yield break;
    }
}
