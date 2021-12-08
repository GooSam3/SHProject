using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine;

abstract public class CCameraControllerBase : CMonoBase
{
	protected Camera m_pCamea = null;	public Camera GetCamera() { return m_pCamea; }
	private UniversalAdditionalCameraData m_pURPCameraData = null;
	private int m_pCameraID = 0; public int GetCameraID() { return m_pCameraID; }
	//------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pCamea = GetComponent<Camera>();
		m_pURPCameraData = GetComponent<UniversalAdditionalCameraData>();
		CManagerCameraBase.Instance?.ImportCameraControllerRegist(this, true);
	}

	private void Update()
	{
		OnUnityUpdate();
	}

	private void LateUpdate()
	{
		OnUnityLateUpdate();
	}

	protected override void OnUnityDestroy()
	{
		base.OnUnityDestroy();
		CManagerCameraBase.Instance?.ImportCameraControllerRegist(this, false);
	}

	//--------------------------------------------------------------------
	internal void ImportControllerDisable()
	{
		SetMonoActive(false);
		OnControllerDisable();
	}

	internal void ImportContollerEnable()
	{
		SetMonoActive(true);
		OnControllerEnable();
	}

	public void DoControllerOverlayCameraStack(Camera pOverlayCamera, bool bStack)
	{		
		if (bStack)
		{
			List<Camera> StackCameraList = new List<Camera>();
			StackCameraList.Add(pOverlayCamera);

			for (int i = 0; i < m_pURPCameraData.cameraStack.Count; i++)
			{
				StackCameraList.Add(m_pURPCameraData.cameraStack[i]);
			}

			m_pURPCameraData.cameraStack.Clear();

			for (int i = 0; i < StackCameraList.Count; i++)
			{
				m_pURPCameraData.cameraStack.Add(StackCameraList[i]);
			}
		}
		else
		{
			m_pURPCameraData.cameraStack.Remove(pOverlayCamera);
		}

		OnControllerOverlayStack(pOverlayCamera, bStack);
	}

	//---------------------------------------------------------------------
	protected virtual void OnUnityUpdate() { }
	protected virtual void OnUnityLateUpdate() { }
	protected virtual void OnControllerEnable() { }
	protected virtual void OnControllerDisable() { }
	protected virtual void OnControllerOverlayStack(Camera pOverlayCamera, bool bStack) { }
}
