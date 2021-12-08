using GameDB;
using System;
using System.Collections.Generic;

/// <summary> 스킬 시스템. 사용가능 여부, 사거리, 쿨타임 처리 등 (MyPc 만 사용하면 될듯.) => 사당에선 몬스터도 사용 </summary>
public class SkillSystem 
{
    /// <summary> 스킬 쿨타임 갱신 </summary>
    private Action<uint, float> mEventSkillEndCoolTime;

    public ZPawn Owner { get; private set; }    
    public Resource_Table ResourceTable { get { return Owner.ResourceTable; } }

    /// <summary> 일반 공격 스킬 관리용</summary>
    private Dictionary<E_CharacterType, List<SkillInfo>> m_listNormalAttack = new Dictionary<E_CharacterType, List<SkillInfo>>();
    /// <summary> 모든 스킬 관리용 </summary>
    private Dictionary<uint, SkillInfo> m_dicSkill = new Dictionary<uint, SkillInfo>();    

    public SkillSystem(ZPawn owner)
    {
        Owner = owner;
    }

    /// <summary> 스킬 초기화 </summary>
    public void Initialize()
    {
        switch(Owner.EntityType)
        {
            case E_UnitType.Character:
                {
                    SetCharacterSkill();
                }
                break;
            case E_UnitType.Monster:
                {
                    SetMonsterSkill();
                }
                break;
        }
    }

    /// <summary> 캐릭터용 일반 공격 </summary>
    private void SetCharacterNormalAttack(E_CharacterType characterType)
    {
        if (false == m_listNormalAttack.ContainsKey(characterType))
        {
            m_listNormalAttack.Add(characterType, new List<SkillInfo>());
        }
        List<Skill_Table> normalAttacks = DBSkill.GetNormalAttackList(characterType);

        for (int i = 0; i < normalAttacks.Count; ++i)
        {
            SkillInfo normalSkill = AddSkill(normalAttacks[i].SkillID);

            if (null != normalSkill)
                m_listNormalAttack[characterType].Add(normalSkill);
        }
    }
    /// <summary> 캐릭터용 스킬 셋팅 </summary>
    private void SetCharacterSkill()
    {
        //일반 공격
        m_listNormalAttack.Clear();
        m_dicSkill.Clear();

        SetCharacterNormalAttack(E_CharacterType.Knight);
        SetCharacterNormalAttack(E_CharacterType.Archer);
        SetCharacterNormalAttack(E_CharacterType.Wizard);
        SetCharacterNormalAttack(E_CharacterType.Assassin);
                
        DBSkill.TryGetCharacterSkillListAll(out var skills);
        for (int i = 0; i < skills.Count; ++i)
        {
            AddSkill(skills[i].SkillID);
        }
    }

    /// <summary> 몬스터용 시킬 정보 셋팅 </summary>
    private void SetMonsterSkill()
    {
        //일반 공격
        m_listNormalAttack.Clear();
        m_dicSkill.Clear();

        var monster = Owner.To<ZPawnMonster>();

        var monsterTable = monster.MonsterData.Table;

        AddSkill(monsterTable.BaseAttackID_01);
        AddSkill(monsterTable.BaseAttackID_02);
        AddSkill(monsterTable.BaseAttackID_03);

        AddSkill(monsterTable.ActiveSkillID_01);
        AddSkill(monsterTable.ActiveSkillID_02);
        AddSkill(monsterTable.ActiveSkillID_03);
        AddSkill(monsterTable.ActiveSkillID_04);
        AddSkill(monsterTable.ActiveSkillID_05);
        AddSkill(monsterTable.ActiveSkillID_06);
        AddSkill(monsterTable.ActiveSkillID_07);
        AddSkill(monsterTable.ActiveSkillID_08);
        AddSkill(monsterTable.ActiveSkillID_09);
    }

    private SkillInfo AddSkill(uint skillTid)
    {
        if (0 >= skillTid)
            return null;

        SkillInfo skill = new SkillInfo(this, skillTid);
        m_dicSkill.Add(skillTid, skill);

        return skill;
    }

    /// <summary> 일반 공격 스킬 정보를 얻어온다. </summary>
    public SkillInfo GetNormalAttack(E_CharacterType characterType, int comboIndex)
    {
        int count = m_listNormalAttack[characterType].Count;
        if (0 >= count)
            return null;

        int index = (comboIndex % count);

        return m_listNormalAttack[characterType][index];
    }


    public List<SkillInfo> GetSkills()
    {
        return new List<SkillInfo>(m_dicSkill.Values);
    }

    /// <summary> 스킬 정보를 얻어온다. </summary>
    public bool TryGetSkillInfo(uint skillId, out SkillInfo skill)
    {        
        if(false == m_dicSkill.TryGetValue(skillId, out skill))
        {
            return false;
        }

		//// 서버에서 알아서 하기때문에 변경할 필요 없어짐
		//if (true == Owner.IsMyPc && skill.CheckChangeSkill()) {
		//	//내 캐릭일 경우
		//	//스킬 변환 대상 스킬이면 처리
		//	if (true == m_dicSkill.TryGetValue(skill.SkillTable.ChangeSkillID, out var changeSkill)) {
		//		skill = changeSkill;
		//	}
		//}

		return true;
    }
    
    /// <summary> SkillId로 스킬 정보를 얻어온다. </summary>
    public SkillInfo GetSkillInfoById(uint skillId)
    {
        TryGetSkillInfo(skillId, out SkillInfo skill);
        return skill;
    }

    /// <summary> 스킬 쿨타임 변경 </summary>
    public void SetCoolTime(uint skillId, float addCoolTime)
    {
        if (false == TryGetSkillInfo(skillId, out var skill))
        {
            return;
        }

        //skill.SetCoolTime(addCoolTime);

        ZNet.Data.Me.CurCharData.SetSkillCoolTime(skillId, (ulong)addCoolTime);
        //서버 시간 기준 쿨타임 완료 시간을 넘겨줌
        mEventSkillEndCoolTime?.Invoke(skillId, skill.EndCoolTimeMs);
    }

    /// <summary> 해당 skillId 사용 </summary>
    public E_SkillSystemError CheckUseSkill(uint skillId, bool isAuto = false)
    {
        if (false == TryGetSkillInfo(skillId, out var skill))
        {
            return E_SkillSystemError.Invalid;
        }

        //쿨타임 체크
        if(false == skill.CheckCoolTime(isAuto))
        {
            return E_SkillSystemError.CoolTime;
        }

        //자동힐 사용가능한지
        if(false == skill.CheckUseAutoHeal(isAuto)) {
            return E_SkillSystemError.CantUseAutoHeal;
        }

        //Mp 체크
        if(false == skill.CheckMp)
        {
            return E_SkillSystemError.NotEnoughMp;
        }

        if (true == Owner.IsMezState(E_ConditionControl.NotSkill))
            return E_SkillSystemError.AbnormalState;

        //Target 찾기

        //캐릭터 타입 체크

        //무기 체크
        if (false == skill.CheckWeapon())
            return E_SkillSystemError.InvalidWeaponType;

        //캐릭터 타입 체크
        if (false == skill.CheckCharacterType())
            return E_SkillSystemError.InvalidCharacterType;

        if(false == skill.CheckUseInTown()){
            return E_SkillSystemError.CantUseInTowen;
        }

        return E_SkillSystemError.None;
    }

    /// <summary> 현재 사용가능한 스킬 얻어오기 </summary>
    public bool TryGetUseableSkillInfos(out List<SkillInfo> skills)
    {
        skills = new List<SkillInfo>();
        foreach (var skill in m_dicSkill)
        {
            if (false == skill.Value.CheckCoolTime())
                continue;

            if (false == skill.Value.CheckCharacterType())
                continue;

            if (false == skill.Value.CheckEnable())
                continue;

            //변환 스킬
            if (true == skill.Value.CheckChangeSkill())
                continue;


            skills.Add(skill.Value);
        }

        return 0 < skills.Count;
    }

    /// <summary> 쿨타임 갱신시 알림 (서버 시간 기준 쿨타임 완료 시간을 넘겨줌 ms) </summary>
    public void DoAddEventSkillEndCoolTime(Action<uint, float> action)
    {
        DoRemoveEventSkillEndCoolTime(action);
        mEventSkillEndCoolTime += action;
    }

    public void DoRemoveEventSkillEndCoolTime(Action<uint, float> action)
    {
        mEventSkillEndCoolTime -= action;
    }
}
