using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameDB;

public class UIMileageShopItemGroup : MonoBehaviour
{
    #region Public Fields
    Action OnClickedListener;
    #endregion

    #region Private Fields
    private List<UIGroupBase> uiGroupIntegrated;
    private bool init;
    #endregion

    #region UI 
    [SerializeField] private UIGroup_Class uiGroup_class;
    [SerializeField] private UIGroup_Item uiGroup_item;
    [SerializeField] private UIGroup_Pet uiGroup_pet;
    #endregion

    #region Preference
    #endregion

    #region Public Methods
    public void SetUI(MileageBaseDataIdentifier identifier)
    {
        if (init == false)
            Initialize();

        switch (identifier.dataType)
        {
            case MileageDataEvaluateTargetDataType.None:
                ZLog.LogError(ZLogChannel.UI, "dataType null");
                break;
            case MileageDataEvaluateTargetDataType.Item:
                SetUI_Item(DBItem.GetItem(identifier.tid));
                break;
            case MileageDataEvaluateTargetDataType.Change:
                SetUI_Change(DBChange.Get(identifier.tid));
                break;
            case MileageDataEvaluateTargetDataType.Pet:
                SetUI_Pet(DBPet.GetPetData(identifier.tid));
                break;
            case MileageDataEvaluateTargetDataType.Rune:
                ZLog.LogError(ZLogChannel.UI, " no implete");
                break;
            default:
                ZLog.LogError(ZLogChannel.UI, "no type matching");
                break;
        }

        bool targetRootFound = false;

        /// 타입에 맞게 Active 
        foreach (var groupInte in uiGroupIntegrated)
        {
            bool rootActive = identifier.dataType == groupInte.dataType;

            if (rootActive)
            {
                targetRootFound = true;

                foreach (var inactiveObj in groupInte.inactiveList)
                {
                    inactiveObj.SetActive(false);
                }

                foreach (var activeObj in groupInte.activeList)
                {
                    activeObj.gameObject.SetActive(true);
                }
            }

            groupInte.root.SetActive(rootActive);
        }

        if (targetRootFound == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Please Add Target UiGroup to the integrated Object");
        }
    }

    public void SetOnClickHandler(Action onClicked)
    {
        OnClickedListener = onClicked;
    }

    public void OnClicked()
    {
        OnClickedListener?.Invoke();
    }
    #endregion

    #region Private Methods
    private void Initialize()
    {
        if (init)
            return;

        /// Group 이 새로 생기면 Add 해주는 처리가 필요함 
        uiGroupIntegrated = new List<UIGroupBase>();
        uiGroupIntegrated.Add(uiGroup_class);
        uiGroupIntegrated.Add(uiGroup_item);
        uiGroupIntegrated.Add(uiGroup_pet);

        init = true;
    }

    private void SetUI_Change(Change_Table data)
    {
        uiGroup_class.imgGrade.sprite = GetGradeSprite(data.Grade);
        uiGroup_class.imgElement.sprite = ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetAttributeSpriteName(data.AttributeType));
        uiGroup_class.imgMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.Icon);
        //uiGroup_class.imgClass.sprite = ZManagerUIPreset.Instance.GetSprite(data.ClassIcon);
        uiGroup_class.imgClass.sprite = ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetCharacterTypeSprite(data.UseAttackType));
        uiGroup_class.txtName.text = DBLocale.GetText(data.ChangeTextID);
        uiGroup_class.txtName.color = UIFrameMileage.GetColorByGrade(data.Grade);
        uiGroup_class.txtProperty.text = string.Format("{0} - {1}", DBLocale.GetText(UIFrameMileage.GetAttributeName(data.AttributeType)), DBLocale.GetCharacterTypeName(data.UseAttackType));
    }

    private void SetUI_Item(Item_Table data)
    {
        uiGroup_item.imgMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
        uiGroup_item.imgClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetCharacterTypeSprite(data.UseCharacterType));
        uiGroup_item.imgGrade.sprite = GetGradeSprite(data.Grade);
        uiGroup_item.txtProperty.text = string.Format("{0} - {1}", DBLocale.GetText(DBLocale.GetCharacterTypeName(data.UseCharacterType)), DBLocale.GetItemTypeText(data.ItemType));
        uiGroup_item.txtName.text = DBLocale.GetText(data.ItemTextID);
        uiGroup_item.txtName.color = UIFrameMileage.GetColorByGrade(data.Grade);
    }

    private void SetUI_Pet(Pet_Table data)
    {
        uiGroup_pet.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.Icon);
        uiGroup_pet.imgGrade.sprite = GetGradeSprite(data.Grade);

        string petTypeSpriteName = "";

        if (data.PetType == E_PetType.Pet)
        {
            petTypeSpriteName = "icon_equip_ride";
        }
        else if (data.PetType == E_PetType.Vehicle)
        {
            petTypeSpriteName = "icon_equip_pet";
        }

        uiGroup_pet.imgPetType.sprite = ZManagerUIPreset.Instance.GetSprite(petTypeSpriteName);
    }

    #region Util
    private Sprite GetGradeSprite(byte grade)
    {
        return ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(grade));
    }


    //private string GetCharacterTypeName(E_CharacterType type)
    //{
    //    switch (type)
    //    {
    //        case E_CharacterType.Knight:
    //            return "Character_Knight";
    //        case E_CharacterType.Archer:
    //            return "Character_Archer";
    //        case E_CharacterType.Wizard:
    //            return "Character_Wizard";
    //        case E_CharacterType.Assassin:
    //            return "Character_Assassin";
    //        case E_CharacterType.All:
    //            ZLog.LogError(ZLogChannel.UI, "cannot be all here");
    //            return "";
    //        default:
    //            break;
    //    }

    //    return "";
    //}

    #endregion

    #endregion

    #region Define
    /// <summary>
    /// UIGroup Base 클래스 
    /// </summary>
    [System.Serializable]
    public class UIGroupBase
    {
        public MileageDataEvaluateTargetDataType dataType;
        public GameObject root;
        public List<GameObject> activeList;
        public List<GameObject> inactiveList;
    }

    [Serializable]
    public class UIGroup_Class : UIGroupBase
    {
        public Image imgMainIcon;
        public Image imgGrade;
        public Image imgClass;
        public Image imgElement;
        public Text txtProperty;
        public Text txtName; /// Grade Color 설정 필요 
    }

    [Serializable]
    public class UIGroup_Item : UIGroupBase
    {
        public Image imgMainIcon;
        public Image imgClassIcon;
        public Image imgGrade;
        public Text txtProperty;
        public Text txtName;
    }

    [Serializable]
    public class UIGroup_Pet : UIGroupBase
    {
        public Image imgIcon;
        public Image imgGrade;
        public Image imgPetType;
    }

    #endregion
}
