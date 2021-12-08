using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEnhanceElementTabTrigger : MonoBehaviour
{
    [Serializable]
    public class TabTrigger
    {
        public E_UnitAttributeType type;

        public Image imgSelected;
        public Image imgNewAlarm;
        public Button btn;
        public Text txtLevel; // ex) Lv.2

        public List<GameObject> activeList;

        [HideInInspector] public bool isNew;
    }

    #region Serialized Field
    [Header("** WARNING : None 타입 제외 , 하나씩 탭 버튼 연결시켜야함 **")
    , SerializeField]
    private List<TabTrigger> tabTriggers;
    #endregion

    #region Properties
    public E_UnitAttributeType SelectedType { get; private set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// 해당 탭 선택 , UpdateUI 는 별도로 호출해야합니다.  
    /// </summary>
    public void Select(E_UnitAttributeType type)
    {
        if (SelectedType.Equals(type))
            type = E_UnitAttributeType.None;

        SelectedType = type;
    }

    public void SetNewAlarmActive(E_UnitAttributeType type, bool isNew)
    {
        var t = Find(type);

        if (t == null)
            return;

        t.isNew = isNew;
    }

    public void UpdateUI()
    {
        foreach (var tab in tabTriggers)
        {
            if (tab.type.Equals(E_AutoNoneInfoType.None))
                continue;

            // Get 타입별 레벨
            uint level = Me.CurCharData.GetAttributeLevelByType(tab.type);
            bool isSelected = SelectedType.Equals(tab.type);

            tab.txtLevel.text = string.Format(DBLocale.GetText("Attribute_Level"), level);
            tab.imgSelected.gameObject.SetActive(isSelected);
            tab.imgNewAlarm.gameObject.SetActive(tab.isNew);
            tab.activeList.ForEach(t => t.SetActive(isSelected));
        }
    }
    #endregion

    #region Private Methods
    private TabTrigger Find(E_UnitAttributeType type)
    {
        TabTrigger result = null;

        for (int i = 0; i < tabTriggers.Count; i++)
        {
            if (tabTriggers[i].type.Equals(type))
            {
                result = tabTriggers[i];
                break;
            }
        }

        return result;
    }
    #endregion
}
