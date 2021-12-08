using System;
using UnityEngine;
using UnityEngine.UI;

public class UIEnhanceStatListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Text TypeText, PrevValueText, NextValeText, AddValueText;
    [SerializeField] private GameObject ValueUpArrow, ValueDownArrow;
    #endregion

    public void Initialize()
    {
        TypeText.text = string.Empty;
        PrevValueText.text = string.Empty;
        NextValeText.text = string.Empty;
        //AddValueText.text = string.Empty;
        ValueUpArrow.SetActive(false);
        ValueDownArrow.SetActive(false);
    }

    public void SetSlot(UIFrameItemEnhance.EnhanceDestInfo _prevAbility)
    {
        Initialize();

        if (_prevAbility.PrevText == null || _prevAbility.NextText == null || _prevAbility.PrevText == string.Empty || _prevAbility.NextText == string.Empty)
            return;

        uint preValue = Convert.ToUInt32(_prevAbility.PrevText.Replace("%",""));
        uint nextValue = Convert.ToUInt32(_prevAbility.NextText.Replace("%",""));

        TypeText.text = _prevAbility.TypeText;
        PrevValueText.text = preValue < nextValue ? _prevAbility.PrevText : _prevAbility.NextText;
        NextValeText.text = preValue < nextValue ? _prevAbility.NextText : _prevAbility.PrevText;

        ValueUpArrow.SetActive(_prevAbility.AddValue != 0 && preValue < nextValue);
        ValueDownArrow.SetActive(_prevAbility.AddValue != 0 && preValue > nextValue);

        if (_prevAbility.AddValue != 0)
        {
            if (preValue < nextValue)
                NextValeText.text += string.Format("(+{0})", _prevAbility.AddValue);
            else
                NextValeText.text += string.Format("({0})", _prevAbility.AddValue);
        }
    }
}
