using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 인벤토리에서 아이템을 선택한다. </summary>
public class TutorialSequence_TouchInventoryItemByCharacterType : TutorialSequence_TouchInventoryItem
{
	/// <summary> 캐릭터 타입에 따른 tid 선택 </summary>
	protected override bool TryGetParam(out uint tid)
	{
		var flag = (int)ZPawnManager.Instance.MyEntity.OriginalCharacterType;
		int index = 0;


		while(0 < (flag >>= 1))
		{
			++index;
		}
		
		//switch (ZPawnManager.Instance.MyEntity.OriginalCharacterType)
		//{
		//	case GameDB.E_CharacterType.Knight: index = 0;  break;
		//	case GameDB.E_CharacterType.Archer:	index = 1; break;
		//	case GameDB.E_CharacterType.Wizard: index = 2; break;
		//	case GameDB.E_CharacterType.Assassin: index = 3; break;
		//}
		
		return uint.TryParse(TutorialTable.GuideParams[index], out tid);
	}
}