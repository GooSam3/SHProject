using UnityEngine;
using ZNet.Data;

public class UIColosseumRankingItem : MonoBehaviour
{
	[SerializeField] private ZImage imgRanking;
	[SerializeField] private ZText txtRanking;
	[SerializeField] private ZImage gradeImage;
	[SerializeField] private ZImage charClassIcon;
	[SerializeField] private ZText nickName;
	[SerializeField] private ZImage guildImage;
	[SerializeField] private ZText guildName;

	public void SetData(ColosseumRankInfoConverted info)
	{
		if (info.Rank <= 3) {
			txtRanking.enabled = false;
			imgRanking.enabled = true;
			imgRanking.sprite = ZManagerUIPreset.Instance.GetSprite($"img_txt_rank_{info.Rank}");
		}
		else {
			txtRanking.enabled = true;
			txtRanking.text = $"{info.Rank}";
			imgRanking.enabled = false;
		}

		var charTable = DBCharacter.Get(info.CharTid);
		charClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(charTable.Icon);
		nickName.text = info.Nick;

		var colosseumTable = DBColosseum.FindByColosseumPoint((uint)info.Score);
		gradeImage.sprite = ZManagerUIPreset.Instance.GetSprite(colosseumTable.GradeIcon);

		if (info.GuildMarkTid != 0) {
			guildImage.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(info.GuildMarkTid));
			guildImage.enabled = true;
		}
		else {
			guildImage.enabled = false;
		}

		guildName.text = info.GuildName;
	}

	/// <summary> 랭커가 아닌 나를 표시하기  </summary>
	public void SetDataMe_NoRank()
	{
		txtRanking.enabled = true;
		txtRanking.text = $"-";
		imgRanking.enabled = false;

		var charTable = DBCharacter.Get(Me.CurCharData.TID);
		charClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(charTable.Icon);
		nickName.text = Me.CurCharData.Nickname;

		var colosseumTable = DBColosseum.FindByColosseumPoint(Me.CurCharData.ColosseumScore);
		gradeImage.sprite = ZManagerUIPreset.Instance.GetSprite(colosseumTable.GradeIcon);

		if (Me.CurCharData.GuildMarkTid != 0) {
			guildImage.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(Me.CurCharData.GuildMarkTid));
			guildImage.enabled = true;
		}
		else {
			guildImage.enabled = false;
		}

		guildName.text = Me.CurCharData.GuildName;
	}
}
