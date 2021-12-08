using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UGUI 서포트용 컴포넌트 
[RequireComponent(typeof(RectTransform))]
abstract public class CUGUIWidgetBase : CMonoBase
{
    protected CUIFrameBase      mUIFrameParent = null;
    protected RectTransform     mRectTransform;
    private bool               mInitialize = false;
    private bool               mFocus = false;    public bool pFocus { get { return mFocus; } } 
    protected List<Graphic>     m_listGrpahic = new List<Graphic>();
    private ZText              mText = null;
    //--------------------------------------------------------------
    private void Reset()
    {
        OnUIWidgetAddComponentInEditor();
    }

    //--------------------------------------------------------------
    public void DoUIWidgetInitialize(CUIFrameBase _UIFrameParent)
    {
        if (mInitialize) return;
 
        mRectTransform = GetComponent<RectTransform>();
        mUIFrameParent = _UIFrameParent;
        GetComponentsInChildren(true, m_listGrpahic);
        mText = GetComponentInChildren<ZText>();

        OnUIWidgetInitialize(_UIFrameParent);   
    }

    public void DoUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
	{
        if (mInitialize) return;
        mInitialize = true;

        OnUIWidgetInitializePost(_UIFrameParent);
	}

    public void SetUIWidgetText(string _Text)
	{
        mText.text = _Text;
        OnUIWidgetSetText(_Text);
    }

    public void DoUIWidgetChangeSprite(Sprite _ChangeSprite)
	{
        OnUIWidgetChangeSprite(_ChangeSprite);
	}

    public void DoUIWidgetFocus(bool _On)
	{
        mFocus = _On;
        OnUIWidgetFocus(_On);
    }

    public void DoUIWidgetShowHide(bool _Show)
    {
        if (gameObject.activeSelf != _Show)
		{
            OnUIWidgetShowHide(_Show);
        }
    }

    public bool DoUIWidgetShowHideSwitch()
	{
        if (gameObject.activeSelf == true)
        {
            DoUIWidgetShowHide(false);
        }
        else
		{
            DoUIWidgetShowHide(true);
        }

        return gameObject.activeSelf;
    }

    //-------------------------------------------------------------
    public void ImportWidgetFrameShowHide(bool _show)
	{
        OnUIWidgetFrameShowHide(_show);
    }

    public void ImportWidgetRemove()
	{
        OnUIWidgetRemove();
    }

    public void ImportWidgetDuplication()
	{
        OnUIWidgetDuplication();
    }


    //--------------------------------------------------------------
    protected void SetUIPositionX(float _X)
    {
        mRectTransform.transform.position = new Vector3(_X, mRectTransform.transform.position.y, 0);
    }

    protected void SetUIPositionY(float _Y)
    {
        mRectTransform.transform.position = new Vector3(mRectTransform.transform.position.x, _Y, 0);
    }

    protected void SetUIPosition(float _X, float _Y)
    {
        mRectTransform.transform.position = new Vector3(_X, _Y, 0);
    }

    protected void SetUIPositionAddX(float _X)
    {
        mRectTransform.transform.position = new Vector3(mRectTransform.transform.position.x + _X, mRectTransform.transform.position.y, 0);
    }

    protected void SetUIPositionAddY(float _Y)
    {
        mRectTransform.transform.position = new Vector3(mRectTransform.transform.position.x, mRectTransform.transform.position.y + _Y, 0);
    }    

    protected void SetUIAllChildGrapicColor(Color _Color)
	{
        for (int i = 0; i < m_listGrpahic.Count; i++)
		{
            m_listGrpahic[i].color = _Color;
		}
	}

    protected float GetUIWidth()
    {
        return mRectTransform.rect.width;
    }

    protected float GetUIHeight()
    {
        return mRectTransform.rect.height;
    }

    //-------------------------------------------------------------
    protected virtual void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent) { }
    protected virtual void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent) { }
    protected virtual void OnUIWidgetShowHide(bool _Show) { SetMonoActive(_Show); }
    protected virtual void OnUIWidgetFrameShowHide(bool _show) { }
    protected virtual void OnUIWidgetFocus(bool _On) { }
    protected virtual void OnUIWidgetRemove() { }
    protected virtual void OnUIWidgetAddComponentInEditor() { }
    protected virtual void OnUIWidgetSetText(string _Text) { }
    protected virtual void OnUIWidgetChangeSprite(Sprite _ChangeSprite) { }
    protected virtual void OnUIWidgetDuplication() { }
}
//--------------------------------------------------------------
public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottonCenter,
    BottomRight,
    BottomStretch,

    VertStretchLeft,
    VertStretchRight,
    VertStretchCenter,

    HorStretchTop,
    HorStretchMiddle,
    HorStretchBottom,

    StretchAll
}

public enum PivotPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
}

public static class RectTransformExtensions
{
    public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
    {
        source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

        switch (allign)
        {
            case (AnchorPresets.TopLeft):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.TopCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.TopRight):
                {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.MiddleLeft):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleRight):
                {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

            case (AnchorPresets.BottomLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
            case (AnchorPresets.BottonCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
            case (AnchorPresets.BottomRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.HorStretchTop):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case (AnchorPresets.HorStretchMiddle):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
            case (AnchorPresets.HorStretchBottom):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.VertStretchLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.VertStretchCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.VertStretchRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.StretchAll):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);                
                    break;
                }
        }
    }

    public static void SetPivot(this RectTransform source, PivotPresets preset)
    {

        switch (preset)
        {
            case (PivotPresets.TopLeft):
                {
                    source.pivot = new Vector2(0, 1);
                    break;
                }
            case (PivotPresets.TopCenter):
                {
                    source.pivot = new Vector2(0.5f, 1);
                    break;
                }
            case (PivotPresets.TopRight):
                {
                    source.pivot = new Vector2(1, 1);
                    break;
                }

            case (PivotPresets.MiddleLeft):
                {
                    source.pivot = new Vector2(0, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleCenter):
                {
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleRight):
                {
                    source.pivot = new Vector2(1, 0.5f);
                    break;
                }

            case (PivotPresets.BottomLeft):
                {
                    source.pivot = new Vector2(0, 0);
                    break;
                }
            case (PivotPresets.BottomCenter):
                {
                    source.pivot = new Vector2(0.5f, 0);
                    break;
                }
            case (PivotPresets.BottomRight):
                {
                    source.pivot = new Vector2(1, 0);
                    break;
                }
        }
    }
}