using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine;

abstract public class CCameraControllerBase : CMonoBase
{
	protected Camera mReferenceCamea = null;
	private int mCameraID = 0; public int GetCameraID() { return mCameraID; }
	//------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

		if (CManagerCameraBase.Instance != null)
		{
			CManagerCameraBase.Instance.ImportCameraControllerRegist(this, true);
		}
	}

	private void LateUpdate()
	{
		OnUnityUpdate();
	}

	protected override void OnUnityDestroy()
	{
		base.OnUnityDestroy();
		CManagerCameraBase.Instance.ImportCameraControllerRegist(this, false);
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

	internal void ImportControllerOverlayCameraStack(Camera pOverlayCamera, bool bStack)
	{		
		UniversalAdditionalCameraData UniCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(mReferenceCamea);
		if (bStack)
		{
			List<Camera> StackCameraList = new List<Camera>();
			StackCameraList.Add(pOverlayCamera);

			for (int i = 0; i < UniCamaraData.cameraStack.Count; i++)
			{
				StackCameraList.Add(UniCamaraData.cameraStack[i]);
			}

			UniCamaraData.cameraStack.Clear();

			for (int i = 0; i < StackCameraList.Count; i++)
			{
				UniCamaraData.cameraStack.Add(StackCameraList[i]);
			}
		}
		else
		{
			UniCamaraData.cameraStack.Remove(pOverlayCamera);
		}

		OnControllerOverlayStack(pOverlayCamera, bStack);
	}

	//---------------------------------------------------------------------
	protected virtual void OnUnityUpdate() { }
	protected virtual void OnControllerEnable() { }
	protected virtual void OnControllerDisable() { }
	protected virtual void OnControllerOverlayStack(Camera pOverlayCamera, bool bStack) { }
}
