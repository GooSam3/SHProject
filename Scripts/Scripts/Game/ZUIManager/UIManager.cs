using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class UIManager : CManagerUIFrameFocusBase
{   public static new UIManager Instance { get { return CManagerUIFrameBase.Instance as UIManager; } }
    [SerializeField] private E_UILocalizing Language = E_UILocalizing.Korean;

    private event UnityAction<CUIFrameBase, bool> mFocusNotify = null;
    private event UnityAction mUpdateUIFrame = null;
    private bool             mUpdateStart = false;    

    /// <summary> 절전모드 </summary>
    public UIFrameScreenSaver ScreenSaver;

    //------------------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        Initialize((int)Language, (int)E_UILocalizing.Korean, nameof(UIScreenBlock));
    }

    protected override void OnUnityStart()
    {
        base.OnUnityStart();
    }

    protected override void OnFullScreenUIShowHide(bool _Show, bool bCullingMask) 
    {
        base.OnFullScreenUIShowHide(_Show, bCullingMask);
       
        if (CameraManager.hasInstance)
		{
            CameraManager.Instance.DoSetCullingMaskForUI(bCullingMask);
        }
    }

	protected override void OnShowHide(CUIFrameBase _uiFrame, bool _show)
	{
        mFocusNotify?.Invoke(_uiFrame, _show);
	}

    protected override void OnSubCameraStack(UniversalAdditionalCameraData _mainCameraData, Camera _overlayCamera, bool _addCamera) 
    {
        CameraManager.Instance.DoPostProcessTurn(!_addCamera);
    }

    protected override void OnUIManagerUpdate()
	{
        base.OnUIManagerUpdate();

        if (mUpdateStart)
		{
            mUpdateUIFrame?.Invoke();
		}

        // hhj : 임시 코드
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnInputKey(KeyCode.Escape);
            return;
        }
    }

    /// <summary>
    /// [박윤성] 절전모드시 FrameScreenSaver위에 띄울 SubUI 레이어 변환용
    /// </summary>
    /// <param name="_obj"></param>
    /// <param name="_newLayer"></param>
    public void SetLayer(GameObject _obj, int _newLayer)
    {
        ZLog.Log(ZLogChannel.UI, "newLayer " + _newLayer);

        if (_obj == null)
            return;

        var canvas = _obj.GetComponent<Canvas>();

        if (canvas != null)
            canvas.sortingLayerName = LayerMask.LayerToName(_newLayer);

        _obj.layer = _newLayer;

        //나중에 자식들까지 필요하게 되면 추가
        //foreach (Transform child in _obj.transform)
        //{
        //    if (child == null)
        //        continue;

        //    SetLayerRecursive(child.gameObject, _newLayer);
        //}
    }

    //----------------------------------------------------------------------------------------
    public void LoadRequiredUI(UnityAction _finish)
	{
		Load<UIFrameLoadingScreen>(nameof(UIFrameLoadingScreen), null);     
        Load<UIFrameNameTag>(nameof(UIFrameNameTag), null);
        Load<UIFrameMessageNotice>(nameof(UIFrameMessageNotice), null);
        Load<UIPopupSystem>(nameof(UIPopupSystem), null);
        Load<UIPopupCostConfirm>(nameof(UIPopupCostConfirm), null);        
        Load<UIGainSystem>(nameof(UIGainSystem), null);
        Load<UIPopupStageMove>(nameof(UIPopupStageMove), null);
        Load<UIControlBlock>( nameof( UIControlBlock ), null );

        Load<UIFrameFadeInOut>(nameof(UIFrameFadeInOut), null);
        // 항상 마지막에 배치 
        Load<UIScreenBlock>(nameof(UIScreenBlock), (string _loadUI, UIScreenBlock _loadFrame) => {
            _finish?.Invoke();
        });
	}

    public void AddEventUIFrameUpdate(UnityAction _updateFrame, bool _add)
	{
        if (_add)
		{
            mUpdateUIFrame += _updateFrame;
        }
        else
		{
            mUpdateUIFrame -= _updateFrame;
		}
    }

    public void SetUIManagerUpdate(bool _updateStart)
	{
        mUpdateStart = _updateStart;
	}

    /// <summary>
    /// UIFrame을 로드한다. (나중에 사용하기 위해 미리 바인딩하는 용도)
    /// </summary>
    /// <param name= "_addressableUIFrame"> Addressable Name </param>
    /// <param name= "_eventFinish"> 로드 후 Callback </param>
    public void Load<FrameType>(string _addressableUIFrame, UnityAction<string, FrameType> _eventFinish) where FrameType : CUIFrameBase
    {
        ZResourceManager.Instance.Instantiate(_addressableUIFrame, (string _loadedName, FrameType _loadedUIFrame) => 
        {
            if (HasFrame(_loadedUIFrame))
			{
                Destroy(_loadedUIFrame);
                _loadedUIFrame = null;
			}
            else
			{
                if (_loadedUIFrame)
                {
                    ImportFrame(_loadedUIFrame, true);
                    InitializeFrame(_loadedUIFrame);
                    _eventFinish?.Invoke(_loadedName, _loadedUIFrame);
                }
            }
        });     
    }

    /// <summary>
    /// UIFrame을 로드한다. (나중에 사용하기 위해 미리 바인딩하는 용도)
    /// </summary>
    /// <param name= "_addressableUIFrame"> Addressable Name </param>
    /// <param name= "_eventFinish"> 로드 후 Callback </param>
    public void Load(string _addressableUIFrame, UnityAction<string, CUIFrameBase> _eventFinish)
    {
        ZResourceManager.Instance.Instantiate(_addressableUIFrame, (string _loadedName, CUIFrameBase _loadedUIFrame) => 
        {
            if (_loadedUIFrame)
            {
                ImportFrame(_loadedUIFrame, true);
                InitializeFrame(_loadedUIFrame);
                _eventFinish?.Invoke(_loadedName, _loadedUIFrame);
            }
        });
    }

    //-------------------------------------------------------------------------------

    /// <summary>
    ///   [주의!] 여러 객체에서 인디케이터를 요청할수 있으므로 내부에 레퍼 카운터가 걸려있다.
    ///   객체가 On을 요청하면 반드시 Off를 해서 인디케이터가 로스트 되지 않아야 한다. 
    ///   _force = true 일 경우 내부 레퍼 카운터가 1이나 0으로 초기화 된다.
    /// </summary>
    public new void ShowGlobalIndicator(bool _on, bool _force = false)
	{
        base.ShowGlobalIndicator(_on, _force); 
	}

    public void Clear()
	{
        mFocusNotify = null;
        mUpdateUIFrame = null;        
        ClearAll();
	}

    public void HideAll()
	{
        HideFrameAll();
    }

    public CUIFrameBase TopMost<FrameType>(bool _topOn, bool _InputEnable = true) where FrameType : CUIFrameBase
    {
        CUIFrameBase uiFrame = ShowTopMost(typeof(FrameType).ToString(), _topOn);
        if (uiFrame != null) uiFrame.ImportInputEnable(_InputEnable);
        return uiFrame;
	}

    /// <summary>
    /// [주의!] 강 참조이므로 객체가 삭제되기 전에 반드시 _regist = false로 제거해야 한다. 메모리 해제가 안될수 있다.
    /// </summary>
    public void RegisterFocusNotify(UnityAction<CUIFrameBase, bool> _eventNotify, bool _regist = false)
	{
        if (_regist)
		{
            mFocusNotify += _eventNotify;
		}
        else
		{
            mFocusNotify -= _eventNotify;
		}
	}

    //--------------------------------------------------------------------------
    #region System Object
    public int SetSystemObject(CUIFrameBase _frame, GameObject _obj, GameObject _parentObj)
    {
        return SetObject(_frame, _obj, _parentObj);
    }

    public bool GetOutSystemObject(CUIFrameBase _frame, int _objKey, GameObject _obj = null)
    {
        return GetOutObject(_frame, _objKey, _obj);
    }
    #endregion

    #region ETC
    public bool GetOpenFocusType(E_UICanvas _canvas, E_UIFrameFocusAction _focusType)
    {
        return GetOpenFocusTypeFrame(_canvas, _focusType);
    }
    #endregion

    #region Camera Stack
    /// <summary>메인 카메라의 스택 순서를 변경한다.</summary>
    /// <param name="_type">변경할 스택 타입</param>
    public void ChangeMainCameraStack(E_UICameraStack _type)
    {
        ChangeUIStackCamera(_type);
    }
    #endregion 

    #region Camera Active
    public void ActiveUICamera(E_UICamera _cameraType, bool isActive)
    {
        SetActiveUICamera(_cameraType, isActive);
    }
    #endregion

    #region UI Input
    public void OnInputKey(KeyCode _key)
    {
        switch(_key)
        {
            case KeyCode.Escape:
                {
                    // 시스템 팝업이 노출된 경우
                    if (SystemFrameList.Find(item => !item.IsBackable && item.Show && item.FocusType == E_UIFrameFocusAction.SystemPopup))
                        return;

                    if (CheckCloseFrame(SystemFrameList))
                        return;

                    if (CheckCloseFrame(ContentFrameList))
                        return;

                    UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                        _popup.Open(ZUIString.WARRING, DBLocale.GetText("게임을 종료하시겠습니까?"), new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
                    _popup.Close(); }, delegate{Application.Quit(); } });
                    });

                    bool CheckCloseFrame(List<CUIFrameBase> _list)
                    {
                        var Frames = _list.FindAll(item => item.IsBackable && item.Show);

                        if (Frames.Count > 0)
                        {
                            Frames.Sort((x, y) =>
                            {
                                if (x.LayerOrder < y.LayerOrder) return -1;
                                return 0;
                            });

                            Close(Frames[0].ID);
                            return true;
                        }

                        return false;
                    }
                }
                break;
        }
    }
    #endregion

    #region Canvas
    public Canvas GetCanvas(E_UICanvas _canvas)
    {
        return GetRootCanvas(_canvas);
    }
    #endregion

    #region Frame Close
    /// <summary>UIFrame을 닫는다. (자주 호출해야되는 경우 가급적 사용 X)</summary>
    /// <param name= "_bRemove"> 제거 여부 (False == Hide) </param>
    public void Close<FrameType>(bool _bRemove = false) where FrameType : CUIFrameBase
    {
        string frame = typeof(FrameType).ToString();
        var findFrame = FindFrame(frame);

        if (findFrame != null)
        {
            if (_bRemove) 
                RemoveFrame(frame);

            
            HideFrame(findFrame, _bRemove);

            CheckUICameraStack();
        }
    }

    /// <summary>UIFrame을 닫는다. </summary>
    /// <param name= "_frameName"> 요청하려는 Frame Name </param>
    /// <param name= "_bRemove"> 제거 여부 (False == Hide) </param>
    public void Close(string _frameName, bool _bRemove = false)
    {
        var findFrame = FindFrame(_frameName);

        if (findFrame != null)
        {
            if (_bRemove) 
                RemoveFrame(_frameName);
            
            HideFrame(findFrame, _bRemove);

            CheckUICameraStack();
        }
    }
    #endregion

    #region Frame Open
    private void Show<FrameType>(string _frameName, UnityAction<string, FrameType> _EventFinish = null) where FrameType : CUIFrameBase
    {
        CUIFrameBase UIFrame = ShowFrame(_frameName);
        _EventFinish?.Invoke(_frameName, UIFrame as FrameType);
    }

    public FrameType Show<FrameType>() where FrameType : CUIFrameBase
	{
        return ShowFrame(typeof(FrameType).ToString()) as FrameType;
    }


    /// <summary> UIFrame을 생성해서 보여준다. (없을 경우만 생성) </summary>
    /// <param name= "_EventFinish"> Open 처리 이후 실행될 Callback </param>
    public void Open<FrameType>(UnityAction<string, FrameType> _EventFinish = null) where FrameType : CUIFrameBase
    {
        string frame = typeof(FrameType).ToString();

        if (FindFrame(frame) != null)
        {
            Show(frame, _EventFinish);
        }
        else
        {
            Load<FrameType>(frame, (string _loadedName, FrameType _loadedUIFrame) => {
                Show(frame, _EventFinish);
            });
        }

        CheckUICameraStack();
    }
    #endregion

    #region Frame FInd
    /// <summary>UIFrame을 찾아서 리턴해준다. </summary>
    /// <param name= "_frameName"> 요청하려는 Frame Name </param>
    public FrameType Find<FrameType>(string _frameName) where FrameType : CUIFrameBase
    {
        return FindFrame(_frameName) as FrameType;
    }

    /// <summary>UIFrame을 찾아서 리턴해준다. (자주 호출해야되는 경우 가급적 사용 X)</summary>
    public FrameType Find<FrameType>() where FrameType : CUIFrameBase
    {
        return FindFrame(typeof(FrameType).ToString()) as FrameType;
    }

    /// <summary> UIFrame의 Null 체크 후 바로 사용할 수 있는 함수 (자주 호출해야되는 경우 가급적 사용 X) </summary>
    public FrameType Find<FrameType>(out FrameType _frame) where FrameType : CUIFrameBase
    {
        _frame = FindFrame(typeof(FrameType).ToString()) as FrameType;
        return _frame;
    }

    /// <summary> UIFrame의 Null 체크 후 바로 사용할 수 있는 함수 </summary>
    public bool Find<FrameType>(string _frameName, out FrameType _frame) where FrameType : CUIFrameBase
    {
        _frame = FindFrame(_frameName) as FrameType;
        return null != _frame;
    }

    #endregion
}