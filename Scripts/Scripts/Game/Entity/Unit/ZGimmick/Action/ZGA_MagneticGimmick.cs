using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary> 자석 기믹 </summary>
public class ZGA_MagneticGimmick : ZGimmickActionBase
{
	[Header("자석으로 끌어올때 최상단 위치")]
	[SerializeField]
	private Transform MagneticTopPos;

	[Header("자석이 최대로 들수있는 무게")]
	[SerializeField]
	private float MaxWeight;

	[Header("자석 효과 유지시간")]
	[SerializeField]
	private float Time;

	[Header("당겨지는 시간")]
	[SerializeField]
	private float MoveTime;

	// 현재 들어온 기믹
	ZGimmick EnterGimmick;

	// 유저가 올라와있는지
	private bool isEnterPC = false;

	// 움직이는 상태인지
	private bool isMoving = false;

	// 움직임이 완료됬는지
	private bool isComplete = false;

	private Vector3 EndPos;

	protected override void InitializeImpl()
	{
		ResetDefault();
		StartCoroutine(nameof(LateFixedUpdate));
	}

	protected override void InvokeImpl()
	{
		StartCoroutine(SetActivity());
	}

	protected override void CancelImpl()
	{
		ResetDefault();
		StopAllCoroutines();
	}

	protected override void DestroyImpl()
	{
		StopAllCoroutines();
	}

	/// <summary>
	/// 자석 활성 이후 'Time' 만큼 지나고 비활성
	/// </summary>
	/// <returns></returns>
	IEnumerator SetActivity()
	{
		isMoving = true;
		isComplete = false;
		MoveGimmick();
		yield return new WaitForSeconds(Time);
		isComplete = false;
		isMoving = false;
	}

	private IEnumerator LateFixedUpdate()
	{
		WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
		while (true)
		{
			yield return _instruction;
			if (true == isComplete && true == isMoving && null != EnterGimmick)
				EnterGimmick.transform.position = EndPos;
		}
	}

	/// <summary>
	/// 기믹 움직임
	/// </summary>
	private void MoveGimmick()
	{
		if (null == EnterGimmick)
			return;

		float weight = EnterGimmick.Weight;
		if (true == isEnterPC)
			weight += 5;

		float distancePercent = 1 - (weight / MaxWeight);
		if (distancePercent <= 0)
		{
			isComplete = false;
			isMoving = false;
			StopAllCoroutines();
			return;
		}

		Vector3 vector = MagneticTopPos.transform.position - EnterGimmick.transform.position;
		vector = vector * distancePercent;

		EnterGimmick.transform.DOLocalMove(vector, MoveTime).SetEase(Ease.InOutQuart).SetRelative().OnComplete(() =>
		{
			EndPos = EnterGimmick.transform.position;
			isComplete = true;
		});
	}

	private void OnTriggerEnter(Collider other)
	{
		// 캐릭터가 들어왔는지
		var pc = other.gameObject.GetComponent<ZPawnMyPc>();
		if(null != pc)
		{
			isEnterPC = true;
			return;
		}

		// 이후 들어온 기믹들 처리
		if (null != EnterGimmick)
			return;

		var mGimmick = other.GetComponent<ZGimmick>();
		if (null == mGimmick)
		{
			var mActionBase = other.GetComponent<ZGimmickActionBase>();
			if (null == mActionBase)
				return;
			else
			{
				mGimmick = mActionBase.Gimmick;
				if (null == mGimmick)
					return;
			}
		}

		var mRigidbody = mGimmick.GetComponent<Rigidbody>();
		if (null == mRigidbody)
			return;

		// 철제 재질인지
		if (mGimmick.Meterial != E_TempleGimmickMeterial.Metal)
			return;

		// 물리효과 사용여부
		if (true == mRigidbody.isKinematic)
			return;

		EnterGimmick = mGimmick;
	}

	private void OnTriggerExit(Collider other)
	{
		// 캐릭터가 나갔는지
		var pc = other.gameObject.GetComponent<ZPawnMyPc>();
		if (null != pc)
		{
			isEnterPC = false;
			return;
		}

		// 이후 기믹들 처리
		var mGimmick = other.GetComponent<ZGimmick>();
		if (null == mGimmick)
		{
			var mActionBase = other.GetComponent<ZGimmickActionBase>();
			if (null == mActionBase)
				return;
			else
			{
				mGimmick = mActionBase.Gimmick;
				if (null == mGimmick)
					return;
			}
		}

		if (mGimmick == EnterGimmick)
		{
			EnterGimmick = null;
		}

		CancelImpl();
	}

	private void ResetDefault()
	{
		EnterGimmick = null;
		isEnterPC = false;
		isComplete = false;
		isMoving = false;
	}
}
