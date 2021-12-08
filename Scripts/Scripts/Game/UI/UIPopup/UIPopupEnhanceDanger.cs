using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIPopupEnhanceDanger : UIPopupBase
{
    private enum E_DangerFxIndex
    {
        Degrade = 0,
        Fail = 1,
        Start = 2,
        Success = 3,
        SuccessHigher = 4,
    }

    private enum E_DrirectionStep
    {
        Start,
        Check,
    }

    [SerializeField] private GameObject objDirection;

    [SerializeField] private GameObject objItemSlot;
    [SerializeField] private Image imgItem;
    [SerializeField] private Image imgGrade;

    [SerializeField] private List<UIFxParticle> listFx;

    public bool IsDirectionEnd { get; private set; } = false;

    private E_EnchantResultType resultType;

    private E_DrirectionStep directionStep;

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        IsDirectionEnd = false;
    }

    public void PlayFX(ZItem _itemDest, E_EnchantResultType _resultType)
    {
        IsDirectionEnd = false;
        resultType = _resultType;

        directionStep = E_DrirectionStep.Start;

        if (DBItem.GetItem(_itemDest.item_tid, out var table) == false)
        {
            ZLog.Log(ZLogChannel.System, $"Item Table Missing Tid : {_itemDest.item_tid}");
            UIManager.Instance.Close<UIPopupEnhanceDanger>();
            IsDirectionEnd = true;
            return;
        }

        imgItem.sprite = UICommon.GetSprite(table.IconID);
        imgGrade.sprite = UICommon.GetGradeSprite(table.Grade);

        objItemSlot.SetActive(true);


        listFx[(int)E_DangerFxIndex.Start].fx.gameObject.SetActive(true);
        objDirection.SetActive(true);
    }

    public void SetNextDirection()
    {
        switch (directionStep)
        {
            case E_DrirectionStep.Start:
                listFx[(int)E_DangerFxIndex.Start].fx.gameObject.SetActive(false);

                switch (resultType)
                {
                    case E_EnchantResultType.Success:
                        listFx[(int)E_DangerFxIndex.Success].fx.gameObject.SetActive(true);
                        break;
                    case E_EnchantResultType.Fail:
                        objItemSlot.SetActive(false);
                        listFx[(int)E_DangerFxIndex.Fail].fx.gameObject.SetActive(true);
                        break;
                    case E_EnchantResultType.BigSuccess:
                        listFx[(int)E_DangerFxIndex.SuccessHigher].fx.gameObject.SetActive(true);
                        break;
                    case E_EnchantResultType.DownSuccess:
                        listFx[(int)E_DangerFxIndex.Degrade].fx.gameObject.SetActive(true);
                        break;
                }
                TimeInvoker.Instance.RequestInvoke(StopDirection, 1.5f);
                directionStep = E_DrirectionStep.Check;
                break;
            case E_DrirectionStep.Check:
                break;
        }
    }

    public void StopDirection()
    {
        listFx.ForEach(item => item.fx.gameObject.SetActive(false));
        objItemSlot.SetActive(false);
        objDirection.SetActive(false);

        IsDirectionEnd = true;

        UIManager.Instance.Close<UIPopupEnhanceDanger>();
    }
}
