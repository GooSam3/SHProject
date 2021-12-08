using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPetAdventureViewHolder : ZAdapterHolderBase<OSA_AdventureData>
{
    private UIPetAdventureListItem listItem;
    private Action<OSA_AdventureData> onClick;

    public override void SetSlot(OSA_AdventureData data)
    {
        listItem.SetSlot(data);
    }

    public override void CollectViews()
    {
        base.CollectViews();

        listItem = root.GetComponent<UIPetAdventureListItem>();
    }

    public void SetAction(Action<OSA_AdventureData> _onClick)
    {
        onClick = _onClick;
        listItem.SetAction(onClick);
    }
}

/// <summary>
/// 완료 - 파랑 a1
/// 노말 - 회색 a.5
/// 대기 - 회색 a.25
/// </summary>

public class UIPetAdventureListItem : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] private Color colorOn;
    [SerializeField] private Color colorOff;

    [SerializeField] private Image imgBG;

    [SerializeField] private Image imgIcon;

    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtState;

    [SerializeField] private GameObject objSelect;

    private OSA_AdventureData data;

    private Action<OSA_AdventureData> onClick;

    public void SetSlot(OSA_AdventureData _data)
    {
        data = _data;

        imgIcon.sprite = UICommon.GetSprite(data.table.AdventureIcon);
        txtTitle.text = DBLocale.GetText(data.table.AdventureNameText);

        imgBG.color = colorOff;

        switch (data.advData.status)
        {
            case WebNet.E_PetAdvStatus.Wait://받을수있다
                canvasGroup.alpha = .5f;
                txtState.text = string.Empty;
                break;
            case WebNet.E_PetAdvStatus.Start://진행중
                if (data.advData.EndDt < TimeManager.NowSec)//완료
                {
                    imgBG.color = colorOn;
                    canvasGroup.alpha = 1;
                    txtState.text = DBLocale.GetText("PetAdventur_End");
                }
                else
                {
                    canvasGroup.alpha = .25f;
                    txtState.text = DBLocale.GetText("PetAdventure_Leaving");
                }
                break;
            case WebNet.E_PetAdvStatus.Reward:// 쿨타임
            case WebNet.E_PetAdvStatus.Cancel://쿨다임
            default:
                canvasGroup.alpha = .25f;
                txtState.text = string.Empty;
                break;

        }

        objSelect.SetActive(data.isSelected);
    }

    public void SetAction(Action<OSA_AdventureData> _onClick)
    {
        onClick = _onClick;
    }

    public void OnClickSlot()
    {
        onClick?.Invoke(data);
    }
}
