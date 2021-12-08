using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpecialShopMainCtg : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public LayoutElement layoutElement;

    public ZToggle toggle;
    public Text txtCateogryName_normal;
    public Text txtCategoryName_on;

    public bool IsDummy { get; private set; }

    public E_SpecialShopType Key { get; private set; }

    public void Initialize(ZToggleGroup toggleGroup, E_SpecialShopType key, string ctgName)
    {
        IsDummy = false;
        toggle.group = toggleGroup;
        toggle.ZToggleGroup = toggleGroup;
        txtCateogryName_normal.text = ctgName;
        txtCategoryName_on.text = ctgName;
        toggleGroup.RegisterToggle(toggle);
        this.Key = key;
    }

    public void Initialize_Dummy(ZToggleGroup toggleGroup)
    {
        IsDummy = true;
        toggle.interactable = false;
        canvasGroup.alpha = 0f;
        layoutElement.ignoreLayout = true;
        transform.localPosition = new Vector3(0, -1000, 0);
        toggle.group = toggleGroup;
        toggle.ZToggleGroup = toggleGroup;
        toggleGroup.RegisterToggle(toggle);
    }
}
