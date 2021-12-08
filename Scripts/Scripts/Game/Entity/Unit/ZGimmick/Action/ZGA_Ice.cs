using UnityEngine;

/// <summary> 물건 옮기기 </summary>
public class ZGA_Ice : ZGimmickActionBase
{
	[Header("얼음 Model")]
	[SerializeField]
	private GameObject IceModel;

	[Header("얼음 충돌처리 발판")]
	[SerializeField]
	private Collider Collider;

	[Header("이펙트 - 얼음 얼기 시작")]
	[SerializeField]
	private GameObject Eff_Start;

	[Header("이펙트 - 얼음 얼고난 이후 Idle")]
	[SerializeField]
	private GameObject Eff_Idle;

	protected override void InitializeImpl()
	{
		base.InitializeImpl();

		ZGimmickManager.Instance.AddIceWater(Gimmick);
		Collider.isTrigger = true;
		IceModel.SetActive(false);
		Eff_Start.SetActive(false);
		Eff_Idle.SetActive(false);
	}

	protected override void InvokeImpl()
	{
		Eff_Start.SetActive(true);
		Eff_Idle.SetActive(true);

		if (null != Collider)
			Collider.isTrigger = false;
	}

	protected override void CancelImpl()
	{

	}

	protected override void DestroyImpl()
	{
		base.DestroyImpl();
	}
}