using UnityEngine;

public class ZGA_Destroy : ZGimmickActionBase
{
	[Header("발동할 이펙트")]
	[SerializeField]
	private uint Fx_Destroy;

	[Header("오브젝트가 사라지는 시간")]
	[SerializeField]
	private float DissolveTime = 0f;

	[Header("오브젝트가 사라지는 대기 시간")]
	[SerializeField]
	private float DissolveDelayTime = 0f;

	//[Header("오브젝트가 제거되는 시간")]
	//[SerializeField]
	//private float DestroyDelayTime = 0f;

	private ZEffectComponent EffectComp = null;

	protected override void InvokeImpl()
	{
		Invoke(nameof(InvokeSpawnEffect), DissolveDelayTime);

		Gimmick.DestroyGimmick(DissolveTime, DissolveDelayTime, DestryGimmick);

		Gimmick.SetGimmickState(E_TempleGimmickState.Die, true);
	}

	protected override void CancelImpl()
	{
		CancelInvoke(nameof(InvokeSpawnEffect));
		DestryGimmick();
	}

	protected override void DestroyImpl()
	{
		CancelInvoke(nameof(InvokeSpawnEffect));
		DestryGimmick();
	}

	private void InvokeSpawnEffect()
	{
		CancelInvoke(nameof(InvokeSpawnEffect));

		ZEffectManager.Instance.SpawnEffect(Fx_Destroy, Gimmick.transform, 0f, 1f, (comp) =>
		{
			if (null != EffectComp && EffectComp.gameObject.activeSelf)
			{
				EffectComp.Despawn();
			}

			EffectComp = comp;
		});
	}

	private void DestryGimmick()
	{
		if (null != EffectComp && EffectComp.gameObject.activeSelf)
		{
			EffectComp.Despawn();
		}

		EffectComp = null;
	}
}