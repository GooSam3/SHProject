using System.Collections.Generic;

/// <summary> 클래스에서 클래스를 선택한다. </summary>
public class TutorialSequence_TouchClassByCharacterID : TutorialSequence_TouchClassSelect
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

		foreach (var data in args)
		{
			var split = data.Split(':');

			m_dicCharacterReward.Add(uint.Parse(split[0]), uint.Parse(split[1]));
		}
	}
}