using System.Collections;
using UnityEngine;

/// <summary>
/// 1. 드랍 연출 애니 실행
/// 2. 애니 끝나면, Tier 이펙트 표시
/// 3. 일정시간 대기
/// 4. 주인에게 정해진 속도로 날라감.
/// </summary>
public class DropItemComponent : MonoBehaviour
{
	static float DespawnDelay = 2f;
	static float BringSpeed = 35f;

	private GameObject OwnerModel;
	private string mTierEffectPrefabName;
	private GameObject mTierEffectGO;

	private Animation ModelAnim;

	public System.Action OnShowTierEffect;
	public System.Action OnHideTierEffect;
	public System.Action<DropItemComponent> OnRemove;

	private Transform LootingTarget;

	public void SetAndStart(Transform lootingTarget, GameObject createdModel, string tierEffPrefabName)
	{
		LootingTarget = lootingTarget;
		OwnerModel = createdModel;
		mTierEffectPrefabName = tierEffPrefabName;
		
		ModelAnim = OwnerModel.GetComponentInChildren<Animation>();
		if (null != ModelAnim)
		{
			ModelAnim.Rewind();
			ModelAnim.Play();
		}

		float animLength = ModelAnim.clip.length;
		StartCoroutine(StartBringRoutine(animLength));
	}

	private void OnDisable()
	{
		DespawnTierEffect();
	}

	private void DespawnTierEffect()
	{
		if (null != mTierEffectPrefabName)
		{
            if(null != mTierEffectGO)
            {
                ZPoolManager.Instance.Return(mTierEffectGO);
            }
			
			mTierEffectGO = null;
			mTierEffectPrefabName = null;
		}

		OnHideTierEffect?.Invoke();
		OnRemove?.Invoke(this);
	}

	IEnumerator StartBringRoutine(float delay)
	{
		yield return new WaitForSeconds(delay);

		OnShowTierEffect?.Invoke();

		if (null != mTierEffectPrefabName)
		{
			ZPoolManager.Instance.Spawn(E_PoolType.Effect, mTierEffectPrefabName, ( createdTierEffGO) =>
			{
				mTierEffectGO = createdTierEffGO;
				mTierEffectGO.transform.SetParent(this.transform, false);
				mTierEffectGO.transform.localPosition = Vector3.zero;
			});
		}

		yield return new WaitForSeconds(DespawnDelay);

		DespawnTierEffect();

		if (null == LootingTarget)
		{
			ZPoolManager.Instance.Return(this.gameObject);
			yield break;
		}

		Transform destTrans = LootingTarget;

		float addSpeed = 0;
		float remainDist = 10f;
		while (remainDist > 1f)
		{
			if (null == destTrans)
				break;

			Vector3 dest = destTrans.position;
			Vector3 curPos = transform.position;
			Vector3 offset = dest - curPos;
			Vector3 calcVelocity = offset.normalized * (BringSpeed + addSpeed);
			addSpeed += Time.deltaTime;

			// 남은거리
			remainDist = offset.magnitude;
			float velDist = calcVelocity.magnitude * Time.deltaTime;

			if (remainDist <= velDist && remainDist != 0)
			{
				// 남은 이동 거리 계산.
				calcVelocity = offset.normalized * ((remainDist / velDist) * BringSpeed);
			}

			transform.position = curPos + calcVelocity * Time.deltaTime;

			yield return null;
		}
		
		ZPoolManager.Instance.Return(this.gameObject);
	}
}
