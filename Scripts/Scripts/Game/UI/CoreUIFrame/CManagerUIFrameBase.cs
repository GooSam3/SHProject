using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


//  UI 중앙관리용 객체 
//  Localizing Key에 따라 해당 UIFrame이 맵핑된다. 없으면 디폴트 (Korean)이 맵핑된다.
//  Localizing UIFrame은 별도의 Thema를 가진다. Thema는 같은 기능을 가진 다른 스킨의  UI이다. 
[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(StandaloneInputModule))]
abstract public class CManagerUIFrameBase : CManagerTemplateBase<CManagerUIFrameBase>
{   
    private class SUIFrameInstance
    {
        public string ID;
        public int LocalizingKey = 0;
        public int CurrentThema = 0;
        public bool IsAddressable = false;
        public Dictionary<int, CUIFrameBase> mapThemaInstance = new Dictionary<int, CUIFrameBase>();
    }

    private CMultiSortedDictionary<string, SUIFrameInstance> m_mapUIFrameLocalizing = new CMultiSortedDictionary<string, SUIFrameInstance>();
    private Dictionary<string, CUIFrameBase>               m_mapUIFrameInstance = new Dictionary<string, CUIFrameBase>();
    private string mScreenBlock; // 전체 화면을 비활성화 해주는 UIFrame; 
    private int mLocalizingKey = 0;
    private int mLocalizingKeyDefault = 0;
    private Vector2 mScreenSize = Vector2.zero;           public Vector2 ScreenSize { get { return mScreenSize; } }
	//--------------------------------------------------------------------------
	protected void OnApplicationQuit()
	{
        Dictionary<string, CUIFrameBase>.Enumerator it = m_mapUIFrameInstance.GetEnumerator();
        while(it.MoveNext())
		{
            it.Current.Value.ImportRemove();
		}
    }

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
        RectTransform UIScreenSize = transform as RectTransform;
        mScreenSize = UIScreenSize.sizeDelta;
    }

    protected virtual void Update()
    {
        OnUIManagerUpdate();
    }

    //--------------------------------------------------------------------------
    /// <summary>
    /// UIFrame 로드
    /// </summary>
    public void ImportFrame(CUIFrameBase _uiFrame, bool _isAddressable = false)
    {
        if (HasFrame(_uiFrame))
        {
            Destroy(_uiFrame);
            return;
        }

        // 나중에 지워야 함
        _uiFrame.gameObject.transform.SetParent(gameObject.transform, false);
        _uiFrame.ImportInitialize(_isAddressable);

        ArrangeInstance(_uiFrame);
        SelectLocalizing(mLocalizingKey, mLocalizingKeyDefault);
    }

    public Vector3 GetUIRootWorldPosition()
	{
        return transform.position;
	}

    //--------------------------------------------------------------------------
    protected void Initialize(int _LocalizingKey, int _LocalizingKeyDefault, string _ScreenBlockUIFrame)
    {
        mLocalizingKey = _LocalizingKey;
        mLocalizingKeyDefault = _LocalizingKeyDefault;
        mScreenBlock = _ScreenBlockUIFrame;

        List<CUIFrameBase> listUIFrameInstance = new List<CUIFrameBase>();
        GetComponentsInChildren(true, listUIFrameInstance);

        InitializeUIFrame(listUIFrameInstance);
        ArrangeInstanceUIList(listUIFrameInstance);
        SelectLocalizing(_LocalizingKey, _LocalizingKeyDefault);

        OnUIManagerInitialize();
    }

    /// <summary>
    /// UIFrame을 제거한다. (_force 여부에 따라 제거 또는 숨김)
    /// </summary>
    /// <param name= "_UIFrameName"> Frame Name </param>
    /// <param name= "_force"> Frame 제거 여부 </param>
    protected void RemoveFrame(string _UIFrameName, bool _force = false)
    {
        CUIFrameBase UIFrame = FindFrame(_UIFrameName);

        if (UIFrame == null) return;

        RemoveFrameInternal(UIFrame, _force);
    }

    /// <summary>
    /// UIFrame을 찾아서 리턴해준다.
    /// </summary>
    /// <param name= "_UIFrameID"> Frame Name </param>
    protected CUIFrameBase FindFrame(string _UIFrameID)
    {
        CUIFrameBase Result = null;
        m_mapUIFrameInstance.TryGetValue(_UIFrameID, out Result);
        return Result;
    }

    protected bool HasFrame(CUIFrameBase _uiFrame)
	{
        bool hasFrame = false;
        Dictionary<string, CUIFrameBase>.Enumerator it = m_mapUIFrameInstance.GetEnumerator();
        while(it.MoveNext())
		{
            if (it.Current.Value.GetType().Name == _uiFrame.GetType().Name)
			{
                hasFrame = true;
                break;
			}
		}
        return hasFrame;
	}

    protected virtual CUIFrameScreenBlockBase FindScreenBlock()
    {
        return FindFrame(mScreenBlock) as CUIFrameScreenBlockBase;
    }

    protected void ClearAll()
	{
        m_mapUIFrameLocalizing.Clear();

        List<CUIFrameBase> listUIFrameAll = m_mapUIFrameInstance.Values.ToList();

        for (int i = 0; i < listUIFrameAll.Count; i++)
		{
            RemoveFrameInternal(listUIFrameAll[i], true);
		}

        m_mapUIFrameInstance.Clear();

        OnClear();
    }

    //-------------------------------------------------------------------------
    private void InitializeUIFrame(List<CUIFrameBase> _listUIFrameInstance)
    {
        for (int i = 0; i < _listUIFrameInstance.Count; i++)
        {
            _listUIFrameInstance[i].ImportInitialize(false);            
        }
    }

    private void ArrangeInstanceUIList(List<CUIFrameBase> _listUIFrameInstance)
    {
        m_mapUIFrameLocalizing.Clear();
        for (int i = 0; i < _listUIFrameInstance.Count; i++)
        {
            ArrangeInstance(_listUIFrameInstance[i]);
        }
    }

    private void ArrangeInstance(CUIFrameBase _UIFrame)
    {
        string UIFrameID = _UIFrame.ID;
        int UILocalizeKey = _UIFrame.LocalizingKey;
        int UIThema = _UIFrame.ThemaType;

        if (m_mapUIFrameLocalizing.ContainsKey(UIFrameID))
        {
            List<SUIFrameInstance> listUIFrameInstance = m_mapUIFrameLocalizing[UIFrameID];
            SUIFrameInstance uiFrameInstance = null;

            for (int j = 0; j < listUIFrameInstance.Count; j++)
            {
                if (listUIFrameInstance[j].LocalizingKey == UILocalizeKey)
                {
                    uiFrameInstance = listUIFrameInstance[j];
                    break;
                }
            }

            if (uiFrameInstance == null)
            {
                uiFrameInstance = new SUIFrameInstance();
                uiFrameInstance.ID = UIFrameID;
                uiFrameInstance.LocalizingKey = UILocalizeKey;
                uiFrameInstance.mapThemaInstance[UIThema] = _UIFrame;
               
                m_mapUIFrameLocalizing.Add(UIFrameID, uiFrameInstance);
            }
            else
            {
                uiFrameInstance.mapThemaInstance[UIThema] = _UIFrame;
            }
        }
        else
        {
            SUIFrameInstance uiFrameInstance = new SUIFrameInstance();
            uiFrameInstance.LocalizingKey = UILocalizeKey;
            uiFrameInstance.ID = UIFrameID;
            uiFrameInstance.mapThemaInstance[UIThema] = _UIFrame;
            
            m_mapUIFrameLocalizing.Add(UIFrameID, uiFrameInstance);
        }
    }

    private void SelectLocalizing(int _LocalizingKey, int _DefaultLocalizingKey)
    {       
        m_mapUIFrameInstance.Clear();

        List<string> listKey = m_mapUIFrameLocalizing.keys.ToList();

        for (int i = 0; i < listKey.Count; i++)
        {
            List<SUIFrameInstance> listUIFrame = m_mapUIFrameLocalizing[listKey[i]];

            SUIFrameInstance SelectedUIFrame = null;
            for (int j = 0; j < listUIFrame.Count; j++)
            {
                SUIFrameInstance uiFrameInstance = listUIFrame[j];
                if (uiFrameInstance.LocalizingKey == _DefaultLocalizingKey)
                {
                    if (SelectedUIFrame == null)
                    {
                        SelectedUIFrame = uiFrameInstance;
                    }
                }
                else if (uiFrameInstance.LocalizingKey == _LocalizingKey)
                {
                    SelectedUIFrame = uiFrameInstance;
                }
            }
            
            if (SelectedUIFrame != null)
            {
                ChangeThema(SelectedUIFrame);
            }        
        }
    }

    private void ChangeThema(SUIFrameInstance _UIFrameInstance)
    {
        CUIFrameBase ThemaUIFrame = null;
        _UIFrameInstance.mapThemaInstance.TryGetValue(_UIFrameInstance.CurrentThema, out ThemaUIFrame);
        if (ThemaUIFrame != null)
        {
            m_mapUIFrameInstance[_UIFrameInstance.ID] = ThemaUIFrame;
        }
    }

    private void RemoveFrameInternal(CUIFrameBase _removeFrame, bool _force)
	{
        _removeFrame.ImportRemove();

        m_mapUIFrameInstance.Remove(_removeFrame.ID);
        m_mapUIFrameLocalizing.Remove(_removeFrame.ID);

        OnRemove(_removeFrame, _force);
    }

    //-----------------------------------------------------------------------------
    protected virtual void OnUIManagerInitialize() { }
    protected virtual void OnRemove(CUIFrameBase _RemoveUIFrame, bool _force) { }
    protected virtual void OnClear() { }
    protected virtual void OnUIManagerUpdate() { }
}
