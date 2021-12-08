using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;
using System;

public class UIGodLandLocalViewer : MonoBehaviour
{
	[SerializeField] private UIGodLandLocalViewerItemAdapter ScrollAdapter;
	[SerializeField] private UIGodLandLocalViewerItem recordItemPf;
	[SerializeField] private ZButton localViewerButton;

	private Action<uint> clickItem;

	public void Initialize(Action<uint> _clickItem)
	{
		clickItem = _clickItem;

		ScrollAdapter.Parameters.ItemPrefab = recordItemPf.GetComponent<RectTransform>();
		var pf = ScrollAdapter.Parameters.ItemPrefab;
		pf.SetParent(transform);
		pf.localScale = Vector2.one;
		pf.localPosition = Vector3.zero;
		pf.gameObject.SetActive(false);
		ScrollAdapter.Initialize(_clickItem);
	}

	public void Show(uint selectGroupId, uint selectLocalTId)
	{
		gameObject.SetActive(true);
		localViewerButton.interactable = false;

		Me.CurCharData.GodLandContainer.REQ_GetGodLandInfo(selectGroupId, false, (list) => {
			var selectItem = list.Find(v => v.GodLandTid == selectLocalTId);
			if (selectItem == null) {
				selectItem = list[0];
			}

			ScrollAdapter.Refresh(list);
			ScrollAdapter.ScrollTo(list.IndexOf(selectItem), 0.5f);

			clickItem?.Invoke(selectItem.GodLandTid);
		});
	}

	public void Refresh(uint selectGodLandTid)
	{
		var list = Me.CurCharData.GodLandContainer.SpotInfoList;
		for (int i = 0; i < list.Count; ++i) {
			list[i].IsSelected = list[i].GodLandTid == selectGodLandTid;
		}
		ScrollAdapter.Refresh(list);
	}

	public void Hide()
	{
		localViewerButton.interactable = true;
		gameObject.SetActive(false);
	}
}