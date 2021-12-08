using UnityEngine;

public class Option_AutoTargetPriority : OptionSetting
{
    [SerializeField] private ZToggle HostileMonsterToggle, QuestMonsterToggle;

    public override void LoadOption()
    {
        base.LoadOption(); 

        HostileMonsterToggle.isOn = ZGameOption.Instance.AutoBattle_Target.HasFlag(ZGameOption.AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER);
        QuestMonsterToggle.isOn = ZGameOption.Instance.AutoBattle_Target.HasFlag(ZGameOption.AutoBattleTargetPriority.TARGET_QUEST_MONSTER);
    }

    public override void SaveOption()
    {
        base.SaveOption();
    }

    public void ChangeHostileMonster(bool _isChange)
    {
        if (_isChange != ZGameOption.Instance.AutoBattle_Target.HasFlag(ZGameOption.AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER))
        {
            if (_isChange)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoBattle_Target, ZGameOption.Instance.AutoBattle_Target | ZGameOption.AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER);
            else
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoBattle_Target, ZGameOption.Instance.AutoBattle_Target ^ ZGameOption.AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER);

            HostileMonsterToggle.isOn = _isChange;

            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 자동 사냥 우선순위 설정 : " + ZGameOption.AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER.ToString() + " ON : " + _isChange);
        }
    }

    public void ChangeQuestMonster(bool _isChange)
    {
        if(_isChange != ZGameOption.Instance.AutoBattle_Target.HasFlag(ZGameOption.AutoBattleTargetPriority.TARGET_QUEST_MONSTER))
        {
            if (_isChange)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoBattle_Target, ZGameOption.Instance.AutoBattle_Target | ZGameOption.AutoBattleTargetPriority.TARGET_QUEST_MONSTER);
            else
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoBattle_Target, ZGameOption.Instance.AutoBattle_Target ^ ZGameOption.AutoBattleTargetPriority.TARGET_QUEST_MONSTER);

            QuestMonsterToggle.isOn = _isChange;

            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 자동 사냥 우선순위 설정 : " + ZGameOption.AutoBattleTargetPriority.TARGET_QUEST_MONSTER.ToString() + " ON : " + _isChange);
        }
    }
}
