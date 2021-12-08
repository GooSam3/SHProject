using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollMonsterDropCell : CUGUIWidgetSlotItemBase
{
    [SerializeField] ZText MonsterName = null;
    [SerializeField] ZImage MonsterAttribute = null;
    private uint mMonsterTID = 0;
    private UIWorldMapMonsterDrop mProcessor = null;
    //--------------------------------------------------------------
    public void DoMonsterDropCell(uint _monsterTID, string _monsterName, Sprite _attributte, UIWorldMapMonsterDrop _processor)
	{
        MonsterName.text = _monsterName;
        MonsterAttribute.sprite = _attributte; 
        mMonsterTID = _monsterTID;
        mProcessor = _processor;
	}

    //---------------------------------------------------------------
    public void HandleMonsterDropList()
	{
        mProcessor.DoMonsterDropList(mMonsterTID);
	}

}
