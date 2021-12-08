using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIInvenEquipSlot : MonoBehaviour
{
    private const string FORMAT_GRADE_BG = "img_grade_0{0}";

    #region UI Variable
    [SerializeField] private Image Icon = null;
    [SerializeField] private Image GradeBoard = null;
    [SerializeField] private Text GradeTxt = null;
    [SerializeField] private Text NumTxt = null;
    [SerializeField] private GameObject Lock = null;
    [SerializeField] private RectTransform BlockRed = null;
    [SerializeField] private Image BlockRedClassIcon = null;
    #endregion

    #region System Variable
    public ZItem Item = null;
    #endregion

    public void Initialize(ZItem _item)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        Item = _item;

        Icon.gameObject.SetActive(Item.item_tid != 0);
        Icon.sprite = UICommon.GetItemIconSprite(Item.item_tid);

        NumTxt.text = string.Empty;

        var grade = DBItem.GetItem(Item.item_tid).Grade;
        bool isUseGradeColor = grade > 0;
        GradeBoard.gameObject.SetActive(isUseGradeColor);
        if (isUseGradeColor)
            GradeBoard.sprite = ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, grade));
        GradeTxt.text = DBItem.GetItem(Item.item_tid).Step == 0 ? string.Empty : "+" + DBItem.GetItem(Item.item_tid).Step.ToString();

        Lock.SetActive(Item.IsLock);

        BlockRed.gameObject.SetActive(!ZNet.Data.Me.CurCharData.IsCharacterEquipable(Item.item_tid));

        // to do : 별도의 테이블 정보가 없어서 임시로 처리
        switch (DBItem.GetUseCharacterType(Item.item_tid))
        {
            case GameDB.E_CharacterType.Knight: BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_gladiator_01_s"); break;
            case GameDB.E_CharacterType.Assassin: BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_assassin_01_s"); break;
            case GameDB.E_CharacterType.Archer: BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_archer_01_s"); break;
            case GameDB.E_CharacterType.Wizard: BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_magician_01_s"); break;
        }
    }

    public void ShowItemInfo()
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
        {
            UISubHUDCharacterState characterState = UIManager.Instance.Find<UISubHUDCharacterState>();
            UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

            if (obj != null)
            {
                characterState.SetInfoPopup(obj);
                obj.transform.SetParent(characterState.gameObject.transform);
                obj.Initialize(E_ItemPopupType.CharacterStateEquip, Item);
            }
        });
    }
}