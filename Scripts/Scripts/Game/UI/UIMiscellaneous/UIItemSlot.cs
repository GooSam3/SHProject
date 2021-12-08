using System;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

// ljh : 공용으로 사용할 아이템슬롯
// 현재 창고에서쓰임, 기능추가시 추가부탁드립니다
// 안쓰는 기능 관련 오브젝트는 모두 꺼져있습니다.
public class UIItemSlot : MonoBehaviour
{
    [Flags]
    public enum E_Item_PostSetting
    {
        None = 0,
        ShadowOff = 1<<0,
        LockOff = 1<<1,
    }

    private const string FORMAT_GRADE_BG = "img_grade_0{0}";
    private const string FORMAT_ENCHANT_STEP = "+{0}";

    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;

    [SerializeField] private Text itemCount;
    [SerializeField] private Text itemEnchantCount;

    [SerializeField] private GameObject objLockState;
    [SerializeField] private GameObject objShadow;// 가림막? 명칭이 애매함 비활성화상태
    [SerializeField] private GameObject objSelected;
    [SerializeField] private GameObject objEquiped;

    private UIScrollItemSlot scrollSlot;

    private Action onClick;

    public uint item_Tid;

    public ZItem ItemInfo;

    public System.Action<UIItemSlot> OnClickISlot;
    
    public void SetItem(ZItem item)
    {
        if(item == null)
        {
            SetItem(0, 0);
            objEquiped.SetActive(false);
            return;
        }

        ItemInfo = item;
        SetItem(item.item_tid, item.cnt);
        objEquiped.SetActive(item.slot_idx > 0);
    }

    // 아이템별로 고유한 기능 은 꺼줌/ 장착 등 
    public void SetItem(uint tid, ulong count)
    {
        item_Tid = tid;
        itemIcon.gameObject.SetActive(true);
        if (DBItem.GetItem(tid, out GameDB.Item_Table table) == false)
        {
            itemIcon.sprite = null;
            itemIcon.color = Color.magenta;
            return;
        }

        // 아이콘
        itemIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);

        // 등급
        bool isUseGradeColor = table.Grade > 0;
        
        background.gameObject.SetActive(isUseGradeColor);
        if (isUseGradeColor)
            background.sprite = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(table.Grade));

		// 스택아이템 설정
		bool isStackItem = table.ItemStackType == GameDB.E_ItemStackType.Stack ||
						   table.ItemStackType == GameDB.E_ItemStackType.AccountStack;

        if(isStackItem)
		{
            if (table.ItemID == DBConfig.Town_Move_ItemID)
            {
                isStackItem = false;
                count = 0;
			}
        }
                            

        itemCount.gameObject.SetActive(isStackItem || count > 1);
        if (isStackItem)
        {
            string countStr = count.ToString();
            if (count==0)
            {
                countStr = string.Empty;
            }
            itemCount.text = countStr;
        }

        // 강화도 설정
        bool isEnchanted = table.Step > 1;

        itemEnchantCount.gameObject.SetActive(isEnchanted);
        if (isEnchanted)
            itemEnchantCount.text = string.Format(FORMAT_ENCHANT_STEP, table.Step);

        // 그림자
        objShadow.SetActive(count <= 0);

        // 잠금여부
        ZItem item = Me.CurCharData.GetItem(tid);
        bool isLock = false;
        if (item != null)
            isLock = item.IsLock;

        objLockState.SetActive(isLock);
        objSelected.SetActive(false);
        objEquiped.SetActive(false);
    }

    public void SetEmpty()
    {
        item_Tid = 0;
        itemIcon.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
        itemCount.gameObject.SetActive(false);
        itemEnchantCount.gameObject.SetActive(false);
        objLockState.SetActive(false);
        objSelected.SetActive(false);
        objEquiped.SetActive(false);
        objShadow.SetActive(false);
    }

    public void SetEquipState(bool state) => objEquiped.SetActive(state);

    public void SetSelectState(bool state) => objSelected.SetActive(state);

    public void SetShadow(bool state) => objShadow.SetActive(state);

    // 임의의 갯수로 초기화시
    public void SetItemCount(uint cnt, bool state)
    {
        itemCount.gameObject.SetActive(state);
        if (state)
            itemCount.text = cnt.ToString();
    }

    public void SetShadowByCount()
    {

    }

    public void SetShadowByClass()
    {

    }

    public void SetPostSetting(E_Item_PostSetting postSetting)
    {
        if (postSetting.HasFlag(E_Item_PostSetting.ShadowOff))
            objShadow.SetActive(false);

        if (postSetting.HasFlag(E_Item_PostSetting.LockOff))
            objLockState.SetActive(false);
    }

    public void InitializeScrollSlot(UIScrollItemSlot _scrollSlot, Action _onClick)
    {
        scrollSlot = _scrollSlot;
        onClick = _onClick;
    }

    public void SetOnClick(Action _onclick)
    {
        onClick = _onclick;
    }

    public void OnClickSlot()
    {
        onClick?.Invoke();
        OnClickISlot?.Invoke(this);
    }

    public void Refresh()
    {
        if (item_Tid == 0)
            return;

        ZItem matItem = ZNet.Data.Me.CurCharData.GetInvenItemUsingMaterial(item_Tid);
        if(matItem != null)
            SetItem(matItem);
    }
}
