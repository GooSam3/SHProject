using UnityEngine;

public class Option_SearchTargetPriority : OptionSetting
{
    [SerializeField] private ZToggle BeAttackPlayerToggle, EnemyGuildPlayerToggle, NearPlayerToggle;

    public override void LoadOption()
    {
        base.LoadOption();

        BeAttackPlayerToggle.isOn = ZGameOption.Instance.Search_Target_Priority.HasFlag(ZGameOption.SearchTargetPriority.TARGET_BE_ATTACK_PLAYER);
        EnemyGuildPlayerToggle.isOn = ZGameOption.Instance.Search_Target_Priority.HasFlag(ZGameOption.SearchTargetPriority.TARGET_ENEMY_GUILD);
        NearPlayerToggle.isOn = ZGameOption.Instance.Search_Target_Priority.HasFlag(ZGameOption.SearchTargetPriority.TARGET_NEAR_CHARACTER);

    }

    public void ChangeTargetAttackPlayer(bool _isChange)
    {
        if (_isChange != ZGameOption.Instance.Search_Target_Priority.HasFlag(ZGameOption.SearchTargetPriority.TARGET_BE_ATTACK_PLAYER))
        {
            if (_isChange)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SearchTarget_Priority, ZGameOption.Instance.Search_Target_Priority | ZGameOption.SearchTargetPriority.TARGET_BE_ATTACK_PLAYER);
            else
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SearchTarget_Priority, ZGameOption.Instance.Search_Target_Priority ^ ZGameOption.SearchTargetPriority.TARGET_BE_ATTACK_PLAYER);

            BeAttackPlayerToggle.isOn = _isChange;
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 타겟 검색 우선 순위 설정 : " + ZGameOption.SearchTargetPriority.TARGET_BE_ATTACK_PLAYER.ToString() + " :: " + _isChange);
        }
    }

    public void ChangeTargetEnemyGuild(bool _isChange)
    {
        if (_isChange != ZGameOption.Instance.Search_Target_Priority.HasFlag(ZGameOption.SearchTargetPriority.TARGET_ENEMY_GUILD))
        {
            if (_isChange)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SearchTarget_Priority, ZGameOption.Instance.Search_Target_Priority | ZGameOption.SearchTargetPriority.TARGET_ENEMY_GUILD);
            else
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SearchTarget_Priority, ZGameOption.Instance.Search_Target_Priority ^ ZGameOption.SearchTargetPriority.TARGET_ENEMY_GUILD);

            EnemyGuildPlayerToggle.isOn = _isChange;
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 타겟 검색 우선 순위 설정 : " + ZGameOption.SearchTargetPriority.TARGET_ENEMY_GUILD.ToString() + " :: " + _isChange);
        }
    }

    public void ChangeTargetNearCharacter(bool _isChange)
    {
        if (_isChange != ZGameOption.Instance.Search_Target_Priority.HasFlag(ZGameOption.SearchTargetPriority.TARGET_NEAR_CHARACTER))
        {
            if (_isChange)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SearchTarget_Priority, ZGameOption.Instance.Search_Target_Priority | ZGameOption.SearchTargetPriority.TARGET_NEAR_CHARACTER);
            else
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SearchTarget_Priority, ZGameOption.Instance.Search_Target_Priority ^ ZGameOption.SearchTargetPriority.TARGET_NEAR_CHARACTER);

            NearPlayerToggle.isOn = _isChange;
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 타겟 검색 우선 순위 설정 : " + ZGameOption.SearchTargetPriority.TARGET_NEAR_CHARACTER.ToString() + " :: " + _isChange);
        }
    }

}
