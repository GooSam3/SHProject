using UnityEngine;

public class UIQuestScrollSubItem : UIQuestScrollItemBase
{
	protected override void OnQuestScrollItemInitialize(UIQuestScrollBase.SQuestChapterInfo _questInfo)
	{
		QuestTitleImage.gameObject.SetActive(true);
		QuestChapter.gameObject.SetActive(false);

		Sprite questIcon = ZManagerUIPreset.Instance.GetSprite(_questInfo.UIChapterIcon);
		if (questIcon)
		{
			QuestTitleImage.sprite = questIcon;
		}
	}

}
