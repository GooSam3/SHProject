using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;

public class UIPopupEventReward : ZUIFrameBase
{
	[Serializable] // 아이템의 수량은 표기하지 않는다. >> 기획사항
	private class SimpleItemInfo
	{
		public Image imgGrade;
		public Image imgIcon;
		public Text txtItemName;

		public GameObject objSlot;

		public void Set(uint tid)
		{
			objSlot.SetActive(tid > 0);

			if (tid <= 0)
				return;

			// 보상 잘못들어올수있으니 초기화 제대로함
			imgGrade.sprite = null;
			imgIcon.sprite = null;
			txtItemName.text = string.Empty;

			if (DBItem.GetItem(tid, out var itemTable) == false)
			{
				ZLog.LogError(ZLogChannel.Event, $"점검 이벤트 보상 잘못설정됨~~~, tid : {tid}");
				return;
			}

			imgGrade.sprite = UICommon.GetGradeSprite(itemTable.Grade);
			imgIcon.sprite = UICommon.GetSprite(itemTable.IconID);
			txtItemName.text = DBUIResouce.GetItemGradeFormat(DBLocale.GetText(itemTable.ItemTextID), itemTable.Grade);
		}
	}

	[SerializeField] private RawImage imgBG;

	[SerializeField] private Text txtTitle;

	[SerializeField] List<SimpleItemInfo> listItem = new List<SimpleItemInfo>();

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		var data = Me.CurUserData.GetLoginEvent();

		if (data.isHaveMail == false || data.reward_Item.Count<=0)
		{
			OnClickClose();
			return;
		}

		for (int i = 0; i < listItem.Count; i++)
		{
			if (data.reward_Item.Count <= i)
			{
				listItem[i].Set(0);
				continue;
			}

			listItem[i].Set(data.reward_Item[i]);
		}

		txtTitle.text = data.title;
		LoadBGTexture(data.bgUrl, data.bgHash);
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	private void LoadBGTexture(string url, string hash)
	{
		if (string.IsNullOrEmpty(url))
		{
			ZLog.Log(ZLogChannel.Event, $"BgUrl is Empty!! this type : {nameof(UIPopupEventReward)}");
			imgBG.texture = null;
		}
		else
		{
			ZResourceManager.Instance.GetTexture2DFromUrl(url, hash,
								(tex) => imgBG.texture = tex, $"{nameof(E_ServerEventCategory)}_{nameof(UIPopupEventReward)}");
		}
	}

	public void OnClickClose()
	{
		UIManager.Instance.Close<UIPopupEventReward>();
	}

	public void OnClickReward()
	{
		OnClickClose();

		UIManager.Instance.Open<UIFrameMailbox>();
		// -->> 메일로이동
	}
}
