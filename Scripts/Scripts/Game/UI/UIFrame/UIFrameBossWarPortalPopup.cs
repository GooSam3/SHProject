using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFrameBossWarPortalPopup : ZUIFrameBase
{
	[SerializeField] private UIBossWarPortalListScrollAdapter ScrollAdapter;
	[SerializeField] private UIBossWarPortalList PortalListItem;
	[SerializeField] private ZButton EnterFieldButton;

	private List<Portal_Table> BossWarPortalList = new List<Portal_Table>();
	public Portal_Table SelectedPortal = null;
	public override bool IsBackable => true;

	public void Init()
	{
		foreach (var portal in GameDBManager.Container.Portal_Table_data.Values)
		{
			if(portal.StageID == ZNet.Data.Me.CurCharData.BossWarContainer.StageTid && portal.PKAreaChangeType == E_PKAreaChangeType.PKArea)
			{
				BossWarPortalList.Add(portal);
			}
		}

		ScrollAdapter.Parameters.ItemPrefab = PortalListItem.GetComponent<RectTransform>();
		var pf = ScrollAdapter.Parameters.ItemPrefab;
		pf.SetParent(transform);
		pf.localScale = Vector2.one;
		pf.localPosition = Vector3.zero;
		pf.gameObject.SetActive(false);
		ScrollAdapter.Initialize(SelectPortal);
		ScrollAdapter.Refresh(BossWarPortalList);

		ZWebManager.Instance.WebGame.REQ_GetServerBossInfo(ZNet.Data.Me.CurCharData.BossWarContainer.StageTid, (recv, recvPacket) =>
		{
			if (recvPacket.OpenBoss.HasValue)
			{
				ZNet.Data.Me.CurCharData.BossWarContainer = new BossWarContainer(recvPacket);
			}
		});

		InvokeRepeating(nameof(CheckEnterableField), 0f, 0.1f);
	}

	public void SelectPortal(Portal_Table portal)
	{
		SelectedPortal = portal;
		ScrollAdapter.UpdateScrollItem();
	}

	private void CheckEnterableField()
	{
		EnterFieldButton.interactable = SelectedPortal != null && ZNet.Data.Me.CurCharData.BossWarContainer.KillableStartTsSec <= TimeManager.NowSec && ZNet.Data.Me.CurCharData.BossWarContainer.KillableEndTsSec > TimeManager.NowSec && !ZNet.Data.Me.CurCharData.BossWarContainer.IsKill;
	}

	public void MoveFieldPortal()
	{
		CancelInvoke(nameof(CheckEnterableField));
		UIManager.Instance.Close<UIFrameBossWarPortalPopup>(true);
		ZGameManager.Instance.TryEnterBossWarField(SelectedPortal.PortalID, () => ZNet.Data.Me.CurCharData.BossWarContainer.Location = BossWarContainer.E_Location.Field);
	}

	public void Close()
	{
		CancelInvoke(nameof(CheckEnterableField));
		UIManager.Instance.Close<UIFrameBossWarPortalPopup>(true);
	}
}
