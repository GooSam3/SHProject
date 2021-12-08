using System;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UISubHUDGodLandItem : MonoBehaviour
{
	[SerializeField] private ZText nickName;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZText hpText;
	[SerializeField] private ZText mpText;
	[SerializeField] private ZText atkText;
	[SerializeField] private ZText defText;
	[SerializeField] private ZText mdefText;
	[SerializeField] private Slider hpSlider;
	[SerializeField] private Slider mpSlider;

	private uint entityId = 0;

	public void Initialize(GodLandStatInfoConverted _userInfo)
	{
		this.gameObject.SetActive(true);

		entityId = _userInfo.ObjectID;

		levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), _userInfo.Level);
		atkText.text = $"{ _userInfo.Attack}";
		defText.text = $"{ _userInfo.MeleeDefence}";
		mdefText.text = $"{ _userInfo.MagicDefence}";

		nickName.text = string.Empty;
		hpText.text = string.Empty;
		mpText.text = string.Empty;
		hpSlider.value = 1;
		mpSlider.value = 1;

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

		entityId = 0;
	}

	public void AddEvent()
	{
		var entity = ZPawnManager.Instance.GetEntity(entityId);
		if (entity != null) {
			DoAddEventCreateEntity(entityId, entity);
		}
		else {
			ZPawnManager.Instance.DoAddEventCreateEntity(DoAddEventCreateEntity);
		}
	}

	private void DoAddEventCreateEntity(uint _entityId, ZPawn pawn)
	{
		if (pawn != null && _entityId == entityId) {
			ZPawnManager.Instance.DoRemoveEventCreateEntity(DoAddEventCreateEntity);
			SetPawn(pawn);
		}
	}

	private void SetPawn(ZPawn pawn)
	{
		pawn.DoAddEventHpUpdated(EventUpdateHp);
		pawn.DoAddEventMpUpdated(EventUpdateMp);
		EventUpdateHp(pawn.CurrentHp, pawn.MaxHp);
		EventUpdateMp(pawn.CurrentMp, pawn.MaxMp);

		nickName.text = pawn.PawnData.Name;
	}

	private void EventUpdateHp(float cur, float max)
	{
		hpSlider.value = cur / max;
		hpText.text = $"{Mathf.RoundToInt(cur)}/{Mathf.RoundToInt(max)}";
	}

	private void EventUpdateMp(float cur, float max)
	{
		mpSlider.value = cur / max;
		mpText.text = $"{Mathf.RoundToInt(cur)}/{Mathf.RoundToInt(max)}";
	}

	private void RemoveEvent()
	{
		ZPawnManager.Instance.DoRemoveEventCreateEntity(DoAddEventCreateEntity);

		var entity = ZPawnManager.Instance.GetEntity(entityId);
		if (entity != null) {
			entity.DoRemoveEventHpUpdated(EventUpdateHp);
			entity.DoRemoveEventMpUpdated(EventUpdateMp);
		}
	}
}