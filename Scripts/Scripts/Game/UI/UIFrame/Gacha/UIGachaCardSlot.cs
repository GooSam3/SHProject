using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class UIGachaCardSlot : MonoBehaviour, IPointerDownHandler
{
	private readonly Vector3 ROT_ORIGIN = new Vector3(0f, 90f, 0f);
	private readonly Vector3 ROT_TURN = new Vector3(0f, 180f, 0f);

	private const string KEY_TURN = "Start_001";
	private const string KEY_END = "End_001";

	private const string FX_FORMAT_LOOP_BACK = "Fx_Ui_CardGrade_Loop0{0}";
	private const string FX_FORMAT_LOOP_FRONT = "Fx_Ui_CardGrade_Ripp0{0}";
	private const string FX_FORMAT_ONTURN = "Fx_Ui_CardGrade_0{0}";

	[SerializeField] private Animator anim;

	[SerializeField] private Image gradeBG;
	[SerializeField] private Image charIcon;
	[SerializeField] private Text charName;

	[SerializeField] private Image classIcon;
	[SerializeField] private Image attributeIcon;

	[SerializeField] private Transform fxRoot;

	[SerializeField] private Animation animOnTurn;

	[SerializeField, Header("Used in GoldCard Only")] private UIFxParticle fxGoldCard;

	[SerializeField] private SkinnedMeshRenderer meshRenderer;
	[SerializeField] private Transform uiRoot;
	[SerializeField] private Transform tagRoot;

	private ParticleSystem fxOnTurn;
	private ParticleSystem fxBackLoop;
	private ParticleSystem fxFrontLoop;

	public bool TurnState { get; private set; } = false;

	public UIGachaEnum.E_GachaStyle SlotType { get; private set; } = UIGachaEnum.E_GachaStyle.None;

	public byte Grade { get; private set; }

	public uint Tid { get; private set; }

	private bool isReady = false;

	private bool isGoldCard = false;

	private Action onTurn;

	private Action onGoldCardClick;

	private bool reserveFxTurn = false;
	private bool reserveBackLoop = false;
	private bool reserveFrontLoop = false;

	public void SetReady()
	{
		isReady = true;
	}
	private void OnEnable()
	{
		if (anim != null)
		{
			if (TurnState)
			{
				anim.Play(KEY_TURN, 0, 1f);

				ForceSwitchCardUI(true);

				animOnTurn.Play();
			}
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"></param>
	/// <param name="tid"></param>
	/// <param name="_onTurn"></param>
	/// <param name="_isGoldCard">골드카드입니까?</param>
	/// <param name="_onGoldClick">**골드카드에서만 사용, 골드카드 클릭시</param>
	public void SetSlotData(UIGachaEnum.E_GachaStyle type, uint tid, Action _onTurn, bool _isGoldCard = false, Action _onGoldClick = null, bool isTurned = false)
	{
		isGoldCard = _isGoldCard;
		onTurn = _onTurn;

		onGoldCardClick = _onGoldClick;

		SlotType = type;

		Tid = tid;

		reserveBackLoop = true;

		switch (type)
		{
			case UIGachaEnum.E_GachaStyle.None:
				return;
			case UIGachaEnum.E_GachaStyle.Pet:
			case UIGachaEnum.E_GachaStyle.Ride:
				SetPetData(tid);
				break;
			case UIGachaEnum.E_GachaStyle.Class:
				SetChangeData(tid);
				break;
			case UIGachaEnum.E_GachaStyle.Item:
				SetItemData(tid);
				break;
		}

		if (isTurned)
		{
			SetReady();
			SetTurn(true, true);
		}

	}

	public void Clear()
	{
		anim.Rebind();
		animOnTurn.Rewind();
		ForceSwitchCardUI(false);
		
		TurnState = false;
		isReady = false;

		SlotType = UIGachaEnum.E_GachaStyle.None;

		if (fxOnTurn != null)
		{
			Addressables.ReleaseInstance(fxOnTurn.gameObject);
			fxOnTurn = null;
			reserveFxTurn = false;
		}
		if (fxBackLoop != null)
		{
			Addressables.ReleaseInstance(fxBackLoop.gameObject);
			fxBackLoop = null;
			reserveBackLoop = false;
		}
		if (fxFrontLoop != null)
		{
			Addressables.ReleaseInstance(fxFrontLoop.gameObject);
			fxFrontLoop = null;
			reserveFrontLoop = false;
		}
	}

	public void SetTurn(bool state, bool direct = false)
	{
		if (isReady == false)
			return;

		//중복값 흘려줌
		if (TurnState == state)
			return;

		TurnState = state;

		// 뒤집혀있는놈은 스킵
		if (TurnState == false)
		{
			ForceSwitchCardUI(false);
			anim.SetTrigger(KEY_END);

			fxBackLoop?.gameObject.SetActive(true);
			reserveBackLoop = true;
			fxOnTurn?.gameObject.SetActive(false);
			reserveFxTurn = false;
			fxFrontLoop?.gameObject.SetActive(false);
			reserveFrontLoop = false;
			return;
		}

		if (isGoldCard == false && Grade >= ZUIConstant.RARE_SEQUENCE_GRADE)
		{
			if (UIManager.Instance.Find(out UIFrameGacha _gacha))
			{
				if (!_gacha.SkipMode)
				{
					switch (_gacha.CurrentGachaStyle)
					{
						case UIGachaEnum.E_GachaStyle.Pet:
						case UIGachaEnum.E_GachaStyle.Ride:
							{
								_gacha.SetIsHidden(true);
								_gacha.CardLinker.SetState(false);

								_gacha.CardLinker.SetActiveGoldCard(_gacha.CurrentGachaStyle, Tid);
								_gacha.GachaVideo.StartPlayVideo();

								onTurn?.Invoke();

								return;
							}

						case UIGachaEnum.E_GachaStyle.Item:
							{
								_gacha.SetIsHidden(true);
								_gacha.CardLinker.SetState(false);

								_gacha.CardLinker.SetActiveGoldCard(_gacha.CurrentGachaStyle, Tid);
								_gacha.GachaVideo.PlayVideo(_gacha.GachaVideo.NextVideo);

								onTurn?.Invoke();

								return;
							}
					}
				}
			}
		}

		fxBackLoop?.gameObject.SetActive(false);

		if (direct == false)
		{
			anim.SetTrigger(KEY_TURN);
			var clipLength = (anim.runtimeAnimatorController as AnimatorOverrideController)[KEY_TURN].length;

			animOnTurn.Play();
			Invoke(nameof(SwitchCardUI), .03f);
			Invoke(nameof(OnTurn), clipLength);
		}
		else
		{
			anim.Play(KEY_TURN, 0, 1f);

			ForceSwitchCardUI(true);
			animOnTurn.Play();
			OnTurn();
		}

	}

	private void SwitchCardUI()
	{
		meshRenderer.enabled = false;
		uiRoot.DOLocalRotate(ROT_TURN, .2f);
	}

	private void ForceSwitchCardUI(bool isTurn)
	{
		meshRenderer.enabled = !isTurn;
		uiRoot.transform.localRotation = isTurn ? Quaternion.Euler(ROT_TURN) : Quaternion.Euler(ROT_ORIGIN);
	}

	private void OnTurn()
	{
		fxOnTurn?.gameObject.SetActive(true);
		fxOnTurn?.Play();
		reserveFxTurn = true;
		fxFrontLoop?.gameObject.SetActive(true);
		fxFrontLoop?.Play();
		reserveFrontLoop = true;

		onTurn?.Invoke();
	}


	public void SetState(bool viewState)
	{
		if (gameObject.activeSelf && TurnState)
		{
			anim.Play(KEY_TURN, 0, 1f);
			ForceSwitchCardUI(true);
			animOnTurn.Play();
		}

		this.gameObject.SetActive(viewState);
	}

	public bool IsRare()
	{
		return Grade >= ZUIConstant.RARE_SEQUENCE_GRADE;
	}

	private void SetItemData(uint tid)
	{
		if (DBItem.GetItem(tid, out var table) == false)
			return;

		Grade = table.Grade;

		gradeBG.sprite = UICommon.GetGradeSprite(table.Grade);
		charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);
		charName.text = UICommon.GetItemText(table);

		classIcon.gameObject.SetActive(true);
		classIcon.sprite = UICommon.GetClassIconSprite(table.UseCharacterType);

		attributeIcon.gameObject.SetActive(false);

		LoadFX(table.Grade);
	}

	private void SetChangeData(uint tid)
	{
		if (DBChange.TryGet(tid, out var table) == false)
			return;

		Grade = table.Grade;

		gradeBG.sprite = UICommon.GetGradeSprite(table.Grade);
		charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
		charName.text = DBChange.GetChangeFullName(table.ChangeID);

		classIcon.gameObject.SetActive(true);
		classIcon.sprite = UICommon.GetClassIconSprite(table.UseAttackType);

		attributeIcon.gameObject.SetActive(true);
		attributeIcon.sprite = UICommon.GetAttributeSprite(table.AttributeType);

		LoadFX(table.Grade);
	}

	private void SetPetData(uint tid)
	{
		if (DBPet.TryGet(tid, out var table) == false)
			return;

		Grade = table.Grade;

		gradeBG.sprite = UICommon.GetGradeSprite(table.Grade);
		charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
		charName.text = DBPet.GetPetFullName(table.PetID);

		classIcon.gameObject.SetActive(false);
		attributeIcon.gameObject.SetActive(false);

		LoadFX(table.Grade);
	}

	// public : test
	public void LoadFX(byte grade)
	{
		if (grade <= 0) return;

		Addressables.InstantiateAsync(string.Format(FX_FORMAT_LOOP_BACK, grade)).Completed += (obj) =>
		{
			if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				var fx = obj.Result.GetComponent<ParticleSystem>();
				AttachParticleSystem(ref fx);

				fxBackLoop = fx;

				fxBackLoop.gameObject.SetActive(reserveBackLoop);
			}
		};
		Addressables.InstantiateAsync(string.Format(FX_FORMAT_LOOP_FRONT, grade)).Completed += (obj) =>
		{
			if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				var fx = obj.Result.GetComponent<ParticleSystem>();
				AttachParticleSystem(ref fx);

				fxFrontLoop = fx;
				fxFrontLoop.gameObject.SetActive(reserveFrontLoop);
			}
		};
		Addressables.InstantiateAsync(string.Format(FX_FORMAT_ONTURN, grade)).Completed += (obj) =>
		{
			if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
			{
				var fx = obj.Result.GetComponent<ParticleSystem>();
				AttachParticleSystem(ref fx);

				fxOnTurn = fx;
				fxOnTurn.gameObject.SetActive(reserveFxTurn);
			}
		};
	}

	public void AttachParticleSystem(ref ParticleSystem ps)
	{
		ps.transform.SetParent(fxRoot);
		ps.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, Vector3.one);
		ps.gameObject.SetLayersRecursively("UIFront");
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (isGoldCard)
		{
			fxGoldCard.Play();

			Invoke(nameof(OnEndGoldCardFx), fxGoldCard.duration);
		}
		else
		{
			SetTurn(true);
		}
	}

	private void OnEndGoldCardFx()
	{
		onGoldCardClick?.Invoke();
	}

#if UNITY_EDITOR
	private void Reset()
	{

	}
#endif
}
