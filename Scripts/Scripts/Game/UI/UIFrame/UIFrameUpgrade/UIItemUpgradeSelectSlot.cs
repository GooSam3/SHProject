using UnityEngine;
using UnityEngine.UI;

public class UIItemUpgradeSelectSlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image UpgradeItemImage;
    [SerializeField] private Text UpgradeItemClass;
    [SerializeField] private Text UpgradeItemText;
	[SerializeField] private Image GradeBoard;
    [SerializeField] public GameObject SelectObj;
    #endregion

    #region System Variable
    public uint UpgadeListTid;
    #endregion

    public void Set(uint _UpgadeListTid)
    {
        UpgadeListTid = _UpgadeListTid;
        uint ItemTid = DBUpgrade.GetItemTid(_UpgadeListTid);
        var tableData = DBItem.GetItem(ItemTid);
        UpgradeItemClass.text = DBLocale.GetItemUseCharacterTypeName(ItemTid);
        UpgradeItemText.text = DBLocale.GetItemLocale(DBItem.GetItem(ItemTid));
        UpgradeItemImage.sprite =  UICommon.GetItemIconSprite(ItemTid);
        GradeBoard.sprite = UICommon.GetItemGradeSprite(ItemTid);
        SelectObj.SetActive(false);
    }
}
