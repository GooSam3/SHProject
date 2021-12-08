using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 캐릭터 클래스, 속성 아이템을 찾아 터치한다.. </summary>
public class TutorialSequence_TouchInventoryItemByCharacterID : TutorialSequence_TouchInventoryItem
{
	private Dictionary<uint, uint> m_dicCharacterReward = new Dictionary<uint, uint>();

	/// <summary> 캐릭터 타입에 따른 tid 선택 </summary>
	protected override bool TryGetParam(out uint tid)
	{
		return m_dicCharacterReward.TryGetValue(ZPawnManager.Instance.MyEntity.TableId, out tid);
	}

	protected override void SetParams(List<string> args)
	{
		m_dicCharacterReward.Clear();

		foreach(var data in args)
		{
			var split = data.Split(':');

			m_dicCharacterReward.Add(uint.Parse(split[0]), uint.Parse(split[1]));
		}
	}
}