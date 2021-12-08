using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using ParadoxNotion;

public class UIGodLandBattleRecordItem : MonoBehaviour
{
	[SerializeField] private ZImage resultIcon;
	[SerializeField] private ZText resultText;
	[SerializeField] private ZText timeText;
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZText nickName;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZImage guildImage;
	[SerializeField] private ZText guildName;
	[SerializeField] private ZImage bgImage;
	[SerializeField] private CanvasGroup canvasGroup;

	public void SetData(GodLandBattleRecordConverted data)
	{
		// 시간
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)data.CreateDt);
		timeText.text = $"{dateTimeOffset.LocalDateTime}";

		// 전투결과
		if (data.Role == WebNet.E_BATTLE_ROLE.ATTACKER) {
			//공격 성공&실패
			resultIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_maphunt");

			if (data.Status == true) {
				resultText.text = DBLocale.GetText("GodLand_Robbery_Success_Title");
			}
			else {
				resultText.text = DBLocale.GetText("GodLand_Robbery_Failure_Title");
			}
		}
		else if (data.Role == WebNet.E_BATTLE_ROLE.DEFENDER) {
			//방어 성공&실패
			resultIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_def_shield");

			if (data.Status == true) {
				resultText.text = DBLocale.GetText("GodLand_Defence_Success_Title");
			}
			else {
				resultText.text = DBLocale.GetText("GodLand_Defence_Failure_Title");
			}
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"알수없는 전투타입, {data.Role}");
		}

		bgImage.enabled = data.Status;
		resultIcon.color = (data.Status) ? ColorUtils.HexToColor("45C6E2") : Color.white;
		canvasGroup.alpha = (data.Status) ? 1 : 0.3f;

		// 캐릭터
		var characterTable = DBCharacter.Get(data.CharTid);
		if (characterTable == null) {
			ZLog.LogError(ZLogChannel.Default, $"DBCharacter 가 null이다, CharTid:{data.CharTid }");
			return;
		}

		if (data.ChangeTid == 0) {
			classIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.Icon);
		}
		else {
			var changeTable = DBChange.Get(data.ChangeTid);
			classIcon.sprite = UICommon.GetClassIconSprite(changeTable.ClassIcon, UICommon.E_SIZE_OPTION.Small);
		}
		nickName.text = data.Nick;
		levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), data.Lv);

		// 길드
		if (data.MarkTid != 0) {
			guildImage.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(data.MarkTid));
			guildImage.enabled = true;
		}
		else {
			guildImage.enabled = false;
		}
		guildName.text = data.GuildName;
	}
}