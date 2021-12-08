using ParadoxNotion.Services;
using UnityEngine;

public class ZGA_ItemKey : ZGimmickActionBase
{
	private ZGimmick myGimmick;

	// 현제 회전값
	private float rotate_y = 0f;

	protected override void InitializeImpl()
	{
		base.InitializeImpl();
		myGimmick = GetComponent<ZGimmick>();
		MonoManager.current.AddUpdateCall(MonoManager.UpdateMode.NormalUpdate, UpdateRotateKey);
	}

	protected override void CancelImpl()
	{
	}

	protected override void InvokeImpl()
	{
	}

	private void OnDisable()
	{
		MonoManager.current.RemoveUpdateCall(MonoManager.UpdateMode.NormalUpdate, UpdateRotateKey);
	}

	private void OnTriggerEnter(Collider other)
	{
		ZPawnMyPc pc = other.gameObject.GetComponent<ZPawnMyPc>();

		if (null == pc)
			return;

		if (true == ZGimmickManager.Instance.GimmickItems_Key.Contains(myGimmick))
			return;

		ZGimmickManager.Instance.GimmickItems_Key.Add(myGimmick);
		gameObject.SetActive(false);
	}

	private void UpdateRotateKey()
	{
		rotate_y += Time.deltaTime * 5f;
		transform.rotation = Quaternion.Euler(0, rotate_y, 0);
	}
}
