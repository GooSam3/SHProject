using UnityEngine;
using UnityEngine.UI;

public class Option_ReturnTownWeight : OptionSetting
{
    [SerializeField] private ZToggle Weight50Button, Weight90Button, OffButton;
    [SerializeField] private Text Weigth50Text, Weight90Text;
    public override void LoadOption()
    {
        base.LoadOption();
        
        switch(ZGameOption.Instance.Auto_Return_Town_Weight_Per)
        {
            case 0:
                OffButton.SelectToggle();
                break;
            case 50:
                Weight50Button.SelectToggle();
                break;
            case 90:
                Weight90Button.SelectToggle();
                break;
        }

        if(DBWeightPenalty.TryGetPaneltyData(10000, out var penaltyTable1))
        {
            Weigth50Text.text = string.Format(DBLocale.GetText("Option_AutoPlay_ReCall_Condition"), penaltyTable1.CharacterWeighRate);
        }

        if (DBWeightPenalty.TryGetPaneltyData(20000, out var penaltyTable2))
        {
            Weight90Text.text = string.Format(DBLocale.GetText("Option_AutoPlay_ReCall_Condition"), penaltyTable2.CharacterWeighRate);
        }

    }

    public void SetReturnTownByWeight(int _weight)
    {
        if(ZGameOption.Instance.Auto_Return_Town_Weight_Per != _weight)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 자동 마을 귀환 설정 " + _weight);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoReturnTown_Weight, (float)_weight);
        }
    }
}
