using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using GameDB;

public class UISpecialShopSubCtg : MonoBehaviour
{
    public ZToggle toggle;
    public Text txtCateogryName_normal;
    public Text txtCategoryName_on;

    public E_SpecialSubTapType Key { get; private set; }

    public LayoutElement layoutElement;
    public CanvasGroup canvasGroup;

    public void Initialize(ZToggleGroup toggleGroup)
    {
        toggle.group = toggleGroup;
        toggle.ZToggleGroup = toggleGroup;
        toggleGroup.RegisterToggle(toggle);
    }

    public void Set(string ctgName, E_SpecialSubTapType key)
    {
        txtCateogryName_normal.text = ctgName;
        txtCategoryName_on.text = ctgName;
        this.Key = key;

        /// 스페셜상점의 None 은 하나의 탭으로 분리됨. 
        /// 하지만 보이지는 않는 탭임 . 즉 메인 카테고리만 누르면
        /// 출력 아이템들이 바로 보이는 형태 . 
        if (key == E_SpecialSubTapType.None)
        {
            canvasGroup.alpha = 0f;
            this.layoutElement.ignoreLayout = true;
        }
        else
        {
            canvasGroup.alpha = 1f;
            this.layoutElement.ignoreLayout = false;
        }
    }
}
