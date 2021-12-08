using GameDB;
using System;
using System.Collections.Generic;

// 삭제 예정
public class ZUIScrollPetChangeList : CUGUIScrollRectListBase//, CUGUIWidgetSlotItemBase.ISlotItemOwner
{
    public Action<C_PetChangeData> OnClickSlot;

    private UIPetChangeListItem selectedSlot;

    private List<C_PetChangeData> listPetChangeData = new List<C_PetChangeData>();

    private E_PetChangeViewType viewType;

    protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitializePost(_UIFrameParent);
    }

   //protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
   //{
   //    UIPetChangeListItem item = _NewItem as UIPetChangeListItem;
   //    item.SetSlotItemOwner(this);
   //    C_PetChangeData data = listPetChangeData[_Index];
   //    data.ItemInstance = item;
   //    item.SetSlot(data);
   //}
   //
   //public void ISlotItemSelect(CUGUIWidgetSlotItemBase _SelectItem)
   //{
   //    selectedSlot?.SetSelectState(false);// 기존에 있던놈 포커스꺼줌
   //
   //    UIPetChangeListItem item = _SelectItem as UIPetChangeListItem;
   //
   //    selectedSlot = item;
   //    OnClickSlot?.Invoke(selectedSlot.SlotData);
   //}
   //
    //----------------------------------------------------------------------------

    public void Initialize(Action<C_PetChangeData> _onClickSlot)
    {
        OnClickSlot = _onClickSlot;
    }

    /// <summary> 리스트 Add 전 반드시 호출 </summary>
    public void SetListType(E_PetChangeViewType _viewType)
    {
        viewType = _viewType;
    }

    public void DoAddData(List<C_PetChangeData> data)
    {
        listPetChangeData.AddRange(data);
    }

    //----change

    public void DoAddData(Change_Table data)
    {
        listPetChangeData.Add(new C_PetChangeData(data));
    }

    public void DoAddData(List<Change_Table> data)
    {
        foreach (var iter in data)
            DoAddData(iter);
    }

    public void DoAddData(List<ZDefine.ChangeData> data)
    {
        foreach (var iter in data)
        {
            if (DBChange.TryGet(iter.ChangeTid, out var table))
                DoAddData(table);
        }
    }

    public void DoAddData(Dictionary<uint, Change_Table>.ValueCollection data)
    {
        foreach (var iter in data)
        {
            DoAddData(iter);
        }
    }

    public void ADDTEMP()
    {
        listPetChangeData.Add(new C_PetChangeData() { type = E_PetChangeViewType.Ride });
    }

    //----------

    //-------pet

    public void DoAddData(Pet_Table data)
    {
        listPetChangeData.Add(new C_PetChangeData(data));
    }

    public void DoAddData(List<Pet_Table> data)
    {
        foreach (var iter in data)
            DoAddData(iter);
    }

    public void DoAddData(Dictionary<uint, ZDefine.PetData>.ValueCollection data)
    {
        foreach (var iter in data)
        {
            if (DBPet.TryGet(iter.PetTid, out var table))
                DoAddData(table);
        }
    }

    public void DoAddData(Dictionary<uint, Pet_Table>.ValueCollection data)
    {
        foreach (var iter in data)
        {
            DoAddData(iter);
        }
    }

    //----------


    public void DoClearListData()
    {
        listPetChangeData.Clear();
    }

    public void DoRefresh()
    {
        ProtUIScrollListInitialize(listPetChangeData.Count);
    }

}
