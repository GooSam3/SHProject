abstract public class CCommandUIWidgetBase
{
 
    //--------------------------------------------------------
    public void DoCommandUIWidgetExcute(CUIFrameBase _OnwerFrame, CUGUIWidgetBase _OwnerWidget)
    {
        OnCommandUIWidget(_OnwerFrame, _OwnerWidget);
    }
    
    //--------------------------------------------------------  
    protected virtual void OnCommandUIWidget(CUIFrameBase _OnwerFrame, CUGUIWidgetBase _OwnerWidget) { }
}
