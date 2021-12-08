using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using ZDefine;

public class UISkillListScrollAdapter : OSA<BaseParamsWithPrefab, TempUISkillListItem>
{
    public SimpleDataHelper<ScrollSkillData> Data { get; private set; }
    private bool IsOrderSettingMode = false;
    List<SkillInfo> SkillInfoList = new List<SkillInfo>();

    protected override void Start() { }

    protected override TempUISkillListItem CreateViewsHolder(int itemIndex)
    {
        var instance = new TempUISkillListItem();

        instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

        return instance;
    }

    protected override void UpdateViewsHolder(TempUISkillListItem _holder)
    {
        if (_holder == null)
            return;

        ScrollSkillData data = Data[_holder.ItemIndex];
        _holder.UpdateTitleByItemIndex(data, IsOrderSettingMode);
    }

    public void UpdateGainSkill()
	{
        for(int i = 0; i < base.GetItemsCount(); i++)
		{
            if(GetItemViewsHolder(i) != null)
			{
                GetItemViewsHolder(i).UpdateGainSkill();
			}
		}
	}

    protected override void OnItemIndexChangedDueInsertOrRemove(TempUISkillListItem shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
    {
        base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
        shiftedViewsHolder.UpdateTitleByItemIndex(Data[shiftedViewsHolder.ItemIndex], IsOrderSettingMode);
    }

    private void Initialize()
    {
        if (Data == null)
            Data = new SimpleDataHelper<ScrollSkillData>(this);
    }

    public void SetScrollData(E_CharacterType _type, E_WeaponType _weaponType, bool IsOrderSetting = false)
    {
        Initialize();
        StopMovement();

        //UIManager.Instance.Find<UIFrameSkill>().UISkillListItem.Clear();

        IsOrderSettingMode = IsOrderSetting;

        SkillInfoList = ZPawnManager.Instance.MyEntity.SkillSystem.GetSkills();
        List<Skill_Table> data = DBSkill.GetSkillListAll(_type, _weaponType);
        List<Skill_Table> skillTable = new List<Skill_Table>();
        
        if (IsOrderSetting)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].SkillType != E_SkillType.PassiveSkill && data[i].OpenItemID != 0 && ZNet.Data.Me.CurCharData.HasGainSkill(data[i].SkillID))
                    skillTable.Add(data[i]);
            }
            
            skillTable.Sort((a, b) =>
            {
                return SkillInfoList.Find(skill => skill.SkillId == a.SkillID).Order.CompareTo(SkillInfoList.Find(skill => skill.SkillId == b.SkillID).Order);
            });
        }
        else
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].OpenItemID != 0)
                    skillTable.Add(data[i]);
            }

            skillTable = skillTable.OrderBy(a => a.SkillSort).ToList();
        }

        // 다른 캐릭터 타입의 스킬이 리스트에 존재하면 제거
        if (Data.List.Count > 0)
            Data.RemoveItemsFromStart(Data.List.Count);

        for (int i = 0; i < skillTable.Count; i++)
        {
            if (Data.List.Find(item => item.SkillData.SkillID == skillTable[i].SkillID) == null)
            {
                Data.InsertOne(i, new ScrollSkillData() { SkillData = skillTable[i] });
                Data.List[i].DoAddEventOrderChange(OrderChange);
                Data.List[i].DoAddEventSkillSelect(SkillSelect);
            }
        }
    }

    public void RefreshData()
    {
        for (int i = 0; i < base.GetItemsCount(); i++)
            UpdateViewsHolder(base.GetItemViewsHolder(i));
    }

    public void RemoveEvent()
    {
        for(int i = 0; i <Data.List.Count; i++)
        {
            Data.List[i].DoRemoveEventOrderChange(OrderChange);
            Data.List[i].DoRemoveEventSkillSelect(SkillSelect);
        }
    }

    /// <summary> 
    /// 스킬 순서 설정에서 스킬의 순서를 변경하는 버튼을 누르면 호출되는 이벤트
    /// </summary>
    private void OrderChange(ScrollSkillData _skill, bool _isUp)
    {
        int nIndex = Data.List.IndexOf(_skill);
        
        if (_isUp && nIndex <= 0 || !_isUp && nIndex >= Data.List.Count - 1)
            return;
        
        ScrollSkillData changeSkill = _isUp ? Data.List[nIndex - 1] : Data.List[nIndex + 1];

        SkillOrderData skillOrderA = ZNet.Data.Me.CurCharData.GetSkillUseOrderData(_skill.SkillData.SkillID);
        SkillOrderData skillOrderB = ZNet.Data.Me.CurCharData.GetSkillUseOrderData(changeSkill.SkillData.SkillID);

        uint skillORderNumA = SkillInfoList.Find(a => a.SkillId == _skill.SkillData.SkillID).Order;
        uint skillOrderNumB = SkillInfoList.Find(a => a.SkillId == changeSkill.SkillData.SkillID).Order;

        if (skillOrderA == null)
        {
            ZNet.Data.Me.CurCharData.SkillUseOrder.Add(new SkillOrderData(_skill.SkillData.SkillID, skillOrderNumB, 0, 0, true));
        }
        else
        {
            skillOrderA.Order = skillOrderNumB;
            skillOrderA.IsChanged = true;
        }

        if(skillOrderB == null)
        {
            ZNet.Data.Me.CurCharData.SkillUseOrder.Add(new SkillOrderData(changeSkill.SkillData.SkillID, skillORderNumA, 0, 0, true));
        }
        else
        {
            skillOrderB.Order = skillORderNumA;
            skillOrderB.IsChanged = true;
        }

        Data.List.Sort((a, b) =>
        {
            return SkillInfoList.Find(skill => skill.SkillId == a.SkillData.SkillID).Order.CompareTo(SkillInfoList.Find(skill => skill.SkillId == b.SkillData.SkillID).Order);
        });

        RefreshData();
    }

    private void SkillSelect(uint _skillId)
    {
        ScrollSkillData selectedSkill = Data.List.Find(a => a.SkillData.SkillID == _skillId);

        for(int i = 0; i < Data.List.Count; i++)
        {
            Data.List[i].IsSelected = false;
        }

        if(selectedSkill != null)
        {
            selectedSkill.IsSelected = true;
        }
    }
}

public class ScrollSkillData
{
    public Skill_Table SkillData;
    public bool IsSelected;
    private Action<ScrollSkillData, bool> mEventOrderChange;
    private Action<uint> mEventSkillSelect;

    public void SkillSelect()
    {
        mEventSkillSelect?.Invoke(SkillData.SkillID);
    }

    public void OrderChange(bool _isUp)
    {
        mEventOrderChange?.Invoke(this, _isUp);
    }
    
    public void DoAddEventOrderChange(Action<ScrollSkillData, bool> _event)
    {
        mEventOrderChange += _event;
    }

    public void DoRemoveEventOrderChange(Action<ScrollSkillData, bool> _event)
    {
        mEventOrderChange -= _event;
    }

    public void DoAddEventSkillSelect(Action<uint> _event)
    {
        mEventSkillSelect += _event;
    }

    public void DoRemoveEventSkillSelect(Action<uint> _event)
    {
        mEventSkillSelect -= _event;
    }
}