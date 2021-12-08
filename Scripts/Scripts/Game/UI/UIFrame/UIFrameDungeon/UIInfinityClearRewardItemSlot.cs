using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIInfinityClearRewardItemSlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image Icon;
    [SerializeField] private Text GradeTxt;
    [SerializeField] private Text NumTxt;
    #endregion

    #region System Variable
    private uint ItemTid;
    private uint ItemCnt;
    #endregion

    private UIPopupItemInfo ItemInfo = null;

    public void Initialize(uint _itemTid, uint _itemCnt)
    {
        ItemTid = _itemTid;
        ItemCnt = _itemCnt;

        Icon.gameObject.SetActive(ItemTid != 0);
        Icon.sprite = UICommon.GetItemIconSprite(ItemTid);
        NumTxt.text = ItemCnt.ToString();
    }

    public void ShowItemInfo()
    {
        if(ItemInfo != null)
        {
            return;
        }

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
        {
            ItemInfo = _obj.GetComponent<UIPopupItemInfo>();

            if (ItemInfo != null)
            {
                ItemInfo.transform.SetParent(UIManager.Instance.Find<UIFrameDungeon>().gameObject.transform);

                ZItem item = new ZItem
                {
                    item_tid = ItemTid,
                    cnt = ItemCnt
                };
                ItemInfo.Initialize(E_ItemPopupType.InfinityTower, item);
            }
        });
    }

    public void CloseInfoPopup()
    {
        if (ItemInfo != null)
        {
            Destroy(ItemInfo.gameObject);
            ItemInfo = null;
        }
    }
}