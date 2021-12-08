using GameDB;
using MmoNet;
using System.Collections.Generic;

using SkillAbilityType = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<GameDB.E_AbilityType, float>>;

/// <summary> 내 캐릭터 정보 캐싱. 어빌리티만 처리하면 될듯 </summary>
public class ZMmoCharacterData
{

    public Dictionary<E_AbilityType, float> m_dicAbility { get; private set; } = new Dictionary<E_AbilityType, float>();
    public SkillAbilityType m_dicSkillAbility { get; private set; } = new SkillAbilityType();
    public ZMmoCharacterData(MmoNet.CharInfo info)
    {
        Reset(info);
    }

    public void Reset(MmoNet.CharInfo info)
    {
        m_dicAbility.Clear();
        m_dicSkillAbility.Clear();

        for (int i = 0; i < info.AbilsLength; ++i)
        {
            var statInfo = info.Abils(i).Value;
            m_dicAbility[(E_AbilityType)statInfo.Type] = statInfo.Value;
        }

        for (int i = 0; i < info.SkillabilsLength; ++i)
        {
            var skillAbilityInfo = info.Skillabils(i).Value;
            uint skillTid = skillAbilityInfo.Skilltid;
            for (int j = 0; j < skillAbilityInfo.AbilsLength; ++j)
            {
                var statInfo = skillAbilityInfo.Abils(j).Value;
                var type = (E_AbilityType)statInfo.Type;
                if (false == m_dicSkillAbility.ContainsKey(skillTid))
                {
                    m_dicSkillAbility.Add(skillTid, new Dictionary<E_AbilityType, float>());
                }

                m_dicSkillAbility[skillTid][type] = statInfo.Value;
            }
        }
    }

    public void AbilityNotify(S2C_AbilityNfy info)
    {
        for (int i = 0; i < info.AbilsLength; ++i)
        {
            var statInfo = info.Abils(i).Value;
            m_dicAbility[(E_AbilityType)statInfo.Type] = statInfo.Value;
        }
    }

    public void SkillAbilityNotiofy(S2C_SkillAbility info)
    {
        for (int i = 0; i < info.SkillabilsLength; ++i)
        {
            SkillAbility skill = info.Skillabils(i).Value;
            uint skillTid = skill.Skilltid;
            for (int j = 0; j < skill.AbilsLength; ++j)
            {
                var statInfo = skill.Abils(j).Value;
                var type = (E_AbilityType)statInfo.Type;
                if (false == m_dicSkillAbility.ContainsKey(skillTid))
                {
                    m_dicSkillAbility.Add(skillTid, new Dictionary<E_AbilityType, float>());
                }

                m_dicSkillAbility[skillTid][type] = statInfo.Value;
            }
        }
    }
}
