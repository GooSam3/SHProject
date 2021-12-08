using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Canvas 단위 출력객체 
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
abstract public class CUIFrameBase : CMonoBase
{
    protected struct SUIFrameInfo
    {
        public string ID;
        public int LocalizingKey;
        public int ThemaType;
        public SUIFrameInfo(string _ID) { ID = _ID; LocalizingKey = 0; ThemaType = 0; }
    }

    [SerializeField] private   CManagerUIFrameFocusBase.E_UIFrameFocusAction FocusAction = CManagerUIFrameFocusBase.E_UIFrameFocusAction.Panel;
    [SerializeField] protected bool AutoShowHide = true;
    [Header("풀스크린일 경우 카메라 culling 변경 여부")]
    [SerializeField] protected bool mIsCullingMaskByFullScreen = true;

    public bool IsCullingMaskByFullScreen { get { return mIsCullingMaskByFullScreen; } }

    public CManagerUIFrameFocusBase.E_UIFrameFocusAction FocusType { get { return FocusAction; } }    

    protected   List<CUGUIWidgetBase> m_listWidgetInstance = new List<CUGUIWidgetBase>();
    private     Stack<CUIFrameBase> m_stackDuplicationFrame = new Stack<CUIFrameBase>();
    private     SUIFrameInfo mUIFrameInfo;                
    public string ID  { get { return mUIFrameInfo.ID; } } 
    public int LocalizingKey { get { return mUIFrameInfo.LocalizingKey; } }
    public int ThemaType { get { return mUIFrameInfo.ThemaType; } } 
                                                   
    public bool InputEnable { get { return mRayCaster.enabled; } }
    private bool mShow = false;         
    public bool Show  { get { return mShow; } }
    private bool mIsAddressable = false;       
    public bool IsAddressable { get { return mIsAddressable; } }
    private bool mIsDuplicated = false;        
    public bool IsDuplication { get { return mIsDuplicated; } }
    public  virtual bool IsDuplicatable { get { return false; } } // true일 경우 중복으로 Show 했을 경우 복제가 가능하다 
    private int mLayerOrder = 0;                     
    public int LayerOrder { get { return mCanvas.sortingOrder; } }
    public int LayerID { get { return mCanvas.sortingLayerID; } }
    public virtual bool IsBackable { get { return false; } }

    protected Canvas            mCanvas = null;
    protected RectTransform      mRectTransform = null;
    protected Vector2           mUIFrameSize = Vector2.zero;
    private   GraphicRaycaster   mRayCaster = null;
    private   bool              mFirstShow = false;
    private   int               mLayerOrigin = 0;
    
    //-------------------------------------------------------------------------
    public void ImportShow(int _LayerOrder)
    {
        mLayerOrder = _LayerOrder;
        ImportShow();
    }

    public void ImportRefreshOrder(int _LayerOrder)
	{
        mLayerOrder = _LayerOrder;
        mCanvas.sortingOrder = _LayerOrder;
        OnRefreshOrder(mLayerOrder);
    }

    public void ImportShowTopMost(int _layerOrder, bool _topMost)
	{
        if (_topMost)
		{
            mLayerOrigin = mLayerOrder;
            ImportShow(_layerOrder);
        }
        else
		{
            ImportShow(mLayerOrigin);
		}
	}

    public void ImportShow()
    {
        if (gameObject.activeSelf == false || mFirstShow == false)
        {
            SetMonoActive(true);
            mCanvas.overrideSorting = true;
            //mCanvas.sortingLayerName = LayerMask.LayerToName(gameObject.layer);
            mCanvas.sortingOrder = mLayerOrder;

            mShow = true;
            mFirstShow = true;
            ChildWidgetShowHide(true);
            OnShow(mCanvas.sortingOrder);
        }
        else
        {
            ImportRefreshOrder(mLayerOrder);
        }
    }

    public void ImportShowAutoShowHide(bool _show)
	{
        if (AutoShowHide)
		{
            if (_show)
			{
                ImportShow();
            }
            else
			{
                ImportHide();
			}
        }
	}

    public void ImportHide()
    {
        if (gameObject.activeSelf == true)
		{ 
            mShow = false;
            SetMonoActive(false);
            ChildWidgetShowHide(false);
            OnHide();
        }     
    }

    public void ImportHideDuplication()
    {
        if (m_stackDuplicationFrame.Count == 0) return;
        CUIFrameBase DuplicationUIFrame = m_stackDuplicationFrame.Pop();
        Destroy(DuplicationUIFrame.gameObject);
    }

    public void ImportRemove()
	{
        RemoveAllDuplicationFrame();
        ChildWidgetRemove();
        OnRemove();
    }

    public void ImportInputEnable(bool _Enable)
    {
        mRayCaster.enabled = _Enable;
        OnInputEnable(_Enable);
    }

    public void ImportInitialize(bool _IsAddressable)
    {
        mCanvas = GetComponent<Canvas>();
       
        mRayCaster = GetComponent<GraphicRaycaster>();
        mRectTransform = transform as RectTransform;
        mUIFrameSize.x = mRectTransform.rect.width;
        mUIFrameSize.y = mRectTransform.rect.height;
        mIsAddressable = _IsAddressable;
        mUIFrameInfo = Infomation();
        CollectWidgetInstance();
        OnInitialize();
    }

    public CUIFrameBase ImportDuplication(int _topLayerOrder)
	{   
        CUIFrameBase CloneInstance = Instantiate(gameObject, transform.parent).GetComponent<CUIFrameBase>();
        CloneInstance.ImportDuplicated(this);
        CloneInstance.ImportInitialize(false);
     
        int LayerOrder = m_stackDuplicationFrame.Count + _topLayerOrder + 1;
        CloneInstance.ImportShow(mLayerOrder + LayerOrder);

        m_stackDuplicationFrame.Push(CloneInstance);
        OnDuplication(CloneInstance);
        return CloneInstance;
    }

    public void ImportDuplicated(CUIFrameBase _OriginUIFrame)
	{
        mIsDuplicated = true;
        DuplicationWidget();
        OnDuplicated(_OriginUIFrame);
	}

    public void ImportCommand(int _CommandID, int _Group, int _Argument, CUGUIWidgetBase _CommandOwner)
    {
        OnCommand(_CommandID, _Group, _Argument, _CommandOwner);
    }

	//----------------------------------------------------------------------
	public void DoRefreshAll()
	{
        OnRefresh();
    }

    public void DoInitilizeMenual()
	{
        CollectWidgetInstance();
    }

    //------------------------------------------------------------------------
    public bool HasDuplication()
	{
        bool HasFrame = false;

        if (m_stackDuplicationFrame.Count > 0)
		{
            HasFrame = true;
		}

        return HasFrame;
	}

    //-------------------------------------------------------------------------
    protected void AddUIWidget(CUGUIWidgetBase _addWidget)
	{
        m_listWidgetInstance.Add(_addWidget);
      
        _addWidget.DoUIWidgetInitialize(this);
        _addWidget.DoUIWidgetInitializePost(this);

        OnAddWidget(_addWidget);
    }

    protected void AddUIWidget(GameObject _gameObject)
	{
        CUGUIWidgetBase [] arrWidget = _gameObject.GetComponentsInChildren<CUGUIWidgetBase>();
        for (int i = 0; i < arrWidget.Length; i++)
		{
            AddUIWidget(arrWidget[i]);
		}
	}

    protected void DeleteUIWidget(CUGUIWidgetBase _deleteWidget)
	{
        _deleteWidget.ImportWidgetRemove();
        m_listWidgetInstance.Remove(_deleteWidget);
        OnDeleteWidget(_deleteWidget);
    }

    //-------------------------------------------------------------------------
    private void CollectWidgetInstance()
    {
        m_listWidgetInstance.Clear();
        GetComponentsInChildren(true, m_listWidgetInstance);
        for (int i = 0; i < m_listWidgetInstance.Count; i++)
        {
            m_listWidgetInstance[i].DoUIWidgetInitialize(this);
        }

        for (int i = 0; i < m_listWidgetInstance.Count; i++)
        {
            m_listWidgetInstance[i].DoUIWidgetInitializePost(this);
        }
    }

    private void ChildWidgetShowHide(bool _Show)
    {
        for (int i = 0; i < m_listWidgetInstance.Count; i++)
        {
            m_listWidgetInstance[i].ImportWidgetFrameShowHide(_Show);
        }
    }

    private void ChildWidgetRemove()
	{
        for (int i = 0; i < m_listWidgetInstance.Count; i++)
        {
            m_listWidgetInstance[i].ImportWidgetRemove();
        }
    }

    private void RemoveAllDuplicationFrame()
	{
        Stack<CUIFrameBase>.Enumerator it = m_stackDuplicationFrame.GetEnumerator();
        while(it.MoveNext())
		{
            Destroy(it.Current);
		}
        m_stackDuplicationFrame.Clear();
    }

    private void DuplicationWidget()
	{
        m_listWidgetInstance.Clear();
        GetComponentsInChildren(true, m_listWidgetInstance);
        for (int i = 0; i < m_listWidgetInstance.Count; i++)
		{
            m_listWidgetInstance[i].ImportWidgetDuplication();
		}
	}

    //-------------------------------------------------------------------------
    protected virtual SUIFrameInfo Infomation() { return new SUIFrameInfo(this.GetType().ToString()); }
    protected virtual void OnShow(int _LayerOrder) { }
    protected virtual void OnHide() { }
    protected virtual void OnRemove() { }
    protected virtual void OnRefreshOrder(int _LayerOrder) { }
    protected virtual void OnRefresh() { }
    protected virtual void OnInputEnable(bool _Enable) { }
    protected virtual void OnCommand(int _CommandID, int _Group, int _Argument, CUGUIWidgetBase _CommandOwner) { }   
    protected virtual void OnInitialize() { }
    protected virtual void OnAddWidget(CUGUIWidgetBase _addWidget) { }
    protected virtual void OnDeleteWidget(CUGUIWidgetBase _addWidget) { }
    protected virtual void OnDuplication(CUIFrameBase _DuplicateUIFrame) { }
    protected virtual void OnDuplicated(CUIFrameBase _OriginUIFrame) { }
}
