using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossWarPortalList : MonoBehaviour
{
	[SerializeField] private ZText PortalName;
	[SerializeField] private GameObject SelectImage;

	private Action<Portal_Table> ClickEvent;
	private Portal_Table Portal;

    public void SetData(Portal_Table portal, Action<Portal_Table> action)
	{
		Portal = portal;

		ClickEvent = action;

		PortalName.text = DBLocale.GetText(Portal.ItemTextID);

		ActiveSelectImage();
	}

	public void SelectPortal()
	{
		ClickEvent?.Invoke(Portal);
		ActiveSelectImage();
	}

	public void ActiveSelectImage()
	{
		if(UIManager.Instance.Find(out UIFrameBossWarPortalPopup portal))
		{
			SelectImage.SetActive(portal.SelectedPortal == Portal);
		}
	}
}
