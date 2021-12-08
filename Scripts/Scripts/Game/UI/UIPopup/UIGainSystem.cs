using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameDB;
using ZNet.Data;

[Serializable]
public class CGainInfo
{
	public E_GainType gainType;

	public uint tid;
}

public enum E_GainType
{
	CollectionChange,
	CollectionItem,
	CollectionPetRide,
}



public class UIGainSystem : ZUIFrameBase
{
	private const float GAIN_POPUP_INTERVAL = .5f;


	private abstract class PopupGainBase
	{
		public GameObject obj;

		public abstract float PlayTime { get; }
		public abstract void Show();
		public virtual void Hide()
		{
			obj.SetActive(false);
		}
	}


	[Serializable]// 수정될여지 있는관계로 직렬클래스로 생성함
	private class PopupGainCollection : PopupGainBase
	{
		public Animation anim;
		public Text title;
		public Text abilities;
		//낭낭하게 0.5초 더해줌
		public override float PlayTime => anim.clip.length;

		// return : isPlay
		public bool SetInfo(CGainInfo info)
		{
			switch (info.gainType)
			{
				case E_GainType.CollectionChange:
					return SetChangeCollection(info.tid);
				case E_GainType.CollectionItem:
					return SetItemCollection(info.tid);
				case E_GainType.CollectionPetRide:
					return SetPetRideCollection(info.tid);
			}
			return false;
		}

		private bool SetChangeCollection(uint tid)
		{
			if (DBChangeCollect.GetCollection(tid, out var table) == false)
				return false;

			title.text = DBLocale.GetText(table.ChangeCollectionTextID);

			SetAbility(table.AbilityActionID_01, table.AbilityActionID_02);

			return true;
		}

		private bool SetPetRideCollection(uint tid)
		{
			if (DBPetCollect.GetPetRideCollection(tid, out var table) == false)
				return false;

			title.text = DBLocale.GetText(table.PetCollectionTextID);

			SetAbility(table.AbilityActionID_01, table.AbilityActionID_02);

			return true;
		}

		private bool SetItemCollection(uint tid)
		{
			if (DBItemCollect.GetCollection(tid, out var table) == false)
				return false;

			title.text = DBLocale.GetText(table.ItemCollectionTextID);

			SetAbility(table.AbilityActionID_01, table.AbilityActionID_02);

			return true;
		}

		private void SetAbility(params uint[] abilityTid)
		{
			List<UIAbilityData> listAbility = new List<UIAbilityData>();
			foreach (var iter in abilityTid)
			{
				DBAbilityAction.GetAbilityTypeList(iter, ref listAbility);
			}

			listAbility = DBAbilityAction.GetMergedTypeList(listAbility);

			string ability = string.Empty;

			for (int i = 0; i < listAbility.Count; i++)
			{
				ability += $"{DBLocale.GetText(listAbility[i].type.ToString())} {DBAbility.ParseAbilityValue(listAbility[i].type, listAbility[i].value)}";

				if (i == listAbility.Count - 1)
					break;

				ability += "\n";
			}

			abilities.text = ability;
		}

		public override void Show()
		{
			anim.Rewind();
			anim.Play();
			obj.SetActive(true);
		}
	}

	private Queue<CGainInfo> queueInfo = new Queue<CGainInfo>();

	private bool isPlaying = false;// ljh : 현재 연출중입니까?

	private bool isPause = false;// ljh : play보다 먼저검사됨, 일시정지중입니까?

	private CGainInfo curPlayGainInfo;

	[SerializeField]
	private PopupGainCollection popupCollectionGain;

	private PopupGainBase curGainPopup;


	protected override void OnInitialize()
	{
		base.OnInitialize();

		popupCollectionGain.Hide();
	}
	protected override void OnRemove()
	{
		base.OnRemove();

		if (Me.FindCurCharData == null)
			return;

		Me.CurCharData.UpdateCompleteChangeCollect -= OnUpdateChangeCollect;
		Me.CurCharData.UpdateCompleteItemCollect -= OnUpdateItemCollect;
		Me.CurCharData.UpdateCompletePetCollect -= OnUpdatePetRideCollect;
		Me.CurCharData.UpdateCompleteRideCollect -= OnUpdatePetRideCollect;
	}

	// initialize 에서 이벤트 등록안하는 이유 : 캐릭터 생성전 로드함
	// 캐릭터 생성 후 본 메소드 호출
	public void SetEvent()
	{
		Me.CurCharData.UpdateCompleteChangeCollect -= OnUpdateChangeCollect;
		Me.CurCharData.UpdateCompleteItemCollect -= OnUpdateItemCollect;
		Me.CurCharData.UpdateCompletePetCollect -= OnUpdatePetRideCollect;
		Me.CurCharData.UpdateCompleteRideCollect -= OnUpdatePetRideCollect;

		Me.CurCharData.UpdateCompleteChangeCollect += OnUpdateChangeCollect;
		Me.CurCharData.UpdateCompleteItemCollect += OnUpdateItemCollect;
		Me.CurCharData.UpdateCompletePetCollect += OnUpdatePetRideCollect;
		Me.CurCharData.UpdateCompleteRideCollect += OnUpdatePetRideCollect;
	}

	public void AddQueue(CGainInfo info)
	{
		queueInfo.Enqueue(info);

		CheckShowGain();
	}

	protected override void OnHide()
	{
		base.OnHide();

		this.CancelInvoke();

		popupCollectionGain.Hide();
	}

	/// <summary>
	/// ljh : 획득 노티 정지
	/// ex) 뽑기등 쪼는컨텐츠에서 미리컬렉션 완성보여질 여지 있는관계로 정지
	/// false 를 호출했다면 반드시 true를 호출해주세요
	/// </summary>
	/// <param name="state"> 실행여부 </param>
	public void SetPlayState(bool isPlay)
	{
		isPause = !isPlay;

		if (isPlay)
		{
			CheckShowGain();
		}
		else
		{
			isPlaying = false;
			curGainPopup?.Hide();
			CancelInvoke();
		}

	}

	private void CheckShowGain()
	{
		if (isPlaying || isPause)
			return;

		if (queueInfo.Count <= 0)
		{
			UIManager.Instance.Close<UIGainSystem>();
			return;
		}
		else
		{
			CheckOpenSystem();
		}

		curPlayGainInfo = queueInfo.Dequeue();

		PlayGainPopup(curPlayGainInfo);
	}

	private void HidePlayPopup()
	{
		isPlaying = false;

		curGainPopup.Hide();

		this.Invoke(nameof(CheckShowGain), GAIN_POPUP_INTERVAL);
	}

	private void PlayGainPopup(CGainInfo info)
	{
		// ljh : 다른 연출등으로 일시정지 상태라면 리턴~
		if (isPause)
			return;

		// 플레이시 레이어 가장위로 올림 : 팝업, 기타 ui생성으로 인해 오더 뒤로 밀렸을시 대응
		// ljh : ui 레이어 정리되면 system 쪽으로 이동 후 아래topmost삭제
		UIManager.Instance.TopMost<UIGainSystem>(true, false);

		switch (info.gainType)
		{
			case E_GainType.CollectionChange:
			case E_GainType.CollectionItem:
			case E_GainType.CollectionPetRide:
				if (popupCollectionGain.SetInfo(info) == false)
				{
					CheckShowGain();
					return;
				}
				curGainPopup = popupCollectionGain;
				popupCollectionGain.Show();

				this.Invoke(nameof(HidePlayPopup), popupCollectionGain.PlayTime);
				break;
			default:
				return;
		}

		isPlaying = true;
	}

	private void CheckOpenSystem()
	{
		if (this.gameObject.activeSelf)
			return;

		UIManager.Instance.Open<UIGainSystem>();
	}

	private void OnUpdatePetRideCollect(uint _tid)
	{
		CheckOpenSystem();
		AddQueue(new CGainInfo() { gainType = E_GainType.CollectionPetRide, tid = _tid });
	}

	private void OnUpdateChangeCollect(uint _tid)
	{
		CheckOpenSystem();
		AddQueue(new CGainInfo() { gainType = E_GainType.CollectionChange, tid = _tid });
	}

	private void OnUpdateItemCollect(uint _tid)
	{
		CheckOpenSystem();
		AddQueue(new CGainInfo() { gainType = E_GainType.CollectionItem, tid = _tid });
	}
}
