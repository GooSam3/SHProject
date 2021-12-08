using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

abstract public class CManagerCameraBase : CManagerTemplateBase<CManagerCameraBase>
{
	private Dictionary<int, CCameraControllerBase> m_mapCameraInstance = new Dictionary<int, CCameraControllerBase>();
	protected CCameraControllerBase m_pActiveController = null;
	//---------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

		if (m_pActiveController == null)
		{
			PrivCameraControllerFindInstacne();
		}
	}
	//----------------------------------------------------------------
	internal void ImportCameraControllerRegist(CCameraControllerBase pCamera, bool bRegist)
	{
		if (bRegist)
		{
			PrivCameraControllerRegist(pCamera);
		}
		else
		{
			PrivCameraControllerUnRegist(pCamera);
		}
	}

	public void DoCameraOverlayStack(Camera pCameraOverlay, bool bStack)
	{
		if (m_pActiveController)
		{
			m_pActiveController.DoControllerOverlayCameraStack(pCameraOverlay, bStack);
		}
	}

	public CCameraControllerBase GetCameraMain()
	{
		return m_pActiveController;
	}

	//-----------------------------------------------------------------
	private void PrivCameraControllerFindInstacne()
	{
		CCameraControllerBase[] aCamera = FindObjectsOfType<CCameraControllerBase>();
		for (int i = 0; i < aCamera.Length; i++)
		{
			PrivCameraControllerRegist(aCamera[i]);
		}
	}

	private void PrivCameraControllerRegist(CCameraControllerBase pCameraController)
	{
		int CameraID = pCameraController.GetCameraID();
		if (m_mapCameraInstance.ContainsKey(CameraID))
		{
			m_mapCameraInstance[CameraID].ImportControllerDisable();
		}

		if (m_pActiveController == null)
		{
			m_pActiveController = pCameraController;
		}

		m_mapCameraInstance[CameraID] = pCameraController;
		pCameraController.ImportContollerEnable();
	}

	private void PrivCameraControllerUnRegist(CCameraControllerBase pCamera)
	{
		int CameraID = pCamera.GetCameraID();
		if (m_mapCameraInstance.ContainsKey(CameraID))
		{
			m_mapCameraInstance.Remove(CameraID);
		}		
	}
}
