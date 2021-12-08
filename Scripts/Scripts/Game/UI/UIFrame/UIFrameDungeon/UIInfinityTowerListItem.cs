using GameDB;
using UnityEngine;

public class UIInfinityTowerListItem : MonoBehaviour
{
    [SerializeField] private GameObject ImageLinePrev;
    [SerializeField] private GameObject ImageLineNext;
    [SerializeField] private GameObject ImageLineGradient;
    [SerializeField] private GameObject ImageArrow;         // 마지막으로 완료한 층수
    [SerializeField] private GameObject ImageSelect;        // 현재 도전할 층수
    [SerializeField] private GameObject ImageFloor;
    [SerializeField] private ZText FloorText;

    private uint CurrentFloor = 0;
    private InfinityDungeon_Table DungeonTable;

    public void SetData(InfinityDungeon_Table table)
	{
        DungeonTable = table;

        SetFloorUI();
    }

    public void SetFloorUI()
    {
        CurrentFloor = ZNet.Data.Me.CurCharData.InfinityTowerContainer.CurrentDungeonStage?.StageLevel ?? 0;

        if (DungeonTable == null)  // 1층 앞 빈 여백 UI를 위한 처리(데이터 null을 주어 일단 처리)
        {
            FloorText.gameObject.SetActive(false);
            ImageFloor.SetActive(false);
            ImageLineGradient.SetActive(false);
            ImageArrow.SetActive(false);
            ImageSelect.SetActive(false);
            ImageLinePrev.SetActive(ZNet.Data.Me.CurCharData.InfinityTowerContainer.CurrentDungeonStage != null);
            ImageLineNext.SetActive(ZNet.Data.Me.CurCharData.InfinityTowerContainer.CurrentDungeonStage != null);

            return;
        }

        ImageFloor.SetActive(true);
        FloorText.gameObject.SetActive(true);

        if (DungeonTable.StageLevel == CurrentFloor) // 현재 클리어한 층수
        {
            ImageLinePrev.SetActive(true);
            ImageLineNext.SetActive(false);
            ImageLineGradient.SetActive(true);
            ImageArrow.SetActive(true);
            ImageSelect.SetActive(false);
        }
        else if (CurrentFloor > DungeonTable.StageLevel) // 이미 클리어한 층수
        {
            ImageLinePrev.SetActive(true);
            ImageLineNext.SetActive(true);
            ImageLineGradient.SetActive(false);
            ImageArrow.SetActive(false);
            ImageSelect.SetActive(false);
        }
        else if (CurrentFloor + 1 == DungeonTable.StageLevel) // 현재 도전할 층수
        {
            ImageLinePrev.SetActive(false);
            ImageLineNext.SetActive(false);
            ImageLineGradient.SetActive(false);
            ImageArrow.SetActive(false);
            ImageSelect.SetActive(true);
        }
        else
        {
            ImageLinePrev.SetActive(false);
            ImageLineNext.SetActive(false);
            ImageLineGradient.SetActive(false);
            ImageArrow.SetActive(false);
            ImageSelect.SetActive(false);
        }

        FloorText.text = DBLocale.GetText("Infinity_DailyReward_Name_02", DungeonTable.StageLevel);
    }
}
