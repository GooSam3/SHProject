using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.UI;

public class UIAbilityViewHolder : BaseItemViewsHolder
{
    private MonoAbilitySlotBase listItem;

    //// 버프용
    //public void SetSlot(S_BuffDetailData data)
    //{
    //    txtName.text = data.name;
    //    txtValue.text = data.value;
    //   // txtToolTip.text = string.Empty;

    //    UnityEngine.Debug.Log($"{data.name}");
    //}

    // 능력치용
    public void SetSlot(UIAbilityData data)
    {
        listItem.SetSlot(data);
    }

    public override void CollectViews()
    {
        base.CollectViews();
         listItem = root.GetComponent<MonoAbilitySlotBase>();
    }
}
