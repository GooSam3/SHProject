using System;
using UnityEngine;
using ZNet.Data;

public class UISubHUDColosseumCharItem : MonoBehaviour
{
	[SerializeField] private ZText indexText;
	[SerializeField] private ZImage gradeImage;
	[SerializeField] private ZText nickName;
	[SerializeField] private GameObject myCheckerObj;
	[SerializeField] private GameObject deadMark;

	private UnityEngine.UI.Slider hpSlider;
	private ulong roomUserCharId = 0;

	public void Initialize(ColosseumRoomUserConverted _roomUser)
	{
		this.gameObject.SetActive(true);

		roomUserCharId = _roomUser.CharId;

		hpSlider = GetComponentInChildren<UnityEngine.UI.Slider>(true);

		indexText.text = $"{_roomUser.TeamOrder + 1}";

		var colosseumTable = DBColosseum.Get(_roomUser.Grade);
		gradeImage.sprite = ZManagerUIPreset.Instance.GetSprite(colosseumTable.GradeIcon);

		nickName.text = _roomUser.Name;

		myCheckerObj.SetActive(_roomUser.CharId == Me.CharID);

		EventUpdateHp(0, 1);
	}

	public void Show()
	{
		this.gameObject.SetActive(true);

		AddEvent();
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);

		RemoveEvent();

		roomUserCharId = 0;
	}

	public void AddEvent()
	{
		var entityData = ZPawnManager.Instance.FindEntityDataByCharID(roomUserCharId);
		if (entityData != null) {
			var entity = ZPawnManager.Instance.GetEntity(entityData.EntityId);
			DoAddEventCreateEntity(entityData.EntityId, entity);
		}
		else {
			ZPawnManager.Instance.DoAddEventCreateEntity(DoAddEventCreateEntity);
		}
	}

	private void DoAddEventCreateEntity(uint entityId, ZPawn pawn)
	{
		if (pawn != null && roomUserCharId == pawn.EntityData.CharacterId) {
			ZPawnManager.Instance.DoRemoveEventCreateEntity(DoAddEventCreateEntity);
			SetPawn(pawn);
		}
	}

	private void SetPawn(ZPawn pawn)
	{
		if (null != pawn) {
			pawn.DoAddEventHpUpdated(EventUpdateHp);
			EventUpdateHp(pawn.CurrentHp, pawn.MaxHp);
		}
	}

	private void EventUpdateHp(float cur, float max)
	{
		hpSlider.value = cur / max;
		deadMark.SetActive(hpSlider.value == 0);
	}

	private void RemoveEvent()
	{
		ZPawnManager.Instance.DoRemoveEventCreateEntity(DoAddEventCreateEntity);

		var data = ZPawnManager.Instance.FindEntityDataByCharID(roomUserCharId);
		if (data != null) {
			var entity = ZPawnManager.Instance.GetEntity(data.EntityId);
			if (entity != null) {
				entity.DoRemoveEventHpUpdated(EventUpdateHp);
			}
		}
	}
}
