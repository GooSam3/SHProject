using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(StandaloneInputModule))]
public abstract class CManagerUIFrameBase : CManagerTemplateBase<CManagerUIFrameBase>
{
	private Dictionary<string, CUIFrameBase> m_mapUIFrameInstance = new Dictionary<string, CUIFrameBase>();
	private Canvas		m_pRootCanvas = null;
	private CanvasScaler	m_pRootCanvasScaler = null;
	private EventSystem   m_pEventSystem = null;
	private StandaloneInputModule m_pStandAloneInputModule = null;


	private Vector3 m_vecScreenSize = Vector3.zero; public Vector3 pScreenSize { get { return m_vecScreenSize; } }
	//-------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pRootCanvas = GetComponent<Canvas>();
		m_pRootCanvasScaler = GetComponent<CanvasScaler>();
		m_pEventSystem = GetComponent<EventSystem>();
		m_pStandAloneInputModule = GetComponent<StandaloneInputModule>();

		PrivMgrUIFrameInitalize();
		PrivMgrUIFrameAttachUICamera();
	}

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
		RectTransform UIScreenSize = transform as RectTransform;
		m_vecScreenSize = UIScreenSize.sizeDelta;
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
				if (pUIFrame.GetUIFrameFocusType() == CManagerUIFrameFocusBase.EUIFrameFocusType.Invisible)
				{
					pUIFrame.SetMonoActive(true);
				}
			}
		}
	}

	protected void ProtMgrUIFrameOverlayCameraStack(Camera pOvelayCamera, bool bAddCamera)
	{
		if (Camera.main == null) return;

		UniversalAdditionalCameraData pUniCameraData = CameraExtensions.GetUniversalAdditionalCameraData(Camera.main);

		if (bAddCamera)
		{
			pUniCameraData.cameraStack.Insert(0, pOvelayCamera);
		}
		else
		{
			pUniCameraData.cameraStack.Remove(pOvelayCamera);
		}

		OnUIMgrOverlayCameraStack(pOvelayCamera, bAddCamera);
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
				ProtMgrUIFrameLoad(pUIFrame, false);
				pUIFrame.ImportUIFrameInitialize();

				if (pUIFrame.GetUIFrameFocusType() == CManagerUIFrameFocusBase.EUIFrameFocusType.Invisible)
				{
					pUIFrame.SetMonoActive(true);
				}
			}
		}

		Dictionary<string, CUIFrameBase>.ValueCollection.Enumerator it = m_mapUIFrameInstance.Values.GetEnumerator();
		while (it.MoveNext())
		{
			it.Current.ImportUIFrameInitializePost();
		}
	}

	private void PrivMgrUIFrameAttachUICamera()
	{
		ProtMgrUIFrameOverlayCameraStack(m_pRootCanvas.worldCamera, true);
	}


	//--------------------------------------------------------------------
	protected virtual void OnUIMgrOverlayCameraStack(Camera pOvelayCamera, bool bAddCamera) { }
}
