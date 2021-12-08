using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBSkill : IGameDBHelper
{
    private static Dictionary<E_CharacterType, List<Skill_Table>> m_dicNormalAttack = new Dictionary<E_CharacterType, List<Skill_Table>>();

    private static Dictionary<E_CharacterType, Dictionary<E_WeaponType, List<Skill_Table>>> m_dicSkill = new Dictionary<E_CharacterType, Dictionary<E_WeaponType, List<Skill_Table>>>();
    private static Dictionary<E_CharacterType, Dictionary<E_WeaponType, List<Skill_Table>>> m_dicSkillAll = new Dictionary<E_CharacterType, Dictionary<E_WeaponType, List<Skill_Table>>>();
    public void OnReadyData()
	{
        m_dicNormalAttack.Clear();
        m_dicSkill.Clear();
        m_dicSkillAll.Clear();
        foreach (Skill_Table table in GameDBManager.Container.Skill_Table_data.Values)
        {
            //일반 공격 스킬을 셋팅해 놓는다.
            if (table.SkillType == E_SkillType.Normal)
            {
                if (false == m_dicNormalAttack.TryGetValue(table.CharacterType, out var list))
                {
                    m_dicNormalAttack.Add(table.CharacterType, new List<Skill_Table>());                    
                }

                m_dicNormalAttack[table.CharacterType].Add(table);
            }
            else if(table.SkillType != E_SkillType.PassiveSkill)
            {
                if (false == m_dicSkill.TryGetValue(table.CharacterType, out var list))
                {
                    m_dicSkill.Add(table.CharacterType, new Dictionary<E_WeaponType, List<Skill_Table>>());
                }

                if(false == m_dicSkill[table.CharacterType].TryGetValue(table.WeaponType, out var weaponList))
                {
                    m_dicSkill[table.CharacterType].Add(table.WeaponType, new List<Skill_Table>());
                }

                m_dicSkill[table.CharacterType][table.WeaponType].Add(table);
            }

            if(table.SkillType != E_SkillType.Normal)
            {
                if (false == m_dicSkillAll.TryGetValue(table.CharacterType, out var list))
                {
                    m_dicSkillAll.Add(table.CharacterType, new Dictionary<E_WeaponType, List<Skill_Table>>());
                }

                if (false == m_dicSkillAll[table.CharacterType].TryGetValue(table.WeaponType, out var weaponList))
                {
                    m_dicSkillAll[table.CharacterType].Add(table.WeaponType, new List<Skill_Table>());
                }

                m_dicSkillAll[table.CharacterType][table.WeaponType].Add(table);
            }
        }
	}

	public static Dictionary<uint, Skill_Table> DicSkill
	{
		get { return GameDBManager.Container.Skill_Table_data; }
	}

    public static List<Skill_Table> GetNormalAttackList(E_CharacterType characterType)
    {
        m_dicNormalAttack.TryGetValue(characterType, out List<Skill_Table> list);
        return list;
    }

    public static List<Skill_Table> GetSkillList(E_CharacterType characterType, E_WeaponType weaponType)
    {
        if (m_dicSkill.TryGetValue(characterType, out var weaponDict))
        {
            weaponDict.TryGetValue(weaponType, out var list);

            return list;
        }

        return null;
    }

    public static List<Skill_Table> GetSkillListAll(E_CharacterType characterType, E_WeaponType weaponType)
    {
        if (m_dicSkillAll.TryGetValue(characterType, out var weaponDict))
        {
            weaponDict.TryGetValue(weaponType, out var list);

            return list;
        }

        return null;
    }

    /// <summary> 캐릭터의 모든 스킬을 가지고온다. </summary>
    public static void TryGetCharacterSkillListAll(out List<Skill_Table> skillTableList)
    {
        skillTableList = new List<Skill_Table>();

        foreach (var table in GameDBManager.Container.Skill_Table_data.Values)
        {
            //일반공격은 패스
            if (table.SkillType == E_SkillType.Normal)
                continue;

            if (table.UnitType != E_UnitType.Character)
                continue;

            skillTableList.Add(table);
        }
    }

    public static bool TryGet(uint _tid, out Skill_Table outTable)
	{
		return GameDBManager.Container.Skill_Table_data.TryGetValue(_tid, out outTable);
	}

	public static Skill_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Skill_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}

    public static List<Skill_Table> GetSkillListByGroupId(uint _groupId)
    {
        List<Skill_Table> list = new List<Skill_Table>();

        foreach(Skill_Table table in GameDBManager.Container.Skill_Table_data.Values)
        {
            if (table.GroupID != _groupId)
                continue;

            list.Add(table);
        }

        return list;
    }

    public static bool Has(uint _skillTid)
    {
        return GameDBManager.Container.Skill_Table_data.ContainsKey(_skillTid);
    }

    public static E_AnimParameter GetSkillAnimParameter(uint _skillTid)
    {
        if (false == TryGet(_skillTid, out Skill_Table table))
            return E_AnimParameter.None;

        return GetSkillAnimParameter(table);
    }

    public static E_AnimParameter GetSkillAnimParameter(Skill_Table table)
    {
        if (null == table)
            return E_AnimParameter.None;

        E_AnimParameter playAnimParameter = E_AnimParameter.None;

        switch (table.SkillType)
        {
            case E_SkillType.Normal:
                {
                    switch (table.SkillSort)
                    {
                        case 1:
                            playAnimParameter = E_AnimParameter.Attack_001;
                            break;
                        case 2:
                            playAnimParameter = E_AnimParameter.Attack_002;
                            break;
                        case 3:
                            playAnimParameter = E_AnimParameter.Attack_003;
                            break;
                        default:
                            playAnimParameter = E_AnimParameter.Attack_001;
                            break;
                    }
                }
                break;
            case E_SkillType.ActiveSkill:
                playAnimParameter = E_AnimParameter.Skill_001;
                break;
            case E_SkillType.BuffSkill:
                playAnimParameter = E_AnimParameter.Buff_001;
                break;
        }

        return playAnimParameter;
    }
}
