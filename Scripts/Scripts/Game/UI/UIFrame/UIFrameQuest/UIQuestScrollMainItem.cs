public class UIQuestScrollMainItem : UIQuestScrollItemBase
{
	//--------------------------------------------------------
	protected override void OnQuestScrollItemInitialize(UIQuestScrollBase.SQuestChapterInfo _questChapterInfo) 
	{
		QuestTitleImage.gameObject.SetActive(false);
		QuestChapter.text = _questChapterInfo.UIChapterTitleName;
	}
	//--------------------------------------------------------

}
