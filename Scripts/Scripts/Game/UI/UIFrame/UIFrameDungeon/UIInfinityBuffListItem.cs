using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfinityBuffListItem : MonoBehaviour
{
    [SerializeField] private ZImage BuffIcon;
    [SerializeField] private GameObject Normal;
    [SerializeField] private GameObject Lock;

    private UIInfinityTowerInfo TowerInfo;

    public void Init(string buffIconString, UIInfinityTowerInfo towerInfo)
    {
        TowerInfo = towerInfo;
        BuffIcon.sprite = ZManagerUIPreset.Instance.GetSprite(buffIconString);
    }

    public void ShowAccumBuffListPopup()
    {
        TowerInfo.OpenAccumBuffListPopup();
    }
}
