using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIEXPRestorePopup;

public class UIEXPRestoreQueryPopup : UIPopupBase
{
    #region UI Variable
    // 무료 복구일시 active 시킬 오브젝트들 
    [SerializeField] private List<RectTransform> freeObjs;
    // 골드로 복구할시 active 시킬 오브젝트들 
    [SerializeField] private List<RectTransform> goldObjs;
    // 다이아로 복구시 active 시킬 오브젝트들 
    [SerializeField] private List<RectTransform> diamondObjs;

    [SerializeField] private Text TitleName;

    [SerializeField] private Button btnConfirm;
    [SerializeField] private Text txtBtnContent;

    [SerializeField] private Text txtCost;
    #endregion

    #region System Variable
    private Action[] mCallback = new Action[1];
    #endregion

    /// <summary>
    /// Free 일시 costAmount 는 의미가 없음 
    /// </summary>
    public void Open(string _title, CostType costType, uint costAmount, string _strConfirmBtn, Action _onConfirm)
    {
        Initialize(costType);
      
        transform.localPosition = Vector2.zero;
        transform.localScale = Vector2.one;

        Set(new Button[] { btnConfirm }, new Text[] { txtBtnContent },
            mCallback
            , new string[] { _strConfirmBtn }
            , new Action[]
            {
                _onConfirm
            });

        TitleName.text = _title;

        if (costType != CostType.Free)
            txtCost.text = costAmount.ToString();
    }

    protected override void Active(bool _active)
    {
        if (!_active)
            UIManager.Instance.Close<UIEXPRestoreQueryPopup>();
    }

    // TODO : UI 로 그냥 임의로 가리는거보다 UIScreenBlock ? 이란거 사용해봐야할듯 
    public void Close()
    {
        UIManager.Instance.Close<UIEXPRestoreQueryPopup>();
        // UIManager.Instance.Close<UIScreenBlock>();
    }

    private void Initialize(CostType costType)
    {
        TitleName.text = string.Empty;

        txtBtnContent.text = string.Empty;
        btnConfirm.gameObject.SetActive(false);

        SetActiveCostObjByType(costType);
    }

    public void InputButton(int _buttonCnt)
    {
        mCallback[_buttonCnt]?.Invoke();
    }

    void SetActiveCostObjByType(CostType costType)
    {
        freeObjs.ForEach(t => t.gameObject.SetActive(false));
        goldObjs.ForEach(t => t.gameObject.SetActive(false));
        diamondObjs.ForEach(t => t.gameObject.SetActive(false));

        switch (costType)
        {
            case CostType.Free:
                freeObjs.ForEach(t => t.gameObject.SetActive(true));
                break;
            case CostType.Gold:
                goldObjs.ForEach(t => t.gameObject.SetActive(true));
                break;
            case CostType.Diamond:
                diamondObjs.ForEach(t => t.gameObject.SetActive(true));
                break;
            default:
                ZLog.LogError(ZLogChannel.UI, "add switch Type plz");
                break;
        }
    }
}
