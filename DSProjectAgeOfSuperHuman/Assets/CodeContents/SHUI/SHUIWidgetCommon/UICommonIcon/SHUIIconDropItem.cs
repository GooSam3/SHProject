using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uTools;
public class SHUIIconDropItem : SHUIIconBase
{
	[SerializeField]
	private SHUIWidgetSpineItemDrop DropNormal;
	[SerializeField]
	private SHUIWidgetSpineItemDrop DropRare;
	[SerializeField]
	private float ActionDelay = 0f;

	private bool m_bDropRare = false;
	private List<uTweener> m_listTween = new List<uTweener>();
	//--------------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		GetComponentsInChildren(true, m_listTween);

		DropNormal.gameObject.SetActive(false);
		DropRare.gameObject.SetActive(false);

		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].enabled = false;
		}
	}

	public void DoIconItemData(SItemData pItemData, UnityAction<SHUIIconBase> delClick)
	{
		if (pItemData != null)
		{
			SetMonoActive(true);
			ProtSHIconInfo(pItemData.ItemIDB.ItemTID, pItemData.ItemIDB.ItemSID, delClick);
			ProtSHIconBody(pItemData.ItemTable.IconName, pItemData.ItemTable.EItemGradeUI, pItemData.ItemIDB.ItemCount, pItemData.ItemIDB.ItemCount);
			ProtIconStickerEnable(EIconStickerType.LeftTop, false);
			PrivDropItemSetDropQuality(pItemData.ItemTable.EItemGradeUI);
			PrivIconDropItemStart();

			if (pItemData.ItemTable.EItemType == EItemType.Equipment)
			{
				ProtSHIconLevel(0);
			}
			else if (pItemData.ItemTable.EItemType == EItemType.Consumable)
			{
				ProtSHIconValue(pItemData.ItemIDB.ItemLevel);
			}
		}
		else
		{
			ProtIconReset();
		}
	}

	//----------------------------------------------------------------------------
	private void PrivDropItemSetDropQuality(EItemGradeUI eItemGrade)
	{
		if (eItemGrade == EItemGradeUI.Legend || eItemGrade == EItemGradeUI.Artifact)
		{
			m_bDropRare = true;
		}
		else
		{
			m_bDropRare = false;
		}
	}

	private void PrivDropItemTweenStart()
	{
		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].enabled = true;
			m_listTween[i].gameObject.SetActive(true);
			m_listTween[i].ResetPlay();
		}
	}

	private void PrivDropItemTweenShowHide(bool bShow)
	{
		for (int i = 0; i < m_listTween.Count; i++)
		{
			m_listTween[i].enabled = false;
			m_listTween[i].gameObject.SetActive(bShow);
		}
	}

	private void PrivIconDropItemStart()
	{
		SetMonoActive(true);
		PrivDropItemTweenShowHide(false);
		if (m_bDropRare)
		{
			StartCoroutine(CoroutineDropItemStart(DropRare));
		}
		else
		{
			StartCoroutine(CoroutineDropItemStart(DropNormal));
		}
	}

	IEnumerator CoroutineDropItemStart(SHUIWidgetSpineItemDrop pItemDrop)
	{
		yield return new WaitForSeconds(ActionDelay);
		pItemDrop.SetMonoActive(true);
		pItemDrop.DoSpineItemDropStart(HandleDropItemSpineAnimationEnd);
		yield break;
	}

	//--------------------------------------------------------------------------------
	public void HandleDropItemSpineAnimationEnd()
	{
		PrivDropItemTweenStart();
	}

	public void HandleDropItemTweenEnd()
	{
		PrivDropItemTweenShowHide(false);
	}
}
