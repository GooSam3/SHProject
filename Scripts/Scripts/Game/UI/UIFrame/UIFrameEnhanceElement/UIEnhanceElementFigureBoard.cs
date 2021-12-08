using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEnhanceElementFigureBoard : MonoBehaviour
{
    [Serializable]
    public class ElementFigure
    {
        public E_UnitAttributeType type;
        public Image imgLevelIndicator;
        public Image imgLevelIndicator02;
    }

    #region SerializedField
    #region UI 
    [SerializeField] private RectTransform scrollTarget;
    [SerializeField] private UIEnhanceElementChainLevel chainLevelSource;
    [SerializeField] private List<ElementFigure> elementIndicators;

    [SerializeField] private RectTransform slotContentRoot;
    [SerializeField] private UIEnhanceElementScrollAdapter slotScrollAdapter;
    private UIEnhanceElementChainAndEnhanceSettingProvider SettingProvider;
    #endregion

    #region Preference Variable
    [SerializeField] private float levelOneIndicatorWid = 280;
    [SerializeField] private float levelIndicatorincrementalWid = 203;

    [SerializeField] private float levelIndicatorNotSelectedHeight = 50f;
    [SerializeField] private float levelIndicatorSelectedHeight = 110f;
    #endregion
    #endregion

    #region System Variable
    bool isInitialized;
    private List<UIEnhanceElementChainLevel> chainLevels;
    private Vector2 scrollTargetOriPos;
    private Action<UIEnhanceElementChainLevel> onClickHandler;
    #endregion

    #region Properties
    public E_UnitAttributeType SelectedAttribute { get; private set; }
    #endregion

    #region Public Methods
    public void Initialize(UIEnhanceElementChainAndEnhanceSettingProvider settingProvider, Action<UIEnhanceElementChainLevel> onClick)
    {
        if (isInitialized)
            return;

        scrollTargetOriPos = scrollTarget.anchoredPosition;
        chainLevelSource.gameObject.SetActive(false);
        chainLevels = new List<UIEnhanceElementChainLevel>(5);
        slotScrollAdapter.onScroll += OnSlotScroll;
        SettingProvider = settingProvider;
        onClickHandler = onClick;

        CreateChainLevels();
        SetupNonChangeableIndicator();
        UpdateUI();

        isInitialized = true;
    }
    #endregion

    #region Unity Methods

    #endregion

    #region Private Methods
    private void CreateChainLevels()
    {
        uint minLevel = DBAttribute.GetAttributeChainMinLevel();
        uint maxLevel = DBAttribute.GetAttributeChainMaxLevel();

        for (uint level = minLevel; level <= maxLevel; level++)
        {
            var data = DBAttribute.GetAttributeChainByLevel(level);
            uint hierarchyIndex = level - minLevel;
            var newChain = AddChainLevel(hierarchyIndex);
            float slotItemSize = slotScrollAdapter.GetItemSize();

            // 속성연계 위치 세팅.

            // 우선 scrollAdapter 의 Content 기준 왼쪽 위치로 설정 
            newChain.transform.position = new Vector3(
                slotScrollAdapter.GetContentLeftXPos()
                , newChain.transform.position.y
                , newChain.transform.position.z);

            // 그 다음에 현재 레벨에 맞게끔 이동시켜줌 . 
            newChain.RectTransform.anchoredPosition += new Vector2(
                (slotItemSize * 0.5f) +
                (DBAttribute.GetAttributeChainRequestLevel(level) - 1) * slotScrollAdapter.GetSlotGroupGapDistance(), 0);

            if (newChain != null)
                RefreshChainLevel(newChain, level);

            //if (data != null)
            //{
            //    DBAttribute.GetAttributeChainRequestLevel(level);
            //}
        }
    }

    private void SetupNonChangeableIndicator ()
    {
        foreach (var indicator in elementIndicators)
        {
            uint level = DBAttribute.GetMaxLevelByType(indicator.type);
            float width = GetLevelIndicatorWidth((int)level);

            // 실선 스프라이트바뀌면 이 -30 수치가 바뀔수있음 . 
            Vector2 sizeDelta = new Vector2(width - 30, indicator.imgLevelIndicator02.rectTransform.sizeDelta.y);
            indicator.imgLevelIndicator02.rectTransform.sizeDelta = sizeDelta;
        }
    }

    private UIEnhanceElementChainLevel AddChainLevel(uint hierarchyIndex)
    {
        var result = Instantiate(chainLevelSource, scrollTarget, true);

        if (result == null)
            return null;

        result.transform.SetSiblingIndex((int)hierarchyIndex);
        result.gameObject.SetActive(true);
        chainLevels.Add(result);
        return result;
    }

    private void RefreshChainLevel(UIEnhanceElementChainLevel target, uint level)
    {
        var data = DBAttribute.GetAttributeChainByLevel(level);

        if (data != null)
        {
            var lvSprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
            //      if (lvSprite == null)
            //        ZLog.LogError(ZLogChannel.UI, "Could not get Sprite (ID : " + data.IconID);

            target.Set(onClickHandler, level, lvSprite, SettingProvider.GetChainEffectImgColor(level));
        }
    }

    private void OnSlotScroll(double normPos)
    {
        scrollTarget.anchoredPosition = new Vector2(scrollTargetOriPos.x - (float)slotScrollAdapter.GetCurrentScrollOffset(normPos), scrollTargetOriPos.y);
    }

    private float GetLevelIndicatorWidth(int level)
    {
        float width = 0;

        if (level >= 1)
        {
            width = levelOneIndicatorWid;
        }

        width += (levelIndicatorincrementalWid * (level - 1));
        return width;
    }
    #endregion

    #region Public Methods
    public void Select(E_UnitAttributeType type)
    {
        if (SelectedAttribute.Equals(type))
            SelectedAttribute = E_UnitAttributeType.None;
        else SelectedAttribute = type;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < elementIndicators.Count; i++)
        {
            bool isSelected = elementIndicators[i].type == SelectedAttribute;

            uint level = Me.CurCharData.GetAttributeLevelByType(elementIndicators[i].type);
            float width = GetLevelIndicatorWidth((int)level);
            float height = isSelected ? this.levelIndicatorSelectedHeight : levelIndicatorNotSelectedHeight;

            Vector2 indicatorSd = new Vector2(width, height);
            elementIndicators[i].imgLevelIndicator.rectTransform.sizeDelta = indicatorSd;
        }

        foreach (var chain in chainLevels)
        {
            RefreshChainLevel(chain, chain.ChainLevel);
        }
    }
    #endregion
}
