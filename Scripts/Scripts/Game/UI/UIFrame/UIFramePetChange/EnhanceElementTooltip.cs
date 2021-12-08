using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameDB;

public class EnhanceElementTooltip : MonoBehaviour
{
    #region UI
    [SerializeField] private GameObject tooltipRoot;

    [SerializeField] private ZImage ElementGlowImage;
    [SerializeField] private ZImage ElementIcon;
    [SerializeField] private Text ElementName;
    [SerializeField] private Text ElementLevel;

    [SerializeField] private GameObject[] AbilityObjs;
    #endregion

    #region System
    [System.Serializable]
    public class AttributeTooltipInfo
    {
        public E_UnitAttributeType type;
        public Color TextColor;
        public Color GlowColor;
    }

    public List<AttributeTooltipInfo> settingValues;
    #endregion

    public void ShowTooltip(E_UnitAttributeType selectType,string name, string level,Sprite icon)
    {
        tooltipRoot.SetActive(true);

        var settingInfo = settingValues.Find(item => item.type == selectType);

        ElementName.text = name;
        ElementName.color = settingInfo?.TextColor ?? Color.white;
        ElementLevel.text = level;
        ElementLevel.color = settingInfo?.TextColor ?? Color.white;
        ElementIcon.sprite = icon;
        ElementGlowImage.color = settingInfo?.GlowColor ?? Color.white;

        //init
        foreach (var obj in AbilityObjs) obj.SetActive(false);

        var attributeId = ZNet.Data.Me.CurCharData.GetAttributeIDByType(selectType);

        List<UIAbilityData> abilityTypeAndValue = new List<UIAbilityData>();

        int useAbilityUICount = 0;

        if (DBAttribute.GetAttributeByID(attributeId, out var tableData))
        {
            var abilityactionData = DBAbilityAction.Get(tableData.AbilityActionID_01);
            if (abilityactionData != null)
            {
                abilityTypeAndValue.Clear();

                DBAbilityAction.GetAbilityTypeList(abilityactionData, ref abilityTypeAndValue);

                foreach (var typeAndValue in abilityTypeAndValue)
                {
                    DBAbility.GetAbility(typeAndValue.type, out var abilityData);

                    if (abilityData != null)
                    {
                        string unit = abilityData.MarkType == E_MarkType.Per ? "%" : string.Empty;

                        AbilityObjs[useAbilityUICount].SetActive(true);

                        AbilityObjs[useAbilityUICount].transform.Find("Txt_Title").GetComponent<Text>().text = DBLocale.GetText(typeAndValue.type.ToString());
                        AbilityObjs[useAbilityUICount].transform.Find("Txt_Value").GetComponent<Text>().text = string.Format("+{0}{1}", typeAndValue.value, unit);

                        useAbilityUICount++;
                    }
                }
            }

            abilityactionData = DBAbilityAction.Get(tableData.AbilityActionID_02);

            if (abilityactionData != null)
            {
                abilityTypeAndValue.Clear();

                DBAbilityAction.GetAbilityTypeList(abilityactionData, ref abilityTypeAndValue);

                foreach (var typeAndValue in abilityTypeAndValue)
                {
                    DBAbility.GetAbility(typeAndValue.type, out var abilityData);

                    if (abilityData != null)
                    {
                        string unit = abilityData.MarkType == E_MarkType.Per ? "%" : string.Empty;

                        AbilityObjs[useAbilityUICount].SetActive(true);

                        AbilityObjs[useAbilityUICount].transform.Find("Txt_Title").GetComponent<Text>().text = DBLocale.GetText(typeAndValue.type.ToString());
                        AbilityObjs[useAbilityUICount].transform.Find("Txt_Value").GetComponent<Text>().text = string.Format("+{0}{1}", typeAndValue.value, unit);

                        useAbilityUICount++;
                    }
                }
            }
        }
    }

    public void HideTooltip()
    {
        tooltipRoot.SetActive(false);
    }
}
