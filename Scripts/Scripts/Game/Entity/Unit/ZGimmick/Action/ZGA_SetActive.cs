using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ZGA_SetActiveSetting
{
	[Header("활성화 비활성화 할 object")]
	public List<GameObject> ObjectList;

	public void SetActive(bool bActive)
	{
		foreach(var go in ObjectList)
		{
			ZTempleHelper.SetActiveFx(go, bActive);
		}
	}
}

public class ZGA_SetActive : ZGimmickActionBase
{
	[Header("속성 레벨별로 활성화/비활성화 할 오브젝트들")]
	[SerializeField]
	private List<ZGA_SetActiveSetting> m_listSettings = new List<ZGA_SetActiveSetting>();

	[Header("오브젝트 활성/비활성 시작상태")]
	[SerializeField]
	private bool IsActiveObject = true;

	protected override void InitializeImpl()
	{
		Disable();

		var setting = GetSetting(ref m_listSettings);
		setting?.SetActive(IsActiveObject);
	}

	protected override void InvokeImpl()
	{
		IsActiveObject = !IsActiveObject;
		var setting = GetSetting(ref m_listSettings);
		setting?.SetActive(IsActiveObject);
	}

	protected override void CancelImpl()
	{		

	}

	private void Disable()
	{
		foreach(var setting in m_listSettings)
		{
			setting.SetActive(false);
		}
	}
}