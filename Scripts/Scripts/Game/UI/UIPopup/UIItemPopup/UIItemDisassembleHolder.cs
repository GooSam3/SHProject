using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.UI;

public class UIItemDisassembleHolder : CellViewsHolder
{
    #region UI Variable
    private Image Icon = null;
    private Image GradeBoard = null;
    private Text GradeTxt = null;
    #endregion

    #region System Variable
    private ZDefine.ZItem Item = null;
    private UIFrameItemDisassemble Frame = null;
    #endregion

    public override void CollectViews()
    {
        base.CollectViews();

        // Component 등록
        views.GetComponentAtPath("ItemSlot_Share_Parts/Item_Icon", out Icon);
        views.GetComponentAtPath("ItemSlot_Share_Parts/Grade_Board", out GradeBoard);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Grade/Txt_Grade", out GradeTxt);

        // Frame 등록
        Frame = UIManager.Instance.Find<UIFrameItemDisassemble>();

        // Button Callback 등록
        views.GetComponent<ZButton>().onClick.AddListener(OnItemSelect);
    }

    public void OnItemSelect()
    {
        if (!UIManager.Instance.Find(out UIFrameInventory _inventory))
            return;

        var data = _inventory.ScrollAdapter.Data.List.Find(item => item.Item == null);
        if (data != null)
            data.Reset(new ScrollInvenData() { Item = Item });

        Frame.ScrollAdapter.RemoveData(Item);
        Frame.RefreshText();
        _inventory.ScrollAdapter.SetData();
        _inventory.RefreshInvenVolume();
    }

    public void UpdateHolder(ScrollDisassembleData _holder)
    {
        Icon.gameObject.SetActive(_holder.Item != null);
        GradeBoard.gameObject.SetActive(_holder.Item != null);

        if (_holder.Item == null)
        {
            GradeTxt.text = string.Empty;
            return;
        }

        Item = _holder.Item;
        var table = DBItem.GetItem(Item.item_tid);
        Icon.sprite = UICommon.GetItemIconSprite(Item.item_tid);
        GradeBoard.sprite = UICommon.GetItemGradeSprite(Item.item_tid);
        GradeTxt.text = table.Step > 0 ? "+" + table.Step.ToString() : string.Empty;
    }
}
