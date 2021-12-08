using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;
using static UIEnhanceElement;

public class UIEnhanceElement_IntegratedElementTxt : MonoBehaviour
{
    public class SingleText
    {
        public UIEnhanceElementTitleValuePair ui;
        public Color titleColor;
        public Color contentColor;
    }

    public class UIDataPair
    {
        public List<SingleText> txts;
        public List<AbilityActionTitleValuePair> data;
    }

    #region Serialized Field
    [SerializeField] private UIEnhanceElementInteElementScrollAdapter ScrollAdapter;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private UIEnhanceElementTitleValuePair sourceObj;
    #endregion

    #region Properties 
    public RectTransform RectTransform { get { return rectTransform; } }
    public Rect SourceObjRect { get { return sourceObj.RectTransform.rect; } }
    public bool IsDataSet { get; private set; }
    #endregion

    #region System Variables
    private UIEnhanceElementSettingProvider SettingProvider;
    private UIDataPair txts;
    private E_UnitAttributeType Type;
    private bool useHighlightFeature;

    private bool isInitialized;
    #endregion

    #region Public Methods
    public void Initialize(UIEnhanceElementSettingProvider settingProvider, int dataCount, bool useHighlight = true)
    {
        if (isInitialized)
            return;

        txts = new UIDataPair();
        txts.txts = new List<SingleText>(dataCount);
        SettingProvider = settingProvider;
        useHighlightFeature = useHighlight;

 //      Generate(dataCount);

        isInitialized = true;
    }

    /// <summary>
    /// ++ 출력 방식 수정 
    /// </summary>
    public void Initialize_Ex(UIEnhanceElementSettingProvider settingProvider, bool useHighlight = true)
    {
        if (isInitialized)
            return;

        SettingProvider = settingProvider;
        useHighlightFeature = useHighlight;

        ScrollAdapter.Initialize(
            useHighlight
            , SettingProvider.chainAndEnhanceProvider.enhance.txtTitleColorOnActive
            , SettingProvider.chainAndEnhanceProvider.enhance.txtContentColorOnActive
            , SettingProvider.chainAndEnhanceProvider.enhanceTitleContentColorOnInactive
            , sourceObj);
        isInitialized = true;
    }

    public void SetData(E_UnitAttributeType type, List<AbilityActionTitleValuePair> data)
    {
        IsDataSet = true; 

        Type = type;
        txts.data = data;
    }

    /// <summary>
    ///  ++ 출력 방식 수정 
    /// </summary>
    public void SetData(E_UnitAttributeType type, List<ScrollEnhanceElementIntegratedData> data)
    {
        IsDataSet = true;

        Type = type;
        if (type == E_UnitAttributeType.None)
        {
            ScrollAdapter.RefreshData(type, Color.black, new List<ScrollEnhanceElementIntegratedData>());
        }
        else
        {
            ScrollAdapter.RefreshData(type, SettingProvider.FindTypeGroupProperty(type).iconLineAndEnhanceColorOnActive, data);
        }
    }

    public void NotifyDataRefreshed()
    {
        ScrollAdapter.NotifyDataChanged();
    }

    public void SnapToNextLevel(int jumpFrameCount = 0, Action onFinished = null)
    {
        ScrollAdapter.SnapToMyNextLevel(jumpFrameCount, onFinished);
    }

    public void SetNormalizePos(float pos)
    {
        ScrollAdapter.SetNormalizedPosition(pos);
    }

    //public void UpdateUI()
    //{
    //    if (txts.data == null)
    //        return;

    //    if (txts.data.Count > txts.txts.Count)
    //    {
    //        Generate(txts.data.Count - txts.txts.Count);
    //    }

    //    //if (txts.Count != data.Count)
    //    //{
    //    //    ZLog.LogError(ZLogChannel.UI, "최초에 데이터 개수만큼 오브젝트를 생성해놓아야 합니다. 테이블 데이터이기 때문에 중간에 달라질수없음 .");
    //    //    return;
    //    //}

    //    Color titleActiveColor = SettingProvider.chainAndEnhanceProvider.enhance.txtTitleColorOnActive;
    //    Color contentActiveColor = SettingProvider.chainAndEnhanceProvider.enhance.txtContentColorOnActive;
    //    Color inactiveColor = SettingProvider.chainAndEnhanceProvider.enhanceTitleContentColorOnInactive;
    //    Color elementColor = SettingProvider.FindTypeGroupProperty(Type).iconLineAndEnhanceColorOnActive;
    //    uint curLevel = Me.CurCharData.GetAttributeLevelByType(Type);

    //    for (int i = 0; i < txts.txts.Count; i++)
    //    {
    //        var ui = txts.txts[i].ui;

    //        if (i < txts.data.Count)
    //        {
    //            // 만약에 내 레벨보다 높은데 하이라이트도 별도로 표시안해주는데 
    //            // 꺼주고싶다하면 여기서 isObtained 랑 isMyNextLevel 체크해서 꺼주거나 하면댐 . ㅇㅇ easy 
    //            // 걍 SetActive(false); 해줌 댐 . 
    //            // 그리고 스크롤 자동이동 그거는 그냥 강제로 0 으로 떄려박으면 되겠지 ? 
    //            // ScrollSnapToHighlightedTxt 한데서 걍 스크롤 쭊 해주면 댈듯 . 전체화면은 어디서 하까 ?  
    //            // 근데 거기는 그냥 맨위로 올림대는거아닌가 ? 
    //            var data = txts.data[i];
    //            bool isObtained = Me.CurCharData.IsThisAttributeObtained_ByID(data.sourceTid);
    //            bool activatedColor = isObtained;
    //            bool isMyNextLevel = false;
    //            bool highlight = false;

    //            if (DBAttribute.GetAttributeByID(data.sourceTid, out var attributeData))
    //            {
    //                if (curLevel + 1 == attributeData.AttributeLevel)
    //                {
    //                    isMyNextLevel = true;
    //                }
    //            }

    //            if (isMyNextLevel && useHighlightFeature)
    //            {
    //                highlight = true;
    //                activatedColor = true;
    //            }

    //            ui.SetText(data.title, "+" + data.value);
    //            ui.SetColor(
    //                activatedColor ? titleActiveColor : inactiveColor
    //                , activatedColor ? contentActiveColor : inactiveColor);

    //            if (highlight)
    //            {
    //                ui.ActivateHighlight(elementColor);
    //            }
    //            else
    //            {
    //                ui.DeactivateHighlight();
    //            }

    //            ui.gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            ui.gameObject.SetActive(false);
    //        }
    //    }
    //}
    #endregion

    #region Private Methods

    //private void Generate(int dataCount)
    //{
    //    int cnt = 0;
    //    while (cnt < dataCount)
    //    {
    //        var obj = new SingleText();
    //        obj.ui = Instantiate(sourceObj, transform);
    //        obj.ui.gameObject.SetActive(false);
    //        txts.txts.Add(obj);
    //        cnt++;
    //    }
    //}
    #endregion
}
