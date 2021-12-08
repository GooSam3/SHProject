using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uTools;

abstract public class CUGUIWidgetTweenBase : CUGUIAlarmLinkedBase
{
    [SerializeField]
    private bool ImmediatleEvent = false;

    private List<uTweener> m_listTweenerInstance = new List<uTweener>();
    private int mFinishCount = 0;
    private bool mStart = false;
    private CCommandUIWidgetBase mCommand = null;

    //------------------------------------------------
    protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitialize(_UIFrameParent);
        mCommand = OnUIWidgetCommandExtract();
        GetComponents(m_listTweenerInstance);
        PrivUITweenEnd();   
    }

    protected override void OnUIWidgetShowHide(bool _Show)
    {
        if (_Show)
        {
            PrivUITweenEnd();
        }
    }

    public UnityAction ExportHandlerTween()
    {
        return HandleUITweenFinish;
    }

    //----------------------------------------------
    protected virtual void ProtUITweenStart()
    {
        if (m_listTweenerInstance.Count > 0)
        {
            if (mStart == false)
            {
                mStart = true;
                PrivUITweenStart();
            }
        }
        else
        {
            HandleUITweenFinish();
        }
    }

    protected void ProtUIWidgetCommandExcute()
    {
        if (mCommand != null)
        {
            mCommand.DoCommandUIWidgetExcute(mUIFrameParent, this);
        }
        OnUIWidgetCommandExcute();
    }

    //-----------------------------------------------
    private void PrivUITweenStart()
    {
        for (int i = 0; i < m_listTweenerInstance.Count; i++)
        {
            m_listTweenerInstance[i].enabled = true;
            m_listTweenerInstance[i].PlayForward();
        }

        if (ImmediatleEvent)
		{
            ProtUIWidgetCommandExcute();
        }
    }

    private void PrivUITweenEnd()
    {
        for (int i = 0; i < m_listTweenerInstance.Count; i++)
        {
            m_listTweenerInstance[i].ResetToBeginning();
            m_listTweenerInstance[i].enabled = false;
        }
        mFinishCount = 0;
        mStart = false;
    }

    //-----------------------------------------------
    private void HandleUITweenFinish()
    {
        mFinishCount++;
        if (mFinishCount >= m_listTweenerInstance.Count)
        {
            PrivUITweenEnd();

            if (ImmediatleEvent == false)
            {
                ProtUIWidgetCommandExcute();
            }
        }
    }

    //-----------------------------------------------
    protected virtual CCommandUIWidgetBase OnUIWidgetCommandExtract() { return null; }
    protected virtual void OnUIWidgetCommandExcute() { }
}
