using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfinitySelectBuff : MonoBehaviour
{
    [SerializeField] private ZText TitleText;
    [SerializeField] private ZText DescriptionText;
    [SerializeField] private ZText SelectBuffButtonText;
    [SerializeField] private List<ZText> BuffNameText;
    [SerializeField] private List<ZText> BuffOptionText;
    [SerializeField] private List<ZImage> BuffIconImage;
    [SerializeField] private List<ZImage> SelectLine;
    [SerializeField] private ZButton SelectBuffButton;
    [SerializeField] private UIInfinityTowerInfo TowerInfo;

    private uint SelectedBuffTid = 0;
    private List<uint> SelectBuffTids = new List<uint>();
    private Action EnterNextStage = null;

    public void Init(WebNet.ResGetInfinityDungeonSelectBuffList recvPacket, Action action = null)
    {
        EnterNextStage = action;

        SelectBuffTids.Clear();

        for (int i = 0; i < recvPacket.InfinityBuffTidsLength; i++)
        {
            SelectBuffTids.Add(recvPacket.InfinityBuffTids(i));
        }

        SetBuffList();
        OnValueChangeToggle(0);
    }

    private void SetBuffList()
    {
        List<InfiBuff_Table> buffTableList = new List<InfiBuff_Table>();
        Dictionary<E_AbilityType, ValueTuple<float, float>> abilitys = new Dictionary<E_AbilityType, ValueTuple<float, float>>();

        for(int i = 0; i < SelectBuffTids.Count; i++)
        {
            if(GameDBManager.Container.InfiBuff_Table_data.TryGetValue(SelectBuffTids[i], out InfiBuff_Table table))
            {
                buffTableList.Add(table);
            }
        }

        for(int i = 0; i < buffTableList.Count; i++)
        {
            if(DBAbilityAction.TryGet(buffTableList[i].AbilityActionID, out AbilityAction_Table table))
            {
                BuffNameText[i].text = DBLocale.GetText(table.NameText);
                BuffIconImage[i].sprite = ZManagerUIPreset.Instance.GetSprite(table.BuffIconString);

                uint abilityActionId = table.AbilityID_01 == E_AbilityType.LINK_ABILITY_BUFF ? table.LinkAbilityActionID : table.AbilityActionID;

                abilitys = DBAbility.GetAllAbilityData(abilityActionId);
                string options = string.Empty;

                foreach (var ability in abilitys)
                {
                    if(!DBAbility.IsParseAbility(ability.Key))
                    {
                        continue;
                    }

                    float abilityMinValue = (int)abilitys[ability.Key].Item1;
                    float abilityMaxValue = (int)abilitys[ability.Key].Item2;

                    options += DBLocale.GetText(DBAbility.GetAbilityName(ability.Key)) + DBAbility.ParseAbilityValue(ability.Key, abilityMinValue, abilityMaxValue) + "\n";
                }

                BuffOptionText[i].text = options;
            }
        }
    }

    public void OnValueChangeToggle(int index)
    {
        for(int i = 0; i < SelectLine.Count; i++)
        {
            SelectLine[i].gameObject.SetActive(false);
        }

        SelectLine[index].gameObject.SetActive(true);

        SelectedBuffTid = SelectBuffTids[index];
    }

    public void SelectBuff()
    {
        ZWebManager.Instance.WebGame.REQ_InfinityDungeonSelectBuff(SelectedBuffTid, delegate
        {
            ClosePopup();
            TowerInfo?.InitBuffList();
            EnterNextStage?.Invoke();
        });
    }

    public void ClosePopup()
    {
        if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Infinity)
            UIManager.Instance.TopMost<UISubHudInfinityInfo>(false);

        this.gameObject.SetActive(false);
    }
}
