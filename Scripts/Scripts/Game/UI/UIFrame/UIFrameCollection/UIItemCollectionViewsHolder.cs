using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;

public class UIItemCollectionViewsHolder : BaseItemViewsHolder
{
    public class CollectionAbilityInfo
    {
        public string strDesc;
        public string value;
    }

    #region OSA UI Variable
    private ZImage Icon;
    private ZText TitleText;
    private ZText TitleNumText;
    private ZText OptionText;
    private ZText CollectionCountText;
    private ZButton ButtonDown, ButtonUp;
    private RectTransform CollectionSlotRow;

    private SelectCollectInfo SelectCollectionInfo_1;
    private SelectCollectInfo SelectCollectionInfo_2;
    private SelectCollectInfo SelectCollectionInfo_3;
    private SelectCollectInfo SelectCollectionInfo_4;
    private SelectCollectInfo SelectCollectionInfo_5;
    private SelectCollectInfo SelectCollectionInfo_6;
    private SelectCollectInfo SelectCollectionInfo_7;
    #endregion

    #region OSA System Variable
    private ScrollItemCollectionData ItemCollectionData = null;
    private ItemCollection_Table ItemCollectionTable = null;
    private List<CollectionAbilityInfo> CollectionAbilityInfoList = new List<CollectionAbilityInfo>();
    #endregion

    public override void CollectViews()
    {
        base.CollectViews();
        root.GetComponentAtPath("Panel/Row_01/Img_Icon", out Icon);
        root.GetComponentAtPath("Panel/Row_01/Txt_Title", out TitleText);
        root.GetComponentAtPath("Panel/Row_01/Txt_Title_Num", out TitleNumText);
        root.GetComponentAtPath("Panel/Row_01/Txt_Option", out OptionText);
        root.GetComponentAtPath("Panel/Row_01/Txt_Title_Num", out CollectionCountText);
        root.GetComponentAtPath("Panel/Row_01/Img_Arrow_Down", out ButtonDown);
        root.GetComponentAtPath("Panel/Row_02/Img_Arrow_Up", out ButtonUp);
        root.GetComponentAtPath("Panel/Row_02", out CollectionSlotRow);

        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (0)", out SelectCollectionInfo_1);
        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (1)", out SelectCollectionInfo_2);
        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (2)", out SelectCollectionInfo_3);
        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (3)", out SelectCollectionInfo_4);
        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (4)", out SelectCollectionInfo_5);
        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (5)", out SelectCollectionInfo_6);
        root.GetComponentAtPath("Panel/Row_02/Grid/SelectCollectInfo (6)", out SelectCollectionInfo_7);

        ButtonDown.onClick.AddListener(OnClickButtonDown);
        ButtonUp.onClick.AddListener(OnClickButtonUp);

        //TODO :: 일단 화살표 비활성화
        ButtonDown.gameObject.SetActive(false);
        ButtonUp.gameObject.SetActive(false);
    }

	private void OnClickButtonDown()
	{
        //CollectionSlotRow.gameObject.SetActive(true);
    }

    private void OnClickButtonUp()
    {
        //CollectionSlotRow.gameObject.SetActive(false);
    }

    public void UpdateViews(ScrollItemCollectionData _model)
    {
        ItemCollectionData = null;
        ItemCollectionTable = null;
        CollectionAbilityInfoList.Clear();

        if (_model == null)
            return;

        ItemCollectionData = _model;
        ItemCollectionTable = _model.ItemCollection;

        TitleText.text = DBLocale.GetText(ItemCollectionTable.ItemCollectionTextID);
        TitleNumText.text = string.Format("({0}/{1})", 0, ItemCollectionTable.CollectionItemCount);

        CollectionAbilityInfoList.Clear();
        CollectionAbilitySetting(ItemCollectionTable);
        OptionText.text = string.Empty;
        for (int i = 0; i < CollectionAbilityInfoList.Count; i++)
        {
            OptionText.text += string.Format("{0} {1}\n", CollectionAbilityInfoList[i].strDesc, CollectionAbilityInfoList[i].value);
        }

        CollectData collectData = Me.CurCharData.GetCollectData(ItemCollectionTable.ItemCollectionID, CollectionType.TYPE_ITEM);
        SelectCollectionInfo_1.Reset(0, collectData, ItemCollectionTable);
        SelectCollectionInfo_2.Reset(1, collectData, ItemCollectionTable);
        SelectCollectionInfo_3.Reset(2, collectData, ItemCollectionTable);
        SelectCollectionInfo_4.Reset(3, collectData, ItemCollectionTable);
        SelectCollectionInfo_5.Reset(4, collectData, ItemCollectionTable);
        SelectCollectionInfo_6.Reset(5, collectData, ItemCollectionTable);
        SelectCollectionInfo_7.Reset(6, collectData, ItemCollectionTable);

        CollectionCountText.text = string.Format("{0}/{1}", collectData?.DataCnt ?? 0, ItemCollectionTable.CollectionItemCount);
    }

    private void CollectionAbilitySetting(ItemCollection_Table _itemCollectionTable)
    {
        List<uint> listAbilityActionIds = new List<uint>();
        Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

        if (ItemCollectionTable.AbilityActionID_01 != 0)
            listAbilityActionIds.Add(ItemCollectionTable.AbilityActionID_01);
        if (ItemCollectionTable.AbilityActionID_02 != 0)
            listAbilityActionIds.Add(ItemCollectionTable.AbilityActionID_02);

        foreach (var abilityActionId in listAbilityActionIds)
        {
            var abilityActionData = DBAbility.GetAction(abilityActionId);
            switch (abilityActionData.AbilityViewType)
            {
                case GameDB.E_AbilityViewType.ToolTip:
                    var itemDescInfo = new CollectionAbilityInfo();
                    itemDescInfo.strDesc = string.Format("{0}{1}", "", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
                    itemDescInfo.value = null;
                    CollectionAbilityInfoList.Add(itemDescInfo);
                    break;
                case GameDB.E_AbilityViewType.Not:
                default:
                    var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        if (!abilitys.ContainsKey(enumer.Current.Key))
                        {
                            abilitys.Add(enumer.Current.Key, enumer.Current.Value);
                        }
                    }
                    break;
            }
        }

        foreach (var ability in abilitys)
        {
            if (!DBAbility.IsParseAbility(ability.Key)) 
                continue;

            float abilityminValue = (uint)abilitys[ability.Key].Item1;
            float abilitymaxValue = (uint)abilitys[ability.Key].Item2;

            var collectionDescInfo = new CollectionAbilityInfo();

            collectionDescInfo.strDesc = DBLocale.GetText(DBAbility.GetAbilityName(ability.Key));
            var newValue = DBAbility.ParseAbilityValue(ability.Key, abilityminValue, abilitymaxValue);
            collectionDescInfo.value = string.Format("{0}", newValue);

            CollectionAbilityInfoList.Add(collectionDescInfo);
        }
    }
}
