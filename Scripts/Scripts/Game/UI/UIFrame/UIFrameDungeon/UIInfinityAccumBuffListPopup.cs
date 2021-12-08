using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfinityAccumBuffListPopup : MonoBehaviour
{
    [SerializeField] private ZText TitleText;
    [SerializeField] private Transform BuffListParent;

    public void Init(List<AbilityAction_Table> abilityActionList)
    {
        TitleText.text = DBLocale.GetText("InfiBuff_Buff_Title");

        for (int i = 0; i < BuffListParent.childCount; i++)
        {
            Destroy(BuffListParent.GetChild(i).gameObject);
        }

        for(int i = 0; i < abilityActionList.Count; i++)
        {
            UIInfinityAccumBuffListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIInfinityAccumBuffListItem)).GetComponent<UIInfinityAccumBuffListItem>();

            obj.transform.SetParent(BuffListParent);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;

            obj.Init(abilityActionList[i]);
        }
    }

    public void ClosePopup()
	{
        this.gameObject.SetActive(false);
	}
}
