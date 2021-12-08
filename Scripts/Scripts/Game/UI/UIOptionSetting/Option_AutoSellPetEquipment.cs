using UnityEngine;

public class Option_AutoSellPetEquipment : OptionSetting
{
    [SerializeField] private ZToggle MagicButton, RareButton, HeroButton, LegendButton, AncientButton;
    [SerializeField] private ZToggle OffButton, Grade01Button, Grade02Button, Grade03Button;
    [SerializeField] private CanvasGroup TierBlockCanvas;

    public override void LoadOption()
    {
        base.LoadOption();

        switch(ZGameOption.Instance.Auto_Sell_PetEquipmentGrade)
        {
            case ZGameOption.AutoSellPetEquipmentType.OFF:
                OffButton.SelectToggle();
                break;
            case ZGameOption.AutoSellPetEquipmentType.Grade_01:
                Grade01Button.SelectToggle();
                break;
            case ZGameOption.AutoSellPetEquipmentType.Grade_02:
                Grade02Button.SelectToggle();
                break;
            case ZGameOption.AutoSellPetEquipmentType.Grade_03:
                Grade03Button.SelectToggle();
                break;
        }

        // 등급이 off일때 등급선택 안하기
        if(ZGameOption.Instance.Auto_Sell_PetEquipmentGrade == ZGameOption.AutoSellPetEquipmentType.OFF)
        {
            TierBlockCanvas.enabled = true;
            TierBlockCanvas.blocksRaycasts = false;
        }
        else
        {
            TierBlockCanvas.enabled = false;
            TierBlockCanvas.blocksRaycasts = true;

            switch (ZGameOption.Instance.Auto_Sell_PetEquipmentGradeType)
            {
                case GameDB.E_RuneGradeType.Normal:
                    MagicButton.SelectToggle();
                    break;
                case GameDB.E_RuneGradeType.HighClass:
                    RareButton.SelectToggle();
                    break;
                case GameDB.E_RuneGradeType.Rare:
                    HeroButton.SelectToggle();
                    break;
                case GameDB.E_RuneGradeType.Legend:
                    LegendButton.SelectToggle();
                    break;
                case GameDB.E_RuneGradeType.Myth:
                    AncientButton.SelectToggle();
                    break;
            }
        }

        
    }

    public void SetAutoSellPetEquipmentType(int _typeIndex)
    {
        GameDB.E_RuneGradeType type = (GameDB.E_RuneGradeType)_typeIndex;

        if(ZGameOption.Instance.Auto_Sell_PetEquipmentGradeType != type)
        {
            ZLog.Log(ZLogChannel.UI, ZGameOption.OptionKey.Option_AutoSellRuneGradeType.ToString() + " : " + type.ToString());
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoSellRuneGradeType, type);
            ZWebManager.Instance.WebGame.REQ_RuneSetOption(ZNet.Data.Me.CurUserData.DropRuneSet, null);

        }
    }

    public void SetAutoSellPetEquipmentGrade(int _gradeIndex)
    {
        ZGameOption.AutoSellPetEquipmentType grade = (ZGameOption.AutoSellPetEquipmentType)_gradeIndex;

        if(ZGameOption.Instance.Auto_Sell_PetEquipmentGrade != grade)
        {
            ZLog.Log(ZLogChannel.UI, ZGameOption.OptionKey.Option_AutoSellRuneGrade.ToString() + " : " + grade.ToString());
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoSellRuneGrade, grade);
            ZWebManager.Instance.WebGame.REQ_RuneSetOption(ZNet.Data.Me.CurUserData.DropRuneSet, null);

            if(grade == ZGameOption.AutoSellPetEquipmentType.OFF)
            {
                TierBlockCanvas.enabled = true;
                TierBlockCanvas.blocksRaycasts = false;
            }
            else
            {
                TierBlockCanvas.enabled = false;
                TierBlockCanvas.blocksRaycasts = true;
            }
        }
    }
}
