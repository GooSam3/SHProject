using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIMailItemSlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image Icon;
    public GameObject NewIcon;
    [SerializeField] private Text GradeTxt;
    [SerializeField] private Text NumTxt;
    #endregion

    #region System Variable
    [SerializeField] private uint ItemTid;
    [SerializeField] private uint ItemCnt;
    public bool isNew = false;
    #endregion

    public void Initialize(uint _itemTid, uint _itemCnt)
    {
        ItemTid = _itemTid;
        ItemCnt = _itemCnt;

        Icon.gameObject.SetActive(ItemTid != 0);
        Icon.sprite = UICommon.GetItemIconSprite(ItemTid);
        NumTxt.text = ItemCnt.ToString();

        if (DBItem.GetEquipSlots(ItemTid) != null)
            NumTxt.text = string.Empty;

        NewIcon.SetActive(isNew);
    }

    public void ShowItemInfo()
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
        {
            UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

            if (obj != null)
            {
                if (UIManager.Instance.Find(out UIFrameMailbox _mailbox))
                    _mailbox.SetInfoPopup(obj);

                obj.transform.SetParent(UIManager.Instance.Find<UIFrameMailbox>().gameObject.transform);

                ZItem item = new ZItem
                {
                    item_tid = ItemTid,
                    cnt = ItemCnt
                };
                obj.Initialize(E_ItemPopupType.Mailbox, item);
            }
        });
    }
}