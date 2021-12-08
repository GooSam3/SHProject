using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option_ScanSearchTarget : OptionSetting
{
    enum MonsterType
    {
        QuestMonster = 0,
        NormalMonster = 1,
        HostileMonster = 2
    }

    enum PlayerType
    {
        EnemyGuildPlayer = 0,
        AlertPlayer = 1,
        NormalPlayer = 2,
        MyGuildPlayer = 3,
        AllianceGuildPlayer = 4,
        PartyPlayer = 5
    }

    [SerializeField] private GameObject MonsterToggleOff, MonsterToggleOn, PlayerToggleOff, PlayerToggleOn;
    [SerializeField] private GameObject QuestMonsterToggleOff, QuestMonsterToggleOn, NormalMonsterToggleOff, NormalMonsterToggleOn, HostileMonsterToggleOff, HostileMonsterToggleOn;
    [SerializeField] private GameObject EnemyGuildPlayerToggleOff, EnemyGuildPlayerToggleOn, AlertPlayerToggleOff, AlertPlayerToggleOn, NormalPlayerToggleOff, NormalPlayerToggleOn, MyGuildPlayerToggleOff, MyGuildPlayerToggleOn, AllianceGuildPlayerToggleOff, AllianceGuildPlayerToggleOn, PartyPlayerToggleOff, PartyPlayerToggleOn;

    [SerializeField] private ZToggle MonsterPriorityToggle, PlayerPriorityToggle;
    [SerializeField] private ZToggle ToggleMonster, TogglePlayer;
    [SerializeField] private List<ZToggle> MonsterTypeToggleList = new List<ZToggle>();
    [SerializeField] private List<ZToggle> PlayerTypeToggleList = new List<ZToggle>();

    [SerializeField] private CanvasGroup MonsterTypeCanvasGroup, PlayerTypeCanvasGroup;
    
    public override void LoadOption()
    {
        base.LoadOption();
        
        ToggleMonster.isOn = ZGameOption.Instance.ScanSearchTarget_Priority.HasFlag(ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER);
        SetScanTargetMonster(ToggleMonster.isOn);

        TogglePlayer.isOn = ZGameOption.Instance.ScanSearchTarget_Priority.HasFlag(ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER);
        SetScanTargetPlayer(TogglePlayer.isOn);

        if (ZGameOption.Instance.bMonsterPriority)
        {
            MonsterPriorityToggle.SelectToggle();
        }
        else
        {
            PlayerPriorityToggle.SelectToggle();
        }

        MonsterTypeToggleList[0].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_QUEST_MONSTER);
        SetQuestMonsterToggle(MonsterTypeToggleList[(int)MonsterType.QuestMonster].isOn);

        MonsterTypeToggleList[1].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_NORMAL_MONSTER);
        SetNormalMonsterToggle(MonsterTypeToggleList[(int)MonsterType.NormalMonster].isOn);

        MonsterTypeToggleList[2].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_HOSTILE_MONSTER);
        SetHostileMonsterToggle(MonsterTypeToggleList[(int)MonsterType.HostileMonster].isOn);

        PlayerTypeToggleList[0].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_ENEMYGUILD_PLAYER);
        SetEnemyGuildPlayerToggle(PlayerTypeToggleList[(int)PlayerType.EnemyGuildPlayer].isOn);

        PlayerTypeToggleList[1].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_ALERT_PLAYER);
        SetAlertPlayerToggle(PlayerTypeToggleList[(int)PlayerType.AlertPlayer].isOn);

        PlayerTypeToggleList[2].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_NORMAL_PLAYER);
        SetNormalPlayerToggle(PlayerTypeToggleList[(int)PlayerType.NormalPlayer].isOn);

        PlayerTypeToggleList[3].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_MYGUILD_PLAYER);
        SetMyGuildPlayerToggle(PlayerTypeToggleList[(int)PlayerType.MyGuildPlayer].isOn);

        PlayerTypeToggleList[4].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_ALLIANCEGUILD_PLAYER);
        SetAllianceGuildPlayerToggle(PlayerTypeToggleList[(int)PlayerType.AllianceGuildPlayer].isOn);

        PlayerTypeToggleList[5].isOn = ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_PARTY_PLAYER);
        SetPartyPlayerToggle(PlayerTypeToggleList[(int)PlayerType.PartyPlayer].isOn);
    }

    public void SetScanTargetMonster(bool _isOn)
    {
        if (!_isOn && TogglePlayer.isOn == false && ZGameOption.Instance.ScanSearchTarget_Priority.HasFlag(ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER))
        {
            ToggleMonster.isOn = true;
            return;
        }

        if (!_isOn)
        {
            PlayerPriorityToggle.SelectToggle();
        }

        MonsterTypeCanvasGroup.enabled = !ToggleMonster.isOn;
        MonsterTypeCanvasGroup.blocksRaycasts = ToggleMonster.isOn;

        MonsterToggleOff.SetActive(!_isOn);
        MonsterToggleOn.SetActive(_isOn);

        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER.ToString() + " : " + _isOn);
        SetOption(_isOn, ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER);
    }

    public void SetScanTargetPlayer(bool _isOn)
    {
        if(!_isOn && ToggleMonster.isOn == false && ZGameOption.Instance.ScanSearchTarget_Priority.HasFlag(ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER))
        {
            TogglePlayer.isOn = true;
            return;
        }

        PlayerTypeCanvasGroup.enabled = !TogglePlayer.isOn;
        PlayerTypeCanvasGroup.blocksRaycasts = TogglePlayer.isOn;

        if (!_isOn)
        {
            MonsterPriorityToggle.SelectToggle();
        }

        PlayerToggleOff.SetActive(!_isOn);
        PlayerToggleOn.SetActive(_isOn);

        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER.ToString() + " : " + _isOn);
        SetOption(_isOn, ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER);
    }

    public void SetMonsterTargetPriority(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bMonsterPriority)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Monster_Priority, _isOn);
            ZLog.Log(ZLogChannel.UI, ZGameOption.OptionKey.Option_Monster_Priority.ToString() + " : " + _isOn);
        }
    }

    public void SetQuestMonsterToggle(bool _isOn)
    {
        if(!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_QUEST_MONSTER, true))
        {
            MonsterTypeToggleList[(int)MonsterType.QuestMonster].isOn = true;
            return;
        }

        QuestMonsterToggleOff.SetActive(!_isOn);
        QuestMonsterToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_QUEST_MONSTER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_QUEST_MONSTER.ToString() + " : " + _isOn);
    }

    public void SetNormalMonsterToggle(bool _isOn)
    {
        if(!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_NORMAL_MONSTER, true))
        {
            MonsterTypeToggleList[(int)MonsterType.NormalMonster].isOn = true;
            return;
        }

        NormalMonsterToggleOff.SetActive(!_isOn);
        NormalMonsterToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_NORMAL_MONSTER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_NORMAL_MONSTER.ToString() + " : " + _isOn);
    }

    public void SetHostileMonsterToggle(bool _isOn)
    {
        if(!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_HOSTILE_MONSTER, true))
        {
            MonsterTypeToggleList[(int)MonsterType.HostileMonster].isOn = true;
            return;
        }

        HostileMonsterToggleOff.SetActive(!_isOn);
        HostileMonsterToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_HOSTILE_MONSTER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_HOSTILE_MONSTER.ToString() + " : " + _isOn);
    }

    public void SetEnemyGuildPlayerToggle(bool _isOn)
    {
        if (!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_ENEMYGUILD_PLAYER, false))
        {
            PlayerTypeToggleList[(int)PlayerType.EnemyGuildPlayer].isOn = true;
            return;
        }

        EnemyGuildPlayerToggleOff.SetActive(!_isOn);
        EnemyGuildPlayerToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_ENEMYGUILD_PLAYER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_ENEMYGUILD_PLAYER.ToString() + " : " + _isOn);
    }

    public void SetAlertPlayerToggle(bool _isOn)
    {
        if (!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_ALERT_PLAYER, false))
        {
            PlayerTypeToggleList[(int)PlayerType.AlertPlayer].isOn = true;
            return;
        }

        AlertPlayerToggleOff.SetActive(!_isOn);
        AlertPlayerToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_ALERT_PLAYER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_ALERT_PLAYER.ToString() + " : " + _isOn);
    }

    public void SetNormalPlayerToggle(bool _isOn)
    {
        if (!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_NORMAL_PLAYER, false))
        {
            PlayerTypeToggleList[(int)PlayerType.NormalPlayer].isOn = true;
            return;
        }

        NormalPlayerToggleOff.SetActive(!_isOn);
        NormalPlayerToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_NORMAL_PLAYER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_NORMAL_PLAYER.ToString() + " : " + _isOn);
    }

    public void SetMyGuildPlayerToggle(bool _isOn)
    {
        if (!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_MYGUILD_PLAYER, false))
        {
            PlayerTypeToggleList[(int)PlayerType.MyGuildPlayer].isOn = true;
            return;
        }

        MyGuildPlayerToggleOff.SetActive(!_isOn);
        MyGuildPlayerToggleOn.SetActive(_isOn);


        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_MYGUILD_PLAYER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_MYGUILD_PLAYER.ToString() + " : " + _isOn);
    }

    public void SetAllianceGuildPlayerToggle(bool _isOn)
    {
        if (!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_ALLIANCEGUILD_PLAYER, false))
        {
            PlayerTypeToggleList[(int)PlayerType.AllianceGuildPlayer].isOn = true;
            return;
        }

        AllianceGuildPlayerToggleOff.SetActive(!_isOn);
        AllianceGuildPlayerToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_ALLIANCEGUILD_PLAYER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_ALLIANCEGUILD_PLAYER.ToString() + " : " + _isOn);
    }

    public void SetPartyPlayerToggle(bool _isOn)
    {
        if (!_isOn && CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType.TARGET_PARTY_PLAYER, false))
        {
            PlayerTypeToggleList[(int)PlayerType.PartyPlayer].isOn = true;
            return;
        }

        PartyPlayerToggleOff.SetActive(!_isOn);
        PartyPlayerToggleOn.SetActive(_isOn);

        SetOption(_isOn, ZGameOption.ScanSearchTargetType.TARGET_PARTY_PLAYER);
        ZLog.Log(ZLogChannel.UI, ZGameOption.ScanSearchTargetType.TARGET_PARTY_PLAYER.ToString() + " : " + _isOn);
    }

    private void SetOption(bool _isOn, ZGameOption.ScanSearchTargetType _type)
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 스캔 타겟 종류 설정 : " + _type);
        if (_isOn)
        {
            if (!(ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(_type)))
            {
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ScanSearchTargetType, ZGameOption.Instance.ScanSearchTarget_Type | _type);
            }
        }
        else
        {
            if (ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(_type))
            {
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ScanSearchTargetType, ZGameOption.Instance.ScanSearchTarget_Type ^ _type);
            }
        }
    }

    private void SetOption(bool _isOn, ZGameOption.ScanSearchTargetPriority _type)
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 스캔 우선 순위 설정 : " + _type);
        if (_isOn)
        {
            if(!(ZGameOption.Instance.ScanSearchTarget_Priority.HasFlag(_type)))
            {
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ScanSearchTargetPriority, ZGameOption.Instance.ScanSearchTarget_Priority | _type);
            }
        }
        else
        {
            if(ZGameOption.Instance.ScanSearchTarget_Priority.HasFlag(_type))
            {
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ScanSearchTargetPriority, ZGameOption.Instance.ScanSearchTarget_Priority ^ _type);
            }
        }
    }

    private bool CheckAllTargetTypeToggleOff(ZGameOption.ScanSearchTargetType _type, bool _isMonsterType)
    {
        bool AllToggleOff = true;

        foreach (var toggle in _isMonsterType ? MonsterTypeToggleList : PlayerTypeToggleList)
        {
            if (toggle.isOn)
            {
                AllToggleOff = false;
                return AllToggleOff;
            }
        }

        return AllToggleOff && ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(_type);
    }
}
