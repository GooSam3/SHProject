using GameDB;
using UnityEngine;

/// <summary> Npc </summary>
public class ZPawnNpc : ZPawn, Zero.IInteractable
{
    public override E_UnitType EntityType { get { return E_UnitType.NPC; } }

    public ZPawnDataNpc NpcData { get { return EntityData.To<ZPawnDataNpc>(); } }

    /// <summary> 사망 여부 </summary>
    public override bool IsDead { get { return false; } }

	/// <summary> 모델 및 네임테그 활성화 비활성화 처리 </summary>
	public void SetUseable(bool bUse)
	{
		SetActive(bUse);

		foreach(var col in GetComponentsInChildren<Collider>())
		{
			col.enabled = bUse;
		}
	}

	protected override void OnPostInitializeImpl()
	{
		base.OnPostInitializeImpl();
	}


	#region ========:: Interaction ::========

	/// <remarks>
	/// 상호작용 더 다양하게 필요한 아키텍쳐 변경할 예정.
	/// </remarks>
	public void Interact(object _interactor)
	{
		ZLog.LogWarn(ZLogChannel.Entity, $"NPC[{TableId}] 상호작용 시작 | {_interactor}");

		var npcTable = ToData<ZPawnDataNpc>().TableData;
		if (null == npcTable)
		{
			ZLog.LogError(ZLogChannel.Entity, $"NPC TableID[{TableId}] 정보가 존재하지 않아서 작동불가.");
			return;
		}

		if (npcTable.NPCType != E_NPCType.Normal && _interactor is EntityBase interactorEntity)
		{
			// 상호작용 대상 바라볼 필요가 있다면. 바라보기 (* 상호작용 끝나면 원래 회전값으로 복귀필요)
			LookAt(interactorEntity.transform.position);
		}

		if (InteractNPCQuest(ZGameModeManager.Instance.StageTid, NpcData.TableData.NPCID) == false)
		{
			InteractNPCJopType(npcTable.JobType);
		}
	}

	private bool InteractNPCQuest(uint _stageTID, uint _npcTID)
	{
		bool _npcQuest = false;
		UIFrameQuest frameQuest = UIManager.Instance.Find<UIFrameQuest>();
		if (frameQuest.Show)
		{
			return true;
		}

		if (frameQuest.QuestChecker.EventNPCTalk(_stageTID, _npcTID))
		{
			_npcQuest = true;
		}
		else
		{
			if (frameQuest.CheckQuestNPCAccept(_stageTID, _npcTID))
			{
				UIManager.Instance.Open<UIFrameQuest>((_uiFrameName, questFrame) => { questFrame.DoUIQuestDialogSubOpen(ZGameModeManager.Instance.StageTid, NpcData.TableData.NPCID); });
				_npcQuest = true;
			}
		}

		return _npcQuest;
	}

	private void InteractNPCJopType(E_JobType _jobType)
	{
		// to do : 풀 스크린 열려 있을 경우 UI 겹치는 문제로 리턴(임시 조치)
		if (UIManager.Instance.GetOpenFocusType( CManagerUIFrameFocusBase.E_UICanvas.Back, CManagerUIFrameFocusBase.E_UIFrameFocusAction.FullScreen))
        {
			ZLog.Log( ZLogChannel.System, "풀 스크린 열려 있으므로 UI 겹치는 이슈 예방을 위해 강제 리턴");
			return;
        }

		switch (_jobType)
		{
			case E_JobType.Store:
			case E_JobType.SkillStore:
				UIFrameItemShop shop = UIManager.Instance.Find<UIFrameItemShop>();

				UIFrameItemShop.E_ShopFrameType shopType = UIFrameItemShop.E_ShopFrameType.Item;

				if (_jobType == E_JobType.SkillStore)
				{
					shopType = UIFrameItemShop.E_ShopFrameType.Skill;
				}

				if (shop == null)
				{
					UIManager.Instance.Load<UIFrameItemShop>(nameof(UIFrameItemShop), (_loadName, _loadFrame) => {
						_loadFrame.Init(() => UIManager.Instance.Open<UIFrameItemShop>((str, frame) =>
						{
							frame.SetShopType(shopType);
						}));
					});
				}
				else
				{
					if (!shop.Show)
					{
						UIManager.Instance.Open<UIFrameItemShop>((str, frame) => frame.SetShopType(shopType));

					}
					else
						UIManager.Instance.Close<UIFrameItemShop>();
				}
				break;

			case E_JobType.Storage:

				UIFrameStorage storage = UIManager.Instance.Find<UIFrameStorage>();

				if (storage == null)
				{
					UIManager.Instance.Load<UIFrameStorage>(nameof(UIFrameStorage), (_loadName, _loadFrame) => {
						_loadFrame.Init(() => UIManager.Instance.Open<UIFrameStorage>());
					});
				}
				else
				{
					if (!storage.Show)
					{
						UIManager.Instance.Open<UIFrameStorage>();

					}
					else
						UIManager.Instance.Close<UIFrameStorage>();
				}
				break;
			case E_JobType.Cleric:
				{
					UIManager.Instance.Open<UIEXPRestorePopup>(
						(str, frame) => frame.Init());
				}
				break;
			default:
				{
					//ZLog.LogError(ZLogChannel.Entity, $"NPC TableID[{TableId}] JobType: {npcTable.JobType} 미처리");
				}
				break;
		}
	}


	protected override void SetAttributeType()
    {
        UnitAttributeType = E_UnitAttributeType.None;
    }

    #region :: 리팩토링 예정 ::

    Vector3 OriginRot;
	Vector3 StartRot, DestRot;
	float StartRotTime;
	float ValueRotTime;
	void LookAtMyPawn()
	{
		OriginRot = transform.eulerAngles;

		StartRot = transform.eulerAngles;
		DestRot = Quaternion.LookRotation(ZPawnManager.Instance.MyEntity.transform.position - transform.position, Vector3.up).eulerAngles;

		if (DestRot.y > 180f)
			DestRot.y = DestRot.y - 360f;

		StartRotTime = Time.time;
		ValueRotTime = 0;

		ChangeRotation();
		//transform.LookAt(UIManager.GetMyPawn.transform.position);
	}

	void RestoreRotation()
	{
		StartRot = transform.eulerAngles;
		DestRot = OriginRot;

		if (StartRot.y > 180f)
			StartRot.y = StartRot.y - 360f;

		StartRotTime = Time.time;
		ValueRotTime = 0;

		ChangeRotation();
		//transform.eulerAngles = OriginRot;
	}

	void ChangeRotation()
	{
		ValueRotTime += Time.deltaTime;

		if (ValueRotTime >= .5f)
		{
			transform.eulerAngles = DestRot;
		}
		else
		{
			transform.eulerAngles = Vector3.Lerp(StartRot, DestRot, ((Time.time - StartRotTime) + ValueRotTime / .5f));
			Invoke("ChangeRotation", .01f);
		}
	}
	#endregion

	#endregion
}
