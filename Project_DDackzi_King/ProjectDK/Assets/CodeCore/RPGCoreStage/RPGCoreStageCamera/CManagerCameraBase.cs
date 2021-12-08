using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

abstract public class CManagerCameraBase : CManagerTemplateBase<CManagerCameraBase>
{
	private Dictionary<int, CCameraControllerBase> m_mapCameraInstance = new Dictionary<int, CCameraControllerBase>();
	protected CCameraControllerBase mActiveController = null;
	//---------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

		if (mActiveController == null)
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
		if (mActiveController)
		{
			mActiveController.ImportControllerOverlayCameraStack(pCameraOverlay, bStack);
		}
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

		if (mActiveController == null)
		{
			mActiveController = pCameraController;
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
