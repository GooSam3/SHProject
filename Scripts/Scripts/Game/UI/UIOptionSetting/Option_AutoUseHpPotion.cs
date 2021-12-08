using UnityEngine;
using UnityEngine.UI;

public class Option_AutoUseHpPotion : OptionSetting
{
    public Slider NormalPotionSlider, BigPotionSlider;
    public Text NormalPotionPerText, BigPotionPerText;
    private float NormalValue, BigValue;
    [SerializeField] private ZToggle NormalPotionButton, BigPotionButton;
    [SerializeField] private ZToggle NormalPotionAutoBuyToggle, BigPotionAutoBuyToggle;
    [SerializeField] private GameObject AutoBuyLockObject;
    [SerializeField] private Text AutoBuyPotionDesText;
    public override void LoadOption()
    {
        base.LoadOption();

        NormalPotionSlider.value = NormalValue = ZGameOption.Instance.NormalHPPotionPer * 10f;
        BigPotionSlider.value = BigValue = ZGameOption.Instance.BigHPPotionPer * 10f;

        AutoBuyLockObject.SetActive(!ZNet.Data.Me.CurCharData.IsCompletePetCollection(DBConfig.PetCollection_Auto_HpPotion));

        if (ZGameOption.Instance.HP_PotionUsePriority == ZGameOption.HPPotionUsePriority.NORMAL)
            NormalPotionButton.isOn = true;
        else
            BigPotionButton.isOn = true;


        //[박윤성] 해당 펫 컬렉션을 모았을때 옵션값을 받아서 적용
        // 컬렉션 미완성시 무조건 off
        if (ZNet.Data.Me.CurCharData.IsCompletePetCollection(DBConfig.PetCollection_Auto_HpPotion))
        {
            NormalPotionAutoBuyToggle.isOn = ZGameOption.Instance.NormalPotionAutoBuy;
            BigPotionAutoBuyToggle.isOn = ZGameOption.Instance.BigPotionAutoBuy;
        }
        else
        {
            NormalPotionAutoBuyToggle.isOn = false;
            BigPotionAutoBuyToggle.isOn = false;
        }

        AutoBuyPotionDesText.text = string.Format(DBLocale.GetText("Auto_Buy_Potion_Available"), DBLocale.GetText(DBPetCollect.GetCollectionName(DBConfig.PetCollection_Auto_HpPotion)));
    }

    public override void SaveOption()
    {
        base.SaveOption();
        if (NormalValue != ZGameOption.Instance.NormalHPPotionPer)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Normal_PotionPer, NormalValue / 10f);
        }
        if (BigValue != ZGameOption.Instance.BigHPPotionPer)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Big_PotionPer, BigValue / 10f);
        }

        ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_HP_Potion_Priority, ZGameOption.Instance.HP_PotionUsePriority);

        if(NormalPotionAutoBuyToggle.isOn != ZGameOption.Instance.NormalPotionAutoBuy)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Normal_PotionAutoBuy, NormalPotionAutoBuyToggle.isOn);
        }
        if(BigPotionAutoBuyToggle.isOn != ZGameOption.Instance.BigPotionAutoBuy)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Big_PotionAutoBuy, BigPotionAutoBuyToggle.isOn);
        }
    }

    public void ChangePotionPriority(int _potionType)
    {
        ZGameOption.HPPotionUsePriority potionType = (ZGameOption.HPPotionUsePriority)_potionType;

        if(ZGameOption.Instance.HP_PotionUsePriority != potionType)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 회복물약 우선 사용 설정 : " + potionType.ToString());
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_HP_Potion_Priority, potionType);
        }
    }

    public void ChangeNormalPotionAutoBuy()
    {
        ZLog.Log(ZLogChannel.UI, $"## ChangeNormalPotionAutoBuy IsOn {NormalPotionAutoBuyToggle.isOn}");
        ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Normal_PotionAutoBuy, NormalPotionAutoBuyToggle.isOn);
    }

    public void ChangeBigPotionAutoBuy()
    {
        ZLog.Log(ZLogChannel.UI, $"## ChangeBigPotionAutoBuy IsOn {BigPotionAutoBuyToggle.isOn}");
        ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Big_PotionAutoBuy, BigPotionAutoBuyToggle.isOn);
    }

    public void ChangeNormalPotionUsePriority(bool _bChanged)
    {
        ZLog.Log(ZLogChannel.UI, $"## ChangeNormalPotionUsePriority IsOn {_bChanged}");

        if (_bChanged)
        {
            if (ZGameOption.Instance.HP_PotionUsePriority != ZGameOption.HPPotionUsePriority.NORMAL)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_HP_Potion_Priority, ZGameOption.HPPotionUsePriority.NORMAL);
        }

    }

    public void ChangeBigPotionUsePriority(bool _bChanged)
    {
        ZLog.Log(ZLogChannel.UI, $"## ChangeBigPotionUsePriority IsOn {_bChanged}");

        if (_bChanged)
        {
            if (ZGameOption.Instance.HP_PotionUsePriority != ZGameOption.HPPotionUsePriority.BIG)
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_HP_Potion_Priority, ZGameOption.HPPotionUsePriority.BIG);
        }
    }

    public void ChangeNormalPotionPer(float fChangeValue)
    {
        NormalPotionPerText.text = string.Format("{0:0}%", (fChangeValue * 10f));

        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 회복물약 우선 사용 설정 : " + fChangeValue);
        NormalValue = fChangeValue;
    }

    public void ChangeBigPotionPer(float fChangeValue)
    {
        BigPotionPerText.text = string.Format("{0:0}%", (fChangeValue * 10f));

        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 회복물약 우선 사용 설정 : " + fChangeValue);
        BigValue = fChangeValue;
    }
}
