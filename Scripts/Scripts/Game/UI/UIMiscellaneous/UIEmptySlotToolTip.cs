using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEmptySlotToolTip : MonoBehaviour
{
    [Serializable]
    private class TooltipComp
	{
        [SerializeField] private GameObject objToolTip;
        [SerializeField] private Text txtToolTip;


        public void SetToolTip(string str)
		{
            objToolTip.SetActive(true);
            txtToolTip.text = DBLocale.GetText(str);
		}

        public void ToolTipOff()
		{
            objToolTip.SetActive(false);
		}
	}


    [SerializeField] private string EquipLocaleID;

    [SerializeField] private TooltipComp toolTip;


    public void OnPointerEnter(BaseEventData eventData)
    {
        toolTip.SetToolTip(EquipLocaleID);
    }

    public void OnPointerExit(BaseEventData eventData)
    {
        toolTip.ToolTipOff();
    }
}
