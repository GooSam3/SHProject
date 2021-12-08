using GameDB;
using System;
using UnityEngine;

public class UIBossWarListItem : MonoBehaviour
{
    [SerializeField] private ZText BossName;
	[SerializeField] private GameObject SelectImage;
	private Stage_Table Stage;
	private Action<Stage_Table> ClickEvent;

	public void ClickTab()
	{
		ClickEvent?.Invoke(Stage);
	}

	public void ActiveSelectImage(uint selectStageId)
	{
		SelectImage.SetActive(Stage.StageID == selectStageId);
	}

	public void SetData(Stage_Table data, Action<Stage_Table> clickEvent)
	{
		Stage = data;

		BossName.text = DBLocale.GetText(data.StageTextID);

		SelectImage.SetActive(false);
		ClickEvent = clickEvent;
	}
}
