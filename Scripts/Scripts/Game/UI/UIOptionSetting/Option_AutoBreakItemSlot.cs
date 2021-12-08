using UnityEngine;
using UnityEngine.UI;

public class Option_AutoBreakItemSlot : OptionSetting
{
    [SerializeField] private ZToggle MagicGradeButton, NormalGradeButton, OffButton;
    [SerializeField] private GameObject BlockObject;
    [SerializeField] private Text AutoBreakDesText;
    public override void LoadOption()
    {
        base.LoadOption();

        switch(ZGameOption.Instance.Auto_Break_Belong_Item)
        {
            case ZGameOption.AutoBreakType.OFF:
                OffButton.SelectToggle();
                break;
            case ZGameOption.AutoBreakType.Tier_1:
                NormalGradeButton.SelectToggle();
                break;
            case ZGameOption.AutoBreakType.Tier_2:
                MagicGradeButton.SelectToggle();
                break;
        }

        BlockObject.SetActive(!ZNet.Data.Me.CurCharData.IsCompleteRideCollection(DBConfig.PetCollection_Auto_Break));

        AutoBreakDesText.text = string.Format(DBLocale.GetText("Auto_Break_Available"), DBLocale.GetText(DBPetCollect.GetCollectionName(DBConfig.PetCollection_Auto_Break)));
    }

    public override void SaveOption()
    {
        base.SaveOption();


    }

    public void ChangedAutoBreakGradeButton(int index)
    {
        ZGameOption.AutoBreakType type = (ZGameOption.AutoBreakType)index;

        if (ZGameOption.Instance.Auto_Break_Belong_Item != type)
        {
            ZLog.Log(ZLogChannel.UI, ZGameOption.OptionKey.Option_AutoBreakBelongItem.ToString() + " : " + type.ToString());
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoBreakBelongItem, type);
            
            //[박윤성] 옵션이 바뀔때만 패킷을 전달
            ZWebManager.Instance.WebGame.REQ_SetItemOption(null);
        }

        
    }
}
