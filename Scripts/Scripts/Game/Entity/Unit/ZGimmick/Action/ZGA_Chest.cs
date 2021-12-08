using UnityEngine;
using System;

public class ZGA_Chest : ZGimmickActionBase
{
	[ReadOnly]
	[SerializeField]
	[Header("상자 번호")]
	private ushort mChestIndex;

	public ushort ChestIndex { get { return mChestIndex; } }

	[SerializeField]
	[Header("기본 이펙트")]
	private GameObject Fx_Idle;

	[SerializeField]
	[Header("오픈 이펙트")]
	private GameObject Fx_Open;

	public bool IsOpened { get; private set; }

	private event Action mEventChestOpen;


	protected override void InitializeImpl()
	{
		//초기화시 ZGimmickManager에 등록되어있는 Gimmick중 Chest를 찾아서 index를 셋팅한다.
		var allGimmicks = ZGimmickManager.Instance.AllGimmick();

		ushort index = 0;
		foreach (var checkGimmick in allGimmicks)
		{
			if (null == checkGimmick || null == checkGimmick.GetComponentInChildren<ZGA_Chest>())
				continue;

			++index;
		}

		mChestIndex = (ushort)(index - 1);
				
		IsOpened = false;

		ZTempleHelper.SetActiveFx(Fx_Idle, true);
		ZTempleHelper.SetActiveFx(Fx_Open, false);

		ZGimmickManager.Instance.AddChest(this);
	}

	public void UpdateChest(bool bOpen)
	{
		// TODO :: 일단 이번 빌드에서 제외
		bOpen = false;
		IsOpened = bOpen;

		if(bOpen)
		{
			Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
			Gimmick.SetEnable(false, InvokeAttributeLevel, true);
		}	

		ZTempleHelper.SetActiveFx(Fx_Idle, false == IsOpened);
		ZTempleHelper.SetActiveFx(Fx_Open, false);
	}

	protected override void InvokeImpl()
	{
		if (true == IsOpened)
			return;

		//TODO :: 일단 이번 빌드에서 제외
		//if(ZWebManager.hasInstance)
		//{
		//    ZWebManager.Instance.WebGame.REQ_TempleGachaItem(ZGameModeManager.Instance.StageTid, ChestIndex, (packet, res) =>               
		//    {
		//        Open();
		//    });
		//}
		//else
		{
			Open();
		}
	}

	private void Open()
	{
		//애니메이션 플레이       
		Gimmick.SetAnimParameter(E_AnimParameter.Start_001);

		ZTempleHelper.SetActiveFx(Fx_Idle, false);
		ZTempleHelper.SetActiveFx(Fx_Open, true);

		IsOpened = true;
		mEventChestOpen?.Invoke();
		mEventChestOpen = null;
	}

	public void DoAddEventChestOpen(Action action)
	{
		DoRemoveEventChestOpen(action);
		mEventChestOpen += action;
	}

	public void DoRemoveEventChestOpen(Action action)
	{
		mEventChestOpen -= action;
	}

	protected override void CancelImpl()
	{
	}
}