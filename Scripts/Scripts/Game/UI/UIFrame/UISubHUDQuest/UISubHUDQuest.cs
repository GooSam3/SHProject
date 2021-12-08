using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class UISubHUDQuest : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] private Transform TargetList = null;
    [SerializeField] private Transform QuestList = null;
    [SerializeField] private ZUIButtonRadio radioBtnQuest;
    [SerializeField] private ZUIButtonRadio radioBtnTargetList;
    [SerializeField] private ZButton BtnDeleteTargetList;
    [SerializeField] public List<UISearchTargetSlot> SearchTargetSlotList = new List<UISearchTargetSlot>();
    [SerializeField] List<UISubHUDQuestItem> QuestTarget = new List<UISubHUDQuestItem>();
    #endregion

    private UIFrameQuest mUIFrameQuest = null;
    //-----------------------------------------------------------------------
    protected override void OnInitialize()
    {
        base.OnInitialize();
        mUIFrameQuest = UIManager.Instance.Find<UIFrameQuest>();

        ZPawnManager.Instance.DoAddEventRemoveEntity(HandleRemoveEntity);
        ZPawnManager.Instance.DoAddEventDieEntity(HandleDieEntity);
        ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);

        radioBtnQuest.DoRadioButtonToggleOn();

        SetTargetList();

        for (int i = 0; i < QuestTarget.Count; i++)
		{
            QuestTarget[i].SetMonoActive(false);
		}
        RegistQuestRefreshCallBack();
    }

    public void DoSubHUDQuestAutoPilot(bool _start, uint _questID)
	{
        UISubHUDQuestItem item = FindQuestItemOrMake(_questID, false);
        if (item != null)
		{
            item.DoSubHudQuestItemAutoPilot(_start);
		}
    }

    public void DoSubHUDQuestAutoPilotAllOff()
    {
        for (int i = 0; i < QuestTarget.Count; i++)
        {
            QuestTarget[i].DoSubHudQuestItemAutoPilot(false);
        }
    }

    //---------------------------------------------------------------------------
    private void HandleCreateMyEntity()
    {
        ZPawnManager.Instance.MyEntity.DoAddEventSetTargetList(SetTargetList);
        ZPawnManager.Instance.MyEntity.DoAddEventClickSearchTarget(ClickSearchTargetButton);

        for (int i = 0; i < SearchTargetSlotList.Count; i++)
        {
            SearchTargetSlotList[i].DoAddEvent();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (ZPawnManager.hasInstance)
        {
            ZPawnManager.Instance.DoRemoveEventRemoveEntity(HandleRemoveEntity);
            ZPawnManager.Instance.DoRemoveEventDieEntity(HandleDieEntity);
            ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);

            if (null != ZPawnManager.Instance.MyEntity)
            {
                ZPawnManager.Instance.MyEntity.DoRemoveEventSetTargetList(SetTargetList);
                ZPawnManager.Instance.MyEntity.DoRemoveEventClickSearchTarget(ClickSearchTargetButton);

                for (int i = 0; i < SearchTargetSlotList.Count; i++)
                {
                    SearchTargetSlotList[i].DoRemoveEvent();
                }
            }
        }
    }

    private void HandleDieEntity(uint _entryID, ZPawn target)
    {
        if (ZPawnManager.Instance.MyEntity.TargetSearchList.Contains(target))
        {
            ZPawnManager.Instance.MyEntity.TargetSearchList.Remove(target);
        }

        SetTargetList();
    }
    

    private void HandleRemoveEntity(uint _entryID)
    {
        if (null != ZPawnManager.Instance.MyEntity)
        {
            ZPawn target = ZPawnManager.Instance.MyEntity.TargetSearchList.Find(a => a.EntityId == _entryID);

            if (null != target)
                ZPawnManager.Instance.MyEntity.TargetSearchList.Remove(target);
        }

        SetTargetList();
    }

    public void SetTargetList()
    {
        ResetTargetListAll();

        if (null == ZPawnManager.Instance.MyEntity)
            return;

        ShowTargetListDeleteButton();

        for (int i = 0; i < ZPawnManager.Instance.MyEntity.TargetSearchList.Count; i++)
        {
            int nIndex = i;

            ZPawn target = ZPawnManager.Instance.MyEntity.TargetSearchList[nIndex];

            SearchTargetSlotList[nIndex].gameObject.SetActive(true);
            SearchTargetSlotList[nIndex].Init(target, nIndex);
        }
    }

    public void ClickResetTargetListAll()
    {
        if(ZPawnManager.Instance.MyEntity != null)
		{
            ZPawnManager.Instance.MyEntity.TargetSearchList.Clear();
        }

        ResetTargetListAll();
    }

    private void ResetTargetListAll()
    {
        for (int i = 0; i < SearchTargetSlotList.Count; i++)
        {
            SearchTargetSlotList[i].ResetTarget();
            SearchTargetSlotList[i].gameObject.SetActive(false);
        }

        BtnDeleteTargetList.gameObject.SetActive(false);
    }

    private void ClickSearchTargetButton()
    {
        radioBtnTargetList.DoRadioButtonToggleOn();
    }

    public void ChangeQuestPannel(bool _active)
    {
        QuestList.gameObject.SetActive(_active);
        TargetList.gameObject.SetActive(!_active);
    }

    public void DeleteTargetList()
    {
        ZPawnManager.Instance.MyEntity?.TargetSearchList.Clear();

        SetTargetList();
    }

    private void ShowTargetListDeleteButton()
    {
        if (null != ZPawnManager.Instance.MyEntity)
        {
            bool bIsActive = ZPawnManager.Instance.MyEntity.TargetSearchList != null && ZPawnManager.Instance.MyEntity.TargetSearchList.Count > 0;

            BtnDeleteTargetList.gameObject.SetActive(bIsActive);
        }
    }

    //----------------------------------------------------------------------------
    private UISubHUDQuestItem FindQuestItemOrMake(uint _questTID, bool _canMake)
    {
        UISubHUDQuestItem item = null;
        for (int i = 0; i < QuestTarget.Count; i++)
        {
            if (QuestTarget[i].gameObject.activeSelf == true && QuestTarget[i].GetQuestItemTID() == _questTID)
            {
                item = QuestTarget[i];
                break;
            }
        }

        if (item == null && _canMake)
		{
            for (int i = 0; i < QuestTarget.Count; i++)
            {
                if (QuestTarget[i].gameObject.activeSelf == false)
                {
                    item = QuestTarget[i];
                    break;
                }
            }
        }

        if (item != null)
		{
            item.SetMonoActive(true);
		}

        return item;
    }

    private void RegistQuestRefreshCallBack()
	{
        UIFrameQuest uiQuest = UIManager.Instance.Find<UIFrameQuest>();
        uiQuest.SetUIQuestUpdateCallBack(HandleQuestRefreshMain, UIFrameQuest.E_QuestCategory.Main);
        uiQuest.SetUIQuestUpdateCallBack(HandleQuestRefreshSub, UIFrameQuest.E_QuestCategory.Sub);
        uiQuest.SetUIQuestUpdateCallBack(HandleQuestRefreshTemple, UIFrameQuest.E_QuestCategory.Temple);
    }

    private void RefreshQuest(uint _questTID, ulong _currentCount, bool _complete, bool _delete, UIFrameQuest.E_QuestCategory _questCategory)
	{
        Quest_Table questTable = DBQuest.GetQuestData(_questTID);
        if (questTable == null) return;

        UISubHUDQuestItem item = FindQuestItemOrMake(_questTID, true);

        if (item == null) return; // 최대 허용 갯수 초과

        if (_delete)
		{
            item.DoSubHudQuestItemAutoPilot(false);
            item.SetMonoActive(false);
		}
        else
		{
            item.DoSubHudQuestItemSetting(_questTID, _questCategory, _currentCount, questTable.TargetCount, _complete);
        }

        if (_questCategory == UIFrameQuest.E_QuestCategory.Main) 
		{
            item.gameObject.transform.SetAsFirstSibling();
		}
    }

    private void HandleQuestRefreshMain(uint _questID, ulong _currentCount, bool _complete, bool _delete)
	{
        RefreshQuest(_questID, _currentCount, _complete, _delete, UIFrameQuest.E_QuestCategory.Main);
    }

    private void HandleQuestRefreshSub(uint _questID, ulong _currentCount, bool _complete, bool _delete)
    {
        RefreshQuest(_questID, _currentCount, _complete, _delete, UIFrameQuest.E_QuestCategory.Sub);
    }

    private void HandleQuestRefreshTemple(uint _questID, ulong _currentCount, bool _complete, bool _delete)
	{
        RefreshQuest(_questID, _currentCount, _complete, _delete, UIFrameQuest.E_QuestCategory.Temple);
    }
    //-----------------------------------------------------------------------------   

}
