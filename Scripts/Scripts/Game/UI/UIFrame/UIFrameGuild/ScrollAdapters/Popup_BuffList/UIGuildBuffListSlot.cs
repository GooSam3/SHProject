using UnityEngine;
using UnityEngine.UI;

public class UIGuildBuffListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtBenefit01;
    [SerializeField] private Text txtBenefit02;
    [SerializeField] private RectTransform obtainedObj;
    #endregion
    #endregion

    #region System Variables
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetData(
        uint level
        , string benefit01
        , string benefit02
        , bool obtained)
    {
        txtLevel.text = string.Format("Lv.{0}", level);
        txtBenefit01.text = benefit01;
        txtBenefit02.text = benefit02;
        obtainedObj.gameObject.SetActive(obtained);
    }
    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClickCancelRequest()
    {
        ZLog.LogError(ZLogChannel.UI, " no imple ");
    }
    #endregion
}
