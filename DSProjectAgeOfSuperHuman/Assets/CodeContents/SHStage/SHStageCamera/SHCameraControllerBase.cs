using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHCameraControllerBase : CCameraControllerBase
{
	private bool m_bAttach = false;

	//-------------------------------------------------------
	protected override void OnUnityUpdate()
	{
		base.OnUnityUpdate();
		if (m_bAttach == false)
		{
			if (UIManager.Instance != null)
			{
				Camera pOverlayCamera = UIManager.Instance.GetUIManagerCamara();
				if (pOverlayCamera != null)
				{
					m_bAttach = true;
					DoControllerOverlayCameraStack(UIManager.Instance.GetUIManagerCamara(), true);
				}
			}
		}
	}

}
