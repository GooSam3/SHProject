using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(StandaloneInputModule))]
public abstract class CManagerUIFrameBase : CManagerTemplateBase<CManagerUIFrameBase>
{
	private Dictionary<string, CUIFrameBase> m_mapUIFrameInstance = new Dictionary<string, CUIFrameBase>();
	private Canvas m_pRootCanvas = null; 
	private CanvasScaler			m_pRootCanvasScaler = null;
	private EventSystem			m_pEventSystem = null;
	private StandaloneInputModule	m_pStandAloneInputModule = null;
	private Vector2 m_vecScreenSize = Vector3.zero; public Vector2 GetUIScreenSize() { return m_vecScreenSize; }
	private bool m_bLoadFinish = false;		public bool IsLoadFinish { get { return m_bLoadFinish; } }
	//-------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pRootCanvas = GetComponent<Canvas>();
		m_pRootCanvasScaler = GetComponent<CanvasScaler>();
		m_pEventSystem = GetComponent<EventSystem>();
		m_pStandAloneInputModule = GetComponent<StandaloneInputModule>();

		PrivMgrUIFrameInitalize();
		OnUIMgrInitializeCanvas(m_pRootCanvas);
	}

	protected override void OnUnityStart()
	{
		// [주의] 이 단계에서 전체 UI의 사이즈가 디바이스에 맞게 픽스된다. 따라서 m_bLoadFinish가 호출되기 이전에 UIFrame을 호출하면 안된다.
		base.OnUnityStart();
		RectTransform UIScreenSize = transform as RectTransform;
		m_vecScreenSize = UIScreenSize.sizeDelta;
		m_bLoadFinish = true;
	}
	//----------------------------------------------------------------
	public Camera GetUIManagerCamara() 
	{
		if (m_pRootCanvas == null) return null;
		return m_pRootCanvas.worldCamera; 
	}

	//-------------------------------------------------------------------
	protected void ProtMgrUIFrameLoad(CUIFrameBase pUIFrame, bool bInit = true)
	{
		string strUIFrameName = pUIFrame.GetType().Name;
		if (m_mapUIFrameInstance.ContainsKey(strUIFrameName))
		{
			//Error!
		}
		else
		{
			m_mapUIFrameInstance[strUIFrameName] = pUIFrame;
			if (bInit)
			{
				pUIFrame.ImportUIFrameInitialize();
				pUIFrame.ImportUIFrameInitializePost();
			}
		}

		if (pUIFrame.GetUIFrameFocusType() == CManagerUIFrameFocusBase.EUIFrameFocusType.Invisible)
		{
			pUIFrame.ImportUIFrameShow(0);
		}
	}

	protected CUIFrameBase FindUIFrame(string strUIFrame)
	{
		CUIFrameBase pFindUIFrame = null;
		if (m_mapUIFrameInstance.ContainsKey(strUIFrame))
		{
			pFindUIFrame = m_mapUIFrameInstance[strUIFrame];
		}
		return pFindUIFrame;
	}

	//--------------------------------------------------------------------
	private void PrivMgrUIFrameInitalize()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			CUIFrameBase pUIFrame = transform.GetChild(i).gameObject.GetComponent<CUIFrameBase>();
			if (pUIFrame)
			{
				pUIFrame.ImportUIFrameInitialize();
				ProtMgrUIFrameLoad(pUIFrame, false);
			}
		}

		Dictionary<string, CUIFrameBase>.ValueCollection.Enumerator it = m_mapUIFrameInstance.Values.GetEnumerator();
		while (it.MoveNext())
		{
			it.Current.ImportUIFrameInitializePost();
		}
	}
	//--------------------------------------------------------------------
	protected virtual void OnUIMgrInitializeCanvas(Canvas pRootCanvas) { }
}
