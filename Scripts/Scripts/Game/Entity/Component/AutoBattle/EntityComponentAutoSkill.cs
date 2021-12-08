using System.Collections.Generic;
using GameDB;

public class EntityComponentAutoSkill : EntityComponentBase<ZPawnMyPc>
{
    private List<SkillInfo> TotalSkillInfoList = new List<SkillInfo>();
    private List<SkillInfo> AutoSkillList = new List<SkillInfo>();
    
    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();

        if (false == ZWebManager.hasInstance || false == ZWebManager.Instance.WebGame.IsUsable)
        {
            return;
        }

        TotalSkillInfoList = Owner.SkillSystem.GetSkills();

        foreach(var slot in ZNet.Data.Me.CurCharData.QuickSlotSet1Dic)
        {
            if(slot.Value.SlotType == QuickSlotType.TYPE_SKILL && slot.Value.bAuto)
            {
                SkillInfo skill = TotalSkillInfoList.Find(a => a.SkillId == slot.Value.TableID);

                if (!AutoSkillList.Contains(skill))
                    AutoSkillList.Add(skill);
            }
        }

        foreach (var slot in ZNet.Data.Me.CurCharData.QuickSlotSet2Dic)
        {
            if (slot.Value.SlotType == QuickSlotType.TYPE_SKILL && slot.Value.bAuto)
            {
                SkillInfo skill = TotalSkillInfoList.Find(a => a.SkillId == slot.Value.TableID);

                if (!AutoSkillList.Contains(skill))
                    AutoSkillList.Add(skill);
            }
        }

        SortSkillByOrderNum();
    }

    public void AddAutoSkill(uint _skillId)
    {
        SkillInfo skillInfo = TotalSkillInfoList.Find(a => a.SkillId == _skillId);

        if (skillInfo != null && !AutoSkillList.Contains(skillInfo))
            AutoSkillList.Add(skillInfo);
        
        SortSkillByOrderNum();
    }

    public void RemoveAutoSkill(uint _skillId)
    {
        SkillInfo SkillInfo = AutoSkillList.Find(a => a.SkillId == _skillId);

        if (SkillInfo != null)
            AutoSkillList.Remove(SkillInfo);

        SortSkillByOrderNum();
    }

    public void SortSkillByOrderNum()
    {
        AutoSkillList.Sort((a, b) =>
        {
            return a.Order.CompareTo(b.Order);
        });
    }

    public SkillInfo GetAvailableQuickSlotAutoBuffSkill()
    {
        for (int i = 0; i < AutoSkillList.Count; i++)
        {
            //TODO :: 변환 스킬 임시 처리
            if (false == Owner.SkillSystem.TryGetSkillInfo(AutoSkillList[i].SkillId, out var skillInfo))
                continue;

            E_TargetType type = skillInfo.SkillTable.TargetType;

            if((type == E_TargetType.Enemmy || type == E_TargetType.Summoner))
            {
                continue;
            }
            
            if ((Owner.CurrentMp - skillInfo.SkillTable.UseMPCount) / Owner.MaxMp < ZGameOption.Instance.RemainMPPer)
            {
                continue;
            }

            //auto 지만 autobutton이 켜져있을 경우에만 동작
            if(skillInfo.QuickSlotAutoType == E_QuickSlotAutoType.AutoButtonOn)
			{
                if (false == Owner.IsAutoPlay)
                    continue;
            }

            if (Owner.SkillSystem.CheckUseSkill(skillInfo.SkillId, true) == E_SkillSystemError.None && skillInfo.IsAvailableAbilityAction)
            {
                return skillInfo;
            }
        }

        return null;
    }

    public SkillInfo GetAvailableQuickSlotAutoSkill()
    {
        for (int i = 0; i < AutoSkillList.Count; i++)
        {
            //TODO :: 변환 스킬 임시 처리
            if (false == Owner.SkillSystem.TryGetSkillInfo(AutoSkillList[i].SkillId, out var skillInfo))
                continue;
            
            if ((Owner.CurrentMp - skillInfo.SkillTable.UseMPCount) / Owner.MaxMp < ZGameOption.Instance.RemainMPPer)
            {
                continue;
            }

            //auto 지만 autobutton이 켜져있을 경우에만 동작
            if (skillInfo.QuickSlotAutoType == E_QuickSlotAutoType.AutoButtonOn)
            {
                if (false == Owner.IsAutoPlay)
                    continue;
            }

            if (skillInfo.SkillSystem.CheckUseSkill(skillInfo.SkillId, true) == E_SkillSystemError.None && skillInfo.IsAvailableAbilityAction)
            {
                return skillInfo;
            }
        }

        return null;
    }
}
