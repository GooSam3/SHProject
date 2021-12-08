using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class CUGUIIconBase : CUGUIAlarmLinkedBase
{
    [SerializeField] private ZImageClickable     IconImage;
    [SerializeField] private ZImage             IconBoard;
    [SerializeField] private GameObject          IconSelect;
    [SerializeField] private Text               IconCount;
      
    private uint mIconCount = 0;         public uint pIconCount { get { return mIconCount; } }
    private uint mIconID = 0;            public uint pIconID { get { return mIconID; } }
    private bool mIconSelected = false;   public bool pIconSelect { get { return mIconSelected; } }
   
    //--------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
    }

	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
        PrivUIIconArrangeChildRaycastTarget();

        if (IconImage)
		{
            IconImage.gameObject.SetActive(true);
        }
        if (IconBoard)
        {
            IconBoard.gameObject.SetActive(true);
        }
        if (IconSelect)
        {
            IconSelect.gameObject.SetActive(false);
        }
        if (IconCount)
        {
            IconCount.gameObject.SetActive(false);
        }
    }
    //-------------------------------------------------------------------
    public void SetIconInputEnable(bool _enable)
	{
        PrivUIIconInputEnable(_enable);
    }

    //--------------------------------------------------------------------
    private void HandleUIIconPointDown(Vector2 _PointPosition)
    {
        OnUIIconPointDown(_PointPosition);
    }

    private void HandleUIIconPointUp(Vector2 _PointPosition)
    {
        OnUIIconPointUp(_PointPosition);
    }

    private void HandleUIIconDrag(Vector2 _pointPosition)
	{
        OnUIIconPointDrag(_pointPosition); 
    }

    //--------------------------------------------------------------------
    protected void ProtUIIconSettingSprite(Sprite _iconSprite, Sprite _iconBoardSprite, uint _iconID)
	{
        if (IconImage != null && _iconSprite != null)
		{
            IconImage.gameObject.SetActive(true);
            IconImage.sprite = _iconSprite;
		}

        if (IconBoard != null && _iconBoardSprite != null)
        {
            IconBoard.gameObject.SetActive(true);
            IconBoard.sprite = _iconBoardSprite;
        }

        mIconID = _iconID;

        OnUIIconSettingSprite();
    }

    protected void ProtUIIconSettingCount(uint _countMin, uint _countMax = 0)
    {
        if (IconCount != null)
        {
            if (_countMax == 0)
			{
                _countMax = _countMin;
            }

            if (_countMin == _countMax)
			{
                if (_countMin == 1)
				{
                    IconCount.gameObject.SetActive(false);
                }
                else
				{
                    IconCount.gameObject.SetActive(true);
                    IconCount.text = _countMin.ToString();
                }
			}
            else
			{
                IconCount.gameObject.SetActive(true);
                IconCount.text = string.Format("{0}-{1} ", _countMin.ToString(), _countMax.ToString());
            }
        }

        OnUIIconSettingCount(_countMin, _countMax);
    }

    protected void ProtUIIconSelect(bool _select)
	{
        if (IconSelect)
		{
            IconSelect.gameObject.SetActive(_select);           
		}

        mIconSelected = _select;
	}

    //--------------------------------------------------------------------
    private void PrivUIIconArrangeChildRaycastTarget()
    {
        PrivUIIconInputEnable(true);
        if (IconImage != null)
        {
            IconImage.SetImageInputEvent(HandleUIIconPointDown, HandleUIIconPointUp, HandleUIIconDrag);
        }
    }

    private void PrivUIIconInputEnable(bool _enable)
	{
        for (int i = 0; i < m_listGrpahic.Count; i++)
        {
            if (m_listGrpahic[i] != IconImage)
            {
                m_listGrpahic[i].raycastTarget = false;
            }
            else
            {
                m_listGrpahic[i].raycastTarget = _enable;
            }
        }
    }

    //-----------------------------------------------------------
    protected virtual void OnUIIconPointDown(Vector2 _PointPosition) { }
    protected virtual void OnUIIconPointUp(Vector2 _PointPosition) { }
    protected virtual void OnUIIconPointDrag(Vector2 _pointPosition) { }
    protected virtual void OnUIIconSettingSprite() { }
    protected virtual void OnUIIconSettingCount(uint _countMin, uint _countMax) { }
}
