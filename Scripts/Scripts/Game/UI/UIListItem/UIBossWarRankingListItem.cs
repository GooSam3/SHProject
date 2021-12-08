using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBossWarRankingListItem : MonoBehaviour
{
	[SerializeField] private ZText RankingNum;
	[SerializeField] private ZText CharacterName;
	[SerializeField] private ZText ServerName;
	[SerializeField] private Image GuildIcon;
	[SerializeField] private GameObject MyPlayerIcon;

    public void SetData(BossWarPointRanking ranking)
	{
		var entity = ZPawnManager.Instance.GetEntity((uint)ranking.CharId);

		if (entity == null)
		{
			return;
		}

		gameObject.SetActive(true);

		var data = entity.ToData<ZPawnDataCharacter>();
		string guildMarkIcon = DBGuild.GetGuildMark((byte)data.GuildMarkId);

		if(string.IsNullOrEmpty(guildMarkIcon))
		{
			guildMarkIcon = "icon_guildmark_none";
		}

		MyPlayerIcon.SetActive(ZPawnManager.Instance.MyEntityId == ranking.CharId);
		GuildIcon.sprite = ZManagerUIPreset.Instance.GetSprite(guildMarkIcon);
		RankingNum.text = ranking.Rank.ToString();
		CharacterName.text = data.Name;
		ServerName.text = ranking.ServerIdx.ToString();
	}

	public void ResetByExitBossWar()
	{
		gameObject.SetActive(false);
	}
}
