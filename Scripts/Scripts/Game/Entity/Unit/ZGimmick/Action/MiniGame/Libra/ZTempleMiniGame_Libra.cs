using System.Collections;
using UnityEngine;

/// <summary>
/// 천칭 미니게임
/// </summary>
public class ZTempleMiniGame_Libra : ZTempleMiniGameBase
{
	public override E_TempleUIType ControlType => E_TempleUIType.None;

	[Header("무게 감지 자식 천칭 객체 1")]
	[SerializeField]
	private ZTempleMiniGame_LibraHandle HandleFirst;

	[Header("무게 감지 자식 천칭 객체 2")]
	[SerializeField]
	private ZTempleMiniGame_LibraHandle HandleSecond;

	ZDirty UpdateDirty { get; set; }

	static protected int HitCheckLayerMask = UnityConstants.Layers.OnlyIncluding(UnityConstants.Layers.MiniGameObject);

	// 현제 들어온 hit Object
	private RaycastHit hitObj;

	// 선택중인 코인
	private ZTempleMiniGame_LibraCoin currentCoin;

	// 첫번쨰 저울 무게
	private float FirstWeight;

	// 두번째 저울 무게
	private float SecondWeight;

	// 저울 AniNormal
	private float aniNormal;

	/// <summary>
	/// 이니셜라이즈
	/// </summary>
	protected override void InitializeImpl()
	{
		base.InitializeImpl();

		HandleFirst.SetBalanceHandle(this, E_Balance.First);
		HandleSecond.SetBalanceHandle(this, E_Balance.Second);

		UpdateDirty = new ZDirty(1f);
		UpdateDirty.CurrentValue = 0.5f;
	}

	/// <summary>
	/// 미니게임 스타트
	/// </summary>
	protected override void InvokeImpl()
	{
		base.InvokeImpl();

		gameObject.SetLayersRecursively(UnityConstants.Layers.MiniGameObject);

		Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
		Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 0f);
		Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, UpdateDirty.CurrentValue);

		StartCoroutine(LateFixedUpdate());

		ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, LibraUpdate);
		ZPawnManager.Instance.MyEntity.ChangeController<EntityComponentController_MiniGameLibra>();
	}

	/// <summary>
	/// InvokeImpl -> 이후 스타트 들어옴
	/// </summary>
	protected override void StartMiniGame() { }

	public float coinPos;
	public Vector3 rayDirection;
	
	/// <summary>
	/// Update
	/// </summary>
	private void LibraUpdate()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		rayDirection = ray.direction;

		if (Input.GetMouseButtonDown(0))
		{
			//Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 5);

			RaycastHit[] hits = Physics.RaycastAll(ray, 10f, HitCheckLayerMask);
			
			if (null != hits)
			{
				foreach (var hit in hits)
				{
					if (hit.collider.isTrigger)
						continue;

					var Coin = hit.transform.GetComponent<ZTempleMiniGame_LibraCoin>();

					// 이미 선택중인 코인이 있다.
					if (null != currentCoin)
					{
						// 핸들을 선택한경우
						var Handle = hit.transform.GetComponent<ZTempleMiniGame_LibraHandle>();
						if(null != Handle)
						{
							Handle.AddGimmick(currentCoin.myGimmick);
							currentCoin.SetSelectEffect(false);
							currentCoin = null;
						}
						// 그 외에 다른곳을 선택한경우 
						else
						{
							DownCoin();
						}
						break;
					}
					// 선택중인 코인이 없다.
					else
					{
						if (null != Coin)
						{
							currentCoin = Coin;
							CarryCoin();
							break;
						}

						var libraSwitch = hit.transform.GetComponent<ZTempleMiniGame_LibraSwitch>();
						if(null != libraSwitch)
						{
							libraSwitch.SwitchOn();
						}

						break;
					}
				}
			}
		}
	}



	public void SetWeight(E_Balance eBalance, float mWeight)
	{
		switch (eBalance)
		{
			case E_Balance.First: FirstWeight = mWeight; break;
			case E_Balance.Second: SecondWeight = mWeight; break;
		}

		aniNormal = 0;

		// 양팔의 무게가 같은경우 중앙
		if (FirstWeight == SecondWeight)
			aniNormal = 0.5f;
		else
		{
			// 왼쪽이 0 이면 애니메이션은 무조건 오른쪽으로
			if (0 == FirstWeight)
			{
				aniNormal = 1;
			}
			// 오른쪽이 0 이면 애니메이션은 왼쪽으로
			else if (0 == SecondWeight)
			{
				aniNormal = 0;
			}
			else
			{
				aniNormal = FirstWeight / SecondWeight * 0.5f;
			}
		}
	}

	/// <summary>
	/// 동전 옮기기 시작
	/// </summary>
	private void CarryCoin()
	{
		if (null == currentCoin)
			return;

		Debug.Log("Coin Carry");
		currentCoin.SetSelectEffect(true);
	}

	/// <summary>
	/// 동전 선택 취소
	/// </summary>
	private void DownCoin()
	{
		if (null == currentCoin)
			return;

		Debug.Log("Coin Drop");
		currentCoin.SetSelectEffect(false);
		currentCoin = null;
	}

	/// <summary>
	/// 무게 체크 시작! 
	/// </summary>
	public void WeightCheckStart()
	{
		UpdateDirty.GoalValue = aniNormal;
		UpdateDirty.IsDirty = true;
	}

	/// <summary>
	/// Libra Update
	/// </summary>
	private IEnumerator LateFixedUpdate()
	{
		WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
		while (true)
		{
			yield return _instruction;

			if (UpdateDirty.IsDirty)
			{
				UpdateDirty.Update();
			}

			Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, UpdateDirty.CurrentValue);
		}
	}

	/// <summary>
	/// 미니게임 직접 취소
	/// </summary>
	protected override void CancelImpl()
	{
		base.CancelImpl();
		StopAllCoroutines();
		ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, LibraUpdate);
	}

	/// <summary>
	/// 미니게임 완료 체크
	/// </summary>
	/// <returns></returns>
	protected override bool CheckCompleteImpl()
	{
		return false;
	}

	/// <summary>
	/// 무엇으로 종료를하든 미니게임이 종료되기 전에 무조건 들어옴
	/// </summary>
	protected override void EndMiniGame()
	{
	}

	protected override void DestroyImpl()
	{
		base.DestroyImpl();
		StopAllCoroutines();
		ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, LibraUpdate);
	}
}
