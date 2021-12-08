using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class ZGA_KeyDoor : ZGimmickActionBase
{
	[Header("열쇠 GimmickID")]
	[SerializeField]
	private string KeyId;

	protected override void InitializeImpl()
	{
		base.InitializeImpl();
	}

	protected override void InvokeImpl()
	{
		if (KeyId.IsNullOrEmpty())
			return;

		var findKey = ZGimmickManager.Instance.GimmickItems_Key.Find(d => d.GimmickId == KeyId);
		if (null == findKey)
			return;

		ZGimmickManager.Instance.GimmickItems_Key.Remove(findKey);
		Gimmick.SetAnimParameter(E_AnimParameter.Start_001);

		enabled = false;
	}

	protected override void CancelImpl()
	{
	}
}
