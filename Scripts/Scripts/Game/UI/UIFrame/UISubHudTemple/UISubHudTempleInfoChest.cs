using System;
using UnityEngine;
using UnityEngine.UI;
using uTools;

public class UISubHudTempleInfoChest : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private GameObject ChestLock;
	[SerializeField] private GameObject ChestOpen;

	[SerializeField] private uTweenScale TweenChestLock;	
	#endregion

	private ZGA_Chest Chest = null;

	public void Set(ZGA_Chest chest, float delayTime)
	{
		TweenChestLock.ResetToBeginning();
		TweenChestLock.enabled = true;
		TweenChestLock.delay = delayTime;

		Chest = chest;

		UpdateChest();
		if(false == Chest.IsOpened)
		{
			Chest.DoAddEventChestOpen(HandleEventChestOpen);
		}
	}

	private void HandleEventChestOpen()
	{
		Chest?.DoRemoveEventChestOpen(HandleEventChestOpen);
		UpdateChest();
	}

	private void UpdateChest()
	{
		ChestLock.SetActive(false == Chest.IsOpened);
		ChestOpen.SetActive(Chest.IsOpened);
	}
}