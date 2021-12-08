using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEventViewHolder : ZAdapterHolderBase<OSA_UIEventData>
{
	private UIEventViewListItem listItem;
	private Action<OSA_UIEventData> onClick;

	public override void SetSlot(OSA_UIEventData data)
	{
		listItem.SetSlot(data);
	}

	public override void CollectViews()
	{
		base.CollectViews();

		listItem = root.GetComponent<UIEventViewListItem>();
	}

	public void SetAction(Action<OSA_UIEventData> _onClick)
	{
		onClick = _onClick;
		listItem.SetAction(onClick);
	}

}

public class UIEventViewListItem : MonoBehaviour
{
	[SerializeField] private Image imgIconOn;
	[SerializeField] private Image imgIconOff;

	[SerializeField] private Text txtTitle;

	[SerializeField] private ZToggle toggle;

	private Action<OSA_UIEventData> onClick;

	public OSA_UIEventData Data { get; private set; }

	public void SetSlot(OSA_UIEventData _data)
	{
		Data = _data;

		txtTitle.text = Data.eventData.title;

		string iconID = string.Empty;

		DBIngameEvent.GetEventIcon(_data.eventData.SubCategory, out iconID);

		var icon = string.IsNullOrEmpty(iconID) ? null : UICommon.GetSprite(iconID);

		imgIconOn.sprite = icon;
		imgIconOff.sprite = icon;

		toggle.SelectToggleSingle(Data.isSelected, false);
	}

	public void SetAction(Action<OSA_UIEventData> _onClick)
	{
		onClick = _onClick;
	}

	public void OnClickSlot()
	{
		onClick?.Invoke(Data);
	}
}
