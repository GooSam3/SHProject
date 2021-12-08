using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Renderpipe 라인에서 UIOver 레이어 추가 및 셋팅
// 메인 카메라에 UI카메라 스택으로 연결 (UI카메라는 오버레이)
// UIOver 레이어는 가장 늦게 그려지게 설정 

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class CManagerUIFrameFocusBase : CManagerUIFrameBase
{
    public enum E_UIFrameFocusAction
    {
        Panel = 0,                   // OrderLayer의 순서대로 출력된다. 최초 비활성 상태가 된다.
        FullScreen = 1,              // 활성화 될 경우 아래 Order는 모두 비활성화 된다. 메인 카메라가 비활성화 된다.

        ScreenBlock = 10,             // 단독으로 별도 사용중 (기존 구조)
        SystemPopup = 11,             // 최상위에 노출되는 팝업 (Front Root에 별도 관리)
        System = 12
    }

    public enum E_UICanvas
    {
        Back = 0,
        Front = 1
    }

    public enum E_UICamera
    {
        Back = 0,
        Model3D = 1,
        Front = 2
    }

    public enum E_UICameraStack
    {
        None = 0,

        Gacha,
    }

    public enum E_CameraStackName
    {
        UICamera,
        UICameraFront,
        PPVolume,
        UICamera3D,
        SubSceneCamera
    }

    public class SystemFrameObject
    {
        public CUIFrameBase Frame;
        public int Key;
        public GameObject Obj;
        public GameObject ParentObj;
    }

    private int mGlobalIndicatorRef = 0;
    private List<CUIFrameBase> m_listTopMost = new List<CUIFrameBase>();

    [SerializeField] private const int SortOrderGap = 5;

    [SerializeField] protected List<CUIFrameBase> ContentFrameList = new List<CUIFrameBase>();
    [SerializeField] protected List<CUIFrameBase> SystemFrameList = new List<CUIFrameBase>();
    [SerializeField] protected List<SystemFrameObject> SystemObjectList = new List<SystemFrameObject>();

    [SerializeField] private int[] RootOrder = new int[Enum.GetNames(typeof(E_UICanvas)).Length];

    [SerializeField] private Canvas[] RootCanvas = new Canvas[Enum.GetNames(typeof(E_UICanvas)).Length];
    [SerializeField] private Camera[] UICamera = new Camera[Enum.GetNames(typeof(E_UICamera)).Length];

    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();

        RootCanvas[Convert.ToInt32(E_UICanvas.Back)].overrideSorting = true;
        RootCanvas[Convert.ToInt32(E_UICanvas.Back)].renderMode = RenderMode.ScreenSpaceCamera;

        for(int i = 0; i < RootOrder.Length; i++)
            RootOrder[i] = RootCanvas[i].sortingOrder;
    }

    protected override void OnUnityStart()
	{
		base.OnUnityStart();

        DoSubCameraStack(RootCanvas[Convert.ToInt32(E_UICanvas.Back)].worldCamera, true, true);
        DoSubCameraStack(UICamera[Convert.ToInt32(E_UICamera.Model3D)], true);
        DoSubCameraStack(RootCanvas[Convert.ToInt32(E_UICanvas.Front)].worldCamera, true);
    }

	protected override void OnRemove(CUIFrameBase _RemoveUIFrame, bool _force)
	{
        if (_force == false)
		{
            HideFrame(_RemoveUIFrame);
            _RemoveUIFrame.ImportRemove();
        }

        Destroy(_RemoveUIFrame.gameObject);       
    }

    protected override void OnClear()
	{
        ContentFrameList.Clear();
        SystemFrameList.Clear();
        mGlobalIndicatorRef = 0;
        m_listTopMost.Clear();
    }

    protected void SetActiveUICamera(E_UICamera _uiCamera, bool isActive)
    {
        switch(_uiCamera)
        {
            case E_UICamera.Front:
                {
                    if(!isActive)
                    {
                        var showFrame = SystemFrameList.Find(item => item.Show);

                        if (showFrame != null)
                            return;
                    }
                    
                    RootCanvas[Convert.ToInt32(E_UICanvas.Front)].gameObject.SetActive(isActive);
                }
                break;
        }

        UICamera[Convert.ToInt32(_uiCamera)].gameObject.SetActive(isActive);
    }

    //--------------------------------------------------------------------------
    //   protected void SetCameraPostProcessing(bool _postProcessingOn, int _layerMask = 0)
    //{
    //       UniversalAdditionalCameraData uniCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(RootBack.worldCamera);
    //       if (_postProcessingOn == false) _layerMask = 0;
    //       uniCamaraData.renderPostProcessing = _postProcessingOn;
    //       uniCamaraData.volumeLayerMask = _layerMask;
    //   }


    #region System Object
    protected int SetObject(CUIFrameBase _frame, GameObject _obj, GameObject _parentObj)
    {
        var hasFrame = ContentFrameList.Find(item => item == _frame);

        if (hasFrame != null && _obj != null)
        {
            int key = 0;

            for (int i = 0; i < SystemObjectList.Count; i++)
                if (key < SystemObjectList[i].Key)
                    key = SystemObjectList[i].Key + 1;

            SystemObjectList.Add(new SystemFrameObject
            {
                Frame = hasFrame,
                Obj = _obj,
                ParentObj = _parentObj,
                Key = key
            });

            _obj.transform.SetParent(RootCanvas[Convert.ToInt32(E_UICanvas.Front)].gameObject.transform, false);
            return key++;
        }

        return 0;
    }

    protected bool GetOutObject(CUIFrameBase _frame, int _objKey, GameObject _parentObj = null)
    {
        var hasObj = SystemObjectList.Find(item => item.Frame == _frame && item.Key == _objKey);

        if(hasObj != null && hasObj.Obj != null)
        {
            GameObject parentObj = _parentObj == null ? hasObj.ParentObj : _parentObj;
            hasObj.Obj.transform.SetParent(parentObj.transform, false);
            SystemObjectList.Remove(hasObj);
            return true;
        }

        return false;
    }
    #endregion

    #region ETC
    protected bool GetOpenFocusTypeFrame(E_UICanvas _canvas, E_UIFrameFocusAction _focusType)
    {
        List<CUIFrameBase> frameList = _canvas == E_UICanvas.Back ? ContentFrameList : SystemFrameList;
        var frame = frameList.Find(item => item.FocusType == _focusType && item.Show);

        if (frame != null)
            return true;
        
        return false;
    }
    #endregion

    //---------------------------------------------------------------------------
    /// <summary>
    /// 카메라 레이어 처리를 위한 스택 설정
    /// </summary>
    public void DoSubCameraStack(Camera _overlayCamera, bool _addCamera, bool _StackClear = false)
    {
        if (_overlayCamera == null) return;
        if (Camera.main == null) return;

        // UI에 오버레이 될 카메라 설정  
        UniversalAdditionalCameraData UniCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(Camera.main);

        if (_addCamera)
        {
            if (_StackClear)
            {
                UniCamaraData.cameraStack.Clear();
                if (_overlayCamera != RootCanvas[Convert.ToInt32(E_UICanvas.Back)].worldCamera)
                {
                    UniCamaraData.cameraStack.Add(_overlayCamera);
                }
                UniCamaraData.cameraStack.Add(RootCanvas[Convert.ToInt32(E_UICanvas.Back)].worldCamera);
            }
            else
            {
                List<Camera> listCamera = new List<Camera>(UniCamaraData.cameraStack);

                // 동일한 카메라가 있다면 추가하지 않는다.
                if (listCamera.Find((_camera) => _camera == _overlayCamera))
                {
                    return;
                }

                UniCamaraData.cameraStack.Clear();
                for (int i = 0; i < listCamera.Count; i++)
                {
                    if (listCamera[i] != RootCanvas[Convert.ToInt32(E_UICanvas.Back)].worldCamera)
                    {
                        UniCamaraData.cameraStack.Add(listCamera[i]);
                    }
                }
                UniCamaraData.cameraStack.Add(_overlayCamera);
                UniCamaraData.cameraStack.Add(RootCanvas[Convert.ToInt32(E_UICanvas.Back)].worldCamera);
                OnSubCameraStack(UniCamaraData, _overlayCamera, _addCamera);
            }
        }
        else
        {
            UniCamaraData.cameraStack.Remove(_overlayCamera);
            OnSubCameraStack(UniCamaraData, _overlayCamera, _addCamera);
        }

        CheckUICameraStack();
    }

    //--------------------------------------------------------------
    private void ActivePPCamera(bool _isActive)
    {
        UniversalAdditionalCameraData mainCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(Camera.main);

        var ppCamera = mainCamaraData.cameraStack.Find(item => item.name == E_CameraStackName.PPVolume.ToString());

        if (ppCamera != null)
            ppCamera.gameObject.SetActive(_isActive);
    }
    private void CheckPPCamera()
    {
        UniversalAdditionalCameraData mainCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(Camera.main);

        var hasSubSceneCamera = mainCamaraData.cameraStack.Find(item => item.name == E_CameraStackName.SubSceneCamera.ToString());
        
        ActivePPCamera(hasSubSceneCamera == null);
    }
    protected void CheckUICameraStack()
    {
        UniversalAdditionalCameraData mainCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(Camera.main);

        CheckPPCamera();

        var fullScreen = ContentFrameList.Find(item => item.FocusType == E_UIFrameFocusAction.FullScreen && item.Show);
        
        if (fullScreen != null)
            ActivePPCamera(false);

        mainCamaraData.cameraStack.Sort((x, y) =>
        {
            if (x.name == E_CameraStackName.PPVolume.ToString()) return -1;
            else if (x.name == E_CameraStackName.SubSceneCamera.ToString() && y.name != E_CameraStackName.PPVolume.ToString()) return -1;
            else if (x.name == E_CameraStackName.UICamera.ToString() && (y.name != E_CameraStackName.PPVolume.ToString() && y.name != E_CameraStackName.SubSceneCamera.ToString())) return -1;
            else if (x.name == E_CameraStackName.UICamera3D.ToString() && y.name == E_CameraStackName.UICameraFront.ToString()) return -1;
            return 0;
        });
    }

    protected void ChangeUIStackCamera(E_UICameraStack _type)
    {
        UniversalAdditionalCameraData mainCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(Camera.main);

        switch (_type)
        {
            case E_UICameraStack.Gacha:
                {
                    CheckPPCamera();

                    mainCamaraData.cameraStack.Sort((x, y) =>
                    {
                        if (x.name == E_CameraStackName.UICamera.ToString()) return -1;
                        else if (x.name == E_CameraStackName.UICamera3D.ToString() && y.name != E_CameraStackName.UICamera.ToString()) return -1;
                        else if (x.name == E_CameraStackName.UICameraFront.ToString() && y.name == E_CameraStackName.PPVolume.ToString()) return -1;
                        return 0;
                    });
                }
                break;
            default:
                {
                    var showGacha = ContentFrameList.Find(item => item.name == nameof(UIFrameGacha) && item.Show);

                    if(showGacha == null)
                        CheckUICameraStack();
                }
                break;
        }
    }

    protected Canvas GetRootCanvas(E_UICanvas _canvas)
    {
        return _canvas == E_UICanvas.Back ? RootCanvas[Convert.ToInt32(E_UICanvas.Back)] : RootCanvas[Convert.ToInt32(E_UICanvas.Front)];
    }

    protected E_UICanvas FindCanvasType(CUIFrameBase _frame)
    {
        var frameContent = ContentFrameList.Find(item => item == _frame);

        return frameContent != null ? E_UICanvas.Back : E_UICanvas.Front;
    }

    protected void InitializeFrame(CUIFrameBase _frame)
    {
        switch(_frame.FocusType)
        {
            case E_UIFrameFocusAction.ScreenBlock:
            case E_UIFrameFocusAction.SystemPopup:
            case E_UIFrameFocusAction.System:
                {
                    var frame = SystemFrameList.Find(item => item.ID == _frame.ID);

                    if(frame == null)
                    {
                        SystemFrameList.Add(_frame);
                        _frame.gameObject.transform.SetParent(RootCanvas[Convert.ToInt32(E_UICanvas.Front)].gameObject.transform, false);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// UIFrame을 찾아서 오픈시켜준다. (없을 경우 null 리턴)
    /// </summary>
    /// <param name= "_UIFrameID"> Frame Name </param>
    protected CUIFrameBase ShowFrame(string _UIFrameID)
    {
        CUIFrameBase UIFrame = FindFrame(_UIFrameID);
        if (UIFrame != null)
        {
            if (UIFrame.Show && UIFrame.IsDuplicatable)
			{
                UIFrame = Duplication(UIFrame, FindCanvasType(UIFrame));
            }
            else
			{
                ShowFrame(UIFrame, UIFrame.FocusType);
            }
        }
        return UIFrame;
    }

    /// <summary>
    /// UIFrame을 찾아서 숨겨준다. (없을 경우 null 리턴)
    /// </summary>
    /// <param name= "_UIFrameID"> Frame Name </param>
    protected CUIFrameBase HideFramed(string _UIFrameID)
    {
        CUIFrameBase UIFrame = FindFrame(_UIFrameID);
        if (UIFrame != null)
        {
            HideFrame(UIFrame);
        }
        return UIFrame;
    }

    /// <summary>
    /// UIFrame을 찾아서 숨겨준다. (Duplicate Frame이면 Destroy)
    /// </summary>
    /// <param name= "_UIFrame"> Frame Name </param>
    protected void HideFrame(CUIFrameBase _UIFrame, bool _force = false)
    {
        if(_force)
        {
            RemoveFrame(_UIFrame);
        }
        else
        {
            if (_UIFrame.HasDuplication())
            {
                _UIFrame.ImportHideDuplication();
            }
            else
            {
                HideFrame(_UIFrame, _UIFrame.FocusType);
            }
        }
    }

    private void RemoveFrame(CUIFrameBase _frame)
    {
        var contentFrame = ContentFrameList.Find(item => item.ID == _frame.ID);
        var systemFrame = SystemFrameList.Find(item => item.ID == _frame.ID);

        if (contentFrame != null)
            ContentFrameList.Remove(contentFrame);

        if (systemFrame != null)
            SystemFrameList.Remove(systemFrame);
    }

    protected void HideFrameAll()
    {
        List<CUIFrameBase>.Enumerator it = ContentFrameList.GetEnumerator();
        while (it.MoveNext())
        {
            HideFrame(it.Current);
            it = ContentFrameList.GetEnumerator();
        }

        it = ContentFrameList.GetEnumerator();
        while(it.MoveNext())
		{
            HideFrame(it.Current);
            it = ContentFrameList.GetEnumerator();
        }
    }

    /// <summary>
    /// 글로벌 인디케이터를 오픈한다.
    /// </summary>
    /// <param name= "_force"> true일 경우 _Show에 따라 참조 카운터를 강제로 1이나 0으로 바꿈 </param>
    protected void ShowGlobalIndicator(bool _show, bool _force)
    {
        CUIFrameScreenBlockBase screenBlock = FindScreenBlock();
        if (screenBlock != null)
        {
            if (_show)
            {
                if (_force)
				{
                    mGlobalIndicatorRef = 1;
				}
                else
				{
                    mGlobalIndicatorRef++;
                }

                int Order = GetOrder(E_UICanvas.Front) + 1;
                screenBlock.ImportShow(Order);
                screenBlock.DoScreenBlockShowIndicator(true);
            }
            else
            {
                if (_force)
				{
                    mGlobalIndicatorRef = 0;
				}
                else
				{
                    mGlobalIndicatorRef--;  // 여러군대에서 글로벌 인디케이터를 요청할 수 있으므로 카운팅
                }

                if (mGlobalIndicatorRef <= 0)
                {
                    mGlobalIndicatorRef = 0;
                    if (screenBlock.Show)
                    {
                        screenBlock.ImportHide();
                        screenBlock.DoScreenBlockShowIndicator(false);
                    }
                }
            }

            SortOrder(GetOrderOver(E_UICanvas.Front), SystemFrameList);
            //ZLog.Log(ZLogChannel.UI, $"GlobalIndicator Ref: {mGlobalIndicatorRef}");
        }
    }

    protected CUIFrameBase ShowTopMost(string _UIFrameID, bool _topOn)
	{
        CUIFrameBase UIFrame = FindFrame(_UIFrameID);

        if (UIFrame != null)
        { 
            int topMost = GetOrderTopMost() + 1;

            m_listTopMost.Add(UIFrame);
            UIFrame.ImportShowTopMost(topMost, _topOn);
        }

        return UIFrame;
    }

    //--------------------------------------------------------
    private void ShowFrame(CUIFrameBase _UIFrame, E_UIFrameFocusAction _FocusAction)
    {
        switch (_FocusAction)
        {
            case E_UIFrameFocusAction.Panel:
                ArrangeTopMost(ContentFrameList);
                SortOrder(RootOrder[Convert.ToInt32(E_UICanvas.Back)], ContentFrameList);
                ShowPanel(_UIFrame, ContentFrameList);
                SortOrder(GetOrderOver(E_UICanvas.Back), ContentFrameList);
                break;
            case E_UIFrameFocusAction.FullScreen:
                ArrangeTopMost(ContentFrameList);
                SortOrder(RootOrder[Convert.ToInt32(E_UICanvas.Back)], ContentFrameList);
                ShowFullScreen(_UIFrame);
                SortOrder(GetOrderOver(E_UICanvas.Back), ContentFrameList);
                break;
            case E_UIFrameFocusAction.System:
                {
                    if(!_UIFrame.Show)
                    {
                        SortOrder(GetOrder(E_UICanvas.Front), SystemFrameList);
                        Show(_UIFrame, E_UICanvas.Front, SystemFrameList);
                    }
                }
                break;
            case E_UIFrameFocusAction.SystemPopup:
                {
                    SortOrder(GetOrder(E_UICanvas.Front), SystemFrameList);
                    Show(_UIFrame, E_UICanvas.Front, SystemFrameList);
                    SetActiveUICamera(E_UICamera.Front, true);
                }
                break;
        }        
    }

    private CUIFrameBase Duplication(CUIFrameBase UIFrame, E_UICanvas _type)
	{
        CUIFrameBase CloneFrame = UIFrame.ImportDuplication(GetOrder(_type));
        return CloneFrame;
	}

    private void ShowPanel(CUIFrameBase _UIFrame, List<CUIFrameBase> _frameList)
    {
        int Order = GetOrder(E_UICanvas.Front);
        ShowInternal(_UIFrame, Order, _frameList);
    }

    private void ShowFullScreen(CUIFrameBase _UIFrame)
    {
        OnFullScreenUIShowHide(true, _UIFrame.IsCullingMaskByFullScreen);
        // 아래쪽의 모든 프레임을 비활성한다.
        List<CUIFrameBase>.Enumerator it = ContentFrameList.GetEnumerator();
        List<CUIFrameBase> listFilter = new List<CUIFrameBase>();
        while (it.MoveNext())
        {
            if (it.Current != _UIFrame)
            {
                listFilter.Add(it.Current);
            }
        }

        for (int i = 0; i < listFilter.Count; i++)
		{
            listFilter[i].ImportShowAutoShowHide(false);
        }

        int Order = GetOrder(E_UICanvas.Back);
        ShowInternal(_UIFrame, Order, ContentFrameList);       
    }

    private void Show(CUIFrameBase _UIFrame, E_UICanvas _type, List<CUIFrameBase> _frameList)
    {
        SetActiveUICamera(E_UICamera.Front, true);

        int Order = GetOrder(_type);
        ShowInternal(_UIFrame, Order, _frameList);

        switch (_UIFrame.FocusType)
        {
            case E_UIFrameFocusAction.SystemPopup:
                {
                    var showFrame = SystemFrameList.Find(item => item.Show && item.FocusType == E_UIFrameFocusAction.SystemPopup);

                    if (showFrame != null)
                    {
                        ScreenBlockActive(true);
                    }
                }
                break;
        }
    }

    private void ShowInternal(CUIFrameBase _uiFrame, int _order, List<CUIFrameBase> _listUIFrame)
    {
        if (_listUIFrame.Contains(_uiFrame))
        {
            _listUIFrame.Remove(_uiFrame);
        }

        _listUIFrame.Add(_uiFrame);


        if (_uiFrame.Show)
        {
            _uiFrame.ImportRefreshOrder(_order);
        }
        else
        {
            _uiFrame.ImportShow(_order);
        }
    }

    //--------------------------------------------------------
    private void HideFrame(CUIFrameBase _UIFrame, E_UIFrameFocusAction _FocusAction)
    {
        switch (_FocusAction)
        {
            case E_UIFrameFocusAction.Panel:
                HidePanel(_UIFrame);
                break;
            case E_UIFrameFocusAction.FullScreen:
                HideFullScreen(_UIFrame);
                break;
            case E_UIFrameFocusAction.SystemPopup:
                Hide(_UIFrame);
                break;
            case E_UIFrameFocusAction.System:
                Hide(_UIFrame);
                break;
        }

        OnShowHide(_UIFrame, false);
    }

    private void HidePanel(CUIFrameBase _UIFrame)
    {
        ContentFrameList.Remove(_UIFrame);
        _UIFrame.ImportHide();
    }

    private void HideFullScreen(CUIFrameBase _UIFrame)
    {
        HidePanel(_UIFrame);
        OnFullScreenUIShowHide(false, false);
        
        // 이전 프레임 중에 풀스크린이 있나 찾는다.
        List<CUIFrameBase>.Enumerator it = ContentFrameList.GetEnumerator();
        List<CUIFrameBase> listFilter = new List<CUIFrameBase>();
        CUIFrameBase anotherFullScreenUIFrame = null;
        while(it.MoveNext())
        {
            if (it.Current.FocusType == E_UIFrameFocusAction.FullScreen)
            {
                anotherFullScreenUIFrame = it.Current;
            }
            else
            {
                listFilter.Add(it.Current);
            }
        }

       
        if (anotherFullScreenUIFrame != null)
        {
            ShowFullScreen(anotherFullScreenUIFrame);
        }
        else
		{
            for (int i = 0; i < listFilter.Count; i++)
            {
                listFilter[i].ImportShowAutoShowHide(true);
            }
        }
    }

    private void Hide(CUIFrameBase _UIFrame)
    {
        _UIFrame.ImportHide();

        switch(_UIFrame.FocusType)
        {
            case E_UIFrameFocusAction.SystemPopup:
                {
                    var showPopup = SystemFrameList.Find(item => item.Show && item.FocusType != E_UIFrameFocusAction.ScreenBlock && item.FocusType == E_UIFrameFocusAction.SystemPopup);
                    var showFrame = SystemFrameList.Find(item => item.Show && item.FocusType != E_UIFrameFocusAction.ScreenBlock);

                    if (showPopup == null)
                        ScreenBlockActive(false);
                    
                    if(showFrame == null)
                        SetActiveUICamera(E_UICamera.Front, false);
                }
                break;
        }
    }

    //--------------------------------------------------------
    private void ScreenBlockActive(bool _isActive)
    {
        CUIFrameScreenBlockBase ScreenBlock = FindScreenBlock();

        if (ScreenBlock != null)
        {
            if(_isActive)
            {
                SortOrder(GetOrder(E_UICanvas.Front), SystemFrameList);
                ShowPanel(ScreenBlock, SystemFrameList);
            }
            else
            {
                if (ScreenBlock != null)
                    Hide(ScreenBlock);
            }
        }
    }

    private void SortOrder(int _RootOrder, List<CUIFrameBase> _listUIFrame)
    {
        int Order = _RootOrder;

        List<CUIFrameBase>.Enumerator it = _listUIFrame.GetEnumerator();
        while (it.MoveNext())
        {
            it.Current.ImportRefreshOrder(Order);
            Order += SortOrderGap;
        }
    }

    private void ArrangeTopMost(List<CUIFrameBase> _listUIFrame)
	{
        for (int i = 0; i < m_listTopMost.Count; i++)
		{
            CUIFrameBase topMost = m_listTopMost[i];
            if (_listUIFrame.Contains(topMost))
			{
                _listUIFrame.Remove(topMost);
                _listUIFrame.Add(topMost);
			}
		}
        m_listTopMost.Clear();
	}

    //--------------------------------------------------------   
    private int GetOrder(E_UICanvas _type)
    {
        int order = 0;

        switch (_type)
        {
            case E_UICanvas.Back: order = (ContentFrameList.Count * SortOrderGap) + RootOrder[Convert.ToInt32(_type)]; break;
            case E_UICanvas.Front: order = (SystemFrameList.Count * SortOrderGap) + RootOrder[Convert.ToInt32(_type)]; break;
        }

        return order;
    }

    private int GetOrderOver(E_UICanvas _type)
    {
        return GetOrder(_type) + SortOrderGap;
    }

    private int GetOrderTopMost()
	{
        int topMost = 0;
        List<CUIFrameBase>.Enumerator it = ContentFrameList.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.LayerOrder > topMost)
            {
                topMost = it.Current.LayerOrder;
            }
        }
        return topMost;
    }
    //--------------------------------------------------------
    protected virtual void OnFullScreenUIShowHide(bool _Show, bool bCullingMask) { } 
    protected virtual void OnShowHide(CUIFrameBase _uiFrame, bool _show) { }
    protected virtual void OnSubCameraStack(UniversalAdditionalCameraData _mainCameraData, Camera _overlayCamera, bool _addCamera) { }
    //-------------------------------------------------------- 
    protected sealed override CUIFrameScreenBlockBase FindScreenBlock() { return base.FindScreenBlock(); }
    //--------------------------------------------------------
}
