using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIMessagePopupPREquip : ZUIFrameBase
{
    private enum E_EquipInterectType
    {
        Equip,
        ReleaseEquip,
        Release
    }

    [SerializeField] private Text TextDesc;

    [SerializeField] private GameObject btnEquip;
    [SerializeField] private Text txtEquipBtn;

    [SerializeField] private GameObject btnDestoryEquip;

    [SerializeField] private GameObject objCostGroup;

    [SerializeField] private Image imgCost;
    [SerializeField] private Text txtCost;

    private E_EquipInterectType equipType;

    private bool isEquipProcess = false;

    private Action postAction;
    private Action onClose;

    private uint targetTid;

    private List<PetRuneData> runeData = new List<PetRuneData>();

    //private PetRuneData runeData;

    public void Set(bool _isEquipProcess, uint _targetTid, List<PetRuneData> _runeData, Action _postAction, Action _onClose)
    {
        runeData.Clear();
        runeData.AddRange(_runeData);

        isEquipProcess = _isEquipProcess;
        postAction = _postAction;
        targetTid = _targetTid;
        onClose = _onClose;

        SetUI();
    }

    public void Set(bool _isEquipProcess, uint _targetTid, PetRuneData _runeData, Action _postAction, Action _onClose)
    {
        runeData.Clear();
        runeData.Add(_runeData);

        isEquipProcess = _isEquipProcess;
        postAction = _postAction;
        targetTid = _targetTid;
        onClose = _onClose;

        SetUI();
    }

    private void SetUI()
    {
        bool isEquiped = GetRemoveIDList().Count>0;

        objCostGroup.gameObject.SetActive(isEquiped);

        btnDestoryEquip.SetActive(isEquiped && isEquipProcess);

        if (isEquipProcess)//해제 후 장착 / 장착
        {
            if (isEquiped)
            {
                TextDesc.text = DBLocale.GetText("Rune_Equip_device_Message");
                txtEquipBtn.text = DBLocale.GetText("Rune_Equip_Disarm");
                equipType = E_EquipInterectType.ReleaseEquip;
            }
            else
            {
                TextDesc.text = DBLocale.GetText("Rune_Equip_Message_01");
                txtEquipBtn.text = DBLocale.GetText("Equip_Text");
                equipType = E_EquipInterectType.Equip;
            }
        }
        else//해제
        {
            TextDesc.text = DBLocale.GetText("Rune_Lift_Message");
            txtEquipBtn.text = DBLocale.GetText("Lift_Text");
            equipType = E_EquipInterectType.Release;
        }

        imgCost.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItem(DBConfig.Runelift_Use_Item).IconID);

        var cost = GetReleaseCost();
        txtCost.text = cost.ToString("N0");

        txtCost.color = ConditionHelper.CheckCompareCost(DBConfig.Runelift_Use_Item, cost, false) ? Color.white : Color.red;

    }

    private ulong GetReleaseCost()
    {
        ulong cost = 0;

        var dic = Me.CurCharData.GetEquipRuneDic(targetTid);

        foreach (var iter in runeData)
        {
            if (dic.ContainsKey(iter.SlotType) == false)
                continue;

            if (DBItem.GetItem(dic[iter.SlotType].RuneTid, out var table) == false)
                continue;

            if (isEquipProcess)
            {
                if (iter.OwnerPetTid > 0)
                    continue;
            }
            else
            {
                if (dic[iter.SlotType].RuneId == iter.RuneId)
                {
                    cost += table.RuneliftItemCount;
                    continue;
                }
            }

            cost += table.RuneliftItemCount;
        }

        return cost;
    }

    private List<ulong> GetEquipIDList()
    {
        List<ulong> equipId = new List<ulong>();

        foreach (var iter in runeData)
        {
            if (iter.OwnerPetTid > 0)
                continue;

            equipId.Add(iter.RuneId);
        }

        return equipId;
    }

    private List<ulong> GetRemoveIDList()
    {
        List<ulong> removeID = new List<ulong>();

        var dic = Me.CurCharData.GetEquipRuneDic(targetTid);

        foreach (var iter in runeData)
        {
            if (dic.ContainsKey(iter.SlotType) == false)
                continue;

            if (isEquipProcess)
            {
                if (iter.OwnerPetTid > 0)
                    continue;
            }
            else
            {
                if (dic[iter.SlotType].RuneId == iter.RuneId)
                {
                    removeID.Add(iter.RuneId);
                    continue;
                }
            }


            removeID.Add(dic[iter.SlotType].RuneId);
        }

        return removeID;
    }


    // 장착/해제
    public void OnClickEquip()
    {
        var equipId = GetEquipIDList();
        var removeId = GetRemoveIDList();

        switch (equipType)
        {
            case E_EquipInterectType.Equip:
                ZWebManager.Instance.WebGame.REQ_RuneEquip(targetTid, equipId, delegate
                {
                    postAction?.Invoke();
                    OnClickCancel();
                }, null);
                break;

            case E_EquipInterectType.ReleaseEquip:
                if (ConditionHelper.CheckCompareCost(DBConfig.Runelift_Use_Item, GetReleaseCost()) == false)
                    return;

                ZWebManager.Instance.WebGame.REQ_RuneUnequip(targetTid, removeId, delegate
                {
                    ZWebManager.Instance.WebGame.REQ_RuneEquip(targetTid, equipId, delegate
                    {
                        postAction?.Invoke();
                        OnClickCancel();
                    }, null);
                }, null);


                break;
            case E_EquipInterectType.Release:
                if (ConditionHelper.CheckCompareCost(DBConfig.Runelift_Use_Item, GetReleaseCost()) == false)
                    return;

                ZWebManager.Instance.WebGame.REQ_RuneUnequip(targetTid, removeId, delegate
                {
                    postAction?.Invoke();
                    OnClickCancel();
                }, null);
                break;
        }
    }

    // 파괴 후 장착
    public void OnClickDestoryEquip()
    {
        var equipId = GetEquipIDList();
        var removeId = GetRemoveIDList();

        ZWebManager.Instance.WebGame.REQ_RuneDelete(removeId, delegate
        {
            ZWebManager.Instance.WebGame.REQ_RuneEquip(targetTid, equipId, (recvPacket, recvMsgPacket) =>
            {
                postAction?.Invoke();
                OnClickCancel();
            }, null);
        }, null);
    }

    public void OnClickCancel()
    {
        onClose?.Invoke();
        UIManager.Instance.Close<UIMessagePopupPREquip>(true);
    }
}
