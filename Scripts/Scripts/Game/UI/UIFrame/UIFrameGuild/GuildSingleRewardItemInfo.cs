using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildSingleRewardItemInfo : MonoBehaviour
{
    public Image imgIcon;
    public Text txtCnt;

    public void Set(Sprite icon, string strCnt)
    {
        imgIcon.sprite = icon;
        txtCnt.text = strCnt;
    }
}
