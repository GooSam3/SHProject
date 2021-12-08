using GameDB;
using System;
using UnityEngine;

public class UIGuildDungeonListItem : MonoBehaviour
{
	[SerializeField] private ZText DungeonName;
	[SerializeField] private GameObject ActiveImage;
	[SerializeField] private GameObject SelectedImage;
	[SerializeField] private GameObject LockImage;

	private Action<Stage_Table> ClickEvent;
	private Stage_Table Stage;

	public void SetData(Stage_Table data, Action<Stage_Table> clickEvent)
	{
		Stage = data;
		ClickEvent = clickEvent;

		SelectedImage.SetActive(false);
		DungeonName.text = DBLocale.GetText(data.StageTextID);
	}

	public void ActiveSelectImage(uint selectedStageId)
	{
		SelectedImage.SetActive(Stage.StageID == selectedStageId);
	}

	public void ClickDungeonList()
	{
		ClickEvent?.Invoke(Stage);
	}
}
