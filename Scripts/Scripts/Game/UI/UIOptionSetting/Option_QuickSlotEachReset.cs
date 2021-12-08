public class Option_QuickSlotEachReset : OptionSetting
{
    public void ClickReset()
    {
        if(UIManager.Instance.Find(out UIFrameOption _option))
        {
            _option.OpenQuickSlotResetMode();
        }
    }
}
