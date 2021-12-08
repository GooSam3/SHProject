using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;
using ZNet.Data;

public class UIScrollEnhanceElementSlot : CellViewsHolder
{
    private UIEnhanceElementSlot targetSlot;
    public UIEnhanceElementSlot Target => targetSlot;

    private ScrollEnhanceElementData scrollData;

    private Action<ScrollEnhanceElementData, UIEnhanceElementSlot> onClickSlot;

    public void UpdateSlot(UIEnhanceElementSettingProvider settingProvider, ScrollEnhanceElementData data, Action<ScrollEnhanceElementData, UIEnhanceElementSlot> _onClickSlot)
    {
        scrollData = data;
        onClickSlot = _onClickSlot;

        bool isObtained = Me.CurCharData.IsThisAttributeObtained_ByID(data.TableData.AttributeID);
        bool isNextTarget = isObtained == false && data.IsObtainedOrNextAvailableLevel;

        targetSlot.Set(
            data.TableData.AttributeID
            , settingProvider
            , data.TableData.AttributeType
            , ZManagerUIPreset.Instance.GetSprite(data.TableData.IconID)
            , data.BgColor
            , data.LineAndEnhanceColor
            , isObtained
            , isNextTarget);
    }

    public override void CollectViews()
    {
        base.CollectViews();

        targetSlot = root.GetComponent<UIEnhanceElementSlot>();
        targetSlot.Initialize(() => onClickSlot(scrollData, targetSlot));

        var targetShaderUpdaters = root.GetComponentsInChildren<UIShaderClipingUpdater>(true);

        if (targetShaderUpdaters != null)
        {
            for (int i = 0; i < targetShaderUpdaters.Length; i++)
            {
                if (targetShaderUpdaters[i] != null)
                {
                    targetShaderUpdaters[i].DoUIWidgetInitialize(UIManager.Instance.Find<UIEnhanceElement>());
                }
            }
        }
    }
}
