using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIPopupRegistPetAdventure : UIPopupBase
{
    private const string HEX_NORMAL = "#BBB8A9";
    private const string HEX_WARN = "#FF0000";

    [SerializeField] private UIPetChangeScrollAdapter osaPet;

    [SerializeField] private Text txtCondition;// 탐험능력 -이상
    [SerializeField] private Text txtConditionDetail;// 탐험능력 -/-
    [SerializeField] private Text txtRegistNum;

    [SerializeField] private List<UIPetChangeListItem> listRegistedPetSlot;

    [SerializeField] private ZButton btnConfirm;

    private List<C_PetChangeData> listPetRegisted=  new List<C_PetChangeData>();

    private List<C_PetChangeData> listPetSorted = new List<C_PetChangeData>();

    private E_PCR_PostSetting postIdle = E_PCR_PostSetting.SelectOff | E_PCR_PostSetting.AdventurePowerOn;

    private OSA_AdventureData adventureData;

    private Action<List<uint>> onClickConfirm;

    private uint selectedAdvPower;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        osaPet.Initilize();

        foreach (var iter in listRegistedPetSlot)
            iter.gameObject.SetActive(false);
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        UIManager.Instance.Close<UIScreenBlock>();
    }

    public void SetPopup(OSA_AdventureData data, List<uint> listRegisted, Action<List<uint>> onConfirm)
    {
        adventureData = data;
        onClickConfirm = onConfirm;
        listPetSorted.Clear();
        listPetRegisted.Clear();

        List<C_PetChangeData> registed = new List<C_PetChangeData>();

        foreach (var iter in DBPetAdventure.GetPossibleAdventurePetList())
        {
            var pet = Me.CurCharData.GetPetData(iter.tid);

            if (pet == null)
                continue;


            var petData = new C_PetChangeData(pet) { postSetting = postIdle };
            listPetSorted.Add(petData);

            if (listRegisted.Contains(iter.tid))
                registed.Add(petData);
        }

        txtCondition.text = DBLocale.GetText("PetAdventure_Join_Pet_Des", adventureData.table.NeedPetPower);

        osaPet.ResetData(listPetSorted, OnClickPetSlot, OnClickPetSlot);

        RegistPetSlot(registed.ToArray());
    }

    public void OnClickPetSlot(C_PetChangeData data)
    {
        RegistPetSlot(data);

        //var registed = listPetRegisted.Find(item=>item.Tid == data.Tid);

        //if(registed!=null)
        //{
        //    listPetRegisted.Remove(registed);
        //    registed.postSetting = postIdle;
        //}
        //else
        //{
        //    if (listPetRegisted.Count >= listRegistedPetSlot.Count)
        //        return;

        //    data.postSetting = postIdle | E_PCR_PostSetting.CheckOn;
        //    listPetRegisted.Add(data);
        //}

    }

    private void RegistPetSlot(params C_PetChangeData[] listData)
    {
        foreach(var iter in listData)
        {
            var registed = listPetRegisted.Find(item => item.Tid == iter.Tid);

            if (registed != null)
            {
                listPetRegisted.Remove(registed);
                registed.postSetting = postIdle;
            }
            else
            {
                if (listPetRegisted.Count >= listRegistedPetSlot.Count)
                    break ;

                iter.postSetting = postIdle | E_PCR_PostSetting.CheckOn;
                listPetRegisted.Add(iter);
            }
        }
        RefreshRegistList();
    }

    public void OnClickPetSlot(int i)
    {
        if (listPetRegisted.Count <= i)
            return;

        listPetRegisted[i].postSetting = postIdle;
        listPetRegisted.RemoveAt(i);

        RefreshRegistList();
    }

    public void RefreshCondition(uint advPower)
    {
        selectedAdvPower = advPower;
        bool isEnough = advPower >= adventureData.table.NeedPetPower;

        var progress = UICommon.GetColoredText(isEnough ? HEX_NORMAL : HEX_WARN, UICommon.GetProgressText((int)advPower, (int)adventureData.table.NeedPetPower));

        txtConditionDetail.text = $"{DBLocale.GetText("PetAdventure_PetPower")} {progress}";
        //btnConfirm.interactable = advPower >= adventureData.table.NeedPetPower;
    }

    public void RefreshRegistList()
    {
        uint totalAdvData = 0;

        for(int i =0;i<listRegistedPetSlot.Count;i++)
        {
            if (listPetRegisted.Count <= i)
            {
                listRegistedPetSlot[i].gameObject.SetActive(false);
                continue;
            }
            listRegistedPetSlot[i].SetSlotSimple(listPetRegisted[i]);
            listRegistedPetSlot[i].gameObject.SetActive(true);


            totalAdvData += DBPetAdventure.GetPetAdventurePower(listPetRegisted[i].Tid);
        }
        osaPet.RefreshData();

        txtRegistNum.text = UICommon.GetProgressText(listPetRegisted.Count, listRegistedPetSlot.Count, false);
        RefreshCondition(totalAdvData);
    }

    public void OnClickAutoRegist()
    {
        List<C_PetChangeData> list = new List<C_PetChangeData>();

        foreach(var iter in listPetSorted)
        {
            if (listPetRegisted.Count >= listRegistedPetSlot.Count)
                return;

            if (listPetRegisted.Contains(iter))
                continue;

            list.Add(iter);
        
        }

        RegistPetSlot(list.ToArray());
    }

    public void OnClickConfirm()
    {
        if(listPetRegisted.Count<=0)
		{
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("No_Fellow_Registration"));
            return;
        }

        if (selectedAdvPower<adventureData.table.NeedPetPower)
		{
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("No_Exploratory_Capacity"));
            return;
		}

        var list = new List<uint>();

        foreach (var iter in listPetRegisted)
            list.Add(iter.Tid);

        onClickConfirm?.Invoke(list);
        OnClickClose();
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close<UIPopupRegistPetAdventure>(true);
    }
}
