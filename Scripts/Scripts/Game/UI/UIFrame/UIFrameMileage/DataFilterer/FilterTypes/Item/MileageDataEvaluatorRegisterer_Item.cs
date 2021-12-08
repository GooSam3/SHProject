using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MileageDataEvaluatorRegisterer
{
    void Initialize_Item(MileageDataWrapper dataWrapper)
    {
        /// 무조건 보여줌 
        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_All, (data) =>
        {
            return true;
        });

        #region Parts 
        /// ( Accessory 같은 경우는 Equip 인데 현재 (2020/10/22) 에는 Accessory 의 필터링 옵션을 UI 로 세팅하는 부분이 없음. 고로 Equip Filtering 일때는 Accessory 무조건 True 체킹
        /// ( 마석(Gem) 도 마찬가지로 Equip 지만 아무 필터링 적용안함 . 
        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_All, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return true;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Helmet, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Helmet;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Armor, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Armor;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Gloves, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Gloves;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Pants, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Pants;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Shoes, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Shoes;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Cape, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Cape;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_Weapon, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.Weapon;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Parts_SideWeapon, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsAccessory(data.ItemType) || DBItem.IsEquipGem(data))
                return true;
            return data.ItemType == GameDB.E_ItemType.SideWeapon;
        });

        #endregion

        #region Character Type

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Character_All, (data) =>
        {
            //if (data.ItemUseType != GameDB.E_ItemUseType.Equip)
              //  return true;
            return true;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Character_Gladiator, (data) =>
        {
            /// 장비가 아니거나 , 젬이면 그냥 바로 통과 나머지 동일 
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsEquipGem(data))
                return true;
            return data.UseCharacterType == GameDB.E_CharacterType.Knight;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Character_Archer, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsEquipGem(data))
                return true;
            return data.UseCharacterType == GameDB.E_CharacterType.Archer;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Character_Wizard, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsEquipGem(data))
                return true;
            return data.UseCharacterType == GameDB.E_CharacterType.Wizard;
        });

        dataWrapper.AddEvaluatorPredicate_Item(MileageDataEvaluatorKey.Item_Equipment_Character_Assassin, (data) =>
        {
            if (data.ItemUseType != GameDB.E_ItemUseType.Equip || DBItem.IsEquipGem(data))
                return true;
            return data.UseCharacterType == GameDB.E_CharacterType.Assassin;
        });

        #endregion
    }
}
