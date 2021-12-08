using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossWarSpawnTimePopup : MonoBehaviour
{
	[SerializeField] private ZText PopupTitle;
	[SerializeField] private ZText SpawnTimeTitle;
	[SerializeField] private ZText Grade;
	[SerializeField] private ZText Damage;
	[SerializeField] private ZText Reward;
	[SerializeField] private ZText S_GradeDamage;
	[SerializeField] private ZText A_GradeDamage;
	[SerializeField] private ZText B_GradeDamage;
	[SerializeField] private ZText C_GradeDamage;
	[SerializeField] private ZText D_GradeDamage;
	[SerializeField] private UIBossWarRewardInfoScrollAdapter ScrollAdapter;
	[SerializeField] private UIBossWarRewardInfoListItem RewardInfoItem;
	[SerializeField] private UIBossWarScheduleGridScrollAdapter GridScrollAdapter;
	[SerializeField] private UIBossWarScheduleGridItem ScheduleGridItem;
	
	private BossWarContainer BossWarContainer;
	private List<BossWar_Table> BossWarRewardTableList = new List<BossWar_Table>();

	public void Initialize()
	{
		ScrollAdapter.Parameters.ItemPrefab = RewardInfoItem.GetComponent<RectTransform>();
		var prefab = ScrollAdapter.Parameters.ItemPrefab;
		prefab.SetParent(RewardInfoItem.transform.parent);
		prefab.localScale = Vector2.one;
		prefab.localPosition = Vector3.zero;
		prefab.gameObject.SetActive(false);
		ScrollAdapter.Initialize();

		GridScrollAdapter.Parameters.Grid.CellPrefab = ScheduleGridItem.GetComponent<RectTransform>();
		var gridPrefab = GridScrollAdapter.Parameters.Grid.CellPrefab;
		gridPrefab.SetParent(ScheduleGridItem.transform.parent);
		gridPrefab.localScale = Vector2.one;
		gridPrefab.localPosition = Vector3.zero;
		gridPrefab.gameObject.SetActive(false);
		GridScrollAdapter.Initialize();
	}

	public void Init(Monster_Table bossMonster)
	{
		if (UIManager.Instance.Find(out UIFrameDungeon dungeon)) dungeon.RemoveInfoPopup();

		PopupTitle.text = $"{DBLocale.GetText(bossMonster.MonsterTextID)} 정보";
		SpawnTimeTitle.text = $"{DBLocale.GetText(bossMonster.MonsterTextID)} 소환시간";
		Grade.text = DBLocale.GetText("WBossWar_AttendReward_Grade");
		Damage.text = DBLocale.GetText("WBossWar_AttendReward_Point");
		Reward.text = DBLocale.GetText("WBossWar_AttendReward_Item");

		BossWarRewardTableList.Clear();

		BossWarContainer = ZNet.Data.Me.CurCharData.BossWarContainer;
		
		foreach(var table in GameDBManager.Container.BossWar_Table_data.Values)
		{
			if(table.StageID == BossWarContainer.StageTid)
			{
				BossWarRewardTableList.Add(table);
			}
		}
		ScrollAdapter.SetNormalizedPosition(1);
		ScrollAdapter.Refresh(BossWarRewardTableList);

		GridScrollAdapter.SetNormalizedPosition(1);
		GridScrollAdapter.Refresh(ZNet.Data.Me.CurCharData.BossWarContainer.SpawnTimeScheduleList);
		GridScrollAdapter.SetPosition();
	}

	public void Close()
	{
		if (UIManager.Instance.Find(out UIFrameDungeon dungeon)) dungeon.RemoveInfoPopup();
		this.gameObject.SetActive(false);
	}
}
