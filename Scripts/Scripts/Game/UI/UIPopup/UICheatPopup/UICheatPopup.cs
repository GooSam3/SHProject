using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class CheatPanel : MonoBehaviour
{
	public abstract void Initialize();

	public virtual void SetActive(bool state)
	{
		this.gameObject.SetActive(state);
	}
}

public class UICheatPopup : ZUIFrameBase
{
	[Serializable]
	private class C_MenuObject
	{
		public CheatPanel targetPanel; // 껏다켜질 오브젝트패널
	}

	private enum E_MenuType
	{
		Item = 0,
		Character = 1,
		Spawn = 2,
	}

	/// <see cref="E_MenuType"/>
	[SerializeField] List<C_MenuObject> listPanel;

	[SerializeField] private ZToggle firstTabRadio;

	protected override void OnInitialize()
	{
		base.OnInitialize();

		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "UI")
		{
			for (int i = 0; i < listPanel.Count; i++)
			{
				listPanel[i].targetPanel.Initialize();
			}
			C_CheatFavoriteHelper.Reload();
		}
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		for (int i = 0; i < listPanel.Count; i++)
		{
			listPanel[i].targetPanel.SetActive(false);
		}
		C_CheatFavoriteHelper.Reload();

		firstTabRadio.SelectToggle(false);
		listPanel[0].targetPanel.SetActive(true);
	}

	public void ClickClose()
	{
		UIManager.Instance.Close<UICheatPopup>();
		C_CheatFavoriteHelper.Save();
	}

	public void ClickMenu(int idx)
	{
		for (int i = 0; i < listPanel.Count; i++)
		{
			listPanel[i].targetPanel.SetActive(idx == i);
		}
	}


	private void OnApplicationQuit()
	{
		C_CheatFavoriteHelper.Save();
	}

#if UNITY_EDITOR

	[ContextMenu("1.VIEW_ITEM")]
	private void CONTEXT_SET_ITEM() => CONTEXT_SET_CHEAT_PANEL(0);

	[ContextMenu("2.VIEW_CHARACTER")]
	private void CONTEXT_CHARACTER() => CONTEXT_SET_CHEAT_PANEL(1);

	[ContextMenu("3.VIEW_SPAWN")]
	private void CONTEXT_SPAWN() => CONTEXT_SET_CHEAT_PANEL(2);

	[ContextMenu("4.VIEW_CONSOLE")]
	private void CONTEXT_CONSOL() => CONTEXT_SET_CHEAT_PANEL(3);

	[ContextMenu("5.VIEW_CUSTOM_CONTENT")]
	private void CONTEXT_CUSTOM_CONTENT() => CONTEXT_SET_CHEAT_PANEL(4);

	private void CONTEXT_SET_CHEAT_PANEL(int i)
	{
		listPanel.ForEach(item => item.targetPanel.gameObject.SetActive(false));

		listPanel[i].targetPanel.gameObject.SetActive(true);
	}

#endif
}