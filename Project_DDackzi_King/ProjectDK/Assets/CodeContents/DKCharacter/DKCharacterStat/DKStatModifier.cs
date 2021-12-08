using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatModifierType
{
	None,
	Buff,
	Equip,
}


public class DKStatModifier : CStatModifierBase
{
	private EStatModifierType m_eStatModifierType = EStatModifierType.None;  public EStatModifierType GetStatModifierType() { return m_eStatModifierType; }
	//-------------------------------------------------------------------------------


}
