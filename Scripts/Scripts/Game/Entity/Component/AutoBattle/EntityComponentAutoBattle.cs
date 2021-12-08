using GameDB;
using System;
using UnityEngine;
using ZDefine;
using ZNet.Data;

public class EntityComponentAutoBattle : EntityComponentBase<ZPawnMyPc>
{
    public bool isAutoPlay = false;

    protected override void OnInitializeComponentImpl()
    {
        isAutoPlay = false;

        base.OnInitializeComponentImpl();

        InvokeRepeating(nameof(CheckMyEntityHp), 0.1f, 0.1f);
        InvokeRepeating(nameof(CheckAutoBuyPotion), 0.1f, 0.1f);
        InvokeRepeating(nameof(CheckMyEntityGodTear), 0.1f, 1.0f);
    }

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();

        CancelInvoke(nameof(CheckMyEntityHp));
        CancelInvoke(nameof(CheckAutoBuyPotion));
        CancelInvoke(nameof(CheckMyEntityGodTear));
    }

    private void CheckMyEntityHp()
    {
        if (false == ZMmoManager.hasInstance || false == ZMmoManager.Instance.IsConnected)
            return;

        if(Owner.IsDead)
            return;

        float hpRate = Owner.CurrentHp / Owner.MaxHp;
        uint potionId = 0;
        float potionPer = 0;

        if(ZGameOption.Instance.HP_PotionUsePriority == ZGameOption.HPPotionUsePriority.NORMAL)
        {
            potionId = DBConfig.HPPotion_Normal_ItemID;
            potionPer = ZGameOption.Instance.NormalHPPotionPer;
        }
        else
        {
            potionId = DBConfig.HPPotion_High_ID;
            potionPer = ZGameOption.Instance.BigHPPotionPer;
        }

        if (hpRate < potionPer)
        {
            var potion = Me.CurCharData.InvenList.Find(item => item.item_tid == potionId && item.netType == NetItemType.TYPE_STACK);

            if(null != potion && potion.cnt > 0)
			{
                float coolTime = DBItem.GetItemCoolTime(potion.item_tid);

                if (potion.UseTime + (ulong)(coolTime * 1000) > TimeManager.NowMs)
                    return;
                
                ZWebManager.Instance.WebGame.UseItemAction(potion, false, null);
            }
        }
    }

    private float LastBuyReqTime;

    private void CheckAutoBuyPotion()
    {
        if (!Me.FindCurCharData?.IsCompletePetCollection(DBConfig.PetCollection_Auto_HpPotion) ?? false)
            return;

        if ((LastBuyReqTime + 10) >= TimeManager.NowMs)
            return;

        if(ZGameOption.Instance.NormalPotionAutoBuy || ZGameOption.Instance.BigPotionAutoBuy)
        {
            if (ZGameOption.Instance.HP_PotionUsePriority == ZGameOption.HPPotionUsePriority.NORMAL)
            {
                if (ZGameOption.Instance.NormalPotionAutoBuy)
                {
                    if(!CheckBuy(DBConfig.HPPotion_Normal_ItemID, DBConfig.AutoBuy_HpPotion))
                    {
                        if(ZGameOption.Instance.BigPotionAutoBuy)
                        {
                            CheckBuy(DBConfig.HPPotion_High_ID, DBConfig.AutoBuy_HpPotion_Large);
                        }
                    }
                }
                else
                {
                    if (ZGameOption.Instance.BigPotionAutoBuy)
                    {
                        CheckBuy(DBConfig.HPPotion_High_ID, DBConfig.AutoBuy_HpPotion_Large);
                    }
                }
            }
            else if (ZGameOption.Instance.HP_PotionUsePriority == ZGameOption.HPPotionUsePriority.BIG)
            {
                if(ZGameOption.Instance.BigPotionAutoBuy)
                {
                    if(!CheckBuy(DBConfig.HPPotion_High_ID, DBConfig.AutoBuy_HpPotion_Large))
                    {
                        if(ZGameOption.Instance.NormalPotionAutoBuy)
                        {
                            CheckBuy(DBConfig.HPPotion_Normal_ItemID, DBConfig.AutoBuy_HpPotion);
                        }
                    }
                }
                else
                {
                    if(ZGameOption.Instance.NormalPotionAutoBuy)
                    {
                        CheckBuy(DBConfig.HPPotion_Normal_ItemID, DBConfig.AutoBuy_HpPotion);
                    }
                }
            }
        }
    }

    private bool CheckBuy(uint itemTid, uint shopTid)
    {
        ulong potionCount = Me.CurCharData.InvenList.Find(item => item.item_tid == itemTid && item.netType == NetItemType.TYPE_STACK)?.cnt ?? 0;

        if (potionCount <= DBConfig.AutoBuy_HpPotion_MinCount)
        {
            var shopData = DBNormalShop.GetShopData(shopTid, E_ShopType.Normal, E_NormalShopType.General);

            ZItem item = Me.CurCharData.GetInvenItemUsingMaterial(shopData.BuyItemID);
            
            if(item != null && (shopData.BuyItemCount * DBConfig.AutoBuy_HpPotion_BuyCount) <= item.cnt)
            {
                LastBuyReqTime = TimeManager.NowMs;

                ZWebManager.Instance.WebGame.REQ_BuyItem(true, shopTid, DBConfig.AutoBuy_HpPotion_BuyCount, item.item_id, item.item_tid, (recvPacket, recvMsgPacket) =>
                {
                    LastBuyReqTime = 0;
                });

                return true;
            }
        }
        
        return false;
    }

    // 신의 은총?(이카루스빛) 자동설정
    private void CheckMyEntityGodTear()
    {
        if (false == ZMmoManager.hasInstance || false == ZMmoManager.Instance.IsConnected)
            return;

        ZItem usePotionItem = Me.CurCharData.InvenList.Find(item => item.item_tid == DBConfig.GodTear_ItemID_02 && item.netType == NetItemType.TYPE_STACK);
        if (null == usePotionItem || usePotionItem.cnt <= 0)
        {
            usePotionItem = null;
        }   

        if (null == usePotionItem)
        {
            usePotionItem = Me.CurCharData.InvenList.Find(item => item.item_tid == DBConfig.GodTear_ItemID && item.netType == NetItemType.TYPE_STACK);
            if (null == usePotionItem || usePotionItem.cnt <= 0)
                return;
        }

        
        uint potionPer = ZGameOption.Instance.GodTearStackCnt;
        uint stackCount = GetGodBuffStackCount();


        if(potionPer > stackCount)
        {
            ulong useCount = potionPer - stackCount;

            useCount = Math.Min(useCount, usePotionItem.cnt);

            if (0 < useCount)
            {
                Me.CurCharData.SetUseItemTime(usePotionItem.item_tid, TimeManager.NowMs);
                ZWebManager.Instance.WebGame.REQ_ItemUse(usePotionItem.item_id, usePotionItem.item_tid, (uint)useCount, null);
                //ZWebManager.Instance.WebGame.UseItemAction(usePotionItem, false, null);
            }
        }    
    }

    private uint GetGodBuffStackCount()
    {
        var buffData = Owner.GetGodBuffAbilityAction();

        if (null == buffData)
            return 0;

        if (buffData.EndServerTime <= TimeManager.NowSec)
            return 0;

        var tabledata = DBAbility.GetAction(buffData.AbilityActionId);
        return (uint)Mathf.CeilToInt((float)(buffData.EndServerTime - TimeManager.NowSec) / (float)tabledata.MinSupportTime);
    }
}
