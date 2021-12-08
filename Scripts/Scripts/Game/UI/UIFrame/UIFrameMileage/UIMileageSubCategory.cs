using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMileageSubCategory : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private ZToggle toggle;
    [SerializeField] private Text txtTitle;

    public RectTransform RectTransform { get { return rectTransform; } }
    public ZToggle Toggle { get { return toggle; } }

    public uint MileageShopID { get; private set; }

    public void Initialize(ToggleGroup toggleGroup)
    {
        if (toggle.group != toggleGroup)
            Toggle.group = toggleGroup;

        var ztg = toggleGroup as ZToggleGroup;

        if (toggle.group != ztg)
            Toggle.ZToggleGroup = ztg;
    }

    public void Set(
        uint subCategoryMileageShopID
        , string title)
    {
        MileageShopID = subCategoryMileageShopID;
        txtTitle.text = title;
    }

    public void SetToggle(bool isOn)
    {
        toggle.isOn = isOn;
    }
}