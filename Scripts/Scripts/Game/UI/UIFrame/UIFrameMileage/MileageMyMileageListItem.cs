using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MileageMyMileageListItem : MonoBehaviour
{
    public Image imgIcon;
    public Text txtTitle;
    public Text txtPercentage;
    public Text txtAffordCnt;

    public uint MileageBuyItemID { get; private set; }
    public uint SingleItemCostCount { get; private set; }
    public ulong AffordCount { get; private set; }

    public void Initialize(Sprite sprite, string title, uint buyItemID, uint singleItemCost)
    {
        imgIcon.sprite = sprite;
        txtTitle.text = title;
        MileageBuyItemID = buyItemID;
        SingleItemCostCount = singleItemCost;
    }

    public void Set(string percentage, ulong affordCnt)
    {
        txtPercentage.text = percentage;
        txtAffordCnt.text = affordCnt.ToString();
        AffordCount = affordCnt;
    }
}
