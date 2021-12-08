using GameDB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 정렬, 상점아이템, 장바구니아이템 
// 극혐코드 죄송합니다, 리스트관련 변경될여지가 있는것 같아 분기태웠습니다
public class CheatItemViewListItem : CUGUIWidgetSlotItemBase
{
    //즐겨찾기 쓰는 슬롯에서만 씀
    public enum E_SlotType
    {
        None = 0,
        Item = 1,
        Monster = 2
    }

    [SerializeField] private Text Title;

    // 즐겨찾기는 상점에서밖에 안씀
    [SerializeField] private Image FavoriteIcon;
    private bool bIsFavorite = false;

    //장바구니는 값(갯수)이 있음
    [SerializeField] private Text Value;

    // 정렬슬롯만!!
    [SerializeField] private Image Bg;
    public Color BGColor => Bg.color;

    public Item_Table ShopItemData { get; private set; }
    public C_CheatItemData WishItemData { get; private set; }
    public C_ItemTypeSlot ItemType { get; private set; }

    public C_MonsterTypeSlot MonsterType { get; private set; }
    public Monster_Table MonsterData { get; private set; }

    [Header("Favorite Used Only!!!"), Space(10f)]
    [SerializeField] private E_SlotType slotType = E_SlotType.None;

    protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitialize(_UIFrameParent);
    }

    // 아이템 타입
    public void SetItemType(C_ItemTypeSlot type)
    {
        slotType = E_SlotType.None;
        SetColor(Color.gray);
        ItemType = type;
        Title.text = type.type.ToString();
    }

    // 상점
    public void SetShopSlot(Item_Table item)
    {
        slotType = E_SlotType.Item;
        Title.text = DBLocale.GetItemLocale(item);
        bIsFavorite = C_CheatFavoriteHelper.HasItemValue(item);
        FavoriteIcon.color = bIsFavorite ? Color.yellow : Color.white;

        ShopItemData = item;
    }

    // 장바구니
    public void SetWishSlot(C_CheatItemData item)
    {
        slotType = E_SlotType.None; 
        Title.text = DBLocale.GetItemLocale(item.table);
        Value.text = $"{item.value.ToString()} 개";
        WishItemData = item;
    }

    // 몬스터 타입
    public void SetMonsterType(C_MonsterTypeSlot type)
    {
        slotType = E_SlotType.None;
        SetColor(Color.gray);
        MonsterType = type;
        Title.text = type.type.ToString();
    }

    // 몬스터
    public void SetMonsterSlot(Monster_Table monster)
    {
        slotType = E_SlotType.Monster;
        Title.text = DBLocale.Get(monster.MonsterTextID).Text;
        bIsFavorite = C_CheatFavoriteHelper.HasMonsterValue(monster);
        FavoriteIcon.color = bIsFavorite ? Color.yellow : Color.white;

        MonsterData = monster;
    }

    public void OnClickFavorite()
    {
        bool result = false;

        switch (slotType)
        {
            case E_SlotType.Item:
                if (bIsFavorite)
                    result = C_CheatFavoriteHelper.RemoveFavoriteItem(ShopItemData);
                else
                    result = C_CheatFavoriteHelper.AddFavoriteItem(ShopItemData);

                bIsFavorite = result ? !bIsFavorite : bIsFavorite;

                FavoriteIcon.color = bIsFavorite ? Color.yellow : Color.white;

                //슬롯 공유로 인해 이미지 바뀌는 순서이슈가 있는관계로 이벤트호출 따로..
                C_CheatFavoriteHelper.InvokeItem();
                break;
            case E_SlotType.Monster:
                if (bIsFavorite)
                    result = C_CheatFavoriteHelper.RemoveFavoriteMonster(MonsterData);
                else
                    result = C_CheatFavoriteHelper.AddFavoriteMonster(MonsterData);

                bIsFavorite = result ? !bIsFavorite : bIsFavorite;

                FavoriteIcon.color = bIsFavorite ? Color.yellow : Color.white;

                //슬롯 공유로 인해 이미지 바뀌는 순서이슈가 있는관계로 이벤트호출 따로..
                C_CheatFavoriteHelper.InvokeMonster();
                break;
        }
    }

    public void HandleSlotSelect()
    {
        mSlotItemOwner.ISlotItemSelect(this);
    }

    public void HandleSlotRightClick(BaseEventData eventData)
    {
        if (eventData.currentInputModule.input.GetMouseButtonDown(1) == false)
            return;

        if(mSlotItemOwner is CheatItemViewList owner)
        {
            owner.OnRightClick(this);
        }

    }

    public void SetColor(Color color)
    {
        Bg.color = color;
    }
}